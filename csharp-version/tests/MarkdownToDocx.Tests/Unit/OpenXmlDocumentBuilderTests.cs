using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using FluentAssertions;
using MarkdownToDocx.Core.Interfaces;
using MarkdownToDocx.Core.Models;
using MarkdownToDocx.Core.OpenXml;
using MarkdownToDocx.Core.TextDirection;
using Xunit;
using CoreListItem = MarkdownToDocx.Core.Models.ListItem;

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
            .WithParameterName("text");
    }

    [Fact]
    public void AddParagraph_WithNullStyle_ShouldThrowArgumentNullException()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);

        // Act
        Action act = () => builder.AddParagraph("Test", null!);

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
        builder.AddParagraph("Test paragraph content.", style);
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
    public void AddQuote_WithNullText_ShouldThrowArgumentNullException()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);

        // Act
        Action act = () => builder.AddQuote(null!, CreateDefaultQuoteStyle());

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("text");
    }

    [Fact]
    public void AddQuote_WithNullStyle_ShouldThrowArgumentNullException()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);

        // Act
        Action act = () => builder.AddQuote("Test quote", null!);

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
        builder.AddQuote("This is a quoted text.", style);
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
    public void Save_ShouldPersistDocument()
    {
        // Arrange
        using var builder = new OpenXmlDocumentBuilder(_stream, _horizontalProvider);
        builder.AddParagraph("Test content", CreateDefaultParagraphStyle());

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
        builder.AddParagraph("Vertical text test", CreateDefaultParagraphStyle());
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
        builder.AddParagraph("Introduction paragraph.", CreateDefaultParagraphStyle());
        builder.AddHeading(2, "Section 1", CreateDefaultHeadingStyle());
        builder.AddList(new List<CoreListItem>
        {
            new CoreListItem { Text = "Point 1" },
            new CoreListItem { Text = "Point 2" }
        }, false, CreateDefaultListStyle());
        builder.AddCodeBlock("var example = true;", "csharp", CreateDefaultCodeBlockStyle());
        builder.AddQuote("Important note", CreateDefaultQuoteStyle());
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
        builder.AddQuote("Quote without border", style);
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
        builder.AddQuote("Quote with background", style);
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
        builder.AddQuote("Minimal quote", style);
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
}
