#!/bin/bash
# Install Git Hooks
# Run this script to install Claude Code hooks for this project

set -e

PROJECT_ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
HOOKS_DIR="$PROJECT_ROOT/.claude/hooks"
GIT_HOOKS_DIR="$PROJECT_ROOT/.git/hooks"

echo "🪝 Installing Git hooks..."
echo "   Project: $PROJECT_ROOT"
echo ""

# Create .git/hooks directory if it doesn't exist
mkdir -p "$GIT_HOOKS_DIR"

# Install each hook
# Format: "hook_name:script_name" pairs
HOOK_PAIRS=(
  "pre-commit:pre-commit-check.sh"
  "pre-push:pre-push.sh"
  "post-merge:post-merge.sh"
)

for pair in "${HOOK_PAIRS[@]}"; do
  # Split "hook_name:script_name"
  IFS=':' read -r hook script <<< "$pair"

  SOURCE="$HOOKS_DIR/$script"
  TARGET="$GIT_HOOKS_DIR/$hook"

  if [ -f "$SOURCE" ]; then
    echo "Installing: $hook"

    # Copy file (not symlink)
    cp "$SOURCE" "$TARGET"

    # Make executable
    chmod +x "$TARGET"

    echo "   ✓ $hook installed"
  else
    echo "   ⚠️  $hook ($script) not found, skipping"
  fi
done

echo ""
echo "✅ Git hooks installed successfully!"
echo ""
echo "Installed hooks:"
echo "   • pre-commit:  YAML syntax, secrets, font license, markdown lint"
echo "   • pre-push:    Code review, build, tests, ADR validation (MANDATORY)"
echo "   • post-merge:  Dependency updates, environment sync"
echo ""
echo "To bypass hooks (NOT recommended):"
echo "   git commit --no-verify"
echo "   git push --no-verify"
echo ""
