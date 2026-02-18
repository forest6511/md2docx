using FluentAssertions;
using MarkdownToDocx.Core.Imaging;
using Xunit;

namespace MarkdownToDocx.Tests.Unit;

/// <summary>
/// Unit tests for ImageDimensionReader
/// </summary>
public class ImageDimensionReaderTests : IDisposable
{
    private readonly string _tempDir;

    public ImageDimensionReaderTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"md2docx_img_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    [Fact]
    public void GetDimensions_WithValidPng_ShouldReturnCorrectDimensions()
    {
        // Arrange - Create a minimal 100x50 PNG
        var pngPath = CreateMinimalPng(100, 50);

        // Act
        var (width, height) = ImageDimensionReader.GetDimensions(pngPath);

        // Assert
        width.Should().Be(100);
        height.Should().Be(50);
    }

    [Fact]
    public void GetDimensions_WithValidJpeg_ShouldReturnCorrectDimensions()
    {
        // Arrange - Create a minimal 200x150 JPEG
        var jpegPath = CreateMinimalJpeg(200, 150);

        // Act
        var (width, height) = ImageDimensionReader.GetDimensions(jpegPath);

        // Assert
        width.Should().Be(200);
        height.Should().Be(150);
    }

    [Fact]
    public void GetDimensions_WithNullPath_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => ImageDimensionReader.GetDimensions(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetDimensions_WithNonExistentFile_ShouldThrowFileNotFoundException()
    {
        // Act
        Action act = () => ImageDimensionReader.GetDimensions("/nonexistent/image.png");

        // Assert
        act.Should().Throw<FileNotFoundException>();
    }

    [Fact]
    public void GetDimensions_WithUnsupportedFormat_ShouldThrowNotSupportedException()
    {
        // Arrange
        var bmpPath = Path.Combine(_tempDir, "test.bmp");
        File.WriteAllBytes(bmpPath, new byte[100]);

        // Act
        Action act = () => ImageDimensionReader.GetDimensions(bmpPath);

        // Assert
        act.Should().Throw<NotSupportedException>();
    }

    [Theory]
    [InlineData(".png", "image/png")]
    [InlineData(".jpg", "image/jpeg")]
    [InlineData(".jpeg", "image/jpeg")]
    public void GetContentType_WithSupportedFormat_ShouldReturnCorrectMimeType(string extension, string expectedType)
    {
        // Arrange
        var filePath = $"test{extension}";

        // Act
        var contentType = ImageDimensionReader.GetContentType(filePath);

        // Assert
        contentType.Should().Be(expectedType);
    }

    [Fact]
    public void GetContentType_WithNullPath_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => ImageDimensionReader.GetContentType(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetContentType_WithUnsupportedFormat_ShouldThrowNotSupportedException()
    {
        // Act
        Action act = () => ImageDimensionReader.GetContentType("test.gif");

        // Assert
        act.Should().Throw<NotSupportedException>();
    }

    /// <summary>
    /// Creates a minimal valid PNG file with the specified dimensions
    /// </summary>
    private string CreateMinimalPng(int width, int height)
    {
        var path = Path.Combine(_tempDir, $"test_{width}x{height}.png");
        using var ms = new MemoryStream();

        // PNG signature
        ms.Write(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });

        // IHDR chunk
        var ihdrData = new byte[13];
        // Width (big-endian)
        ihdrData[0] = (byte)(width >> 24);
        ihdrData[1] = (byte)(width >> 16);
        ihdrData[2] = (byte)(width >> 8);
        ihdrData[3] = (byte)width;
        // Height (big-endian)
        ihdrData[4] = (byte)(height >> 24);
        ihdrData[5] = (byte)(height >> 16);
        ihdrData[6] = (byte)(height >> 8);
        ihdrData[7] = (byte)height;
        // Bit depth, color type, compression, filter, interlace
        ihdrData[8] = 8; // bit depth
        ihdrData[9] = 2; // color type (RGB)

        WriteChunk(ms, "IHDR", ihdrData);

        // Minimal IDAT chunk (empty compressed data)
        WriteChunk(ms, "IDAT", new byte[] { 0x78, 0x01, 0x01, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x01 });

        // IEND chunk
        WriteChunk(ms, "IEND", Array.Empty<byte>());

        File.WriteAllBytes(path, ms.ToArray());
        return path;
    }

    /// <summary>
    /// Creates a minimal valid JPEG file with the specified dimensions
    /// </summary>
    private string CreateMinimalJpeg(int width, int height)
    {
        var path = Path.Combine(_tempDir, $"test_{width}x{height}.jpg");
        using var ms = new MemoryStream();

        // SOI marker
        ms.Write(new byte[] { 0xFF, 0xD8 });

        // SOF0 marker (Start of Frame, baseline)
        ms.Write(new byte[] { 0xFF, 0xC0 });
        // Length (17 bytes for 3-component image)
        ms.Write(new byte[] { 0x00, 0x11 });
        // Precision
        ms.WriteByte(8);
        // Height (big-endian)
        ms.WriteByte((byte)(height >> 8));
        ms.WriteByte((byte)height);
        // Width (big-endian)
        ms.WriteByte((byte)(width >> 8));
        ms.WriteByte((byte)width);
        // Number of components (3 for RGB)
        ms.WriteByte(3);
        // Component info (3 components x 3 bytes each)
        for (int i = 1; i <= 3; i++)
        {
            ms.WriteByte((byte)i);  // Component ID
            ms.WriteByte(0x11);      // Sampling factors
            ms.WriteByte(0);         // Quantization table
        }

        // EOI marker
        ms.Write(new byte[] { 0xFF, 0xD9 });

        File.WriteAllBytes(path, ms.ToArray());
        return path;
    }

    private static void WriteChunk(MemoryStream ms, string type, byte[] data)
    {
        // Length (big-endian)
        var length = data.Length;
        ms.WriteByte((byte)(length >> 24));
        ms.WriteByte((byte)(length >> 16));
        ms.WriteByte((byte)(length >> 8));
        ms.WriteByte((byte)length);

        // Type
        var typeBytes = System.Text.Encoding.ASCII.GetBytes(type);
        ms.Write(typeBytes);

        // Data
        ms.Write(data);

        // CRC (simplified - just write zeros for test purposes)
        ms.Write(new byte[] { 0, 0, 0, 0 });
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, true);
        }
        GC.SuppressFinalize(this);
    }
}
