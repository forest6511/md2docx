namespace MarkdownToDocx.Core.Models;

/// <summary>
/// Represents styling configuration for list elements
/// </summary>
public sealed record ListStyle
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
    /// Left indentation in twips
    /// </summary>
    public string LeftIndent { get; init; } = "720";

    /// <summary>
    /// Hanging indentation in twips
    /// </summary>
    public string HangingIndent { get; init; } = "360";

    /// <summary>
    /// Space before list item in twips
    /// </summary>
    public string SpaceBefore { get; init; } = "100";

    /// <summary>
    /// Space after list item in twips
    /// </summary>
    public string SpaceAfter { get; init; } = "100";
}
