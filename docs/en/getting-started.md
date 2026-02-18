---
layout: default
title: Getting Started
lang: en
---

# Getting Started

This guide will help you install and use md2docx for the first time.

## Installation

### Docker (Recommended)

Docker provides the easiest and most consistent experience across all platforms.

```bash
# Pull the latest image
docker pull forest6511/md2docx:latest

# Verify installation
docker run --rm forest6511/md2docx:latest --help
```

**Advantages of Docker:**
- No .NET installation required
- Embedded fonts (Noto CJK) for consistent output
- Works identically on Windows, Mac, and Linux
- No dependency conflicts

### .NET CLI

For development or if you prefer not to use Docker:

**Requirements:**
- .NET 8.0 SDK or later

```bash
# Clone the repository
git clone https://github.com/forest6511/md2docx.git
cd md2docx

# Build the project
cd csharp-version
dotnet build src/MarkdownToDocx.sln

# Run
dotnet run --project src/MarkdownToDocx.CLI -- --help
```

## Your First Conversion

### Step 1: Create a Markdown File

Create a file named `example.md`:

```markdown
# My First Document

This is a paragraph with **bold** and *italic* text.

## Section 1

- Item 1
- Item 2
- Item 3

## Code Example

\```python
def hello_world():
    print("Hello, World!")
\```

> This is a quote block.
```

### Step 2: Convert to DOCX

**Using Docker:**

```bash
docker run --rm -v $(pwd):/workspace forest6511/md2docx:latest \
  example.md -o example.docx -p default --preset-dir /app/config/presets
```

**Using .NET CLI:**

```bash
dotnet run --project src/MarkdownToDocx.CLI -- \
  example.md -o example.docx -p default --preset-dir ../config/presets
```

### Step 3: Open the Result

Open `example.docx` in Microsoft Word, LibreOffice, or any compatible word processor.

## Available Presets

md2docx comes with several built-in presets:

- **minimal** - Bare minimum styling, black and white
- **default** - Balanced, general-purpose formatting
- **technical** - Optimized for technical documentation
- **vertical-novel** - Japanese vertical text for novels

Try different presets:

```bash
# Minimal style
docker run --rm -v $(pwd):/workspace forest6511/md2docx:latest \
  example.md -o minimal.docx -p minimal --preset-dir /app/config/presets

# Technical style
docker run --rm -v $(pwd):/workspace forest6511/md2docx:latest \
  example.md -o technical.docx -p technical --preset-dir /app/config/presets
```

## Command-Line Options

```
Usage: md2docx [options] <input-file>

Arguments:
  <input-file>    Input Markdown file

Options:
  -o, --output <file>         Output DOCX file (default: input.docx)
  -p, --preset <name>         Preset name (minimal, default, technical)
  -c, --config <file>         Custom YAML configuration file
  --cover-image <file>        Cover image for title page
  --preset-dir <directory>    Preset directory path
  -v, --verbose               Verbose output
  -h, --help                  Show help information
```

## Shell Alias (Optional)

For convenience, create a shell alias:

**Bash/Zsh (~/.bashrc or ~/.zshrc):**

```bash
alias md2docx='docker run --rm -v $(pwd):/workspace forest6511/md2docx:latest'
```

**PowerShell:**

```powershell
function md2docx { docker run --rm -v ${PWD}:/workspace forest6511/md2docx:latest @args }
```

After adding the alias, you can use:

```bash
md2docx example.md -o output.docx -p default --preset-dir /app/config/presets
```

## Next Steps

- [Configuration Guide](./configuration) - Learn how to customize styling
- [Presets Reference](./presets) - Explore all available presets
- [API Documentation](./api-reference) - Use md2docx as a library

## Troubleshooting

### Docker Permission Errors

If you encounter permission errors when writing output files:

```bash
# Run with user permissions
docker run --user $(id -u):$(id -g) \
  --rm -v $(pwd):/workspace forest6511/md2docx:latest \
  input.md -o output.docx
```

### Font Not Found

Docker images include Noto fonts. If you're using .NET CLI and seeing different fonts:

1. Use Docker for consistent font handling
2. Or install Noto fonts on your system
3. Or specify system fonts in your YAML config

### Conversion Fails

Check that:
1. Input file exists and is valid Markdown
2. Output directory is writable
3. YAML configuration is valid (if using custom config)

For more help:
- [GitHub Issues](https://github.com/forest6511/md2docx/issues)
- [GitHub Discussions](https://github.com/forest6511/md2docx/discussions)

---

**Last Updated**: 2026-02-17
