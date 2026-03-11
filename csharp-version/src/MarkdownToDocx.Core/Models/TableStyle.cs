namespace MarkdownToDocx.Core.Models;

/// <summary>
/// Represents styling configuration for table elements
/// </summary>
public sealed record TableStyle
{
    /// <summary>
    /// Font size in half-points (e.g., 20 = 10pt)
    /// </summary>
    public int FontSize { get; init; } = 20; // 10pt

    /// <summary>
    /// Header row background color in hexadecimal format (e.g., "2c3e50")
    /// </summary>
    public string HeaderBackgroundColor { get; init; } = "2c3e50";

    /// <summary>
    /// Header row text color in hexadecimal format (e.g., "ecf0f1")
    /// </summary>
    public string HeaderTextColor { get; init; } = "ecf0f1";

    /// <summary>
    /// Body cell text color in hexadecimal format (e.g., "2c3e50")
    /// </summary>
    public string BodyTextColor { get; init; } = "2c3e50";

    /// <summary>
    /// Table border color in hexadecimal format (e.g., "bdc3c7")
    /// </summary>
    public string BorderColor { get; init; } = "bdc3c7";

    /// <summary>
    /// Border thickness in eighths of a point (e.g., 4 = 0.5pt)
    /// </summary>
    public uint BorderSize { get; init; } = 4;

    /// <summary>
    /// Whether to apply bold formatting to header row
    /// </summary>
    public bool HeaderBold { get; init; } = true;

    /// <summary>
    /// Cell top padding in twips (1/20 of a point)
    /// </summary>
    public uint CellPaddingTop { get; init; } = 40;

    /// <summary>
    /// Cell bottom padding in twips
    /// </summary>
    public uint CellPaddingBottom { get; init; } = 40;

    /// <summary>
    /// Cell left padding in twips
    /// </summary>
    public uint CellPaddingLeft { get; init; } = 80;

    /// <summary>
    /// Cell right padding in twips
    /// </summary>
    public uint CellPaddingRight { get; init; } = 80;

    /// <summary>
    /// Space before table in twips
    /// </summary>
    public string SpaceBefore { get; init; } = "160";

    /// <summary>
    /// Space after table in twips
    /// </summary>
    public string SpaceAfter { get; init; } = "160";
}
