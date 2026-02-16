---
name: session-update
description: Update session progress mid-session or after accidental exit - captures what was done and what's next
user-invocable: true
---

# Session Update

Capture current session progress at any time. Useful for:

- Mid-session checkpoints
- After accidental exit (to preserve work context)
- Before taking a break
- When switching tasks

## Usage

```text
/session-update
```text

## What It Does

### 1. Captures Current Work

```bash
# Recent commits
git log --oneline --since="6 hours ago" --no-merges

# Uncommitted changes
git status --short
git diff --stat
```text

### 2. Summarizes Progress

- **Completed**: What was finished this session
- **In Progress**: What's currently being worked on
- **Next**: What should be done next
- **Blockers**: Any impediments discovered

### 3. Updates Session Log

Creates/updates: `internal-docs/ja/sessions/YYYY-MM-DD.md`

```markdown
## Session: HH:MM - HH:MM

### Completed
- [List of finished work]

### In Progress
- [Current work status]

### Next Steps
- [What to do next]

### Blockers
- [Any issues encountered]

### Notes
- [Important observations]
```text

### 4. Updates PROGRESS.md

If milestones were completed, updates completion percentage.

## Output Example

```text
## Session Update - 2026-02-14 15:30

### Work Summary
**Duration**: 2.5 hours
**Commits**: 3
**Files Changed**: 8

### Completed This Session
✅ Implemented YAML schema validator
✅ Added pre-commit hook for YAML syntax
✅ Updated RULES.md with R001

### In Progress
🔄 ADR-source validator Python script (70% done)
   - Font validation: ✅
   - Library validation: ✅
   - Schema validation: 🔄 (in progress)
   - Pattern validation: ⏳ (pending)

### Next Steps
1. Complete schema validation in ADR validator
2. Add unit tests for validator
3. Update CLAUDE.md with validator usage

### Blockers
None

### Notes
- PyYAML must be installed for YAML validation
- Consider adding gitleaks for better secret detection

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
✅ Session progress saved
→ Log: internal-docs/ja/sessions/2026-02-14.md
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
```text

## When to Use

**Use /session-update when:**
- ✅ Taking a break mid-session
- ✅ Accidentally exited Claude Code
- ✅ Switching between tasks
- ✅ Want to preserve context
- ✅ Need to document blockers

**Use /session-end when:**
- ✅ Finishing work for the day
- ✅ Ready to exit session completely
- ✅ Need comprehensive cleanup
- ✅ Want full session report

## Integration

Session updates are cumulative - multiple updates per day append to the same session log.

Final `/session-end` will incorporate all session-update data into comprehensive report.
