namespace MarkdownToDocx.Core.Models;

/// <summary>
/// Structured content extracted from a fenced div block.
/// Contains an ordered list of typed child blocks ready for rendering.
/// </summary>
public sealed class FencedDivContent
{
    /// <summary>
    /// Ordered sequence of child blocks within the fenced div.
    /// </summary>
    public IReadOnlyList<FencedDivBlock> Blocks { get; init; } = [];
}

/// <summary>
/// Base type for all child blocks within a fenced div.
/// </summary>
public abstract class FencedDivBlock { }

/// <summary>
/// A paragraph (with inline formatting) inside a fenced div.
/// </summary>
public sealed class FencedDivParagraph : FencedDivBlock
{
    /// <summary>
    /// Structured inline runs with bold/italic/code formatting.
    /// </summary>
    public IReadOnlyList<InlineRun> Runs { get; init; } = [];
}

/// <summary>
/// A heading inside a fenced div.
/// </summary>
public sealed class FencedDivHeading : FencedDivBlock
{
    /// <summary>
    /// Heading level (1–6).
    /// </summary>
    public int Level { get; init; }

    /// <summary>
    /// Plain text content of the heading.
    /// </summary>
    public string Text { get; init; } = string.Empty;
}

/// <summary>
/// A table inside a fenced div.
/// </summary>
public sealed class FencedDivTable : FencedDivBlock
{
    /// <summary>
    /// Structured table data with rows, cells, and alignment.
    /// </summary>
    public TableData Data { get; init; } = new();
}

/// <summary>
/// A list (ordered or unordered) inside a fenced div.
/// </summary>
public sealed class FencedDivList : FencedDivBlock
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
