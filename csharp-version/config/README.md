# Configuration Files Guide

This directory contains various configuration files for the Markdown to Word Converter.

## 📁 Directory Structure

```text
config/
├── presets/           # General-purpose presets
│   ├── minimal.yaml       # Minimal styling (AS-IS conversion)
│   ├── default.yaml       # Standard style (recommended)
│   ├── business.yaml      # Business documents
│   ├── technical.yaml     # Technical documentation
│   └── blog.yaml          # Blog posts
│
├── custom/            # User custom presets (add freely)
│   └── .gitkeep
│
├── examples/          # Samples and references
│   └── styling-options-reference.yaml  # Complete options reference
│
└── README.md          # This file
```text

## 🎯 Preset Selection Guide

### Simple Conversion

```bash
# Minimal styling conversion
md2word input.md -o output.docx --preset minimal
```text

**Use case**: Convert Markdown to Word format with minimal styling

---

### General Documents

```bash
# Balanced standard style
md2word input.md -o output.docx --preset default
```text

**Use case**: Regular documents, reports, articles

---

### Business Documents

```bash
# Professional business style
md2word proposal.md -o proposal.docx --preset business
```text

**Use cases**:

- Proposals
- Reports
- Meeting minutes
- Presentation materials

---

### Technical Documentation

```bash
# Optimized for technical documents
md2word README.md -o README.docx --preset technical
```text

**Use cases**:

- API specifications
- Technical manuals
- README documents
- Development documentation

---

## 🛠️ Creating Custom Configurations

### Step 1: Copy a Sample

```bash
# Copy an existing preset
cp config/presets/default.yaml config/custom/my-style.yaml
```text

### Step 2: Edit

```yaml
# my-style.yaml

# Page settings
page:
  size: A4
  margins:
    top: 30mm
    bottom: 30mm
    left: 25mm
    right: 25mm

# Font
font:
  family: "Times New Roman"
  size: 12pt

# Styles
styles:
  h1:
    size: 24pt
    bold: true
    color: "FF0000"  # Change to red
    # ... customize freely
```text

### Step 3: Use

```bash
md2word input.md -o output.docx --config config/custom/my-style.yaml
```text

## 📖 Configuration File Structure

### Basic Structure

```yaml
# Mode setting
mode: standard  # minimal / standard / rich / custom

# Page settings
page:
  size: A4  # A4, A5, Letter, Legal, custom
  orientation: portrait  # portrait / landscape
  margins:
    top: 25mm
    bottom: 25mm
    left: 25mm
    right: 25mm

# Font settings
font:
  family: "Times New Roman"
  size: 11pt
  embed: false

# Text direction
text_direction: horizontal  # horizontal / vertical

# Style definitions
styles:
  h1:
    size: 20pt
    bold: true
    color: "000000"
    # ... other properties

  paragraph:
    size: 11pt
    line_spacing: 1.5
    # ...

  # Other elements...
```text

### Available Style Properties

See `examples/styling-options-reference.yaml` for details.

Main properties:

- **Text**: `size`, `bold`, `italic`, `color`, `underline`, `strike`
- **Borders**: `border` (top/bottom/left/right/all)
- **Background**: `background`, `shading`
- **Spacing**: `spacing_before`, `spacing_after`, `line_spacing`
- **Alignment**: `alignment` (left/center/right/both)
- **Indent**: `first_line_indent`, `hanging_indent`
- **Padding**: `padding`

## 🔍 Complete Reference

### All Options List

All styling options are documented in `examples/styling-options-reference.yaml`.

```bash
# View reference
cat config/examples/styling-options-reference.yaml
```text

## 💡 Tips

### 1. Batch Convert Multiple Files

```bash
# Convert multiple files with same preset
md2word chapters/*.md -o book.docx --preset default
```text

### 2. Preset Inheritance

Inherit from existing preset in custom configuration:

```yaml
# custom/my-style.yaml
inherit: presets/default.yaml

# Override specific parts
styles:
  h1:
    color: "FF0000"  # Only change heading color
```text

### 3. Environment Variable Configuration

```bash
# Specify default preset via environment variable
export MD2WORD_PRESET=technical
md2word input.md -o output.docx
```text

## 🤝 Community Presets

User-contributed presets will be placed in `custom/community/`:

```text
custom/
└── community/
    ├── academic-paper.yaml     # Academic papers
    ├── magazine-article.yaml   # Magazine articles
    ├── cookbook.yaml           # Recipe books
    └── ...
```text

Preset contributions are welcome!

## 📚 Related Documentation

- [Project Vision](../PROJECT_VISION.md) - Overall project direction
- [Styling Reference](examples/styling-options-reference.yaml) - Complete options details

## ❓ FAQ

### Q: Which preset should I use?

A: Depends on your use case:

- **Keep it simple**: `minimal`
- **General documents**: `default` (recommended)
- **Business documents**: `business`
- **Technical content**: `technical`

### Q: Where to save custom configurations?

A: Save them in the `config/custom/` directory.
Not tracked by Git, so you can create your own configurations freely.

### Q: Preset preview?

A: Conversion samples for each preset will be added to `examples/` (coming soon).

## 🚀 Next Steps

1. Choose a preset that fits your use case
2. Try converting
3. Customize as needed
4. Save to `custom/` for reuse

Happy converting! 🎉
