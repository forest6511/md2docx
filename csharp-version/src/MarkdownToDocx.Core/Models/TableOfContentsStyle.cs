namespace MarkdownToDocx.Core.Models;

/// <summary>
/// Represents styling configuration for table of contents generation
/// </summary>
public sealed record TableOfContentsStyle
{
    /// <summary>
    /// Whether TOC generation is enabled
    /// </summary>
    public bool Enabled { get; init; } = false;

    /// <summary>
    /// Heading depth to include (1-6, e.g., 3 = H1-H3)
    /// </summary>
    public int Depth { get; init; } = 3;

    /// <summary>
    /// Optional title displayed above the TOC (null or empty = no title)
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// Whether to insert a page break after the TOC
    /// </summary>
    public bool PageBreakAfter { get; init; } = false;
}
