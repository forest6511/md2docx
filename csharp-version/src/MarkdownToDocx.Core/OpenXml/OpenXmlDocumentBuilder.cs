using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using MarkdownToDocx.Core.Interfaces;
using MarkdownToDocx.Core.Models;
using CoreListItem = MarkdownToDocx.Core.Models.ListItem;
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
        var paragraphConfig = _textDirection.GetParagraphConfiguration();

        var sectionProps = new SectionProperties();

        // Text direction
        sectionProps.AppendChild(new W.TextDirection { Val = paragraphConfig.TextDirection });

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

    /// <inheritdoc/>
    public void AddHeading(int level, string text, HeadingStyle style)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(style);

        if (level < 1 || level > 6)
        {
            throw new ArgumentOutOfRangeException(nameof(level), "Heading level must be between 1 and 6");
        }

        var paragraph = _body.AppendChild(new Paragraph());
        var paragraphProps = CreateHeadingParagraphProperties(level, style);
        paragraph.AppendChild(paragraphProps);

        var run = paragraph.AppendChild(new Run());
        var runProps = CreateHeadingRunProperties(style);
        run.AppendChild(runProps);
        run.AppendChild(new Text(text) { Space = SpaceProcessingModeValues.Preserve });
    }

    /// <summary>
    /// Creates paragraph properties for heading elements
    /// </summary>
    private ParagraphProperties CreateHeadingParagraphProperties(int level, HeadingStyle style)
    {
        var paragraphConfig = _textDirection.GetParagraphConfiguration();
        var props = new ParagraphProperties();

        // Text direction
        props.AppendChild(new W.TextDirection { Val = paragraphConfig.TextDirection });

        // Kinsoku (Japanese line breaking)
        if (paragraphConfig.Kinsoku)
        {
            props.AppendChild(new Kinsoku { Val = OnOffValue.FromBoolean(true) });
        }

        // Border (for level 1 headings or if specified)
        if (level == 1 && style.ShowBorder)
        {
            props.AppendChild(new ParagraphBorders(
                new LeftBorder
                {
                    Val = BorderValues.Single,
                    Color = style.BorderColor ?? "3498db",
                    Size = style.BorderSize,
                    Space = 10U
                }
            ));
        }

        // Spacing
        props.AppendChild(new SpacingBetweenLines
        {
            Before = style.SpaceBefore,
            After = style.SpaceAfter
        });

        return props;
    }

    /// <summary>
    /// Creates run properties for heading text
    /// </summary>
    private static RunProperties CreateHeadingRunProperties(HeadingStyle style)
    {
        var props = new RunProperties();

        if (style.Bold)
        {
            props.AppendChild(new Bold());
        }

        props.AppendChild(new FontSize { Val = style.FontSize.ToString() });
        props.AppendChild(new Color { Val = style.Color });

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
        var runProps = CreateParagraphRunProperties(style);
        run.AppendChild(runProps);
        run.AppendChild(new Text(text) { Space = SpaceProcessingModeValues.Preserve });
    }

    /// <summary>
    /// Creates paragraph properties for standard paragraphs
    /// </summary>
    private ParagraphProperties CreateParagraphProperties(ParagraphStyle style)
    {
        var paragraphConfig = _textDirection.GetParagraphConfiguration();
        var props = new ParagraphProperties();

        // Text direction
        props.AppendChild(new W.TextDirection { Val = paragraphConfig.TextDirection });

        // Kinsoku
        if (paragraphConfig.Kinsoku)
        {
            props.AppendChild(new Kinsoku { Val = OnOffValue.FromBoolean(true) });
        }

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

    /// <summary>
    /// Creates run properties for paragraph text
    /// </summary>
    private static RunProperties CreateParagraphRunProperties(ParagraphStyle style)
    {
        var props = new RunProperties();
        props.AppendChild(new FontSize { Val = style.FontSize.ToString() });
        props.AppendChild(new Color { Val = style.Color });
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
            var runProps = CreateListRunProperties(style);
            run.AppendChild(runProps);

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
        var paragraphConfig = _textDirection.GetParagraphConfiguration();
        var props = new ParagraphProperties();

        // Text direction
        props.AppendChild(new W.TextDirection { Val = paragraphConfig.TextDirection });

        // Kinsoku
        if (paragraphConfig.Kinsoku)
        {
            props.AppendChild(new Kinsoku { Val = OnOffValue.FromBoolean(true) });
        }

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

    /// <summary>
    /// Creates run properties for list item text
    /// </summary>
    private static RunProperties CreateListRunProperties(ListStyle style)
    {
        var props = new RunProperties();
        props.AppendChild(new FontSize { Val = style.FontSize.ToString() });
        props.AppendChild(new Color { Val = style.Color });
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
        var runProps = CreateCodeBlockRunProperties(style);
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
        var paragraphConfig = _textDirection.GetParagraphConfiguration();
        var props = new ParagraphProperties();

        // Text direction
        props.AppendChild(new W.TextDirection { Val = paragraphConfig.TextDirection });

        // Kinsoku
        if (paragraphConfig.Kinsoku)
        {
            props.AppendChild(new Kinsoku { Val = OnOffValue.FromBoolean(true) });
        }

        // Borders
        props.AppendChild(new ParagraphBorders(
            new TopBorder { Val = BorderValues.Single, Color = style.BorderColor, Size = 4, Space = 8 },
            new BottomBorder { Val = BorderValues.Single, Color = style.BorderColor, Size = 4, Space = 8 },
            new LeftBorder { Val = BorderValues.Single, Color = style.BorderColor, Size = 4, Space = 8 },
            new RightBorder { Val = BorderValues.Single, Color = style.BorderColor, Size = 4, Space = 8 }
        ));

        // Background shading
        props.AppendChild(new Shading
        {
            Val = ShadingPatternValues.Clear,
            Color = "auto",
            Fill = style.BackgroundColor
        });

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

    /// <summary>
    /// Creates run properties for code block text
    /// </summary>
    private static RunProperties CreateCodeBlockRunProperties(CodeBlockStyle style)
    {
        var props = new RunProperties();
        props.AppendChild(new RunFonts
        {
            Ascii = style.MonospaceFontAscii,
            EastAsia = style.MonospaceFontEastAsia
        });
        props.AppendChild(new FontSize { Val = style.FontSize.ToString() });
        props.AppendChild(new Color { Val = style.Color });
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
        var runProps = CreateQuoteRunProperties(style);
        run.AppendChild(runProps);
        run.AppendChild(new Text(text) { Space = SpaceProcessingModeValues.Preserve });
    }

    /// <summary>
    /// Creates paragraph properties for quote blocks
    /// </summary>
    private ParagraphProperties CreateQuoteParagraphProperties(QuoteStyle style)
    {
        var paragraphConfig = _textDirection.GetParagraphConfiguration();
        var props = new ParagraphProperties();

        // Text direction
        props.AppendChild(new W.TextDirection { Val = paragraphConfig.TextDirection });

        // Kinsoku
        if (paragraphConfig.Kinsoku)
        {
            props.AppendChild(new Kinsoku { Val = OnOffValue.FromBoolean(true) });
        }

        // Border (if enabled)
        if (style.ShowBorder)
        {
            OpenXmlElement border = style.BorderPosition.ToLowerInvariant() switch
            {
                "right" => new RightBorder { Val = BorderValues.Single, Color = style.BorderColor, Size = style.BorderSize, Space = 10U },
                "top" => new TopBorder { Val = BorderValues.Single, Color = style.BorderColor, Size = style.BorderSize, Space = 10U },
                "bottom" => new BottomBorder { Val = BorderValues.Single, Color = style.BorderColor, Size = style.BorderSize, Space = 10U },
                _ => new LeftBorder { Val = BorderValues.Single, Color = style.BorderColor, Size = style.BorderSize, Space = 10U }
            };
            props.AppendChild(new ParagraphBorders(border));
        }

        // Background (if specified)
        if (!string.IsNullOrEmpty(style.BackgroundColor))
        {
            props.AppendChild(new Shading
            {
                Val = ShadingPatternValues.Clear,
                Fill = style.BackgroundColor
            });
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

    /// <summary>
    /// Creates run properties for quote text
    /// </summary>
    private static RunProperties CreateQuoteRunProperties(QuoteStyle style)
    {
        var props = new RunProperties();

        if (style.Italic)
        {
            props.AppendChild(new Italic());
        }

        props.AppendChild(new FontSize { Val = style.FontSize.ToString() });
        props.AppendChild(new Color { Val = style.Color });
        return props;
    }

    /// <inheritdoc/>
    public void AddThematicBreak()
    {
        var paragraph = _body.AppendChild(new Paragraph());
        var paragraphConfig = _textDirection.GetParagraphConfiguration();

        var props = new ParagraphProperties();
        props.AppendChild(new W.TextDirection { Val = paragraphConfig.TextDirection });
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
