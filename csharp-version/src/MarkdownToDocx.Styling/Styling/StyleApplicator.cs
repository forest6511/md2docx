using MarkdownToDocx.Core.Models;
using MarkdownToDocx.Styling.Interfaces;
using MarkdownToDocx.Styling.Models;

namespace MarkdownToDocx.Styling.Styling;

/// <summary>
/// Applies style configuration to document elements
/// Converts YAML configuration models to OpenXML style objects
/// </summary>
public sealed class StyleApplicator : IStyleApplicator
{
    /// <inheritdoc/>
    public HeadingStyle ApplyHeadingStyle(int level, StyleConfiguration config)
    {
        ArgumentNullException.ThrowIfNull(config);

        if (level < 1 || level > 6)
        {
            throw new ArgumentOutOfRangeException(nameof(level), "Heading level must be between 1 and 6");
        }

        var headingConfig = level switch
        {
            1 => config.H1,
            2 => config.H2,
            3 => config.H3,
            4 => config.H4,
            5 => config.H5,
            _ => config.H6  // level == 6, guaranteed by guard clause above
        };

        return new HeadingStyle
        {
            FontSize = headingConfig.Size * 2, // Convert pt to half-points
            Bold = headingConfig.Bold,
            Color = headingConfig.Color,
            ShowBorder = headingConfig.ShowBorder,
            BorderColor = headingConfig.BorderColor,
            BorderSize = headingConfig.BorderSize,
            BorderSpace = headingConfig.BorderSpace,
            BorderPosition = headingConfig.BorderPosition,
            BackgroundColor = headingConfig.BackgroundColor,
            LineSpacing = headingConfig.LineSpacing,
            PageBreakBefore = headingConfig.PageBreakBefore,
            SpaceBefore = headingConfig.SpaceBefore,
            SpaceAfter = headingConfig.SpaceAfter,
            BorderExtent = headingConfig.BorderExtent,
            LeftIndent = headingConfig.LeftIndent,
            SuppressPageBreakIfPrevHeadingLevel = headingConfig.SuppressPageBreakIfPrevHeadingLevel,
            BorderStyle = headingConfig.BorderStyle,
            IconPrefix = headingConfig.IconPrefix,
            IconPrefixColor = headingConfig.IconPrefixColor
        };
    }

    /// <inheritdoc/>
    public ParagraphStyle ApplyParagraphStyle(StyleConfiguration config)
    {
        ArgumentNullException.ThrowIfNull(config);

        return new ParagraphStyle
        {
            FontSize = config.Paragraph.Size * 2, // Convert pt to half-points
            Color = config.Paragraph.Color,
            LineSpacing = config.Paragraph.LineSpacing,
            FirstLineIndent = config.Paragraph.FirstLineIndent,
            LeftIndent = config.Paragraph.LeftIndent,
            InlineCodeFontAscii = config.Paragraph.InlineCodeFontAscii,
            InlineCodeFontEastAsia = config.Paragraph.InlineCodeFontEastAsia
        };
    }

    /// <inheritdoc/>
    public ListStyle ApplyListStyle(StyleConfiguration config)
    {
        ArgumentNullException.ThrowIfNull(config);

        return new ListStyle
        {
            FontSize = config.List.Size * 2, // Convert pt to half-points
            Color = config.List.Color,
            LeftIndent = config.List.LeftIndent,
            HangingIndent = config.List.HangingIndent,
            SpaceBefore = config.List.SpaceBefore,
            SpaceAfter = config.List.SpaceAfter
        };
    }

    /// <inheritdoc/>
    public CodeBlockStyle ApplyCodeBlockStyle(StyleConfiguration config)
    {
        ArgumentNullException.ThrowIfNull(config);

        return new CodeBlockStyle
        {
            FontSize = config.CodeBlock.Size * 2, // Convert pt to half-points
            Color = config.CodeBlock.Color,
            BackgroundColor = config.CodeBlock.BackgroundColor,
            BorderColor = config.CodeBlock.BorderColor,
            MonospaceFontAscii = config.CodeBlock.MonospaceFontAscii,
            MonospaceFontEastAsia = config.CodeBlock.MonospaceFontEastAsia,
            LineSpacing = config.CodeBlock.LineSpacing,
            SpaceBefore = config.CodeBlock.SpaceBefore,
            SpaceAfter = config.CodeBlock.SpaceAfter,
            BorderSpace = config.CodeBlock.BorderSpace,
            WordWrap = config.CodeBlock.WordWrap
        };
    }

    /// <inheritdoc/>
    public QuoteStyle ApplyQuoteStyle(StyleConfiguration config)
    {
        ArgumentNullException.ThrowIfNull(config);

        return new QuoteStyle
        {
            FontSize = config.Quote.Size * 2, // Convert pt to half-points
            Color = config.Quote.Color,
            Italic = config.Quote.Italic,
            ShowBorder = config.Quote.ShowBorder,
            BorderColor = config.Quote.BorderColor,
            BorderSize = config.Quote.BorderSize,
            BorderSpace = config.Quote.BorderSpace,
            BorderPosition = config.Quote.BorderPosition,
            BackgroundColor = config.Quote.BackgroundColor,
            LeftIndent = config.Quote.LeftIndent,
            SpaceBefore = config.Quote.SpaceBefore,
            SpaceAfter = config.Quote.SpaceAfter,
            PaddingSpace = config.Quote.PaddingSpace,
            InlineCodeFontAscii = config.Quote.InlineCodeFontAscii,
            InlineCodeFontEastAsia = config.Quote.InlineCodeFontEastAsia
        };
    }

