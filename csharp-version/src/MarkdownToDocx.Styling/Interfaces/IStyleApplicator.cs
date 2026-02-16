using MarkdownToDocx.Core.Models;
using MarkdownToDocx.Styling.Models;

namespace MarkdownToDocx.Styling.Interfaces;

/// <summary>
/// Interface for applying style configuration to document elements
/// </summary>
public interface IStyleApplicator
{
    /// <summary>
    /// Apply style configuration to heading
    /// </summary>
    /// <param name="level">Heading level (1-6)</param>
    /// <param name="config">Style configuration</param>
    /// <returns>Heading style</returns>
    HeadingStyle ApplyHeadingStyle(int level, StyleConfiguration config);

    /// <summary>
    /// Apply style configuration to paragraph
    /// </summary>
    /// <param name="config">Style configuration</param>
    /// <returns>Paragraph style</returns>
    ParagraphStyle ApplyParagraphStyle(StyleConfiguration config);

    /// <summary>
    /// Apply style configuration to list
    /// </summary>
    /// <param name="config">Style configuration</param>
    /// <returns>List style</returns>
    ListStyle ApplyListStyle(StyleConfiguration config);

    /// <summary>
    /// Apply style configuration to code block
    /// </summary>
    /// <param name="config">Style configuration</param>
    /// <returns>Code block style</returns>
    CodeBlockStyle ApplyCodeBlockStyle(StyleConfiguration config);

    /// <summary>
    /// Apply style configuration to quote
    /// </summary>
    /// <param name="config">Style configuration</param>
    /// <returns>Quote style</returns>
    QuoteStyle ApplyQuoteStyle(StyleConfiguration config);
}
