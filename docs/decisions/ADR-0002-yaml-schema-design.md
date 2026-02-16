# ADR-0002: YAML Schema Design

**Date**: 2026-02-11
**Status**: Accepted
**Decided by**: system-architect agent

---

## Context

**Problem Statement**:

- Users need flexible styling control for Word documents
- Configuration must be human-readable and maintainable
- Support multiple presets (minimal, default, technical, vertical, etc.)
- Backward compatibility for schema evolution

**Current Situation**:

- No existing configuration system
- Styling hardcoded in code is not acceptable for general-purpose tool
- Need balance between simplicity and power

---

## Decision

Use **YAML-based schema** for all styling configuration with hierarchical structure and preset system.

**Key Points**:

- YAML for human-readability and comments support
- Hierarchical schema: Fonts → Page Layout → Styles (Heading/Paragraph/List/Code/Quote)
- Preset directory system for multiple configurations
- Schema versioning for backward compatibility

**Scope**:

- Covers: All styling aspects (fonts, colors, spacing, borders, backgrounds)
- Not covered: Markdown parsing rules, custom Markdown extensions

---

## Consequences

### Positive

- ✅ User-friendly configuration format
- ✅ Comments allowed in preset files (documentation inline)
- ✅ Easy to create and share custom presets
- ✅ Version control friendly (text-based, diff-able)
- ✅ Supports validation through schema
- ✅ No compilation needed for style changes

### Negative

- ❌ YAML parsing overhead (minimal, one-time at load)
- ❌ Validation errors possible if users write invalid YAML
- ❌ Schema evolution requires migration strategy

### Neutral

- ℹ️ Requires YamlDotNet library dependency
- ℹ️ Schema documentation maintenance needed

---

## Alternatives Considered

### Alternative 1: JSON Configuration

**Description**: Use JSON instead of YAML

**Pros**:

- Native .NET serialization support
- Strict syntax (less ambiguity)
- Widely used in .NET ecosystem

**Cons**:

- No comments support (critical for preset documentation)
- Less human-readable (especially nested structures)
- JSON syntax verbose (brackets, quotes)

**Why rejected**: Lack of comments is dealbreaker for preset documentation

---

### Alternative 2: TOML Configuration

**Description**: Use TOML format

**Pros**:

- Human-readable
- Comments supported
- Popular in Rust/Go ecosystems

**Cons**:

- Less familiar to most users
- Limited .NET library support
- Table syntax can be confusing for deep nesting

**Why rejected**: YAML more familiar to target audience (developers, publishers)

---

### Alternative 3: C# Code Configuration (Fluent API)

**Description**: Configure styles programmatically

```csharp
config.Headings.H1
    .FontSize(24)
    .Bold()
    .Color("#2C3E50");
```

**Pros**:

- Compile-time validation
- IDE autocomplete support
- Type-safe

**Cons**:

- Requires C# knowledge
- Recompilation needed for changes
- Not user-friendly for non-developers
- Cannot be shared as simple files

**Why rejected**: Target users include non-developers (writers, publishers)

---

## Implementation

**Files affected**:

- `csharp-version/src/MarkdownToDocx.Styling/Configuration/YamlConfigurationLoader.cs` - YAML loader
- `csharp-version/src/MarkdownToDocx.Styling/Models/ConversionConfiguration.cs` - Configuration model
- `csharp-version/src/MarkdownToDocx.Styling/Models/StyleConfiguration.cs` - Style definitions
- `config/presets/*.yaml` - Preset files (minimal, default, technical)
- `config/vertical/*.yaml` - Vertical text presets
- `csharp-version/tests/MarkdownToDocx.Tests/Unit/YamlConfigurationLoaderTests.cs` - 14 unit tests

**Code references**:

```yaml
# Example: minimal.yaml
version: "1.0"

fonts:
  body: "Noto Sans"
  heading: "Noto Serif"
  code: "Noto Sans Mono"

page:
  width: 210mm
  height: 297mm  # A4
  margins:
    top: 25mm
    bottom: 25mm
    left: 20mm
    right: 20mm

styles:
  heading:
    h1:
      font_size: 24pt
      bold: true
      color: "#2C3E50"
```

**Migration path** (schema evolution):

1. Add `version` field to all YAML files
2. YamlConfigurationLoader checks version
3. Apply version-specific migrations if needed
4. Default to latest schema if version missing

**Rollback plan**:

If YAML proves problematic:
1. Abstract IConfigurationLoader interface allows alternative formats
2. Implement JsonConfigurationLoader as fallback
3. Provide YAML → JSON converter tool
4. Maintain backward compatibility via adapter pattern

---

## Validation

**Success Criteria**:
- [x] Load all preset files successfully
- [x] Validate YAML syntax errors with clear messages
- [x] Support comments in preset files
- [x] Schema version tracking working
- [x] All 14 unit tests passing

**Testing Strategy**:
- Unit tests: Valid/invalid YAML, missing fields, type mismatches
- Integration tests: Full preset → DOCX conversion
- User testing: Non-developers can modify presets

**Monitoring**:
- YAML parse errors in production
- Most commonly used presets
- Custom preset creation rate

---

## Related Decisions

**Supersedes**:
- None (initial decision)

**Related to**:
- ADR-0001: Markdig Parser Selection (parsed AST + YAML config → styled DOCX)
- ADR-0003: Vertical Text Implementation (uses same YAML schema)

**Depends on**:
- None

---

## References

**Documentation**:
- [YAML Specification 1.2](https://yaml.org/spec/1.2/spec.html)
- [YamlDotNet Library](https://github.com/aaubry/YamlDotNet)

**Issues/PRs**:
- None (initial implementation)

**Research**:
- [Configuration Format Comparison](https://en.wikipedia.org/wiki/Comparison_of_data-serialization_formats)
- [Why YAML for Configuration](https://blog.stackpath.com/yaml/)

---

## Timeline

| Date | Event |
|------|-------|
| 2026-02-11 | Schema designed and proposed |
| 2026-02-11 | Approved (comments requirement critical) |
| 2026-02-14 | Implementation started (YamlConfigurationLoader) |
| 2026-02-15 | Implementation completed |
| 2026-02-15 | 14 unit tests passing |
| 2026-02-15 | 4 presets created and tested |
| 2026-02-16 | Validated via Docker conversion test |

---

## Notes

YAML schema design prioritized **user experience** over technical convenience. The ability to document presets inline with comments proved invaluable for creating self-explanatory configuration files.

Key insight: Hierarchical structure (fonts → page → styles) matches mental model of document formatting, making it intuitive for users.

Future enhancements:
- Schema validation tool (`md2docx validate-config`)
- Visual preset editor (GUI tool, optional)
- Preset marketplace/repository

---

**Last Updated**: 2026-02-16
**Next Review**: 2026-06-01 (quarterly review)
