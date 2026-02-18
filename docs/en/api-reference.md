---
layout: default
title: API Reference
lang: en
---

# API Reference

Use md2docx as a library in your .NET applications.

## Installation

Add md2docx to your project:

```bash
# Add package reference (once published to NuGet)
dotnet add package MarkdownToDocx

# Or reference the project directly
dotnet add reference path/to/MarkdownToDocx.Core.csproj
```

## Quick Start

### Basic Conversion

```csharp
using MarkdownToDocx.Core;
using MarkdownToDocx.Styling;

// Load YAML configuration
var config = YamlConfigurationLoader.LoadFromFile("config/presets/default.yaml");

// Convert Markdown to DOCX
var converter = new MarkdownConverter(config);
converter.ConvertFile("input.md", "output.docx");
```

### Using Presets

```csharp
using MarkdownToDocx.Styling;

// Load built-in preset
var config = YamlConfigurationLoader.LoadPreset("default", "/app/config/presets");

// Convert with preset
var converter = new MarkdownConverter(config);
converter.ConvertFile("input.md", "output.docx");
```

## Core Components

### MarkdownConverter

Main converter class that orchestrates the conversion process.

**Namespace**: `MarkdownToDocx.Core`

#### Constructor

```csharp
public MarkdownConverter(ConversionConfiguration config)
```

**Parameters**:
- `config` - Style configuration loaded from YAML

**Example**:

```csharp
var config = YamlConfigurationLoader.LoadFromFile("config.yaml");
var converter = new MarkdownConverter(config);
```

#### Methods

##### ConvertFile

```csharp
public void ConvertFile(string inputPath, string outputPath)
```

Converts a Markdown file to DOCX.

**Parameters**:
- `inputPath` - Path to input Markdown file
- `outputPath` - Path to output DOCX file

**Throws**:
- `FileNotFoundException` - Input file not found
- `InvalidMarkdownException` - Invalid Markdown syntax
- `ConversionException` - Conversion error

**Example**:

```csharp
try
{
    converter.ConvertFile("document.md", "document.docx");
    Console.WriteLine("Conversion successful!");
}
catch (FileNotFoundException ex)
{
    Console.Error.WriteLine($"File not found: {ex.Message}");
}
catch (ConversionException ex)
{
    Console.Error.WriteLine($"Conversion failed: {ex.Message}");
}
```

##### ConvertString

```csharp
public void ConvertString(string markdown, string outputPath)
```

Converts a Markdown string to DOCX.

**Parameters**:
- `markdown` - Markdown content as string
- `outputPath` - Path to output DOCX file

**Example**:

```csharp
var markdown = @"
# My Document

This is a paragraph with **bold** text.
";

converter.ConvertString(markdown, "output.docx");
```

##### ConvertToStream

```csharp
public Stream ConvertToStream(string markdown)
```

Converts Markdown to a DOCX stream (for web applications).

**Parameters**:
- `markdown` - Markdown content as string

**Returns**: `Stream` - DOCX content as memory stream

**Example**:

```csharp
var markdown = File.ReadAllText("input.md");
var stream = converter.ConvertToStream(markdown);

// Return as HTTP response (ASP.NET Core)
return File(stream, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "document.docx");
```

---

### YamlConfigurationLoader

Loads and parses YAML configuration files.

**Namespace**: `MarkdownToDocx.Styling`

#### Methods

##### LoadFromFile

```csharp
public static ConversionConfiguration LoadFromFile(string yamlPath)
```

Loads configuration from a YAML file.

**Parameters**:
- `yamlPath` - Path to YAML configuration file

**Returns**: `ConversionConfiguration` - Parsed configuration

**Throws**:
- `FileNotFoundException` - YAML file not found
- `YamlException` - Invalid YAML syntax
- `ConfigValidationException` - Invalid configuration schema

**Example**:

```csharp
var config = YamlConfigurationLoader.LoadFromFile("config/custom/my-style.yaml");
```

##### LoadPreset

