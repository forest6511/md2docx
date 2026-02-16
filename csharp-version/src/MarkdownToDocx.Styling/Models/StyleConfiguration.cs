namespace MarkdownToDocx.Styling.Models;

/// <summary>
/// Style configuration for all document elements
/// </summary>
public sealed class StyleConfiguration
{
    public HeadingStyleConfig H1 { get; init; } = new();
    public HeadingStyleConfig H2 { get; init; } = new();
    public HeadingStyleConfig H3 { get; init; } = new();
    public HeadingStyleConfig H4 { get; init; } = new();
    public HeadingStyleConfig H5 { get; init; } = new();
    public HeadingStyleConfig H6 { get; init; } = new();
    public ParagraphStyleConfig Paragraph { get; init; } = new();
    public ListStyleConfig List { get; init; } = new();
    public CodeBlockStyleConfig CodeBlock { get; init; } = new();
    public QuoteStyleConfig Quote { get; init; } = new();
}

public sealed class HeadingStyleConfig
{
    public int Size { get; init; }
    public bool Bold { get; init; } = true;
    public string Color { get; init; } = string.Empty;
    public bool ShowBorder { get; init; } = false;
    public string? BorderColor { get; init; }
    public uint BorderSize { get; init; } = 12;
    public string SpaceBefore { get; init; } = "240";
    public string SpaceAfter { get; init; } = "120";
}

public sealed class ParagraphStyleConfig
{
    public int Size { get; init; }
    public string Color { get; init; } = string.Empty;
    public string LineSpacing { get; init; } = "360";
    public string FirstLineIndent { get; init; } = "0";
    public string LeftIndent { get; init; } = "0";
}

public sealed class ListStyleConfig
{
    public int Size { get; init; }
    public string Color { get; init; } = string.Empty;
    public string LeftIndent { get; init; } = "720";
    public string HangingIndent { get; init; } = "360";
    public string SpaceBefore { get; init; } = "60";
    public string SpaceAfter { get; init; } = "60";
}

public sealed class CodeBlockStyleConfig
{
    public int Size { get; init; }
    public string Color { get; init; } = string.Empty;
    public string BackgroundColor { get; init; } = string.Empty;
    public string BorderColor { get; init; } = string.Empty;
    public string MonospaceFontAscii { get; init; } = string.Empty;
    public string MonospaceFontEastAsia { get; init; } = string.Empty;
    public string LineSpacing { get; init; } = "240";
    public string SpaceBefore { get; init; } = "240";
    public string SpaceAfter { get; init; } = "240";
}

public sealed class QuoteStyleConfig
{
    public int Size { get; init; }
    public string Color { get; init; } = string.Empty;
    public bool Italic { get; init; } = true;
    public bool ShowBorder { get; init; } = true;
    public string BorderColor { get; init; } = string.Empty;
    public uint BorderSize { get; init; } = 12;
    public string BorderPosition { get; init; } = "left";
    public string? BackgroundColor { get; init; }
    public string LeftIndent { get; init; } = "720";
    public string SpaceBefore { get; init; } = "240";
    public string SpaceAfter { get; init; } = "240";
}
