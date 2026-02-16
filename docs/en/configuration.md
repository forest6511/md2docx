---
layout: default
title: Configuration Guide
lang: en
---

# Configuration Guide

Learn how to customize document styling through YAML configuration files.

## Configuration File Structure

md2docx uses YAML files to define document styling. All configurations follow Schema Version 2.0.

### Configuration Keys

Top-level configuration keys (required):

SchemaVersion: Schema version identifier
Metadata: Preset metadata and information
TextDirection: Text flow direction (Horizontal or Vertical)
PageLayout: Page dimensions and margins
Fonts: Font family and size settings
Styles: Element-specific styling rules

### Basic Structure

```yaml
SchemaVersion: "2.0"

Metadata:
  Name: "My Custom Style"
  Description: "Custom styling for my documents"
  Author: "Your Name"
  Version: "1.0.0"

TextDirection: "Horizontal"  # or "Vertical" for Japanese tategaki

PageLayout:
  Width: 21.0      # A4 width in cm
  Height: 29.7     # A4 height in cm
  # ... margin settings

Fonts:
  Ascii: "Noto Serif"
  EastAsia: "Noto Serif CJK JP"
  DefaultSize: 11

Styles:
  H1:
    Size: 26
    Bold: true
    Color: "2c3e50"
    # ... additional properties
  # ... other element styles
```

## Configuration Sections

### Metadata

Descriptive information about the preset:

```yaml
Metadata:
  Name: "My Style"           # Display name
  Description: "Brief description of this style"
  Author: "Your Name"        # Creator name
  Version: "1.0.0"          # Semantic version
```

### Text Direction

Controls the flow of text in the document:

```yaml
TextDirection: "Horizontal"  # Left-to-right, top-to-bottom
# or
TextDirection: "Vertical"    # Top-to-bottom, right-to-left (Japanese tategaki)
```

**Important**: Vertical text is optimized for Japanese documents and novels.

### Page Layout

Define page dimensions and margins (all values in centimeters):

```yaml
PageLayout:
  Width: 21.0              # Page width
  Height: 29.7             # Page height
  MarginTop: 2.54          # Top margin
  MarginBottom: 2.54       # Bottom margin
  MarginLeft: 3.17         # Left margin
  MarginRight: 3.17        # Right margin
  MarginHeader: 1.27       # Header margin from edge
  MarginFooter: 1.27       # Footer margin from edge
  MarginGutter: 0          # Gutter (for binding)
```

**Common Page Sizes**:
- **A4**: 21.0 × 29.7 cm
- **Letter**: 21.59 × 27.94 cm
- **A5**: 14.8 × 21.0 cm (common for vertical novels)

### Fonts

Specify font families and default size:

```yaml
Fonts:
  Ascii: "Noto Serif"              # Font for ASCII characters
  EastAsia: "Noto Serif CJK JP"    # Font for Japanese/Chinese/Korean
  DefaultSize: 11                   # Default font size in points
```

**Available Fonts** (in Docker images):
- **Serif**: Noto Serif, Noto Serif CJK JP
- **Sans-serif**: Noto Sans, Noto Sans CJK JP
- **Monospace**: Noto Sans Mono, Noto Sans Mono CJK JP

**Note**: When using .NET CLI, ensure fonts are installed on your system or use Docker for consistent font handling.

### Styles

Define styling for each Markdown element.

#### Heading Styles (H1-H6)

```yaml
Styles:
  H1:
    Size: 26                    # Font size in points
    Bold: true                  # Bold text
    Italic: false               # Italic text (optional)
    Color: "2c3e50"            # Hex color (without #)
    ShowBorder: true            # Display border
    BorderColor: "3498db"       # Border color (hex)
    BorderSize: 6               # Border thickness in eighths of a point
    BorderPosition: "bottom"    # "top", "bottom", "left", "right"
    SpaceBefore: "600"          # Space before (in twips, 1/20 point)
    SpaceAfter: "300"           # Space after (in twips)
```

**Spacing Values** (twips = 1/20 point):
- `"240"` = 12pt spacing
- `"360"` = 18pt spacing
- `"480"` = 24pt spacing
- `"600"` = 30pt spacing

#### Paragraph Style

```yaml
Styles:
  Paragraph:
    Size: 11                    # Font size
    Color: "2c3e50"            # Text color
    LineSpacing: "360"          # Line spacing (twips)
    FirstLineIndent: "0"        # First line indent (twips)
    LeftIndent: "0"             # Left indent (twips)
    SpaceBefore: "0"            # Space before
    SpaceAfter: "180"           # Space after
```

**Line Spacing Examples**:
- `"240"` = Single spacing (1.0)
- `"360"` = 1.5 spacing
- `"480"` = Double spacing (2.0)

**Indent Examples**:
- `"0"` = No indent
- `"360"` = 0.5cm indent
- `"720"` = 1cm indent

#### List Style

```yaml
Styles:
  List:
    Size: 11                    # Font size
    Color: "2c3e50"            # Text color
    LeftIndent: "720"           # Left margin (twips)
    HangingIndent: "360"        # Hanging indent for bullets/numbers
    SpaceBefore: "60"           # Space before each item
    SpaceAfter: "60"            # Space after each item
```

#### Code Block Style

