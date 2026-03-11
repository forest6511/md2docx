using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;
using MarkdownToDocx.Core.Interfaces;
using MarkdownToDocx.Core.Models;
using MarkdownToDocx.Styling.Models;

namespace MarkdownToDocx.Styling.TextDirection;

/// <summary>
/// Decorator that applies YAML PageLayout configuration over a base ITextDirectionProvider.
/// When Width and Height are specified in the YAML preset, they override the provider's defaults.
/// </summary>
public sealed class ConfigurableTextDirectionProvider(
    ITextDirectionProvider inner,
    PageLayoutConfig layout) : ITextDirectionProvider
{
    private const double CmToTwips = 567.0;

    /// <inheritdoc/>
    public TextDirectionMode Mode => inner.Mode;

    /// <inheritdoc/>
    public PageConfiguration GetPageConfiguration()
    {
        var baseConfig = inner.GetPageConfiguration();

        if (layout.Width <= 0 || layout.Height <= 0)
            return baseConfig;

        var width = (uint)Math.Round(layout.Width * CmToTwips);
        var height = (uint)Math.Round(layout.Height * CmToTwips);
        var orientation = width > height
            ? PageOrientationValues.Landscape
            : PageOrientationValues.Portrait;

        return baseConfig with
        {
            Width = new UInt32Value(width),
            Height = new UInt32Value(height),
            Orientation = orientation,
            TopMargin = layout.MarginTop > 0 ? (int)Math.Round(layout.MarginTop * CmToTwips) : baseConfig.TopMargin,
            BottomMargin = layout.MarginBottom > 0 ? (int)Math.Round(layout.MarginBottom * CmToTwips) : baseConfig.BottomMargin,
            LeftMargin = layout.MarginLeft > 0 ? (int)Math.Round(layout.MarginLeft * CmToTwips) : baseConfig.LeftMargin,
            RightMargin = layout.MarginRight > 0 ? (int)Math.Round(layout.MarginRight * CmToTwips) : baseConfig.RightMargin,
            HeaderMargin = layout.MarginHeader.HasValue ? (int)Math.Round(layout.MarginHeader.Value * CmToTwips) : baseConfig.HeaderMargin,
            FooterMargin = layout.MarginFooter.HasValue ? (int)Math.Round(layout.MarginFooter.Value * CmToTwips) : baseConfig.FooterMargin,
            GutterMargin = layout.MarginGutter.HasValue ? (int)Math.Round(layout.MarginGutter.Value * CmToTwips) : baseConfig.GutterMargin,
            MirrorMargins = layout.MirrorMargins,
        };
    }

    /// <inheritdoc/>
    public ParagraphConfiguration GetParagraphConfiguration() =>
        inner.GetParagraphConfiguration();
}
