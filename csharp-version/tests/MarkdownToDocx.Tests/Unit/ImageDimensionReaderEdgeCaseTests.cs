using FluentAssertions;
using MarkdownToDocx.Core.Imaging;
using Xunit;

namespace MarkdownToDocx.Tests.Unit;

/// <summary>
/// Edge case tests for ImageDimensionReader targeting uncovered branches
/// </summary>
public class ImageDimensionReaderEdgeCaseTests : IDisposable
{
    private readonly string _tempDir;

    public ImageDimensionReaderEdgeCaseTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"md2docx_img_edge_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    [Fact]
    public void GetPngDimensions_WithTruncatedFile_ShouldThrowInvalidDataException()
    {
        // Arrange - PNG with less than 24 bytes
        var path = Path.Combine(_tempDir, "truncated.png");
        File.WriteAllBytes(path, new byte[] { 137, 80, 78, 71, 13, 10, 26, 10, 0, 0, 0, 13 });

        // Act
        Action act = () => ImageDimensionReader.GetDimensions(path);

        // Assert
        act.Should().Throw<InvalidDataException>()
            .WithMessage("*too small*PNG*");
    }

    [Fact]
    public void GetPngDimensions_WithInvalidSignature_ShouldThrowInvalidDataException()
    {
        // Arrange - 24+ bytes but invalid PNG signature
        var path = Path.Combine(_tempDir, "invalid_sig.png");
        var data = new byte[24];
        data[0] = 0x00; // Wrong first byte (should be 137)
        File.WriteAllBytes(path, data);

        // Act
        Action act = () => ImageDimensionReader.GetDimensions(path);

        // Assert
        act.Should().Throw<InvalidDataException>()
            .WithMessage("*not a valid PNG*");
    }

    [Fact]
    public void GetJpegDimensions_WithInvalidSoiMarker_ShouldThrowInvalidDataException()
    {
        // Arrange - File that doesn't start with 0xFF 0xD8
        var path = Path.Combine(_tempDir, "invalid_soi.jpg");
        File.WriteAllBytes(path, new byte[] { 0x00, 0x00, 0xFF, 0xC0 });

        // Act
        Action act = () => ImageDimensionReader.GetDimensions(path);

        // Assert
        act.Should().Throw<InvalidDataException>()
            .WithMessage("*not a valid JPEG*");
    }

    [Fact]
    public void GetJpegDimensions_WithNoSofMarker_ShouldThrowInvalidDataException()
    {
        // Arrange - Valid SOI but no SOF marker, just reaches EOF
        var path = Path.Combine(_tempDir, "no_sof.jpg");
        using var ms = new MemoryStream();
        // SOI marker
        ms.Write(new byte[] { 0xFF, 0xD8 });
        // APP0 marker with small segment
        ms.Write(new byte[] { 0xFF, 0xE0, 0x00, 0x04, 0x00, 0x00 });
        // EOI marker (end without SOF)
        ms.Write(new byte[] { 0xFF, 0xD9 });
        File.WriteAllBytes(path, ms.ToArray());

        // Act
        Action act = () => ImageDimensionReader.GetDimensions(path);

        // Assert
        act.Should().Throw<InvalidDataException>()
            .WithMessage("*Could not find dimensions*");
    }

    [Fact]
    public void GetJpegDimensions_WithSof2Progressive_ShouldReturnDimensions()
    {
        // Arrange - JPEG with SOF2 (progressive) instead of SOF0
        var path = Path.Combine(_tempDir, "progressive.jpg");
        using var ms = new MemoryStream();
        // SOI
        ms.Write(new byte[] { 0xFF, 0xD8 });
        // SOF2 marker (progressive)
        ms.Write(new byte[] { 0xFF, 0xC2 });
        // Length (17 bytes for 3 components)
        ms.Write(new byte[] { 0x00, 0x11 });
        // Precision
        ms.WriteByte(8);
        // Height = 300 (big-endian)
        ms.WriteByte(0x01);
        ms.WriteByte(0x2C);
        // Width = 400 (big-endian)
        ms.WriteByte(0x01);
        ms.WriteByte(0x90);
        // 3 components
        ms.WriteByte(3);
        for (int i = 1; i <= 3; i++)
        {
            ms.WriteByte((byte)i);
            ms.WriteByte(0x11);
            ms.WriteByte(0);
        }
        File.WriteAllBytes(path, ms.ToArray());

        // Act
        var (width, height) = ImageDimensionReader.GetDimensions(path);

        // Assert
        width.Should().Be(400);
        height.Should().Be(300);
    }

