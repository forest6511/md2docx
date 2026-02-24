namespace MarkdownToDocx.Core.Models;

/// <summary>
/// Represents styling configuration for inline image elements
/// </summary>
public sealed record ImageStyle
{
    /// <summary>
    /// Maximum width of the image as a percentage of the printable area (1-100).
    /// The image is scaled down to fit but never upscaled.
    /// </summary>
    public int MaxWidthPercent { get; init; } = 100;

    /// <summary>
    /// Horizontal alignment of the image paragraph ("left", "center", "right")
    /// </summary>
    public string Alignment { get; init; } = "center";
}
