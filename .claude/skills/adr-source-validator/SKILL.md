---
name: adr-source-validator
description: Validate Architecture Decision Records match actual implementation in code and configuration
disable-model-invocation: false
---

# ADR-Source Consistency Validator

Validates that accepted ADRs are actually implemented in the codebase.

## Purpose

Prevents documentation drift - ensures architectural decisions documented in ADRs match reality in code, configuration, and dependencies.

## Usage

```bash
/adr-source-validator
```text

Or automatically on:
- Pre-push hook (full validation)
- Session start (summary check)
- ADR file changes

## What It Validates

### 1. Status-Implementation Consistency

| ADR Status | Expected State | Enforcement |
|------------|----------------|-------------|
| **Proposed** | No implementation | Warning if exists |
| **Accepted** | Must be implemented | ERROR if missing |
| **Deprecated** | May exist with warnings | Warning if used |
| **Superseded** | Old code should be removed | ERROR if remains |
| **Rejected** | Must NOT exist | ERROR if found |

### 2. Technology Choices

**Example ADR**:
```markdown
# ADR-0015: Use DocumentFormat.OpenXml

## Decision
Use Microsoft's official DocumentFormat.OpenXml library.

## Alternatives Rejected
- ❌ NPOI
- ❌ Aspose.Words
```text

**Validation**:
- ✅ DocumentFormat.OpenXml in *.csproj
- ❌ NPOI NOT in *.csproj
- ❌ Aspose.Words NOT in *.csproj

### 3. Configuration Standards

**Example ADR**:
```markdown
# ADR-0020: YAML Schema Version 2.0

## Decision
All presets must use `schema_version: "2.0"`
```text

**Validation**:
- Check all YAML files in config/presets/
- Verify schema_version field exists
- Validate version matches "2.0"

### 4. Font Licensing

**Example ADR**:
```markdown
# ADR-0012: Use Noto Serif JP

## Decision
Noto Serif JP as default (SIL OFL licensed)

## Rejected
- 游明朝 (Commercial license)
```text

**Validation**:
- ✅ Noto Serif JP in Dockerfile
- ❌ No 游明朝 in Dockerfile or presets

### 5. Design Patterns

**Example ADR**:
```markdown
# ADR-0008: Factory Pattern for Parsers

Implementation: `MarkdownToDocx.Core/Parsers/ParserFactory.cs`
```text

**Validation**:
- File exists at specified path
- Contains "Factory" class
- Has Create* methods

## Validation Script

Located: `.claude/skills/adr-source-validator/validate.py`

Run directly:
```bash
python3 .claude/skills/adr-source-validator/validate.py
```text

## Output Example

```text
🔍 Validating ADR-Source Consistency...

📋 ADR-0012: Use Noto Serif JP as Default Font
   Status: Accepted
   ✅ Font in Dockerfile
   ✅ No commercial fonts detected

📋 ADR-0015: Use DocumentFormat.OpenXml
   Status: Accepted
   ✅ DocumentFormat.OpenXml in dependencies
   ✅ No rejected libraries detected

📋 ADR-0020: YAML Schema Version 2.0
   Status: Accepted
   ❌ 3 presets with wrong schema version

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
📊 ADR-Source Consistency Report
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

❌ VIOLATIONS:
   • ADR-0020: 3 presets don't use schema v2.0

✅ PASSED: 5 checks

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Compliance: 83.3% (5/6)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
```text

## Exit Codes

- `0`: All validations passed
- `1`: Violations detected (blocks pre-push)

## Integration

### Pre-Push Hook
```bash
python3 .claude/skills/adr-source-validator/validate.py
if [ $? -ne 0 ]; then
  echo "❌ ADR-Source inconsistencies detected"
  exit 1
fi
```text

### Session Start Hook
```bash
# Quick summary only
ACCEPTED=$(grep -l "Status: Accepted" docs/decisions/ADR-*.md | wc -l)
echo "  ✅ $ACCEPTED accepted ADRs (should be implemented)"
```text

## See Also

- `.claude/RULES.md` - Rule R006: ADR-Source Consistency
- `docs/decisions/` - All ADR files
- `.claude/hooks/pre-push-check.sh` - Enforcement hook
