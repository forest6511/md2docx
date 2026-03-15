namespace MarkdownToDocx.Core.Models;

/// <summary>
/// Styling configuration for a fenced div block (:::classname ... :::).
/// Background color is applied to all child paragraphs.
/// Top/bottom separator lines appear on the first and last paragraph-like elements.
/// </summary>
public sealed record FencedDivStyle
{
    /// <summary>
    /// Background fill color in hex (e.g., "F2F2F2"). Empty = no shading.
    /// </summary>
    public string BackgroundColor { get; init; } = string.Empty;

    /// <summary>
    /// Top separator line color in hex (e.g., "AAAAAA"). Empty = no top border.
    /// </summary>
    public string BorderTopColor { get; init; } = string.Empty;

    /// <summary>
    /// Top separator line thickness in eighths of a point (default: 4 = 0.5pt).
    /// </summary>
    public uint BorderTopSize { get; init; } = 4;

    /// <summary>
    /// Bottom separator line color in hex (e.g., "AAAAAA"). Empty = no bottom border.
    /// </summary>
    public string BorderBottomColor { get; init; } = string.Empty;

    /// <summary>
    /// Bottom separator line thickness in eighths of a point (default: 4 = 0.5pt).
    /// </summary>
    public uint BorderBottomSize { get; init; } = 4;

    /// <summary>
    /// Space between separator line and text in points (default: 0).
    /// </summary>
    public uint BorderSpace { get; init; } = 0;

    /// <summary>
    /// Spacing before the first block in the div in twips.
    /// </summary>
    public string SpaceBefore { get; init; } = "0";

    /// <summary>
    /// Spacing after the last block in the div in twips.
    /// </summary>
    public string SpaceAfter { get; init; } = "0";

    /// <summary>
    /// Left indent for div paragraphs in twips.
    /// </summary>
    public string LeftIndent { get; init; } = "0";

    /// <summary>
    /// Font size in half-points (inherited from paragraph style).
    /// </summary>
    public int FontSize { get; init; }

    /// <summary>
    /// Text color in hex (inherited from paragraph style).
    /// </summary>
    public string Color { get; init; } = string.Empty;

    /// <summary>
    /// Line spacing in twips (inherited from paragraph style).
    /// </summary>
    public string LineSpacing { get; init; } = "360";

    /// <summary>
    /// Monospace font for inline code ASCII characters.
    /// </summary>
    public string InlineCodeFontAscii { get; init; } = "Courier New";

    /// <summary>
    /// Monospace font for inline code East Asian characters.
    /// </summary>
    public string InlineCodeFontEastAsia { get; init; } = "Noto Sans Mono CJK JP";
}
