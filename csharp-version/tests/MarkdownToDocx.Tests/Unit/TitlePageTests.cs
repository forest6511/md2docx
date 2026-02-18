using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using FluentAssertions;
using MarkdownToDocx.Core.Models;
using MarkdownToDocx.Core.OpenXml;
using MarkdownToDocx.Core.TextDirection;
using MarkdownToDocx.Styling.Models;
using MarkdownToDocx.Styling.Styling;
using Xunit;

namespace MarkdownToDocx.Tests.Unit;

/// <summary>
/// Unit tests for title page functionality (builder + style applicator)
/// </summary>
public class TitlePageTests : IDisposable
{
    private readonly string _tempDir;

    public TitlePageTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"md2docx_titlepage_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    #region OpenXmlDocumentBuilder.AddTitlePage Tests

    [Fact]
    public void AddTitlePage_WithNullStyle_ShouldThrowArgumentNullException()
    {
        // Arrange
        using var stream = new MemoryStream();
        using var builder = new OpenXmlDocumentBuilder(stream, new HorizontalTextProvider());

        // Act
        Action act = () => builder.AddTitlePage(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("style");
    }

    [Fact]
    public void AddTitlePage_WhenDisabled_ShouldNotAddContent()
    {
        // Arrange
        using var stream = new MemoryStream();
        using var builder = new OpenXmlDocumentBuilder(stream, new HorizontalTextProvider());
        var style = new TitlePageStyle { Enabled = false };

        // Act
        builder.AddTitlePage(style);
        builder.Save();

        // Assert
        stream.Position = 0;
        using var doc = WordprocessingDocument.Open(stream, false);
        var body = doc.MainDocumentPart!.Document.Body!;
        // Only SectionProperties should be present (no paragraphs added for title page)
        body.Elements<Paragraph>().Should().BeEmpty();
    }

    [Fact]
    public void AddTitlePage_WithNullImagePath_ShouldNotAddContent()
    {
        // Arrange
        using var stream = new MemoryStream();
        using var builder = new OpenXmlDocumentBuilder(stream, new HorizontalTextProvider());
        var style = new TitlePageStyle { Enabled = true, ImagePath = null };

        // Act
        builder.AddTitlePage(style);
        builder.Save();

        // Assert
        stream.Position = 0;
        using var doc = WordprocessingDocument.Open(stream, false);
        var body = doc.MainDocumentPart!.Document.Body!;
        body.Elements<Paragraph>().Should().BeEmpty();
    }

    [Fact]
    public void AddTitlePage_WithNonExistentImage_ShouldThrowFileNotFoundException()
    {
        // Arrange
        using var stream = new MemoryStream();
        using var builder = new OpenXmlDocumentBuilder(stream, new HorizontalTextProvider());
        var style = new TitlePageStyle
        {
            Enabled = true,
            ImagePath = "/nonexistent/cover.png"
        };

        // Act
        Action act = () => builder.AddTitlePage(style);

        // Assert
        act.Should().Throw<FileNotFoundException>();
    }

    [Fact]
    public void AddTitlePage_WithValidPng_ShouldAddCenteredImageAndPageBreak()
    {
        // Arrange
        var pngPath = CreateMinimalPng(800, 600);
        using var stream = new MemoryStream();
        using var builder = new OpenXmlDocumentBuilder(stream, new HorizontalTextProvider());
        var style = new TitlePageStyle
        {
            Enabled = true,
            ImagePath = pngPath,
            ImageMaxWidthPercent = 80,
            ImageMaxHeightPercent = 80,
            PageBreakAfter = true
        };

        // Act
        builder.AddTitlePage(style);
        builder.Save();

        // Assert
        stream.Position = 0;
        using var doc = WordprocessingDocument.Open(stream, false);
        var body = doc.MainDocumentPart!.Document.Body!;
        var paragraphs = body.Elements<Paragraph>().ToList();

        // Should have 2 paragraphs: image paragraph + page break paragraph
        paragraphs.Should().HaveCount(2);

        // First paragraph should be centered with a Drawing element
        var imageParagraph = paragraphs[0];
        var justification = imageParagraph.ParagraphProperties?.Justification;
        justification.Should().NotBeNull();
        justification!.Val!.Value.Should().Be(JustificationValues.Center);

        var drawing = imageParagraph.Descendants<Drawing>().FirstOrDefault();
        drawing.Should().NotBeNull();

        // Second paragraph should have page break
        var breakParagraph = paragraphs[1];
        var pageBreak = breakParagraph.Descendants<Break>().FirstOrDefault();
        pageBreak.Should().NotBeNull();
        pageBreak!.Type!.Value.Should().Be(BreakValues.Page);

        // Should have an image part
        doc.MainDocumentPart.ImageParts.Should().HaveCount(1);
    }

    [Fact]
    public void AddTitlePage_WithPageBreakAfterFalse_ShouldNotAddPageBreak()
    {
        // Arrange
        var pngPath = CreateMinimalPng(100, 100);
        using var stream = new MemoryStream();
        using var builder = new OpenXmlDocumentBuilder(stream, new HorizontalTextProvider());
        var style = new TitlePageStyle
        {
            Enabled = true,
            ImagePath = pngPath,
            PageBreakAfter = false
        };

        // Act
        builder.AddTitlePage(style);
        builder.Save();

        // Assert
        stream.Position = 0;
        using var doc = WordprocessingDocument.Open(stream, false);
        var body = doc.MainDocumentPart!.Document.Body!;
        var paragraphs = body.Elements<Paragraph>().ToList();

        // Only image paragraph, no page break
        paragraphs.Should().HaveCount(1);
        paragraphs[0].Descendants<Drawing>().Should().HaveCount(1);
    }

    [Fact]
    public void AddTitlePage_WithValidJpeg_ShouldAddImageWithJpegPart()
    {
        // Arrange
        var jpegPath = CreateMinimalJpeg(400, 300);
        using var stream = new MemoryStream();
        using var builder = new OpenXmlDocumentBuilder(stream, new HorizontalTextProvider());
        var style = new TitlePageStyle
        {
            Enabled = true,
            ImagePath = jpegPath,
            PageBreakAfter = false
        };

        // Act
        builder.AddTitlePage(style);
        builder.Save();

        // Assert
        stream.Position = 0;
        using var doc = WordprocessingDocument.Open(stream, false);
        doc.MainDocumentPart!.ImageParts.Should().HaveCount(1);
        var imagePart = doc.MainDocumentPart.ImageParts.First();
        imagePart.ContentType.Should().Be("image/jpeg");
    }

    #endregion

    #region StyleApplicator.ApplyTitlePageStyle Tests

    [Fact]
    public void ApplyTitlePageStyle_WithNullConfig_ShouldThrowArgumentNullException()
    {
        // Arrange
        var applicator = new StyleApplicator();

        // Act
        Action act = () => applicator.ApplyTitlePageStyle(null!, "/tmp/test.md");

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("config");
    }

    [Fact]
    public void ApplyTitlePageStyle_WithNullInputPath_ShouldThrowArgumentNullException()
    {
        // Arrange
        var applicator = new StyleApplicator();
        var config = new ConversionConfiguration();

        // Act
        Action act = () => applicator.ApplyTitlePageStyle(config, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("inputFilePath");
    }

    [Fact]
    public void ApplyTitlePageStyle_WithDefaults_ShouldReturnDisabledStyle()
    {
        // Arrange
        var applicator = new StyleApplicator();
        var config = new ConversionConfiguration();

        // Act
        var style = applicator.ApplyTitlePageStyle(config, "/tmp/test.md");

        // Assert
        style.Enabled.Should().BeFalse();
    }

    [Fact]
    public void ApplyTitlePageStyle_WithEnabledConfig_ShouldMapAllProperties()
    {
        // Arrange
        var applicator = new StyleApplicator();
        var imagePath = CreateMinimalPng(100, 100);
        var inputPath = Path.Combine(_tempDir, "test.md");
        File.WriteAllText(inputPath, "# Test");

        var config = new ConversionConfiguration
        {
            TitlePage = new TitlePageConfig
            {
                Enabled = true,
                ImagePath = Path.GetFileName(imagePath),
                ImageMaxWidthPercent = 70,
                ImageMaxHeightPercent = 60,
                PageBreakAfter = false
            }
        };

        // Act
        var style = applicator.ApplyTitlePageStyle(config, inputPath);

        // Assert
        style.Enabled.Should().BeTrue();
        style.ImagePath.Should().Be(imagePath);
        style.ImageMaxWidthPercent.Should().Be(70);
        style.ImageMaxHeightPercent.Should().Be(60);
        style.PageBreakAfter.Should().BeFalse();
    }

    [Fact]
    public void ApplyTitlePageStyle_WithCoverImageOverride_ShouldEnableAndOverridePath()
    {
        // Arrange
        var applicator = new StyleApplicator();
        var config = new ConversionConfiguration
        {
            TitlePage = new TitlePageConfig
            {
                Enabled = false,
                ImagePath = "original.png"
            }
        };
        var overridePath = "/absolute/path/cover.jpg";

        // Act
        var style = applicator.ApplyTitlePageStyle(config, "/tmp/test.md", overridePath);

        // Assert
        style.Enabled.Should().BeTrue();
        style.ImagePath.Should().Be(overridePath);
    }

    [Fact]
    public void ApplyTitlePageStyle_WithRelativePath_ShouldResolveAgainstInputFile()
    {
        // Arrange
        var applicator = new StyleApplicator();
        var inputPath = Path.Combine(_tempDir, "subdir", "test.md");
        Directory.CreateDirectory(Path.GetDirectoryName(inputPath)!);
        File.WriteAllText(inputPath, "# Test");

        var config = new ConversionConfiguration
        {
            TitlePage = new TitlePageConfig
            {
                Enabled = true,
                ImagePath = "../cover.png"
            }
        };

        // Act
        var style = applicator.ApplyTitlePageStyle(config, inputPath);

        // Assert
        style.Enabled.Should().BeTrue();
        var expectedPath = Path.GetFullPath(Path.Combine(_tempDir, "cover.png"));
        style.ImagePath.Should().Be(expectedPath);
    }

    [Theory]
    [InlineData(-10, 1)]
    [InlineData(0, 1)]
    [InlineData(150, 100)]
    public void ApplyTitlePageStyle_WithOutOfRangePercent_ShouldClamp(int inputPercent, int expectedPercent)
    {
        // Arrange
        var applicator = new StyleApplicator();
        var config = new ConversionConfiguration
        {
            TitlePage = new TitlePageConfig
            {
                Enabled = true,
                ImagePath = "/absolute/cover.png",
                ImageMaxWidthPercent = inputPercent,
                ImageMaxHeightPercent = inputPercent
            }
        };

        // Act
        var style = applicator.ApplyTitlePageStyle(config, "/tmp/test.md");

        // Assert
        style.ImageMaxWidthPercent.Should().Be(expectedPercent);
        style.ImageMaxHeightPercent.Should().Be(expectedPercent);
    }

    #endregion

    #region YAML Configuration Tests

    [Fact]
    public void TitlePageConfig_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var config = new TitlePageConfig();

        // Assert
        config.Enabled.Should().BeFalse();
        config.ImagePath.Should().BeNull();
        config.ImageMaxWidthPercent.Should().Be(80);
        config.ImageMaxHeightPercent.Should().Be(80);
        config.PageBreakAfter.Should().BeTrue();
    }

    [Fact]
    public void ConversionConfiguration_ShouldIncludeTitlePageConfig()
    {
        // Arrange & Act
        var config = new ConversionConfiguration();

        // Assert
        config.TitlePage.Should().NotBeNull();
        config.TitlePage.Enabled.Should().BeFalse();
    }

    #endregion

    #region Helper Methods

    private string CreateMinimalPng(int width, int height)
    {
        var path = Path.Combine(_tempDir, $"test_{width}x{height}_{Guid.NewGuid():N}.png");
        using var ms = new MemoryStream();

        // PNG signature
        ms.Write(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });

        // IHDR chunk
        var ihdrData = new byte[13];
        ihdrData[0] = (byte)(width >> 24);
        ihdrData[1] = (byte)(width >> 16);
        ihdrData[2] = (byte)(width >> 8);
        ihdrData[3] = (byte)width;
        ihdrData[4] = (byte)(height >> 24);
        ihdrData[5] = (byte)(height >> 16);
        ihdrData[6] = (byte)(height >> 8);
        ihdrData[7] = (byte)height;
        ihdrData[8] = 8; // bit depth
        ihdrData[9] = 2; // color type (RGB)

        WriteChunk(ms, "IHDR", ihdrData);
        WriteChunk(ms, "IDAT", new byte[] { 0x78, 0x01, 0x01, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x01 });
        WriteChunk(ms, "IEND", Array.Empty<byte>());

        File.WriteAllBytes(path, ms.ToArray());
        return path;
    }

