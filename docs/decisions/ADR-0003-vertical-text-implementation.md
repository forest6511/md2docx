# ADR-0003: Vertical Text Implementation Approach

**Date**: 2026-02-12
**Status**: Accepted
**Decided by**: system-architect agent + csharp-dev agent

---

## Context

**Problem Statement**:

- Need support for Japanese vertical text (tategaki)
- Novel and document publishing requires vertical layout
- DocumentFormat.OpenXml API for vertical text is complex
- Must maintain same YAML configuration simplicity

**Current Situation**:

- No vertical text support in Markdown ecosystem
- Most Markdown → Word converters only support horizontal text
- Japanese publishers require vertical text for novels, essays

---

## Decision

Implement **text direction as pluggable providers** with horizontal and vertical implementations, controlled by single YAML parameter.

**Key Points**:

- `ITextDirectionProvider` interface for text direction logic
- `HorizontalTextProvider` for standard left-to-right, top-to-bottom
- `VerticalTextProvider` for Japanese right-to-left, top-to-bottom (tategaki)
- YAML parameter: `text_direction: horizontal` or `vertical`
- Provider handles page configuration (orientation, margins) and paragraph settings

**Scope**:

- Covers: Page orientation, text flow direction, paragraph direction
- Not covered: Vertical-specific punctuation positioning (future enhancement)

---

## Consequences

### Positive

- ✅ Clean abstraction via ITextDirectionProvider interface
- ✅ Simple YAML switch (`text_direction: vertical`)
- ✅ No code changes needed for new text directions
- ✅ DocumentFormat.OpenXml fully supports vertical text
- ✅ Enables Japanese novel publishing use case
- ✅ Page layout automatically adjusted (landscape for vertical)

### Negative

- ❌ Vertical text testing requires Japanese language knowledge
- ❌ Additional complexity in DocumentBuilder
- ❌ Margin calculations differ between horizontal/vertical

### Neutral

- ℹ️ Two provider implementations to maintain
- ℹ️ Text direction affects font metrics (line height, spacing)

---

## Alternatives Considered

### Alternative 1: Hardcoded Vertical Support

**Description**: Add if-statements throughout DocumentBuilder

```csharp
if (isVertical)
{
    // Vertical-specific code
}
else
{
    // Horizontal-specific code
}
```

**Pros**:

- Simple initial implementation
- No abstraction overhead

**Cons**:

- Violates Single Responsibility Principle
- Difficult to test in isolation
- Future text directions (e.g., right-to-left Arabic) require more if-statements
- Code becomes unmaintainable

**Why rejected**: Poor maintainability and extensibility

---

### Alternative 2: Separate VerticalDocumentBuilder

**Description**: Create separate builder class for vertical text

**Pros**:

- Complete separation of concerns
- No shared code complexity

**Cons**:

- Massive code duplication (90% overlap)
- Inconsistent behavior risk
- Double maintenance burden
- User confusion (which builder to use?)

**Why rejected**: Code duplication violates DRY principle

---

### Alternative 3: CSS-like Writing Mode

**Description**: Implement CSS `writing-mode` property emulation

**Pros**:

- Familiar to web developers
- Standardized concept

**Cons**:

- CSS writing-mode has many values (horizontal-tb, vertical-rl, vertical-lr, etc.)
- OpenXML API doesn't map cleanly to CSS model
- Overengineering for current needs (only horizontal/vertical needed)

**Why rejected**: Unnecessary complexity for binary choice (horizontal vs vertical)

---

## Implementation

**Files affected**:

- `csharp-version/src/MarkdownToDocx.Core/Interfaces/ITextDirectionProvider.cs` - Provider interface
- `csharp-version/src/MarkdownToDocx.Core/TextDirection/HorizontalTextProvider.cs` - Horizontal implementation
- `csharp-version/src/MarkdownToDocx.Core/TextDirection/VerticalTextProvider.cs` - Vertical implementation
- `csharp-version/src/MarkdownToDocx.Core/Models/TextDirectionMode.cs` - Enum definition
- `csharp-version/src/MarkdownToDocx.Core/Models/PageConfiguration.cs` - Page settings
- `csharp-version/src/MarkdownToDocx.Core/Models/ParagraphConfiguration.cs` - Paragraph direction
- `config/vertical/vertical-novel.yaml` - Vertical text preset
- `csharp-version/tests/MarkdownToDocx.Tests/Unit/TextDirectionProviderTests.cs` - 7 unit tests
- `csharp-version/tests/MarkdownToDocx.Tests/Integration/MarkdownToDocxIntegrationTests.cs` - Integration test

**Code references**:

