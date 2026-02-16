#!/bin/bash
# Pre-Push Hook with Mandatory Codex Review
# Ensures code quality before pushing to remote

set -e

echo "🔍 Running pre-push validation..."
echo ""

# Get the remote and branch being pushed to
remote="$1"
url="$2"

# Get changed files compared to origin/main
CHANGED_FILES=$(git diff --name-only --diff-filter=d origin/main...HEAD 2>/dev/null | grep -E '\.(cs|csproj|yaml|yml|json)$' || true)

if [ -z "$CHANGED_FILES" ]; then
  echo "ℹ️  No relevant files changed (*.cs, *.yaml, *.json)"
  echo "   Skipping code review"
  echo ""
else
  echo "📊 Files to review:"
  echo "$CHANGED_FILES" | sed 's/^/   - /'
  echo ""

  # ============================================================
  # MANDATORY: Codex Code Review
  # ============================================================
  echo "🤖 Codex Review..."
  echo ""

  # TODO: Implement actual Codex review skill
  # Current status: Temporarily using alternative static analysis
  # See: ADR-XXXX for Codex integration strategy

  # Check if codex skill is available
  if command -v claude-code &> /dev/null && claude-code list-skills 2>/dev/null | grep -q "codex-review"; then
    echo "   Running Codex review..."

    REVIEW_OUTPUT=$(mktemp)

    if claude-code run-skill codex-review \
        --files "$CHANGED_FILES" \
        --severity high \
        --format summary \
        > "$REVIEW_OUTPUT" 2>&1; then

      echo "✅ Codex review passed"
      cat "$REVIEW_OUTPUT"
      rm "$REVIEW_OUTPUT"
    else
      echo ""
      echo "❌ Codex review found issues:"
      echo ""
      cat "$REVIEW_OUTPUT"
      rm "$REVIEW_OUTPUT"
      echo ""
      echo "📋 Next steps:"
      echo "   1. Review the issues above"
      echo "   2. Fix the problems"
      echo "   3. Commit the fixes"
      echo "   4. Try pushing again"
      echo ""
      exit 1
    fi
  else
    # Fallback: Use dotnet format verification
    echo "   ⚠️  Codex skill not available, using dotnet format verification"

    # Check if C# code exists
    CS_FILE_COUNT=$(find csharp-version/src -name "*.cs" 2>/dev/null | wc -l)

    if [ "$CS_FILE_COUNT" -gt "0" ] && [ -f "csharp-version/src/MarkdownToDocx.sln" ]; then
      if ! dotnet format csharp-version/src/MarkdownToDocx.sln --verify-no-changes --verbosity quiet 2>/dev/null; then
        echo ""
        echo "❌ Code formatting issues detected"
        echo "   Run: dotnet format csharp-version/src/MarkdownToDocx.sln"
        echo ""
        exit 1
      fi

      echo "   ✅ Code formatting verified"
    else
      echo "   ⏭️  No C# code found, skipping format check"
    fi
  fi
  echo ""
fi

# ============================================================
# R201: Public Repository Safety Check (CRITICAL)
# ============================================================
echo "🔒 Public repository safety check..."
echo ""

# Protected patterns that should NEVER be in public repository
PROTECTED_PATTERNS=(
  "internal-docs/"
  "CLAUDE.local.md"
  "^\.env$"
  "^\.env\."
  "\.backup$"
  "credentials\."
  "secrets\."
)

# Get commits being pushed (from origin/main to HEAD)
COMMITS_TO_PUSH=$(git log origin/main..HEAD --oneline 2>/dev/null || git log --oneline -10)

if [ -z "$COMMITS_TO_PUSH" ]; then
  echo "   ℹ️  No new commits to push"
