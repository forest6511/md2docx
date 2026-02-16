using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;
using MarkdownToDocx.Core.Interfaces;
using MarkdownToDocx.Core.Models;

namespace MarkdownToDocx.Core.TextDirection;

/// <summary>
/// Provides configuration for horizontal text rendering (Western style)
/// Text flows left-to-right, top-to-bottom
/// </summary>
public sealed class HorizontalTextProvider : ITextDirectionProvider
{
    // Page size constants (A4 portrait, in twips: 1cm = 567 twips)
    private const uint A4_WIDTH_TWIPS = 8646;   // 15.24cm
    private const uint A4_HEIGHT_TWIPS = 12950; // 22.86cm

    // Margin constants (in twips)
    private const int MARGIN_2CM = 1134;     // 2cm
    private const int MARGIN_2_5CM = 1417;   // 2.5cm
    private const int MARGIN_1_25CM = 708;   // 1.25cm

    // Line spacing constant
    private const string LINE_SPACING_1_5X = "360"; // 1.5x line spacing

    /// <inheritdoc/>
    public TextDirectionMode Mode => TextDirectionMode.Horizontal;

    /// <inheritdoc/>
    public PageConfiguration GetPageConfiguration()
    {
        return new PageConfiguration
        {
            Width = new UInt32Value(A4_WIDTH_TWIPS),
            Height = new UInt32Value(A4_HEIGHT_TWIPS),
            Orientation = PageOrientationValues.Portrait,

            TopMargin = MARGIN_2CM,
            BottomMargin = MARGIN_2CM,
            LeftMargin = MARGIN_2_5CM,
            RightMargin = MARGIN_2_5CM,
            HeaderMargin = MARGIN_1_25CM,
            FooterMargin = MARGIN_1_25CM,
            GutterMargin = 0
        };
    }

    /// <inheritdoc/>
    public ParagraphConfiguration GetParagraphConfiguration()
    {
        return new ParagraphConfiguration
        {
            TextDirection = TextDirectionValues.LefToRightTopToBottom,
            Kinsoku = false,
            LineSpacing = LINE_SPACING_1_5X,
            LineSpacingRule = LineSpacingRuleValues.Auto
        };
    }
}
