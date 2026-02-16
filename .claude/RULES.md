# Project Rules - markdown-to-docx

**Purpose**: Explicit, enforceable rules for code quality, security, and architectural consistency.

---

## 🔴 CRITICAL Rules (Enforced by Hooks - Blocks Commit/Push)

### R001: YAML Syntax Validation

**Rule**: All YAML files must be syntactically valid before commit
**Enforcement**: `pre-commit` hook blocks invalid YAML
**Detection**: `python3 -c "import yaml; yaml.safe_load(file)"`
**Auto-fix**: No (manual fix required)
**Rationale**: Invalid YAML breaks runtime configuration loading

**Violation Example**:

```yaml
# Invalid - missing quotes for value with colon
title: Problem: Solution  # ❌ Parse error
title: "Problem: Solution"  # ✅ Correct
```text

---

### R002: Test Coverage Minimum
**Rule**: Overall test coverage must be ≥80% before push
**Enforcement**: `pre-push` hook blocks <80% coverage
**Detection**: `dotnet test --collect:"XPlat Code Coverage"`
**Auto-fix**: No (write tests)
**Rationale**: OSS projects require high quality standards

**Per-Module Targets**:
- Core conversion logic: ≥85%
- Styling engine: ≥80%
- CLI: ≥70% (UI code is harder to test)
- YAML loader: ≥90% (critical path)

---

### R003: No Secrets in Commits
**Rule**: No API keys, passwords, tokens, or credentials in commits
**Enforcement**: `pre-commit` hook scans for patterns
**Detection**: Regex patterns for common secrets
**Auto-fix**: No (remove and re-commit)
**Rationale**: Security - prevent credential leaks

**Detected Patterns**:
- `api[_-]?key.*=.*['\"][a-zA-Z0-9]{20,}['\"]`
- `password.*=.*['\"][^'\"]{8,}['\"]`
- `token.*=.*['\"][a-zA-Z0-9]{20,}['\"]`
- `secret.*=.*['\"][a-zA-Z0-9]{20,}['\"]`

---

### R004: Codex Review Required
**Rule**: All commits must pass Codex review before push
**Enforcement**: `pre-push` hook calls Codex skill
**Detection**: Codex skill execution status
**Auto-fix**: No (fix Codex issues)
**Rationale**: Maintain code quality standards for OSS

---

### R005: Font License Compliance
**Rule**: Only SIL OFL licensed fonts in Docker images and defaults
**Enforcement**: `pre-commit` hook detects commercial fonts
**Detection**: Pattern matching for commercial font names
**Auto-fix**: No (replace with Noto fonts)
**Rationale**: OSS distribution requires free redistribution rights

**Allowed Fonts** (SIL OFL):
- Noto Serif CJK JP
- Noto Sans CJK JP
- Noto Serif
- Noto Sans

**Forbidden Fonts** (Commercial):
- 游明朝 (Yu Mincho)
- 游ゴシック (Yu Gothic)
- ヒラギノ (Hiragino)
- MS 明朝/ゴシック
- Any bundled Windows/macOS system fonts

---

### R006: ADR-Source Consistency
**Rule**: Accepted ADRs must be implemented in code
**Enforcement**: `pre-push` hook validates implementation
**Detection**: Python script checks each ADR's requirements
**Auto-fix**: No (requires manual implementation)
**Rationale**: Prevent documentation drift from reality

**Validation Areas**:
1. Technology choices (libraries, frameworks)
2. Configuration standards (YAML schema versions)
3. Design patterns (Factory, Strategy, etc.)
4. Font licensing decisions
5. API contracts and interfaces

**Status-Based Enforcement**:
- **Proposed**: No enforcement (not yet decided)
- **Accepted**: Must be implemented
- **Deprecated**: Warning if still actively used
- **Superseded**: Old implementation should be removed
- **Rejected**: Must NOT be implemented

---

## 🟡 IMPORTANT Rules (Warning + Auto-fix on Session End)

### R101: Markdown Lint Clean
**Rule**: No markdown lint errors in documentation
**Enforcement**: Warning on `pre-commit`
**Detection**: `markdownlint-cli2 --config .markdownlint.jsonc`
**Auto-fix**: Manual via `scripts/fix-markdown-lint.sh`
**Rationale**: Professional documentation quality

