using Markdig.Extensions.Tables;
using Markdig.Syntax;
using MarkdownToDocx.Core.Models;

namespace MarkdownToDocx.Core.Markdown;

/// <summary>
/// Extracts structured TableData from a Markdig Table AST node.
/// </summary>
public static class TableExtractor
{
    /// <summary>
    /// Converts a Markdig <see cref="Table"/> AST node into a <see cref="TableData"/> model.
    /// Column alignment is read from <see cref="TableColumnDefinition"/> entries.
    /// Cell content supports inline formatting (bold, italic, code spans) via <see cref="InlineRun"/>.
    /// </summary>
    public static TableData Extract(Table table)
    {
        ArgumentNullException.ThrowIfNull(table);

        // Map column alignments from TableColumnDefinitions
        var alignments = table.ColumnDefinitions
            .Select(col => col.Alignment switch
            {
                TableColumnAlign.Center => "center",
                TableColumnAlign.Right => "right",
                _ => "left"
            })
            .ToList();

        var rows = new List<TableRowData>();
        foreach (var child in table)
        {
            if (child is not TableRow row) continue;

            var cells = new List<TableCellData>();
            int colIndex = 0;
            foreach (var cellChild in row)
            {
                if (cellChild is not TableCell cell) continue;

                var runs = new List<InlineRun>();
                foreach (var cellBlock in cell)
                {
                    if (cellBlock is ParagraphBlock para && para.Inline != null)
                    {
                        ExtractInlineRuns(para.Inline, runs, bold: false, italic: false);
                    }
                }

                string align = colIndex < alignments.Count ? alignments[colIndex] : "left";
                cells.Add(new TableCellData { Runs = runs, Alignment = align });
                colIndex++;
            }

            rows.Add(new TableRowData { Cells = cells, IsHeader = row.IsHeader });
        }

        // Use actual cell count from rows (Markdig may produce extra phantom ColumnDefinitions
        // for tables with trailing pipes, so derive column count from content instead).
        int columnCount = rows.Count > 0 ? rows.Max(r => r.Cells.Count) : alignments.Count;
        return new TableData { Rows = rows, ColumnCount = columnCount };
    }

    private static void ExtractInlineRuns(
        Markdig.Syntax.Inlines.Inline? inline,
        List<InlineRun> runs,
        bool bold,
        bool italic)
    {
        if (inline == null) return;

        if (inline is Markdig.Syntax.Inlines.EmphasisInline emphasis)
        {
            bool isBold = emphasis.DelimiterCount >= 2;
            bool isItalic = emphasis.DelimiterCount == 1 || emphasis.DelimiterCount == 3;
            foreach (var child in emphasis)
                ExtractInlineRuns(child, runs, bold || isBold, italic || isItalic);
        }
        else if (inline is Markdig.Syntax.Inlines.ContainerInline container)
        {
            foreach (var child in container)
                ExtractInlineRuns(child, runs, bold, italic);
        }
        else if (inline is Markdig.Syntax.Inlines.LiteralInline literal)
        {
            var text = literal.Content.ToString();
            if (!string.IsNullOrEmpty(text))
                runs.Add(new InlineRun { Text = text, Bold = bold, Italic = italic });
        }
        else if (inline is Markdig.Syntax.Inlines.CodeInline code)
        {
            runs.Add(new InlineRun { Text = code.Content, IsCode = true });
        }
        else if (inline is Markdig.Syntax.Inlines.LineBreakInline)
        {
            runs.Add(new InlineRun { Text = " " });
        }
    }
}
