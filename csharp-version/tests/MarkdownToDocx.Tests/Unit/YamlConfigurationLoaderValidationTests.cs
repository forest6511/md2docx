using FluentAssertions;
using MarkdownToDocx.Styling.Configuration;
using Xunit;

namespace MarkdownToDocx.Tests.Unit;

/// <summary>
/// Validation tests for YamlConfigurationLoader targeting uncovered branches
/// </summary>
public class YamlConfigurationLoaderValidationTests : IDisposable
{
    private readonly string _tempDir;
    private readonly YamlConfigurationLoader _loader;

    public YamlConfigurationLoaderValidationTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"md2docx_yaml_val_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _loader = new YamlConfigurationLoader(_tempDir);
    }

    [Fact]
    public void Load_WithInvalidYamlSyntax_ShouldThrowInvalidDataException()
    {
        // Arrange - Malformed YAML
        var path = WriteTempYaml("invalid_syntax.yaml", "{ invalid: yaml: [broken");

        // Act
        Action act = () => _loader.Load(path);

        // Assert
        act.Should().Throw<InvalidDataException>()
            .WithMessage("*Invalid YAML format*");
    }

    [Fact]
    public void Load_WithEmptySchemaVersion_ShouldThrowInvalidDataException()
    {
        // Arrange
        var yaml = CreateValidYaml(schemaVersion: "");
        var path = WriteTempYaml("empty_schema.yaml", yaml);

        // Act
        Action act = () => _loader.Load(path);

        // Assert
        act.Should().Throw<InvalidDataException>()
            .WithMessage("*Schema version is required*");
    }

    [Fact]
    public void Load_WithEmptyMetadataName_ShouldThrowInvalidDataException()
    {
        // Arrange
        var yaml = CreateValidYaml(metadataName: "");
        var path = WriteTempYaml("empty_name.yaml", yaml);

        // Act
        Action act = () => _loader.Load(path);

        // Assert
        act.Should().Throw<InvalidDataException>()
            .WithMessage("*Metadata name is required*");
    }

    [Fact]
    public void Load_WithEmptyMetadataDescription_ShouldThrowInvalidDataException()
    {
        // Arrange
        var yaml = CreateValidYaml(metadataDescription: "");
        var path = WriteTempYaml("empty_desc.yaml", yaml);

        // Act
        Action act = () => _loader.Load(path);

        // Assert
        act.Should().Throw<InvalidDataException>()
            .WithMessage("*Metadata description is required*");
    }

    [Fact]
    public void Load_WithZeroPageWidth_ShouldThrowInvalidDataException()
    {
        // Arrange
        var yaml = CreateValidYaml(pageWidth: 0);
        var path = WriteTempYaml("zero_width.yaml", yaml);

        // Act
        Action act = () => _loader.Load(path);

        // Assert
        act.Should().Throw<InvalidDataException>()
            .WithMessage("*Page width must be greater than 0*");
    }

    [Fact]
    public void Load_WithZeroPageHeight_ShouldThrowInvalidDataException()
    {
        // Arrange
        var yaml = CreateValidYaml(pageHeight: 0);
        var path = WriteTempYaml("zero_height.yaml", yaml);

        // Act
        Action act = () => _loader.Load(path);

        // Assert
        act.Should().Throw<InvalidDataException>()
            .WithMessage("*Page height must be greater than 0*");
    }

    [Fact]
    public void Load_WithEmptyAsciiFontName_ShouldThrowInvalidDataException()
    {
        // Arrange
        var yaml = CreateValidYaml(fontsAscii: "");
        var path = WriteTempYaml("empty_ascii_font.yaml", yaml);

        // Act
        Action act = () => _loader.Load(path);

        // Assert
        act.Should().Throw<InvalidDataException>()
            .WithMessage("*ASCII font name is required*");
    }

    [Fact]
    public void Load_WithEmptyEastAsiaFontName_ShouldThrowInvalidDataException()
    {
        // Arrange
        var yaml = CreateValidYaml(fontsEastAsia: "");
        var path = WriteTempYaml("empty_ea_font.yaml", yaml);

        // Act
        Action act = () => _loader.Load(path);

        // Assert
        act.Should().Throw<InvalidDataException>()
            .WithMessage("*East Asia font name is required*");
    }

    [Fact]
    public void Load_WithZeroDefaultFontSize_ShouldThrowInvalidDataException()
    {
        // Arrange
        var yaml = CreateValidYaml(fontsDefaultSize: 0);
        var path = WriteTempYaml("zero_fontsize.yaml", yaml);

        // Act
        Action act = () => _loader.Load(path);

        // Assert
        act.Should().Throw<InvalidDataException>()
            .WithMessage("*Default font size must be greater than 0*");
    }

    [Fact]
    public void Load_WithValidConfiguration_ShouldSucceed()
    {
        // Arrange
        var yaml = CreateValidYaml();
        var path = WriteTempYaml("valid.yaml", yaml);

        // Act
        var config = _loader.Load(path);

        // Assert
        config.Should().NotBeNull();
        config.SchemaVersion.Should().Be("2.0");
        config.Metadata.Name.Should().Be("Test");
    }

    private string WriteTempYaml(string fileName, string content)
    {
        var path = Path.Combine(_tempDir, fileName);
        File.WriteAllText(path, content);
        return path;
    }

    private static string CreateValidYaml(
        string schemaVersion = "2.0",
        string metadataName = "Test",
        string metadataDescription = "Test preset",
        double pageWidth = 21.0,
        double pageHeight = 29.7,
        string fontsAscii = "Noto Serif",
        string fontsEastAsia = "Noto Serif CJK JP",
        int fontsDefaultSize = 11)
    {
        return $@"SchemaVersion: ""{schemaVersion}""
Metadata:
  Name: ""{metadataName}""
  Description: ""{metadataDescription}""
TextDirection: Horizontal
PageLayout:
  Width: {pageWidth}
  Height: {pageHeight}
  MarginTop: 2.5
  MarginBottom: 2.5
  MarginLeft: 2.5
  MarginRight: 2.5
Fonts:
  Ascii: ""{fontsAscii}""
  EastAsia: ""{fontsEastAsia}""
  DefaultSize: {fontsDefaultSize}
Styles:
  H1:
    Size: 24
    Bold: true
    Color: ""000000""
  Paragraph:
    Size: 11
    Color: ""000000""
    LineSpacing: ""360""
  CodeBlock:
    Size: 10
    Color: ""000000""
    BackgroundColor: ""f5f5f5""
    BorderColor: ""cccccc""
    MonospaceFontAscii: ""Courier New""
    MonospaceFontEastAsia: ""MS Gothic""
  Quote:
    Size: 11
    Color: ""666666""
TableOfContents:
  Enabled: false
  Depth: 3
  Title: ""Contents""
  PageBreakAfter: true
";
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, true);
        }
        GC.SuppressFinalize(this);
    }
}