**Auto-Fixed Issues**:
- MD032: Blanks around lists
- MD022: Headings spacing
- MD040: Fenced code language
- MD060: Duplicate headings

---

### R102: C# Format Consistency
**Rule**: All C# code follows `dotnet format` standards
**Enforcement**: Warning on `session-start`, auto-fix on `session-end`
**Detection**: `dotnet format --verify-no-changes`
**Auto-fix**: Yes (dotnet format)
**Rationale**: Consistent code style across contributors

---

### R103: Documentation-Source Sync
**Rule**: README examples must match actual YAML presets
**Enforcement**: Warning on `session-start`
**Detection**: Python script comparing examples
**Auto-fix**: Partial (can regenerate examples)
**Rationale**: Prevent user confusion from outdated docs

**Sync Checks**:
1. YAML examples in README are syntactically valid
2. YAML examples match actual preset files in `config/`
3. CLI help text matches README usage section
4. Version numbers consistent across all files
5. Dockerfile font list matches DOCKER_STRATEGY.md

---

## 🟢 RECOMMENDED Rules (Informational - No Enforcement)

### R201: ADR for Architectural Changes
**Rule**: Major architectural decisions should have ADR
**Enforcement**: None (user discretion)
**Triggers**: Consider ADR when:
- Adding/removing major dependencies
- Changing YAML schema structure
- Adopting new design patterns
- Modifying public API contracts
- Making Docker image architecture changes

---

### R202: Session Logs Maintained
**Rule**: Maintain session logs for audit trail
**Enforcement**: Auto-executed on `session-end`
**Detection**: Check `docs/knowledge/sessions/` existence
**Auto-fix**: Yes (auto-created)
**Rationale**: Track development history and decisions

---

## 📊 Rule Enforcement Matrix

| Rule | Check Point | Action on Violation | Auto-Fix | Severity |
|------|-------------|---------------------|----------|----------|
| R001 | pre-commit | BLOCK | No | Critical |
| R002 | pre-push | BLOCK | No | Critical |
| R003 | pre-commit | BLOCK | No | Critical |
| R004 | pre-push | BLOCK | No | Critical |
| R005 | pre-commit | BLOCK | No | Critical |
| R006 | pre-push | BLOCK | No | Critical |
| R101 | session-start, session-end | WARN → AUTO-FIX | Yes | Important |
| R102 | session-start, session-end | WARN → AUTO-FIX | Yes | Important |
| R103 | session-start | WARN | Partial | Important |
| R201 | Manual | INFO | No | Recommended |
| R202 | session-end | AUTO-EXECUTE | Yes | Recommended |

---

## 🔧 Hook Execution Flow

### Pre-Commit (<30 seconds - Fast)
```text
1. R001: YAML syntax validation
2. R003: Secret detection
3. R005: Font license compliance
4. R101: Markdown lint (auto-fix if needed)
```text

### Pre-Push (<2 minutes - Comprehensive)
```text
1. R002: Test coverage ≥80%
2. R004: Codex review
3. R006: ADR-source consistency
4. Breaking change detection
5. Build verification
```text

### Session Start
```text
1. Display git status
2. R103: Documentation sync check (warn only)
3. Load PROGRESS.md summary
4. Show pending ADRs
5. Display blockers
```text

### Session End
```text
1. R101: Auto-fix markdown lint
2. R102: Auto-format C# code
3. Update PROGRESS.md
4. R202: Create session log
5. Docker size check (if Dockerfile changed)
```text

---

## 🎯 Compliance Targets

| Metric | Target | Current | Status |
|--------|--------|---------|--------|
| Critical Rules Passing | 100% | - | Required for push |
| Important Rules Passing | ≥90% | - | Auto-fixed |
| Test Coverage | ≥80% | 72% | In progress |
| ADR Coverage | 100% | - | Enforced |
| Documentation Sync | 100% | - | Monitored |

---

## 📚 References

- **Git Hooks**: `.claude/hooks/pre-commit-check.sh`, `pre-push-check.sh`
- **ADR Validator**: `.claude/skills/adr-source-validator/`
- **Doc Sync**: `.claude/skills/check-docs-consistency/`

---

**Last Updated**: 2026-02-14
**Maintained By**: Project maintainers and Claude Code automation
**Review Frequency**: When new critical rules are needed
