using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using FluentAssertions;
using MarkdownToDocx.Core.Interfaces;
using MarkdownToDocx.Core.Models;
using MarkdownToDocx.Core.OpenXml;
using MarkdownToDocx.Core.TextDirection;
using Xunit;
using CoreListItem = MarkdownToDocx.Core.Models.ListItem;
using CoreTableStyle = MarkdownToDocx.Core.Models.TableStyle;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;

namespace MarkdownToDocx.Tests.Unit;

/// <summary>
/// Unit tests for OpenXmlDocumentBuilder
/// </summary>
public class OpenXmlDocumentBuilderTests : IDisposable
{
    private readonly MemoryStream _stream;
    private readonly ITextDirectionProvider _horizontalProvider;
    private readonly ITextDirectionProvider _verticalProvider;

    public OpenXmlDocumentBuilderTests()
    {
        _stream = new MemoryStream();
        _horizontalProvider = new HorizontalTextProvider();
        _verticalProvider = new VerticalTextProvider();
    }

    [Fact]
    public void Constructor_WithNullStream_ShouldThrowArgumentNullException()
    {
        // Arrange
        Stream? nullStream = null;

        // Act
        Action act = () => new OpenXmlDocumentBuilder(nullStream!, _horizontalProvider);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("outputStream");
    }

    [Fact]
    public void Constructor_WithNullTextDirection_ShouldThrowArgumentNullException()
    {
        // Arrange
        ITextDirectionProvider? nullProvider = null;

        // Act
        Action act = () => new OpenXmlDocumentBuilder(_stream, nullProvider!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("textDirection");
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateDocument()
    {
        // Arrange & Act
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        builder.Save();

        // Assert
        _stream.Position = 0;
        using var doc = WordprocessingDocument.Open(_stream, false);
        doc.Should().NotBeNull();
        doc.MainDocumentPart.Should().NotBeNull();
        doc.MainDocumentPart!.Document.Should().NotBeNull();
        doc.MainDocumentPart.Document.Body.Should().NotBeNull();
    }

    [Fact]
    public void AddHeading_WithNullText_ShouldThrowArgumentNullException()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);

        // Act
        Action act = () => builder.AddHeading(1, null!, CreateDefaultHeadingStyle());

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("text");
    }

