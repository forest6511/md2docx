using System.Globalization;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using MarkdownToDocx.Core.Imaging;
using MarkdownToDocx.Core.Interfaces;
using MarkdownToDocx.Core.Models;
using CoreListItem = MarkdownToDocx.Core.Models.ListItem;
using CoreTableStyle = MarkdownToDocx.Core.Models.TableStyle;
using A = DocumentFormat.OpenXml.Drawing;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using PIC = DocumentFormat.OpenXml.Drawing.Pictures;
using W = DocumentFormat.OpenXml.Wordprocessing;

namespace MarkdownToDocx.Core.OpenXml;

/// <summary>
/// Builds Word (DOCX) documents using DocumentFormat.OpenXml
/// Implements the Builder pattern for document construction
/// </summary>
public sealed class OpenXmlDocumentBuilder : IDocumentBuilder
{
    private readonly WordprocessingDocument _document;
    private readonly Body _body;
    private readonly ITextDirectionProvider _textDirection;
    private readonly ParagraphConfiguration _paragraphConfig;
    private bool _disposed;
    private uint _drawingId;

    /// <summary>
    /// Initializes a new instance of <see cref="OpenXmlDocumentBuilder"/>
    /// </summary>
    /// <param name="outputStream">Stream to write the document to</param>
    /// <param name="textDirection">Text direction provider for layout configuration</param>
    /// <exception cref="ArgumentNullException">Thrown when parameters are null</exception>
    public OpenXmlDocumentBuilder(Stream outputStream, ITextDirectionProvider textDirection)
    {
        ArgumentNullException.ThrowIfNull(outputStream);
        ArgumentNullException.ThrowIfNull(textDirection);

        _textDirection = textDirection;
        _paragraphConfig = textDirection.GetParagraphConfiguration();
        _document = WordprocessingDocument.Create(outputStream, WordprocessingDocumentType.Document);

        try
        {
            InitializeDocument();
            _body = _document.MainDocumentPart?.Document?.Body
                ?? throw new InvalidOperationException("Failed to initialize document structure");
        }
        catch
        {
            _document.Dispose();
            throw;
        }
    }

    /// <summary>
    /// Initializes the document structure with main document part and body
    /// </summary>
    private void InitializeDocument()
    {
        var mainPart = _document.AddMainDocumentPart();
        mainPart.Document = new Document();
        var body = mainPart.Document.AppendChild(new Body());

        // Add mirror margins to document settings if required
        var pageConfig = _textDirection.GetPageConfiguration();
        if (pageConfig.MirrorMargins)
        {
            var settingsPart = mainPart.AddNewPart<DocumentSettingsPart>();
            settingsPart.Settings = new Settings(new W.MirrorMargins());
        }

        // Add section properties for page layout and text direction
        var sectionProps = CreateSectionProperties();
        body.AppendChild(sectionProps);
    }

    /// <summary>
    /// Creates section properties based on text direction configuration
    /// </summary>
    private SectionProperties CreateSectionProperties()
    {
        var pageConfig = _textDirection.GetPageConfiguration();

        var sectionProps = new SectionProperties();

        // Text direction
        sectionProps.AppendChild(new W.TextDirection { Val = _paragraphConfig.TextDirection });

        // Page size
        sectionProps.AppendChild(new PageSize
        {
            Width = pageConfig.Width,
            Height = pageConfig.Height,
            Orient = pageConfig.Orientation
        });

        // Page margins (Top/Bottom are Int32Value, Left/Right/Header/Footer/Gutter are UInt32Value)
        sectionProps.AppendChild(new PageMargin
        {
            Top = pageConfig.TopMargin,
            Bottom = pageConfig.BottomMargin,
            Left = (uint)pageConfig.LeftMargin,
            Right = (uint)pageConfig.RightMargin,
            Header = (uint)pageConfig.HeaderMargin,
            Footer = (uint)pageConfig.FooterMargin,
            Gutter = (uint)pageConfig.GutterMargin
        });

        return sectionProps;
    }

    /// <summary>
    /// Creates ParagraphProperties pre-configured with text direction and kinsoku settings
    /// </summary>
    private ParagraphProperties CreateBaseParagraphProperties()
    {
        var props = new ParagraphProperties();

        props.AppendChild(new W.TextDirection { Val = _paragraphConfig.TextDirection });

        if (_paragraphConfig.Kinsoku)
        {
            props.AppendChild(new Kinsoku { Val = OnOffValue.FromBoolean(true) });
        }

        return props;
    }

    /// <summary>
    /// Converts a border style name to the corresponding BorderValues enum value.
    /// </summary>
    private static BorderValues ParseBorderStyle(string borderStyle) => borderStyle.ToLowerInvariant() switch
    {
        "double" => BorderValues.Double,
        "thick" => BorderValues.Thick,
        "dotted" => BorderValues.Dotted,
        "dashed" => BorderValues.Dashed,
        "dotdash" => BorderValues.DotDash,
        "wave" => BorderValues.Wave,
        "triple" => BorderValues.Triple,
        _ => BorderValues.Single
    };

    /// <summary>
    /// Creates a ParagraphBorders element from a comma-separated position string
    /// </summary>
    private static ParagraphBorders CreateBordersFromPositions(
        string borderPosition,
        string borderColor,
        uint borderSize,
        uint borderSpace,
        string borderStyle = "single")
    {
        var positions = borderPosition
            .ToLowerInvariant()
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        var borderVal = ParseBorderStyle(borderStyle);
        var paragraphBorders = new ParagraphBorders();

        foreach (var pos in positions)
        {
            OpenXmlElement border = pos switch
            {
                "left" => new LeftBorder { Val = borderVal, Color = borderColor, Size = borderSize, Space = borderSpace },
                "right" => new RightBorder { Val = borderVal, Color = borderColor, Size = borderSize, Space = borderSpace },
                "top" => new TopBorder { Val = borderVal, Color = borderColor, Size = borderSize, Space = borderSpace },
                _ => new BottomBorder { Val = borderVal, Color = borderColor, Size = borderSize, Space = borderSpace }
            };
            paragraphBorders.AppendChild(border);
        }

        // When only "left" is specified, Word anchors the border at the paragraph indent (flush
        // with text) instead of at the page content edge. Adding Nil borders for the unspecified
        // sides forces Word into box-mode rendering, which anchors the left border at position 0
        // so that LeftIndent creates a visible gap between the border line and the text.
        var specifiedPositions = new HashSet<string>(positions);
        if (specifiedPositions.Contains("left"))
        {
            if (!specifiedPositions.Contains("top"))
                paragraphBorders.AppendChild(new TopBorder { Val = BorderValues.Nil });
            if (!specifiedPositions.Contains("bottom"))
                paragraphBorders.AppendChild(new BottomBorder { Val = BorderValues.Nil });
            if (!specifiedPositions.Contains("right"))
                paragraphBorders.AppendChild(new RightBorder { Val = BorderValues.Nil });
        }

        return paragraphBorders;
    }

