# Markdown to Word Converter

[![Version](https://img.shields.io/badge/version-0.1.0-blue.svg)](https://github.com/forest6511/md2docx/releases)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/)
[![Tests](https://img.shields.io/badge/tests-82%2F82%20passing-brightgreen.svg)]()
[![Coverage](https://img.shields.io/badge/coverage-90.1%25-brightgreen.svg)]()

**A flexible, high-quality Markdown to Word (DOCX) converter with YAML-based styling customization.**

Transform your Markdown documents into professionally formatted Word files with complete control over styling through YAML configuration files. Supports horizontal and vertical (Japanese tategaki) text, making it ideal for technical documentation, business reports, and publishing workflows.

## 📖 Documentation

**[📘 English Documentation](https://forest6511.github.io/md2docx/en/)** | **[📗 日本語ドキュメント](https://forest6511.github.io/md2docx/ja/)**

Visit our comprehensive documentation site for detailed guides, configuration examples, and API references.

---

## ✨ Features

- **🎨 YAML-Based Styling**: Customize fonts, colors, borders, backgrounds, spacing through simple YAML files
- **📝 Rich Formatting**: Headings (H1-H6), paragraphs, lists, code blocks, quotes with full styling control
- **🌐 Cross-Platform**: Docker-based distribution works on Windows, Mac, Linux
- **🎌 Japanese Support**: Full vertical text (tategaki) support for novel and document formatting
- **🔧 Extensible**: Easy-to-create custom presets for any use case
- **⚡ Performance**: Optimized for documents up to 500+ pages
- **🆓 Open Source**: MIT licensed, community-driven

---

## 🚀 Quick Start

### Using Docker (Recommended)

```bash
# Pull the image from Docker Hub
docker pull forest6511/md2docx:latest

# Convert a file
docker run --rm -v $(pwd):/workspace forest6511/md2docx:latest \
  input.md -o output.docx -p default --preset-dir /app/config/presets

# Or build locally
docker build -t md2docx:latest -f Dockerfile .
docker run --rm -v $(pwd):/workspace md2docx:latest \
  input.md -o output.docx -p default --preset-dir /app/config/presets
```

### Using .NET CLI

```bash
cd csharp-version
dotnet run --project src/MarkdownToDocx.CLI/MarkdownToDocx.CLI.csproj -- \
  input.md -o output.docx -p default --preset-dir ../config/presets
```

### Installation Alias (Optional)

```bash
# Add to ~/.bashrc or ~/.zshrc
alias md2docx='docker run --rm -v $(pwd):/workspace forest6511/md2docx:latest'

# Then use simply:
md2docx input.md -o output.docx -p default --preset-dir /app/config/presets
```

---

## 📖 Use Cases

### Technical Documentation

```bash
docker run --rm -v $(pwd):/workspace forest6511/md2docx:latest \
  README.md -o README.docx -p technical --preset-dir /app/config/presets
```

Perfect for:
- API documentation
- Software manuals
- Technical specifications
- README files

**Features**: Compact layout, monospace code blocks, syntax highlighting markers

### Business Documents

```bash
docker run --rm -v $(pwd):/workspace forest6511/md2docx:latest proposal.md -o proposal.docx -p default --preset-dir /app/config/presets
```

Perfect for:
- Proposals and reports
- Meeting minutes
- Project documentation
- General-purpose documents

**Features**: Balanced styling, professional appearance, colored headings

### Japanese Vertical Writing

```bash
docker run --rm -v $(pwd):/workspace forest6511/md2docx:latest novel.md -o novel.docx -p vertical-novel --preset-dir /app/config/vertical
```

Perfect for:
- Japanese novels
- Traditional documents
- Vertical text publications

**Features**: A5 page size, vertical text flow, appropriate line spacing

---

## 🎨 Available Presets

### Standard Presets (`config/presets/`)

- **`minimal`** - Bare minimum styling
  - Black & white, no borders
  - Straightforward conversion
  - Smallest file size

- **`default`** - Balanced general-purpose
  - Colored headings (#2c3e50, #34495e)
  - Bottom borders on H1/H2
  - Code blocks with light gray background
  - Quote blocks with left border

- **`technical`** - Code-heavy documentation
  - Sans-serif fonts (Noto Sans)
  - Compact layout (10pt base)
  - Emphasized code blocks
  - Simple monochrome palette

### Vertical Text Presets (`config/vertical/`)

- **`vertical-novel`** - Japanese novel formatting
  - A5 page size (14.8cm × 21.0cm)
  - Vertical text direction
  - Wide line spacing (440 twips)
  - Minimal styling for readability

---

## 🔧 Supported Markdown Elements

✅ **Currently Supported**:
- Headings (H1-H6)
- Paragraphs with inline formatting (bold, italic)
- Ordered and unordered lists
- Fenced code blocks with language specification
- Blockquotes
- Horizontal rules (thematic breaks)

🚧 **Planned for Future Releases**:
- Tables
- Images
- Inline code
- Links
- Task lists
- Footnotes

---

## 🎨 Customization

### Create Custom Style

1. Copy an existing preset:

```bash
cp config/presets/default.yaml config/custom/my-style.yaml
```

2. Edit YAML file to customize styling:

```yaml
SchemaVersion: "2.0"

Metadata:
  Name: "My Custom Style"
  Description: "Custom styling for my documents"
  Author: "Your Name"
  Version: "1.0.0"

TextDirection: "Horizontal"  # or "Vertical"

Fonts:
  Ascii: "Noto Serif"
  EastAsia: "Noto Serif CJK JP"
  DefaultSize: 11

Styles:
  H1:
    Size: 26
    Bold: true
    Color: "2c3e50"
    ShowBorder: true
    BorderColor: "3498db"
    BorderSize: 6
    BorderPosition: "bottom"
    SpaceBefore: "600"
    SpaceAfter: "300"

  Paragraph:
    Size: 11
    Color: "2c3e50"
    LineSpacing: "360"
    FirstLineIndent: "0"
    LeftIndent: "0"

  CodeBlock:
    Size: 10
    Color: "2c3e50"
    BackgroundColor: "ecf0f1"
    BorderColor: "bdc3c7"
    MonospaceFontAscii: "Noto Sans Mono"
    MonospaceFontEastAsia: "Noto Sans Mono CJK JP"
    ShowBorder: true
```

3. Use your custom style:

```bash
md2docx input.md -o output.docx -c config/custom/my-style.yaml
```

See existing presets in `config/presets/` for complete examples.

---

## 🐳 Docker

### Building Images

```bash
# Standard image (recommended)
./scripts/build-docker.sh

# Manual build
docker build -t md2docx:latest -f Dockerfile .
```

### Image Details

| Component | Details |
|-----------|---------|
| Base Image | mcr.microsoft.com/dotnet/runtime:8.0 |
| Size | ~560MB |
| Included Fonts | Noto Serif, Noto Sans, Noto CJK JP, Noto Mono |
| Presets | 4 built-in (minimal, default, technical, vertical-novel) |

### Testing Docker Image

```bash
# Run automated tests
./scripts/test-docker.sh

# Manual test
docker run --rm md2docx:latest --help
```

---

## 🔧 Development

### Prerequisites

- .NET 8.0 SDK
- Docker (optional, for container builds)
- Git

### Setup

```bash
# Clone repository
git clone https://github.com/forest6511/md2docx.git
cd md2docx

# Install git hooks
./.claude/hooks/install.sh

# Build project
cd csharp-version
dotnet build

# Run tests
dotnet test
```

### Project Structure

```
md2docx/
├── csharp-version/
│   ├── src/
│   │   ├── MarkdownToDocx.Core/        # Markdown parsing & DOCX generation
│   │   ├── MarkdownToDocx.Styling/     # YAML configuration & styling
│   │   └── MarkdownToDocx.CLI/         # Command-line interface
│   └── tests/
│       └── MarkdownToDocx.Tests/       # Unit & integration tests
├── config/
│   ├── presets/                        # Standard presets
│   └── vertical/                       # Vertical text presets
├── scripts/
│   ├── build-docker.sh                 # Docker build automation
│   └── test-docker.sh                  # Docker test automation
├── Dockerfile                          # Production Docker image
└── docker-compose.yml                  # Docker Compose configuration
```

### Testing

```bash
# Run all tests
cd csharp-version
dotnet test

# Current test coverage: 82/82 tests passing (90.1% coverage)
# - Core layer: 42 tests (91.5% coverage)
# - Styling layer: 37 tests (87.1% coverage)
# - Integration: 3 tests
```

### Development Workflow

1. **Create feature branch**: `git checkout -b feature/your-feature`
2. **Make changes**: Implement your feature
3. **Test**: `dotnet test`
4. **Commit**: Pre-commit hooks run automatically (YAML validation, secret detection)
5. **Push**: Pre-push hooks run Codex review
6. **Create PR**: Review and merge

---

## 🤝 Contributing

Contributions are welcome! Please see [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

### Areas for Contribution

- **Presets**: Create new YAML presets for specific use cases
- **Features**: Implement tables, images, inline code, links
- **Bug Fixes**: Report and fix issues
- **Documentation**: Improve docs, add examples
- **Testing**: Add test cases, improve coverage

---

## 📚 Documentation

### Online Documentation

- **[📘 English Documentation](https://forest6511.github.io/md2docx/en/)** - Complete documentation in English
  - [Getting Started](https://forest6511.github.io/md2docx/en/getting-started) - Installation and first steps
  - [Configuration](https://forest6511.github.io/md2docx/en/configuration) - YAML configuration guide
  - [Presets](https://forest6511.github.io/md2docx/en/presets) - Built-in presets reference
  - [API Reference](https://forest6511.github.io/md2docx/en/api-reference) - C# API documentation

- **[📗 日本語ドキュメント](https://forest6511.github.io/md2docx/ja/)** - 日本語の完全なドキュメント
  - [はじめに](https://forest6511.github.io/md2docx/ja/getting-started) - インストールと初回使用
  - [設定](https://forest6511.github.io/md2docx/ja/configuration) - YAML設定ガイド
  - [プリセット](https://forest6511.github.io/md2docx/ja/presets) - ビルトインプリセットリファレンス
  - [APIリファレンス](https://forest6511.github.io/md2docx/ja/api-reference) - C# API ドキュメント

### Repository Documentation

- [CLAUDE.md](CLAUDE.md) - Project configuration for Claude Code
- [DIRECTORY_STRUCTURE.md](docs/DIRECTORY_STRUCTURE.md) - Complete project structure
- [Config Examples](config/presets/) - YAML preset examples

---

## 🐛 Troubleshooting

### Font Not Found

**Problem**: Generated DOCX shows different font than expected

**Solution**:
- Docker images include Noto fonts automatically
- Verify font name in YAML matches available fonts
- Use Noto family fonts for maximum compatibility

### Conversion Fails

**Problem**: Error during conversion

**Solution**:
1. Validate Markdown syntax
2. Check YAML configuration: `python3 -c "import yaml; yaml.safe_load(open('config.yaml'))"`
3. Run with `-v` or `--verbose` flag for detailed error messages
4. Check [GitHub Issues](https://github.com/forest6511/md2docx/issues)

### Docker Permission Errors

**Problem**: Cannot write output file

**Solution**:

```bash
# Ensure output directory is writable
chmod 755 $(pwd)

# Or run with user permissions
docker run --user $(id -u):$(id -g) \
  -v $(pwd):/workspace md2docx:latest input.md -o output.docx
```

---

## 📄 License

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.

### Font Licenses

- **Noto Fonts**: SIL Open Font License 1.1
- Commercial fonts (MS Gothic, Yu Mincho, Hiragino, etc.) are NOT included

---

## ⭐ Star Us

If you find this project useful, please consider giving it a star on GitHub!

[![GitHub stars](https://img.shields.io/github/stars/forest6511/md2docx.svg?style=social&label=Star)](https://github.com/forest6511/md2docx)

---

Made with ❤️ by the open-source community
