using FluentAssertions;
using Markdig;
using Markdig.Extensions.Tables;
using Markdig.Syntax;
using MarkdownToDocx.Core.Markdown;
using Xunit;

namespace MarkdownToDocx.Tests.Unit;

/// <summary>
/// Unit tests for TableExtractor (table data extraction from Markdig AST)
/// </summary>
public class HelpersTableTests
{
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .Build();

    private static Table ParseTable(string markdown)
    {
        var doc = Markdown.Parse(markdown, Pipeline);
        var table = doc.Descendants<Table>().FirstOrDefault()
            ?? throw new InvalidOperationException("No table found in markdown");
        return table;
    }

    [Fact]
    public void GetTableData_WithHeaderAndBodyRows_ShouldSetIsHeaderCorrectly()
    {
        // Arrange
        const string md = """
            | A | B |
            |---|---|
            | 1 | 2 |
            """;
        var table = ParseTable(md);

        // Act
        var data = TableExtractor.Extract(table);

        // Assert
        data.Rows.Should().HaveCount(2);
        data.Rows[0].IsHeader.Should().BeTrue();
        data.Rows[1].IsHeader.Should().BeFalse();
    }

    [Fact]
    public void GetTableData_ShouldSetColumnCount()
    {
        // Arrange
        const string md = """
            | A | B | C |
            |---|---|---|
            | 1 | 2 | 3 |
            """;
        var table = ParseTable(md);

        // Act
        var data = TableExtractor.Extract(table);

        // Assert
        data.ColumnCount.Should().Be(3);
    }

    [Fact]
    public void GetTableData_WithCenterAlignment_ShouldMapToCenter()
    {
        // Arrange
        const string md = """
            | A | B | C |
            |:--|:--:|--:|
            | 1 | 2 | 3 |
            """;
        var table = ParseTable(md);

        // Act
        var data = TableExtractor.Extract(table);

        // Assert
        var headerCells = data.Rows[0].Cells;
        headerCells[0].Alignment.Should().Be("left");
        headerCells[1].Alignment.Should().Be("center");
        headerCells[2].Alignment.Should().Be("right");
    }

    [Fact]
    public void GetTableData_WithDefaultAlignment_ShouldDefaultToLeft()
    {
        // Arrange
        const string md = """
            | A |
            |---|
            | 1 |
            """;
        var table = ParseTable(md);

        // Act
        var data = TableExtractor.Extract(table);

        // Assert
        data.Rows[0].Cells[0].Alignment.Should().Be("left");
    }

    [Fact]
    public void GetTableData_WithBoldTextInCell_ShouldExtractInlineRunWithBold()
    {
        // Arrange
        const string md = """
            | **bold** |
            |----------|
            | normal   |
            """;
        var table = ParseTable(md);

        // Act
        var data = TableExtractor.Extract(table);

        // Assert
        var headerRuns = data.Rows[0].Cells[0].Runs;
        headerRuns.Should().ContainSingle(r => r.Bold);
    }

    [Fact]
    public void GetTableData_WithInlineCodeInCell_ShouldExtractRunWithIsCode()
    {
        // Arrange
        const string md = """
            | `code` |
            |--------|
            | text   |
            """;
        var table = ParseTable(md);

        // Act
        var data = TableExtractor.Extract(table);

        // Assert
        var headerRuns = data.Rows[0].Cells[0].Runs;
        headerRuns.Should().ContainSingle(r => r.IsCode);
    }

    [Fact]
    public void GetTableData_WithPlainCellText_ShouldExtractTextContent()
    {
        // Arrange
        const string md = """
            | Feature | Status |
            |---------|--------|
            | Tables  | Done   |
            """;
        var table = ParseTable(md);

        // Act
        var data = TableExtractor.Extract(table);

        // Assert: body row cells contain expected text
        var bodyRow = data.Rows.First(r => !r.IsHeader);
        bodyRow.Cells[0].Runs.Should().ContainSingle(r => r.Text == "Tables");
        bodyRow.Cells[1].Runs.Should().ContainSingle(r => r.Text == "Done");
    }

    [Fact]
    public void Extract_WithNullTable_ShouldThrowArgumentNullException()
    {
        // Arrange
        Table? nullTable = null;

        // Act
        Action act = () => TableExtractor.Extract(nullTable!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
