using DocumentFormat.OpenXml.Wordprocessing;
using FluentAssertions;
using MarkdownToDocx.Core.Models;
using MarkdownToDocx.Core.TextDirection;
using Xunit;

namespace MarkdownToDocx.Tests.Unit;

/// <summary>
/// Unit tests for TextDirectionProvider implementations
/// </summary>
public class TextDirectionProviderTests
{
    [Fact]
    public void VerticalTextProvider_Mode_ShouldBeVertical()
    {
        // Arrange
        var provider = new VerticalTextProvider();

        // Act & Assert
        provider.Mode.Should().Be(TextDirectionMode.Vertical);
    }

    [Fact]
    public void VerticalTextProvider_PageConfiguration_ShouldHaveLandscapeOrientation()
    {
        // Arrange
        var provider = new VerticalTextProvider();

        // Act
        var config = provider.GetPageConfiguration();

        // Assert
        config.Orientation.Should().Be(PageOrientationValues.Landscape);
        config.Width.Value.Should().Be(12950U);  // 22.86cm
        config.Height.Value.Should().Be(8646U);  // 15.24cm
    }

    [Fact]
    public void VerticalTextProvider_ParagraphConfiguration_ShouldHaveTopToBottomRightToLeft()
    {
        // Arrange
        var provider = new VerticalTextProvider();

        // Act
        var config = provider.GetParagraphConfiguration();

        // Assert
        config.TextDirection.Should().Be(TextDirectionValues.TopToBottomRightToLeft);
        config.Kinsoku.Should().BeTrue();
    }

    [Fact]
    public void HorizontalTextProvider_Mode_ShouldBeHorizontal()
    {
        // Arrange
        var provider = new HorizontalTextProvider();

        // Act & Assert
        provider.Mode.Should().Be(TextDirectionMode.Horizontal);
    }

    [Fact]
    public void HorizontalTextProvider_PageConfiguration_ShouldHavePortraitOrientation()
    {
        // Arrange
        var provider = new HorizontalTextProvider();

        // Act
        var config = provider.GetPageConfiguration();

        // Assert
        config.Orientation.Should().Be(PageOrientationValues.Portrait);
        config.Width.Value.Should().Be(8646U);   // 15.24cm
        config.Height.Value.Should().Be(12950U); // 22.86cm
    }

    [Fact]
    public void HorizontalTextProvider_ParagraphConfiguration_ShouldHaveLeftToRightTopToBottom()
    {
        // Arrange
        var provider = new HorizontalTextProvider();

        // Act
        var config = provider.GetParagraphConfiguration();

        // Assert
        config.TextDirection.Should().Be(TextDirectionValues.LefToRightTopToBottom);
        config.Kinsoku.Should().BeFalse();
    }

    [Fact]
    public void VerticalTextProvider_PageMargins_ShouldBeConfiguredForVerticalLayout()
    {
        // Arrange
        var provider = new VerticalTextProvider();

        // Act
        var config = provider.GetPageConfiguration();

        // Assert
        config.TopMargin.Should().Be(1134);     // 2cm
        config.BottomMargin.Should().Be(1134);  // 2cm
        config.LeftMargin.Should().Be(1417);    // 2.5cm
        config.RightMargin.Should().Be(1417);   // 2.5cm
    }

    [Fact]
    public void HorizontalTextProvider_PageMargins_ShouldBeStandard()
    {
        // Arrange
        var provider = new HorizontalTextProvider();

        // Act
        var config = provider.GetPageConfiguration();

        // Assert
        config.TopMargin.Should().Be(1134);     // 2cm
        config.BottomMargin.Should().Be(1134);  // 2cm
        config.LeftMargin.Should().Be(1417);    // 2.5cm
        config.RightMargin.Should().Be(1417);   // 2.5cm
    }
}
