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

    /// <summary>
    /// Apply style configuration to inline image
    /// </summary>
    /// <param name="config">Style configuration</param>
    /// <returns>Image style</returns>
    ImageStyle ApplyImageStyle(StyleConfiguration config);

    /// <summary>
    /// Apply table of contents configuration
    /// </summary>
    /// <param name="config">Conversion configuration containing TOC settings</param>
    /// <returns>Table of contents style</returns>
    TableOfContentsStyle ApplyTableOfContentsStyle(ConversionConfiguration config);

    /// <summary>
    /// Apply title page configuration, resolving image paths
    /// </summary>
    /// <param name="config">Conversion configuration containing title page settings</param>
    /// <param name="inputFilePath">Path to the input markdown file (for relative path resolution)</param>
    /// <param name="coverImageOverride">CLI override for cover image path (implicitly enables title page)</param>
    /// <returns>Title page style</returns>
    TitlePageStyle ApplyTitlePageStyle(ConversionConfiguration config, string inputFilePath, string? coverImageOverride = null);
}
