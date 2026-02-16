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
    /// <inheritdoc/>
    public TextDirectionMode Mode => TextDirectionMode.Vertical;

    /// <inheritdoc/>
    public PageConfiguration GetPageConfiguration()
    {
        // For vertical text, we use landscape orientation with swapped dimensions
        // Horizontal 15.24cm x 22.86cm becomes Vertical 22.86cm x 15.24cm
        return new PageConfiguration
        {
            Width = new UInt32Value(12950U),   // 22.86cm in twips (22.86 * 567)
            Height = new UInt32Value(8646U),    // 15.24cm in twips (15.24 * 567)
            Orientation = PageOrientationValues.Landscape,

            // Margins for vertical layout
            // In vertical text, top/bottom become right/left sides
            TopMargin = 1134,      // 2cm - becomes right margin in vertical
            BottomMargin = 1134,   // 2cm - becomes left margin in vertical
            LeftMargin = 1417,     // 2.5cm - becomes top margin in vertical
            RightMargin = 1417,    // 2.5cm - becomes bottom margin in vertical
            HeaderMargin = 708,    // 1.25cm
            FooterMargin = 708,    // 1.25cm
            GutterMargin = 0
        };
    }

    /// <inheritdoc/>
    public ParagraphConfiguration GetParagraphConfiguration()
    {
        return new ParagraphConfiguration
        {
            TextDirection = TextDirectionValues.TopToBottomRightToLeft,
            Kinsoku = true,  // Enable Japanese line breaking rules
            LineSpacing = "360",  // 1.5x line spacing
            LineSpacingRule = LineSpacingRuleValues.Auto
        };
    }
}
