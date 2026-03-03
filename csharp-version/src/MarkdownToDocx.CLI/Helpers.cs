using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using MarkdownToDocx.Core.Models;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace MarkdownToDocx.CLI;

public static class Helpers
{
    public static int GetHeadingLevel(HeadingBlock heading) => heading.Level;

    public static string GetBlockText(LeafBlock block)
    {
        if (block.Inline != null)
        {
            return ExtractInlineText(block.Inline);
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

    public static IEnumerable<ListItem> GetListItems(ListBlock block)
    {
        var items = new List<ListItem>();

        foreach (var item in block)
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
                // Items with no extractable text (e.g. image-only) are intentionally skipped
            }
        }

        return items;
    }

    public static string GetCodeBlockText(FencedCodeBlock fencedCode)
    {
        var sb = new StringBuilder();
        foreach (var line in fencedCode.Lines.Lines)
        {
            if (sb.Length > 0) sb.Append('\n');
            sb.Append(line.Slice.ToString());
        }
        return sb.ToString().TrimEnd('\r', '\n');
    }

    public static string? GetCodeBlockLanguage(FencedCodeBlock fencedCode) => fencedCode.Info;

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

    public static IReadOnlyList<InlineRun> GetQuoteRuns(QuoteBlock block)
    {
        var runs = new List<InlineRun>();
        bool firstParagraph = true;

        foreach (var child in block)
        {
            if (child is ParagraphBlock paragraph && paragraph.Inline != null)
            {
                if (!firstParagraph)
                    runs.Add(new InlineRun { Text = " " });

                ExtractInlineRuns(paragraph.Inline, runs, bold: false, italic: false);
                firstParagraph = false;
            }
        }

        return runs;
    }

    /// <summary>
    /// Resolves a potentially relative path against the directory of a base file.
    /// Absolute paths are returned unchanged.
    /// </summary>
    public static string ResolveRelativePath(string path, string basePath)
    {
        if (Path.IsPathRooted(path)) return path;
        var baseDir = Path.GetDirectoryName(Path.GetFullPath(basePath));
        return baseDir != null ? Path.GetFullPath(Path.Combine(baseDir, path)) : path;
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
