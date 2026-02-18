namespace MarkdownToDocx.Core.Imaging;

/// <summary>
/// Reads image dimensions from PNG and JPEG files without external dependencies.
/// Parses raw bytes to extract width/height from file headers.
/// </summary>
public static class ImageDimensionReader
{
    /// <summary>
    /// Gets the pixel dimensions of a PNG or JPEG image file
    /// </summary>
    /// <param name="filePath">Absolute path to the image file</param>
    /// <returns>Tuple of (Width, Height) in pixels</returns>
    /// <exception cref="ArgumentNullException">Thrown when filePath is null</exception>
    /// <exception cref="FileNotFoundException">Thrown when file does not exist</exception>
    /// <exception cref="NotSupportedException">Thrown for unsupported image formats</exception>
    public static (int Width, int Height) GetDimensions(string filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Image file not found: {filePath}", filePath);
        }

        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return extension switch
        {
            ".png" => GetPngDimensions(filePath),
            ".jpg" or ".jpeg" => GetJpegDimensions(filePath),
            _ => throw new NotSupportedException($"Unsupported image format: {extension}")
        };
    }

    /// <summary>
    /// Gets the MIME content type for an image file based on its extension
    /// </summary>
    /// <param name="filePath">Path to the image file</param>
    /// <returns>MIME content type string</returns>
    /// <exception cref="ArgumentNullException">Thrown when filePath is null</exception>
    /// <exception cref="NotSupportedException">Thrown for unsupported image formats</exception>
    public static string GetContentType(string filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);

        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return extension switch
        {
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            _ => throw new NotSupportedException($"Unsupported image format: {extension}")
        };
    }

    /// <summary>
    /// Reads PNG dimensions from the IHDR chunk (bytes 16-23 of the file)
    /// PNG format: 8-byte signature, then IHDR chunk with width at offset 16 and height at offset 20
    /// </summary>
    private static (int Width, int Height) GetPngDimensions(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        var header = new byte[24];

        if (stream.Read(header, 0, 24) < 24)
        {
            throw new InvalidDataException($"File too small to be a valid PNG: {filePath}");
        }

        // Verify PNG signature: 137 80 78 71 13 10 26 10
        if (header[0] != 137 || header[1] != 80 || header[2] != 78 || header[3] != 71)
        {
            throw new InvalidDataException($"File is not a valid PNG: {filePath}");
        }

        // Width and height are big-endian 32-bit integers at offsets 16 and 20
        int width = (header[16] << 24) | (header[17] << 16) | (header[18] << 8) | header[19];
        int height = (header[20] << 24) | (header[21] << 16) | (header[22] << 8) | header[23];

        return (width, height);
    }

    /// <summary>
    /// Reads JPEG dimensions by scanning for SOF0 (0xFFC0) or SOF2 (0xFFC2) markers
    /// JPEG SOF format: marker (2 bytes), length (2 bytes), precision (1 byte), height (2 bytes), width (2 bytes)
    /// </summary>
    private static (int Width, int Height) GetJpegDimensions(string filePath)
    {
        using var stream = File.OpenRead(filePath);

        // Verify JPEG SOI marker: 0xFF 0xD8
        if (stream.ReadByte() != 0xFF || stream.ReadByte() != 0xD8)
        {
            throw new InvalidDataException($"File is not a valid JPEG: {filePath}");
        }

        while (stream.Position < stream.Length)
        {
            // Find next marker
            int b = stream.ReadByte();
            if (b != 0xFF)
            {
                continue;
            }

            // Skip padding 0xFF bytes
            int marker;
            do
            {
                marker = stream.ReadByte();
            } while (marker == 0xFF);

            if (marker < 0)
            {
                break;
            }

            // SOF0 (0xC0) or SOF2 (0xC2) - Start of Frame markers contain dimensions
            if (marker == 0xC0 || marker == 0xC2)
            {
                var data = new byte[7];
                if (stream.Read(data, 0, 7) < 7)
                {
                    break;
                }

                // Skip length (2 bytes) and precision (1 byte), then read height and width (big-endian)
                int height = (data[3] << 8) | data[4];
                int width = (data[5] << 8) | data[6];

                return (width, height);
            }

            // Skip other segments: read length and advance
            int high = stream.ReadByte();
            int low = stream.ReadByte();
            if (high < 0 || low < 0)
            {
                break;
            }

            int segmentLength = (high << 8) | low;
            if (segmentLength < 2)
            {
                break;
            }

            stream.Seek(segmentLength - 2, SeekOrigin.Current);
        }

        throw new InvalidDataException($"Could not find dimensions in JPEG file: {filePath}");
    }
}
