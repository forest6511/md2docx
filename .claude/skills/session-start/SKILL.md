---
name: session-start
description: Comprehensive project status dashboard - manually check context, ADRs, quality metrics, and recommended tasks
---

# Session Start

Comprehensive project status check and context loading.

**Note**: A lightweight version runs automatically via hook at session start. Use this skill for:

- Manual status refresh mid-session
- Detailed dashboard view
- Re-checking context after hook failure
- Planning next tasks

## Usage

```text
/session-start
```text

## What It Provides

### 1. Git Status & Branch Strategy

```bash
git status
git branch --show-current
git log --oneline -5
git branch --list
```text

**Output**:
- Current branch
- Uncommitted changes count
- Recent commits
- Branch strategy recommendation

**Warnings**:
- Working on main branch
- Uncommitted changes present
- Unpushed commits
- Stale feature branches

### 2. Project Progress

**From PROGRESS.md**:
- Current sprint/phase
- Completion percentage
- Completed milestones
- In-progress tasks
- Upcoming milestones
- Blockers

**Calculates**:
- Velocity (milestones/week)
- Estimated completion date
- Recent progress trend

### 3. ADR Status

**Checks**:
```bash
ls -la docs/decisions/ADR-*.md
grep "Status:" docs/decisions/ADR-*.md
```text

**Categorizes**:
- **Proposed**: Pending user approval (action needed)
- **Accepted**: Implementation required
- **Deprecated**: Candidates for removal
- **Superseded**: Old implementation cleanup needed

**Recent ADRs**: Shows last 3 ADRs with status

### 4. Quality Metrics

**Test Coverage**:
```bash
# Parse coverage.cobertura.xml
python3 -c "..."
```text
- Current coverage percentage
- Target: 80%
- Trend (if historical data available)

**Code Quality**:
- Markdown lint status
- C# format compliance (if applicable)
- YAML syntax validation results

**ADR Consistency**:
- Last validation result
- Compliance percentage
- Known violations

### 5. YAML Presets Status

```bash
find config/presets -name "*.yaml"
find config/publishing -name "*.yaml"
find config/vertical -name "*.yaml"
```text

**Checks**:
- Total preset count
- Syntax validation status
- Schema version consistency
- Recent changes

### 6. Docker Image Status

```bash
docker images md2word:latest --format "..."
docker images md2word:slim --format "..."
ls -l Dockerfile Dockerfile.slim
```text

**Shows**:
- Image sizes (latest, slim)
- Build status (exists/not built)
- Dockerfile last modified
- Size targets comparison

### 7. Documentation Health

**Quick Audit**:
- CLAUDE.md last updated
- README.md YAML examples validity
- Internal links status
- Version number consistency

### 8. GitHub Issues

```bash
gh issue list --state open --limit 20
```text

**Categorizes**:
- Bugs (high priority)
- Enhancements
- Documentation
- ADR-related

**Shows**:
- Total open issues
- Recent activity
- Assigned issues

### 9. Development Environment

**Checks**:
```bash
dotnet --version
docker ps
python3 --version
markdownlint-cli2 --version
```text

**Status**:
- .NET SDK installed/version
- Docker running/stopped
- Python available
- Linting tools available

### 10. Recommended Tasks

**Based on**:
- Open issues priority
- ADRs pending action
- Test coverage gaps
- Incomplete milestones
- Recent commit patterns

**Suggests 3 tasks**:
1. Highest priority (with rationale)
2. Next recommended
3. Optional/future work

## Output Example

