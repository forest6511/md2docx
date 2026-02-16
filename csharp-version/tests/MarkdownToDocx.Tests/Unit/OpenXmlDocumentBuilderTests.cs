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
        builder.AddParagraph("縦書きテスト", CreateDefaultParagraphStyle());
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

    public void Dispose()
    {
        _stream?.Dispose();
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
