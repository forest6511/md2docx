namespace MarkdownToDocx.Core.Models;

/// <summary>
/// Structured content extracted from a blockquote.
/// Contains an ordered list of typed child blocks ready for rendering.
/// </summary>
public sealed class QuoteContent
{
    /// <summary>
    /// Ordered sequence of child blocks within the blockquote.
    /// </summary>
    public IReadOnlyList<QuoteContentBlock> Blocks { get; init; } = [];
}

/// <summary>
/// Base type for all child blocks within a blockquote.
/// </summary>
public abstract class QuoteContentBlock { }

/// <summary>
/// A paragraph (with inline formatting) inside a blockquote.
/// </summary>
public sealed class QuoteParagraph : QuoteContentBlock
{
    /// <summary>
    /// Structured inline runs with bold/italic/code formatting.
    /// </summary>
    public IReadOnlyList<InlineRun> Runs { get; init; } = [];
}

/// <summary>
/// A list (ordered or unordered) inside a blockquote.
/// </summary>
public sealed class QuoteList : QuoteContentBlock
{
    /// <summary>
    /// List items.
    /// </summary>
    public IReadOnlyList<ListItem> Items { get; init; } = [];

    /// <summary>
    /// True for numbered list, false for bullet list.
    /// </summary>
    public bool IsOrdered { get; init; }

    /// <summary>
    /// First number for ordered lists.
    /// </summary>
    public int StartNumber { get; init; } = 1;
}