```csharp
public static ConversionConfiguration LoadPreset(string presetName, string presetDir)
```

Loads a built-in preset by name.

**Parameters**:
- `presetName` - Preset name (without `.yaml` extension)
- `presetDir` - Directory containing presets

**Returns**: `ConversionConfiguration` - Preset configuration

**Example**:

```csharp
var config = YamlConfigurationLoader.LoadPreset("default", "/app/config/presets");
```

##### Validate

```csharp
public static bool Validate(ConversionConfiguration config, out List<string> errors)
```

Validates a configuration object.

**Parameters**:
- `config` - Configuration to validate
- `errors` - Output list of validation errors

**Returns**: `bool` - `true` if valid, `false` otherwise

**Example**:

```csharp
var config = YamlConfigurationLoader.LoadFromFile("config.yaml");
if (!YamlConfigurationLoader.Validate(config, out var errors))
{
    foreach (var error in errors)
    {
        Console.Error.WriteLine($"Validation error: {error}");
    }
}
```

---

### ConversionConfiguration

Configuration model representing YAML structure.

**Namespace**: `MarkdownToDocx.Styling`

#### Properties

```csharp
public class ConversionConfiguration
{
    public string SchemaVersion { get; set; }
    public Metadata Metadata { get; set; }
    public string TextDirection { get; set; }
    public PageLayout PageLayout { get; set; }
    public Fonts Fonts { get; set; }
    public Styles Styles { get; set; }
}
```

#### Nested Classes

##### Metadata

```csharp
public class Metadata
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Author { get; set; }
    public string Version { get; set; }
}
```

##### PageLayout

```csharp
public class PageLayout
{
    public double Width { get; set; }
    public double Height { get; set; }
    public double MarginTop { get; set; }
    public double MarginBottom { get; set; }
    public double MarginLeft { get; set; }
    public double MarginRight { get; set; }
    public double MarginHeader { get; set; }
    public double MarginFooter { get; set; }
    public double MarginGutter { get; set; }
}
```

##### Fonts

```csharp
public class Fonts
{
    public string Ascii { get; set; }
    public string EastAsia { get; set; }
    public int DefaultSize { get; set; }
}
```

##### Styles

```csharp
public class Styles
{
    public HeadingStyle H1 { get; set; }
    public HeadingStyle H2 { get; set; }
    public HeadingStyle H3 { get; set; }
    public HeadingStyle H4 { get; set; }
    public HeadingStyle H5 { get; set; }
    public HeadingStyle H6 { get; set; }
    public ParagraphStyle Paragraph { get; set; }
    public ListStyle List { get; set; }
    public CodeBlockStyle CodeBlock { get; set; }
    public QuoteStyle Quote { get; set; }
}
```

---

## Advanced Usage

### Custom Configuration in Code

Create configuration programmatically without YAML:

```csharp
var config = new ConversionConfiguration
{
    SchemaVersion = "2.0",
    Metadata = new Metadata
    {
        Name = "Custom Style",
        Description = "Programmatically created style",
        Author = "Your App",
        Version = "1.0.0"
    },
    TextDirection = "Horizontal",
    PageLayout = new PageLayout
    {
        Width = 21.0,
        Height = 29.7,
        MarginTop = 2.54,
        MarginBottom = 2.54,
        MarginLeft = 3.17,
        MarginRight = 3.17,
        MarginHeader = 1.27,
        MarginFooter = 1.27,
        MarginGutter = 0
    },
    Fonts = new Fonts
    {
        Ascii = "Noto Serif",
        EastAsia = "Noto Serif CJK JP",
        DefaultSize = 11
    },
    Styles = new Styles
    {
        H1 = new HeadingStyle
        {
            Size = 26,
            Bold = true,
            Color = "2c3e50",
            ShowBorder = true,
            BorderColor = "3498db",
            BorderSize = 6,
            BorderPosition = "bottom",
            SpaceBefore = "600",
            SpaceAfter = "300"
        },
        Paragraph = new ParagraphStyle
        {
            Size = 11,
            Color = "2c3e50",
            LineSpacing = "360",
            FirstLineIndent = "0",
            LeftIndent = "0",
            SpaceBefore = "0",
            SpaceAfter = "180"
        }
        // ... other styles
    }
};

var converter = new MarkdownConverter(config);
converter.ConvertFile("input.md", "output.docx");
```

