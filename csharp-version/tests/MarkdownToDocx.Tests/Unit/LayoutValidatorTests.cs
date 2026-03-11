using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using FluentAssertions;
using MarkdownToDocx.Core.Interfaces;
using MarkdownToDocx.Core.Models;
using MarkdownToDocx.Core.OpenXml;
using MarkdownToDocx.Core.TextDirection;
using MarkdownToDocx.Styling.Models;
using MarkdownToDocx.Styling.TextDirection;
using Xunit;
using W = DocumentFormat.OpenXml.Wordprocessing;

namespace MarkdownToDocx.Tests.Unit;

/// <summary>
/// Tests for the LayoutValidator — structural overflow risk detection.
/// </summary>
public class LayoutValidatorTests : IDisposable
{
    private readonly MemoryStream _stream = new();

    public void Dispose() => _stream.Dispose();

    // ── PageGeometry ─────────────────────────────────────────────────────────

    [Fact]
    public void GetPageGeometry_ShouldReturnCorrectDimensions()
    {
        // Arrange: A5 horizontal, margins 1.59cm left, 1.27cm right
        var provider = BuildProvider(14.8, 21.0, marginLeft: 1.59, marginRight: 1.27);
        using var builder = new OpenXmlDocumentBuilder(_stream, provider);
        builder.AddParagraph(ToRuns("test"), DefaultParagraphStyle());
        builder.Save();

        // Act
        _stream.Position = 0;
        using var doc = WordprocessingDocument.Open(_stream, false);
        var geometry = LayoutValidator.GetPageGeometry(doc);

        // Assert
        geometry.LeftMarginTwips.Should().Be(Round(1.59));
        geometry.RightMarginTwips.Should().Be(Round(1.27));
        geometry.PrintableWidthTwips.Should().Be(Round(14.8) - Round(1.59) - Round(1.27));
    }

    // ── MarginCheck ──────────────────────────────────────────────────────────

    [Fact]
    public void CheckMinimumMargins_WithAdequateMargins_ShouldPass()
    {
        // Arrange: outer margin = 1.27cm = 720 twips (> 567 minimum)
        var geometry = BuildGeometry(leftTwips: 900, rightTwips: 720, mirrorMargins: true);

        // Act
        var result = LayoutValidator.CheckMinimumMargins(geometry);

        // Assert
        result.Passes.Should().BeTrue();
        result.OuterMarginMm.Should().BeApproximately(12.7, 0.5);
    }

    [Fact]
    public void CheckMinimumMargins_WithTooNarrowMargin_ShouldFail()
    {
        // Arrange: outer margin = 0.64cm = 363 twips (< 567 minimum = 10mm)
        var geometry = BuildGeometry(leftTwips: 900, rightTwips: 363, mirrorMargins: true);

        // Act
        var result = LayoutValidator.CheckMinimumMargins(geometry);

        // Assert
        result.Passes.Should().BeFalse();
        result.OuterMarginMm.Should().BeApproximately(6.4, 0.5);
    }

    // ── WordWrap check ───────────────────────────────────────────────────────

    [Fact]
    public void CheckWordWrapOnCodeBlocks_WhenEnabled_ShouldPass()
    {
        // Arrange
        var provider = BuildProvider(14.8, 21.0);
        using var builder = new OpenXmlDocumentBuilder(_stream, provider);
        builder.AddCodeBlock("var x = 42;", null, DefaultCodeBlockStyle(wordWrap: true));
        builder.Save();

        // Act
        _stream.Position = 0;
        using var doc = WordprocessingDocument.Open(_stream, false);
        var result = LayoutValidator.CheckWordWrapOnCodeBlocks(doc);

        // Assert
        result.TotalCodeBlocks.Should().Be(1);
        result.Passes.Should().BeTrue();
        result.MissingWordWrap.Should().Be(0);
    }

    [Fact]
    public void CheckWordWrapOnCodeBlocks_WhenDisabled_ShouldFail()
    {
        // Arrange
        var provider = BuildProvider(14.8, 21.0);
        using var builder = new OpenXmlDocumentBuilder(_stream, provider);
        builder.AddCodeBlock("var x = 42;", null, DefaultCodeBlockStyle(wordWrap: false));
        builder.Save();

        // Act
        _stream.Position = 0;
        using var doc = WordprocessingDocument.Open(_stream, false);
        var result = LayoutValidator.CheckWordWrapOnCodeBlocks(doc);

        // Assert
        result.TotalCodeBlocks.Should().Be(1);
        result.Passes.Should().BeFalse();
        result.MissingWordWrap.Should().Be(1);
    }

    // ── CodeBlock line length ─────────────────────────────────────────────────

