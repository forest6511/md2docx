#!/bin/bash
# Post-Merge Hook - Update Dependencies After Merge

echo "🔄 Post-merge: Checking for updates..."

# ============================================================
# .NET Dependencies
# ============================================================
if [ -f "csharp-version/src/MarkdownToDocx.sln" ]; then
  echo "📦 Checking .NET dependencies..."

  cd csharp-version

  # Check if packages.lock.json changed
  if git diff-tree -r --name-only --no-commit-id ORIG_HEAD HEAD | grep -q 'packages.lock.json'; then
    echo "   Dependencies changed, restoring..."
    dotnet restore --nologo
    echo "✅ Dependencies updated"
  else
    echo "   No dependency changes"
  fi

  cd ..
fi

# ============================================================
# Docker Image
# ============================================================
if git diff-tree -r --name-only --no-commit-id ORIG_HEAD HEAD | grep -q 'Dockerfile'; then
  echo "🐳 Dockerfile changed"
  echo "   Consider rebuilding Docker image:"
  echo "   ./scripts/build-docker.sh"
fi

# ============================================================
# Configuration Files
# ============================================================
if git diff-tree -r --name-only --no-commit-id ORIG_HEAD HEAD | grep -q 'config/.*\.yaml'; then
  echo "⚙️  Configuration files changed"
  echo "   Review new presets in config/"
fi

echo "✅ Post-merge complete"
