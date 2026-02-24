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
            6 => config.H6,
            _ => throw new ArgumentOutOfRangeException(nameof(level))
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
            BorderExtent = headingConfig.BorderExtent
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
            LeftIndent = config.Paragraph.LeftIndent
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
            BorderSpace = config.CodeBlock.BorderSpace
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
            SpaceAfter = config.Quote.SpaceAfter
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
        if (!Path.IsPathRooted(imagePath))
        {
            var inputDirectory = Path.GetDirectoryName(Path.GetFullPath(inputFilePath));
            if (inputDirectory != null)
            {
                imagePath = Path.GetFullPath(Path.Combine(inputDirectory, imagePath));
            }
        }

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
