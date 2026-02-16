using DocumentFormat.OpenXml.Wordprocessing;

namespace MarkdownToDocx.Core.Models;

/// <summary>
/// Represents paragraph-level configuration for text direction and typography
/// </summary>
public sealed record ParagraphConfiguration
{
    /// <summary>
    /// Text direction setting for paragraphs
    /// </summary>
    public required TextDirectionValues TextDirection { get; init; }

    /// <summary>
    /// Whether to enable Kinsoku (Japanese line breaking rules)
    /// </summary>
    public bool Kinsoku { get; init; } = true;

    /// <summary>
    /// Line spacing value (in twips)
    /// </summary>
    public string? LineSpacing { get; init; }

    /// <summary>
    /// Line spacing rule
    /// </summary>
    public LineSpacingRuleValues? LineSpacingRule { get; init; }
}
