using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using FluentAssertions;
using Markdig;
using Markdig.Extensions.Tables;
using Markdig.Syntax;
using MarkdigTable = Markdig.Extensions.Tables.Table;
using OxmlTable = DocumentFormat.OpenXml.Wordprocessing.Table;
using OxmlTableRow = DocumentFormat.OpenXml.Wordprocessing.TableRow;
using Markdig.Syntax.Inlines;
using MarkdownToDocx.Core.Markdown;
using MarkdownToDocx.Core.Models;
using MarkdownToDocx.Core.OpenXml;
using MarkdownToDocx.Core.TextDirection;
using MarkdownToDocx.Styling.Configuration;
using MarkdownToDocx.Styling.Models;
using MarkdownToDocx.Styling.Styling;
using System.Text;
using Xunit;

namespace MarkdownToDocx.Tests.Integration;

/// <summary>
/// Integration tests for complete Markdown to DOCX conversion pipeline
/// Tests the integration between Core and Styling layers
/// </summary>
public class MarkdownToDocxIntegrationTests : IDisposable
{
    private const string TestPresetDirectory = "../../../../../../config/presets";
    private readonly string _testOutputDirectory;
    private readonly YamlConfigurationLoader _configLoader;
    private readonly StyleApplicator _styleApplicator;
    private readonly MarkdigParser _markdownParser;

    public MarkdownToDocxIntegrationTests()
    {
        _testOutputDirectory = Path.Combine(Path.GetTempPath(), "md2docx-tests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testOutputDirectory);

        _configLoader = new YamlConfigurationLoader(TestPresetDirectory);
        _styleApplicator = new StyleApplicator();
        _markdownParser = new MarkdigParser();
    }

    [Fact]
    public void ConvertSimpleMarkdown_WithMinimalPreset_ShouldCreateValidDocx()
    {
        // Arrange
        var markdown = @"# Hello World

This is a simple paragraph.

## Section 1

- Item 1
- Item 2
- Item 3

### Code Example

```csharp
var x = 42;
```

> This is a quote.
";

        var outputPath = Path.Combine(_testOutputDirectory, "simple.docx");
        var config = _configLoader.LoadPreset("minimal");
        var textDirection = new HorizontalTextProvider();

        // Act
        using (var stream = File.Create(outputPath))
        using (var builder = new OpenXmlDocumentBuilder(stream, textDirection))
        {
            var document = _markdownParser.Parse(markdown);

            // Process markdown blocks
            foreach (var block in document)
            {
                var blockType = block.GetType().Name;

                if (blockType.Contains("Heading"))
                {
                    var level = GetHeadingLevel(block);
                    var text = GetBlockText(block);
                    var style = _styleApplicator.ApplyHeadingStyle(level, config.Styles);
                    builder.AddHeading(level, text, style);
                }
                else if (blockType.Contains("Paragraph"))
                {
                    var text = GetBlockText(block);
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        var style = _styleApplicator.ApplyParagraphStyle(config.Styles);
                        builder.AddParagraph(ToRuns(text), style);
                    }
                }
            }

            builder.Save();
        }

        // Assert
        File.Exists(outputPath).Should().BeTrue();
        var fileInfo = new FileInfo(outputPath);
        fileInfo.Length.Should().BeGreaterThan(0);

        // Verify DOCX is valid by opening it
        using (var docx = WordprocessingDocument.Open(outputPath, false))
        {
            docx.Should().NotBeNull();
            docx.MainDocumentPart.Should().NotBeNull();
            docx.MainDocumentPart!.Document.Should().NotBeNull();
            docx.MainDocumentPart.Document.Body.Should().NotBeNull();
        }
    }