### Batch Conversion

Convert multiple files with the same configuration:

```csharp
var config = YamlConfigurationLoader.LoadPreset("default", "/app/config/presets");
var converter = new MarkdownConverter(config);

var files = Directory.GetFiles("input", "*.md");
foreach (var file in files)
{
    var outputFile = Path.Combine("output",
        Path.GetFileNameWithoutExtension(file) + ".docx");

    try
    {
        converter.ConvertFile(file, outputFile);
        Console.WriteLine($"Converted: {file} → {outputFile}");
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Failed to convert {file}: {ex.Message}");
    }
}
```

### ASP.NET Core Integration

Use md2docx in a web API:

```csharp
using Microsoft.AspNetCore.Mvc;
using MarkdownToDocx.Core;
using MarkdownToDocx.Styling;

[ApiController]
[Route("api/[controller]")]
public class ConvertController : ControllerBase
{
    private readonly ILogger<ConvertController> _logger;
    private readonly ConversionConfiguration _defaultConfig;

    public ConvertController(ILogger<ConvertController> logger)
    {
        _logger = logger;
        _defaultConfig = YamlConfigurationLoader.LoadPreset("default", "/app/config/presets");
    }

    [HttpPost]
    public IActionResult ConvertMarkdown([FromBody] ConvertRequest request)
    {
        try
        {
            var converter = new MarkdownConverter(_defaultConfig);
            var stream = converter.ConvertToStream(request.Markdown);

            return File(stream,
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "document.docx");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Conversion failed");
            return BadRequest(new { error = ex.Message });
        }
    }
}

public class ConvertRequest
{
    public string Markdown { get; set; }
    public string Preset { get; set; }
}
```

### Background Processing

Use with background job systems (Hangfire, Quartz.NET):

```csharp
using Hangfire;

public class DocumentService
{
    [AutomaticRetry(Attempts = 3)]
    public void ConvertDocument(int documentId)
    {
        var document = _repository.GetDocument(documentId);

        var config = YamlConfigurationLoader.LoadPreset(
            document.Preset ?? "default",
            "/app/config/presets"
        );

        var converter = new MarkdownConverter(config);
        var outputPath = $"output/{documentId}.docx";

        converter.ConvertString(document.MarkdownContent, outputPath);

        document.DocxPath = outputPath;
        document.Status = "Completed";
        _repository.Update(document);
    }
}

// Usage
BackgroundJob.Enqueue<DocumentService>(x => x.ConvertDocument(123));
```

## Error Handling

### Exception Types

#### ConversionException

Thrown when conversion fails.

```csharp
try
{
    converter.ConvertFile("input.md", "output.docx");
}
catch (ConversionException ex)
{
    Console.Error.WriteLine($"Conversion error: {ex.Message}");
    Console.Error.WriteLine($"Line: {ex.LineNumber}");
}
```

#### ConfigValidationException

Thrown when YAML configuration is invalid.

```csharp
try
{
    var config = YamlConfigurationLoader.LoadFromFile("config.yaml");
}
catch (ConfigValidationException ex)
{
    Console.Error.WriteLine("Invalid configuration:");
    foreach (var error in ex.ValidationErrors)
    {
        Console.Error.WriteLine($"  - {error}");
    }
}
```

#### InvalidMarkdownException

Thrown when Markdown syntax is invalid.

```csharp
try
{
    converter.ConvertFile("input.md", "output.docx");
}
catch (InvalidMarkdownException ex)
{
    Console.Error.WriteLine($"Invalid Markdown: {ex.Message}");
    Console.Error.WriteLine($"Line {ex.LineNumber}: {ex.InvalidContent}");
}
```

### Best Practices

1. **Always validate configuration before use**:

