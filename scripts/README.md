# Documentation Maintenance Scripts

Lightweight shell scripts for automated documentation consistency checks.

## 📋 Overview

These scripts complement the existing `/check-docs-consistency` SKILL with lightweight, automation-friendly checks.

### When to use what?

| Tool | Use Case | Features |
|------|----------|----------|
| **`/check-docs-consistency` SKILL** | Manual comprehensive check | Auto-fix, detailed reports, AI analysis |
| **`scripts/check-*.sh`** | Automated checks (hooks, CI/CD) | Fast, lightweight, detection-only |

---

## 🛠️ Available Scripts

### 1. `update-version.sh` - Version Number Synchronization

Updates version numbers across all project files.

**Usage**:
```bash
./scripts/update-version.sh 0.2.0
```

**Updates**:
- README.md version badge
- docs/_config.yml
- docs/index.md
- *.csproj files
- CHANGELOG.md (adds new version section)

**When**: Before every release

---

### 2. `check-docs-sync.sh` - Bilingual Documentation Sync

Checks English and Japanese documentation are in sync.

**Usage**:
```bash
./scripts/check-docs-sync.sh
```

**Checks**:
- File count match (en/ vs ja/)
- File name consistency
- "Last Updated" markers
- Broken internal links

**When**:
- Before committing documentation changes
- Pre-push hook (automated)
- CI/CD pipeline

---

### 3. `check-docs-source-consistency.sh` - Documentation-Source Consistency

Verifies documentation matches actual source code and configuration.

**Usage**:
```bash
./scripts/check-docs-source-consistency.sh
```

**Checks**:
- YAML configuration keys (config/presets/*.yaml ⇄ docs/*/configuration.md)
- Preset files (config/presets/*.yaml ⇄ docs/*/presets.md)
- CLI options (Program.cs ⇄ docs/*/getting-started.md)
- Code example syntax (bash blocks in markdown)
- Version number consistency (README, _config.yml, *.csproj)

**When**:
- After adding/removing configuration keys
- After creating new preset files
- After changing CLI options
- Before push (automated via hook)

---

### 4. `generate-release-notes.sh` - Release Notes Generator

Generates release notes from git commit history.

**Usage**:
```bash
./scripts/generate-release-notes.sh 0.2.0
```

**Generates**:
- `docs/RELEASE_NOTES_v0.2.0.md` (public, English)
- `internal-docs/ja/releases/RELEASE_v0.2.0.md` (internal, Japanese)

**Output includes**:
- Categorized changes (feat, fix, docs, refactor, etc.)
- Full commit history
- Contributors list
- Release checklist (internal)

**When**: During release preparation

---

## 🔄 Integration with Existing Tools

### Relationship with `/check-docs-consistency` SKILL

```
┌────────────────────────────────────────┐
│ /check-docs-consistency SKILL          │
│ (Comprehensive, AI-powered)            │
│                                        │
│ ✓ Auto-fixes issues                    │
│ ✓ Detailed reports                     │
│ ✓ AI analysis                          │
│ ✓ Table of contents sync              │
│ ✓ Code snippet compilation            │
└────────────────────────────────────────┘
              ↓ Complements
┌────────────────────────────────────────┐
│ scripts/check-*.sh                     │
│ (Lightweight, automation-friendly)     │
│                                        │
│ ✓ Fast execution (<5s)                │
│ ✓ No dependencies                      │
│ ✓ Pre-push hook friendly               │
│ ✓ CI/CD integration ready              │
│ ✗ Detection only (no auto-fix)        │
└────────────────────────────────────────┘
```

### Recommended Workflow

```bash
# 1. Quick check during development
./scripts/check-docs-sync.sh

# 2. Commit changes
git add docs/
git commit -m "docs: update configuration guide"

# 3. Pre-push hook runs automatically
# (lightweight scripts check for errors)

# 4. Before release: comprehensive check
/check-docs-consistency

# 5. Release preparation
./scripts/update-version.sh 0.2.0
./scripts/generate-release-notes.sh 0.2.0
```

---

## ⚙️ Automation Setup

### Pre-Push Hook Integration

Add to `.claude/hooks/pre-push.sh`:

```bash
# Check if documentation files changed
if git diff --name-only origin/main...HEAD | grep -qE '^docs/'; then
  echo "📚 Checking documentation consistency..."

  # Lightweight checks
  ./scripts/check-docs-sync.sh || exit 1
  ./scripts/check-docs-source-consistency.sh || exit 1
fi
```

### GitHub Actions Integration

Create `.github/workflows/docs-check.yml`:

```yaml
name: Documentation Check

on:
  pull_request:
    paths:
      - 'docs/**'
      - 'config/**'

jobs:
  check:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Check documentation sync
        run: ./scripts/check-docs-sync.sh

      - name: Check source consistency
        run: ./scripts/check-docs-source-consistency.sh
```

---

## 🎯 Performance Comparison

| Tool | Execution Time | Use Case |
|------|---------------|----------|
| `check-docs-sync.sh` | ~2s | Quick sync check |
| `check-docs-source-consistency.sh` | ~5s | Source consistency |
| `/check-docs-consistency` | ~30-60s | Comprehensive validation |

**Recommendation**: Use lightweight scripts for frequent automated checks, SKILL for periodic comprehensive validation.

---

## 📚 Related Documentation

- `/check-docs-consistency` SKILL - Comprehensive documentation validation
- `internal-docs/ja/DOCUMENTATION_ARCHITECTURE.md` - Documentation management strategy
- `internal-docs/ja/CONSISTENCY_CHECKS.md` - Detailed consistency check guide

---

## 🔧 Troubleshooting

### Script fails with "Permission denied"

```bash
chmod +x scripts/*.sh
```

### False positives in YAML key detection

Edit `check-docs-source-consistency.sh` to exclude specific patterns:

```bash
# Add to the script
EXCLUDE_KEYS="SchemaVersion|Metadata"
```

### Pre-push hook too slow

Consider running only lightweight sync check:

```bash
# Only check file sync (fastest)
./scripts/check-docs-sync.sh
```

---

**Maintained by**: md2docx development team
**Last Updated**: 2026-02-16
