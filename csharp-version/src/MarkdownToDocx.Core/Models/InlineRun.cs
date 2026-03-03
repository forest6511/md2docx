namespace MarkdownToDocx.Core.Models;

/// <summary>
/// Represents a single formatted run of text extracted from inline Markdown content
/// </summary>
public sealed record InlineRun
{
    /// <summary>
    /// The text content of this run
    /// </summary>
    public string Text { get; init; } = string.Empty;

    /// <summary>
    /// Whether this run should be rendered in bold
    /// </summary>
    public bool Bold { get; init; }

    /// <summary>
    /// Whether this run should be rendered in italic
    /// </summary>
    public bool Italic { get; init; }

    /// <summary>
    /// Whether this run is inline code (monospace font, no italic override)
    /// </summary>
    public bool IsCode { get; init; }
}