```csharp
public interface ITextDirectionProvider
{
    TextDirectionMode Mode { get; }
    PageConfiguration GetPageConfiguration();
    ParagraphConfiguration GetParagraphConfiguration();
}

public class VerticalTextProvider : ITextDirectionProvider
{
    public TextDirectionMode Mode => TextDirectionMode.Vertical;

    public PageConfiguration GetPageConfiguration()
    {
        return new PageConfiguration
        {
            Orientation = PageOrientation.Landscape,  // Rotated for vertical
            Width = 297mm,  // A4 height becomes width
            Height = 210mm, // A4 width becomes height
            Margins = new Margins
            {
                Top = 20mm,    // Right margin in vertical
                Bottom = 20mm, // Left margin in vertical
                Left = 25mm,   // Top margin in vertical
                Right = 25mm   // Bottom margin in vertical
            }
        };
    }

    public ParagraphConfiguration GetParagraphConfiguration()
    {
        return new ParagraphConfiguration
        {
            TextDirection = TextDirection.TopToBottomRightToLeft,
            VerticalAlignment = VerticalAlignment.Top
        };
    }
}
```

**YAML Usage**:

```yaml
# vertical-novel.yaml
version: "1.0"
text_direction: vertical  # Single parameter switch

fonts:
  body: "Noto Serif CJK JP"

page:
  size: A4  # Automatically rotated to landscape

styles:
  paragraph:
    line_spacing: 1.8  # Wider for vertical reading
```

**Migration path**:

No migration needed (initial implementation).

**Rollback plan**:

If vertical text proves problematic:
1. Set `text_direction: horizontal` in all presets (instant rollback)
2. ITextDirectionProvider interface allows disabling vertical support
3. Existing horizontal implementation unaffected

---

## Validation

**Success Criteria**:
- [x] Horizontal text works as before
- [x] Vertical text renders correctly in Word
- [x] Page orientation auto-switches (portrait/landscape)
- [x] Margins calculated correctly for both modes
- [x] All 7 unit tests passing
- [x] Integration test validates vertical conversion

**Testing Strategy**:
- Unit tests: Both providers return correct configurations
- Integration tests: Vertical-novel.yaml → DOCX conversion
- Manual testing: Open DOCX in Microsoft Word, verify vertical rendering

**Monitoring**:
- Vertical preset usage metrics
- Rendering issues reported by Japanese users

---

## Related Decisions

**Supersedes**:
- None (initial decision)

**Related to**:
- ADR-0001: Markdig Parser Selection (parser output is direction-agnostic)
- ADR-0002: YAML Schema Design (uses same YAML configuration)

**Depends on**:
- None

---

## References

**Documentation**:
- [OpenXML Vertical Text Tutorial](https://learn.microsoft.com/en-us/office/open-xml/word-processing/working-with-vertical-text)
- [Japanese Vertical Writing (Tategaki)](https://en.wikipedia.org/wiki/Horizontal_and_vertical_writing_in_East_Asian_scripts)

**Issues/PRs**:
- None (initial implementation)

**Research**:
- [CSS Writing Modes Level 4](https://www.w3.org/TR/css-writing-modes-4/)
- [Japanese Typography Best Practices](https://www.w3.org/TR/jlreq/)

---

## Timeline

| Date | Event |
|------|-------|
| 2026-02-12 | Decision proposed (provider pattern) |
| 2026-02-12 | Approved by architect and dev team |
| 2026-02-15 | Implementation started |
| 2026-02-15 | HorizontalTextProvider completed |
| 2026-02-15 | VerticalTextProvider completed |
| 2026-02-15 | 7 unit tests passing |
| 2026-02-15 | vertical-novel.yaml preset created |
| 2026-02-15 | Integration test validates vertical conversion |
| 2026-02-16 | Validated via Docker test |

---

## Notes

**Key Insight**: Text direction is fundamentally about **page layout and paragraph configuration**, not content. By abstracting this as providers, we achieved clean separation and eliminated conditional logic throughout the codebase.

**Vertical Text Challenges Discovered**:
1. Margins must be remapped (top → right, right → bottom, etc.)
2. Page orientation must switch (portrait → landscape)
3. Line spacing affects column width in vertical mode

**Future Enhancements**:
1. Right-to-left text support (Arabic, Hebrew) - requires new RTL provider
2. Vertical punctuation positioning (currently uses horizontal rules)
3. Mixed horizontal/vertical sections (advanced typography)

**Testing Notes**:
Vertical text validated with actual Japanese content. Rendering in Microsoft Word matches native vertical documents created manually.

---

**Last Updated**: 2026-02-16
**Next Review**: 2026-06-01 (quarterly review)
