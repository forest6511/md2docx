# Quality Enforcement Implementation Guide

**Purpose**: Implementation details for rules defined in RULES.md

**Audience**: Developers implementing or modifying quality enforcement hooks

---

## Hook Architecture

```text
.claude/hooks/
├── pre-commit-check.sh      # Fast validation (<30s)
├── pre-push-check.sh         # Comprehensive validation (<2min)
└── skill-activation-prompt.js # Conditional skill loading
```text

---

## R001: YAML Syntax Validation

### Implementation

**Location**: `pre-commit-check.sh`

**Detection Method**:
```bash
python3 -c "import yaml; yaml.safe_load(open('$yaml_file'))"
```text

**Performance**: ~50-100ms per YAML file

**Error Output Example**:
```text
❌ R001 VIOLATION: Invalid YAML in config/presets/minimal.yaml
   yaml.scanner.ScannerError: mapping values are not allowed here
     in "config/presets/minimal.yaml", line 15, column 18
```text

**Common Violations**:
1. Missing quotes for values with colons: `title: Problem: Solution`
2. Incorrect indentation (mixing tabs/spaces)
3. Unclosed strings or brackets

**Fix Guidance**:
- Use YAML linter: `yamllint config/presets/minimal.yaml`
- Most editors have YAML validation built-in

---

## R002: Test Coverage Minimum

### Implementation

**Location**: `pre-push-check.sh` (not yet implemented in provided files)

**Detection Method**:
```bash
dotnet test --collect:"XPlat Code Coverage" /p:CollectCoverage=true
```text

**Performance**: ~30-60 seconds for full test suite

**Coverage Calculation**:
```bash
# Use coverlet or ReportGenerator
reportgenerator -reports:"coverage.xml" -targetdir:"coverage-report"
grep -oP 'Line coverage: \K[\d.]+' coverage-report/index.html
```text

**Thresholds**:
- Overall: ≥80%
- Core: ≥85%
- Styling: ≥80%
- CLI: ≥70%
- YAML loader: ≥90%

**Bypass** (emergency only):
```bash
git push --no-verify  # Skip all hooks (use with caution)
```text

---

## R003: No Secrets in Commits

### Implementation

**Location**: `pre-commit-check.sh`

**Detection Patterns**:
```bash
declare -A SECRET_PATTERNS=(
  ["API key"]="api[_-]?key[\"']?\s*[:=]\s*[\"'][a-zA-Z0-9]{20,}[\"']"
  ["Password"]="password[\"']?\s*[:=]\s*[\"'][^\"']{8,}[\"']"
  ["Token"]="token[\"']?\s*[:=]\s*[\"'][a-zA-Z0-9]{20,}[\"']"
  ["AWS key"]="AKIA[0-9A-Z]{16}"
  ["Private key"]="-----BEGIN (RSA |EC )?PRIVATE KEY-----"
)
```text

**Performance**: ~100-200ms for full diff scan

**False Positives**:
- Test fixtures with fake credentials
- Documentation examples

**Whitelist** (if needed):
```bash
# In pre-commit-check.sh, add:
WHITELIST_FILES=("tests/fixtures/fake-api-key.json")
```text

**Recommended Tool** (future):
```bash
# Replace DIY regex with gitleaks
gitleaks detect --no-git --verbose
```text

---

## R004: Codex Review Required

### Implementation

**Location**: `pre-push.sh` ✅ **IMPLEMENTED**

**Status**: Using fallback `dotnet format --verify-no-changes` until Codex skill is available

**Execution**:
```bash
# Primary: Codex skill (when available)
claude-code run-skill codex-review --files "$CHANGED_FILES"

# Fallback: dotnet format verification
dotnet format --verify-no-changes
```text

**Performance**:
- Codex: ~30-90 seconds (when available)
- Fallback: ~2-5 seconds

**Checks**:
- Code formatting consistency (dotnet format)
- Future: Code quality, security, performance (Codex)

**Bypass** (not recommended):
```bash
git push --no-verify
```text

---

## R005: Font License Compliance

### Implementation

**Location**: `pre-commit-check.sh`

**Detection Method**:
```bash
# Check Dockerfile and YAML configs
git diff --cached Dockerfile config/ | grep -iE "游明朝|Yu.*Mincho|Hiragino"
```text

