using FluentAssertions;
using Markdig;
using Markdig.Extensions.Tables;
using Markdig.Syntax;
using MarkdownToDocx.Core.Markdown;
using Xunit;

namespace MarkdownToDocx.Tests.Unit;

/// <summary>
/// Regression tests for issue #68: Bold markers with tilde inside table cells
/// cause extra phantom columns due to Markdig subscript extension.
/// </summary>
public class TableBoldTildeRegressionTests
{
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .Build();

    private static Table ParseTable(string markdown)
    {
        var doc = Markdown.Parse(markdown, Pipeline);
        return doc.Descendants<Table>().FirstOrDefault()
            ?? throw new InvalidOperationException("No table found in markdown");
    }

    [Fact]
    public void Extract_BoldWithTildeInCells_ShouldNotCreatePhantomColumns()
    {
        // Arrange — exact reproduction from issue #68
        const string md = """
            | Component | Per query | Per 1,000 queries |
            |-----------|-----------|-------------------|
            | Embedding | ~$0.00001 | ~$0.01 |
            | LLM       | ~$0.0015  | ~$1.50 |
            | **Total** | **~$0.0015** | **~$1.50** |
            """;

        // Act — use MarkdigParser (which now uses the fixed pipeline)
        var parser = new MarkdigParser();
        var doc = parser.Parse(md);
        var table = doc.Descendants<Table>().First();
        var data = TableExtractor.Extract(table);

        // Assert
        data.ColumnCount.Should().Be(3, "bold+tilde should not create phantom columns");
        data.Rows.Should().HaveCount(4); // 1 header + 3 body rows
        data.Rows.Should().OnlyContain(r => r.Cells.Count == 3);
    }

    [Fact]
    public void Extract_BoldWithTildeInCells_ShouldPreserveBoldFormatting()
    {
        const string md = """
            | A | B |
            |---|---|
            | **~$1.50** | normal |
            """;

        var parser = new MarkdigParser();
        var doc = parser.Parse(md);
        var table = doc.Descendants<Table>().First();
        var data = TableExtractor.Extract(table);

        data.ColumnCount.Should().Be(2);
        var boldCell = data.Rows[1].Cells[0];
        boldCell.Runs.Should().Contain(r => r.Bold && r.Text.Contains("$1.50"));
    }

    [Fact]
    public void Extract_MultipleRowsWithBoldTilde_ShouldAllHaveCorrectColumnCount()
    {
        // From issue #68: "Real-world impact" example
        const string md = """
            | Item | Cost | Monthly |
            |------|------|---------|
            | **Total without rerank** | **~$0.0015** | **~$1.50** |
            | **Total with rerank**    | **~$0.0019** | **~$1.90** |
            """;

        var parser = new MarkdigParser();
        var doc = parser.Parse(md);
        var table = doc.Descendants<Table>().First();
        var data = TableExtractor.Extract(table);

        data.ColumnCount.Should().Be(3);
        data.Rows.Should().OnlyContain(r => r.Cells.Count == 3);
    }

    [Fact]
    public void Extract_StrikethroughInTable_ShouldStillWork()
    {
        // Ensure ~~strikethrough~~ is not broken by the fix
        const string md = """
            | Feature | Status |
            |---------|--------|
            | ~~Removed~~ | Done |
            """;

        var parser = new MarkdigParser();
        var doc = parser.Parse(md);
        var table = doc.Descendants<Table>().First();
        var data = TableExtractor.Extract(table);

        data.ColumnCount.Should().Be(2);
        data.Rows[1].Cells[0].Runs.Should().HaveCount(1);
    }

    [Fact]
    public void Extract_TildeWithoutBold_ShouldPreserveLiteralTilde()
    {
        const string md = """
            | Value |
            |-------|
            | ~$0.01 |
            """;

        var parser = new MarkdigParser();
        var doc = parser.Parse(md);
        var table = doc.Descendants<Table>().First();
        var data = TableExtractor.Extract(table);

        data.ColumnCount.Should().Be(1);
        var text = string.Join("", data.Rows[1].Cells[0].Runs.Select(r => r.Text));
        text.Should().Contain("~");
        text.Should().Contain("$0.01");
    }
}