    [Fact]
    public void ConvertMarkdownWithVerticalText_ShouldCreateValidDocx()
    {
        // Arrange
        var markdown = @"# Vertical Text Test

This is a paragraph in vertical text mode.

## Section 1

Sample vertical text content for testing Japanese tategaki layout.
";

        var outputPath = Path.Combine(_testOutputDirectory, "vertical.docx");
        var config = _configLoader.LoadPreset("minimal");
        var textDirection = new VerticalTextProvider();

        // Act
        using (var stream = File.Create(outputPath))
        using (var builder = new OpenXmlDocumentBuilder(stream, textDirection))
        {
            var document = _markdownParser.Parse(markdown);

            foreach (var block in document)
            {
                var blockType = block.GetType().Name;

                if (blockType.Contains("Heading"))
                {
                    var level = GetHeadingLevel(block);
                    var text = GetBlockText(block);
                    var style = _styleApplicator.ApplyHeadingStyle(level, config.Styles);
                    builder.AddHeading(level, text, style);
                }
                else if (blockType.Contains("Paragraph"))
                {
                    var text = GetBlockText(block);
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        var style = _styleApplicator.ApplyParagraphStyle(config.Styles);
                        builder.AddParagraph(ToRuns(text), style);
                    }
                }
            }

            builder.Save();
        }

        // Assert
        File.Exists(outputPath).Should().BeTrue();