    /// <summary>
    /// Applies monospace font settings to RunProperties for inline code spans.
    /// Centralises the font assignment used by both AddParagraph and AddQuote.
    /// </summary>
    private static void ApplyInlineCodeFont(RunProperties runProps, string asciiFont, string eastAsiaFont) =>
        runProps.AppendChild(new RunFonts { Ascii = asciiFont, EastAsia = eastAsiaFont });

    /// <summary>
    /// Creates a background shading element
    /// </summary>
    private static Shading CreateBackgroundShading(string fillColor) => new()
    {
        Val = ShadingPatternValues.Clear,
        Color = "auto",
        Fill = fillColor
    };

    /// <summary>
    /// Creates base RunProperties with font size, color, and optional bold/italic
    /// </summary>
    private static RunProperties CreateBaseRunProperties(
        int fontSize,
        string color,
        bool bold = false,
        bool italic = false)
    {
        var props = new RunProperties();

        if (bold)
        {
            props.AppendChild(new Bold());
        }

        if (italic)
        {
            props.AppendChild(new Italic());
        }

        props.AppendChild(new FontSize { Val = fontSize.ToString(CultureInfo.InvariantCulture) });

        // Guard against empty color strings from YAML configs that omit the color field
        if (!string.IsNullOrEmpty(color))
        {
            props.AppendChild(new Color { Val = color });
        }

        return props;
    }

    /// <inheritdoc/>
    public void AddTitlePage(TitlePageStyle style)
    {
        ArgumentNullException.ThrowIfNull(style);

        if (!style.Enabled || string.IsNullOrEmpty(style.ImagePath))
        {
            return;
        }

        if (!File.Exists(style.ImagePath))
        {
            throw new FileNotFoundException($"Cover image not found: {style.ImagePath}", style.ImagePath);
        }

        var mainPart = _document.MainDocumentPart
            ?? throw new InvalidOperationException("MainDocumentPart is not initialized");

        // Read image dimensions and calculate display size
        var (imageWidth, imageHeight) = ImageDimensionReader.GetDimensions(style.ImagePath);
        var contentType = ImageDimensionReader.GetContentType(style.ImagePath);

        // Calculate printable area in EMUs (1 twip = 635 EMUs)
        var pageConfig = _textDirection.GetPageConfiguration();
        long printableWidthEmu = ((long)(uint)pageConfig.Width - pageConfig.LeftMargin - pageConfig.RightMargin) * 635L;
        long printableHeightEmu = ((long)(uint)pageConfig.Height - pageConfig.TopMargin - pageConfig.BottomMargin) * 635L;

        // Max dimensions based on configured percentages
        long maxWidthEmu = printableWidthEmu * Math.Clamp(style.ImageMaxWidthPercent, 1, 100) / 100;
        long maxHeightEmu = printableHeightEmu * Math.Clamp(style.ImageMaxHeightPercent, 1, 100) / 100;

        // Scale image to fit within max area, maintaining aspect ratio, no upscaling
        long imageWidthEmu = (long)imageWidth * 914400L / 96L;  // pixels to EMUs (96 DPI)
        long imageHeightEmu = (long)imageHeight * 914400L / 96L;

        double scaleX = imageWidthEmu > maxWidthEmu ? (double)maxWidthEmu / imageWidthEmu : 1.0;
        double scaleY = imageHeightEmu > maxHeightEmu ? (double)maxHeightEmu / imageHeightEmu : 1.0;
        double scale = Math.Min(scaleX, scaleY);

        long displayWidthEmu = (long)(imageWidthEmu * scale);
        long displayHeightEmu = (long)(imageHeightEmu * scale);

        // Add image part
        var imagePart = mainPart.AddImagePart(contentType switch
        {
            "image/png" => ImagePartType.Png,
            _ => ImagePartType.Jpeg
        });

        using (var imageStream = File.OpenRead(style.ImagePath))
        {
            imagePart.FeedData(imageStream);
        }

        string relationshipId = mainPart.GetIdOfPart(imagePart);

        // Build Drawing element with inline image
        var drawing = CreateImageDrawing(relationshipId, displayWidthEmu, displayHeightEmu, ++_drawingId, "Cover Image", "cover");

        // Create centered paragraph with the image
        var paragraph = _body.AppendChild(new Paragraph());
        var paragraphProps = CreateBaseParagraphProperties();
        paragraphProps.AppendChild(new Justification { Val = JustificationValues.Center });

        // Use a section break to create an independent title page section
        // with vertical centering, instead of a simple page break
        if (style.PageBreakAfter)
        {
            var titleSectionProps = CreateSectionProperties();
            titleSectionProps.AppendChild(new VerticalTextAlignmentOnPage
            {
                Val = VerticalJustificationValues.Center
            });
            titleSectionProps.AppendChild(new SectionType { Val = SectionMarkValues.NextPage });
            paragraphProps.AppendChild(titleSectionProps);
        }

        paragraph.AppendChild(paragraphProps);

        var run = paragraph.AppendChild(new Run());
        run.AppendChild(drawing);
    }

