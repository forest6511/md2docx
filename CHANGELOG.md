# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.1.0] - 2026-02-16

### Added

#### Core Features
- **Markdown to Word (DOCX) conversion** using DocumentFormat.OpenXml
- **YAML-based styling system** with hierarchical configuration
- **Text direction support**: Horizontal and vertical (Japanese tategaki) layouts
- **Rich Markdown elements**: Headings (H1-H6), paragraphs, lists (ordered/unordered), code blocks, blockquotes, thematic breaks
- **Docker distribution** with embedded Noto CJK fonts for cross-platform consistency

#### Styling & Customization
- **Built-in presets**: minimal, default, technical configurations
- **Vertical text preset**: vertical-novel for Japanese tategaki documents
- **Customizable properties**: Fonts, colors, sizes, borders, backgrounds, spacing, indentation
- **Font support**: Noto Serif CJK JP, Noto Sans CJK JP (SIL Open Font License)

#### CLI Interface
- Command-line tool with intuitive arguments
- Preset selection via \`-p\` or \`--preset\` flag
- Custom preset directory support with \`--preset-dir\`
- Output file specification with \`-o\` or \`--output\`

#### Quality Assurance
- **Test coverage**: 90.1% overall (Core: 91.5%, Styling: 87.1%)
- **82 unit and integration tests** (100% passing)
- **Zero warnings**: Clean build with full XML documentation
- **3 Architecture Decision Records (ADRs)** documenting key technical decisions

#### Development Infrastructure
- Git hooks for quality enforcement (pre-commit, pre-push)
- Markdown linting automation
- Documentation consistency validation
- Comprehensive project documentation in English

### Technical Stack
- **Language**: C# 12.0 with .NET 8.0
- **Markdown Parser**: Markdig 0.45.0
- **Word Generation**: DocumentFormat.OpenXml 3.4.1
- **Configuration**: YamlDotNet 16.0.0
- **Distribution**: Docker multi-stage builds

### Documentation
- Comprehensive README with quick start guide
- YAML configuration examples with inline comments
- Architecture Decision Records (ADR-0001 to ADR-0003)
- Docker strategy documentation
- Project vision and design philosophy

### Known Limitations
- No nested list support in current version
- Image embedding not yet implemented
- Table support planned for future release
- Hyperlink conversion not yet available

### Credits
- Built with DocumentFormat.OpenXml (Microsoft, MIT License)
- Powered by Markdig (Alexandre Mutel, BSD 2-Clause License)
- Fonts: Noto CJK (Google, SIL Open Font License)

---

## Release Notes

This is the **initial public release** (v0.1.0) of md2docx. The project provides a solid foundation for Markdown to Word conversion with extensive styling customization through YAML configuration.

The codebase has achieved high test coverage (90.1%) and follows modern C# best practices with full XML documentation. All public documentation is in English to support the international open-source community.

**Target Audience**: Developers, technical writers, content creators needing programmatic Word document generation from Markdown with precise styling control.

**Stability**: This is an early release. While thoroughly tested, users should validate output for their specific use cases. Feedback and contributions are welcome!

---

[0.1.0]: https://github.com/forest6511/md2docx/releases/tag/v0.1.0