    /// <inheritdoc/>
    public TableStyle ApplyTableStyle(StyleConfiguration config)
    {
        ArgumentNullException.ThrowIfNull(config);

        return new TableStyle
        {
            FontSize = config.Table.Size * 2, // Convert pt to half-points
            HeaderBackgroundColor = config.Table.HeaderBackgroundColor,
            HeaderTextColor = config.Table.HeaderTextColor,
            BodyTextColor = config.Table.BodyTextColor,
            BorderColor = config.Table.BorderColor,
            BorderSize = config.Table.BorderSize,
            HeaderBold = config.Table.HeaderBold,
            CellPaddingTop = config.Table.CellPaddingTop,
            CellPaddingBottom = config.Table.CellPaddingBottom,
            CellPaddingLeft = config.Table.CellPaddingLeft,
            CellPaddingRight = config.Table.CellPaddingRight,
            SpaceBefore = config.Table.SpaceBefore,
            SpaceAfter = config.Table.SpaceAfter,
            WidthPercent = Math.Clamp(config.Table.WidthPercent, 1, 100)
        };
    }

    /// <inheritdoc/>
    public ImageStyle ApplyImageStyle(StyleConfiguration config)
    {
        ArgumentNullException.ThrowIfNull(config);

        return new ImageStyle
        {
            MaxWidthPercent = Math.Clamp(config.Image.MaxWidthPercent, 1, 100),
            Alignment = config.Image.Alignment
        };
    }

    /// <inheritdoc/>
    public TableOfContentsStyle ApplyTableOfContentsStyle(ConversionConfiguration config)
    {
        ArgumentNullException.ThrowIfNull(config);

        var tocConfig = config.TableOfContents;
        var depth = Math.Clamp(tocConfig.Depth, 1, 6);

        return new TableOfContentsStyle
        {
            Enabled = tocConfig.Enabled,
            Depth = depth,
            Title = tocConfig.Title,
            PageBreakAfter = tocConfig.PageBreakAfter
        };
    }

    /// <inheritdoc/>
    public FencedDivStyle ApplyFencedDivStyle(FencedDivClassConfig divConfig, StyleConfiguration styles)
    {
        ArgumentNullException.ThrowIfNull(divConfig);
        ArgumentNullException.ThrowIfNull(styles);

        var para = styles.Paragraph;
        return new FencedDivStyle
        {
            BackgroundColor = divConfig.BackgroundColor,
            BorderTopColor = divConfig.BorderTopColor,
            BorderTopSize = divConfig.BorderTopSize,
            BorderBottomColor = divConfig.BorderBottomColor,
            BorderBottomSize = divConfig.BorderBottomSize,
            BorderSpace = divConfig.BorderSpace,
            SpaceBefore = divConfig.SpaceBefore,
            SpaceAfter = divConfig.SpaceAfter,
            LeftIndent = divConfig.LeftIndent,
            FontSize = para.Size * 2, // Convert pt to half-points
            Color = para.Color,
            LineSpacing = para.LineSpacing,
            InlineCodeFontAscii = para.InlineCodeFontAscii,
            InlineCodeFontEastAsia = para.InlineCodeFontEastAsia
        };
    }

    /// <summary>
    /// Resolves a potentially relative path against the directory of a base file.
    /// Absolute paths are returned unchanged.
    /// </summary>
    private static string ResolveRelativePath(string path, string basePath)
    {
        if (Path.IsPathRooted(path)) return path;
        var baseDir = Path.GetDirectoryName(Path.GetFullPath(basePath));
        return baseDir != null ? Path.GetFullPath(Path.Combine(baseDir, path)) : path;
    }

    /// <inheritdoc/>
    public TitlePageStyle ApplyTitlePageStyle(ConversionConfiguration config, string inputFilePath, string? coverImageOverride = null)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(inputFilePath);

        var titlePageConfig = config.TitlePage;

        // CLI override takes precedence and implicitly enables title page
        string? imagePath = coverImageOverride ?? titlePageConfig.ImagePath;
        bool enabled = coverImageOverride != null || titlePageConfig.Enabled;

        if (!enabled || string.IsNullOrEmpty(imagePath))
        {
            return new TitlePageStyle { Enabled = false };
        }

        // Resolve relative path against input file directory
        imagePath = ResolveRelativePath(imagePath, inputFilePath);

        return new TitlePageStyle
        {
            Enabled = true,
            ImagePath = imagePath,
            ImageMaxWidthPercent = Math.Clamp(titlePageConfig.ImageMaxWidthPercent, 1, 100),
            ImageMaxHeightPercent = Math.Clamp(titlePageConfig.ImageMaxHeightPercent, 1, 100),
            PageBreakAfter = titlePageConfig.PageBreakAfter
        };
    }
}