    /// <summary>
    /// Creates a Drawing element containing an inline image with the specified dimensions
    /// </summary>
    private static Drawing CreateImageDrawing(
        string relationshipId,
        long widthEmu,
        long heightEmu,
        uint docPropertiesId,
        string docPropertiesName,
        string pictureName)
    {
        return new Drawing(
            new DW.Inline(
                new DW.Extent { Cx = widthEmu, Cy = heightEmu },
                new DW.EffectExtent { LeftEdge = 0, TopEdge = 0, RightEdge = 0, BottomEdge = 0 },
                new DW.DocProperties { Id = docPropertiesId, Name = docPropertiesName },
                new DW.NonVisualGraphicFrameDrawingProperties(
                    new A.GraphicFrameLocks { NoChangeAspect = true }),
                new A.Graphic(
                    new A.GraphicData(
                        new PIC.Picture(
                            new PIC.NonVisualPictureProperties(
                                new PIC.NonVisualDrawingProperties { Id = 0U, Name = pictureName },
                                new PIC.NonVisualPictureDrawingProperties()),
                            new PIC.BlipFill(
                                new A.Blip { Embed = relationshipId },
                                new A.Stretch(new A.FillRectangle())),
                            new PIC.ShapeProperties(
                                new A.Transform2D(
                                    new A.Offset { X = 0, Y = 0 },
                                    new A.Extents { Cx = widthEmu, Cy = heightEmu }),
                                new A.PresetGeometry(new A.AdjustValueList())
                                { Preset = A.ShapeTypeValues.Rectangle }))
                    )
                    { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" }))
            {
                DistanceFromTop = 0U,
                DistanceFromBottom = 0U,
                DistanceFromLeft = 0U,
                DistanceFromRight = 0U
            });
    }

    /// <inheritdoc/>
    public void AddImage(string imagePath, string altText, ImageStyle style)
    {
        ArgumentNullException.ThrowIfNull(imagePath);
        ArgumentNullException.ThrowIfNull(altText);
        ArgumentNullException.ThrowIfNull(style);

        if (!File.Exists(imagePath))
        {
            throw new FileNotFoundException($"Image file not found: {imagePath}", imagePath);
        }

        var mainPart = _document.MainDocumentPart
            ?? throw new InvalidOperationException("MainDocumentPart is not initialized");

        // Read image dimensions and calculate display size
        var (imageWidth, imageHeight) = ImageDimensionReader.GetDimensions(imagePath);
        var contentType = ImageDimensionReader.GetContentType(imagePath);

        // Calculate printable area in EMUs (1 twip = 635 EMUs)
        var pageConfig = _textDirection.GetPageConfiguration();
        long printableWidthEmu = ((long)(uint)pageConfig.Width - pageConfig.LeftMargin - pageConfig.RightMargin) * 635L;

        // Max width based on configured percentage
        long maxWidthEmu = printableWidthEmu * Math.Clamp(style.MaxWidthPercent, 1, 100) / 100;

        // Scale image to fit within max width, maintaining aspect ratio, no upscaling
        long imageWidthEmu = (long)imageWidth * 914400L / 96L;   // pixels → EMUs (96 DPI)
        long imageHeightEmu = (long)imageHeight * 914400L / 96L;

        double scale = imageWidthEmu > maxWidthEmu ? (double)maxWidthEmu / imageWidthEmu : 1.0;
        long displayWidthEmu = (long)(imageWidthEmu * scale);
        long displayHeightEmu = (long)(imageHeightEmu * scale);

        // Add image part
        var imagePart = mainPart.AddImagePart(contentType switch
        {
            "image/png" => ImagePartType.Png,
            _ => ImagePartType.Jpeg
        });

        using (var imageStream = File.OpenRead(imagePath))
        {
            imagePart.FeedData(imageStream);
        }

        string relationshipId = mainPart.GetIdOfPart(imagePart);
        uint id = ++_drawingId;

        var drawing = CreateImageDrawing(relationshipId, displayWidthEmu, displayHeightEmu, id, altText, altText);

        // Create paragraph with alignment
        var paragraph = _body.AppendChild(new Paragraph());
        var paragraphProps = CreateBaseParagraphProperties();

        var justification = style.Alignment.ToLowerInvariant() switch
        {
            "right" => JustificationValues.Right,
            "left" => JustificationValues.Left,
            _ => JustificationValues.Center
        };
        paragraphProps.AppendChild(new Justification { Val = justification });
        paragraph.AppendChild(paragraphProps);

        var run = paragraph.AppendChild(new Run());
        run.AppendChild(drawing);
    }

    /// <inheritdoc/>
    public void AddTableOfContents(TableOfContentsStyle style)
    {
        ArgumentNullException.ThrowIfNull(style);

        if (!style.Enabled)
        {
            return;
        }

        // Optional title paragraph
        if (!string.IsNullOrEmpty(style.Title))
        {
            var titleParagraph = _body.AppendChild(new Paragraph());
            var titleProps = CreateBaseParagraphProperties();
            titleProps.AppendChild(new SpacingBetweenLines { Before = "240", After = "120" });
            titleParagraph.AppendChild(titleProps);

            var titleRun = titleParagraph.AppendChild(new Run());
            titleRun.AppendChild(CreateBaseRunProperties(32, "000000", bold: true));
            titleRun.AppendChild(new Text(style.Title) { Space = SpaceProcessingModeValues.Preserve });
        }

        // TOC field code paragraph
        var tocParagraph = _body.AppendChild(new Paragraph());
        var tocProps = CreateBaseParagraphProperties();
        tocParagraph.AppendChild(tocProps);

        // Begin field
        var beginRun = tocParagraph.AppendChild(new Run());
        beginRun.AppendChild(new FieldChar { FieldCharType = FieldCharValues.Begin });

        // Instruction text
        var instrRun = tocParagraph.AppendChild(new Run());
        instrRun.AppendChild(new FieldCode($" TOC \\o \"1-{style.Depth}\" \\u ")
        {
            Space = SpaceProcessingModeValues.Preserve
        });

        // Separate field
        var separateRun = tocParagraph.AppendChild(new Run());
        separateRun.AppendChild(new FieldChar { FieldCharType = FieldCharValues.Separate });

        // Placeholder text shown until user updates the field
        var placeholderRun = tocParagraph.AppendChild(new Run());
        placeholderRun.AppendChild(CreateBaseRunProperties(22, "808080"));
        placeholderRun.AppendChild(new Text(
            "Right-click here and select 'Update Field' to generate the table of contents.")
        {
            Space = SpaceProcessingModeValues.Preserve
        });

        // End field
        var endRun = tocParagraph.AppendChild(new Run());
        endRun.AppendChild(new FieldChar { FieldCharType = FieldCharValues.End });

        // Optional page break after TOC
        if (style.PageBreakAfter)
        {
            var breakParagraph = _body.AppendChild(new Paragraph());
            var breakProps = CreateBaseParagraphProperties();
            breakParagraph.AppendChild(breakProps);

            var breakRun = breakParagraph.AppendChild(new Run());
            breakRun.AppendChild(new Break { Type = BreakValues.Page });
        }
    }

    /// <inheritdoc/>
    public void AddHeading(int level, string text, HeadingStyle style)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(style);

        if (level < 1 || level > 6)
        {
            throw new ArgumentOutOfRangeException(nameof(level), "Heading level must be between 1 and 6");
        }

        if (style.ShowBorder && string.Equals(style.BorderExtent, "text", StringComparison.OrdinalIgnoreCase))
        {
            AddHeadingWithSpacers(level, text, style);
        }
        else
        {
            AddHeadingSingleParagraph(level, text, style);
        }
    }

    /// <summary>
    /// Adds a heading as a single paragraph (default behavior).
    /// Border encompasses the full paragraph box including spacing.
    /// </summary>
    private void AddHeadingSingleParagraph(int level, string text, HeadingStyle style)
    {
        var paragraph = _body.AppendChild(new Paragraph());
        var paragraphProps = CreateHeadingParagraphProperties(level, style);
        paragraph.AppendChild(paragraphProps);

        // Icon prefix run
        if (!string.IsNullOrEmpty(style.IconPrefix))
        {
            var iconColor = string.IsNullOrEmpty(style.IconPrefixColor) ? style.Color : style.IconPrefixColor;
            var iconRun = paragraph.AppendChild(new Run());
            iconRun.AppendChild(CreateBaseRunProperties(style.FontSize, iconColor, bold: style.Bold));
            iconRun.AppendChild(new Text(style.IconPrefix + " ") { Space = SpaceProcessingModeValues.Preserve });
        }

        var run = paragraph.AppendChild(new Run());
        run.AppendChild(CreateBaseRunProperties(style.FontSize, style.Color, bold: style.Bold));
        run.AppendChild(new Text(text) { Space = SpaceProcessingModeValues.Preserve });
    }

    /// <summary>
    /// Adds a heading using spacer paragraphs so the border hugs the text only.
    /// Up to 3 paragraphs: before-spacer (SpaceBefore), main heading (border, zero spacing),
    /// after-spacer (SpaceAfter).
    /// </summary>
    private void AddHeadingWithSpacers(int level, string text, HeadingStyle style)
    {
        bool hasSpaceBefore = style.SpaceBefore != "0" && !string.IsNullOrEmpty(style.SpaceBefore);
        bool hasSpaceAfter = style.SpaceAfter != "0" && !string.IsNullOrEmpty(style.SpaceAfter);

        // Before-spacer paragraph: carries SpaceBefore and PageBreakBefore
        if (hasSpaceBefore || style.PageBreakBefore)
        {
            var beforeParagraph = _body.AppendChild(new Paragraph());
            var beforeProps = CreateBaseParagraphProperties();

            if (style.PageBreakBefore)
            {
                beforeProps.AppendChild(new W.PageBreakBefore { Val = OnOffValue.FromBoolean(true) });
            }

            var beforeSpacing = new SpacingBetweenLines
            {
                Before = hasSpaceBefore ? style.SpaceBefore : "0",
                After = "0"
            };
            beforeProps.AppendChild(beforeSpacing);

            beforeParagraph.AppendChild(beforeProps);
        }

        // Main heading paragraph: border + OutlineLevel + BackgroundColor, zero spacing
        {
            var mainParagraph = _body.AppendChild(new Paragraph());
            var mainProps = CreateBaseParagraphProperties();

            // Outline level for TOC and navigation (only on main paragraph)
            mainProps.AppendChild(new OutlineLevel { Val = level - 1 });

            // Border
            mainProps.AppendChild(CreateBordersFromPositions(
                style.BorderPosition,
                style.BorderColor ?? "3498db",
                style.BorderSize,
                style.BorderSpace,
                style.BorderStyle));

            // Background shading
            if (!string.IsNullOrEmpty(style.BackgroundColor))
            {
                mainProps.AppendChild(CreateBackgroundShading(style.BackgroundColor));
            }

            // Indentation
            if (!string.IsNullOrEmpty(style.LeftIndent) && style.LeftIndent != "0")
            {
                mainProps.AppendChild(new Indentation { Left = style.LeftIndent });
            }

            // Zero spacing so border hugs the text
            var mainSpacing = new SpacingBetweenLines { Before = "0", After = "0" };
            if (!string.IsNullOrEmpty(style.LineSpacing))
            {
                mainSpacing.Line = style.LineSpacing;
                mainSpacing.LineRule = LineSpacingRuleValues.Exact;
            }
            mainProps.AppendChild(mainSpacing);

            mainParagraph.AppendChild(mainProps);

            // Icon prefix run
            if (!string.IsNullOrEmpty(style.IconPrefix))
            {
                var iconColor = string.IsNullOrEmpty(style.IconPrefixColor) ? style.Color : style.IconPrefixColor;
                var iconRun = mainParagraph.AppendChild(new Run());
                iconRun.AppendChild(CreateBaseRunProperties(style.FontSize, iconColor, bold: style.Bold));
                iconRun.AppendChild(new Text(style.IconPrefix + " ") { Space = SpaceProcessingModeValues.Preserve });
            }

            var run = mainParagraph.AppendChild(new Run());
            run.AppendChild(CreateBaseRunProperties(style.FontSize, style.Color, bold: style.Bold));
            run.AppendChild(new Text(text) { Space = SpaceProcessingModeValues.Preserve });
        }

        // After-spacer paragraph: carries SpaceAfter
        if (hasSpaceAfter)
        {
            var afterParagraph = _body.AppendChild(new Paragraph());
            var afterProps = CreateBaseParagraphProperties();

            var afterSpacing = new SpacingBetweenLines
            {
                Before = "0",
                After = style.SpaceAfter
            };
            afterProps.AppendChild(afterSpacing);

            afterParagraph.AppendChild(afterProps);
        }
    }

    /// <summary>
    /// Creates paragraph properties for heading elements
    /// </summary>
    private ParagraphProperties CreateHeadingParagraphProperties(int level, HeadingStyle style)
    {
        var props = CreateBaseParagraphProperties();

        // Outline level for TOC and navigation support (0-based: H1=0, H2=1, ...)
        props.AppendChild(new OutlineLevel { Val = level - 1 });

        // Page break before
        if (style.PageBreakBefore)
        {
            props.AppendChild(new W.PageBreakBefore { Val = OnOffValue.FromBoolean(true) });
        }

        // Border
        if (style.ShowBorder)
        {
            props.AppendChild(CreateBordersFromPositions(
                style.BorderPosition,
                style.BorderColor ?? "3498db",
                style.BorderSize,
                style.BorderSpace,
                style.BorderStyle));
        }

        // Background shading
        if (!string.IsNullOrEmpty(style.BackgroundColor))
        {
            props.AppendChild(CreateBackgroundShading(style.BackgroundColor));
        }

        // Indentation
        if (!string.IsNullOrEmpty(style.LeftIndent))
        {
            props.AppendChild(new Indentation { Left = style.LeftIndent });
        }

        // Spacing
        var spacing = new SpacingBetweenLines
        {
            Before = style.SpaceBefore,
            After = style.SpaceAfter
        };
        if (!string.IsNullOrEmpty(style.LineSpacing))
        {
            spacing.Line = style.LineSpacing;
            spacing.LineRule = LineSpacingRuleValues.Exact;
        }
        props.AppendChild(spacing);

        return props;
    }

    /// <inheritdoc/>
    public void AddParagraph(IReadOnlyList<InlineRun> runs, ParagraphStyle style)
    {
        ArgumentNullException.ThrowIfNull(runs);
        ArgumentNullException.ThrowIfNull(style);

        var paragraph = _body.AppendChild(new Paragraph());
        var paragraphProps = CreateParagraphProperties(style);
        paragraph.AppendChild(paragraphProps);

        foreach (var inlineRun in runs)
        {
            var run = paragraph.AppendChild(new Run());
            var runProps = CreateBaseRunProperties(
                style.FontSize,
                style.Color,
                bold: inlineRun.Bold,
                italic: inlineRun.IsCode ? false : inlineRun.Italic);

            if (inlineRun.IsCode)
            {
                ApplyInlineCodeFont(runProps, style.InlineCodeFontAscii, style.InlineCodeFontEastAsia);
            }

            run.AppendChild(runProps);
            run.AppendChild(new Text(inlineRun.Text) { Space = SpaceProcessingModeValues.Preserve });
        }
    }

    /// <summary>
    /// Creates paragraph properties for standard paragraphs
    /// </summary>
    private ParagraphProperties CreateParagraphProperties(ParagraphStyle style)
    {
        var props = CreateBaseParagraphProperties();

        // Line spacing
        props.AppendChild(new SpacingBetweenLines
        {
            Line = style.LineSpacing,
            LineRule = LineSpacingRuleValues.Auto
        });

        // Indentation
        props.AppendChild(new Indentation
        {
            FirstLine = style.FirstLineIndent,
            Left = style.LeftIndent
        });

        return props;
    }

    /// <inheritdoc/>
    public void AddList(IEnumerable<CoreListItem> items, bool isOrdered, ListStyle style, int startNumber = 1)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(style);

        int itemNumber = startNumber;
        foreach (var item in items)
        {
            var paragraph = _body.AppendChild(new Paragraph());
            var paragraphProps = CreateListParagraphProperties(style);
            paragraph.AppendChild(paragraphProps);

            var run = paragraph.AppendChild(new Run());
            run.AppendChild(CreateBaseRunProperties(style.FontSize, style.Color));

            // Add bullet or number
            string bullet = isOrdered ? $"{itemNumber}. " : "• ";
            run.AppendChild(new Text(bullet + item.Text) { Space = SpaceProcessingModeValues.Preserve });

            if (isOrdered)
            {
                itemNumber++;
            }
        }
    }

    /// <summary>
    /// Creates paragraph properties for list items
    /// </summary>
    private ParagraphProperties CreateListParagraphProperties(ListStyle style)
    {
        var props = CreateBaseParagraphProperties();

        // Indentation
        props.AppendChild(new Indentation
        {
            Left = style.LeftIndent,
            Hanging = style.HangingIndent
        });

        // Spacing
        props.AppendChild(new SpacingBetweenLines
        {
            Before = style.SpaceBefore,
            After = style.SpaceAfter
        });

        return props;
    }

    /// <inheritdoc/>
    public void AddCodeBlock(string code, string? language, CodeBlockStyle style)
    {
        ArgumentNullException.ThrowIfNull(code);
        ArgumentNullException.ThrowIfNull(style);

        var paragraph = _body.AppendChild(new Paragraph());
        var paragraphProps = CreateCodeBlockParagraphProperties(style);
        paragraph.AppendChild(paragraphProps);

        var run = paragraph.AppendChild(new Run());
        var runProps = CreateBaseRunProperties(style.FontSize, style.Color);
        runProps.AppendChild(new RunFonts
        {
            Ascii = style.MonospaceFontAscii,
            EastAsia = style.MonospaceFontEastAsia
        });
        run.AppendChild(runProps);

        // Add code with preserved whitespace
        var lines = code.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            run.AppendChild(new Text(lines[i].TrimEnd('\r')) { Space = SpaceProcessingModeValues.Preserve });
            if (i < lines.Length - 1)
            {
                run.AppendChild(new Break());
            }
        }
    }

