namespace MarkdownToDocx.Styling.Models;

/// <summary>
/// Style configuration for all document elements
/// </summary>
public sealed class StyleConfiguration
{
    public HeadingStyleConfig H1 { get; set; } = new();
    public HeadingStyleConfig H2 { get; set; } = new();
    public HeadingStyleConfig H3 { get; set; } = new();
    public HeadingStyleConfig H4 { get; set; } = new();
    public HeadingStyleConfig H5 { get; set; } = new();
    public HeadingStyleConfig H6 { get; set; } = new();
    public ParagraphStyleConfig Paragraph { get; set; } = new();
    public ListStyleConfig List { get; set; } = new();
    public CodeBlockStyleConfig CodeBlock { get; set; } = new();
    public QuoteStyleConfig Quote { get; set; } = new();
}

public sealed class HeadingStyleConfig
{
    public int Size { get; set; }
    public bool Bold { get; set; } = true;
    public string Color { get; set; } = string.Empty;
    public bool ShowBorder { get; set; } = false;
    public string? BorderColor { get; set; }
    public uint BorderSize { get; set; } = 12;
    public string SpaceBefore { get; set; } = "240";
    public string SpaceAfter { get; set; } = "120";
}

public sealed class ParagraphStyleConfig
{
    public int Size { get; set; }
    public string Color { get; set; } = string.Empty;
    public string LineSpacing { get; set; } = "360";
    public string FirstLineIndent { get; set; } = "0";
    public string LeftIndent { get; set; } = "0";
}

public sealed class ListStyleConfig
{
    public int Size { get; set; }
    public string Color { get; set; } = string.Empty;
    public string LeftIndent { get; set; } = "720";
    public string HangingIndent { get; set; } = "360";
    public string SpaceBefore { get; set; } = "60";
    public string SpaceAfter { get; set; } = "60";
}

public sealed class CodeBlockStyleConfig
{
    public int Size { get; set; }
    public string Color { get; set; } = string.Empty;
    public string BackgroundColor { get; set; } = string.Empty;
    public string BorderColor { get; set; } = string.Empty;
    public string MonospaceFontAscii { get; set; } = string.Empty;
    public string MonospaceFontEastAsia { get; set; } = string.Empty;
    public string LineSpacing { get; set; } = "240";
    public string SpaceBefore { get; set; } = "240";
    public string SpaceAfter { get; set; } = "240";
}

public sealed class QuoteStyleConfig
{
    public int Size { get; set; }
    public string Color { get; set; } = string.Empty;
    public bool Italic { get; set; } = true;
    public bool ShowBorder { get; set; } = true;
    public string BorderColor { get; set; } = string.Empty;
    public uint BorderSize { get; set; } = 12;
    public string BorderPosition { get; set; } = "left";
    public string? BackgroundColor { get; set; }
    public string LeftIndent { get; set; } = "720";
    public string SpaceBefore { get; set; } = "240";
    public string SpaceAfter { get; set; } = "240";
}
