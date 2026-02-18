using FluentAssertions;
using MarkdownToDocx.Core.Models;
using MarkdownToDocx.Styling.Configuration;
using Xunit;

namespace MarkdownToDocx.Tests.Unit;

/// <summary>
/// Unit tests for YamlConfigurationLoader
/// </summary>
public class YamlConfigurationLoaderTests
{
    private const string TestPresetDirectory = "../../../../../../config/presets";
    private readonly YamlConfigurationLoader _loader;

    public YamlConfigurationLoaderTests()
    {
        _loader = new YamlConfigurationLoader(TestPresetDirectory);
    }

    [Fact]
    public void Constructor_WithNullDirectory_ShouldThrowArgumentNullException()
    {
        // Arrange & Act
        Action act = () => new YamlConfigurationLoader(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("presetDirectory");
    }

    [Fact]
    public void LoadPreset_WithMinimalPreset_ShouldLoadSuccessfully()
    {
        // Arrange
        const string presetName = "minimal";

        // Act
        var config = _loader.LoadPreset(presetName);

        // Assert
        config.Should().NotBeNull();
        config.SchemaVersion.Should().Be("2.0");
        config.Metadata.Name.Should().Be("Minimal");
        config.TextDirection.Should().Be(TextDirectionMode.Horizontal);
    }

    [Fact]
    public void LoadPreset_WithMinimalPreset_ShouldHaveCorrectPageLayout()
    {
        // Arrange
        const string presetName = "minimal";

        // Act
        var config = _loader.LoadPreset(presetName);

        // Assert
        config.PageLayout.Should().NotBeNull();
        config.PageLayout.Width.Should().Be(21.0);
        config.PageLayout.Height.Should().Be(29.7);
        config.PageLayout.MarginTop.Should().Be(2.5);
        config.PageLayout.MarginBottom.Should().Be(2.5);
        config.PageLayout.MarginLeft.Should().Be(2.5);
        config.PageLayout.MarginRight.Should().Be(2.5);
    }

    [Fact]
    public void LoadPreset_WithMinimalPreset_ShouldHaveCorrectFonts()
    {
        // Arrange
        const string presetName = "minimal";

        // Act
        var config = _loader.LoadPreset(presetName);

        // Assert
        config.Fonts.Should().NotBeNull();
        config.Fonts.Ascii.Should().Be("Noto Serif");
        config.Fonts.EastAsia.Should().Be("Noto Serif CJK JP");
        config.Fonts.DefaultSize.Should().Be(11);
    }

    [Fact]
    public void LoadPreset_WithMinimalPreset_ShouldHaveCorrectHeadingStyles()
    {
        // Arrange
        const string presetName = "minimal";

        // Act
        var config = _loader.LoadPreset(presetName);

        // Assert
        config.Styles.Should().NotBeNull();
        config.Styles.H1.Should().NotBeNull();
        config.Styles.H1.Size.Should().Be(24);
        config.Styles.H1.Bold.Should().BeTrue();
        config.Styles.H1.Color.Should().Be("000000");
        config.Styles.H1.ShowBorder.Should().BeFalse();
    }

    [Fact]
    public void LoadPreset_WithMinimalPreset_ShouldHaveCorrectParagraphStyle()
    {
        // Arrange
        const string presetName = "minimal";

        // Act
        var config = _loader.LoadPreset(presetName);

        // Assert
        config.Styles.Paragraph.Should().NotBeNull();
        config.Styles.Paragraph.Size.Should().Be(11);
        config.Styles.Paragraph.Color.Should().Be("000000");
        config.Styles.Paragraph.LineSpacing.Should().Be("360");
    }

    [Fact]
    public void LoadPreset_WithMinimalPreset_ShouldHaveCorrectCodeBlockStyle()
    {
        // Arrange
        const string presetName = "minimal";

        // Act
        var config = _loader.LoadPreset(presetName);

        // Assert
        config.Styles.CodeBlock.Should().NotBeNull();
        config.Styles.CodeBlock.Size.Should().Be(10);
        config.Styles.CodeBlock.MonospaceFontAscii.Should().Be("Noto Sans Mono");
        config.Styles.CodeBlock.BackgroundColor.Should().Be("f5f5f5");
    }

    [Fact]
    public void LoadPreset_WithMinimalPreset_ShouldHaveDefaultPageBreakBefore()
    {
        // Arrange
        const string presetName = "minimal";

        // Act
        var config = _loader.LoadPreset(presetName);

        // Assert - PageBreakBefore defaults to false when not specified in YAML
        config.Styles.H1.PageBreakBefore.Should().BeFalse();
        config.Styles.H2.PageBreakBefore.Should().BeFalse();
    }

    [Fact]
    public void LoadPreset_WithNonExistentPreset_ShouldThrowFileNotFoundException()
    {
        // Arrange
        const string presetName = "nonexistent";

        // Act
        Action act = () => _loader.LoadPreset(presetName);

        // Assert
        act.Should().Throw<FileNotFoundException>()
            .WithMessage("*nonexistent*");
    }

    [Fact]
    public void LoadPreset_WithNullPresetName_ShouldThrowArgumentNullException()
    {
        // Arrange & Act
        Action act = () => _loader.LoadPreset(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("presetName");
    }

    [Fact]
    public void Load_WithValidPath_ShouldLoadSuccessfully()
    {
        // Arrange
        var configPath = Path.Combine(TestPresetDirectory, "minimal.yaml");

        // Act
        var config = _loader.Load(configPath);

        // Assert
        config.Should().NotBeNull();
        config.Metadata.Name.Should().Be("Minimal");
    }

    [Fact]
    public void Load_WithNullPath_ShouldThrowArgumentNullException()
    {
        // Arrange & Act
        Action act = () => _loader.Load(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("configPath");
    }

    [Fact]
    public void Load_WithNonExistentPath_ShouldThrowFileNotFoundException()
    {
        // Arrange
        const string configPath = "/nonexistent/path/config.yaml";

        // Act
        Action act = () => _loader.Load(configPath);

        // Assert
        act.Should().Throw<FileNotFoundException>();
    }
}
