namespace MarkdownToDocx.Core.Models;

/// <summary>
/// Represents a single item in a list
/// </summary>
public sealed record ListItem
{
    /// <summary>
    /// The text content of the list item
    /// </summary>
    public required string Text { get; init; }

    /// <summary>
    /// Nested sub-items (for nested lists)
    /// </summary>
    public IReadOnlyList<ListItem>? SubItems { get; init; }
}
