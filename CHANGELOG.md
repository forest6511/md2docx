# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [Unreleased]

### Planned
- Improve test coverage to 80%+ (currently 64.5%)
- Add XML documentation comments for public APIs
- Docker Hub publication (multi-arch: amd64, arm64)
- Slim Docker image variant (<300MB)

---

## [0.1.0] - 2026-02-16

### 🎉 Initial Release

First public release of md2docx - a flexible Markdown to Word converter.

### Added

#### Core Features
- **Markdown to DOCX conversion** using Markdig parser
  - Full CommonMark support with extensions
  - Tables, strikethrough, task lists
  - Headings (H1-H6), paragraphs, lists, code blocks, quotes

- **YAML-based configuration system**
  - Hierarchical styling (fonts, page layout, element styles)
  - 4 built-in presets: minimal, default, technical, vertical-novel
  - Schema version 1.0 with validation

- **Text direction support**
  - Horizontal text (standard left-to-right)
  - Vertical text (Japanese tategaki/縦書き)
  - Automatic page orientation adjustment

- **Docker distribution**
  - Pre-built image with embedded fonts (Noto CJK)
  - Cross-platform support (macOS, Linux, Windows)
  - Volume mount for easy file conversion

- **CLI interface**
  - Simple command: `md2docx input.md -o output.docx`
  - Preset selection: `-c config.yaml`
  - Help system and usage examples

#### Documentation
- Comprehensive README with quick start guide
- 3 Architecture Decision Records (ADRs):
  - ADR-0001: Markdig Parser Selection
  - ADR-0002: YAML Schema Design
  - ADR-0003: Vertical Text Implementation
- YAML preset examples with inline comments
- Docker usage documentation

#### Quality
- 49 unit and integration tests (100% passing)
- Build successful with 0 errors
- Test coverage: 64.5% overall
  - Core: 53.6%
  - Styling: 87.2%

### Technical Details

**Technology Stack**:
- Language: C# (.NET 8.0)
- Markdown: Markdig (CommonMark + extensions)
- Word Generation: DocumentFormat.OpenXml (Microsoft official)
- Configuration: YamlDotNet
- Fonts: Noto Serif/Sans CJK JP (SIL OFL license)

**Architecture**:
```
Markdown → Markdig Parser → Styling Engine (YAML) → OpenXml → DOCX
```

**Modules**:
- `MarkdownToDocx.Core`: Markdown parsing and document building
- `MarkdownToDocx.Styling`: YAML configuration and style application
- `MarkdownToDocx.CLI`: Command-line interface
- `MarkdownToDocx.Tests`: Unit and integration tests

### Known Limitations

- Test coverage below 80% target (improvement planned)
- Docker image size: 560MB (optimization planned)
- XML documentation warnings: 54 (documentation planned)
- No GUI (CLI only for v0.1.0)

### Contributors

- Development: forest6511
- AI Assistance: Claude Sonnet 4.5 (code implementation)
- Project Management: AI-driven autonomous workflow

---

## Release Notes

### Installation

**Docker (Recommended)**:
```bash
docker pull forest6511/md2docx:0.1.0
docker run --rm -v $(pwd):/workspace forest6511/md2docx:0.1.0 input.md -o output.docx
```

**From Source**:
```bash
git clone https://github.com/forest6511/md2docx.git
cd md2docx/csharp-version/src
dotnet build
dotnet run --project MarkdownToDocx.CLI -- input.md -o output.docx
```

### Upgrade Notes

This is the initial release - no upgrade path needed.

### Breaking Changes

None - initial release.

### Deprecations

None - initial release.

---

## Future Roadmap

### v0.2.0 (Planned)
- Test coverage ≥80%
- XML documentation complete
- Docker image optimization (<300MB)
- Performance benchmarks (100+ page documents)

### v0.3.0 (Planned)
- GUI application (Electron-based)
- VSCode extension
- Additional presets (academic, business, resume)
- Custom font support

### v1.0.0 (Long-term)
- Plugin system for custom Markdown extensions
- Web-based converter (WASM)
- Multi-language support (i18n)
- Advanced typography features

---

[Unreleased]: https://github.com/forest6511/md2docx/compare/v0.1.0...HEAD
[0.1.0]: https://github.com/forest6511/md2docx/releases/tag/v0.1.0
