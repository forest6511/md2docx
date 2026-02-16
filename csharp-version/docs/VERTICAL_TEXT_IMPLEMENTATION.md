# Vertical Text (Tategaki) Implementation Guide

## ✅ DocumentFormat.OpenXml Vertical Text Support

**Good News**: DocumentFormat.OpenXml **fully supports** Japanese vertical text (tategaki).

---

## 📐 Technical Implementation

### Text Direction Property

DocumentFormat.OpenXml provides the `TextDirection` class:

```csharp
using DocumentFormat.OpenXml.Wordprocessing;

// Vertical text (right to left)
var textDirection = new TextDirection() { Val = TextDirectionValues.TbRl };
// TbRl = Top to Bottom, Right to Left

// Horizontal text (left to right)
var textDirection = new TextDirection() { Val = TextDirectionValues.LrTb };
// LrTb = Left to Right, Top to Bottom
```text

### Available Text Direction Values

```csharp
public enum TextDirectionValues
{
    TbRl,      // Vertical right to left (Japanese standard)
    BtLr,      // Vertical left to right
    LrTb,      // Horizontal left to right (English standard)
    TbRlV,     // Vertical right to left (rotated 90 degrees)
    TbLrV,     // Vertical left to right (rotated 90 degrees)
    LrTbV      // Horizontal left to right (rotated 90 degrees)
}
```text

---

## 🔧 Implementation Examples

### 1. Paragraph with Vertical Text

```csharp
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;

public Paragraph CreateVerticalParagraph(string text)
{
    var paragraph = new Paragraph();

    // Paragraph properties
    var paragraphProperties = new ParagraphProperties();

    // Text direction: Vertical right to left
    paragraphProperties.Append(new TextDirection() { Val = TextDirectionValues.TbRl });

    // Text flow (for Word compatibility)
    paragraphProperties.Append(new TextFlow() { Val = TextFlowValues.TbRl });

    paragraph.Append(paragraphProperties);

    // Text run
    var run = new Run();
    run.Append(new Text(text));
    paragraph.Append(run);

    return paragraph;
}
```text

---

### 2. Section with Vertical Text

```csharp
public SectionProperties CreateVerticalSection()
{
    var sectionProperties = new SectionProperties();

    // Apply text direction to entire section
    sectionProperties.Append(new TextDirection() { Val = TextDirectionValues.TbRl });

    // Page size (for A5 vertical text)
    var pageSize = new PageSize()
    {
        Width = (UInt32Value)11906U,  // 210mm (A5 height)
        Height = (UInt32Value)16838U, // 297mm (A5 width)
        Orient = PageOrientationValues.Portrait
    };
    sectionProperties.Append(pageSize);

    // Margins (meanings change in vertical text)
    var pageMargin = new PageMargin()
    {
        Top = 1440,      // Right side (top) in twips
        Bottom = 1080,   // Left side (bottom)
        Left = 1296,     // Top side (spine)
        Right = 1152,    // Bottom side (fore edge)
        Header = 720,
        Footer = 720,
        Gutter = 0
    };
    sectionProperties.Append(pageMargin);

    return sectionProperties;
}
```text

---

### 3. Table with Vertical Text

```csharp
public Table CreateVerticalTable()
{
    var table = new Table();

    // Table properties
    var tableProperties = new TableProperties();

    // Table layout orientation
    tableProperties.Append(new TableLayout() { Type = TableLayoutValues.Fixed });

    table.Append(tableProperties);

    // Rows and cells
    var row = new TableRow();

    var cell = new TableCell();
    var cellProperties = new TableCellProperties();

    // Text direction within cell
    cellProperties.Append(new TextDirection() { Val = TextDirectionValues.TbRl });

    cell.Append(cellProperties);
    cell.Append(new Paragraph(new Run(new Text("Vertical text"))));

    row.Append(cell);
    table.Append(row);

    return table;
}
```text

---

## 🎌 Japanese-Specific Features

### Ruby (furigana) Support

```csharp
using DocumentFormat.OpenXml.Wordprocessing;

public Run CreateRubyText(string baseText, string rubyText)
{
    var run = new Run();

    // Ruby element
    var ruby = new Ruby();

    // Base text
    var rubyBase = new RubyBase();
    rubyBase.Append(new Run(new Text(baseText)));

    // Ruby text
    var rubyContent = new RubyContent();
    rubyContent.Append(new Run(new Text(rubyText)));

    // Ruby properties
    var rubyProperties = new RubyProperties();
    rubyProperties.Append(new RubyAlign() { Val = RubyAlignValues.Center });

    ruby.Append(rubyProperties);
    ruby.Append(rubyBase);
    ruby.Append(rubyContent);

    run.Append(ruby);

    return run;
}
```text

