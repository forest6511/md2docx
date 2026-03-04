using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace MarkdownToDocx.Tests;

/// <summary>
/// Validates OOXML layout properties to detect margin overflow risks.
/// Operates on structural metadata only — no rendering engine required.
/// </summary>
public static class LayoutValidator
{
    private const double TwipsPerCm = 567.0;

    /// <summary>Minimum acceptable outer margin in twips (10mm = 1.0cm).</summary>
    public const uint MinOuterMarginTwips = 567; // 10mm

    /// <summary>
    /// Maximum estimated characters per line before flagging overflow risk.
    /// Based on: printable width / (fontSize * 0.6 scaling factor for monospace).
    /// Callers should compute this dynamically; this constant is used as a fallback.
    /// </summary>
    public const int DefaultMaxCharsPerLine = 80;

    /// <summary>
    /// Reads page geometry from a WordprocessingDocument and returns a summary.
    /// </summary>
    public static PageGeometry GetPageGeometry(WordprocessingDocument doc)
    {
        var body = doc.MainDocumentPart!.Document.Body!;
        var sectionProps = body.Elements<SectionProperties>().LastOrDefault()
            ?? throw new InvalidOperationException("No SectionProperties found in document.");

        var pageSize = sectionProps.Elements<PageSize>().FirstOrDefault()
            ?? throw new InvalidOperationException("No PageSize found in SectionProperties.");

        var pageMargin = sectionProps.Elements<PageMargin>().FirstOrDefault()
            ?? throw new InvalidOperationException("No PageMargin found in SectionProperties.");

        var settingsPart = doc.MainDocumentPart.DocumentSettingsPart;
        bool mirrorMargins = settingsPart?.Settings
            .Elements<MirrorMargins>().Any() == true;

        uint widthTwips = pageSize.Width?.Value
            ?? throw new InvalidOperationException("PageSize.Width is null.");
        uint heightTwips = pageSize.Height?.Value
            ?? throw new InvalidOperationException("PageSize.Height is null.");
        uint leftTwips = pageMargin.Left?.Value
            ?? throw new InvalidOperationException("PageMargin.Left is null.");
        uint rightTwips = pageMargin.Right?.Value
            ?? throw new InvalidOperationException("PageMargin.Right is null.");

        uint printableWidthTwips = widthTwips - leftTwips - rightTwips;

        return new PageGeometry(
            WidthTwips: widthTwips,
            HeightTwips: heightTwips,
            LeftMarginTwips: leftTwips,
            RightMarginTwips: rightTwips,
            PrintableWidthTwips: printableWidthTwips,
            MirrorMargins: mirrorMargins);
    }

    /// <summary>
    /// Checks that both left and right margins meet the minimum threshold.
    /// With MirrorMargins, the outer margin is min(left, right).
    /// </summary>
    public static MarginCheckResult CheckMinimumMargins(PageGeometry geometry)
    {
        uint outerMarginTwips = geometry.MirrorMargins
            ? Math.Min(geometry.LeftMarginTwips, geometry.RightMarginTwips)
            : Math.Min(geometry.LeftMarginTwips, geometry.RightMarginTwips);

        bool passes = outerMarginTwips >= MinOuterMarginTwips;

        return new MarginCheckResult(
            Passes: passes,
            OuterMarginTwips: outerMarginTwips,
            OuterMarginMm: outerMarginTwips / TwipsPerCm * 10.0,
            MinRequiredTwips: MinOuterMarginTwips);
    }

    /// <summary>
    /// Checks code block paragraphs for lines that may exceed printable width.
    /// Uses character count × font size estimation (monospace assumption).
    /// Returns all violations found.
    /// </summary>
    public static IReadOnlyList<CodeBlockLineViolation> CheckCodeBlockLineWidths(
        WordprocessingDocument doc,
        PageGeometry geometry)
    {
        var violations = new List<CodeBlockLineViolation>();
        var body = doc.MainDocumentPart!.Document.Body!;

        // Code block paragraphs are identified by having ParagraphBorders on all 4 sides
        var codeBlockParagraphs = body.Descendants<Paragraph>()
            .Where(IsCodeBlockParagraph)
            .ToList();

        foreach (var para in codeBlockParagraphs)
        {
            // Retrieve font size from first run (in half-points → convert to pt)
            double fontSizePt = GetCodeBlockFontSizePt(para);

            // Estimate max chars that fit: printable_width_mm / (fontSize_pt * 0.21mm/pt * charWidthRatio)
            // Monospace char width ≈ fontSize × 0.6 (empirical for Consolas/Noto Sans Mono)
            double printableWidthMm = geometry.PrintableWidthTwips / TwipsPerCm * 10.0;
            int estimatedMaxChars = fontSizePt > 0
                ? (int)(printableWidthMm / (fontSizePt * 0.21 * 0.6))
                : DefaultMaxCharsPerLine;

            // Reconstruct lines from Text elements (separated by Break elements)
            var run = para.Elements<Run>().FirstOrDefault();
            if (run is null) continue;

            var lineTexts = ReconstructLines(run);
            for (int i = 0; i < lineTexts.Count; i++)
            {
                if (lineTexts[i].Length > estimatedMaxChars)
                {
                    violations.Add(new CodeBlockLineViolation(
                        LineIndex: i,
                        CharCount: lineTexts[i].Length,
                        EstimatedMaxChars: estimatedMaxChars,
                        FontSizePt: fontSizePt));
                }
            }
        }

        return violations;
    }