    [Fact]
    public void CheckCodeBlockLineWidths_ShortLines_ShouldHaveNoViolations()
    {
        // Arrange: A5 printable width ~119mm, 7pt → max ~134 chars, 10-char line is fine
        var provider = BuildProvider(14.8, 21.0, marginLeft: 1.59, marginRight: 1.27);
        using var builder = new OpenXmlDocumentBuilder(_stream, provider);
        builder.AddCodeBlock("short line", null, DefaultCodeBlockStyle(wordWrap: true));
        builder.Save();

        // Act
        _stream.Position = 0;
        using var doc = WordprocessingDocument.Open(_stream, false);
        var geometry = LayoutValidator.GetPageGeometry(doc);
        var violations = LayoutValidator.CheckCodeBlockLineWidths(doc, geometry);

        // Assert
        violations.Should().BeEmpty();
    }

    [Fact]
    public void CheckCodeBlockLineWidths_VeryLongLine_ShouldReportViolation()
    {
        // Arrange: 200-char line will exceed any reasonable A5 width estimate
        var longLine = new string('x', 200);
        var provider = BuildProvider(14.8, 21.0, marginLeft: 1.59, marginRight: 1.27);
        using var builder = new OpenXmlDocumentBuilder(_stream, provider);
        builder.AddCodeBlock(longLine, null, DefaultCodeBlockStyle(wordWrap: false));
        builder.Save();

        // Act
        _stream.Position = 0;
        using var doc = WordprocessingDocument.Open(_stream, false);
        var geometry = LayoutValidator.GetPageGeometry(doc);
        var violations = LayoutValidator.CheckCodeBlockLineWidths(doc, geometry);

        // Assert
        violations.Should().NotBeEmpty();
        violations[0].CharCount.Should().Be(200);
    }

    // ── Indentation check ─────────────────────────────────────────────────────

    [Fact]
    public void CheckParagraphIndentation_NormalIndent_ShouldHaveNoViolations()
    {
        // Arrange: standard list indent (640 twips) is well within printable width
        var provider = BuildProvider(14.8, 21.0, marginLeft: 1.59, marginRight: 1.27);
        using var builder = new OpenXmlDocumentBuilder(_stream, provider);
        var listStyle = DefaultListStyle();
        builder.AddList(
            new[] { new MarkdownToDocx.Core.Models.ListItem { Text = "item" } },
            false,
            listStyle);
        builder.Save();

        // Act
        _stream.Position = 0;
        using var doc = WordprocessingDocument.Open(_stream, false);
        var geometry = LayoutValidator.GetPageGeometry(doc);
        var violations = LayoutValidator.CheckParagraphIndentation(doc, geometry);

        // Assert
        violations.Should().BeEmpty();
    }

    // ── helpers ───────────────────────────────────────────────────────────────

    private static uint Round(double cm) => (uint)Math.Round(cm * 567.0);

    private static PageGeometry BuildGeometry(
        uint leftTwips, uint rightTwips, bool mirrorMargins = false)
    {
        uint width = 8390; // A5 approx
        return new PageGeometry(
            WidthTwips: width,
            HeightTwips: 11900,
            LeftMarginTwips: leftTwips,
            RightMarginTwips: rightTwips,
            PrintableWidthTwips: width - leftTwips - rightTwips,
            MirrorMargins: mirrorMargins);
    }

    private static ITextDirectionProvider BuildProvider(
        double widthCm, double heightCm,
        double marginLeft = 1.59, double marginRight = 1.27)
    {
        var layout = new PageLayoutConfig
        {
            Width = widthCm,
            Height = heightCm,
            MarginTop = 1.9,
            MarginBottom = 1.9,
            MarginLeft = marginLeft,
            MarginRight = marginRight,
            MirrorMargins = true
        };
        return new ConfigurableTextDirectionProvider(new HorizontalTextProvider(), layout);
    }

    private static InlineRun[] ToRuns(string text)
        => new[] { new InlineRun { Text = text } };

    private static ParagraphStyle DefaultParagraphStyle() => new()
    {
        FontSize = 18,
        Color = "000000",
        LineSpacing = "240",
        FirstLineIndent = "0",
        LeftIndent = "0"
    };

    private static CodeBlockStyle DefaultCodeBlockStyle(bool wordWrap = true) => new()
    {
        FontSize = 14, // 7pt in half-points
        Color = "333333",
        BackgroundColor = "f5f5f5",
        BorderColor = "cccccc",
        MonospaceFontAscii = "Consolas",
        MonospaceFontEastAsia = "Noto Sans Mono CJK JP",
        SpaceBefore = "120",
        SpaceAfter = "120",
        LineSpacing = "230",
        BorderSpace = 4,
        WordWrap = wordWrap
    };

    private static ListStyle DefaultListStyle() => new()
    {
        FontSize = 18,
        Color = "000000",
        LeftIndent = "640",
        HangingIndent = "320",
        SpaceBefore = "40",
        SpaceAfter = "40"
    };
}
