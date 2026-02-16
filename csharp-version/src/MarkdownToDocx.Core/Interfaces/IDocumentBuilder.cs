using MarkdownToDocx.Core.Models;

namespace MarkdownToDocx.Core.Interfaces;

/// <summary>
/// Provides abstraction for building Word (DOCX) documents
/// </summary>
public interface IDocumentBuilder : IDisposable
{
    /// <summary>
    /// Adds a heading to the document
    /// </summary>
    /// <param name="level">Heading level (1-6)</param>
    /// <param name="text">Heading text content</param>
    /// <param name="style">Heading style configuration</param>
    void AddHeading(int level, string text, HeadingStyle style);

    /// <summary>
    /// Adds a paragraph to the document
    /// </summary>
    /// <param name="text">Paragraph text content</param>
    /// <param name="style">Paragraph style configuration</param>
    void AddParagraph(string text, ParagraphStyle style);

    /// <summary>
    /// Adds a list (ordered or unordered) to the document
    /// </summary>
    /// <param name="items">List items</param>
    /// <param name="isOrdered">True for numbered list, false for bullet list</param>
    /// <param name="style">List style configuration</param>
    void AddList(IEnumerable<ListItem> items, bool isOrdered, ListStyle style);

    /// <summary>
    /// Adds a code block to the document
    /// </summary>
    /// <param name="code">Code content</param>
    /// <param name="language">Programming language (optional)</param>
    /// <param name="style">Code block style configuration</param>
    void AddCodeBlock(string code, string? language, CodeBlockStyle style);

    /// <summary>
    /// Adds a quote block to the document
    /// </summary>
    /// <param name="text">Quote text content</param>
    /// <param name="style">Quote style configuration</param>
    void AddQuote(string text, QuoteStyle style);

    /// <summary>
    /// Adds a thematic break (horizontal rule) to the document
    /// </summary>
    void AddThematicBreak();

    /// <summary>
    /// Finalizes and saves the document
    /// </summary>
    void Save();
}
