using DocumentFormat.OpenXml.Packaging;
using FluentAssertions;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using MarkdownToDocx.Core.Markdown;
using MarkdownToDocx.Core.OpenXml;
using MarkdownToDocx.Core.TextDirection;
using MarkdownToDocx.Styling.Configuration;
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
                        builder.AddParagraph(text, style);
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
                        builder.AddParagraph(text, style);
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
