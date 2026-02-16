---
layout: default
title: md2docx - Markdown to Word Converter
---

# md2docx

**Flexible, high-quality Markdown to Word (DOCX) converter with YAML-based styling customization.**

[![Version](https://img.shields.io/badge/version-0.1.0-blue.svg)](https://github.com/forest6511/md2docx/releases)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/)
[![Tests](https://img.shields.io/badge/tests-82%2F82%20passing-brightgreen.svg)]()
[![Coverage](https://img.shields.io/badge/coverage-90.1%25-brightgreen.svg)]()

---

## Choose Your Language / 言語を選択

<div class="language-selector">
  <a href="./en/" class="language-option">
    <h3>🇬🇧 English</h3>
    <p>Read documentation in English</p>
  </a>
  <a href="./ja/" class="language-option">
    <h3>🇯🇵 日本語</h3>
    <p>日本語でドキュメントを読む</p>
  </a>
</div>

---

## Quick Start

### Docker (Recommended)

```bash
# Pull the image
docker pull forest6511/md2docx:latest

# Convert a file
docker run --rm -v $(pwd):/workspace forest6511/md2docx:latest \
  input.md -o output.docx -p default --preset-dir /app/config/presets
```

### .NET CLI

```bash
cd csharp-version
dotnet run --project src/MarkdownToDocx.CLI -- input.md -o output.docx
```

---

## Features

- **🎨 YAML-Based Styling**: Complete control over document styling
- **📝 Rich Formatting**: Headings, lists, code blocks, quotes
- **🌐 Cross-Platform**: Docker distribution for Windows, Mac, Linux
- **🎌 Japanese Support**: Vertical text (tategaki) support
- **🔧 Extensible**: Easy-to-create custom presets
- **⚡ Performance**: Optimized for large documents
- **🆓 Open Source**: MIT licensed

---

## Links

- [GitHub Repository](https://github.com/forest6511/md2docx)
- [Releases](https://github.com/forest6511/md2docx/releases)
- [Issues](https://github.com/forest6511/md2docx/issues)
- [Discussions](https://github.com/forest6511/md2docx/discussions)
- [Docker Hub](https://hub.docker.com/r/forest6511/md2docx)
