# Release Notes: v0.1.0

**Release Date**: 2026-02-16
**Tag**: v0.1.0
**Status**: ✅ Ready for GitHub Release

---

## 🎉 Initial Public Release

md2docx v0.1.0 is the first public release of a flexible, high-quality Markdown to Word (DOCX) converter with YAML-based styling customization.

---

## ✨ Key Features

### Core Functionality
- **Markdown to DOCX Conversion**: Transform Markdown documents into professionally formatted Word files
- **YAML-Based Styling**: Complete control over document styling through simple YAML configuration files
- **Text Direction Support**: Full support for both horizontal and vertical (Japanese tategaki) text layouts
- **Rich Formatting**: Headings (H1-H6), paragraphs, lists, code blocks, blockquotes with full styling control

### Built-in Presets
- **minimal**: Clean, minimalist styling
- **default**: Balanced, general-purpose formatting
- **technical**: Optimized for technical documentation
- **vertical-novel**: Japanese tategaki for novel formatting

### Distribution
- **Docker-First**: Cross-platform Docker images with embedded Noto CJK fonts
- **CLI Tool**: Simple command-line interface for easy integration
- **Open Source**: MIT licensed, community-driven development

---

## 📊 Quality Metrics

- ✅ **Test Coverage**: 90.1% overall (Core: 91.5%, Styling: 87.1%)
- ✅ **Tests**: 82/82 passing (100%)
- ✅ **Build**: 0 errors, 13 warnings (nullable reference only)
- ✅ **Documentation**: Full XML documentation for public APIs
- ✅ **ADRs**: 3 Architecture Decision Records documenting key decisions

---

## 🚀 Quick Start

### Docker (Recommended)

```bash
# Pull the image
docker pull forest6511/md2docx:0.1.0

# Convert with default preset
docker run --rm -v $(pwd):/workspace md2docx:0.1.0 input.md -o output.docx

# Convert with specific preset
docker run --rm -v $(pwd):/workspace md2docx:0.1.0 input.md -p minimal -o output.docx
```

### .NET CLI

```bash
# Build and run
cd csharp-version/src
dotnet run --project MarkdownToDocx.CLI -- input.md -o output.docx
```

---

## 📦 Assets

### Docker Images
- `md2docx:latest` - Standard image with Noto CJK fonts (560MB)
- `md2docx:slim` - Minimal image (planned)
- `md2docx:full` - Full image with all fonts (planned)

### Source Code
- **GitHub Repository**: https://github.com/forest6511/md2docx
- **Tag**: v0.1.0
- **Commit**: 67de825

---

## 📚 Documentation

- **README.md**: Quick start guide and feature overview
- **CHANGELOG.md**: Detailed changelog for v0.1.0
- **CLAUDE.md**: Development guidelines and project configuration
- **CONTRIBUTING.md**: Contribution guidelines
- **ADRs**: Architecture Decision Records (ADR-0001 to ADR-0003)

---

## 🛠️ Technical Stack

| Component | Technology | Version |
|-----------|------------|---------|
| Language | C# | 12.0 |
| Runtime | .NET | 8.0 |
| Markdown Parser | Markdig | 0.45.0 |
| Word Generation | DocumentFormat.OpenXml | 3.4.1 |
| Configuration | YamlDotNet | 16.0.0 |
| Distribution | Docker | Multi-stage builds |

---

## ⚠️ Known Limitations

This is an early release with the following known limitations:

- ❌ **Nested lists**: Not yet supported
- ❌ **Images**: Image embedding not implemented
- ❌ **Tables**: Table support planned for future release
- ❌ **Hyperlinks**: Link conversion not yet available
- ❌ **Inline code**: Only code blocks supported currently

These features are planned for future releases. Contributions welcome!

---

## 🙏 Credits

### Third-Party Libraries
- **DocumentFormat.OpenXml** by Microsoft (MIT License)
- **Markdig** by Alexandre Mutel (BSD 2-Clause License)
- **YamlDotNet** by Antoine Aubry and contributors (MIT License)

### Fonts
- **Noto CJK** by Google (SIL Open Font License 1.1)

### Development Tools
- **Claude Code** by Anthropic (development assistance)

---

## 🤝 Contributing

We welcome contributions! Please see [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

Areas for contribution:
- Nested list support
- Image embedding
- Table conversion
- Hyperlink support
- Additional styling options
- Performance optimizations
- Additional presets

---

## 📝 License

MIT License - see [LICENSE](LICENSE) for details.

---

## 🔗 Links

- **GitHub Repository**: https://github.com/forest6511/md2docx
- **Issues**: https://github.com/forest6511/md2docx/issues
- **Discussions**: https://github.com/forest6511/md2docx/discussions
- **Releases**: https://github.com/forest6511/md2docx/releases

---

## 🎯 Next Steps for Users

1. **Try it out**: Use Docker image to convert your first Markdown file
2. **Explore presets**: Test different styling presets
3. **Create custom presets**: Design your own YAML configurations
4. **Report issues**: Help improve the project by reporting bugs
5. **Contribute**: Submit pull requests for new features

---

**Thank you for using md2docx!** 🙏

Your feedback and contributions help make this project better.
