namespace MarkdownToDocx.Core.Models;

/// <summary>
/// Represents styling configuration for code block elements
/// </summary>
public sealed record CodeBlockStyle
{
    /// <summary>
    /// Font size in half-points
    /// </summary>
    public int FontSize { get; init; } = 18; // 9pt

    /// <summary>
    /// Text color in hexadecimal format
    /// </summary>
    public string Color { get; init; } = "000000";

    /// <summary>
    /// Background color in hexadecimal format
    /// </summary>
    public string BackgroundColor { get; init; } = "f8f8f8";

    /// <summary>
    /// Border color in hexadecimal format
    /// </summary>
    public string BorderColor { get; init; } = "cccccc";

    /// <summary>
    /// Monospace font family for ASCII characters
    /// </summary>
    public string MonospaceFontAscii { get; init; } = "Consolas";

    /// <summary>
    /// Monospace font family for East Asian characters
    /// </summary>
    public string MonospaceFontEastAsia { get; init; } = "Noto Sans Mono CJK JP";

    /// <summary>
    /// Space before code block in twips
    /// </summary>
    public string SpaceBefore { get; init; } = "300";

    /// <summary>
    /// Space after code block in twips
    /// </summary>
    public string SpaceAfter { get; init; } = "300";

    /// <summary>
    /// Line spacing within code block
    /// </summary>
    public string LineSpacing { get; init; } = "240";
}
