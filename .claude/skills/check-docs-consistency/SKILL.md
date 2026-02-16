---
name: check-docs
description: Validate and synchronize documentation with actual code and configuration. Checks YAML examples, code snippets, broken links, and version consistency.
disable-model-invocation: false
---

# Check Documentation Consistency

Autonomous validation and synchronization of documentation.

## What it checks

1. **YAML Examples** - All YAML code blocks in markdown are valid
2. **Code Snippets** - C# examples compile correctly
3. **Configuration Sync** - README examples match actual preset files
4. **Links** - All internal and external links resolve
5. **Version Numbers** - Consistent across README, CSPROJ, Dockerfile
6. **Table of Contents** - Matches actual headings

## Usage

```bash
/check-docs
```text

## Auto-fix capabilities

✅ YAML formatting
✅ Link updates
✅ Version synchronization
✅ Table of Contents regeneration

## Output

Generates validation report: `docs/reports/consistency-YYYY-MM-DD.md`

## Example report

```markdown
# Documentation Consistency Report

**Files Checked**: 25 markdown, 12 YAML
**Issues Found**: 8 (3 auto-fixed, 5 manual)

## Summary
✅ YAML Examples: 45/45 valid
⚠️ Links: 3 broken external links
✅ Version Consistency: All synchronized

## Auto-Fixed
1. README.md:85 - YAML syntax formatted
2. DOCKER_STRATEGY.md:220 - Config synced

## Manual Review Needed
1. CLAUDE.md:120 - Broken external link (404)
```text

## Integration

Used by:
- Quality Guardian (weekly deep scan)
- Manual execution via `/check-docs` command
