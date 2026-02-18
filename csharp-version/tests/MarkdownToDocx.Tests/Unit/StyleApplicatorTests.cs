using FluentAssertions;
using MarkdownToDocx.Styling.Models;
using MarkdownToDocx.Styling.Styling;
using Xunit;

namespace MarkdownToDocx.Tests.Unit;

/// <summary>
/// Unit tests for StyleApplicator
/// </summary>
public class StyleApplicatorTests
{
    private readonly StyleApplicator _applicator;
    private readonly StyleConfiguration _testConfig;

    public StyleApplicatorTests()
    {
        _applicator = new StyleApplicator();
        _testConfig = CreateTestConfiguration();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    public void ApplyHeadingStyle_WithValidLevel_ShouldReturnCorrectStyle(int level)
    {
        // Act
        var style = _applicator.ApplyHeadingStyle(level, _testConfig);

        // Assert
        style.Should().NotBeNull();
        style.FontSize.Should().BeGreaterThan(0);
        style.Color.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void ApplyHeadingStyle_WithLevel1_ShouldConvertSizeToHalfPoints()
    {
        // Arrange
        const int level = 1;
        const int expectedHalfPoints = 24 * 2; // 24pt = 48 half-points

        // Act
        var style = _applicator.ApplyHeadingStyle(level, _testConfig);

        // Assert
        style.FontSize.Should().Be(expectedHalfPoints);
    }

    [Fact]
    public void ApplyHeadingStyle_WithLevel1_ShouldApplyCorrectProperties()
    {
        // Arrange
        const int level = 1;

        // Act
        var style = _applicator.ApplyHeadingStyle(level, _testConfig);

        // Assert
        style.Bold.Should().BeTrue();
        style.Color.Should().Be("000000");
        style.ShowBorder.Should().BeFalse();
        style.SpaceBefore.Should().Be("480");
        style.SpaceAfter.Should().Be("240");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(7)]
    [InlineData(-1)]
    public void ApplyHeadingStyle_WithInvalidLevel_ShouldThrowArgumentOutOfRangeException(int level)
    {
        // Act
        Action act = () => _applicator.ApplyHeadingStyle(level, _testConfig);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("level");
    }

    [Fact]
    public void ApplyHeadingStyle_WithNullConfig_ShouldThrowArgumentNullException()
    {
        // Arrange
        const int level = 1;

        // Act
        Action act = () => _applicator.ApplyHeadingStyle(level, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("config");
    }

    [Fact]
    public void ApplyHeadingStyle_ShouldMapBorderPositionAndBackgroundColor()
    {
        // Arrange
        var config = new StyleConfiguration
        {
            H2 = new HeadingStyleConfig
            {
                Size = 13,
                Bold = true,
                Color = "333333",
                ShowBorder = true,
                BorderColor = "e8a735",
                BorderSize = 16,
                BorderPosition = "left",
                BackgroundColor = "faf5eb"
            }
        };

        // Act
        var style = _applicator.ApplyHeadingStyle(2, config);

        // Assert
        style.BorderPosition.Should().Be("left");
        style.BackgroundColor.Should().Be("faf5eb");
        style.ShowBorder.Should().BeTrue();
    }

    [Fact]
    public void ApplyHeadingStyle_WithDefaultBorderPosition_ShouldBeBottom()
    {
        // Arrange - H1 in test config has no explicit BorderPosition
        // Act
        var style = _applicator.ApplyHeadingStyle(1, _testConfig);

        // Assert
        style.BorderPosition.Should().Be("bottom");
        style.BackgroundColor.Should().BeNull();
    }

    [Fact]
    public void ApplyHeadingStyle_WithPageBreakBefore_ShouldMapCorrectly()
    {
        // Arrange
        var config = new StyleConfiguration
        {
            H1 = new HeadingStyleConfig
            {
                Size = 24,
                Bold = true,
                Color = "000000",
                PageBreakBefore = true,
                SpaceBefore = "480",
                SpaceAfter = "240"
            }
        };

        // Act
        var style = _applicator.ApplyHeadingStyle(1, config);

        // Assert
        style.PageBreakBefore.Should().BeTrue();
    }

    [Fact]
    public void ApplyHeadingStyle_WithDefaultPageBreakBefore_ShouldBeFalse()
    {
        // Act
        var style = _applicator.ApplyHeadingStyle(1, _testConfig);

        // Assert
        style.PageBreakBefore.Should().BeFalse();
    }

    [Fact]
    public void ApplyParagraphStyle_ShouldReturnCorrectStyle()
    {
        // Act
        var style = _applicator.ApplyParagraphStyle(_testConfig);

        // Assert
        style.Should().NotBeNull();
        style.FontSize.Should().Be(11 * 2); // 11pt = 22 half-points
        style.Color.Should().Be("000000");
        style.LineSpacing.Should().Be("360");
        style.FirstLineIndent.Should().Be("0");
        style.LeftIndent.Should().Be("0");
    }

    [Fact]
    public void ApplyParagraphStyle_WithNullConfig_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => _applicator.ApplyParagraphStyle(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("config");
    }

    [Fact]
    public void ApplyListStyle_ShouldReturnCorrectStyle()
    {
        // Act
        var style = _applicator.ApplyListStyle(_testConfig);

        // Assert
        style.Should().NotBeNull();
        style.FontSize.Should().Be(11 * 2);
        style.Color.Should().Be("000000");
        style.LeftIndent.Should().Be("720");
        style.HangingIndent.Should().Be("360");
    }

    [Fact]
    public void ApplyCodeBlockStyle_ShouldReturnCorrectStyle()
    {
        // Act
        var style = _applicator.ApplyCodeBlockStyle(_testConfig);

        // Assert
        style.Should().NotBeNull();
        style.FontSize.Should().Be(10 * 2);
        style.Color.Should().Be("333333");
        style.BackgroundColor.Should().Be("f5f5f5");
        style.BorderColor.Should().Be("cccccc");
        style.MonospaceFontAscii.Should().Be("Noto Sans Mono");
        style.MonospaceFontEastAsia.Should().Be("Noto Sans Mono CJK JP");
    }

    [Fact]
    public void ApplyQuoteStyle_ShouldReturnCorrectStyle()
    {
        // Act
        var style = _applicator.ApplyQuoteStyle(_testConfig);

        // Assert
        style.Should().NotBeNull();
        style.FontSize.Should().Be(11 * 2);
        style.Color.Should().Be("555555");
        style.Italic.Should().BeTrue();
        style.ShowBorder.Should().BeTrue();
        style.BorderColor.Should().Be("999999");
        style.BorderPosition.Should().Be("left");
        style.LeftIndent.Should().Be("720");
    }

    private static StyleConfiguration CreateTestConfiguration()
    {
        return new StyleConfiguration
        {
            H1 = new HeadingStyleConfig
            {
                Size = 24,
                Bold = true,
                Color = "000000",
                ShowBorder = false,
                SpaceBefore = "480",
                SpaceAfter = "240"
            },
            H2 = new HeadingStyleConfig
            {
                Size = 20,
                Bold = true,
                Color = "000000"
            },
            H3 = new HeadingStyleConfig
            {
                Size = 16,
                Bold = true,
                Color = "000000"
            },
            H4 = new HeadingStyleConfig
            {
                Size = 14,
                Bold = true,
                Color = "000000"
            },
            H5 = new HeadingStyleConfig
            {
                Size = 12,
                Bold = true,
                Color = "000000"
            },
            H6 = new HeadingStyleConfig
            {
                Size = 11,
                Bold = true,
                Color = "000000"
            },
            Paragraph = new ParagraphStyleConfig
            {
                Size = 11,
                Color = "000000",
                LineSpacing = "360",
                FirstLineIndent = "0",
                LeftIndent = "0"
            },
            List = new ListStyleConfig
            {
                Size = 11,
                Color = "000000",
                LeftIndent = "720",
                HangingIndent = "360"
            },
            CodeBlock = new CodeBlockStyleConfig
            {
                Size = 10,
                Color = "333333",
                BackgroundColor = "f5f5f5",
                BorderColor = "cccccc",
                MonospaceFontAscii = "Noto Sans Mono",
                MonospaceFontEastAsia = "Noto Sans Mono CJK JP"
            },
            Quote = new QuoteStyleConfig
            {
                Size = 11,
                Color = "555555",
                Italic = true,
                ShowBorder = true,
                BorderColor = "999999",
                BorderPosition = "left",
                LeftIndent = "720"
            }
        };
    }
}