```text
╔════════════════════════════════════════════════════════════╗
║            SESSION START DASHBOARD                         ║
║            2026-02-14 15:30                                ║
╚════════════════════════════════════════════════════════════╝

📂 GIT STATUS
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
   Branch: main
   ⚠️  Working on main - recommend creating feature branch

   Uncommitted: 0 files
   Recent commits:
   - abc1234 feat: Add quality assurance system
   - def5678 docs: Update CLAUDE.md with workflow
   - ghi9012 refactor: Simplify session hooks

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

📊 PROJECT PROGRESS
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
   Phase: Foundation & Architecture
   Completion: 75% (15/20 milestones)

   ✅ Completed (15):
   - Project structure designed
   - YAML schema defined
   - Docker strategy finalized
   - Quality assurance system
   ... (11 more)

   🔄 In Progress (2):
   - YAML config loader (40% done)
   - Basic Markdown → DOCX converter (20% done)

   ⏳ Upcoming (3):
   - Styling engine implementation
   - Create presets
   - Docker image build

   Velocity: 3.5 milestones/week
   Estimated completion: 2026-02-20

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

📋 ADR STATUS
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
   Total: 3 ADRs

   Status Breakdown:
   ⏳ Proposed: 0 (no action needed)
   ✅ Accepted: 3 (implementation required)

   Recent ADRs:
   1. ADR-0012: Use Noto Serif JP (Accepted)
   2. ADR-0015: DocumentFormat.OpenXml (Accepted)
   3. ADR-0020: YAML Schema v2.0 (Accepted)

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

🧪 QUALITY METRICS
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
   Test Coverage: N/A (no tests yet)
   Target: 80%
   Status: ⚠️  Testing not started

   Code Quality:
   ✅ Markdown Lint: 0 errors
   ✅ YAML Syntax: All valid (23 files)
   ⏳ C# Format: N/A (no code yet)

   ADR Consistency:
   ⏳ Not validated yet
   Recommendation: Run /adr-source-validator

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

📦 YAML PRESETS
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
   Presets: 15 files
   ✅ Syntax: All valid
   ⚠️  Schema: 3 files need update to v2.0

   Distribution:
   - config/presets/: 8 files
   - config/publishing/: 4 files
   - config/vertical/: 3 files

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

🐳 DOCKER STATUS
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
   md2word:latest - Not built yet
   md2word:slim - Not built yet

   Dockerfile last modified: 2 days ago

   Recommendation: Build images after core implementation

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

📝 DOCUMENTATION HEALTH
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
   CLAUDE.md: ✅ Updated today
   README.md: ✅ YAML examples valid
   RULES.md: ✅ Recently created
   PROGRESS.md: ✅ Auto-updated

   Quick Check:
   ✅ Internal links valid
   ✅ Version numbers consistent

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

🎫 GITHUB ISSUES
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
   Open: 5 issues

   By Type:
   🐛 Bug: 2
   ✨ Enhancement: 2
   📚 Documentation: 1

   Recent:
   #12: Implement YAML schema validation
   #11: Add vertical text support
   #10: Docker multi-arch build

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

🔧 DEVELOPMENT ENVIRONMENT
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
   ✅ .NET SDK: 8.0.1
   ✅ Docker: Running
   ✅ Python3: 3.11.5
   ✅ markdownlint-cli2: 0.12.1

   All required tools available

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

🎯 RECOMMENDED TASKS FOR TODAY
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

1. 🥇 HIGHEST PRIORITY: Complete YAML Config Loader

   Why: Blocking milestone (40% done)
   Files: csharp-version/src/MarkdownToDocx.Styling/ConfigLoader.cs
   Steps:
   - Finish schema validation
   - Add unit tests
   - Test with all presets
   Estimated: 2-3 hours

2. 🥈 NEXT: Start Basic Markdown Parser

   Why: Next sequential milestone
   Steps:
   - Evaluate markdown-it vs custom parser
   - Implement basic AST handling
   - Create first integration test
   Estimated: 3-4 hours

3. 🥉 OPTIONAL: Validate Accepted ADRs

   Why: 3 Accepted ADRs need implementation check
   Command: /adr-source-validator
   Estimated: 15 minutes

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

💡 REMINDERS
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
   ⚠️  Create feature branch before starting work
   ✅ Codex review required before push
   ✅ Test coverage target: ≥80%
   ✅ ADR required for architectural changes
   ✅ Commercial fonts prohibited (Noto only)

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

📖 AVAILABLE COMMANDS
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
   /session-start           - This dashboard (manual refresh)
   /session-update          - Save progress mid-session
   /session-end             - Comprehensive session closure
   /adr-source-validator    - Validate ADR consistency
   /check-docs-consistency  - Check documentation sync

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Ready to work! Suggested: Create feature/yaml-config-loader branch
```text

## When to Use

**Use `/session-start` when:**
- ✅ Want full project status overview
- ✅ Hook execution failed
- ✅ Mid-session context refresh needed
- ✅ Planning next tasks
- ✅ After long break

**Hook runs automatically when:**
- ✅ Claude Code session starts
- ✅ Lightweight check only

## Integration

- More comprehensive than automatic hook
- Includes task recommendations
- Shows historical trends
- Provides detailed breakdowns
- Suggests specific next actions