    [Fact]
    public void GetJpegDimensions_WithMultipleSegmentsBeforeSof_ShouldReturnDimensions()
    {
        // Arrange - Multiple non-SOF segments before the SOF0
        var path = Path.Combine(_tempDir, "multi_segment.jpg");
        using var ms = new MemoryStream();
        // SOI
        ms.Write(new byte[] { 0xFF, 0xD8 });
        // APP0 segment
        ms.Write(new byte[] { 0xFF, 0xE0, 0x00, 0x10 });
        ms.Write(new byte[14]); // 16 - 2 = 14 bytes of data
        // APP1 segment
        ms.Write(new byte[] { 0xFF, 0xE1, 0x00, 0x08 });
        ms.Write(new byte[6]); // 8 - 2 = 6 bytes of data
        // DQT segment
        ms.Write(new byte[] { 0xFF, 0xDB, 0x00, 0x06 });
        ms.Write(new byte[4]); // 6 - 2 = 4 bytes of data
        // SOF0 marker
        ms.Write(new byte[] { 0xFF, 0xC0 });
        ms.Write(new byte[] { 0x00, 0x11 });
        ms.WriteByte(8);
        // Height = 480
        ms.WriteByte(0x01);
        ms.WriteByte(0xE0);
        // Width = 640
        ms.WriteByte(0x02);
        ms.WriteByte(0x80);
        ms.WriteByte(3);
        for (int i = 1; i <= 3; i++)
        {
            ms.WriteByte((byte)i);
            ms.WriteByte(0x11);
            ms.WriteByte(0);
        }
        File.WriteAllBytes(path, ms.ToArray());

        // Act
        var (width, height) = ImageDimensionReader.GetDimensions(path);

        // Assert
        width.Should().Be(640);
        height.Should().Be(480);
    }

    [Fact]
    public void GetJpegDimensions_WithTruncatedSofData_ShouldThrowInvalidDataException()
    {
        // Arrange - SOF0 marker but not enough data after it
        var path = Path.Combine(_tempDir, "truncated_sof.jpg");
        using var ms = new MemoryStream();
        // SOI
        ms.Write(new byte[] { 0xFF, 0xD8 });
        // SOF0 marker with truncated data (only 3 bytes instead of 7)
        ms.Write(new byte[] { 0xFF, 0xC0, 0x00, 0x05, 0x08 });
        File.WriteAllBytes(path, ms.ToArray());

        // Act
        Action act = () => ImageDimensionReader.GetDimensions(path);

        // Assert
        act.Should().Throw<InvalidDataException>()
            .WithMessage("*Could not find dimensions*");
    }

    [Fact]
    public void GetJpegDimensions_WithFfPaddingBytes_ShouldSkipPaddingAndReturnDimensions()
    {
        // Arrange - 0xFF padding bytes before the marker type byte
        var path = Path.Combine(_tempDir, "ff_padding.jpg");
        using var ms = new MemoryStream();
        // SOI
        ms.Write(new byte[] { 0xFF, 0xD8 });
        // Multiple 0xFF padding bytes followed by SOF0
        ms.Write(new byte[] { 0xFF, 0xFF, 0xFF, 0xC0 });
        // SOF0 data
        ms.Write(new byte[] { 0x00, 0x11 });
        ms.WriteByte(8);
        // Height = 100
        ms.WriteByte(0x00);
        ms.WriteByte(0x64);
        // Width = 200
        ms.WriteByte(0x00);
        ms.WriteByte(0xC8);
        ms.WriteByte(3);
        for (int i = 1; i <= 3; i++)
        {
            ms.WriteByte((byte)i);
            ms.WriteByte(0x11);
            ms.WriteByte(0);
        }
        File.WriteAllBytes(path, ms.ToArray());

        // Act
        var (width, height) = ImageDimensionReader.GetDimensions(path);

        // Assert
        width.Should().Be(200);
        height.Should().Be(100);
    }

    [Fact]
    public void GetJpegDimensions_WithTruncatedSegmentLength_ShouldThrowInvalidDataException()
    {
        // Arrange - Non-SOF segment with truncated length bytes
        var path = Path.Combine(_tempDir, "truncated_len.jpg");
        using var ms = new MemoryStream();
        // SOI
        ms.Write(new byte[] { 0xFF, 0xD8 });
        // APP0 marker followed by only 1 byte (need 2 for length)
        ms.Write(new byte[] { 0xFF, 0xE0, 0x00 });
        File.WriteAllBytes(path, ms.ToArray());

        // Act
        Action act = () => ImageDimensionReader.GetDimensions(path);

        // Assert
        act.Should().Throw<InvalidDataException>()
            .WithMessage("*Could not find dimensions*");
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
