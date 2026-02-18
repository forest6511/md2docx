using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using MarkdownToDocx.Core.Imaging;
using MarkdownToDocx.Core.Interfaces;
using MarkdownToDocx.Core.Models;
using CoreListItem = MarkdownToDocx.Core.Models.ListItem;
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

        InitializeDocument();
        _body = _document.MainDocumentPart?.Document?.Body
            ?? throw new InvalidOperationException("Failed to initialize document structure");
    }

    /// <summary>
    /// Initializes the document structure with main document part and body
    /// </summary>
    private void InitializeDocument()
    {
        var mainPart = _document.AddMainDocumentPart();
        mainPart.Document = new Document();
        var body = mainPart.Document.AppendChild(new Body());

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
    /// Creates a ParagraphBorders element from a comma-separated position string
    /// </summary>
    private static ParagraphBorders CreateBordersFromPositions(
        string borderPosition,
        string borderColor,
        uint borderSize,
        uint borderSpace)
    {
        var positions = borderPosition
            .ToLowerInvariant()
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        var paragraphBorders = new ParagraphBorders();

        foreach (var pos in positions)
        {
            OpenXmlElement border = pos switch
            {
                "left" => new LeftBorder { Val = BorderValues.Single, Color = borderColor, Size = borderSize, Space = borderSpace },
                "right" => new RightBorder { Val = BorderValues.Single, Color = borderColor, Size = borderSize, Space = borderSpace },
                "top" => new TopBorder { Val = BorderValues.Single, Color = borderColor, Size = borderSize, Space = borderSpace },
                _ => new BottomBorder { Val = BorderValues.Single, Color = borderColor, Size = borderSize, Space = borderSpace }
            };
            paragraphBorders.AppendChild(border);
        }

        return paragraphBorders;
    }

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

        props.AppendChild(new FontSize { Val = fontSize.ToString() });
        props.AppendChild(new Color { Val = color });

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
        var drawing = CreateImageDrawing(relationshipId, displayWidthEmu, displayHeightEmu);

        // Create centered paragraph with the image
        var paragraph = _body.AppendChild(new Paragraph());
        var paragraphProps = CreateBaseParagraphProperties();
        paragraphProps.AppendChild(new Justification { Val = JustificationValues.Center });
        paragraph.AppendChild(paragraphProps);

        var run = paragraph.AppendChild(new Run());
        run.AppendChild(drawing);

        // Optional page break after title page
        if (style.PageBreakAfter)
        {
            var breakParagraph = _body.AppendChild(new Paragraph());
            var breakProps = CreateBaseParagraphProperties();
            breakParagraph.AppendChild(breakProps);

            var breakRun = breakParagraph.AppendChild(new Run());
            breakRun.AppendChild(new Break { Type = BreakValues.Page });
        }
    }

    /// <summary>
    /// Creates a Drawing element containing an inline image with the specified dimensions
    /// </summary>
    private static Drawing CreateImageDrawing(string relationshipId, long widthEmu, long heightEmu)
    {
        return new Drawing(
            new DW.Inline(
                new DW.Extent { Cx = widthEmu, Cy = heightEmu },
                new DW.EffectExtent { LeftEdge = 0, TopEdge = 0, RightEdge = 0, BottomEdge = 0 },
                new DW.DocProperties { Id = 1U, Name = "Cover Image" },
                new DW.NonVisualGraphicFrameDrawingProperties(
                    new A.GraphicFrameLocks { NoChangeAspect = true }),
                new A.Graphic(
                    new A.GraphicData(
                        new PIC.Picture(
                            new PIC.NonVisualPictureProperties(
                                new PIC.NonVisualDrawingProperties { Id = 0U, Name = "cover" },
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
        instrRun.AppendChild(new FieldCode($" TOC \\o \"1-{style.Depth}\" \\h \\z \\u ")
        {
            Space = SpaceProcessingModeValues.Preserve
        });

        // Separate field
        var separateRun = tocParagraph.AppendChild(new Run());
        separateRun.AppendChild(new FieldChar { FieldCharType = FieldCharValues.Separate });

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
                style.BorderSpace));

            // Background shading
            if (!string.IsNullOrEmpty(style.BackgroundColor))
            {
                mainProps.AppendChild(CreateBackgroundShading(style.BackgroundColor));
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
                style.BorderSpace));
        }

        // Background shading
        if (!string.IsNullOrEmpty(style.BackgroundColor))
        {
            props.AppendChild(CreateBackgroundShading(style.BackgroundColor));
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
    public void AddParagraph(string text, ParagraphStyle style)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(style);

        var paragraph = _body.AppendChild(new Paragraph());
        var paragraphProps = CreateParagraphProperties(style);
        paragraph.AppendChild(paragraphProps);

        var run = paragraph.AppendChild(new Run());
        run.AppendChild(CreateBaseRunProperties(style.FontSize, style.Color));
        run.AppendChild(new Text(text) { Space = SpaceProcessingModeValues.Preserve });
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
    public void AddList(IEnumerable<CoreListItem> items, bool isOrdered, ListStyle style)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(style);

        int itemNumber = 1;
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
            run.AppendChild(new Text(lines[i]) { Space = SpaceProcessingModeValues.Preserve });
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
            new TopBorder { Val = BorderValues.Single, Color = style.BorderColor, Size = 4, Space = 8 },
            new BottomBorder { Val = BorderValues.Single, Color = style.BorderColor, Size = 4, Space = 8 },
            new LeftBorder { Val = BorderValues.Single, Color = style.BorderColor, Size = 4, Space = 8 },
            new RightBorder { Val = BorderValues.Single, Color = style.BorderColor, Size = 4, Space = 8 }
        ));

        // Background shading
        props.AppendChild(CreateBackgroundShading(style.BackgroundColor));

        // Spacing
        props.AppendChild(new SpacingBetweenLines
        {
            Before = style.SpaceBefore,
            After = style.SpaceAfter,
            Line = style.LineSpacing,
            LineRule = LineSpacingRuleValues.Auto
        });

        return props;
    }

    /// <inheritdoc/>
    public void AddQuote(string text, QuoteStyle style)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(style);

        var paragraph = _body.AppendChild(new Paragraph());
        var paragraphProps = CreateQuoteParagraphProperties(style);
        paragraph.AppendChild(paragraphProps);

        var run = paragraph.AppendChild(new Run());
        run.AppendChild(CreateBaseRunProperties(style.FontSize, style.Color, italic: style.Italic));
        run.AppendChild(new Text(text) { Space = SpaceProcessingModeValues.Preserve });
    }

    /// <summary>
    /// Creates paragraph properties for quote blocks
    /// </summary>
    private ParagraphProperties CreateQuoteParagraphProperties(QuoteStyle style)
    {
        var props = CreateBaseParagraphProperties();

        // Border
        if (style.ShowBorder)
        {
            props.AppendChild(CreateBordersFromPositions(
                style.BorderPosition,
                style.BorderColor,
                style.BorderSize,
                style.BorderSpace));
        }

        // Background shading
        if (!string.IsNullOrEmpty(style.BackgroundColor))
        {
            props.AppendChild(CreateBackgroundShading(style.BackgroundColor));
        }

        // Indentation
        props.AppendChild(new Indentation { Left = style.LeftIndent });

        // Spacing
        props.AppendChild(new SpacingBetweenLines
        {
            Before = style.SpaceBefore,
            After = style.SpaceAfter
        });

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
    }
}
