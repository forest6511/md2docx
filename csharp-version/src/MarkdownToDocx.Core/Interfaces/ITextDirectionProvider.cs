using MarkdownToDocx.Core.Models;

namespace MarkdownToDocx.Core.Interfaces;

/// <summary>
/// Provides text direction configuration for document rendering
/// </summary>
public interface ITextDirectionProvider
{
    /// <summary>
    /// Gets the text direction mode (Horizontal or Vertical)
    /// </summary>
    TextDirectionMode Mode { get; }

    /// <summary>
    /// Gets page-level configuration based on text direction
    /// </summary>
    PageConfiguration GetPageConfiguration();

    /// <summary>
    /// Gets paragraph-level configuration based on text direction
    /// </summary>
    ParagraphConfiguration GetParagraphConfiguration();
}