else
  echo "   📋 Commits to push:"
  echo "$COMMITS_TO_PUSH" | head -5 | sed 's/^/      /'

  if [ "$(echo "$COMMITS_TO_PUSH" | wc -l)" -gt 5 ]; then
    echo "      ... and $(($(echo "$COMMITS_TO_PUSH" | wc -l) - 5)) more"
  fi
  echo ""

  # Scan each commit for protected files
  PROTECTED_FILES_FOUND=0

  while IFS= read -r commit_line; do
    commit_hash=$(echo "$commit_line" | awk '{print $1}')

    # Get files in this commit (added or modified only, not deleted)
    FILES_IN_COMMIT=$(git diff-tree --no-commit-id --name-only --diff-filter=AM -r "$commit_hash" 2>/dev/null)

    # Check each file against protected patterns
    for pattern in "${PROTECTED_PATTERNS[@]}"; do
      if echo "$FILES_IN_COMMIT" | grep -qE "$pattern"; then
        if [ "$PROTECTED_FILES_FOUND" -eq "0" ]; then
          echo "   ❌ R201 VIOLATION: Protected files detected in commit history"
          echo ""
        fi

        MATCHING_FILES=$(echo "$FILES_IN_COMMIT" | grep -E "$pattern")
        echo "   ${commit_hash}: Protected file(s) found:"
        echo "$MATCHING_FILES" | sed 's/^/      - /'
        echo ""

        PROTECTED_FILES_FOUND=1
      fi
    done
  done <<< "$COMMITS_TO_PUSH"

  if [ "$PROTECTED_FILES_FOUND" -eq "1" ]; then
    echo "   💡 Fix: Remove protected files from Git history"
    echo ""
    echo "   Method 1 - Remove from tracking (recommended):"
    echo "      git rm --cached <file>"
    echo "      git commit --amend"
    echo ""
    echo "   Method 2 - Interactive rebase (advanced):"
    echo "      git rebase -i origin/main"
    echo "      # Mark commits with 'edit', remove protected files"
    echo ""
    echo "   Method 3 - Create new commit removing files:"
    echo "      git rm --cached <file>"
    echo "      git commit -m 'chore: remove protected files from tracking'"
    echo ""
    echo "   ⚠️  After fixing, ensure files are in .gitignore"
    echo ""
    exit 1
  else
    echo "   ✅ R201 Passed: No protected files in commit history"
  fi
fi
echo ""

# ============================================================
# Build Verification
# ============================================================
# Check if C# code exists
CS_FILE_COUNT_BUILD=$(find csharp-version/src -name "*.cs" 2>/dev/null | wc -l)

if [ "$CS_FILE_COUNT_BUILD" -gt "0" ] && [ -f "csharp-version/src/MarkdownToDocx.sln" ]; then
  echo "🔨 Build verification..."

  dotnet build csharp-version/src/MarkdownToDocx.sln -c Release --nologo --verbosity quiet || {
    echo "❌ Build failed"
    echo "   Fix build errors before pushing"
    exit 1
  }

  echo "✅ Build successful"
  echo ""
else
  echo "🔨 Build verification..."
  echo "   ⏭️  No C# code found, skipping build"
  echo ""
fi

# ============================================================
# Unit Tests
# ============================================================
# Check if test files exist
TEST_FILE_COUNT=$(find csharp-version/tests -name "*.cs" 2>/dev/null | wc -l)

if [ "$TEST_FILE_COUNT" -gt "0" ] && [ -d "csharp-version/tests" ]; then
  echo "🧪 Running unit tests..."

  dotnet test csharp-version/src/MarkdownToDocx.sln \
    --filter "Category=Unit" \
    --nologo \
    --verbosity quiet \
    --no-build \
    -c Release || {
    echo "❌ Unit tests failed"
    echo "   All tests must pass before pushing"
    exit 1
  }

  echo "✅ Unit tests passed"
  echo ""
else
  echo "🧪 Running unit tests..."
  echo "   ⏭️  No test files found, skipping tests"
  echo ""
fi

# ============================================================
# YAML Validation
# ============================================================
CHANGED_YAML=$(echo "$CHANGED_FILES" | grep -E '\.(yaml|yml)$' || true)

if [ -n "$CHANGED_YAML" ]; then
  echo "📝 Validating YAML files..."

  for yaml_file in $CHANGED_YAML; do
    if [ -f "$yaml_file" ]; then
      # Basic YAML syntax check using Python
      python3 -c "import yaml; yaml.safe_load(open('$yaml_file'))" 2>/dev/null || {
        echo "❌ YAML syntax error in: $yaml_file"
        exit 1
      }
      echo "   ✓ $yaml_file"
    fi
  done

  echo "✅ YAML validation passed"
  echo ""
fi

# ============================================================
# Dockerfile Validation
# ============================================================
CHANGED_DOCKER=$(echo "$CHANGED_FILES" | grep 'Dockerfile' || true)

if [ -n "$CHANGED_DOCKER" ]; then
  echo "🐳 Dockerfile validation..."

  for dockerfile in $CHANGED_DOCKER; do
    if [ -f "$dockerfile" ]; then
      # Check if dockerfile uses FROM
      if ! grep -q '^FROM' "$dockerfile"; then
        echo "❌ Invalid Dockerfile: $dockerfile"
        echo "   Missing FROM instruction"
        exit 1
      fi
      echo "   ✓ $dockerfile"
    fi
  done

  echo "✅ Dockerfile validation passed"
  echo ""
fi

# ============================================================
# Success Summary
# ============================================================
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "✅ All pre-push checks passed!"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo ""
echo "📤 Pushing to: $remote"
echo ""

exit 0
