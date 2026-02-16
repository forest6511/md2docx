using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;
using MarkdownToDocx.Core.Interfaces;
using MarkdownToDocx.Core.Models;

namespace MarkdownToDocx.Core.TextDirection;

/// <summary>
/// Provides configuration for vertical text rendering (Japanese tategaki style)
/// Text flows top-to-bottom, right-to-left
/// </summary>
public sealed class VerticalTextProvider : ITextDirectionProvider
{
    // Page size constants for vertical layout (A4 landscape, in twips: 1cm = 567 twips)
    // For vertical text, dimensions are swapped: width and height are reversed
    private const uint A4_WIDTH_VERTICAL_TWIPS = 12950;  // 22.86cm (original height)
    private const uint A4_HEIGHT_VERTICAL_TWIPS = 8646;  // 15.24cm (original width)

    // Margin constants (in twips)
    // In vertical text, margins have different semantic meanings
    private const int MARGIN_2CM = 1134;     // 2cm - top/bottom become right/left
    private const int MARGIN_2_5CM = 1417;   // 2.5cm - left/right become top/bottom
    private const int MARGIN_1_25CM = 708;   // 1.25cm

    // Line spacing constant
    private const string LINE_SPACING_1_5X = "360"; // 1.5x line spacing

    /// <inheritdoc/>
    public TextDirectionMode Mode => TextDirectionMode.Vertical;

    /// <inheritdoc/>
    public PageConfiguration GetPageConfiguration()
    {
        return new PageConfiguration
        {
            Width = new UInt32Value(A4_WIDTH_VERTICAL_TWIPS),
            Height = new UInt32Value(A4_HEIGHT_VERTICAL_TWIPS),
            Orientation = PageOrientationValues.Landscape,

            // In vertical text layout, margin semantics change:
            // Top/Bottom → Right/Left sides in rendered output
            // Left/Right → Top/Bottom in rendered output
            TopMargin = MARGIN_2CM,      // Becomes right margin in vertical
            BottomMargin = MARGIN_2CM,   // Becomes left margin in vertical
            LeftMargin = MARGIN_2_5CM,   // Becomes top margin in vertical
            RightMargin = MARGIN_2_5CM,  // Becomes bottom margin in vertical
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
            TextDirection = TextDirectionValues.TopToBottomRightToLeft,
            Kinsoku = true, // Enable Japanese line breaking rules (kinsoku shori)
            LineSpacing = LINE_SPACING_1_5X,
            LineSpacingRule = LineSpacingRuleValues.Auto
        };
    }
}