```csharp
var config = YamlConfigurationLoader.LoadFromFile("config.yaml");
if (!YamlConfigurationLoader.Validate(config, out var errors))
{
    throw new InvalidOperationException($"Invalid config: {string.Join(", ", errors)}");
}
```

2. **Use try-catch for file operations**:

```csharp
try
{
    converter.ConvertFile(inputPath, outputPath);
}
catch (FileNotFoundException)
{
    // Handle missing file
}
catch (UnauthorizedAccessException)
{
    // Handle permission errors
}
catch (ConversionException ex)
{
    // Handle conversion errors
}
```

3. **Dispose streams properly**:

```csharp
using var stream = converter.ConvertToStream(markdown);
// Stream is automatically disposed
```

4. **Cache configurations for repeated use**:

```csharp
// Load once
private static readonly ConversionConfiguration _defaultConfig =
    YamlConfigurationLoader.LoadPreset("default", "/app/config/presets");

// Reuse
public void Convert(string input, string output)
{
    var converter = new MarkdownConverter(_defaultConfig);
    converter.ConvertFile(input, output);
}
```

## Performance Considerations

### Memory Usage

- Each `MarkdownConverter` instance is lightweight
- DOCX generation uses streaming for large documents
- Configuration objects can be reused safely

### Optimization Tips

1. **Reuse Configuration Objects**:

```csharp
// Good
var config = YamlConfigurationLoader.LoadPreset("default", "/app/config/presets");
for (int i = 0; i < 100; i++)
{
    var converter = new MarkdownConverter(config);
    // ... convert
}

// Avoid
for (int i = 0; i < 100; i++)
{
    var config = YamlConfigurationLoader.LoadPreset("default", "/app/config/presets"); // Wasteful
    // ...
}
```

2. **Use Streams for Web Applications**:

```csharp
// Good - streams directly to response
var stream = converter.ConvertToStream(markdown);
return File(stream, "application/...", "doc.docx");

// Avoid - unnecessary file I/O
converter.ConvertString(markdown, "temp.docx");
var bytes = File.ReadAllBytes("temp.docx");
return File(bytes, "application/...", "doc.docx");
```

3. **Batch Processing**:

```csharp
// Process in parallel for multiple files
Parallel.ForEach(files, file =>
{
    var converter = new MarkdownConverter(config);
    converter.ConvertFile(file, OutputPath(file));
});
```

## Testing

### Unit Testing Example

```csharp
using Xunit;
using MarkdownToDocx.Core;
using MarkdownToDocx.Styling;

public class ConverterTests
{
    [Fact]
    public void ConvertString_ValidMarkdown_CreatesDocx()
    {
        // Arrange
        var config = YamlConfigurationLoader.LoadPreset("minimal", "config/presets");
        var converter = new MarkdownConverter(config);
        var markdown = "# Test\n\nParagraph.";
        var output = "test_output.docx";

        // Act
        converter.ConvertString(markdown, output);

        // Assert
        Assert.True(File.Exists(output));
        Assert.True(new FileInfo(output).Length > 0);

        // Cleanup
        File.Delete(output);
    }

    [Fact]
    public void LoadPreset_InvalidName_ThrowsException()
    {
        // Arrange & Act & Assert
        Assert.Throws<FileNotFoundException>(() =>
            YamlConfigurationLoader.LoadPreset("nonexistent", "config/presets")
        );
    }
}
```

## Next Steps

- [Configuration Guide](./configuration) - Learn YAML configuration
- [Presets Reference](./presets) - Explore built-in presets
- [GitHub Repository](https://github.com/forest6511/md2docx) - Source code and examples

## Support

- [GitHub Issues](https://github.com/forest6511/md2docx/issues) - Report bugs
- [GitHub Discussions](https://github.com/forest6511/md2docx/discussions) - Ask questions
- [Contributing Guide](https://github.com/forest6511/md2docx/blob/main/CONTRIBUTING.md) - Contribute to the project

---

**Last Updated**: 2026-02-17
