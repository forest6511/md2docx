namespace MarkdownToDocx.Core.Models;

/// <summary>
/// Represents styling configuration for paragraph elements
/// </summary>
public sealed record ParagraphStyle
{
    /// <summary>
    /// Font size in half-points (e.g., 22 = 11pt)
    /// </summary>
    public int FontSize { get; init; } = 22;

    /// <summary>
    /// Text color in hexadecimal format
    /// </summary>
    public string Color { get; init; } = "000000";

    /// <summary>
    /// Line spacing value
    /// </summary>
    public string LineSpacing { get; init; } = "360"; // 1.5x

    /// <summary>
    /// First line indentation in twips
    /// </summary>
    public string FirstLineIndent { get; init; } = "0";

    /// <summary>
    /// Left indentation in twips
    /// </summary>
    public string LeftIndent { get; init; } = "0";
}