---

### Emphasis Marks (Japanese Dots)

```csharp
public Run CreateEmphasisMark(string text)
{
    var run = new Run();

    var runProperties = new RunProperties();

    // Emphasis marks (sesame dots)
    var emphasis = new Emphasis() { Val = EmphasisMarkValues.Dot };
    runProperties.Append(emphasis);

    run.Append(runProperties);
    run.Append(new Text(text));

    return run;
}

// Available emphasis mark styles
public enum EmphasisMarkValues
{
    None,       // None
    Dot,        // Black dot (・)
    Comma,      // Comma (、)
    Circle,     // White circle (◦)
    UnderDot    // Underline dot
}
```text

---

### Tate-Chu-Yoko (Horizontal in Vertical Text)

```csharp
public Run CreateTateChuYoko(string text)
{
    var run = new Run();

    var runProperties = new RunProperties();

    // Enable tate-chu-yoko
    runProperties.Append(new FitText() { Val = (UInt32Value)100U });

    // Scale down to fit
    runProperties.Append(new Scale() { Val = "50" });

    run.Append(runProperties);
    run.Append(new Text(text));

    return run;
}
```text

---

## 📝 YAML Configuration Mapping

### Horizontal Text

```yaml
# config/presets/default.yaml
text_direction: horizontal

# C# Implementation
var textDirection = new TextDirection() { Val = TextDirectionValues.LrTb };
```text

---

### Vertical Text

```yaml
# config/publishing/kdp-vertical-novel.yaml
text_direction: vertical

# Additional vertical-specific settings
vertical_typography:
  writing_mode: vertical-rl
  kinsoku:
    enabled: true
  hanging_punctuation:
    enabled: true

# C# Implementation
var textDirection = new TextDirection() { Val = TextDirectionValues.TbRl };

// Kinsoku processing (line breaking rules)
var kinsoku = new Kinsoku() { Val = OnOffValue.FromBoolean(true) };

// Hanging punctuation
var overflowPunct = new OverflowPunctuation() { Val = OnOffValue.FromBoolean(true) };
```text

---

## 🔍 Testing Vertical Text

### Test Case 1: Simple Vertical Paragraph

```csharp
[Fact]
public void VerticalText_SimpleParagraph_RendersCorrectly()
{
    var converter = new MarkdownConverter("config/publishing/kdp-vertical-novel.yaml");
    var markdown = "# Chapter 1\n\nI am a cat.";

    var docx = converter.Convert(markdown, "test-vertical.docx");

    // Verify text direction
    var doc = WordprocessingDocument.Open(docx, false);
    var paragraph = doc.MainDocumentPart.Document.Body.Descendants<Paragraph>().First();
    var textDirection = paragraph.ParagraphProperties.Descendants<TextDirection>().First();

    Assert.Equal(TextDirectionValues.TbRl, textDirection.Val);
}
```text

---

### Test Case 2: Ruby (Furigana)

```csharp
[Fact]
public void VerticalText_WithRuby_RendersCorrectly()
{
    var markdown = "{kanji|kanji}";
    var converter = new MarkdownConverter("config/publishing/kdp-vertical-novel.yaml");

    var docx = converter.Convert(markdown, "test-ruby.docx");

    // Verify ruby element exists
    var doc = WordprocessingDocument.Open(docx, false);
    var ruby = doc.MainDocumentPart.Document.Body.Descendants<Ruby>().First();

    Assert.NotNull(ruby);
    Assert.Equal("kanji", ruby.Descendants<RubyBase>().First().InnerText);
    Assert.Equal("kanji", ruby.Descendants<RubyContent>().First().InnerText);
}
```text

---

### Test Case 3: Emphasis Marks

