namespace MarkdownToDocx.Styling.Models;

/// <summary>
/// Style configuration for all document elements
/// </summary>
public sealed class StyleConfiguration
{
    /// <summary>
    /// Style configuration for H1 headings
    /// </summary>
    public HeadingStyleConfig H1 { get; init; } = new();

    /// <summary>
    /// Style configuration for H2 headings
    /// </summary>
    public HeadingStyleConfig H2 { get; init; } = new();

    /// <summary>
    /// Style configuration for H3 headings
    /// </summary>
    public HeadingStyleConfig H3 { get; init; } = new();

    /// <summary>
    /// Style configuration for H4 headings
    /// </summary>
    public HeadingStyleConfig H4 { get; init; } = new();

    /// <summary>
    /// Style configuration for H5 headings
    /// </summary>
    public HeadingStyleConfig H5 { get; init; } = new();

    /// <summary>
    /// Style configuration for H6 headings
    /// </summary>
    public HeadingStyleConfig H6 { get; init; } = new();

    /// <summary>
    /// Style configuration for paragraph text
    /// </summary>
    public ParagraphStyleConfig Paragraph { get; init; } = new();

    /// <summary>
    /// Style configuration for list items
    /// </summary>
    public ListStyleConfig List { get; init; } = new();

    /// <summary>
    /// Style configuration for code blocks
    /// </summary>
    public CodeBlockStyleConfig CodeBlock { get; init; } = new();

    /// <summary>
    /// Style configuration for quote blocks
    /// </summary>
    public QuoteStyleConfig Quote { get; init; } = new();
}

/// <summary>
/// Style configuration for heading elements (H1-H6)
/// </summary>
public sealed class HeadingStyleConfig
{
    /// <summary>
    /// Font size in points (pt)
    /// </summary>
    public int Size { get; init; }

    /// <summary>
    /// Whether to apply bold formatting
    /// </summary>
    public bool Bold { get; init; } = true;

    /// <summary>
    /// Text color in hex format (e.g., "000000" for black)
    /// </summary>
    public string Color { get; init; } = string.Empty;

    /// <summary>
    /// Whether to show bottom border under heading
    /// </summary>
    public bool ShowBorder { get; init; } = false;

    /// <summary>
    /// Border color in hex format, or null to use text color
    /// </summary>
    public string? BorderColor { get; init; }

    /// <summary>
    /// Border thickness in eighths of a point (default: 12 = 1.5pt)
    /// </summary>
    public uint BorderSize { get; init; } = 12;

    /// <summary>
    /// Space between border and content in points (default: 0)
    /// </summary>
    public uint BorderSpace { get; init; } = 0;

    /// <summary>
    /// Border position ("bottom", "left", "right", "top")
    /// </summary>
    public string BorderPosition { get; init; } = "bottom";

    /// <summary>
    /// Background color in hex format, or null for no background
    /// </summary>
    public string? BackgroundColor { get; init; }

    /// <summary>
    /// Line spacing in twips (exact mode). Null uses default line spacing.
    /// </summary>
    public string? LineSpacing { get; init; }

    /// <summary>
    /// Spacing before heading in twips (1/20 of a point)
    /// </summary>
    public string SpaceBefore { get; init; } = "240";

    /// <summary>
    /// Spacing after heading in twips (1/20 of a point)
    /// </summary>
    public string SpaceAfter { get; init; } = "120";
}

/// <summary>
/// Style configuration for paragraph text
/// </summary>
public sealed class ParagraphStyleConfig
{
    /// <summary>
    /// Font size in points (pt)
    /// </summary>
    public int Size { get; init; }

    /// <summary>
    /// Text color in hex format (e.g., "000000" for black)
    /// </summary>
    public string Color { get; init; } = string.Empty;

    /// <summary>
    /// Line spacing in twips (1/20 of a point)
    /// </summary>
    public string LineSpacing { get; init; } = "360";

    /// <summary>
    /// First line indent in twips (1/20 of a point)
    /// </summary>
    public string FirstLineIndent { get; init; } = "0";

    /// <summary>
    /// Left margin indent in twips (1/20 of a point)
    /// </summary>
    public string LeftIndent { get; init; } = "0";
}

