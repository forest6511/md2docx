namespace MarkdownToDocx.Core.Models;

/// <summary>
/// Represents styling configuration for quote block elements
/// </summary>
public sealed record QuoteStyle
{
    /// <summary>
    /// Font size in half-points
    /// </summary>
    public int FontSize { get; init; } = 22;

    /// <summary>
    /// Text color in hexadecimal format
    /// </summary>
    public string Color { get; init; } = "000000";

    /// <summary>
    /// Whether text is italic
    /// </summary>
    public bool Italic { get; init; } = true;

    /// <summary>
    /// Whether to show border
    /// </summary>
    public bool ShowBorder { get; init; } = true;

    /// <summary>
    /// Border position (left, right, top, bottom)
    /// </summary>
    public string BorderPosition { get; init; } = "left";

    /// <summary>
    /// Border color in hexadecimal format
    /// </summary>
    public string BorderColor { get; init; } = "95a5a6";

    /// <summary>
    /// Border size
    /// </summary>
    public uint BorderSize { get; init; } = 24;

    /// <summary>
    /// Space between border and content in points
    /// </summary>
    public uint BorderSpace { get; init; } = 0;

    /// <summary>
    /// Background color in hexadecimal format (optional)
    /// </summary>
    public string? BackgroundColor { get; init; }

    /// <summary>
    /// Left indentation in twips
    /// </summary>
    public string LeftIndent { get; init; } = "720";

    /// <summary>
    /// Space before quote in twips
    /// </summary>
    public string SpaceBefore { get; init; } = "120";

    /// <summary>
    /// Space after quote in twips
    /// </summary>
    public string SpaceAfter { get; init; } = "120";

    /// <summary>
    /// Internal padding on top, right, and bottom in points.
    /// Creates invisible borders matching the background color to produce
    /// spacing between the background fill and the text content.
    /// Only effective when BackgroundColor is set.
    /// </summary>
    public uint PaddingSpace { get; init; } = 0;

    /// <summary>
    /// Monospace font family for inline code (ASCII characters)
    /// </summary>
    public string InlineCodeFontAscii { get; init; } = "Courier New";

    /// <summary>
    /// Monospace font family for inline code (East Asian characters)
    /// </summary>
    public string InlineCodeFontEastAsia { get; init; } = "Noto Sans Mono CJK JP";
}
