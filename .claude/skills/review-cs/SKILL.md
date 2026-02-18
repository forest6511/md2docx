---
name: review-cs
description: Review C# code for best practices, SOLID principles, and DocumentFormat.OpenXml patterns. Use before committing C# changes.
---

# C# Code Review

Review all modified or newly created `.cs` files in `csharp-version/src/` and `csharp-version/tests/` against the criteria below. Use `git diff --name-only` to identify changed files, then read and analyze each.

## Review Criteria

### 1. SOLID Principles

- **Single Responsibility**: Each class has ONE clear purpose. Flag classes doing too much.
- **Open/Closed**: New features should extend, not modify existing classes. Check for switch statements that grow.
- **Liskov Substitution**: Derived types must be substitutable for base types.
- **Interface Segregation**: Interfaces should be focused. Flag large interfaces (>7 methods).
- **Dependency Inversion**: Depend on abstractions (IDocumentBuilder, IStyleApplicator, IConfigurationLoader), not concretions.

### 2. C# Idioms and Patterns

- **Nullable reference types**: All parameters properly annotated. Use `ArgumentNullException.ThrowIfNull()`.
- **Records for immutable data**: Style models (HeadingStyle, ParagraphStyle, etc.) should be `sealed record`.
- **Pattern matching**: Prefer `switch` expressions and `is` patterns over type casting.
- **File-scoped namespaces**: Use `namespace Foo;` not `namespace Foo { }`.
- **Primary constructors**: Use where appropriate (C# 12).
- **Dispose pattern**: `IDisposable` implementations must call `GC.SuppressFinalize(this)`.
- **readonly fields**: Private fields that are never reassigned should be `readonly`.

### 3. Naming Conventions

| Element | Convention | Example |
|---------|-----------|---------|
| Public members | PascalCase | `AddHeading()`, `FontSize` |
| Private fields | _camelCase | `_document`, `_body` |
| Constants | PascalCase (UPPER_CASE for legacy) | `A4WidthTwips` |
| Interfaces | I-prefix | `IDocumentBuilder` |
| Type parameters | T-prefix | `TResult` |
| Async methods | Async suffix | `LoadAsync()` |
| Test methods | Method_Scenario_Expected | `AddHeading_WithNullText_ShouldThrow()` |

### 4. DocumentFormat.OpenXml Specific

- Proper disposal of `WordprocessingDocument` (via `using` or `IDisposable`).
- Use typed API (`new Paragraph()`, `new Run()`) not string manipulation.
- `AppendChild()` returns the appended element - use it for chaining.
- EMU conversions: 1 twip = 635 EMU, 1 cm = 567 twips, 1 inch = 1440 twips.
- `ImagePart` must use correct content type (image/png, image/jpeg).
- Section properties should be the last child of Body.

### 5. Architecture Rules

- **Core** has zero dependency on Styling or CLI.
- **Styling** depends only on Core (for models/interfaces).
- **CLI** depends on Styling (which transitively includes Core).
- **Models in Core** are POCOs - no OpenXml types in public interfaces.
- **Configuration flows one way**: YAML -> ConversionConfiguration -> StyleApplicator -> Core models.

### 6. Error Handling

- Use specific exception types (`ArgumentNullException`, `FileNotFoundException`), not generic `Exception`.
- Guard clauses at method entry, not deep nesting.
- `ArgumentNullException.ThrowIfNull()` preferred over manual null checks.
- Never swallow exceptions silently.

### 7. Test Quality

- Follow AAA pattern (Arrange / Act / Assert).
- One logical assertion per test (multiple FluentAssertions on same object is OK).
- Test method naming: `Method_Scenario_ExpectedResult`.
- Use `[Theory]` + `[InlineData]` for parameterized tests.
- Use FluentAssertions (`.Should()`) consistently.

## Output Format

```markdown
## Code Review Report

### Summary
- Files reviewed: N
- Issues found: N (Critical: N, Important: N, Suggestion: N)

### File: path/to/File.cs

#### Critical
- **Line N**: [Issue description]. Fix: [recommendation]

#### Important
- **Line N**: [Issue description]. Fix: [recommendation]

#### Suggestions
- **Line N**: [Suggestion]

### Verdict
[PASS / PASS WITH NOTES / NEEDS CHANGES]
```

## Quick Check Mode

If invoked with `--quick`, only check:
1. Null safety (ArgumentNullException.ThrowIfNull)
2. Naming conventions
3. Architecture boundaries (no wrong dependencies)
4. Dispose pattern