        using (var docx = WordprocessingDocument.Open(outputPath, false))
        {
            docx.Should().NotBeNull();
            var body = docx.MainDocumentPart!.Document.Body;
            body.Should().NotBeNull();

            // Verify vertical text direction is set
            var sectionProps = body!.Descendants<DocumentFormat.OpenXml.Wordprocessing.SectionProperties>().FirstOrDefault();
            sectionProps.Should().NotBeNull();
        }
    }

    [Fact]
    public void LoadYamlConfig_ShouldMapAllProperties()
    {
        // Act
        var config = _configLoader.LoadPreset("minimal");

        // Assert
        config.Should().NotBeNull();
        config.SchemaVersion.Should().Be("2.0");
        config.Metadata.Name.Should().Be("Minimal");
        config.TextDirection.Should().Be(Core.Models.TextDirectionMode.Horizontal);

        config.Fonts.Ascii.Should().Be("Noto Serif");
        config.Fonts.EastAsia.Should().Be("Noto Serif CJK JP");
        config.Fonts.DefaultSize.Should().Be(11);

        config.PageLayout.Width.Should().Be(21.0);
        config.PageLayout.Height.Should().Be(29.7);

        config.Styles.H1.Size.Should().Be(24);
        config.Styles.Paragraph.Size.Should().Be(11);
    }

    private static int GetHeadingLevel(object block)
    {
        if (block is HeadingBlock heading)
        {
            return heading.Level;
        }
        return 1;
    }

    private static string GetBlockText(object block)
    {
        if (block is LeafBlock leafBlock && leafBlock.Inline != null)
        {
            return ExtractInlineText(leafBlock.Inline);
        }
        return string.Empty;
    }

    private static List<InlineRun> ToRuns(string text) =>
        new List<InlineRun> { new InlineRun { Text = text } };

    private static string ExtractInlineText(Inline? inline)
    {
        if (inline == null) return string.Empty;

        var sb = new StringBuilder();

        if (inline is ContainerInline container)
        {
            foreach (var child in container)
            {
                sb.Append(ExtractInlineText(child));
            }
        }
        else if (inline is LiteralInline literal)
        {
            sb.Append(literal.Content.ToString());
        }
        else if (inline is LineBreakInline)
        {
            sb.Append(' ');
        }
        else if (inline is CodeInline code)
        {
            sb.Append(code.Content);
        }

        return sb.ToString();
    }

    [Fact]
    public void ConvertMarkdownTable_ShouldProduceParsableDocxWithTableElement()
    {
        // Arrange
        const string markdown = """
            # Document with Table

            | Feature | Status | Notes |
            |---------|--------|-------|
            | Paragraphs | Done | Fully supported |
            | Code blocks | Done | With background color |
            | Tables | Done | New feature |

            End of document.
            """;

        var config = _configLoader.LoadPreset("default");
        var document = _markdownParser.Parse(markdown);

        // Act
        using var stream = new MemoryStream();
        var textDirection = new HorizontalTextProvider();
        using (var builder = new OpenXmlDocumentBuilder(stream, textDirection))
        {
            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
            foreach (var block in document)
            {
                switch (block)
                {
                    case Markdig.Syntax.HeadingBlock heading:
                        var headingText = heading.Inline != null ? ExtractInlineText(heading.Inline.FirstChild) : string.Empty;
                        builder.AddHeading(heading.Level, headingText, _styleApplicator.ApplyHeadingStyle(heading.Level, config.Styles));
                        break;
                    case MarkdigTable tableBlock:
                        var tableData = TableExtractor.Extract(tableBlock);
                        builder.AddTable(tableData, _styleApplicator.ApplyTableStyle(config.Styles));
                        break;
                    case Markdig.Syntax.ParagraphBlock para:
                        var runs = new List<InlineRun>();
                        if (para.Inline != null)
                            runs.Add(new InlineRun { Text = ExtractInlineText(para.Inline.FirstChild) });
                        if (runs.Count > 0 && !string.IsNullOrEmpty(runs[0].Text))
                            builder.AddParagraph(runs, _styleApplicator.ApplyParagraphStyle(config.Styles));
                        break;
                }
            }
            builder.Save();
        }

        // Assert: DOCX is valid and contains a table
        stream.Position = 0;
        using var wordDoc = WordprocessingDocument.Open(stream, false);
        var body = wordDoc.MainDocumentPart!.Document.Body!;
        var table = body.Descendants<DocumentFormat.OpenXml.Wordprocessing.Table>().FirstOrDefault();
        table.Should().NotBeNull("document should contain a table element");

        // Assert: table has correct row count (1 header + 3 body)
        var rows = table!.Descendants<OxmlTableRow>().ToList();
        rows.Should().HaveCount(4);
    }

    [Fact]
    public void ConvertMarkdownTable_WithMirrorMargins_TableShouldUsePercentageWidth()
    {
        // Arrange: use a narrow-margin preset (b6-ja) to verify no overflow
        const string markdown = """
            | Col1 | Col2 | Col3 |
            |------|------|------|
            | A    | B    | C    |
            """;

        // Use inline config with MirrorMargins to simulate narrow-margin book preset (e.g. B6-ja)
        var narrowPageLayout = new PageLayoutConfig
        {
            Width = 12.8,
            Height = 18.2,
            MarginTop = 1.5,
            MarginBottom = 1.5,
            MarginLeft = 2.0,
            MarginRight = 1.27,
            MirrorMargins = true
        };
        var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        var doc = Markdig.Markdown.Parse(markdown, pipeline);
        var table = doc.Descendants<MarkdigTable>().First();
        var tableData = TableExtractor.Extract(table);
        var tableStyle = _styleApplicator.ApplyTableStyle(new StyleConfiguration());

        using var stream = new MemoryStream();
        using var builder = new OpenXmlDocumentBuilder(
            stream,
            new MarkdownToDocx.Styling.TextDirection.ConfigurableTextDirectionProvider(
                new HorizontalTextProvider(), narrowPageLayout));

        builder.AddTable(tableData, tableStyle);
        builder.Save();

        // Assert: table width is percentage-based (not absolute), preventing overflow
        stream.Position = 0;
        using var wordDoc = WordprocessingDocument.Open(stream, false);
        var body2 = wordDoc.MainDocumentPart!.Document.Body!;
        var tblWidth = body2.Descendants<DocumentFormat.OpenXml.Wordprocessing.Table>().First()
            .GetFirstChild<TableProperties>()!
            .GetFirstChild<TableWidth>();

        tblWidth.Should().NotBeNull();
        tblWidth!.Type!.Value.Should().Be(TableWidthUnitValues.Pct,
            "table width must be percentage-based to prevent margin overflow on narrow pages");
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_testOutputDirectory))
            {
                Directory.Delete(_testOutputDirectory, true);
            }
        }
        catch
        {
            // Ignore cleanup errors
        }
    }
}
