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
    /// Adds a paragraph to the document with inline formatting support
    /// </summary>
    /// <param name="runs">Structured inline runs (bold, italic, code) extracted from the paragraph</param>
    /// <param name="style">Paragraph style configuration</param>
    void AddParagraph(IReadOnlyList<InlineRun> runs, ParagraphStyle style);

    /// <summary>
    /// Adds a list (ordered or unordered) to the document
    /// </summary>
    /// <param name="items">List items</param>
    /// <param name="isOrdered">True for numbered list, false for bullet list</param>
    /// <param name="style">List style configuration</param>
    /// <param name="startNumber">First number for ordered lists (default: 1)</param>
    void AddList(IEnumerable<ListItem> items, bool isOrdered, ListStyle style, int startNumber = 1);

    /// <summary>
    /// Adds a code block to the document
    /// </summary>
    /// <param name="code">Code content</param>
    /// <param name="language">Programming language (optional)</param>
    /// <param name="style">Code block style configuration</param>
    void AddCodeBlock(string code, string? language, CodeBlockStyle style);

    /// <summary>
    /// Adds a quote block to the document with structured content support.
    /// Renders paragraphs and lists within the blockquote with quote styling.
    /// </summary>
    /// <param name="content">Structured child blocks extracted from the blockquote</param>
    /// <param name="style">Quote style configuration</param>
    void AddQuote(QuoteContent content, QuoteStyle style);

    /// <summary>
    /// Adds a title page with a cover image to the document.
    /// Must be called before any other content is added.
    /// </summary>
    /// <param name="style">Title page style configuration</param>
    void AddTitlePage(TitlePageStyle style);

    /// <summary>
    /// Adds a table of contents to the document.
    /// Must be called before any content (headings, paragraphs, etc.) is added.
    /// </summary>
    /// <param name="style">Table of contents style configuration</param>
    void AddTableOfContents(TableOfContentsStyle style);

    /// <summary>
    /// Adds an inline image to the document
    /// </summary>
    /// <param name="imagePath">Absolute path to the image file (PNG or JPEG)</param>
    /// <param name="altText">Alternative text for accessibility</param>
    /// <param name="style">Image style configuration</param>
    void AddImage(string imagePath, string altText, ImageStyle style);

    /// <summary>
    /// Adds a table to the document
    /// </summary>
    /// <param name="tableData">Structured table data with rows, cells, and alignment</param>
    /// <param name="style">Table style configuration</param>
    void AddTable(TableData tableData, TableStyle style);

    /// <summary>
    /// Adds a fenced div block to the document.
    /// Renders child blocks with background shading and optional top/bottom separator borders.
    /// </summary>
    /// <param name="content">Structured child blocks extracted from the fenced div</param>
    /// <param name="style">Fenced div visual style</param>
    void AddFencedDiv(FencedDivContent content, FencedDivStyle style);

    /// <summary>
    /// Adds a thematic break (horizontal rule) to the document
    /// </summary>
    void AddThematicBreak();

    /// <summary>
    /// Finalizes and saves the document
    /// </summary>
    void Save();
}
