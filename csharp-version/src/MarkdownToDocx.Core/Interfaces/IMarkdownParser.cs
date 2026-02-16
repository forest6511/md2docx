using Markdig.Syntax;

namespace MarkdownToDocx.Core.Interfaces;

/// <summary>
/// Provides abstraction for parsing Markdown content into a document structure
/// </summary>
public interface IMarkdownParser
{
    /// <summary>
    /// Parses Markdown text into a structured document
    /// </summary>
    /// <param name="markdown">The Markdown text to parse</param>
    /// <returns>Parsed Markdown document structure</returns>
    MarkdownDocument Parse(string markdown);
}
