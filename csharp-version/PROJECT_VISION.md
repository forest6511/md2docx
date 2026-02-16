# Markdown to Word Converter - Project Vision

## 🎯 Project Purpose

### A General-Purpose Markdown → Word Conversion OSS Tool

This project aims to create a flexible and powerful Markdown to Word conversion tool that serves **all use cases**.

## 📋 Core Concepts

### 1. General Purpose First

- **Primary Goal**: Convert Markdown files to high-quality Word documents for any purpose
- **Target Users**:
  - Technical writers
  - Document creators
  - Bloggers
  - Researchers
  - Anyone who works with Markdown

### 2. Flexible Configuration

- **Complete Customization via YAML**
  - Fonts, sizes, colors
  - Borders, underlines, backgrounds
  - Margins, line spacing, character spacing
  - Styling for tables, code blocks, and quotes
- **Preset Configurations Provided**
  - Simple conversion (AS-IS)
  - Business documents
  - Technical documentation
  - Academic papers
  - Blog posts

## 🏗️ Architecture

### Core Features

```text
┌─────────────────────────────────────────┐
│   Markdown Parser                       │
│   - Standard Markdown                   │
│   - Extensions (tables, code, etc.)     │
└─────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────┐
│   Styling Engine (YAML Driver)          │
│   - Configuration file loading          │
│   - Style application logic             │
│   - Custom rule processing              │
└─────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────┐
│   Word Generator (DocumentFormat.OpenXml)│
│   - DOCX generation                     │
│   - Style definitions                   │
│   - Horizontal/vertical text support    │
└─────────────────────────────────────────┘
```text

### Configuration File Structure

```text
config/
│
├── presets/           # Preset configurations
│   ├── minimal.yaml        # Minimal styling
│   ├── default.yaml        # Standard styling
│   ├── business.yaml       # Business documents
│   ├── technical.yaml      # Technical documentation
│   ├── academic.yaml       # Academic papers
│   └── blog.yaml           # Blog posts
│
└── custom/            # User custom
    └── my-style.yaml       # Free customization
```text

## 🎨 Styling Flexibility Levels

### Level 1: AS-IS Conversion (Minimal)

```yaml
# minimal.yaml
mode: minimal
# Convert with default Word styles
# No decoration, simple
```text

### Level 2: Standard Style

```yaml
# default.yaml
mode: standard
# Moderate decoration
# - Heading font sizes
# - Table borders
# - Code block backgrounds
```text

### Level 3: Rich Style

```yaml
# rich.yaml
mode: rich
# Rich decoration
# - Headings with underlines/borders
# - Tables with striping
# - Code blocks with syntax highlighting style
# - Quotes with side bars
```text

### Level 4: Fully Custom

```yaml
# custom.yaml
mode: custom
# User-defined everything
# - All element styles freely customizable
# - Borders, backgrounds, fonts, spacing, etc.
```text

## 🌍 Example Use Cases

### 1. Technical Documentation

```yaml
# technical.yaml
- Emphasized code blocks
- Specification tables
- Warning/note boxes
- API specifications
- README → Word conversion
```text

### 2. Business Documents

```yaml
# business.yaml
- Proposals
- Reports
- Meeting minutes
- Presentation documents
```text

### 3. Academic Papers

```yaml
# academic.yaml
- Citation management
- Figure/table numbering
- Footnotes/endnotes
- Equations (limited support)
```text

### 4. Blog Posts

```yaml
# blog.yaml
- Save Markdown articles in Word format
- Share with editors
- Backup purposes
```text

### 5. Novels/Essays (Vertical Text)

```yaml
# novel.yaml
- Japanese vertical text
- Ruby (furigana)
- Emphasis marks
- Dialog formatting
```text

## 🔧 Usage Examples

### Basic Usage

```bash
# Minimal conversion
md2word input.md -o output.docx

# Use preset
md2word input.md -o output.docx --preset default

# Custom configuration
md2word input.md -o output.docx --config my-style.yaml

# Batch convert multiple files
md2word chapters/*.md -o book.docx
```text

### Technical Documentation

```bash
md2word README.md -o README.docx --preset technical
```text

### Blog Post Archiving

```bash
md2word blog-post.md -o blog-post.docx --preset blog
```text

### Novel Writing (Vertical Text)

```bash
md2word novel-chapter1.md -o chapter1.docx --preset novel
```text

## 🚀 Extensibility

### Plugin System (Future)

```yaml
plugins:
  - syntax-highlighting  # Code syntax highlighting
  - math-equations       # Math support
  - diagram-support      # Diagrams (Mermaid, etc.)
  - citation-manager     # Citation management
```text

### Custom Markdown Syntax

```markdown
<!-- Standard Markdown -->
# Heading
**Bold**
`Code`

<!-- Extensions -->
{Kanji|かんじ}  <!-- Ruby -->
《《Emphasis》》    <!-- Emphasis marks -->
｜30｜         <!-- Tate-chu-yoko -->
:::warning
Warning content
:::            <!-- Custom boxes -->
```text

## 📦 Distribution Methods

### 1. CLI Tool

```bash
dotnet tool install -g markdown-to-word
md2word input.md -o output.docx
```text

### 2. Docker Image

```bash
docker run -v $(pwd):/workspace markdown-to-word \
  input.md -o output.docx
```text

### 3. Library (NuGet)

```csharp
using MarkdownToDocx;

var converter = new MarkdownConverter();
converter.LoadConfig("custom.yaml");
converter.Convert("input.md", "output.docx");
```text

### 4. Web API (Future)

```bash
curl -X POST https://api.md2word.example.com/convert \
  -F "file=@input.md" \
  -F "preset=default" \
  -o output.docx
```text

## 💡 Design Philosophy

### 1. Keep It Simple

- Works with minimal configuration by default
- Complex configurations available when needed

### 2. Intuitive Configuration

- YAML format for readability
- Rich commented samples
- Helpful error messages

### 3. Extensibility

- Easy with presets
- Flexible with customization
- Unlimited with plugins

### 4. Quality Focus

- High-quality Word document generation
- Accurate style reproduction
- Robust error handling

## 🎉 Summary

This project is:

✅ **General-purpose Markdown → Word conversion tool** (primary goal)
✅ **Fully customizable via YAML**
✅ **Supports all use cases** (technical docs, business, academic, etc.)
✅ **Open source** (MIT License)
✅ **Extensible** (plugins, preset additions)

**Not a specialized tool, but a general-purpose converter for everyone.**
