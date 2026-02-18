# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.2.1] - 2026-02-18

### Improved

#### Test Coverage
- Added **27 new tests** (total: 187, up from 160)
- `ImageDimensionReader`: 9 edge case tests covering truncated files, invalid signatures, progressive JPEG, FF padding, and truncated segments
- `YamlConfigurationLoader`: 10 validation tests covering all `ValidateConfiguration` branches (invalid YAML, empty schema version, missing metadata, zero page dimensions, missing fonts)
- `OpenXmlDocumentBuilder`: 8 tests for remaining uncovered branches (right/top/all border positions, quote ShowBorder=false, quote BackgroundColor, heading LineSpacing, BorderExtent spacer skip)

#### Build Quality
- Resolved all build warnings (previously ~62 warnings, now 0)
- Fixed CA1305: Added `CultureInfo.InvariantCulture` to `int.ToString()` in `OpenXmlDocumentBuilder`
- Suppressed CA1805 (explicit default initializers) globally — intentional for API documentation clarity
- Added test-specific suppressions for CS8602, CA1806, CA1816, CA1507 in test project

---

## [0.2.0] - 2026-02-18

### Added

#### Cover Page / Title Page
- **Cover image support** with centered display (horizontal and vertical centering via section break)
- CLI option `--cover-image <file>` to specify cover image (overrides YAML, implicitly enables title page)
- YAML `TitlePage` configuration: `Enabled`, `ImagePath`, `ImageMaxWidthPercent`, `ImageMaxHeightPercent`, `PageBreakAfter`
- Cross-platform image dimension reader for PNG (IHDR chunk) and JPEG (SOF0/SOF2 markers) without System.Drawing dependency

#### Table of Contents
- **Auto-generated TOC** with configurable depth (1-6 levels) and custom title
- TOC field codes compatible with Word field update workflow
- Removed `\h` and `\z` switches from TOC field for KDP PDF compatibility
- English placeholder text guiding users to update the field
- YAML `TableOfContents` configuration: `Enabled`, `Depth`, `Title`, `PageBreakAfter`

#### Heading Enhancements
- **Outline level** on headings for TOC and Word navigation pane support
- **PageBreakBefore** property for chapter-style page breaks
- **BorderExtent** property: `"text"` (border hugs text) or `"paragraph"` (border spans full width)

#### Quality Infrastructure
- 3-layer C# code quality automation (dotnet format + analyzers on pre-push hook)
- Test suite expanded from 82 to **160 tests** (all passing)

### Fixed
- TOC was not generated despite all infrastructure being complete (CLI wiring bug)
- dotnet format warnings (IDE0270, CA1866) resolved

### Known Limitations
- No inline image support (only cover/title page images)
- No nested list support
- Table support planned for future release

---

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

### Known Limitations (v0.1.0)
- No nested list support in current version
- Image embedding not yet implemented (added in v0.2.0)
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

[0.2.1]: https://github.com/forest6511/md2docx/releases/tag/v0.2.1
[0.2.0]: https://github.com/forest6511/md2docx/releases/tag/v0.2.0
[0.1.0]: https://github.com/forest6511/md2docx/releases/tag/v0.1.0
