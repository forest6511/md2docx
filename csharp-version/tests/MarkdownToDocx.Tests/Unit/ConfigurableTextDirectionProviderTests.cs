using DocumentFormat.OpenXml.Wordprocessing;
using FluentAssertions;
using MarkdownToDocx.Styling.TextDirection;
using MarkdownToDocx.Core.Models;
using MarkdownToDocx.Core.TextDirection;
using MarkdownToDocx.Styling.Models;
using Xunit;

namespace MarkdownToDocx.Tests.Unit;

/// <summary>
/// Unit tests for ConfigurableTextDirectionProvider
/// </summary>
public class ConfigurableTextDirectionProviderTests
{
    private static PageLayoutConfig LayoutWith(double width, double height,
        double marginTop = 1.9, double marginBottom = 1.9,
        double marginLeft = 1.5, double marginRight = 0.6,
        bool mirrorMargins = false) => new()
        {
            Width = width,
            Height = height,
            MarginTop = marginTop,
            MarginBottom = marginBottom,
            MarginLeft = marginLeft,
            MarginRight = marginRight,
            MirrorMargins = mirrorMargins,
        };

    [Fact]
    public void GetPageConfiguration_WhenWidthAndHeightSet_OverridesDefaults()
    {
        // Arrange: A5 (14.8 × 21.0 cm)
        var provider = new ConfigurableTextDirectionProvider(
            new HorizontalTextProvider(),
            LayoutWith(14.8, 21.0));

        // Act
        var config = provider.GetPageConfiguration();

        // Assert: 14.8cm × 567 = 8392, 21.0cm × 567 = 11907
        config.Width.Value.Should().Be((uint)Math.Round(14.8 * 567.0));
        config.Height.Value.Should().Be((uint)Math.Round(21.0 * 567.0));
        config.Orientation.Should().Be(PageOrientationValues.Portrait);
    }

    [Fact]
    public void GetPageConfiguration_WhenWidthAndHeightZero_ReturnsBaseDefaults()
    {
        // Arrange: no page size in YAML
        var provider = new ConfigurableTextDirectionProvider(
            new HorizontalTextProvider(),
            new PageLayoutConfig());

        // Act
        var config = provider.GetPageConfiguration();

        // Assert: falls back to HorizontalTextProvider defaults
        config.Width.Value.Should().Be(8646U);
        config.Height.Value.Should().Be(12950U);
    }

    [Fact]
    public void GetPageConfiguration_LandscapeWhenWidthExceedsHeight()
    {
        // Arrange: landscape (width > height)
        var provider = new ConfigurableTextDirectionProvider(
            new HorizontalTextProvider(),
            LayoutWith(29.7, 21.0));

        // Act
        var config = provider.GetPageConfiguration();

        // Assert
        config.Orientation.Should().Be(PageOrientationValues.Landscape);
    }

    [Fact]
    public void GetPageConfiguration_AppliesAllMarginsFromYaml()
    {
        // Arrange
        var provider = new ConfigurableTextDirectionProvider(
            new HorizontalTextProvider(),
            LayoutWith(15.24, 22.86, marginTop: 1.91, marginBottom: 1.91,
                       marginLeft: 1.59, marginRight: 0.64));

        // Act
        var config = provider.GetPageConfiguration();

        // Assert (1cm = 567 twips)
        config.TopMargin.Should().Be((int)Math.Round(1.91 * 567.0));
        config.BottomMargin.Should().Be((int)Math.Round(1.91 * 567.0));
        config.LeftMargin.Should().Be((int)Math.Round(1.59 * 567.0));
        config.RightMargin.Should().Be((int)Math.Round(0.64 * 567.0));
    }

    [Fact]
    public void GetPageConfiguration_MirrorMarginsTrue_SetInPageConfiguration()
    {
        // Arrange
        var provider = new ConfigurableTextDirectionProvider(
            new HorizontalTextProvider(),
            LayoutWith(14.8, 21.0, mirrorMargins: true));

        // Act
        var config = provider.GetPageConfiguration();

        // Assert
        config.MirrorMargins.Should().BeTrue();
    }

    [Fact]
    public void GetPageConfiguration_MirrorMarginsFalse_NotSetInPageConfiguration()
    {
        // Arrange
        var provider = new ConfigurableTextDirectionProvider(
            new HorizontalTextProvider(),
            LayoutWith(14.8, 21.0, mirrorMargins: false));

        // Act
        var config = provider.GetPageConfiguration();

        // Assert
        config.MirrorMargins.Should().BeFalse();
    }

    [Fact]
    public void GetParagraphConfiguration_DelegatesToInner()
    {
        // Arrange
        var inner = new HorizontalTextProvider();
        var provider = new ConfigurableTextDirectionProvider(inner, new PageLayoutConfig());

        // Act
        var config = provider.GetParagraphConfiguration();

        // Assert: same as HorizontalTextProvider
        config.TextDirection.Should().Be(TextDirectionValues.LefToRightTopToBottom);
        config.Kinsoku.Should().BeFalse();
    }

    [Fact]
    public void Mode_DelegatesToInner()
    {
        // Arrange
        var provider = new ConfigurableTextDirectionProvider(
            new HorizontalTextProvider(), new PageLayoutConfig());

        // Act & Assert
        provider.Mode.Should().Be(TextDirectionMode.Horizontal);
    }

    [Fact]
    public void Mode_VerticalProvider_ReturnsVertical()
    {
        // Arrange
        var provider = new ConfigurableTextDirectionProvider(
            new VerticalTextProvider(), new PageLayoutConfig());

        // Act & Assert
        provider.Mode.Should().Be(TextDirectionMode.Vertical);
    }
}
