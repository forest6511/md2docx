using MarkdownToDocx.Core.Models;

namespace MarkdownToDocx.Styling.Models;

/// <summary>
/// Root configuration for Markdown to Word conversion
/// </summary>
public sealed class ConversionConfiguration
{
    /// <summary>
    /// Schema version for compatibility checking
    /// </summary>
    public string SchemaVersion { get; set; } = string.Empty;

    /// <summary>
    /// Conversion metadata (name, description, author)
    /// </summary>
    public MetadataConfig Metadata { get; set; } = new();

    /// <summary>
    /// Text direction mode (horizontal or vertical)
    /// </summary>
    public TextDirectionMode TextDirection { get; set; }

    /// <summary>
    /// Page layout configuration
    /// </summary>
    public PageLayoutConfig PageLayout { get; set; } = new();

    /// <summary>
    /// Style configuration for document elements
    /// </summary>
    public StyleConfiguration Styles { get; set; } = new();

    /// <summary>
    /// Font configuration
    /// </summary>
    public FontConfig Fonts { get; set; } = new();

    /// <summary>
    /// Text transformation rules (optional, for vertical text)
    /// </summary>
    public TransformationRules? Transformations { get; set; }
}

/// <summary>
/// Configuration metadata
/// </summary>
public sealed class MetadataConfig
{
    /// <summary>
    /// Configuration name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Configuration description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Author name
    /// </summary>
    public string? Author { get; set; }

    /// <summary>
    /// Configuration version
    /// </summary>
    public string? Version { get; set; }
}

/// <summary>
/// Page layout configuration
/// </summary>
public sealed class PageLayoutConfig
{
    /// <summary>
    /// Page width in cm
    /// </summary>
    public double Width { get; set; }

    /// <summary>
    /// Page height in cm
    /// </summary>
    public double Height { get; set; }

    /// <summary>
    /// Top margin in cm
    /// </summary>
    public double MarginTop { get; set; }

    /// <summary>
    /// Bottom margin in cm
    /// </summary>
    public double MarginBottom { get; set; }

    /// <summary>
    /// Left margin in cm
    /// </summary>
    public double MarginLeft { get; set; }

    /// <summary>
    /// Right margin in cm
    /// </summary>
    public double MarginRight { get; set; }

    /// <summary>
    /// Header margin in cm
    /// </summary>
    public double? MarginHeader { get; set; }

    /// <summary>
    /// Footer margin in cm
    /// </summary>
    public double? MarginFooter { get; set; }

    /// <summary>
    /// Gutter margin in cm
    /// </summary>
    public double? MarginGutter { get; set; }
}

/// <summary>
/// Font configuration
/// </summary>
public sealed class FontConfig
{
    /// <summary>
    /// ASCII font name
    /// </summary>
    public string Ascii { get; set; } = string.Empty;

    /// <summary>
    /// East Asian font name (for CJK characters)
    /// </summary>
    public string EastAsia { get; set; } = string.Empty;

    /// <summary>
    /// Default font size in pt
    /// </summary>
    public int DefaultSize { get; set; }
}

/// <summary>
/// Text transformation rules for vertical text
/// </summary>
public sealed class TransformationRules
{
    /// <summary>
    /// Enable bracket transformation
    /// </summary>
    public bool ConvertBrackets { get; set; } = true;

    /// <summary>
    /// Enable punctuation transformation
    /// </summary>
    public bool ConvertPunctuation { get; set; } = true;

    /// <summary>
    /// Enable leader character transformation
    /// </summary>
    public bool ConvertLeaders { get; set; } = true;

    /// <summary>
    /// Convert halfwidth to fullwidth
    /// </summary>
    public bool HalfwidthToFullwidth { get; set; } = true;

    /// <summary>
    /// Mark horizontal text in vertical layout (tate-chu-yoko)
    /// </summary>
    public bool MarkTateChuYoko { get; set; } = true;
}
