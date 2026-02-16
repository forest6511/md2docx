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
    /// <inheritdoc/>
    public TextDirectionMode Mode => TextDirectionMode.Horizontal;

    /// <inheritdoc/>
    public PageConfiguration GetPageConfiguration()
    {
        // Standard portrait orientation for horizontal text
        return new PageConfiguration
        {
            Width = new UInt32Value(8646U),    // 15.24cm in twips (15.24 * 567)
            Height = new UInt32Value(12950U),  // 22.86cm in twips (22.86 * 567)
            Orientation = PageOrientationValues.Portrait,

            // Standard margins for horizontal layout
            TopMargin = 1134,      // 2cm
            BottomMargin = 1134,   // 2cm
            LeftMargin = 1417,     // 2.5cm
            RightMargin = 1417,    // 2.5cm
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
            TextDirection = TextDirectionValues.LefToRightTopToBottom,  // Standard horizontal layout
            Kinsoku = false,  // Not needed for horizontal Western text
            LineSpacing = "360",  // 1.5x line spacing
            LineSpacingRule = LineSpacingRuleValues.Auto
        };
    }
}