    /// <summary>
    /// Creates paragraph properties for code blocks
    /// </summary>
    private ParagraphProperties CreateCodeBlockParagraphProperties(CodeBlockStyle style)
    {
        var props = CreateBaseParagraphProperties();

        // Borders
        props.AppendChild(new ParagraphBorders(
            new TopBorder { Val = BorderValues.Single, Color = style.BorderColor, Size = 4, Space = style.BorderSpace },
            new BottomBorder { Val = BorderValues.Single, Color = style.BorderColor, Size = 4, Space = style.BorderSpace },
            new LeftBorder { Val = BorderValues.Single, Color = style.BorderColor, Size = 4, Space = style.BorderSpace },
            new RightBorder { Val = BorderValues.Single, Color = style.BorderColor, Size = 4, Space = style.BorderSpace }
        ));

        // Background shading
        props.AppendChild(CreateBackgroundShading(style.BackgroundColor));

        // Indentation: left/right borders use w:space (points) as external gap toward the margin.
        // Without indentation the border extends space points outside the text area into the margin,
        // causing visible overflow on narrow outer margins (e.g. MirrorMargins with 1.27 cm outer).
        // Adding indent equal to BorderSpace * 20 twips anchors the border at the margin boundary.
        if (style.BorderSpace > 0)
        {
            string indentTwips = (style.BorderSpace * 20).ToString();
            props.AppendChild(new Indentation { Left = indentTwips, Right = indentTwips });
        }

        // Spacing
        props.AppendChild(new SpacingBetweenLines
        {
            Before = style.SpaceBefore,
            After = style.SpaceAfter,
            Line = style.LineSpacing,
            LineRule = LineSpacingRuleValues.Auto
        });

        // Word wrap
        if (style.WordWrap)
        {
            props.AppendChild(new WordWrap());
        }

        return props;
    }

