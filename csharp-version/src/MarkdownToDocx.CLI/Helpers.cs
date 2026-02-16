using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using MarkdownToDocx.Core.Models;
using System.Text;

namespace MarkdownToDocx.CLI;

public static class Helpers
{
    public static int GetHeadingLevel(object block)
    {
        if (block is HeadingBlock heading)
        {
            return heading.Level;
        }
        return 1;
    }

    public static string GetBlockText(object block)
    {
        if (block is LeafBlock leafBlock && leafBlock.Inline != null)
        {
            return ExtractInlineText(leafBlock.Inline);
        }
        return string.Empty;
    }

    private static string ExtractInlineText(Inline? inline)
    {
        if (inline == null) return string.Empty;

        var sb = new StringBuilder();

        if (inline is ContainerInline container)
        {
            foreach (var child in container)
            {
                sb.Append(ExtractInlineText(child));
            }
        }
        else if (inline is LiteralInline literal)
        {
            sb.Append(literal.Content.ToString());
        }
        else if (inline is LineBreakInline)
        {
            sb.Append(' '); // Convert line breaks to spaces for inline text
        }
        else if (inline is CodeInline code)
        {
            sb.Append(code.Content);
        }

        return sb.ToString();
    }

    public static IEnumerable<ListItem> GetListItems(object block)
    {
        var items = new List<ListItem>();

        if (block is ListBlock listBlock)
        {
            foreach (var item in listBlock)
            {
                if (item is ListItemBlock listItem)
                {
                    var sb = new StringBuilder();

                    // ListItemBlock contains child blocks (usually ParagraphBlocks)
                    foreach (var child in listItem)
                    {
                        if (child is ParagraphBlock paragraph && paragraph.Inline != null)
                        {
                            var text = ExtractInlineText(paragraph.Inline);
                            if (!string.IsNullOrWhiteSpace(text))
                            {
                                if (sb.Length > 0) sb.Append(' ');
                                sb.Append(text);
                            }
                        }
                    }

                    if (sb.Length > 0)
                    {
                        items.Add(new ListItem { Text = sb.ToString() });
                    }
                }
            }
        }

        return items;
    }

    public static string GetCodeBlockText(object block)
    {
        if (block is FencedCodeBlock fencedCode)
        {
            // Extract text from Lines property
            var sb = new StringBuilder();
            foreach (var line in fencedCode.Lines.Lines)
            {
                if (sb.Length > 0) sb.AppendLine();
                sb.Append(line.Slice.ToString());
            }
            return sb.ToString();
        }

        return string.Empty;
    }

    public static string? GetCodeBlockLanguage(object block)
    {
        if (block is FencedCodeBlock fencedCode)
        {
            return fencedCode.Info;
        }
        return null;
    }

    public static string GetQuoteText(object block)
    {
        if (block is QuoteBlock quoteBlock)
        {
            var sb = new StringBuilder();

            foreach (var child in quoteBlock)
            {
                if (child is ParagraphBlock paragraph && paragraph.Inline != null)
                {
                    var text = ExtractInlineText(paragraph.Inline);
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        if (sb.Length > 0) sb.AppendLine();
                        sb.Append(text);
                    }
                }
            }

            return sb.ToString();
        }

        return string.Empty;
    }
}
