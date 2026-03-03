using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using MarkdownToDocx.Core.Models;
using System.Diagnostics.CodeAnalysis;
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
            return sb.ToString().TrimEnd('\r', '\n');
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

    /// <summary>
    /// Returns true when a ParagraphBlock contains a single standalone image inline.
    /// Outputs the resolved image path and alt text.
    /// </summary>
    public static bool IsStandaloneImage(
        ParagraphBlock paragraph,
        [NotNullWhen(true)] out string? imagePath,
        out string altText)
    {
        imagePath = null;
        altText = string.Empty;

        if (paragraph.Inline == null)
        {
            return false;
        }

        // Collect non-whitespace inlines
        var inlines = paragraph.Inline
            .Where(i => i is not LineBreakInline)
            .ToList();

        if (inlines.Count != 1 || inlines[0] is not LinkInline { IsImage: true } link)
        {
            return false;
        }

        imagePath = link.Url;
        altText = link.FirstChild is LiteralInline lit ? lit.Content.ToString() : string.Empty;

        return imagePath != null;
    }

    public static IReadOnlyList<InlineRun> GetParagraphRuns(ParagraphBlock paragraph)
    {
        var runs = new List<InlineRun>();
        if (paragraph.Inline != null)
            ExtractInlineRuns(paragraph.Inline, runs, bold: false, italic: false);
        return runs;
    }

    public static IReadOnlyList<InlineRun> GetQuoteRuns(object block)
    {
        var runs = new List<InlineRun>();

        if (block is QuoteBlock quoteBlock)
        {
            bool firstParagraph = true;

            foreach (var child in quoteBlock)
            {
                if (child is ParagraphBlock paragraph && paragraph.Inline != null)
                {
                    if (!firstParagraph)
                        runs.Add(new InlineRun { Text = " " });

                    ExtractInlineRuns(paragraph.Inline, runs, bold: false, italic: false);
                    firstParagraph = false;
                }
            }
        }

        return runs;
    }

    private static void ExtractInlineRuns(Inline? inline, List<InlineRun> runs, bool bold, bool italic)
    {
        if (inline == null) return;

        if (inline is EmphasisInline emphasis)
        {
            bool isBold = emphasis.DelimiterCount >= 2;
            bool isItalic = emphasis.DelimiterCount == 1 || emphasis.DelimiterCount == 3;
            foreach (var child in emphasis)
                ExtractInlineRuns(child, runs, bold || isBold, italic || isItalic);
        }
        else if (inline is ContainerInline container)
        {
            foreach (var child in container)
                ExtractInlineRuns(child, runs, bold, italic);
        }
        else if (inline is LiteralInline literal)
        {
            var text = literal.Content.ToString();
            if (!string.IsNullOrEmpty(text))
                runs.Add(new InlineRun { Text = text, Bold = bold, Italic = italic });
        }
        else if (inline is CodeInline code)
        {
            runs.Add(new InlineRun { Text = code.Content, IsCode = true });
        }
        else if (inline is LineBreakInline)
        {
            runs.Add(new InlineRun { Text = " " });
        }
    }
}
