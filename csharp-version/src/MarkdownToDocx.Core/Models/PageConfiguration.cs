using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;

namespace MarkdownToDocx.Core.Models;

/// <summary>
/// Represents page layout configuration for a Word document
/// </summary>
public sealed record PageConfiguration
{
    /// <summary>
    /// Page width in twips (1/1440 inch)
    /// </summary>
    public required UInt32Value Width { get; init; }

    /// <summary>
    /// Page height in twips (1/1440 inch)
    /// </summary>
    public required UInt32Value Height { get; init; }

    /// <summary>
    /// Page orientation (Portrait or Landscape)
    /// </summary>
    public required PageOrientationValues Orientation { get; init; }

    /// <summary>
    /// Top margin in twips
    /// </summary>
    public int TopMargin { get; init; } = 1134; // 2cm default

    /// <summary>
    /// Bottom margin in twips
    /// </summary>
    public int BottomMargin { get; init; } = 1134; // 2cm default

    /// <summary>
    /// Left margin in twips
    /// </summary>
    public int LeftMargin { get; init; } = 1417; // 2.5cm default

    /// <summary>
    /// Right margin in twips
    /// </summary>
    public int RightMargin { get; init; } = 1417; // 2.5cm default

    /// <summary>
    /// Header margin in twips
    /// </summary>
    public int HeaderMargin { get; init; } = 708;

    /// <summary>
    /// Footer margin in twips
    /// </summary>
    public int FooterMargin { get; init; } = 708;

    /// <summary>
    /// Gutter margin for binding
    /// </summary>
    public int GutterMargin { get; init; } = 0;
}
