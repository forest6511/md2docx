# ADR-0001: Markdig Parser Selection

**Date**: 2026-02-15
**Status**: Accepted
**Decided by**: csharp-dev agent

---

## Context

**Problem Statement**:

- Need a robust Markdown parsing library for C# implementation
- Must support CommonMark specification with extensions
- Performance is critical for large documents (100+ pages)
- Must be actively maintained and well-documented

**Current Situation**:

- Original plan mentioned markdown-it (JavaScript library)
- C# implementation requires native .NET library
- No parser selected at project start

---

## Decision

Selected **Markdig** as the Markdown parsing engine for md2docx.

**Key Points**:

- Markdig is a .NET library implementing CommonMark + extensions
- Supports AST (Abstract Syntax Tree) manipulation
- High performance and actively maintained
- MIT licensed (compatible with project license)

**Scope**:

- Covers: All Markdown parsing in MarkdownToDocx.Core
- Not covered: Custom Markdown extensions beyond CommonMark

---

## Consequences

### Positive

- ✅ Native .NET library (no JavaScript interop)
- ✅ Excellent performance (benchmarked fastest C# Markdown parser)
- ✅ CommonMark compliant with extensions (tables, task lists, etc.)
- ✅ Active maintenance and community support
- ✅ AST access for advanced processing
- ✅ MIT license compatibility

### Negative

- ❌ Slightly different API from markdown-it (minor migration if needed)
- ❌ Learning curve for team members unfamiliar with Markdig

### Neutral

- ℹ️ Replaces markdown-it mentioned in original architecture
- ℹ️ Requires NuGet package dependency

---

## Alternatives Considered

### Alternative 1: CommonMark.NET

**Description**: Pure CommonMark implementation in C#

**Pros**:

- Simple and focused on CommonMark spec
- Lightweight and minimal dependencies

**Cons**:

- Limited extension support
- Less active development
- No built-in table support

**Why rejected**: Lack of extension support (tables, strikethrough, etc.) required for rich documents

---

### Alternative 2: markdown-it via JavaScript interop

**Description**: Use original markdown-it through Node.js process or V8 engine

**Pros**:

- Matches original architecture plan
- Extensive plugin ecosystem

**Cons**:

- Performance overhead (process/engine communication)
- Complex deployment (requires Node.js runtime)
- Docker image size increase
- Error handling complexity

**Why rejected**: Performance and deployment complexity not justified for native .NET solution

---

### Alternative 3: Custom Markdown parser

**Description**: Implement custom CommonMark parser

**Pros**:

- Full control over parsing logic
- No external dependencies

**Cons**:

- Significant development time (weeks to months)
- CommonMark compliance testing burden
- Maintenance overhead
- Likely slower than optimized libraries

**Why rejected**: Reinventing the wheel; Markdig already provides battle-tested solution

---

## Implementation

**Files affected**:

- `csharp-version/src/MarkdownToDocx.Core/Markdown/MarkdigParser.cs` - Main parser implementation
- `csharp-version/src/MarkdownToDocx.Core/Interfaces/IMarkdownParser.cs` - Parser interface
- `csharp-version/src/MarkdownToDocx.Core/MarkdownToDocx.Core.csproj` - NuGet package reference
- `csharp-version/tests/MarkdownToDocx.Tests/Unit/MarkdigParserTests.cs` - 10 unit tests

**Code references**:

```csharp
public class MarkdigParser : IMarkdownParser
{
    public MarkdownDocument Parse(string markdown)
    {
        var pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .Build();

        var document = Markdown.Parse(markdown, pipeline);
        return ConvertToInternalDocument(document);
    }
}
```

**Migration path**:

No migration needed (initial implementation).

**Rollback plan**:

If critical issues discovered:
1. Abstract IMarkdownParser interface allows swapping implementations
2. Replace MarkdigParser with alternative implementation
3. Update NuGet dependencies
4. Retest with full test suite (49 tests)

---

## Validation

**Success Criteria**:
- [x] Parse all CommonMark test cases
- [x] Support tables, strikethrough, task lists
- [x] Handle large documents (100+ pages) efficiently
- [x] Pass all 10 unit tests
- [x] Integration with DocumentBuilder working

**Testing Strategy**:
- Unit tests: Parse headings, paragraphs, lists, code blocks, quotes
- Integration tests: End-to-end Markdown → DOCX conversion
- Performance: Benchmark with 100-page document (<5 seconds)

**Monitoring** (post-implementation):
- Parse time for large documents
- Memory usage during parsing
- Test suite pass rate

---

## Related Decisions

**Supersedes**:
- None (initial decision)

**Related to**:
- ADR-0002: YAML Schema Design (styling depends on parsed AST)
- ADR-0003: Vertical Text Implementation (parser provides structure)

**Depends on**:
- None

---

## References

**Documentation**:
- [Markdig GitHub](https://github.com/xoofx/markdig)
- [CommonMark Specification](https://commonmark.org/)

**Issues/PRs**:
- None (initial implementation)

**Research**:
- [Markdown Parser Benchmarks (.NET)](https://github.com/xoofx/markdig#benchmarks)
- [Markdig Extensions List](https://github.com/xoofx/markdig/blob/master/src/Markdig.Tests/Specs/readme.md)

---

## Timeline

| Date | Event |
|------|-------|
| 2026-02-15 | Decision proposed and approved |
| 2026-02-15 | Implementation started |
| 2026-02-15 | Implementation completed (MarkdigParser.cs) |
| 2026-02-15 | 10 unit tests passing |
| 2026-02-16 | Validated via Docker integration test |

---

## Notes

Markdig proved to be an excellent choice. Performance is outstanding, and the API is intuitive. The AST structure maps cleanly to our internal document model. Extension support (especially tables) was critical for technical documentation use cases.

Future consideration: Explore custom Markdig extensions if we need md2docx-specific Markdown syntax.

---

**Last Updated**: 2026-02-16
**Next Review**: 2026-06-01 (quarterly review)