/// <summary>
/// Style configuration for list items (ordered and unordered)
/// </summary>
public sealed class ListStyleConfig
{
    /// <summary>
    /// Font size in points (pt)
    /// </summary>
    public int Size { get; init; }

    /// <summary>
    /// Text color in hex format (e.g., "000000" for black)
    /// </summary>
    public string Color { get; init; } = string.Empty;

    /// <summary>
    /// Left margin indent for list items in twips (1/20 of a point)
    /// </summary>
    public string LeftIndent { get; init; } = "720";

    /// <summary>
    /// Hanging indent for list markers in twips (1/20 of a point)
    /// </summary>
    public string HangingIndent { get; init; } = "360";

    /// <summary>
    /// Spacing before list item in twips (1/20 of a point)
    /// </summary>
    public string SpaceBefore { get; init; } = "60";

    /// <summary>
    /// Spacing after list item in twips (1/20 of a point)
    /// </summary>
    public string SpaceAfter { get; init; } = "60";
}

/// <summary>
/// Style configuration for code blocks (fenced code blocks)
/// </summary>
public sealed class CodeBlockStyleConfig
{
    /// <summary>
    /// Font size in points (pt)
    /// </summary>
    public int Size { get; init; }

    /// <summary>
    /// Text color in hex format (e.g., "000000" for black)
    /// </summary>
    public string Color { get; init; } = string.Empty;

    /// <summary>
    /// Background color in hex format (e.g., "F5F5F5" for light gray)
    /// </summary>
    public string BackgroundColor { get; init; } = string.Empty;

    /// <summary>
    /// Border color in hex format (e.g., "CCCCCC" for gray)
    /// </summary>
    public string BorderColor { get; init; } = string.Empty;

    /// <summary>
    /// Monospace font family for ASCII characters (e.g., "Courier New")
    /// </summary>
    public string MonospaceFontAscii { get; init; } = string.Empty;

    /// <summary>
    /// Monospace font family for East Asian characters (e.g., "MS Gothic")
    /// </summary>
    public string MonospaceFontEastAsia { get; init; } = string.Empty;

    /// <summary>
    /// Line spacing in twips (1/20 of a point)
    /// </summary>
    public string LineSpacing { get; init; } = "240";

    /// <summary>
    /// Spacing before code block in twips (1/20 of a point)
    /// </summary>
    public string SpaceBefore { get; init; } = "240";

    /// <summary>
    /// Spacing after code block in twips (1/20 of a point)
    /// </summary>
    public string SpaceAfter { get; init; } = "240";
}

/// <summary>
/// Style configuration for quote blocks (blockquotes)
/// </summary>
public sealed class QuoteStyleConfig
{
    /// <summary>
    /// Font size in points (pt)
    /// </summary>
    public int Size { get; init; }

    /// <summary>
    /// Text color in hex format (e.g., "666666" for gray)
    /// </summary>
    public string Color { get; init; } = string.Empty;

    /// <summary>
    /// Whether to apply italic formatting
    /// </summary>
    public bool Italic { get; init; } = true;

    /// <summary>
    /// Whether to show left border on quote block
    /// </summary>
    public bool ShowBorder { get; init; } = true;

    /// <summary>
    /// Border color in hex format (e.g., "CCCCCC" for gray)
    /// </summary>
    public string BorderColor { get; init; } = string.Empty;

    /// <summary>
    /// Border thickness in eighths of a point (default: 12 = 1.5pt)
    /// </summary>
    public uint BorderSize { get; init; } = 12;

    /// <summary>
    /// Space between border and content in points (default: 0)
    /// </summary>
    public uint BorderSpace { get; init; } = 0;

    /// <summary>
    /// Border position ("left", "right", "top", "bottom")
    /// </summary>
    public string BorderPosition { get; init; } = "left";

    /// <summary>
    /// Background color in hex format, or null for no background
    /// </summary>
    public string? BackgroundColor { get; init; }

    /// <summary>
    /// Left margin indent in twips (1/20 of a point)
    /// </summary>
    public string LeftIndent { get; init; } = "720";

    /// <summary>
    /// Spacing before quote block in twips (1/20 of a point)
    /// </summary>
    public string SpaceBefore { get; init; } = "240";

    /// <summary>
    /// Spacing after quote block in twips (1/20 of a point)
    /// </summary>
    public string SpaceAfter { get; init; } = "240";
}