    [Fact]
    public void AddHeading_WithNullStyle_ShouldThrowArgumentNullException()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);

        // Act
        Action act = () => builder.AddHeading(1, "Test", null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("style");
    }

    [Fact]
    public void AddHeading_WithInvalidLevel_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        var style = CreateDefaultHeadingStyle();

        // Act & Assert
        Action act0 = () => builder.AddHeading(0, "Test", style);
        act0.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("level");

        Action act7 = () => builder.AddHeading(7, "Test", style);
        act7.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("level");
    }

    [Fact]
    public void AddHeading_WithValidParameters_ShouldAddHeading()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        var style = CreateDefaultHeadingStyle();

        // Act
        builder.AddHeading(1, "Test Heading", style);
        builder.Save();

        // Assert
        _stream.Position = 0;
        using var doc = WordprocessingDocument.Open(_stream, false);
        var paragraphs = doc.MainDocumentPart!.Document.Body!.Elements<Paragraph>().ToList();

        // Should have at least 1 paragraph (excluding section properties)
        paragraphs.Should().NotBeEmpty();

        var textContent = string.Join("", paragraphs
            .SelectMany(p => p.Descendants<Text>())
            .Select(t => t.Text));
        textContent.Should().Contain("Test Heading");
    }

    [Fact]
    public void AddHeading_WithMultipleLevels_ShouldAddAllHeadings()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        var style = CreateDefaultHeadingStyle();

        // Act
        for (int level = 1; level <= 6; level++)
        {
            builder.AddHeading(level, $"Heading {level}", style);
        }
        builder.Save();

        // Assert
        _stream.Position = 0;
        using var doc = WordprocessingDocument.Open(_stream, false);
        var paragraphs = doc.MainDocumentPart!.Document.Body!.Elements<Paragraph>().ToList();

        for (int level = 1; level <= 6; level++)
        {
            var textContent = string.Join("", paragraphs
                .SelectMany(p => p.Descendants<Text>())
                .Select(t => t.Text));
            textContent.Should().Contain($"Heading {level}");
        }
    }

    [Fact]
    public void AddParagraph_WithNullText_ShouldThrowArgumentNullException()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);

        // Act
        Action act = () => builder.AddParagraph(null!, CreateDefaultParagraphStyle());

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("runs");
    }

    [Fact]
    public void AddParagraph_WithNullStyle_ShouldThrowArgumentNullException()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);

        // Act
        Action act = () => builder.AddParagraph(ToRuns("Test"), null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("style");
    }

    [Fact]
    public void AddParagraph_WithValidParameters_ShouldAddParagraph()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        var style = CreateDefaultParagraphStyle();

        // Act
        builder.AddParagraph(ToRuns("Test paragraph content."), style);
        builder.Save();

        // Assert
        _stream.Position = 0;
        using var doc = WordprocessingDocument.Open(_stream, false);
        var textContent = string.Join("", doc.MainDocumentPart!.Document.Body!
            .Descendants<Text>()
            .Select(t => t.Text));
        textContent.Should().Contain("Test paragraph content.");
    }

    [Fact]
    public void AddParagraph_WithBoldRun_ShouldRenderBoldText()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        var style = CreateDefaultParagraphStyle();
        var runs = new List<InlineRun>
        {
            new InlineRun { Text = "Challenge" },
            new InlineRun { Text = ": " },
            new InlineRun { Text = "important", Bold = true }
        };

        // Act
        builder.AddParagraph(runs, style);
        builder.Save();

        // Assert
        _stream.Position = 0;
        using var doc = WordprocessingDocument.Open(_stream, false);
        var paragraph = doc.MainDocumentPart!.Document.Body!.Elements<Paragraph>().First();
        var allRuns = paragraph.Elements<Run>().ToList();
        allRuns.Should().HaveCount(3);
        allRuns[2].RunProperties?.Bold.Should().NotBeNull();
        var textContent = string.Join("", paragraph.Descendants<Text>().Select(t => t.Text));
        textContent.Should().Contain("important");
    }

    [Fact]
    public void AddParagraph_WithCodeRun_ShouldRenderMonospaceFont()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        var style = CreateDefaultParagraphStyle();
        var runs = new List<InlineRun>
        {
            new InlineRun { Text = "Run " },
            new InlineRun { Text = "claude --help", IsCode = true }
        };

        // Act
        builder.AddParagraph(runs, style);
        builder.Save();

        // Assert
        _stream.Position = 0;
        using var doc = WordprocessingDocument.Open(_stream, false);
        var paragraph = doc.MainDocumentPart!.Document.Body!.Elements<Paragraph>().First();
        var allRuns = paragraph.Elements<Run>().ToList();
        allRuns.Should().HaveCount(2);
        var codeRunFonts = allRuns[1].RunProperties?.RunFonts;
        codeRunFonts.Should().NotBeNull();
        codeRunFonts!.Ascii!.Value.Should().Be("Courier New");
    }

    [Fact]
    public void AddList_WithNullItems_ShouldThrowArgumentNullException()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);

        // Act
        Action act = () => builder.AddList(null!, false, CreateDefaultListStyle());

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("items");
    }

    [Fact]
    public void AddList_WithNullStyle_ShouldThrowArgumentNullException()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        var items = new[] { new CoreListItem { Text = "Test" } };

        // Act
        Action act = () => builder.AddList(items, false, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("style");
    }

    [Fact]
    public void AddList_WithUnorderedList_ShouldAddBullets()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        var items = new List<CoreListItem>
        {
            new CoreListItem { Text = "Item 1" },
            new CoreListItem { Text = "Item 2" },
            new CoreListItem { Text = "Item 3" }
        };
        var style = CreateDefaultListStyle();

        // Act
        builder.AddList(items, false, style);
        builder.Save();

        // Assert
        _stream.Position = 0;
        using var doc = WordprocessingDocument.Open(_stream, false);
        var textContent = string.Join("", doc.MainDocumentPart!.Document.Body!
            .Descendants<Text>()
            .Select(t => t.Text));

        textContent.Should().Contain("• Item 1");
        textContent.Should().Contain("• Item 2");
        textContent.Should().Contain("• Item 3");
    }

    [Fact]
    public void AddList_WithOrderedList_ShouldAddNumbers()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        var items = new List<CoreListItem>
        {
            new CoreListItem { Text = "First" },
            new CoreListItem { Text = "Second" },
            new CoreListItem { Text = "Third" }
        };
        var style = CreateDefaultListStyle();

        // Act
        builder.AddList(items, true, style);
        builder.Save();

        // Assert
        _stream.Position = 0;
        using var doc = WordprocessingDocument.Open(_stream, false);
        var textContent = string.Join("", doc.MainDocumentPart!.Document.Body!
            .Descendants<Text>()
            .Select(t => t.Text));

        textContent.Should().Contain("1. First");
        textContent.Should().Contain("2. Second");
        textContent.Should().Contain("3. Third");
    }

    [Fact]
    public void AddList_WithOrderedListAndStartNumber_ShouldBeginFromSpecifiedNumber()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        var items = new List<CoreListItem>
        {
            new CoreListItem { Text = "Individual settings" },
            new CoreListItem { Text = "Project settings" },
            new CoreListItem { Text = "Local override" }
        };
        var style = CreateDefaultListStyle();

        // Act - simulate a list starting at 2 (interrupted by a code block)
        builder.AddList(items, true, style, startNumber: 2);
        builder.Save();

        // Assert
        _stream.Position = 0;
        using var doc = WordprocessingDocument.Open(_stream, false);
        var textContent = string.Join("", doc.MainDocumentPart!.Document.Body!
            .Descendants<Text>()
            .Select(t => t.Text));

        textContent.Should().Contain("2. Individual settings");
        textContent.Should().Contain("3. Project settings");
        textContent.Should().Contain("4. Local override");
        textContent.Should().NotContain("1. Individual settings");
    }

    [Fact]
    public void AddCodeBlock_WithNullCode_ShouldThrowArgumentNullException()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);

        // Act
        Action act = () => builder.AddCodeBlock(null!, "csharp", CreateDefaultCodeBlockStyle());

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("code");
    }

    [Fact]
    public void AddCodeBlock_WithNullStyle_ShouldThrowArgumentNullException()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);

        // Act
        Action act = () => builder.AddCodeBlock("var x = 42;", "csharp", null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("style");
    }

    [Fact]
    public void AddCodeBlock_WithValidParameters_ShouldAddCodeBlock()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        var style = CreateDefaultCodeBlockStyle();

        // Act
        builder.AddCodeBlock("var x = 42;\nConsole.WriteLine(x);", "csharp", style);
        builder.Save();

        // Assert
        _stream.Position = 0;
        using var doc = WordprocessingDocument.Open(_stream, false);
        var textContent = string.Join("", doc.MainDocumentPart!.Document.Body!
            .Descendants<Text>()
            .Select(t => t.Text));

        textContent.Should().Contain("var x = 42;");
        textContent.Should().Contain("Console.WriteLine(x);");
    }

    [Fact]
    public void AddCodeBlock_ShouldUseBorderSpaceFromStyle()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        var style = CreateDefaultCodeBlockStyle() with { BorderSpace = 2U };

        // Act
        builder.AddCodeBlock("echo hello", null, style);
        builder.Save();

        // Assert: all four paragraph borders must carry Space = 2
        _stream.Position = 0;
        using var doc = WordprocessingDocument.Open(_stream, false);
        var paragraph = doc.MainDocumentPart!.Document.Body!
            .Descendants<Paragraph>()
            .First(p => p.ParagraphProperties?.ParagraphBorders != null);

        var borders = paragraph.ParagraphProperties!.ParagraphBorders!;
        borders.GetFirstChild<TopBorder>()!.Space!.Value.Should().Be(2U);
        borders.GetFirstChild<BottomBorder>()!.Space!.Value.Should().Be(2U);
        borders.GetFirstChild<LeftBorder>()!.Space!.Value.Should().Be(2U);
        borders.GetFirstChild<RightBorder>()!.Space!.Value.Should().Be(2U);
    }

    [Fact]
    public void AddCodeBlock_SingleLine_ShouldNotHaveTrailingBreaks()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        var style = CreateDefaultCodeBlockStyle();

        // Act - simulate Markdig output: trailing \r\n sequences after code content
        builder.AddCodeBlock("touch CLAUDE.md", null, style);
        builder.Save();

        // Assert: only one Text element with the actual content, no trailing Break elements
        _stream.Position = 0;
        using var doc = WordprocessingDocument.Open(_stream, false);
        var paragraph = doc.MainDocumentPart!.Document.Body!.Descendants<Paragraph>().First();
        var run = paragraph.Elements<Run>().First();

        var texts = run.Elements<Text>().ToList();
        var breaks = run.Elements<Break>().ToList();

        texts.Should().HaveCount(1);
        texts[0].Text.Should().Be("touch CLAUDE.md");
        breaks.Should().BeEmpty();
    }

    [Fact]
    public void AddCodeBlock_WithTrailingCrLf_ShouldStripTrailingNewlines()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        var style = CreateDefaultCodeBlockStyle();

        // Act - code string ending with \r\n\r\n as produced by AppendLine + Markdig trailing lines
        builder.AddCodeBlock("line1\r\nline2", null, style);
        builder.Save();

        // Assert: two Text elements (line1, line2), one Break between them, no trailing Break
        _stream.Position = 0;
        using var doc = WordprocessingDocument.Open(_stream, false);
        var paragraph = doc.MainDocumentPart!.Document.Body!.Descendants<Paragraph>().First();
        var run = paragraph.Elements<Run>().First();

        var texts = run.Elements<Text>().ToList();
        var breaks = run.Elements<Break>().ToList();

        texts.Should().HaveCount(2);
        texts[0].Text.Should().Be("line1");
        texts[1].Text.Should().Be("line2");
        breaks.Should().HaveCount(1);
    }

    [Fact]
    public void AddCodeBlock_MultiLine_ShouldNotHaveTrailingBreaks()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        var style = CreateDefaultCodeBlockStyle();

        // Act
        builder.AddCodeBlock("line1\nline2\nline3", null, style);
        builder.Save();

        // Assert: 3 Text elements, 2 Break elements (between lines only)
        _stream.Position = 0;
        using var doc = WordprocessingDocument.Open(_stream, false);
        var paragraph = doc.MainDocumentPart!.Document.Body!.Descendants<Paragraph>().First();
        var run = paragraph.Elements<Run>().First();

        var texts = run.Elements<Text>().ToList();
        var breaks = run.Elements<Break>().ToList();

        texts.Should().HaveCount(3);
        breaks.Should().HaveCount(2);
    }

    [Fact]
    public void AddCodeBlock_WithWordWrapEnabled_ShouldAddWordWrapElement()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        var style = CreateDefaultCodeBlockStyle() with { WordWrap = true };

        // Act
        builder.AddCodeBlock("var x = 42;", null, style);
        builder.Save();

        // Assert: paragraph properties must contain a WordWrap element
        _stream.Position = 0;
        using var doc = WordprocessingDocument.Open(_stream, false);
        var paragraph = doc.MainDocumentPart!.Document.Body!
            .Descendants<Paragraph>()
            .First(p => p.ParagraphProperties?.ParagraphBorders != null);

        paragraph.ParagraphProperties!
            .GetFirstChild<WordWrap>()
            .Should().NotBeNull("WordWrap element should be present when WordWrap = true");
    }

    [Fact]
    public void AddCodeBlock_WithWordWrapDisabled_ShouldNotAddWordWrapElement()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        var style = CreateDefaultCodeBlockStyle() with { WordWrap = false };

        // Act
        builder.AddCodeBlock("var x = 42;", null, style);
        builder.Save();

        // Assert: paragraph properties must NOT contain a WordWrap element
        _stream.Position = 0;
        using var doc = WordprocessingDocument.Open(_stream, false);
        var paragraph = doc.MainDocumentPart!.Document.Body!
            .Descendants<Paragraph>()
            .First(p => p.ParagraphProperties?.ParagraphBorders != null);

        paragraph.ParagraphProperties!
            .GetFirstChild<WordWrap>()
            .Should().BeNull("WordWrap element should not be present when WordWrap = false");
    }

    [Fact]
    public void AddCodeBlock_WithBorderSpace_ShouldAddMatchingIndentation()
    {
        // Arrange: BorderSpace = 4 pt → indent must be 4 * 20 = 80 twips on both sides
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        var style = CreateDefaultCodeBlockStyle() with { BorderSpace = 4 };

        // Act
        builder.AddCodeBlock("var x = 42;", null, style);
        builder.Save();

        // Assert
        _stream.Position = 0;
        using var doc = WordprocessingDocument.Open(_stream, false);
        var paragraph = doc.MainDocumentPart!.Document.Body!
            .Descendants<Paragraph>()
            .First(p => p.ParagraphProperties?.ParagraphBorders != null);

        var indent = paragraph.ParagraphProperties!.GetFirstChild<Indentation>();
        indent.Should().NotBeNull("Indentation must be added when BorderSpace > 0");
        indent!.Left?.Value.Should().Be("80");
        indent!.Right?.Value.Should().Be("80");
    }

    [Fact]
    public void AddCodeBlock_WithZeroBorderSpace_ShouldNotAddIndentation()
    {
        // Arrange: default BorderSpace = 0 → no indentation added
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        var style = CreateDefaultCodeBlockStyle() with { BorderSpace = 0 };

        // Act
        builder.AddCodeBlock("var x = 42;", null, style);
        builder.Save();

        // Assert
        _stream.Position = 0;
        using var doc = WordprocessingDocument.Open(_stream, false);
        var paragraph = doc.MainDocumentPart!.Document.Body!
            .Descendants<Paragraph>()
            .First(p => p.ParagraphProperties?.ParagraphBorders != null);

        paragraph.ParagraphProperties!
            .GetFirstChild<Indentation>()
            .Should().BeNull("No Indentation should be added when BorderSpace = 0");
    }

    [Fact]
    public void AddQuote_WithPaddingSpaceAndBackground_ShouldAddRightIndentation()
    {
        // Arrange: PaddingSpace = 4 pt → right indent must be 4 * 20 = 80 twips
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        var style = CreateDefaultQuoteStyle() with
        {
            BackgroundColor = "f0f0f0",
            PaddingSpace = 4,
            LeftIndent = "560"
        };

        // Act
        builder.AddQuote(ToRuns("test"), style);
        builder.Save();

        // Assert
        _stream.Position = 0;
        using var doc = WordprocessingDocument.Open(_stream, false);
        var paragraph = doc.MainDocumentPart!.Document.Body!
            .Descendants<Paragraph>()
            .First(p => p.ParagraphProperties?.ParagraphBorders != null);

        var indent = paragraph.ParagraphProperties!.GetFirstChild<Indentation>();
        indent.Should().NotBeNull();
        indent!.Left?.Value.Should().Be("560");
        indent!.Right?.Value.Should().Be("80", "right indent = PaddingSpace * 20 twips prevents right border overflow");
    }

    [Fact]
    public void AddQuote_WithoutPaddingSpace_ShouldNotAddRightIndentation()
    {
        // Arrange: PaddingSpace = 0 or no background → no right indent
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        var style = CreateDefaultQuoteStyle() with
        {
            BackgroundColor = null,
            PaddingSpace = 0,
            LeftIndent = "720"
        };

        // Act
        builder.AddQuote(ToRuns("test"), style);
        builder.Save();

        // Assert
        _stream.Position = 0;
        using var doc = WordprocessingDocument.Open(_stream, false);
        var paragraph = doc.MainDocumentPart!.Document.Body!
            .Descendants<Paragraph>()
            .First(p => p.ParagraphProperties?.ParagraphBorders != null);

        var indent = paragraph.ParagraphProperties!.GetFirstChild<Indentation>();
        indent.Should().NotBeNull();
        indent!.Right.Should().BeNull("no right indentation when PaddingSpace = 0");
    }

    [Fact]
    public void AddQuote_WithNullRuns_ShouldThrowArgumentNullException()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);

        // Act
        Action act = () => builder.AddQuote(null!, CreateDefaultQuoteStyle());

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("runs");
    }

    [Fact]
    public void AddQuote_WithNullStyle_ShouldThrowArgumentNullException()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);

        // Act
        Action act = () => builder.AddQuote(ToRuns("Test quote"), null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("style");
    }

    [Fact]
    public void AddQuote_WithValidParameters_ShouldAddQuote()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        var style = CreateDefaultQuoteStyle();

        // Act
        builder.AddQuote(ToRuns("This is a quoted text."), style);
        builder.Save();

        // Assert
        _stream.Position = 0;
        using var doc = WordprocessingDocument.Open(_stream, false);
        var textContent = string.Join("", doc.MainDocumentPart!.Document.Body!
            .Descendants<Text>()
            .Select(t => t.Text));

        textContent.Should().Contain("This is a quoted text.");
    }

    [Fact]
    public void AddThematicBreak_ShouldAddHorizontalLine()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);

        // Act
        builder.AddThematicBreak();
        builder.Save();

        // Assert
        _stream.Position = 0;
        using var doc = WordprocessingDocument.Open(_stream, false);
        var paragraphs = doc.MainDocumentPart!.Document.Body!.Elements<Paragraph>().ToList();

        // Should have at least one paragraph with border
        paragraphs.Should().NotBeEmpty();
        var borderedParagraph = paragraphs.FirstOrDefault(p =>
            p.ParagraphProperties?.ParagraphBorders != null);
        borderedParagraph.Should().NotBeNull();
    }

    [Fact]
    public void AddImage_WithNullPath_ShouldThrowArgumentNullException()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        var style = new ImageStyle();

        // Act
        Action act = () => builder.AddImage(null!, "alt text", style);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("imagePath");
    }

    [Fact]
    public void AddImage_WithNullAltText_ShouldThrowArgumentNullException()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        var style = new ImageStyle();

        // Act
        Action act = () => builder.AddImage("some/path.png", null!, style);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("altText");
    }

    [Fact]
    public void AddImage_WithNonExistentFile_ShouldThrowFileNotFoundException()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        var style = new ImageStyle();

        // Act
        Action act = () => builder.AddImage("/nonexistent/image.png", "alt text", style);

        // Assert
        act.Should().Throw<FileNotFoundException>();
    }

    [Fact]
    public void AddImage_WithValidPng_ShouldEmbedImageInDocument()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        var imagePath = Path.Combine(AppContext.BaseDirectory, "TestAssets", "test-image.png");
        var style = new ImageStyle { MaxWidthPercent = 80, Alignment = "center" };

        // Act
        builder.AddImage(imagePath, "Test image", style);
        builder.Save();

        // Assert: document should contain an image part
        _stream.Position = 0;
        using var doc = WordprocessingDocument.Open(_stream, false);
        doc.MainDocumentPart!.ImageParts.Should().NotBeEmpty();

        // Assert: document body should contain a Drawing element
        var drawings = doc.MainDocumentPart.Document.Body!.Descendants<Drawing>().ToList();
        drawings.Should().NotBeEmpty();
    }

    [Fact]
    public void AddImage_MultipleTimes_ShouldEmbedMultipleImages()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        var imagePath = Path.Combine(AppContext.BaseDirectory, "TestAssets", "test-image.png");
        var style = new ImageStyle();

        // Act
        builder.AddImage(imagePath, "Image 1", style);
        builder.AddImage(imagePath, "Image 2", style);
        builder.Save();

        // Assert: two separate image parts and Drawing elements
        _stream.Position = 0;
        using var doc = WordprocessingDocument.Open(_stream, false);
        doc.MainDocumentPart!.ImageParts.Count().Should().Be(2);

        var drawings = doc.MainDocumentPart.Document.Body!.Descendants<Drawing>().ToList();
        drawings.Should().HaveCount(2);

        // DocProperties IDs must be unique
        var ids = drawings
            .Select(d => d.Descendants<DW.DocProperties>().First().Id!.Value)
            .ToList();
        ids.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void Save_ShouldPersistDocument()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        builder.AddParagraph(ToRuns("Test content"), CreateDefaultParagraphStyle());

        // Act
        builder.Save();

        // Assert
        _stream.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Dispose_ShouldCleanupResources()
    {
        // Arrange
        var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);

        // Act
        builder.Dispose();

        // Assert - should not throw
        builder.Invoking(b => b.Dispose()).Should().NotThrow();
    }

    [Fact]
    public void Save_AfterDispose_ShouldThrowObjectDisposedException()
    {
        // Arrange
        var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        builder.Dispose();

        // Act
        Action act = () => builder.Save();

        // Assert
        act.Should().Throw<ObjectDisposedException>();
    }

    [Fact]
    public void VerticalTextDirection_ShouldConfigureCorrectly()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _verticalProvider);

        // Act
        builder.AddParagraph(ToRuns("Vertical text test"), CreateDefaultParagraphStyle());
        builder.Save();

        // Assert
        _stream.Position = 0;
        using var doc = WordprocessingDocument.Open(_stream, false);
        var sectionProps = doc.MainDocumentPart!.Document.Body!.Elements<SectionProperties>().FirstOrDefault();
        sectionProps.Should().NotBeNull();

        var textDirection = sectionProps!.Elements<DocumentFormat.OpenXml.Wordprocessing.TextDirection>().FirstOrDefault();
        textDirection.Should().NotBeNull();
    }

    [Fact]
    public void ComplexDocument_WithAllElements_ShouldBuildSuccessfully()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);

        // Act
        builder.AddHeading(1, "Main Title", CreateDefaultHeadingStyle());
        builder.AddParagraph(ToRuns("Introduction paragraph."), CreateDefaultParagraphStyle());
        builder.AddHeading(2, "Section 1", CreateDefaultHeadingStyle());
        builder.AddList(new List<CoreListItem>
        {
            new CoreListItem { Text = "Point 1" },
            new CoreListItem { Text = "Point 2" }
        }, false, CreateDefaultListStyle());
        builder.AddCodeBlock("var example = true;", "csharp", CreateDefaultCodeBlockStyle());
        builder.AddQuote(ToRuns("Important note"), CreateDefaultQuoteStyle());
        builder.AddThematicBreak();
        builder.Save();

        // Assert
        _stream.Position = 0;
        using var doc = WordprocessingDocument.Open(_stream, false);
        var textContent = string.Join("", doc.MainDocumentPart!.Document.Body!
            .Descendants<Text>()
            .Select(t => t.Text));

        textContent.Should().Contain("Main Title");
        textContent.Should().Contain("Introduction paragraph.");
        textContent.Should().Contain("Section 1");
        textContent.Should().Contain("Point 1");
        textContent.Should().Contain("var example = true;");
        textContent.Should().Contain("Important note");
    }

    [Fact]
    public void AddHeading_WithShowBorderOnH2_ShouldRenderBorder()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        var style = new HeadingStyle
        {
            FontSize = 26,
            Color = "333333",
            Bold = true,
            ShowBorder = true,
            BorderColor = "e8a735",
            BorderSize = 16,
            BorderPosition = "bottom",
            SpaceBefore = "200",
            SpaceAfter = "200"
        };

        // Act
        builder.AddHeading(2, "H2 with border", style);
        builder.Save();

        // Assert
        _stream.Position = 0;
        using var doc = WordprocessingDocument.Open(_stream, false);
        var paragraph = doc.MainDocumentPart!.Document.Body!.Elements<Paragraph>().First();
        var borders = paragraph.ParagraphProperties?.ParagraphBorders;
        borders.Should().NotBeNull();
        borders!.Elements<BottomBorder>().Should().NotBeEmpty();
    }

    [Fact]
    public void AddHeading_WithBorderPositionLeft_ShouldRenderLeftBorder()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        var style = new HeadingStyle
        {
            FontSize = 26,
            Color = "333333",
            Bold = true,
            ShowBorder = true,
            BorderColor = "4a90e2",
            BorderSize = 24,
            BorderPosition = "left",
            SpaceBefore = "200",
            SpaceAfter = "200"
        };

        // Act
        builder.AddHeading(3, "H3 with left border", style);
        builder.Save();

        // Assert
        _stream.Position = 0;
        using var doc = WordprocessingDocument.Open(_stream, false);
        var paragraph = doc.MainDocumentPart!.Document.Body!.Elements<Paragraph>().First();
        var borders = paragraph.ParagraphProperties?.ParagraphBorders;
        borders.Should().NotBeNull();
        borders!.Elements<LeftBorder>().Should().NotBeEmpty();
    }

    [Fact]
    public void AddHeading_WithBackgroundColor_ShouldRenderShading()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        var style = new HeadingStyle
        {
            FontSize = 22,
            Color = "ffffff",
            Bold = true,
            ShowBorder = false,
            BackgroundColor = "f0b64d",
            SpaceBefore = "200",
            SpaceAfter = "200"
        };

        // Act
        builder.AddHeading(3, "H3 with background", style);
        builder.Save();

        // Assert
        _stream.Position = 0;
        using var doc = WordprocessingDocument.Open(_stream, false);
        var paragraph = doc.MainDocumentPart!.Document.Body!.Elements<Paragraph>().First();
        var shading = paragraph.ParagraphProperties?.Shading;
        shading.Should().NotBeNull();
        shading!.Fill!.Value.Should().Be("f0b64d");
    }

    [Fact]
    public void AddHeading_WithNullBackgroundColor_ShouldNotRenderShading()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        var style = new HeadingStyle
        {
            FontSize = 32,
            Color = "333333",
            Bold = true,
            ShowBorder = false,
            BackgroundColor = null,
            SpaceBefore = "240",
            SpaceAfter = "120"
        };

        // Act
        builder.AddHeading(1, "H1 without background", style);
        builder.Save();

        // Assert
        _stream.Position = 0;
        using var doc = WordprocessingDocument.Open(_stream, false);
        var paragraph = doc.MainDocumentPart!.Document.Body!.Elements<Paragraph>().First();
        var shading = paragraph.ParagraphProperties?.Shading;
        shading.Should().BeNull();
    }

    [Fact]
    public void AddHeading_WithDefaultBorderPosition_ShouldRenderBottomBorder()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        var style = new HeadingStyle
        {
            FontSize = 32,
            Color = "333333",
            Bold = true,
            ShowBorder = true,
            BorderColor = "e8a735",
            BorderSize = 32,
            // BorderPosition not set - should default to "bottom"
            SpaceBefore = "240",
            SpaceAfter = "240"
        };

        // Act
        builder.AddHeading(1, "H1 default border", style);
        builder.Save();

        // Assert
        _stream.Position = 0;
        using var doc = WordprocessingDocument.Open(_stream, false);
        var paragraph = doc.MainDocumentPart!.Document.Body!.Elements<Paragraph>().First();
        var borders = paragraph.ParagraphProperties?.ParagraphBorders;
        borders.Should().NotBeNull();
        borders!.Elements<BottomBorder>().Should().NotBeEmpty();
        var bottomBorder = borders.Elements<BottomBorder>().First();
        bottomBorder.Color!.Value.Should().Be("e8a735");
    }

    [Fact]
    public void AddHeading_WithPageBreakBefore_ShouldRenderPageBreak()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        var style = new HeadingStyle
        {
            FontSize = 32,
            Color = "333333",
            Bold = true,
            PageBreakBefore = true,
            SpaceBefore = "240",
            SpaceAfter = "120"
        };

        // Act
        builder.AddHeading(1, "Chapter 1", style);
        builder.Save();

        // Assert
        _stream.Position = 0;
        using var doc = WordprocessingDocument.Open(_stream, false);
        var paragraph = doc.MainDocumentPart!.Document.Body!.Elements<Paragraph>().First();
        var pageBreak = paragraph.ParagraphProperties?.Elements<PageBreakBefore>().FirstOrDefault();
        pageBreak.Should().NotBeNull();
    }

    [Fact]
    public void AddHeading_WithPageBreakBeforeFalse_ShouldNotRenderPageBreak()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        var style = new HeadingStyle
        {
            FontSize = 32,
            Color = "333333",
            Bold = true,
            PageBreakBefore = false,
            SpaceBefore = "240",
            SpaceAfter = "120"
        };

        // Act
        builder.AddHeading(1, "Chapter 1", style);
        builder.Save();

        // Assert
        _stream.Position = 0;
        using var doc = WordprocessingDocument.Open(_stream, false);
        var paragraph = doc.MainDocumentPart!.Document.Body!.Elements<Paragraph>().First();
        var pageBreak = paragraph.ParagraphProperties?.Elements<PageBreakBefore>().FirstOrDefault();
        pageBreak.Should().BeNull();
    }

    [Fact]
    public void AddHeading_WithDefaultStyle_ShouldNotRenderPageBreak()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        var style = CreateDefaultHeadingStyle();

        // Act
        builder.AddHeading(1, "Default Heading", style);
        builder.Save();

        // Assert
        _stream.Position = 0;
        using var doc = WordprocessingDocument.Open(_stream, false);
        var paragraph = doc.MainDocumentPart!.Document.Body!.Elements<Paragraph>().First();
        var pageBreak = paragraph.ParagraphProperties?.Elements<PageBreakBefore>().FirstOrDefault();
        pageBreak.Should().BeNull();
    }

    [Fact]
    public void AddHeading_WithPageBreakBeforeAndShowBorder_ShouldRenderBoth()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        var style = new HeadingStyle
        {
            FontSize = 32,
            Color = "333333",
            Bold = true,
            PageBreakBefore = true,
            ShowBorder = true,
            BorderColor = "e8a735",
            BorderSize = 16,
            BorderPosition = "bottom",
            SpaceBefore = "240",
            SpaceAfter = "120"
        };

        // Act
        builder.AddHeading(1, "Chapter With Border", style);
        builder.Save();

        // Assert
        _stream.Position = 0;
        using var doc = WordprocessingDocument.Open(_stream, false);
        var paragraph = doc.MainDocumentPart!.Document.Body!.Elements<Paragraph>().First();
        paragraph.ParagraphProperties?.Elements<PageBreakBefore>().Should().HaveCount(1);
        paragraph.ParagraphProperties?.ParagraphBorders.Should().NotBeNull();
    }

    [Fact]
    public void AddHeading_SecondHeadingWithPageBreakBefore_ShouldBreakOnlyThatHeading()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        var styleNoBreak = new HeadingStyle
        {
            FontSize = 32,
            Color = "333333",
            Bold = true,
            PageBreakBefore = false,
            SpaceBefore = "240",
            SpaceAfter = "120"
        };
        var styleWithBreak = new HeadingStyle
        {
            FontSize = 32,
            Color = "333333",
            Bold = true,
            PageBreakBefore = true,
            SpaceBefore = "240",
            SpaceAfter = "120"
        };

        // Act
        builder.AddHeading(1, "Chapter 1", styleNoBreak);
        builder.AddHeading(1, "Chapter 2", styleWithBreak);
        builder.Save();

        // Assert
        _stream.Position = 0;
        using var doc = WordprocessingDocument.Open(_stream, false);
        var paragraphs = doc.MainDocumentPart!.Document.Body!.Elements<Paragraph>().ToList();
        paragraphs[0].ParagraphProperties?.Elements<PageBreakBefore>().Should().BeEmpty();
        paragraphs[1].ParagraphProperties?.Elements<PageBreakBefore>().Should().HaveCount(1);
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(2, 1)]
    [InlineData(3, 2)]
    [InlineData(4, 3)]
    [InlineData(5, 4)]
    [InlineData(6, 5)]
    public void AddHeading_ShouldSetOutlineLevelBasedOnHeadingLevel(int headingLevel, int expectedOutlineLevel)
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        var style = CreateDefaultHeadingStyle();

        // Act
        builder.AddHeading(headingLevel, $"Heading {headingLevel}", style);
        builder.Save();

        // Assert
        _stream.Position = 0;
        using var doc = WordprocessingDocument.Open(_stream, false);
        var paragraph = doc.MainDocumentPart!.Document.Body!.Elements<Paragraph>().First();
        var outlineLevel = paragraph.ParagraphProperties?.Elements<OutlineLevel>().FirstOrDefault();
        outlineLevel.Should().NotBeNull();
        outlineLevel!.Val!.Value.Should().Be(expectedOutlineLevel);
    }

    [Fact]
    public void AddHeading_MultipleHeadings_ShouldEachHaveCorrectOutlineLevel()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        var style = CreateDefaultHeadingStyle();

        // Act
        builder.AddHeading(1, "Chapter", style);
        builder.AddHeading(2, "Section", style);
        builder.AddHeading(3, "Subsection", style);
        builder.Save();

        // Assert
        _stream.Position = 0;
        using var doc = WordprocessingDocument.Open(_stream, false);
        var paragraphs = doc.MainDocumentPart!.Document.Body!.Elements<Paragraph>().ToList();
        paragraphs[0].ParagraphProperties?.Elements<OutlineLevel>().First().Val!.Value.Should().Be(0);
        paragraphs[1].ParagraphProperties?.Elements<OutlineLevel>().First().Val!.Value.Should().Be(1);
        paragraphs[2].ParagraphProperties?.Elements<OutlineLevel>().First().Val!.Value.Should().Be(2);
    }

    [Fact]
    public void AddTableOfContents_WithNullStyle_ShouldThrowArgumentNullException()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);

        // Act
        Action act = () => builder.AddTableOfContents(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("style");
    }

    [Fact]
    public void AddTableOfContents_WhenDisabled_ShouldNotAddContent()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        var style = new TableOfContentsStyle { Enabled = false };

        // Act
        builder.AddTableOfContents(style);
        builder.Save();

        // Assert
        _stream.Position = 0;
        using var doc = WordprocessingDocument.Open(_stream, false);
        var paragraphs = doc.MainDocumentPart!.Document.Body!.Elements<Paragraph>().ToList();
        paragraphs.Should().BeEmpty();
    }

    [Fact]
    public void AddTableOfContents_WhenEnabled_ShouldAddFieldCode()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        var style = new TableOfContentsStyle { Enabled = true, Depth = 3 };

        // Act
        builder.AddTableOfContents(style);
        builder.Save();

        // Assert
        _stream.Position = 0;
        using var doc = WordprocessingDocument.Open(_stream, false);
        var body = doc.MainDocumentPart!.Document.Body!;

        // Should have field chars (begin, separate, end)
        var fieldChars = body.Descendants<FieldChar>().ToList();
        fieldChars.Should().HaveCount(3);
        fieldChars[0].FieldCharType!.Value.Should().Be(FieldCharValues.Begin);
        fieldChars[1].FieldCharType!.Value.Should().Be(FieldCharValues.Separate);
        fieldChars[2].FieldCharType!.Value.Should().Be(FieldCharValues.End);

        // Should have instruction text with correct depth
        var instrText = body.Descendants<FieldCode>().FirstOrDefault();
        instrText.Should().NotBeNull();
        instrText!.Text.Should().Contain("TOC");
        instrText.Text.Should().Contain("1-3");

        // Should have placeholder text between separate and end markers
        var texts = body.Descendants<Text>().ToList();
        texts.Should().Contain(t => t.Text.Contains("Update Field"));
    }

    [Theory]
    [InlineData(1, "1-1")]
    [InlineData(2, "1-2")]
    [InlineData(4, "1-4")]
    [InlineData(6, "1-6")]
    public void AddTableOfContents_WithVariousDepths_ShouldUseCorrectDepthInFieldCode(int depth, string expectedRange)
    {
        // Arrange
        using var stream = new MemoryStream();
        using var builder = new OpenXmlDocumentBuilder(stream, _horizontalProvider);
        var style = new TableOfContentsStyle { Enabled = true, Depth = depth };

        // Act
        builder.AddTableOfContents(style);
        builder.Save();

        // Assert
        stream.Position = 0;
        using var doc = WordprocessingDocument.Open(stream, false);
        var instrText = doc.MainDocumentPart!.Document.Body!.Descendants<FieldCode>().First();
        instrText.Text.Should().Contain(expectedRange);
    }

    [Fact]
    public void AddTableOfContents_WithTitle_ShouldAddTitleParagraph()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        var style = new TableOfContentsStyle { Enabled = true, Title = "Contents" };

        // Act
        builder.AddTableOfContents(style);
        builder.Save();

        // Assert
        _stream.Position = 0;
        using var doc = WordprocessingDocument.Open(_stream, false);
        var texts = doc.MainDocumentPart!.Document.Body!.Descendants<Text>().ToList();
        texts.Should().Contain(t => t.Text == "Contents");
    }

    [Fact]
    public void AddTableOfContents_WithoutTitle_ShouldNotAddTitleParagraph()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        var style = new TableOfContentsStyle { Enabled = true, Title = null };

        // Act
        builder.AddTableOfContents(style);
        builder.Save();

        // Assert
        _stream.Position = 0;
        using var doc = WordprocessingDocument.Open(_stream, false);
        var texts = doc.MainDocumentPart!.Document.Body!.Descendants<Text>().ToList();
        // No title text, only placeholder instruction text
        texts.Should().NotContain(t => t.Text == "Contents");
        texts.Should().Contain(t => t.Text.Contains("Update Field"));
    }

    [Fact]
    public void AddTableOfContents_WithPageBreakAfter_ShouldAddPageBreak()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        var style = new TableOfContentsStyle { Enabled = true, PageBreakAfter = true };

        // Act
        builder.AddTableOfContents(style);
        builder.Save();

        // Assert
        _stream.Position = 0;
        using var doc = WordprocessingDocument.Open(_stream, false);
        var breaks = doc.MainDocumentPart!.Document.Body!.Descendants<Break>().ToList();
        breaks.Should().ContainSingle();
        breaks[0].Type!.Value.Should().Be(BreakValues.Page);
    }

    [Fact]
    public void AddTableOfContents_WithoutPageBreakAfter_ShouldNotAddPageBreak()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        var style = new TableOfContentsStyle { Enabled = true, PageBreakAfter = false };

        // Act
        builder.AddTableOfContents(style);
        builder.Save();

        // Assert
        _stream.Position = 0;
        using var doc = WordprocessingDocument.Open(_stream, false);
        var breaks = doc.MainDocumentPart!.Document.Body!.Descendants<Break>().ToList();
        breaks.Should().BeEmpty();
    }

    [Fact]
    public void AddHeading_WithBorderExtentText_ShouldCreateThreeParagraphs()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        var style = new HeadingStyle
        {
            FontSize = 32,
            Color = "333333",
            Bold = true,
            ShowBorder = true,
            BorderColor = "e8a735",
            BorderSize = 32,
            BorderPosition = "bottom",
            BorderExtent = "text",
            SpaceBefore = "240",
            SpaceAfter = "240"
        };

        // Act
        builder.AddHeading(1, "Bordered Heading", style);
        builder.Save();

        // Assert
        _stream.Position = 0;
        using var doc = WordprocessingDocument.Open(_stream, false);
        var paragraphs = doc.MainDocumentPart!.Document.Body!.Elements<Paragraph>().ToList();
        paragraphs.Should().HaveCount(3, "before-spacer + main heading + after-spacer");
    }

    [Fact]
    public void AddHeading_WithBorderExtentText_OutlineLevelOnlyOnMainParagraph()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        var style = new HeadingStyle
        {
            FontSize = 32,
            Color = "333333",
            Bold = true,
            ShowBorder = true,
            BorderColor = "e8a735",
            BorderSize = 32,
            BorderExtent = "text",
            SpaceBefore = "240",
            SpaceAfter = "240"
        };

        // Act
        builder.AddHeading(2, "Heading Level 2", style);
        builder.Save();

        // Assert
        _stream.Position = 0;
        using var doc = WordprocessingDocument.Open(_stream, false);
        var paragraphs = doc.MainDocumentPart!.Document.Body!.Elements<Paragraph>().ToList();

        // Before-spacer: no OutlineLevel
        paragraphs[0].ParagraphProperties?.Elements<OutlineLevel>().Should().BeEmpty();
        // Main heading: OutlineLevel = 1 (H2)
        var outlineLevel = paragraphs[1].ParagraphProperties?.Elements<OutlineLevel>().FirstOrDefault();
        outlineLevel.Should().NotBeNull();
        outlineLevel!.Val!.Value.Should().Be(1);
        // After-spacer: no OutlineLevel
        paragraphs[2].ParagraphProperties?.Elements<OutlineLevel>().Should().BeEmpty();
    }

    [Fact]
    public void AddHeading_WithBorderExtentText_BorderOnlyOnMainParagraph()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        var style = new HeadingStyle
        {
            FontSize = 32,
            Color = "333333",
            Bold = true,
            ShowBorder = true,
            BorderColor = "e8a735",
            BorderSize = 32,
            BorderPosition = "bottom",
            BorderExtent = "text",
            SpaceBefore = "240",
            SpaceAfter = "240"
        };

        // Act
        builder.AddHeading(1, "Bordered", style);
        builder.Save();

        // Assert
        _stream.Position = 0;
        using var doc = WordprocessingDocument.Open(_stream, false);
        var paragraphs = doc.MainDocumentPart!.Document.Body!.Elements<Paragraph>().ToList();

        // Before-spacer: no border
        paragraphs[0].ParagraphProperties?.ParagraphBorders.Should().BeNull();
        // Main heading: has border
        paragraphs[1].ParagraphProperties?.ParagraphBorders.Should().NotBeNull();
        paragraphs[1].ParagraphProperties!.ParagraphBorders!.Elements<BottomBorder>().Should().NotBeEmpty();
        // After-spacer: no border
        paragraphs[2].ParagraphProperties?.ParagraphBorders.Should().BeNull();
    }

    [Fact]
    public void AddHeading_WithBorderExtentText_MainParagraphHasZeroSpacing()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        var style = new HeadingStyle
        {
            FontSize = 32,
            Color = "333333",
            Bold = true,
            ShowBorder = true,
            BorderColor = "e8a735",
            BorderSize = 32,
            BorderExtent = "text",
            SpaceBefore = "240",
            SpaceAfter = "240"
        };

        // Act
        builder.AddHeading(1, "Zero Spacing Main", style);
        builder.Save();

        // Assert
        _stream.Position = 0;
        using var doc = WordprocessingDocument.Open(_stream, false);
        var paragraphs = doc.MainDocumentPart!.Document.Body!.Elements<Paragraph>().ToList();

        var mainSpacing = paragraphs[1].ParagraphProperties?.Elements<SpacingBetweenLines>().FirstOrDefault();
        mainSpacing.Should().NotBeNull();
        mainSpacing!.Before!.Value.Should().Be("0");
        mainSpacing.After!.Value.Should().Be("0");
    }

    [Fact]
    public void AddHeading_WithBorderExtentParagraph_ShouldStaySingleParagraph()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        var style = new HeadingStyle
        {
            FontSize = 32,
            Color = "333333",
            Bold = true,
            ShowBorder = true,
            BorderColor = "e8a735",
            BorderSize = 32,
            BorderExtent = "paragraph",
            SpaceBefore = "240",
            SpaceAfter = "240"
        };

        // Act
        builder.AddHeading(1, "Single Paragraph", style);
        builder.Save();

        // Assert
        _stream.Position = 0;
        using var doc = WordprocessingDocument.Open(_stream, false);
        var paragraphs = doc.MainDocumentPart!.Document.Body!.Elements<Paragraph>().ToList();
        paragraphs.Should().HaveCount(1);
    }

    [Fact]
    public void AddHeading_WithBorderExtentText_PageBreakBeforeGoesOnBeforeSpacer()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        var style = new HeadingStyle
        {
            FontSize = 32,
            Color = "333333",
            Bold = true,
            ShowBorder = true,
            BorderColor = "e8a735",
            BorderSize = 32,
            BorderExtent = "text",
            PageBreakBefore = true,
            SpaceBefore = "240",
            SpaceAfter = "240"
        };

        // Act
        builder.AddHeading(1, "Page Break Heading", style);
        builder.Save();

        // Assert
        _stream.Position = 0;
        using var doc = WordprocessingDocument.Open(_stream, false);
        var paragraphs = doc.MainDocumentPart!.Document.Body!.Elements<Paragraph>().ToList();
        paragraphs.Should().HaveCount(3);

        // PageBreakBefore on before-spacer
        paragraphs[0].ParagraphProperties?.Elements<PageBreakBefore>().Should().HaveCount(1);
        // Not on main heading
        paragraphs[1].ParagraphProperties?.Elements<PageBreakBefore>().Should().BeEmpty();
    }

    [Fact]
    public void AddHeading_WithBorderExtentTextAndShowBorderFalse_ShouldStaySingleParagraph()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        var style = new HeadingStyle
        {
            FontSize = 32,
            Color = "333333",
            Bold = true,
            ShowBorder = false,
            BorderExtent = "text",
            SpaceBefore = "240",
            SpaceAfter = "240"
        };

        // Act
        builder.AddHeading(1, "No Border Text Extent", style);
        builder.Save();

        // Assert
        _stream.Position = 0;
        using var doc = WordprocessingDocument.Open(_stream, false);
        var paragraphs = doc.MainDocumentPart!.Document.Body!.Elements<Paragraph>().ToList();
        paragraphs.Should().HaveCount(1, "BorderExtent=text has no effect when ShowBorder=false");
    }

    [Fact]
    public void AddHeading_WithBorderPositionRight_ShouldRenderRightBorder()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        var style = new HeadingStyle
        {
            FontSize = 26,
            Color = "333333",
            Bold = true,
            ShowBorder = true,
            BorderColor = "4a90e2",
            BorderSize = 16,
            BorderPosition = "right",
            SpaceBefore = "200",
            SpaceAfter = "200"
        };

        // Act
        builder.AddHeading(2, "H2 with right border", style);
        builder.Save();

        // Assert
        _stream.Position = 0;
        using var doc = WordprocessingDocument.Open(_stream, false);
        var paragraph = doc.MainDocumentPart!.Document.Body!.Elements<Paragraph>().First();
        var borders = paragraph.ParagraphProperties?.ParagraphBorders;
        borders.Should().NotBeNull();
        borders!.Elements<RightBorder>().Should().NotBeEmpty();
    }

    [Fact]
    public void AddHeading_WithBorderPositionTop_ShouldRenderTopBorder()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        var style = new HeadingStyle
        {
            FontSize = 26,
            Color = "333333",
            Bold = true,
            ShowBorder = true,
            BorderColor = "e8a735",
            BorderSize = 16,
            BorderPosition = "top",
            SpaceBefore = "200",
            SpaceAfter = "200"
        };

        // Act
        builder.AddHeading(2, "H2 with top border", style);
        builder.Save();

        // Assert
        _stream.Position = 0;
        using var doc = WordprocessingDocument.Open(_stream, false);
        var paragraph = doc.MainDocumentPart!.Document.Body!.Elements<Paragraph>().First();
        var borders = paragraph.ParagraphProperties?.ParagraphBorders;
        borders.Should().NotBeNull();
        borders!.Elements<TopBorder>().Should().NotBeEmpty();
    }

    [Fact]
    public void AddHeading_WithAllBorderPositions_ShouldRenderAllBorders()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        var style = new HeadingStyle
        {
            FontSize = 26,
            Color = "333333",
            Bold = true,
            ShowBorder = true,
            BorderColor = "333333",
            BorderSize = 12,
            BorderPosition = "left,right,top,bottom",
            SpaceBefore = "200",
            SpaceAfter = "200"
        };

        // Act
        builder.AddHeading(1, "Heading with all borders", style);
        builder.Save();

        // Assert
        _stream.Position = 0;
        using var doc = WordprocessingDocument.Open(_stream, false);
        var paragraph = doc.MainDocumentPart!.Document.Body!.Elements<Paragraph>().First();
        var borders = paragraph.ParagraphProperties?.ParagraphBorders;
        borders.Should().NotBeNull();
        borders!.Elements<LeftBorder>().Should().NotBeEmpty();
        borders!.Elements<RightBorder>().Should().NotBeEmpty();
        borders!.Elements<TopBorder>().Should().NotBeEmpty();
        borders!.Elements<BottomBorder>().Should().NotBeEmpty();
    }

    [Fact]
    public void AddQuote_WithShowBorderFalse_ShouldNotRenderBorder()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        var style = new QuoteStyle
        {
            FontSize = 22,
            Color = "555555",
            Italic = true,
            ShowBorder = false,
            LeftIndent = "720",
            SpaceBefore = "120",
            SpaceAfter = "120"
        };

        // Act
        builder.AddQuote(ToRuns("Quote without border"), style);
        builder.Save();

        // Assert
        _stream.Position = 0;
        using var doc = WordprocessingDocument.Open(_stream, false);
        var paragraph = doc.MainDocumentPart!.Document.Body!.Elements<Paragraph>().First();
        var borders = paragraph.ParagraphProperties?.ParagraphBorders;
        borders.Should().BeNull();
    }

    [Fact]
    public void AddQuote_WithBackgroundColor_ShouldRenderShading()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        var style = new QuoteStyle
        {
            FontSize = 22,
            Color = "555555",
            Italic = true,
            ShowBorder = true,
            BorderPosition = "left",
            BorderColor = "3498db",
            BorderSize = 12,
            BackgroundColor = "f0f4f8",
            LeftIndent = "720",
            SpaceBefore = "120",
            SpaceAfter = "120"
        };

        // Act
        builder.AddQuote(ToRuns("Quote with background"), style);
        builder.Save();

        // Assert
        _stream.Position = 0;
        using var doc = WordprocessingDocument.Open(_stream, false);
        var paragraph = doc.MainDocumentPart!.Document.Body!.Elements<Paragraph>().First();
        var shading = paragraph.ParagraphProperties?.Shading;
        shading.Should().NotBeNull();
        shading!.Fill!.Value.Should().Be("f0f4f8");
    }

    [Fact]
    public void AddQuote_WithNoBorderAndNoBackground_ShouldRenderMinimalProperties()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        var style = new QuoteStyle
        {
            FontSize = 22,
            Color = "555555",
            Italic = false,
            ShowBorder = false,
            BackgroundColor = null,
            LeftIndent = "720",
            SpaceBefore = "120",
            SpaceAfter = "120"
        };

        // Act
        builder.AddQuote(ToRuns("Minimal quote"), style);
        builder.Save();

        // Assert
        _stream.Position = 0;
        using var doc = WordprocessingDocument.Open(_stream, false);
        var paragraph = doc.MainDocumentPart!.Document.Body!.Elements<Paragraph>().First();
        paragraph.ParagraphProperties?.ParagraphBorders.Should().BeNull();
        paragraph.ParagraphProperties?.Shading.Should().BeNull();
        var textContent = string.Join("", paragraph.Descendants<Text>().Select(t => t.Text));
        textContent.Should().Contain("Minimal quote");
    }

    [Fact]
    public void AddQuote_WithBoldRun_ShouldRenderBoldText()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        var style = CreateDefaultQuoteStyle();
        var runs = new List<InlineRun>
        {
            new InlineRun { Text = "normal " },
            new InlineRun { Text = "bold", Bold = true },
            new InlineRun { Text = " text" }
        };

        // Act
        builder.AddQuote(runs, style);
        builder.Save();

        // Assert
        _stream.Position = 0;
        using var doc = WordprocessingDocument.Open(_stream, false);
        var paragraph = doc.MainDocumentPart!.Document.Body!.Elements<Paragraph>().First();
        var allRuns = paragraph.Elements<Run>().ToList();
        allRuns.Should().HaveCount(3);
        allRuns[1].RunProperties?.Bold.Should().NotBeNull();
        var textContent = string.Join("", paragraph.Descendants<Text>().Select(t => t.Text));
        textContent.Should().Contain("normal ");
        textContent.Should().Contain("bold");
        textContent.Should().Contain(" text");
    }

    [Fact]
    public void AddQuote_WithCodeRun_ShouldRenderMonospaceFont()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        var style = CreateDefaultQuoteStyle();
        var runs = new List<InlineRun>
        {
            new InlineRun { Text = "Use " },
            new InlineRun { Text = "code()", IsCode = true },
            new InlineRun { Text = " here" }
        };

        // Act
        builder.AddQuote(runs, style);
        builder.Save();

        // Assert
        _stream.Position = 0;
        using var doc = WordprocessingDocument.Open(_stream, false);
        var paragraph = doc.MainDocumentPart!.Document.Body!.Elements<Paragraph>().First();
        var allRuns = paragraph.Elements<Run>().ToList();
        allRuns.Should().HaveCount(3);
        var codeRunFonts = allRuns[1].RunProperties?.RunFonts;
        codeRunFonts.Should().NotBeNull();
        codeRunFonts!.Ascii!.Value.Should().Be("Courier New");
    }

    [Fact]
    public void AddQuote_WithPaddingSpaceAndBackground_ShouldRenderInvisiblePaddingBorders()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        var style = new QuoteStyle
        {
            FontSize = 22,
            Color = "555555",
            Italic = false,
            ShowBorder = true,
            BorderPosition = "left",
            BorderColor = "3498db",
            BorderSize = 24,
            BackgroundColor = "f0f4f8",
            LeftIndent = "720",
            SpaceBefore = "120",
            SpaceAfter = "120",
            PaddingSpace = 4
        };

        // Act
        builder.AddQuote(ToRuns("Padded quote"), style);
        builder.Save();

        // Assert: top/right/bottom invisible borders should be present with background color
        _stream.Position = 0;
        using var doc = WordprocessingDocument.Open(_stream, false);
        var paragraph = doc.MainDocumentPart!.Document.Body!.Elements<Paragraph>().First();
        var borders = paragraph.ParagraphProperties?.ParagraphBorders;
        borders.Should().NotBeNull();
        borders!.GetFirstChild<TopBorder>()!.Space!.Value.Should().Be(4U);
        borders.GetFirstChild<TopBorder>()!.Color!.Value.Should().Be("f0f4f8");
        borders.GetFirstChild<RightBorder>()!.Space!.Value.Should().Be(4U);
        borders.GetFirstChild<BottomBorder>()!.Space!.Value.Should().Be(4U);
        // Left border should remain the visible border
        borders.GetFirstChild<LeftBorder>()!.Color!.Value.Should().Be("3498db");
    }

    [Fact]
    public void AddQuote_WithPaddingSpaceButNoBackground_ShouldNotRenderPaddingBorders()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        var style = new QuoteStyle
        {
            FontSize = 22,
            Color = "555555",
            ShowBorder = false,
            BackgroundColor = null,
            LeftIndent = "720",
            SpaceBefore = "120",
            SpaceAfter = "120",
            PaddingSpace = 4  // should be ignored without background
        };

        // Act
        builder.AddQuote(ToRuns("No padding without background"), style);
        builder.Save();

        // Assert: no borders rendered when ShowBorder=false and no BackgroundColor
        _stream.Position = 0;
        using var doc = WordprocessingDocument.Open(_stream, false);
        var paragraph = doc.MainDocumentPart!.Document.Body!.Elements<Paragraph>().First();
        paragraph.ParagraphProperties?.ParagraphBorders.Should().BeNull();
    }

    [Fact]
    public void AddHeading_WithLineSpacing_ShouldRenderExactSpacing()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        var style = new HeadingStyle
        {
            FontSize = 32,
            Color = "333333",
            Bold = true,
            ShowBorder = false,
            LineSpacing = "480",
            SpaceBefore = "240",
            SpaceAfter = "120"
        };

        // Act
        builder.AddHeading(1, "Heading with line spacing", style);
        builder.Save();

        // Assert
        _stream.Position = 0;
        using var doc = WordprocessingDocument.Open(_stream, false);
        var paragraph = doc.MainDocumentPart!.Document.Body!.Elements<Paragraph>().First();
        var spacing = paragraph.ParagraphProperties?.Elements<SpacingBetweenLines>().FirstOrDefault();
        spacing.Should().NotBeNull();
        spacing!.Line!.Value.Should().Be("480");
        spacing!.LineRule!.Value.Should().Be(LineSpacingRuleValues.Exact);
    }

    [Fact]
    public void AddHeading_WithBorderExtentText_NoSpaceBefore_NoPageBreak_ShouldSkipBeforeSpacer()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        var style = new HeadingStyle
        {
            FontSize = 32,
            Color = "333333",
            Bold = true,
            ShowBorder = true,
            BorderColor = "e8a735",
            BorderSize = 32,
            BorderPosition = "bottom",
            BorderExtent = "text",
            PageBreakBefore = false,
            SpaceBefore = "0",
            SpaceAfter = "240"
        };

        // Act
        builder.AddHeading(1, "No before spacer", style);
        builder.Save();

        // Assert
        _stream.Position = 0;
        using var doc = WordprocessingDocument.Open(_stream, false);
        var paragraphs = doc.MainDocumentPart!.Document.Body!.Elements<Paragraph>().ToList();
        // Should only have main heading + after-spacer (no before-spacer)
        paragraphs.Should().HaveCount(2, "no before-spacer when SpaceBefore=0 and PageBreakBefore=false");
    }

    [Fact]
    public void AddHeading_WithLeftIndent_ShouldRenderIndentation()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        var style = new HeadingStyle
        {
            FontSize = 32,
            Color = "333333",
            Bold = true,
            ShowBorder = true,
            BorderPosition = "left",
            BorderColor = "3498db",
            BorderSize = 24,
            SpaceBefore = "240",
            SpaceAfter = "120",
            LeftIndent = "400"
        };

        // Act
        builder.AddHeading(3, "H3 with left indent", style);
        builder.Save();

        // Assert
        _stream.Position = 0;
        using var doc = WordprocessingDocument.Open(_stream, false);
        var paragraph = doc.MainDocumentPart!.Document.Body!.Elements<Paragraph>().First();
        var indentation = paragraph.ParagraphProperties?.Elements<Indentation>().FirstOrDefault();
        indentation.Should().NotBeNull();
        indentation!.Left!.Value.Should().Be("400");
    }

    [Fact]
    public void AddHeading_WithDefaultLeftIndent_ShouldNotRenderIndentation()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        var style = CreateDefaultHeadingStyle();

        // Act
        builder.AddHeading(1, "Default heading", style);
        builder.Save();

        // Assert
        _stream.Position = 0;
        using var doc = WordprocessingDocument.Open(_stream, false);
        var paragraph = doc.MainDocumentPart!.Document.Body!.Elements<Paragraph>().First();
        var indentation = paragraph.ParagraphProperties?.Elements<Indentation>().FirstOrDefault();
        indentation.Should().BeNull();
    }

    [Fact]
    public void AddParagraph_WithCodeRun_ShouldUseConfiguredFont()
    {
        // Verifies that InlineCodeFontAscii/EastAsia is driven by ParagraphStyle, not hardcoded.
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        var style = new ParagraphStyle
        {
            FontSize = 22,
            Color = "333333",
            LineSpacing = "360",
            FirstLineIndent = "0",
            LeftIndent = "0",
            InlineCodeFontAscii = "Consolas",
            InlineCodeFontEastAsia = "MS Gothic"
        };
        var runs = new List<InlineRun>
        {
            new InlineRun { Text = "git status", IsCode = true }
        };

        // Act
        builder.AddParagraph(runs, style);
        builder.Save();

        // Assert
        _stream.Position = 0;
        using var doc = WordprocessingDocument.Open(_stream, false);
        var paragraph = doc.MainDocumentPart!.Document.Body!.Elements<Paragraph>().First();
        var codeRunFonts = paragraph.Elements<Run>().First().RunProperties?.RunFonts;
        codeRunFonts.Should().NotBeNull();
        codeRunFonts!.Ascii!.Value.Should().Be("Consolas");
        codeRunFonts!.EastAsia!.Value.Should().Be("MS Gothic");
    }

    [Fact]
    public void AddQuote_WithCodeRun_ShouldUseConfiguredFont()
    {
        // Verifies that InlineCodeFontAscii/EastAsia is driven by QuoteStyle, not hardcoded.
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        var style = new QuoteStyle
        {
            FontSize = 22,
            Color = "555555",
            ShowBorder = false,
            InlineCodeFontAscii = "JetBrains Mono",
            InlineCodeFontEastAsia = "Noto Sans Mono CJK JP"
        };
        var runs = new List<InlineRun>
        {
            new InlineRun { Text = "fmt.Println()", IsCode = true }
        };

        // Act
        builder.AddQuote(runs, style);
        builder.Save();

        // Assert
        _stream.Position = 0;
        using var doc = WordprocessingDocument.Open(_stream, false);
        var paragraph = doc.MainDocumentPart!.Document.Body!.Elements<Paragraph>().First();
        var codeRunFonts = paragraph.Elements<Run>().First().RunProperties?.RunFonts;
        codeRunFonts.Should().NotBeNull();
        codeRunFonts!.Ascii!.Value.Should().Be("JetBrains Mono");
        codeRunFonts!.EastAsia!.Value.Should().Be("Noto Sans Mono CJK JP");
    }

    [Fact]
    public void CoreStyleRecords_WithSameProperties_ShouldBeEqual()
    {
        // Verifies that new InlineCodeFont properties participate in record equality.
        var p1 = new ParagraphStyle { InlineCodeFontAscii = "Consolas" };
        var p2 = new ParagraphStyle { InlineCodeFontAscii = "Consolas" };
        p1.Should().Be(p2);

        var q1 = new QuoteStyle { InlineCodeFontAscii = "Consolas" };
        var q2 = new QuoteStyle { InlineCodeFontAscii = "Consolas" };
        q1.Should().Be(q2);

        var q3 = new QuoteStyle { InlineCodeFontAscii = "Other" };
        q1.Should().NotBe(q3);
    }

    [Fact]
    public void AddHeading_WithLeftIndentAndBorderExtentText_ShouldRenderIndentationOnMainParagraph()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        var style = new HeadingStyle
        {
            FontSize = 32,
            Color = "333333",
            Bold = true,
            ShowBorder = true,
            BorderPosition = "left",
            BorderColor = "3498db",
            BorderSize = 24,
            BorderExtent = "text",
            SpaceBefore = "240",
            SpaceAfter = "120",
            LeftIndent = "400"
        };

        // Act
        builder.AddHeading(3, "H3 spacer mode with indent", style);
        builder.Save();

        // Assert
        _stream.Position = 0;
        using var doc = WordprocessingDocument.Open(_stream, false);
        var paragraphs = doc.MainDocumentPart!.Document.Body!.Elements<Paragraph>().ToList();
        // BorderExtent=text produces before-spacer, main, after-spacer => 3 paragraphs
        paragraphs.Should().HaveCount(3);
        // Main paragraph is index 1 (after before-spacer)
        var mainParagraph = paragraphs[1];
        var indentation = mainParagraph.ParagraphProperties?.Elements<Indentation>().FirstOrDefault();
        indentation.Should().NotBeNull();
        indentation!.Left!.Value.Should().Be("400");
    }

    public void Dispose()
    {
        _stream?.Dispose();
        GC.SuppressFinalize(this);
    }

    // Helper methods to create default styles
    private static HeadingStyle CreateDefaultHeadingStyle() => new()
    {
        FontSize = 32,
        Color = "2c3e50",
        Bold = true,
        ShowBorder = false,
        BorderColor = "3498db",
        BorderSize = 12,
        SpaceBefore = "240",
        SpaceAfter = "120"
    };

    private static ParagraphStyle CreateDefaultParagraphStyle() => new()
    {
        FontSize = 22,
        Color = "333333",
        LineSpacing = "360",
        FirstLineIndent = "0",
        LeftIndent = "0"
    };

    private static ListStyle CreateDefaultListStyle() => new()
    {
        FontSize = 22,
        Color = "333333",
        LeftIndent = "720",
        HangingIndent = "360",
        SpaceBefore = "60",
        SpaceAfter = "60"
    };

    private static CodeBlockStyle CreateDefaultCodeBlockStyle() => new()
    {
        FontSize = 18,
        Color = "2c3e50",
        BackgroundColor = "f8f9fa",
        BorderColor = "dee2e6",
        MonospaceFontAscii = "Consolas",
        MonospaceFontEastAsia = "MS Gothic",
        SpaceBefore = "120",
        SpaceAfter = "120",
        LineSpacing = "300"
    };

    private static QuoteStyle CreateDefaultQuoteStyle() => new()
    {
        FontSize = 22,
        Color = "555555",
        Italic = true,
        ShowBorder = true,
        BorderPosition = "left",
        BorderColor = "3498db",
        BorderSize = 12,
        BackgroundColor = null,
        LeftIndent = "720",
        SpaceBefore = "120",
        SpaceAfter = "120"
    };

    private static List<InlineRun> ToRuns(string text) =>
        new List<InlineRun> { new InlineRun { Text = text } };

    // ── Table tests ────────────────────────────────────────────────────────

    [Fact]
    public void AddTable_WithNullTableData_ShouldThrowArgumentNullException()
    {
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        TableData? nullData = null;
        Action act = () => builder.AddTable(nullData!, CreateDefaultTableStyle());
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddTable_WithNullStyle_ShouldThrowArgumentNullException()
    {
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        CoreTableStyle? nullStyle = null;
        var tableData = new TableData { Rows = [], ColumnCount = 2 };
        Action act = () => builder.AddTable(tableData, nullStyle!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddTable_WithRows_ShouldAddTableElementToBody()
    {
        // Arrange
        using var stream = new MemoryStream();
        using var builder = new OpenXmlDocumentBuilder(stream, _horizontalProvider);

        var tableData = new TableData
        {
            ColumnCount = 2,
            Rows =
            [
                new TableRowData
                {
                    IsHeader = true,
                    Cells =
                    [
                        new TableCellData { Runs = [new InlineRun { Text = "Column A" }], Alignment = "left" },
                        new TableCellData { Runs = [new InlineRun { Text = "Column B" }], Alignment = "center" }
                    ]
                },
                new TableRowData
                {
                    IsHeader = false,
                    Cells =
                    [
                        new TableCellData { Runs = [new InlineRun { Text = "Value 1" }], Alignment = "left" },
                        new TableCellData { Runs = [new InlineRun { Text = "Value 2" }], Alignment = "center" }
                    ]
                }
            ]
        };

        builder.AddTable(tableData, CreateDefaultTableStyle());
        builder.Save();

        // Assert: document contains a Table element
        stream.Position = 0;
        using var doc = WordprocessingDocument.Open(stream, false);
        var body = doc.MainDocumentPart!.Document.Body!;
        var table = body.Descendants<DocumentFormat.OpenXml.Wordprocessing.Table>().FirstOrDefault();
        table.Should().NotBeNull();
    }

    [Fact]
    public void AddTable_ShouldUsePercentageWidth()
    {
        // Arrange
        using var stream = new MemoryStream();
        using var builder = new OpenXmlDocumentBuilder(stream, _horizontalProvider);
        var tableData = new TableData
        {
            ColumnCount = 2,
            Rows = [new TableRowData { IsHeader = false, Cells = [new TableCellData { Runs = [new InlineRun { Text = "A" }] }] }]
        };

        builder.AddTable(tableData, CreateDefaultTableStyle());
        builder.Save();

        // Assert: w:tblW has type="pct" and w="5000"
        stream.Position = 0;
        using var doc = WordprocessingDocument.Open(stream, false);
        var body = doc.MainDocumentPart!.Document.Body!;
        var tblWidth = body.Descendants<DocumentFormat.OpenXml.Wordprocessing.Table>().First()
            .GetFirstChild<TableProperties>()!
            .GetFirstChild<TableWidth>();

        tblWidth.Should().NotBeNull();
        tblWidth!.Type!.Value.Should().Be(TableWidthUnitValues.Pct);
        tblWidth.Width!.Value.Should().Be("5000");
    }

    [Fact]
    public void AddTable_ShouldHaveFixedTableLayout()
    {
        // Arrange
        using var stream = new MemoryStream();
        using var builder = new OpenXmlDocumentBuilder(stream, _horizontalProvider);
        var tableData = new TableData
        {
            ColumnCount = 2,
            Rows = [new TableRowData { IsHeader = false, Cells = [new TableCellData { Runs = [new InlineRun { Text = "A" }] }, new TableCellData { Runs = [new InlineRun { Text = "B" }] }] }]
        };

        builder.AddTable(tableData, CreateDefaultTableStyle());
        builder.Save();

        // Assert: w:tblLayout has type="fixed"
        stream.Position = 0;
        using var doc = WordprocessingDocument.Open(stream, false);
        var body = doc.MainDocumentPart!.Document.Body!;
        var tblLayout = body.Descendants<DocumentFormat.OpenXml.Wordprocessing.Table>().First()
            .GetFirstChild<TableProperties>()!
            .GetFirstChild<TableLayout>();

        tblLayout.Should().NotBeNull("table must use fixed layout to prevent autofit overflow");
        tblLayout!.Type!.Value.Should().Be(TableLayoutValues.Fixed);
    }

    [Fact]
    public void AddTable_ShouldHaveTableGridWithCorrectColumnCount()
    {
        // Arrange
        using var stream = new MemoryStream();
        using var builder = new OpenXmlDocumentBuilder(stream, _horizontalProvider);
        const int expectedColumns = 3;
        var tableData = new TableData
        {
            ColumnCount = expectedColumns,
            Rows = [new TableRowData
            {
                IsHeader = false,
                Cells = [
                new TableCellData { Runs = [new InlineRun { Text = "A" }] },
                    new TableCellData { Runs = [new InlineRun { Text = "B" }] },
                    new TableCellData { Runs = [new InlineRun { Text = "C" }] }
            ]
            }]
        };

        builder.AddTable(tableData, CreateDefaultTableStyle());
        builder.Save();

        // Assert: w:tblGrid contains 3 w:gridCol elements
        stream.Position = 0;
        using var doc = WordprocessingDocument.Open(stream, false);
        var body = doc.MainDocumentPart!.Document.Body!;
        var grid = body.Descendants<DocumentFormat.OpenXml.Wordprocessing.Table>().First()
            .GetFirstChild<TableGrid>();

        grid.Should().NotBeNull("tblGrid is required for fixed-layout column width enforcement");
        grid!.Descendants<GridColumn>().Should().HaveCount(expectedColumns);
    }

    [Fact]
    public void AddTable_CellWidth_ShouldUseDxaType()
    {
        // Arrange
        using var stream = new MemoryStream();
        using var builder = new OpenXmlDocumentBuilder(stream, _horizontalProvider);
        var tableData = new TableData
        {
            ColumnCount = 2,
            Rows = [new TableRowData
            {
                IsHeader = false,
                Cells = [
                new TableCellData { Runs = [new InlineRun { Text = "A" }] },
                    new TableCellData { Runs = [new InlineRun { Text = "B" }] }
            ]
            }]
        };

        builder.AddTable(tableData, CreateDefaultTableStyle());
        builder.Save();

        // Assert: all w:tcW elements use type="dxa" (matches gridCol widths for fixed layout)
        stream.Position = 0;
        using var doc = WordprocessingDocument.Open(stream, false);
        var body = doc.MainDocumentPart!.Document.Body!;
        var cells = body.Descendants<TableCell>().ToList();
        cells.Should().NotBeEmpty();
        foreach (var cell in cells)
        {
            var tcWidth = cell.GetFirstChild<TableCellProperties>()!.GetFirstChild<TableCellWidth>();
            tcWidth.Should().NotBeNull();
            tcWidth!.Type!.Value.Should().Be(TableWidthUnitValues.Dxa,
                "cell width must use dxa to match tblGrid gridCol widths in fixed-layout tables");
        }
    }

    [Fact]
    public void AddTable_WithHeaderRow_ShouldApplyHeaderBackground()
    {
        // Arrange
        using var stream = new MemoryStream();
        using var builder = new OpenXmlDocumentBuilder(stream, _horizontalProvider);
        const string expectedColor = "2c3e50";
        var tableData = new TableData
        {
            ColumnCount = 1,
            Rows = [new TableRowData { IsHeader = true, Cells = [new TableCellData { Runs = [new InlineRun { Text = "Header" }] }] }]
        };
        var style = CreateDefaultTableStyle() with { HeaderBackgroundColor = expectedColor };

        builder.AddTable(tableData, style);
        builder.Save();

        // Assert: header cell has shading with expected fill color
        stream.Position = 0;
        using var doc = WordprocessingDocument.Open(stream, false);
        var body = doc.MainDocumentPart!.Document.Body!;
        var cell = body.Descendants<TableCell>().First();
        var shading = cell.GetFirstChild<TableCellProperties>()!.GetFirstChild<Shading>();
        shading.Should().NotBeNull();
        shading!.Fill!.Value.Should().Be(expectedColor);
    }

    [Fact]
    public void AddTable_WithCenterAlignment_ShouldApplyJustificationCenter()
    {
        // Arrange
        using var stream = new MemoryStream();
        using var builder = new OpenXmlDocumentBuilder(stream, _horizontalProvider);
        var tableData = new TableData
        {
            ColumnCount = 1,
            Rows = [new TableRowData { IsHeader = false, Cells = [new TableCellData { Runs = [new InlineRun { Text = "Centered" }], Alignment = "center" }] }]
        };

        builder.AddTable(tableData, CreateDefaultTableStyle());
        builder.Save();

        // Assert: cell paragraph has center justification
        stream.Position = 0;
        using var doc = WordprocessingDocument.Open(stream, false);
        var body = doc.MainDocumentPart!.Document.Body!;
        var para = body.Descendants<TableCell>().First().Descendants<Paragraph>().First();
        var jc = para.GetFirstChild<ParagraphProperties>()!.GetFirstChild<Justification>();
        jc.Should().NotBeNull();
        jc!.Val!.Value.Should().Be(JustificationValues.Center);
    }

    [Fact]
    public void AddTable_WithRightAlignment_ShouldApplyJustificationRight()
    {
        // Arrange
        using var stream = new MemoryStream();
        using var builder = new OpenXmlDocumentBuilder(stream, _horizontalProvider);
        var tableData = new TableData
        {
            ColumnCount = 1,
            Rows = [new TableRowData { IsHeader = false, Cells = [new TableCellData { Runs = [new InlineRun { Text = "Right" }], Alignment = "right" }] }]
        };

        builder.AddTable(tableData, CreateDefaultTableStyle());
        builder.Save();

        // Assert
        stream.Position = 0;
        using var doc = WordprocessingDocument.Open(stream, false);
        var body = doc.MainDocumentPart!.Document.Body!;
        var para = body.Descendants<TableCell>().First().Descendants<Paragraph>().First();
        var jc = para.GetFirstChild<ParagraphProperties>()!.GetFirstChild<Justification>();
        jc!.Val!.Value.Should().Be(JustificationValues.Right);
    }

    [Fact]
    public void AddTable_HeaderRow_ShouldHaveTableHeaderProperty()
    {
        // Arrange
        using var stream = new MemoryStream();
        using var builder = new OpenXmlDocumentBuilder(stream, _horizontalProvider);
        var tableData = new TableData
        {
            ColumnCount = 1,
            Rows = [new TableRowData { IsHeader = true, Cells = [new TableCellData { Runs = [new InlineRun { Text = "H" }] }] }]
        };

        builder.AddTable(tableData, CreateDefaultTableStyle());
        builder.Save();

        // Assert: header row has TableRowProperties with TableHeader
        stream.Position = 0;
        using var doc = WordprocessingDocument.Open(stream, false);
        var body = doc.MainDocumentPart!.Document.Body!;
        var row = body.Descendants<TableRow>().First();
        var trProps = row.GetFirstChild<TableRowProperties>();
        trProps.Should().NotBeNull();
        trProps!.GetFirstChild<TableHeader>().Should().NotBeNull();
    }

    private static CoreTableStyle CreateDefaultTableStyle() => new()
    {
        FontSize = 20,
        HeaderBackgroundColor = "2c3e50",
        HeaderTextColor = "ecf0f1",
        BodyTextColor = "2c3e50",
        BorderColor = "bdc3c7",
        BorderSize = 4,
        HeaderBold = true,
        CellPaddingTop = 40,
        CellPaddingBottom = 40,
        CellPaddingLeft = 80,
        CellPaddingRight = 80,
        SpaceBefore = "160",
        SpaceAfter = "160"
    };
}
