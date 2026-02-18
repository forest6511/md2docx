namespace MarkdownToDocx.Core.Models;

/// <summary>
/// Represents styling configuration for title page generation
/// </summary>
public sealed record TitlePageStyle
{
    /// <summary>
    /// Whether title page generation is enabled
    /// </summary>
    public bool Enabled { get; init; }

    /// <summary>
    /// Resolved absolute path to cover image
    /// </summary>
    public string? ImagePath { get; init; }

    /// <summary>
    /// Maximum width of the image as percentage of printable area (1-100)
    /// </summary>
    public int ImageMaxWidthPercent { get; init; } = 80;

    /// <summary>
    /// Maximum height of the image as percentage of printable area (1-100)
    /// </summary>
    public int ImageMaxHeightPercent { get; init; } = 80;

    /// <summary>
    /// Whether to insert a page break after the title page
    /// </summary>
    public bool PageBreakAfter { get; init; } = true;
}