**Performance**: ~50-100ms

**Forbidden Patterns**:
```text
游明朝, 游ゴシック
Yu Mincho, Yu Gothic
Hiragino, ヒラギノ
MS 明朝, MS ゴシック
```text

**Allowed Fonts**:
```text
Noto Serif CJK JP
Noto Sans CJK JP
Noto Serif
Noto Sans
IPA明朝, IPAゴシック (OSS)
```text

**Common Mistake**:
```yaml
# ❌ Wrong
font: "游明朝"

# ✅ Correct
font: "Noto Serif CJK JP"
```text

---

## R006: ADR-Source Consistency

### Implementation

**Location**: `.claude/skills/adr-source-validator/validate.py`

**Execution**:
```bash
python3 .claude/skills/adr-source-validator/validate.py
```text

**Performance**: ~2-5 seconds for full ADR validation

**Validation Types**:

1. **Font Decisions** (FontLicenseValidator)
   - Checks Dockerfile for required fonts
   - Validates no commercial fonts present

2. **Library Decisions** (LibraryDependencyValidator)
   - Checks .csproj for required NuGet packages
   - Validates rejected alternatives not present

3. **YAML Schema** (YAMLSchemaValidator)
   - Checks all presets for correct schema_version
   - Reports files with mismatched versions

4. **Design Patterns** (DesignPatternValidator)
   - Validates implementation files exist
   - Checks pattern implementation present

**Extensibility**:
```python
# Add new validator in validate.py
class MyCustomValidator(BaseValidator):
    def can_validate(self, adr: ADR) -> bool:
        return 'my-keyword' in adr.content.lower()

    def validate(self, adr: ADR) -> List[ValidationResult]:
        # Your validation logic
        pass
```text

---

## R101: Markdown Lint Clean

### Implementation

**Location**: `pre-commit-check.sh` (warning)

**Tool**: markdownlint-cli2

**Installation**:
```bash
npm install -g markdownlint-cli2
```text

**Configuration**: `.markdownlint.jsonc`

**Auto-Fix**:
```bash
markdownlint-cli2 --fix --config .markdownlint.jsonc *.md
```text

**Performance**: ~100-200ms per file

**Common Issues**:
- MD032: Blank lines around lists
- MD022: Headers spacing
- MD040: Code blocks without language
- MD013: Line length (warning only)

---

## R102: C# Format Consistency

### Implementation

**Location**: Manual execution or quality automation

**Tool**: dotnet format

**Execution**:
```bash
dotnet format --verify-no-changes  # Check
dotnet format                       # Fix
```text

**Performance**: ~2-5 seconds for full codebase

**Configuration**: `.editorconfig`

**Standards**:
- 4-space indentation
- LF line endings
- UTF-8 encoding
- No trailing whitespace

---

## R103: Documentation-Source Sync

### Implementation

**Location**: `/check-docs` skill (manual execution)

**Checks**:

1. **YAML Examples Match Actual Files**:
```bash
# Extract YAML from README
awk '/```yaml/,/```/' README.md > extracted.yaml

# Compare with actual preset
diff extracted.yaml config/presets/minimal.yaml
```text

2. **CLI Help Matches README**:
```bash
# Extract help text
dotnet run --project src/CLI -- --help > help.txt

# Compare with README usage section
diff help.txt docs/cli-usage.txt
```text

3. **Version Consistency**:
```bash
# Check all version references
grep -r "version" \
  *.csproj \
  README.md \
  CHANGELOG.md \
  Dockerfile
```text

**Performance**: ~1-2 seconds

---

## Skill Auto-Activation

### Implementation

**Location**: `.claude/hooks/skill-activation-prompt.js`

**Execution**: Called automatically by Claude Code before processing user input

**Confidence Calculation**:
```javascript
confidence = (keyword_score * 0.6) + (path_score * 0.4)

// Activate if confidence >= threshold
if (confidence >= 0.7) {
  activate_skill()
}
```text

**Keyword Matching**:
- Direct match: "adr" matches "adr"
- Synonym match: "architecture decision" matches "adr"
- Case-insensitive