    /// <summary>
    /// Checks that all code block paragraphs have WordWrap enabled.
    /// </summary>
    public static WordWrapCheckResult CheckWordWrapOnCodeBlocks(WordprocessingDocument doc)
    {
        var body = doc.MainDocumentPart!.Document.Body!;
        var codeBlockParagraphs = body.Descendants<Paragraph>()
            .Where(IsCodeBlockParagraph)
            .ToList();

        int total = codeBlockParagraphs.Count;
        int missing = codeBlockParagraphs
            .Count(p => p.ParagraphProperties?.GetFirstChild<WordWrap>() is null);

        return new WordWrapCheckResult(
            TotalCodeBlocks: total,
            MissingWordWrap: missing,
            Passes: missing == 0);
    }

    /// <summary>
    /// Checks that no paragraph's left indentation exceeds half the printable width.
    /// </summary>
    public static IReadOnlyList<IndentViolation> CheckParagraphIndentation(
        WordprocessingDocument doc,
        PageGeometry geometry)
    {
        var violations = new List<IndentViolation>();
        var body = doc.MainDocumentPart!.Document.Body!;

        foreach (var para in body.Descendants<Paragraph>())
        {
            var indent = para.ParagraphProperties?.Elements<Indentation>().FirstOrDefault();
            if (indent?.Left?.Value is not null
                && int.TryParse(indent.Left.Value, out int leftIndentTwips)
                && leftIndentTwips > (int)(geometry.PrintableWidthTwips / 2))
            {
                violations.Add(new IndentViolation(
                    LeftIndentTwips: leftIndentTwips,
                    PrintableWidthTwips: (int)geometry.PrintableWidthTwips));
            }
        }

        return violations;
    }

    // ── private helpers ───────────────────────────────────────────────────────

    private static bool IsCodeBlockParagraph(Paragraph para)
    {
        var borders = para.ParagraphProperties?.ParagraphBorders;
        if (borders is null) return false;

        return borders.GetFirstChild<TopBorder>() is not null
            && borders.GetFirstChild<BottomBorder>() is not null
            && borders.GetFirstChild<LeftBorder>() is not null
            && borders.GetFirstChild<RightBorder>() is not null;
    }

    private static double GetCodeBlockFontSizePt(Paragraph para)
    {
        var fontSize = para.Elements<Run>().FirstOrDefault()
            ?.RunProperties
            ?.FontSize;

        if (fontSize?.Val?.Value is string val && int.TryParse(val, out int halfPoints))
            return halfPoints / 2.0;

        return 9.0; // fallback: 9pt default
    }

    private static List<string> ReconstructLines(Run run)
    {
        var lines = new List<string>();
        var currentLine = new System.Text.StringBuilder();

        foreach (var child in run.ChildElements)
        {
            if (child is Text text)
            {
                currentLine.Append(text.Text);
            }
            else if (child is Break)
            {
                lines.Add(currentLine.ToString());
                currentLine.Clear();
            }
        }

        if (currentLine.Length > 0 || lines.Count == 0)
            lines.Add(currentLine.ToString());

        return lines;
    }
}

// ── Result types ──────────────────────────────────────────────────────────────

public record PageGeometry(
    uint WidthTwips,
    uint HeightTwips,
    uint LeftMarginTwips,
    uint RightMarginTwips,
    uint PrintableWidthTwips,
    bool MirrorMargins);

public record MarginCheckResult(
    bool Passes,
    uint OuterMarginTwips,
    double OuterMarginMm,
    uint MinRequiredTwips);

public record CodeBlockLineViolation(
    int LineIndex,
    int CharCount,
    int EstimatedMaxChars,
    double FontSizePt);

public record WordWrapCheckResult(
    int TotalCodeBlocks,
    int MissingWordWrap,
    bool Passes);

public record IndentViolation(
    int LeftIndentTwips,
    int PrintableWidthTwips);
