---
name: session-end
description: Comprehensive session summary before exit - what was done, what's next, cleanup, and quality checks
---

# Session End

Comprehensive session closure before exiting Claude Code.

Run this **before `/exit`** to:

- Summarize all work done today
- Plan next session
- Clean up temporary files
- Validate code quality
- Update documentation

## Usage

```text
/session-end
```text

## What It Does

### 1. Work Summary

```bash
# All commits today
git log --oneline --since="today" --no-merges

# Uncommitted changes
git status
git diff --stat
```text

Generates complete list of:
- Commits made
- Files changed
- Features implemented
- Bugs fixed

### 2. Quality Checks

**Code Quality**:
```bash
# Markdown lint (auto-fix)
markdownlint-cli2 --fix --config .markdownlint.jsonc "**/*.md"

# C# format (if applicable)
dotnet format --verify-no-changes

# YAML syntax
find config -name "*.yaml" -o -name "*.yml" | while read f; do
  python3 -c "import yaml; yaml.safe_load(open('$f'))" || echo "ERROR: $f"
done
```text

**Test Coverage**:
```bash
# Current coverage
if [ -f coverage.cobertura.xml ]; then
  python3 -c "import xml.etree.ElementTree as ET; tree = ET.parse('coverage.cobertura.xml'); print(f\"Coverage: {tree.getroot().get('line-rate', 'N/A')}\")"
else
  echo "No coverage file found"
fi
```text

**ADR Consistency**:
```bash
python3 .claude/skills/adr-source-validator/validate.py
```text

**Lint Summary**:
```bash
echo ""
echo "=== Lint Check Summary ==="
MD_ERRORS=$(markdownlint-cli2 --config .markdownlint.jsonc "**/*.md" 2>&1 | grep -c "error" || echo "0")
if [ "$MD_ERRORS" -eq "0" ]; then
  echo "✅ Markdown: Clean"
else
  echo "⚠️  Markdown: $MD_ERRORS errors (auto-fixed)"
fi
```text

### 3. Documentation Audit

**CLAUDE.md**:
- Technology stack current?
- New rules added?
- Last updated date?

**README.md**:
- Examples match actual presets?
- New features documented?
- Installation steps current?

**RULES.md**:
- New quality rules added?
- Enforcement matrix updated?

### 4. PROGRESS.md Update

```python
# Calculate completion percentage
completed = count("✅")
total = count("✅" + "🔄" + "⏳")
percentage = (completed / total) * 100

# Update file
update_completion(percentage)
update_timestamp()
```text

### 5. Cleanup

**Temporary Files**:
```bash
find . -name "*.tmp"
find . -name "*.bak"
find . -name "*.original"
find . -name "debug*.sh"
find . -name "temp_*"
find . -name ".DS_Store"
```text

**Auto-delete** (no confirmation):
- `*.tmp` - Temporary files
- `*.bak` - Backup files
- `*.original` - Backup files from refactoring
- `.DS_Store` - macOS system files
- `debug*.sh` - Debug scripts
- `temp_*` - Temporary files

```bash
# Cleanup command
find . -type f \( -name "*.tmp" -o -name "*.bak" -o -name "*.original" -o -name ".DS_Store" \) -delete
find . -type f \( -name "debug*.sh" -o -name "temp_*" \) -delete
```text

### 6. Git Status

**Uncommitted Changes**:
- List all uncommitted files
- Ask if should commit
- Suggest appropriate commit message

**Unpushed Commits**:
- Warn if commits not pushed
- Remind about Codex review requirement

**Branch Status**:
- Suggest PR creation if feature complete
- Recommend branch cleanup if merged

### 7. Next Session Planning

Based on:
- Uncompleted work
- Open issues
- ADRs pending approval
- Test coverage gaps

Suggest 3 priority tasks for next session.

## Output Example

```text
## Session End Report - 2026-02-14

**Duration**: 4.5 hours
**Commits**: 8
**Files Changed**: 23

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

### Completed This Session

✅ Implemented Quality Assurance System
   - Created .claude/RULES.md (11 rules defined)
   - Created pre-commit hook (R001, R003, R005, R101)
   - Created ADR-source validator skill
   - Created skill auto-activation hook

✅ Documentation
   - Updated CLAUDE.md with session workflow
   - Created session-start/session-update/session-end skills
   - Updated PROGRESS.md (70% → 75%)

✅ Skills
   - /session-update - Mid-session progress capture
   - /session-end - Comprehensive closure
   - /adr-source-validator - ADR consistency

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

### Quality Metrics

**Code Quality**:
- Markdown Lint: ✅ 0 errors (210 fixed)
- C# Format: N/A (no code yet)
- YAML Syntax: ✅ All valid

**Test Coverage**:
- Current: N/A (no tests yet)
- Target: 80%

**ADR Consistency**:
- Not run (no Accepted ADRs yet)

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

### Documentation Status

**CLAUDE.md**: ✅ Updated
- Added session workflow section
- Removed outdated Agent Mode reference
- Last updated: 2026-02-14

**README.md**: ✅ No changes needed

**RULES.md**: ✅ Created
- 11 rules defined
- Enforcement matrix complete

**PROGRESS.md**: ✅ Updated
- Completion: 75% (15/20 milestones)
- Last updated: 2026-02-14 18:30

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

### Git Status

**Uncommitted Changes**: 0 files
**Unpushed Commits**: 0 commits
**Branch**: main

⚠️  Working on main branch - consider creating feature branch next session

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

### Cleanup

**Deleted Files**: 0
**Deleted Directories**: 0

No temporary files found.

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

### Next Session - Recommended Tasks

**Priority 1**: Implement YAML Config Loader
- Create ConfigLoader.cs
- Add schema validation
- Unit tests
- Estimated: 2-3 hours

**Priority 2**: Start Markdown → DOCX Converter
- Evaluate parser options
- Implement basic AST handling
- Create first integration test
- Estimated: 3-4 hours

**Priority 3**: Docker Image Build
- Create Dockerfile
- Test font embedding
- Optimize image size
- Estimated: 1-2 hours

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

### Reminders

- ✅ All quality rules enforced via hooks
- ⚠️  Codex review required before push
- ⚠️  Create feature branch for next work
- ✅ Session progress saved to internal-docs/ja/sessions/

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Session ended successfully - Ready to exit

```text

## When to Use

**Always use before `/exit`** to:
- Get comprehensive work summary
- Plan next session
- Ensure quality standards
- Clean up workspace
- Update all documentation

**Don't use mid-session** - use `/session-update` instead for checkpoints.

## Integration

- Incorporates all `/session-update` data from current day
- Creates final entry in `internal-docs/ja/sessions/YYYY-MM-DD.md`
- Updates `PROGRESS.md` with latest completion percentage
- Suggests commits for auto-generated changes