**Path Matching**:
- Glob pattern: `docs/decisions/**` matches `docs/decisions/ADR-0001.md`
- Score based on percentage of working files matching

**Output**:
```json
{
  "skills": [
    {
      "name": "adr-source-validator",
      "confidence": 0.85,
      "reason": "keywords: adr, decision; files: ADR-0001.md"
    }
  ]
}
```text

**Debug Mode**:
```bash
export SKILL_DEBUG=true
node skill-activation-prompt.js "validate adr"
```text

---

## Performance Targets

### Pre-Commit Hook (<30 seconds)

**Breakdown**:
- R001 YAML validation: ~0.5s (10 files)
- R003 Secret scan: ~0.2s
- R005 Font check: ~0.1s
- Total: **~0.8s** ✅

**Optimization**:
- Single-pass diff scanning
- Parallel checks where possible
- Early exit on violations

### Pre-Push Hook (<2 minutes)

**Breakdown**:
- R002 Test coverage: ~45s
- R004 Codex review: ~60s
- R006 ADR validation: ~5s
- Total: **~110s** ✅

**Optimization**:
- Cache test results when possible
- Incremental Codex review (diff only)
- Parallel ADR validators

---

## Testing Quality System

### Unit Tests

**Location**: `tests/quality-system/`

**Run**:
```bash
# Skill activation tests
node tests/quality-system/test_skill_activation.js

# ADR validator tests (future)
python3 tests/quality-system/test_adr_validator.py
```text

### Integration Tests

**Benchmark hooks**:
```bash
bash tests/quality-system/benchmark_hooks.sh
```text

### Manual Testing

**Test pre-commit**:
```bash
# Create invalid YAML
echo "invalid: yaml: content" > test.yaml
git add test.yaml
git commit -m "test"  # Should fail
```text

**Test ADR validator**:
```bash
python3 .claude/skills/adr-source-validator/validate.py --verbose
```text

---

## Debugging

### Hook Not Running

**Check installation**:
```bash
ls -la .git/hooks/
cat .git/hooks/pre-commit
```text

**Reinstall**:
```bash
.claude/hooks/install.sh
```text

### Hook Failing Unexpectedly

**Run manually**:
```bash
bash -x .claude/hooks/pre-commit-check.sh
```text

**Check dependencies**:
```bash
which python3
which markdownlint-cli2
which dotnet
```text

### Skill Not Activating

**Enable debug mode**:
```bash
export SKILL_DEBUG=true
```text

**Check skill rules**:
```bash
cat .claude/skills/skill-rules.json
```text

---

## Maintenance

### Adding New Rules

1. Update `RULES.md` with rule definition
2. Implement check in appropriate hook
3. Add tests in `tests/quality-system/`
4. Update this document with implementation details
5. Communicate to team

### Modifying Thresholds

**Test coverage**:
```bash
# Edit pre-push-check.sh
MIN_COVERAGE=85  # Change from 80
```text

**Confidence threshold**:
```json
// Edit skill-rules.json
"confidence_threshold": 0.8  // Change from 0.7
```text

### Disabling Rules (Temporary)

**Environment variables** (add support):
```bash
export SKIP_R001=true  # Skip YAML validation
export SKIP_R005=true  # Skip font check
git commit -m "emergency fix"
```text

---

## Best Practices

1. **Keep hooks fast**: Users will bypass slow hooks
2. **Clear error messages**: Tell users exactly what to fix
3. **Auto-fix when possible**: R101, R102 auto-fix at session end
4. **Test before enforcing**: Use warnings before making rules critical
5. **Version control**: Keep hooks in repo, not just local
6. **Document exceptions**: If bypassing hook, document why

---

## Troubleshooting

### Common Issues

**"Python not found"**:
```bash
which python3
# Install Python 3.8+
```text

**"markdownlint-cli2 not found"**:
```bash
npm install -g markdownlint-cli2
```text

**"Permission denied"**:
```bash
chmod +x .claude/hooks/*.sh
```text

**"Hook takes too long"**:
- Check for network calls in hooks (should be local only)
- Profile with `time bash -x hook.sh`
- Consider moving slow checks to pre-push

---

**Last Updated**: 2026-02-14
**Maintainer**: Quality Engineering Team
