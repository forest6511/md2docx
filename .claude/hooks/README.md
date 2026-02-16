# Hooks Overview

Automated scripts that run at specific lifecycle events.

## Active Hooks

### pre-commit-check.sh

**When**: Before every `git commit`
**Purpose**: Fast validation to catch errors early
**Duration**: <30 seconds (developer-friendly)
**Checks**:

- R001: YAML syntax validation
- R003: Secret detection
- R005: Font license compliance
- R101: Markdown lint (with auto-fix)

**Blocks commit if**: Critical rules violated

---

### skill-activation-prompt.js

**When**: On user prompt submission
**Purpose**: Auto-activate relevant skills based on context
**How**: Analyzes prompt keywords and file paths, loads matching skills
**Critical**: THE most important hook for preventing CLAUDE.md bloat

---

## Removed Hooks

### ~~pre-push-check.sh~~ → To be created separately

**Status**: Not yet implemented
**Will include**: R002 (coverage), R004 (Codex), R006 (ADR validation)
**Duration**: <2 minutes

---

## Hook vs Skill Decision Matrix

| Use Case | Hook (Auto) | Skill (Manual) |
|----------|-------------|----------------|
| Pre-commit validation | ✅ Catch errors | ❌ |
| Pre-push validation | ✅ Final check | ❌ |
| Skill auto-activation | ✅ Essential | ❌ |

---

## File Structure

```text
.claude/hooks/
├── README.md                    # This file
├── pre-commit-check.sh          # Auto: Fast validation
├── skill-activation-prompt.js   # Auto: Skill loading
└── (pre-push-check.sh)          # TODO: Comprehensive validation
```text

**Total**: 2 active hooks (minimal, focused)

---

## Integration with Skills

Hooks complement skills:
- **Hooks**: Automatic, fast, essential checks
- **Skills**: Manual, detailed, when user needs them

Example workflow:
```text
1. Work...
2. Git commit → pre-commit-check.sh (auto, 30sec)
3. Git push → pre-push-check.sh (auto, 2min) [TODO]
```text

---

**Last Updated**: 2026-02-14
**Maintained By**: Quality system automation