    private string CreateMinimalJpeg(int width, int height)
    {
        var path = Path.Combine(_tempDir, $"test_{width}x{height}_{Guid.NewGuid():N}.jpg");
        using var ms = new MemoryStream();

        ms.Write(new byte[] { 0xFF, 0xD8 });
        ms.Write(new byte[] { 0xFF, 0xC0 });
        ms.Write(new byte[] { 0x00, 0x11 });
        ms.WriteByte(8);
        ms.WriteByte((byte)(height >> 8));
        ms.WriteByte((byte)height);
        ms.WriteByte((byte)(width >> 8));
        ms.WriteByte((byte)width);
        ms.WriteByte(3);
        for (int i = 1; i <= 3; i++)
        {
            ms.WriteByte((byte)i);
            ms.WriteByte(0x11);
            ms.WriteByte(0);
        }
        ms.Write(new byte[] { 0xFF, 0xD9 });

        File.WriteAllBytes(path, ms.ToArray());
        return path;
    }

    private static void WriteChunk(MemoryStream ms, string type, byte[] data)
    {
        var length = data.Length;
        ms.WriteByte((byte)(length >> 24));
        ms.WriteByte((byte)(length >> 16));
        ms.WriteByte((byte)(length >> 8));
        ms.WriteByte((byte)length);
        ms.Write(System.Text.Encoding.ASCII.GetBytes(type));
        ms.Write(data);
        ms.Write(new byte[] { 0, 0, 0, 0 }); // CRC placeholder
    }

    #endregion

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, true);
        }
        GC.SuppressFinalize(this);
    }
}