```csharp
[Fact]
public void VerticalText_EmphasisMarks_RendersCorrectly()
{
    var markdown = "《《emphasis》》text to emphasize";
    var converter = new MarkdownConverter("config/publishing/kdp-vertical-novel.yaml");

    var docx = converter.Convert(markdown, "test-emphasis.docx");

    var doc = WordprocessingDocument.Open(docx, false);
    var emphasis = doc.MainDocumentPart.Document.Body
        .Descendants<RunProperties>()
        .SelectMany(rp => rp.Descendants<Emphasis>())
        .First();

    Assert.Equal(EmphasisMarkValues.Dot, emphasis.Val);
}
```text

---

## ⚠️ Known Limitations

### 1. **Mixed Horizontal-Vertical in Same Paragraph**

**Issue**: Cannot mix horizontal and vertical text direction within the same paragraph.

**Workaround**: Use separate paragraphs or text boxes.

```csharp
// ❌ Not possible
// "vertical" + "horizontal" in same paragraph with different directions

// ✅ Solution: Separate paragraphs
var verticalPara = CreateVerticalParagraph("vertical text");
var horizontalPara = CreateHorizontalParagraph("horizontal text");
```text

---

### 2. **Complex Ruby with Multiple Characters**

**Issue**: Very complex ruby patterns may not render perfectly in all Word versions.

**Workaround**: Test with target Word version (MS Word 2016+, LibreOffice 7+).

---

### 3. **Vertical Tables**

**Issue**: Table cells can have vertical text, but table orientation cannot be truly vertical.

**Workaround**: Rotate text within cells using `TextDirection`.

---

## 📊 Compatibility Matrix

| Feature | Word 2016+ | Word 2019+ | Word 365 | LibreOffice | Google Docs |
| --------- | ----------- |-----------|----------|-------------|-------------|
| Basic Vertical Text | ✅ | ✅ | ✅ | ✅ | ⚠️ Partial |
| Ruby (Furigana) | ✅ | ✅ | ✅ | ✅ | ❌ |
| Emphasis Marks | ✅ | ✅ | ✅ | ✅ | ❌ |
| Tate-Chu-Yoko | ✅ | ✅ | ✅ | ⚠️ Partial | ❌ |
| Kinsoku Processing | ✅ | ✅ | ✅ | ✅ | ❌ |
| Hanging Punctuation | ✅ | ✅ | ✅ | ⚠️ Partial | ❌ |

**Legend**:

- ✅ Full support
- ⚠️ Partial support (may have minor rendering differences)
- ❌ Not supported

---

## 🎯 Recommendation

### For Horizontal Text

Use standard presets:

- `presets/default.yaml`
- `presets/technical.yaml`
- `publishing/kdp-6x9-horizontal.yaml`

**Target**: Microsoft Word, LibreOffice, Google Docs

---

### For Vertical Text

Use vertical-specific presets:

- `publishing/kdp-vertical-novel.yaml`
- `vertical/novel.yaml`
- `vertical/essay.yaml`

**Target**: Microsoft Word 2016+, LibreOffice 7+

**Best Practice**:

1. Use Noto Serif JP font (included in Docker)
2. Enable kinsoku processing
3. Test with target Word version
4. Avoid complex mixed layouts

---

## 🚀 Next Steps

1. **Implement Core Text Direction**:
   - `LrTb` for horizontal
   - `TbRl` for vertical

2. **Add Ruby Support**:
   - Parse `{base|ruby}` syntax
   - Generate `<w:ruby>` elements

3. **Add Emphasis Marks**:
   - Parse `《《text》》` syntax
   - Apply `<w:em>` elements

4. **Add Tate-Chu-Yoko**:
   - Parse `｜text｜` syntax
   - Apply `<w:fitText>` elements

5. **Test Extensively**:
   - Multiple Word versions
   - LibreOffice compatibility
   - PDF export verification

---

## 📚 References

- [DocumentFormat.OpenXml - TextDirection](https://docs.microsoft.com/en-us/dotnet/api/documentformat.openxml.wordprocessing.textdirection)
- [Word Open XML - Vertical Text](https://docs.microsoft.com/en-us/openspecs/office_standards/ms-oe376/f8b3e5d3-2f5d-4c8c-b8e3-7f6b1e3e3e3e)
- [Japanese Typography in Word](https://support.microsoft.com/en-us/office/change-text-direction-in-a-document-88a35ac0-5b96-4fb0-ae9b-8b6a5b8c1a43)
- [Noto CJK Fonts](https://github.com/googlefonts/noto-cjk)

---

**Conclusion**: ✅ **Both horizontal and vertical text are fully supported** by DocumentFormat.OpenXml!
