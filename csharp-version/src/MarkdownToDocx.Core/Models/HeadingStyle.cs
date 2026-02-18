namespace MarkdownToDocx.Core.Models;

/// <summary>
/// Represents styling configuration for heading elements
/// </summary>
public sealed record HeadingStyle
{
    /// <summary>
    /// Font size in half-points (e.g., 24 = 12pt)
    /// </summary>
    public int FontSize { get; init; } = 24;

    /// <summary>
    /// Whether text is bold
    /// </summary>
    public bool Bold { get; init; } = true;

    /// <summary>
    /// Text color in hexadecimal format (e.g., "2c3e50")
    /// </summary>
    public string Color { get; init; } = "2c3e50";

    /// <summary>
    /// Whether to show border
    /// </summary>
    public bool ShowBorder { get; init; } = false;

    /// <summary>
    /// Border color in hexadecimal format
    /// </summary>
    public string? BorderColor { get; init; }

    /// <summary>
    /// Border width (size)
    /// </summary>
    public uint BorderSize { get; init; } = 24;

    /// <summary>
    /// Space between border and content in points
    /// </summary>
    public uint BorderSpace { get; init; } = 0;

    /// <summary>
    /// Border position ("bottom", "left", "right", "top")
    /// </summary>
    public string BorderPosition { get; init; } = "bottom";

    /// <summary>
    /// Background color in hexadecimal format (optional)
    /// </summary>
    public string? BackgroundColor { get; init; }

    /// <summary>
    /// Line spacing in twips (exact mode). Null uses default line spacing.
    /// </summary>
    public string? LineSpacing { get; init; }

    /// <summary>
    /// Whether to insert a page break before this heading
    /// </summary>
    public bool PageBreakBefore { get; init; } = false;

    /// <summary>
    /// Space before heading in twips
    /// </summary>
    public string SpaceBefore { get; init; } = "400";

    /// <summary>
    /// Space after heading in twips
    /// </summary>
    public string SpaceAfter { get; init; } = "200";
}
