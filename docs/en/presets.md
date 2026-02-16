---
layout: default
title: Presets Reference
lang: en
---

# Presets Reference

md2docx includes several built-in presets optimized for different use cases. This guide provides detailed information about each preset.

## Overview

| Preset | Directory | Best For | Text Direction |
|--------|-----------|----------|----------------|
| [minimal](#minimal) | `config/presets/` | Simple documents, quick conversions | Horizontal |
| [default](#default) | `config/presets/` | General-purpose documents | Horizontal |
| [technical](#technical) | `config/presets/` | Technical documentation, API docs | Horizontal |
| [vertical-novel](#vertical-novel) | `config/vertical/` | Japanese novels, traditional documents | Vertical |

## Standard Presets

### minimal

**Location**: `config/presets/minimal.yaml`

**Description**: Bare minimum styling with black and white color scheme. Ideal for straightforward Markdown to Word conversion without visual embellishments.

**Characteristics**:
- **Page Size**: A4 (21.0 × 29.7 cm)
- **Fonts**: Noto Serif / Noto Serif CJK JP
- **Base Font Size**: 11pt
- **Color Scheme**: Black and white only
- **Borders**: None
- **Background**: None

**Style Details**:

```yaml
H1: 24pt, Bold, Black
H2: 20pt, Bold, Black
H3: 16pt, Bold, Black
Paragraph: 11pt, 1.5 line spacing
Code Block: 10pt, Light gray background (#f5f5f5)
Quote: 11pt, Italic, Left border (#999999)
```

**Use Cases**:
- Quick document conversion
- Print-friendly output
- Minimal file size
- Academic papers (APA/MLA style)

**Example Usage**:

```bash
docker run --rm -v $(pwd):/workspace forest6511/md2docx:latest \
  input.md -o output.docx -p minimal --preset-dir /app/config/presets
```

---

### default

**Location**: `config/presets/default.yaml`

**Description**: Balanced styling suitable for most documents. Professional appearance with subtle colors and borders.

**Characteristics**:
- **Page Size**: A4 (21.0 × 29.7 cm)
- **Fonts**: Noto Serif / Noto Serif CJK JP
- **Base Font Size**: 11pt
- **Color Scheme**: Navy blue (#2c3e50) and gray tones
- **Borders**: Bottom borders for H1/H2, left border for quotes
- **Background**: Light gray for code blocks, light blue for quotes

**Style Details**:

```yaml
H1: 26pt, Bold, Navy (#2c3e50), Blue bottom border (#3498db)
H2: 22pt, Bold, Dark gray (#34495e), Gray bottom border (#95a5a6)
H3: 18pt, Bold, Dark gray (#34495e)
Paragraph: 11pt, 1.5 line spacing, Navy
Code Block: 10pt, Monospace, Light gray background (#ecf0f1)
Quote: 11pt, Italic, Gray (#7f8c8d), Blue left border (#3498db)
```

**Use Cases**:
- Business documents
- Proposals and reports
- Meeting minutes
- General documentation

**Example Usage**:

```bash
docker run --rm -v $(pwd):/workspace forest6511/md2docx:latest \
  input.md -o output.docx -p default --preset-dir /app/config/presets
```

---

### technical

**Location**: `config/presets/technical.yaml`

**Description**: Optimized for technical documentation with compact layout and code-friendly styling.

**Characteristics**:
- **Page Size**: A4 (21.0 × 29.7 cm)
- **Fonts**: Noto Sans / Noto Sans CJK JP (sans-serif for better readability)
- **Base Font Size**: 10pt (compact)
- **Color Scheme**: Dark gray (#1a1a1a) with blue accents
- **Borders**: Bottom borders for H1/H2, borders for code blocks
- **Background**: Light gray for code blocks and quotes

**Style Details**:

```yaml
H1: 24pt, Bold, Dark (#1a1a1a), Blue bottom border (#0066cc)
H2: 20pt, Bold, Dark gray (#333333), Gray bottom border (#4d4d4d)
H3: 16pt, Bold, Dark gray (#333333)
Paragraph: 10pt, 1.4 line spacing, Dark (#1a1a1a)
Code Block: 9pt, Monospace, Light gray background (#f5f5f5), Border
Quote: 10pt, Gray (#4d4d4d), Blue left border (#0066cc)
```

**Use Cases**:
- API documentation
- Software manuals
- Technical specifications
- Code-heavy documents
- README files

**Example Usage**:

```bash
docker run --rm -v $(pwd):/workspace forest6511/md2docx:latest \
  README.md -o README.docx -p technical --preset-dir /app/config/presets
```

---

## Vertical Text Presets

### vertical-novel

**Location**: `config/vertical/vertical-novel.yaml`

**Description**: Japanese vertical writing (tategaki) optimized for novel formatting. Traditional right-to-left, top-to-bottom text flow.

**Characteristics**:
- **Page Size**: A5 (14.8 × 21.0 cm) - common for Japanese novels
- **Text Direction**: Vertical (tategaki)
- **Fonts**: Noto Serif / Noto Serif CJK JP
- **Base Font Size**: 11pt
- **Color Scheme**: Black only
- **Borders**: None (clean, minimal design)
- **Line Spacing**: Wide (440 twips) for readability

**Style Details**:

```yaml
H1: 18pt, Bold, Black
H2: 16pt, Bold, Black
H3: 14pt, Bold, Black
Paragraph: 11pt, Wide line spacing (440 twips), Black
Code Block: 10pt, Monospace, Light gray background
Quote: 11pt, Dark gray (#333333)
```

**Use Cases**:
- Japanese novels
- Traditional Japanese documents
- Vertical text publications
- Literary works in Japanese

**Important Notes**:
- Optimized for Japanese text (Hiragana, Katakana, Kanji)
- Page size follows Japanese book standards
- Right-to-left reading direction
- Ideal for long-form text (novels, essays)

**Example Usage**:

```bash
docker run --rm -v $(pwd):/workspace forest6511/md2docx:latest \
  novel.md -o novel.docx -p vertical-novel --preset-dir /app/config/vertical
```

---

## Comparison Table

### Visual Characteristics

| Feature | minimal | default | technical | vertical-novel |
|---------|---------|---------|-----------|----------------|
| **Font Family** | Serif | Serif | Sans-serif | Serif |
| **Base Size** | 11pt | 11pt | 10pt | 11pt |
| **Colors** | B&W | Navy/Blue/Gray | Dark/Blue | Black |
| **H1 Borders** | No | Bottom | Bottom | No |
| **Code Blocks** | Gray BG | Gray BG | Gray BG + Border | Gray BG |
| **Quote Blocks** | Left Border | Left Border + BG | Left Border + BG | No Border |

### Layout Characteristics

| Feature | minimal | default | technical | vertical-novel |
|---------|---------|---------|-----------|----------------|
| **Page Size** | A4 | A4 | A4 | A5 |
| **Text Direction** | Horizontal | Horizontal | Horizontal | Vertical |
| **Margins** | Standard | Standard | Compact | Standard |
| **Line Spacing** | 1.5x | 1.5x | 1.4x | Wide (2.2x) |

### File Size (Approximate)

| Preset | Typical File Size* |
|--------|--------------------|
| minimal | ~12 KB |
| default | ~14 KB |
| technical | ~13 KB |
| vertical-novel | ~11 KB |

*For a 10-page document with standard Markdown content

## Choosing a Preset

### Decision Tree

```
Start Here
│
├─ Need vertical Japanese text?
│  └─ Yes → vertical-novel
│
├─ Technical documentation?
│  └─ Yes → technical
│
├─ Professional business document?
│  └─ Yes → default
│
└─ Simple, minimal styling?
   └─ Yes → minimal
```

### By Document Type

| Document Type | Recommended Preset |
|---------------|-------------------|
| Technical Documentation | technical |
| API Reference | technical |
| Business Proposal | default |
| Meeting Minutes | default |
| Project Report | default |
| Academic Paper | minimal |
| Quick Note | minimal |
| Japanese Novel | vertical-novel |
| Traditional Japanese Document | vertical-novel |

## Customizing Presets

All presets can be customized by copying and modifying the YAML files:

### 1. Copy a Preset

```bash
cp config/presets/default.yaml config/custom/my-style.yaml
```

### 2. Modify Settings

Edit the YAML file to adjust colors, fonts, spacing, etc. See the [Configuration Guide](./configuration) for detailed information.

### 3. Use Your Custom Preset

```bash
docker run --rm -v $(pwd):/workspace forest6511/md2docx:latest \
  input.md -o output.docx -c config/custom/my-style.yaml
```

## Best Practices

### Choosing Font Size

- **10pt**: Compact, technical documents
- **11pt**: Standard readability
- **12pt**: Enhanced readability, presentations

### Choosing Line Spacing

- **1.0x**: Very compact (not recommended)
- **1.4x**: Compact technical docs
- **1.5x**: Standard readability ✓
- **2.0x**: High readability, accessibility

### Color Schemes

**Professional**:
- Navy/Blue/Gray (default preset)
- Black/Dark Gray (minimal preset)

**Technical**:
- Dark Gray/Blue accents (technical preset)
- Monochrome with subtle highlights

**Creative**:
- Custom color palette (create your own)

## Troubleshooting

### Preset Not Found

**Problem**: `Preset 'xxx' not found`

**Solution**:
- Verify preset name matches file name (without `.yaml`)
- Ensure `--preset-dir` points to correct directory
- Use absolute path or relative path from current directory

**Example**:

```bash
# Correct
docker run --rm -v $(pwd):/workspace forest6511/md2docx:latest \
  input.md -o output.docx -p default --preset-dir /app/config/presets

# Also correct
docker run --rm -v $(pwd):/workspace forest6511/md2docx:latest \
  input.md -o output.docx -c /app/config/presets/default.yaml
```

### Fonts Not Rendering

**Problem**: Fonts appear different in output document

**Solution**:
- Use Docker images (fonts embedded) ✓
- Install Noto fonts on system (if using .NET CLI)
- Verify font names in YAML match available fonts

### Vertical Text Issues

**Problem**: Vertical text not displaying correctly

**Check**:
- Using `vertical-novel` preset or `TextDirection: "Vertical"` in custom config
- Document viewer supports vertical text (Microsoft Word, LibreOffice)
- Japanese fonts are available (Noto Serif CJK JP)

## Next Steps

- [Configuration Guide](./configuration) - Learn how to create custom styles
- [API Documentation](./api-reference) - Use md2docx as a library
- [GitHub Repository](https://github.com/forest6511/md2docx) - Source code and examples

## Reference Files

All preset files are available in the repository:

- `config/presets/minimal.yaml`
- `config/presets/default.yaml`
- `config/presets/technical.yaml`
- `config/vertical/vertical-novel.yaml`

View the complete source files for detailed styling specifications.