```yaml
Styles:
  CodeBlock:
    Size: 10                              # Font size
    Color: "2c3e50"                      # Text color
    BackgroundColor: "ecf0f1"            # Background color
    BorderColor: "bdc3c7"                # Border color
    MonospaceFontAscii: "Noto Sans Mono"           # Monospace font (ASCII)
    MonospaceFontEastAsia: "Noto Sans Mono CJK JP" # Monospace font (CJK)
    LineSpacing: "280"                   # Line spacing
    SpaceBefore: "300"                   # Space before
    SpaceAfter: "300"                    # Space after
    ShowBorder: true                     # Display border
    BorderSize: 4                        # Border thickness
```

#### Quote Block Style

```yaml
Styles:
  Quote:
    Size: 11                    # Font size
    Color: "7f8c8d"            # Text color
    Italic: true                # Italic text
    ShowBorder: true            # Display left border
    BorderColor: "3498db"       # Border color
    BorderSize: 16              # Border thickness
    BorderPosition: "left"      # Border position
    LeftIndent: "720"           # Left indent
    BackgroundColor: "f8f9fa"   # Background color
    SpaceBefore: "300"          # Space before
    SpaceAfter: "300"           # Space after
```

## Creating Custom Presets

### Step 1: Copy an Existing Preset

```bash
cp config/presets/default.yaml config/custom/my-style.yaml
```

### Step 2: Edit the Configuration

Modify the YAML file with your desired styling:

```yaml
SchemaVersion: "2.0"

Metadata:
  Name: "Corporate Style"
  Description: "Professional styling for business documents"
  Author: "Your Company"
  Version: "1.0.0"

TextDirection: "Horizontal"

PageLayout:
  Width: 21.0
  Height: 29.7
  # ... customize margins

Fonts:
  Ascii: "Noto Sans"
  EastAsia: "Noto Sans CJK JP"
  DefaultSize: 10

Styles:
  H1:
    Size: 22
    Bold: true
    Color: "003366"  # Corporate blue
    ShowBorder: true
    BorderColor: "003366"
    # ... additional properties
```

### Step 3: Use Your Custom Preset

**With custom YAML file:**

```bash
docker run --rm -v $(pwd):/workspace forest6511/md2docx:latest \
  input.md -o output.docx -c config/custom/my-style.yaml
```

**With preset name** (if placed in preset directory):

```bash
docker run --rm -v $(pwd):/workspace forest6511/md2docx:latest \
  input.md -o output.docx -p my-style --preset-dir /workspace/config/custom
```

## Color Reference

Colors are specified as hexadecimal values without the `#` prefix.

### Common Colors

| Color Name | Hex Value | Example |
|------------|-----------|---------|
| Black | `000000` | <span style="color: #000000">■</span> |
| Dark Gray | `333333` | <span style="color: #333333">■</span> |
| Medium Gray | `7f8c8d` | <span style="color: #7f8c8d">■</span> |
| Light Gray | `95a5a6` | <span style="color: #95a5a6">■</span> |
| Navy Blue | `2c3e50` | <span style="color: #2c3e50">■</span> |
| Dark Blue | `34495e` | <span style="color: #34495e">■</span> |
| Blue | `3498db` | <span style="color: #3498db">■</span> |
| Light Blue | `0066cc` | <span style="color: #0066cc">■</span> |

## Best Practices

### 1. Maintain Consistency

- Use the same font family throughout the document
- Keep color palette limited (2-3 main colors)
- Consistent spacing patterns

### 2. Readability First

- **Line Spacing**: Use at least 1.5x spacing (`"360"`) for paragraphs
- **Font Size**: 10-12pt for body text, larger for headings
- **Contrast**: Ensure sufficient contrast between text and background

### 3. Professional Appearance

- Avoid excessive borders and colors
- Use appropriate margins (2-3cm minimum)
- Consistent heading hierarchy

### 4. Test on Target Platform

- Generate sample documents and test in Word/LibreOffice
- Verify fonts render correctly
- Check page layout on printed output if needed

## Validation

Validate your YAML configuration before use:

```bash
# Using Python
python3 -c "import yaml; yaml.safe_load(open('config/custom/my-style.yaml'))"

# Using Ruby
ruby -e "require 'yaml'; YAML.load_file('config/custom/my-style.yaml')"
```

If no errors are shown, your YAML is valid.

## Troubleshooting

### Font Not Found

**Problem**: Generated document uses different fonts than specified

**Solutions**:
1. Use Docker images (fonts embedded)
2. Install Noto fonts on your system
3. Verify font names match exactly (case-sensitive)

### Border Not Showing

**Problem**: Border settings not visible in output

**Check**:
1. `ShowBorder: true` is set
2. `BorderSize` is sufficient (try `6` or higher)
3. `BorderColor` has valid hex value

### Spacing Issues

**Problem**: Spacing appears incorrect

**Check**:
1. Values are in twips (1/20 point)
2. Line spacing sufficient for font size
3. No negative values used

### YAML Syntax Errors

**Problem**: Configuration file not loading

**Common Issues**:
- Missing quotes around color values
- Incorrect indentation (use 2 spaces)
- Missing colons after keys
- Invalid YAML structure

**Validation**: Run YAML validation command (see above)

## Next Steps

- [Presets Reference](./presets) - Explore built-in presets
- [API Documentation](./api-reference) - Use md2docx as a library
- [GitHub Issues](https://github.com/forest6511/md2docx/issues) - Report issues

## Reference

### Complete Example

See `config/presets/default.yaml` for a complete, well-commented example configuration.

### Schema Version History

- **2.0** (Current): PascalCase keys for C# mapping, improved structure
- **1.0**: Initial schema (deprecated)

---

**Last Updated**: 2026-02-17
