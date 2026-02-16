namespace MarkdownToDocx.Core.Models;

/// <summary>
/// Defines the text direction mode for document rendering
/// </summary>
public enum TextDirectionMode
{
    /// <summary>
    /// Horizontal text (left-to-right, top-to-bottom) - Western style
    /// </summary>
    Horizontal,

    /// <summary>
    /// Vertical text (top-to-bottom, right-to-left) - Traditional Japanese style
    /// </summary>
    Vertical
}
