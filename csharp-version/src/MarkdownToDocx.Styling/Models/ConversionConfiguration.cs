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
    public string SchemaVersion { get; init; } = string.Empty;

    /// <summary>
    /// Conversion metadata (name, description, author)
    /// </summary>
    public MetadataConfig Metadata { get; init; } = new();

    /// <summary>
    /// Text direction mode (horizontal or vertical)
    /// </summary>
    public TextDirectionMode TextDirection { get; init; }

    /// <summary>
    /// Page layout configuration
    /// </summary>
    public PageLayoutConfig PageLayout { get; init; } = new();

    /// <summary>
    /// Style configuration for document elements
    /// </summary>
    public StyleConfiguration Styles { get; init; } = new();

    /// <summary>
    /// Font configuration
    /// </summary>
    public FontConfig Fonts { get; init; } = new();

    /// <summary>
    /// Text transformation rules (optional, for vertical text)
    /// </summary>
    public TransformationRules? Transformations { get; init; }
}

/// <summary>
/// Configuration metadata
/// </summary>
public sealed class MetadataConfig
{
    /// <summary>
    /// Configuration name
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Configuration description
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Author name
    /// </summary>
    public string? Author { get; init; }

    /// <summary>
    /// Configuration version
    /// </summary>
    public string? Version { get; init; }
}

/// <summary>
/// Page layout configuration
/// </summary>
public sealed class PageLayoutConfig
{
    /// <summary>
    /// Page width in cm
    /// </summary>
    public double Width { get; init; }

    /// <summary>
    /// Page height in cm
    /// </summary>
    public double Height { get; init; }

    /// <summary>
    /// Top margin in cm
    /// </summary>
    public double MarginTop { get; init; }

    /// <summary>
    /// Bottom margin in cm
    /// </summary>
    public double MarginBottom { get; init; }

    /// <summary>
    /// Left margin in cm
    /// </summary>
    public double MarginLeft { get; init; }

    /// <summary>
    /// Right margin in cm
    /// </summary>
    public double MarginRight { get; init; }

    /// <summary>
    /// Header margin in cm
    /// </summary>
    public double? MarginHeader { get; init; }

    /// <summary>
    /// Footer margin in cm
    /// </summary>
    public double? MarginFooter { get; init; }

    /// <summary>
    /// Gutter margin in cm
    /// </summary>
    public double? MarginGutter { get; init; }
}

/// <summary>
/// Font configuration
/// </summary>
public sealed class FontConfig
{
    /// <summary>
    /// ASCII font name
    /// </summary>
    public string Ascii { get; init; } = string.Empty;

    /// <summary>
    /// East Asian font name (for CJK characters)
    /// </summary>
    public string EastAsia { get; init; } = string.Empty;

    /// <summary>
    /// Default font size in pt
    /// </summary>
    public int DefaultSize { get; init; }
}

/// <summary>
/// Text transformation rules for vertical text
/// </summary>
public sealed class TransformationRules
{
    /// <summary>
    /// Enable bracket transformation
    /// </summary>
    public bool ConvertBrackets { get; init; } = true;

    /// <summary>
    /// Enable punctuation transformation
    /// </summary>
    public bool ConvertPunctuation { get; init; } = true;

    /// <summary>
    /// Enable leader character transformation
    /// </summary>
    public bool ConvertLeaders { get; init; } = true;

    /// <summary>
    /// Convert halfwidth to fullwidth
    /// </summary>
    public bool HalfwidthToFullwidth { get; init; } = true;

    /// <summary>
    /// Mark horizontal text in vertical layout (tate-chu-yoko)
    /// </summary>
    public bool MarkTateChuYoko { get; init; } = true;
}
