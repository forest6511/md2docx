---
layout: default
title: md2docx Documentation
lang: en
---

# md2docx Documentation

Welcome to the official documentation for **md2docx**, a flexible Markdown to Word (DOCX) converter.

## Navigation

- [Getting Started](./getting-started) - Installation and first conversion
- [Configuration](./configuration) - YAML configuration guide
- [Presets](./presets) - Built-in and custom presets
- [API Reference](./api-reference) - C# API documentation

## What is md2docx?

md2docx is a command-line tool that converts Markdown documents into professionally formatted Word (DOCX) files. Unlike other converters, md2docx gives you complete control over styling through simple YAML configuration files.

## Key Features

### 🎨 YAML-Based Styling
Define your document style in a simple YAML file:

```yaml
Styles:
  H1:
    Size: 26
    Bold: true
    Color: "2c3e50"
    ShowBorder: true

  Paragraph:
    Size: 11
    LineSpacing: "360"
```

### 📝 Rich Markdown Support
- Headings (H1-H6)
- Paragraphs with inline formatting
- Ordered and unordered lists
- Fenced code blocks
- Blockquotes
- Horizontal rules

### 🌐 Cross-Platform
Distributed as Docker images with embedded fonts for consistent results across all platforms.

### 🎌 Japanese Support
Full support for Japanese vertical text (tategaki) for novels and traditional documents.

## Quick Example

```bash
# Using Docker
docker run --rm -v $(pwd):/workspace forest6511/md2docx:latest \
  input.md -o output.docx -p default --preset-dir /app/config/presets

# Using .NET CLI
dotnet run --project src/MarkdownToDocx.CLI -- input.md -o output.docx
```

## Use Cases

- **Technical Documentation** - API docs, software manuals, README files
- **Business Documents** - Proposals, reports, meeting minutes
- **Publishing** - Book manuscripts with custom formatting
- **Japanese Vertical Text** - Novels and traditional documents

## Getting Help

- [GitHub Issues](https://github.com/forest6511/md2docx/issues) - Report bugs
- [GitHub Discussions](https://github.com/forest6511/md2docx/discussions) - Ask questions
- [Contributing Guide](https://github.com/forest6511/md2docx/blob/main/CONTRIBUTING.md) - Contribute to the project

## License

md2docx is open source software licensed under the [MIT License](https://opensource.org/licenses/MIT).