    /// <inheritdoc/>
    public void AddQuote(QuoteContent content, QuoteStyle style)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(style);

        foreach (var block in content.Blocks)
        {
            switch (block)
            {
                case QuoteParagraph p:
                    AddQuoteParagraph(p.Runs, style);
                    break;

                case QuoteList l:
                    AddQuoteList(l, style);
                    break;
            }
        }
    }

    /// <summary>
    /// Renders a paragraph inside a blockquote with quote styling.
    /// </summary>
    private void AddQuoteParagraph(IReadOnlyList<InlineRun> runs, QuoteStyle style)
    {
        var paragraph = _body.AppendChild(new Paragraph());
        var paragraphProps = CreateQuoteParagraphProperties(style);
        paragraph.AppendChild(paragraphProps);

        foreach (var inlineRun in runs)
        {
            var run = paragraph.AppendChild(new Run());
            var runProps = CreateBaseRunProperties(
                style.FontSize,
                style.Color,
                bold: inlineRun.Bold,
                italic: inlineRun.IsCode ? false : (style.Italic || inlineRun.Italic));

            if (inlineRun.IsCode)
            {
                ApplyInlineCodeFont(runProps, style.InlineCodeFontAscii, style.InlineCodeFontEastAsia);
            }

            run.AppendChild(runProps);
            run.AppendChild(new Text(inlineRun.Text) { Space = SpaceProcessingModeValues.Preserve });
        }
    }

    /// <summary>
    /// Renders a list inside a blockquote with quote styling on each item.
    /// </summary>
    private void AddQuoteList(QuoteList list, QuoteStyle style)
    {
        int itemNumber = list.StartNumber;
        foreach (var item in list.Items)
        {
            var paragraph = _body.AppendChild(new Paragraph());
            var paragraphProps = CreateQuoteParagraphProperties(style);
            paragraph.AppendChild(paragraphProps);

            var run = paragraph.AppendChild(new Run());
            var runProps = CreateBaseRunProperties(
                style.FontSize,
                style.Color,
                italic: style.Italic);
            run.AppendChild(runProps);

            string bullet = list.IsOrdered ? $"{itemNumber}. " : "\u2022 ";
            run.AppendChild(new Text(bullet + item.Text) { Space = SpaceProcessingModeValues.Preserve });

            if (list.IsOrdered) itemNumber++;
        }
    }

    /// <summary>
    /// Creates paragraph properties for quote blocks
    /// </summary>
    private ParagraphProperties CreateQuoteParagraphProperties(QuoteStyle style)
    {
        var props = CreateBaseParagraphProperties();

        // Border and/or padding
        bool hasPadding = style.PaddingSpace > 0 && !string.IsNullOrEmpty(style.BackgroundColor);
        if (style.ShowBorder || hasPadding)
        {
            var borders = new ParagraphBorders();

            if (style.ShowBorder)
            {
                var positions = style.BorderPosition
                    .ToLowerInvariant()
                    .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                foreach (var pos in positions)
                {
                    OpenXmlElement border = pos switch
                    {
                        "left" => new LeftBorder { Val = BorderValues.Single, Color = style.BorderColor, Size = style.BorderSize, Space = style.BorderSpace },
                        "right" => new RightBorder { Val = BorderValues.Single, Color = style.BorderColor, Size = style.BorderSize, Space = style.BorderSpace },
                        "top" => new TopBorder { Val = BorderValues.Single, Color = style.BorderColor, Size = style.BorderSize, Space = style.BorderSpace },
                        _ => new BottomBorder { Val = BorderValues.Single, Color = style.BorderColor, Size = style.BorderSize, Space = style.BorderSpace }
                    };
                    borders.AppendChild(border);
                }
            }

            if (hasPadding)
            {
                // Add invisible padding borders on top/right/bottom (skip positions already occupied by visible border)
                if (borders.GetFirstChild<TopBorder>() == null)
                    borders.AppendChild(new TopBorder { Val = BorderValues.Single, Color = style.BackgroundColor!, Size = 4, Space = style.PaddingSpace });
                if (borders.GetFirstChild<RightBorder>() == null)
                    borders.AppendChild(new RightBorder { Val = BorderValues.Single, Color = style.BackgroundColor!, Size = 4, Space = style.PaddingSpace });
                if (borders.GetFirstChild<BottomBorder>() == null)
                    borders.AppendChild(new BottomBorder { Val = BorderValues.Single, Color = style.BackgroundColor!, Size = 4, Space = style.PaddingSpace });
            }

            props.AppendChild(borders);
        }

        // Background shading
        if (!string.IsNullOrEmpty(style.BackgroundColor))
        {
            props.AppendChild(CreateBackgroundShading(style.BackgroundColor));
        }

        // Indentation: right padding border extends PaddingSpace points outside the right text
        // boundary into the margin. Add matching right indent to anchor the border at the margin.
        var indentation = new Indentation { Left = style.LeftIndent };
        if (hasPadding && style.PaddingSpace > 0)
        {
            indentation.Right = (style.PaddingSpace * 20).ToString();
        }
        props.AppendChild(indentation);

        // Spacing
        props.AppendChild(new SpacingBetweenLines
        {
            Before = style.SpaceBefore,
            After = style.SpaceAfter
        });

        return props;
    }

    /// <inheritdoc/>
    public void AddTable(TableData tableData, CoreTableStyle style)
    {
        ArgumentNullException.ThrowIfNull(tableData);
        ArgumentNullException.ThrowIfNull(style);

        // Compute printable text area width in twips from page configuration,
        // then scale by WidthPercent so the table can be narrower than the full text area.
        var pageConfig = _textDirection.GetPageConfiguration();
        int fullTextAreaTwips = (int)(uint)pageConfig.Width
            - pageConfig.LeftMargin
            - pageConfig.RightMargin
            - pageConfig.GutterMargin;
        int textAreaTwips = (int)(fullTextAreaTwips * style.WidthPercent / 100.0);

        // Spacer paragraph before table
        AddTableSpacer(style.SpaceBefore);

        var table = _body.AppendChild(new Table());
        table.AppendChild(CreateTableProperties(style));
        table.AppendChild(CreateTableGrid(tableData.ColumnCount, textAreaTwips));

        foreach (var row in tableData.Rows)
        {
            table.AppendChild(CreateTableRow(row, tableData.ColumnCount, style, textAreaTwips));
        }

        // Spacer paragraph after table
        AddTableSpacer(style.SpaceAfter);
    }

    /// <summary>
    /// Adds an empty spacer paragraph with specified spacing
    /// </summary>
    private void AddTableSpacer(string spacing)
    {
        if (string.IsNullOrEmpty(spacing) || spacing == "0") return;

        var spacer = _body.AppendChild(new Paragraph());
        var spacerProps = CreateBaseParagraphProperties();
        spacerProps.AppendChild(new SpacingBetweenLines { Before = "0", After = spacing });
        spacer.AppendChild(spacerProps);
    }

    /// <summary>
    /// Creates TableProperties with percentage-based width (overflow safety net) and
    /// fixed layout so Word honours the tblGrid column widths.
    /// </summary>
    private static TableProperties CreateTableProperties(CoreTableStyle style)
    {
        var tableProps = new TableProperties();

        // tblW in pct units (5000 = 100%). Scaled by WidthPercent so the table
        // can be narrower than the full text area (e.g. 90% → "4500").
        int tblWidthPct = style.WidthPercent * 50; // 1% = 50 in OOXML pct units
        tableProps.AppendChild(new TableWidth
        {
            Type = TableWidthUnitValues.Pct,
            Width = tblWidthPct.ToString(CultureInfo.InvariantCulture)
        });

        // Fixed layout: Word must honour tblGrid column widths, not autofit to content
        tableProps.AppendChild(new TableLayout { Type = TableLayoutValues.Fixed });

        // Table borders: all sides + inside horizontal/vertical
        tableProps.AppendChild(new TableBorders(
            new TopBorder { Val = BorderValues.Single, Color = style.BorderColor, Size = style.BorderSize, Space = 0U },
            new BottomBorder { Val = BorderValues.Single, Color = style.BorderColor, Size = style.BorderSize, Space = 0U },
            new LeftBorder { Val = BorderValues.Single, Color = style.BorderColor, Size = style.BorderSize, Space = 0U },
            new RightBorder { Val = BorderValues.Single, Color = style.BorderColor, Size = style.BorderSize, Space = 0U },
            new InsideHorizontalBorder { Val = BorderValues.Single, Color = style.BorderColor, Size = style.BorderSize, Space = 0U },
            new InsideVerticalBorder { Val = BorderValues.Single, Color = style.BorderColor, Size = style.BorderSize, Space = 0U }
        ));

        // Default cell padding
        tableProps.AppendChild(new TableCellMarginDefault(
            new TopMargin { Width = style.CellPaddingTop.ToString(CultureInfo.InvariantCulture), Type = TableWidthUnitValues.Dxa },
            new BottomMargin { Width = style.CellPaddingBottom.ToString(CultureInfo.InvariantCulture), Type = TableWidthUnitValues.Dxa },
            new StartMargin { Width = style.CellPaddingLeft.ToString(CultureInfo.InvariantCulture), Type = TableWidthUnitValues.Dxa },
            new EndMargin { Width = style.CellPaddingRight.ToString(CultureInfo.InvariantCulture), Type = TableWidthUnitValues.Dxa }
        ));

        return tableProps;
    }

    /// <summary>
    /// Creates a TableGrid element that defines equal-width columns so Word's fixed-layout
    /// engine can distribute the text area width evenly across all columns.
    /// </summary>
    private static TableGrid CreateTableGrid(int columnCount, int textAreaTwips)
    {
        var grid = new TableGrid();
        int colWidth = columnCount > 0 ? textAreaTwips / columnCount : textAreaTwips;
        for (int i = 0; i < columnCount; i++)
        {
            grid.AppendChild(new GridColumn { Width = colWidth.ToString(CultureInfo.InvariantCulture) });
        }
        return grid;
    }

    /// <summary>
    /// Creates a TableRow element with cells
    /// </summary>
    private TableRow CreateTableRow(TableRowData rowData, int columnCount, CoreTableStyle style, int textAreaTwips)
    {
        var row = new TableRow();

        // Mark header row
        if (rowData.IsHeader)
        {
            row.AppendChild(new TableRowProperties(new TableHeader()));
        }

        foreach (var cell in rowData.Cells)
        {
            row.AppendChild(CreateTableCell(cell, columnCount, rowData.IsHeader, style, textAreaTwips));
        }

        return row;
    }

    /// <summary>
    /// Creates a TableCell element with content
    /// </summary>
    private TableCell CreateTableCell(TableCellData cellData, int columnCount, bool isHeader, CoreTableStyle style, int textAreaTwips)
    {
        var cell = new TableCell();

        // Cell width in dxa matches the tblGrid gridCol width so Word renders fixed columns
        int colWidthDxa = columnCount > 0 ? textAreaTwips / columnCount : textAreaTwips;
        var cellProps = new TableCellProperties();
        cellProps.AppendChild(new TableCellWidth
        {
            Type = TableWidthUnitValues.Dxa,
            Width = colWidthDxa.ToString(CultureInfo.InvariantCulture)
        });

        // Cell background shading: header always uses HeaderBackgroundColor;
        // body cells use BodyCellBackgroundColor when set (e.g., inside a fenced div).
        if (isHeader)
        {
            cellProps.AppendChild(CreateBackgroundShading(style.HeaderBackgroundColor));
        }
        else if (!string.IsNullOrEmpty(style.BodyCellBackgroundColor))
        {
            cellProps.AppendChild(CreateBackgroundShading(style.BodyCellBackgroundColor));
        }

        cell.AppendChild(cellProps);

        // Cell paragraph
        var paragraph = cell.AppendChild(new Paragraph());
        var paraProps = CreateBaseParagraphProperties();

        // Column alignment
        var justification = cellData.Alignment.ToLowerInvariant() switch
        {
            "center" => JustificationValues.Center,
            "right" => JustificationValues.Right,
            _ => JustificationValues.Left
        };
        paraProps.AppendChild(new Justification { Val = justification });
        // Remove extra spacing inside cells
        paraProps.AppendChild(new SpacingBetweenLines { Before = "0", After = "0" });
        paragraph.AppendChild(paraProps);

        // Cell runs
        string textColor = isHeader ? style.HeaderTextColor : style.BodyTextColor;
        foreach (var inlineRun in cellData.Runs)
        {
            var run = paragraph.AppendChild(new Run());
            var runProps = CreateBaseRunProperties(
                style.FontSize,
                textColor,
                bold: isHeader && style.HeaderBold || inlineRun.Bold,
                italic: inlineRun.IsCode ? false : inlineRun.Italic);

            if (inlineRun.IsCode)
            {
                // Use default monospace fonts for inline code in cells
                runProps.AppendChild(new RunFonts { Ascii = "Consolas", EastAsia = "Noto Sans Mono CJK JP" });
            }

            run.AppendChild(runProps);
            run.AppendChild(new Text(inlineRun.Text) { Space = SpaceProcessingModeValues.Preserve });
        }

        // Ensure cell always has at least one paragraph (required by OOXML spec)
        if (!cellData.Runs.Any())
        {
            var emptyRun = paragraph.AppendChild(new Run());
            emptyRun.AppendChild(CreateBaseRunProperties(style.FontSize, textColor));
        }

        return cell;
    }

    /// <inheritdoc/>
    public void AddFencedDiv(FencedDivContent content, FencedDivStyle style)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(style);

        var blocks = content.Blocks;
        if (blocks.Count == 0) return;

        // Identify the first and last block indices that can carry paragraph borders.
        // Tables use cell shading instead of paragraph borders, so they are excluded here.
        int firstBorderable = -1;
        int lastBorderable = -1;
        for (int i = 0; i < blocks.Count; i++)
        {
            if (blocks[i] is not FencedDivTable)
            {
                if (firstBorderable < 0) firstBorderable = i;
                lastBorderable = i;
            }
        }

        // When the div starts or ends with a table, inject invisible spacer paragraphs
        // that carry the separator borders so the zone boundary is still visible.
        bool needTopSpacer = firstBorderable != 0 && !string.IsNullOrEmpty(style.BorderTopColor);
        bool needBottomSpacer = lastBorderable != blocks.Count - 1 && !string.IsNullOrEmpty(style.BorderBottomColor);

        if (needTopSpacer)
            AddDivBorderSpacer(style, addTop: true, addBottom: false, isFirst: true, isLast: false);

        for (int i = 0; i < blocks.Count; i++)
        {
            bool addTopBorder = !needTopSpacer && i == firstBorderable;
            bool addBottomBorder = !needBottomSpacer && i == lastBorderable;

            switch (blocks[i])
            {
                case FencedDivParagraph p:
                    AddDivParagraph(p.Runs, style, addTopBorder, addBottomBorder, i == 0, i == blocks.Count - 1);
                    break;

                case FencedDivHeading h:
                    AddDivHeading(h, style, addTopBorder, addBottomBorder, i == 0, i == blocks.Count - 1);
                    break;

                case FencedDivList l:
                    AddDivList(l, style, addTopBorder, addBottomBorder, i == 0, i == blocks.Count - 1);
                    break;

                case FencedDivTable t:
                    AddDivTable(t.Data, style);
                    break;
            }
        }

        if (needBottomSpacer)
            AddDivBorderSpacer(style, addTop: false, addBottom: true, isFirst: false, isLast: true);
    }

    /// <summary>
    /// Renders a paragraph inside a fenced div with background shading and optional separator borders.
    /// </summary>
    private void AddDivParagraph(
        IReadOnlyList<InlineRun> runs,
        FencedDivStyle style,
        bool addTopBorder,
        bool addBottomBorder,
        bool isFirst,
        bool isLast)
    {
        var paragraph = _body.AppendChild(new Paragraph());
        var props = CreateDivParagraphProperties(style, addTopBorder, addBottomBorder, isFirst, isLast);
        paragraph.AppendChild(props);

        foreach (var inlineRun in runs)
        {
            var run = paragraph.AppendChild(new Run());
            var runProps = CreateBaseRunProperties(style.FontSize, style.Color, inlineRun.Bold, inlineRun.Italic);
            if (inlineRun.IsCode)
                ApplyInlineCodeFont(runProps, style.InlineCodeFontAscii, style.InlineCodeFontEastAsia);
            run.AppendChild(runProps);
            run.AppendChild(new Text(inlineRun.Text) { Space = SpaceProcessingModeValues.Preserve });
        }
    }

    /// <summary>
    /// Renders a heading inside a fenced div as a bold paragraph with background shading.
    /// </summary>
    private void AddDivHeading(
        FencedDivHeading heading,
        FencedDivStyle style,
        bool addTopBorder,
        bool addBottomBorder,
        bool isFirst,
        bool isLast)
    {
        var paragraph = _body.AppendChild(new Paragraph());
        var props = CreateDivParagraphProperties(style, addTopBorder, addBottomBorder, isFirst, isLast);
        paragraph.AppendChild(props);

        var run = paragraph.AppendChild(new Run());
        run.AppendChild(CreateBaseRunProperties(style.FontSize, style.Color, bold: true, italic: false));
        run.AppendChild(new Text(heading.Text) { Space = SpaceProcessingModeValues.Preserve });
    }

    /// <summary>
    /// Renders a list inside a fenced div with background shading on each item paragraph.
    /// </summary>
    private void AddDivList(
        FencedDivList list,
        FencedDivStyle style,
        bool addTopBorder,
        bool addBottomBorder,
        bool isFirst,
        bool isLast)
    {
        int itemNumber = list.StartNumber;
        var items = list.Items.ToList();
        for (int j = 0; j < items.Count; j++)
        {
            bool itemIsFirst = isFirst && j == 0;
            bool itemIsLast = isLast && j == items.Count - 1;
            bool itemTopBorder = addTopBorder && j == 0;
            bool itemBottomBorder = addBottomBorder && j == items.Count - 1;

            var paragraph = _body.AppendChild(new Paragraph());
            var props = CreateDivParagraphProperties(style, itemTopBorder, itemBottomBorder, itemIsFirst, itemIsLast);
            paragraph.AppendChild(props);

            var run = paragraph.AppendChild(new Run());
            run.AppendChild(CreateBaseRunProperties(style.FontSize, style.Color));

            string bullet = list.IsOrdered ? $"{itemNumber}. " : "• ";
            run.AppendChild(new Text(bullet + items[j].Text) { Space = SpaceProcessingModeValues.Preserve });

            if (list.IsOrdered) itemNumber++;
        }
    }

    /// <summary>
    /// Renders a table inside a fenced div, applying background shading to all body cells.
    /// </summary>
    private void AddDivTable(TableData tableData, FencedDivStyle style)
    {
        var pageConfig = _textDirection.GetPageConfiguration();
        int fullTextAreaTwips = (int)(uint)pageConfig.Width
            - pageConfig.LeftMargin
            - pageConfig.RightMargin
            - pageConfig.GutterMargin;

        // Use a default TableStyle with the div's background applied to body cells
        var tableStyle = new CoreTableStyle
        {
            BodyCellBackgroundColor = string.IsNullOrEmpty(style.BackgroundColor) ? null : style.BackgroundColor
        };

        int textAreaTwips = (int)(fullTextAreaTwips * tableStyle.WidthPercent / 100.0);

        var table = _body.AppendChild(new Table());
        table.AppendChild(CreateTableProperties(tableStyle));
        table.AppendChild(CreateTableGrid(tableData.ColumnCount, textAreaTwips));

        foreach (var row in tableData.Rows)
        {
            table.AppendChild(CreateTableRow(row, tableData.ColumnCount, tableStyle, textAreaTwips));
        }
    }

    /// <summary>
    /// Creates an invisible spacer paragraph carrying a single separator border,
    /// used when the first or last block in a fenced div is a table.
    /// </summary>
    private void AddDivBorderSpacer(FencedDivStyle style, bool addTop, bool addBottom, bool isFirst, bool isLast)
    {
        var paragraph = _body.AppendChild(new Paragraph());
        var props = CreateDivParagraphProperties(style, addTop, addBottom, isFirst, isLast);
        // Zero-height spacer: suppress spacing so the line appears flush with the table
        props.AppendChild(new SpacingBetweenLines { Before = "0", After = "0" });
        paragraph.AppendChild(props);
    }

    /// <summary>
    /// Builds paragraph properties for a block inside a fenced div.
    /// Applies background shading, optional separator borders, and spacing.
    /// </summary>
    private ParagraphProperties CreateDivParagraphProperties(
        FencedDivStyle style,
        bool addTopBorder,
        bool addBottomBorder,
        bool isFirst,
        bool isLast)
    {
        var props = CreateBaseParagraphProperties();

        // Separator borders (top and/or bottom separator lines delimiting the zone)
        bool hasTopBorder = addTopBorder && !string.IsNullOrEmpty(style.BorderTopColor);
        bool hasBottomBorder = addBottomBorder && !string.IsNullOrEmpty(style.BorderBottomColor);

        if (hasTopBorder || hasBottomBorder)
        {
            var borders = new ParagraphBorders();
            if (hasTopBorder)
                borders.AppendChild(new TopBorder { Val = BorderValues.Single, Color = style.BorderTopColor, Size = style.BorderTopSize, Space = style.BorderSpace });
            if (hasBottomBorder)
                borders.AppendChild(new BottomBorder { Val = BorderValues.Single, Color = style.BorderBottomColor, Size = style.BorderBottomSize, Space = style.BorderSpace });
            props.AppendChild(borders);
        }

        // Background shading
        if (!string.IsNullOrEmpty(style.BackgroundColor))
            props.AppendChild(CreateBackgroundShading(style.BackgroundColor));

        // Line spacing
        props.AppendChild(new SpacingBetweenLines
        {
            Before = isFirst ? style.SpaceBefore : "0",
            After = isLast ? style.SpaceAfter : "0",
            Line = style.LineSpacing,
            LineRule = LineSpacingRuleValues.Auto
        });

        // Left indent
        if (style.LeftIndent != "0" && !string.IsNullOrEmpty(style.LeftIndent))
            props.AppendChild(new Indentation { Left = style.LeftIndent });

        return props;
    }

    /// <inheritdoc/>
    public void AddThematicBreak()
    {
        var paragraph = _body.AppendChild(new Paragraph());

        var props = CreateBaseParagraphProperties();
        props.AppendChild(new ParagraphBorders(
            new BottomBorder
            {
                Val = BorderValues.Single,
                Color = "cccccc",
                Size = 6,
                Space = 1
            }
        ));
        props.AppendChild(new SpacingBetweenLines
        {
            Before = "200",
            After = "200"
        });

        paragraph.AppendChild(props);
    }

    /// <inheritdoc/>
    public void Save()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        // OOXML spec §17.6.17: SectionProperties must be the last child of Body.
        // Content added via Add* methods appends after the initially placed SectionProperties,
        // so we re-anchor it here at save time.
        var sectionProps = _body.GetFirstChild<SectionProperties>();
        if (sectionProps != null)
        {
            sectionProps.Remove();
            _body.AppendChild(sectionProps);
        }

        _document.Save();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _document?.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
