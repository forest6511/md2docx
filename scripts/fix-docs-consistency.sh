#!/bin/bash
# Auto-fix documentation consistency issues
# Generated: 2026-02-16

set -e

echo "🔧 Fixing documentation consistency issues..."
echo ""

# 1. Fix version in Dockerfiles
echo "📦 Updating Dockerfile versions (1.0.0 → 0.1.0)..."
for dockerfile in Dockerfile Dockerfile.slim Dockerfile.full Dockerfile.dev; do
  if [ -f "$dockerfile" ]; then
    if grep -q 'LABEL version="1.0.0"' "$dockerfile"; then
      sed -i '' 's/LABEL version="1.0.0"/LABEL version="0.1.0"/' "$dockerfile"
      echo "   ✅ $dockerfile updated"
    else
      echo "   ⏭️  $dockerfile (no change needed)"
    fi
  fi
done
echo ""

# 2. Fix broken link in CONTRIBUTING.md
echo "🔗 Fixing broken links..."
if [ -f "CONTRIBUTING.md" ]; then
  if grep -q 'STATUS\.md' CONTRIBUTING.md; then
    sed -i '' 's/STATUS\.md/PROGRESS.md/g' CONTRIBUTING.md
    echo "   ✅ CONTRIBUTING.md: STATUS.md → PROGRESS.md"
  else
    echo "   ⏭️  CONTRIBUTING.md (no broken links)"
  fi
fi
echo ""

echo "✅ Auto-fixes complete!"
echo ""
echo "📋 Manual steps remaining:"
echo "  1. Add <Version>0.1.0</Version> to csproj PropertyGroup"
echo "  2. Add version badge to README.md"
echo "  3. Commit: git commit -m 'docs: fix consistency issues'"
echo ""
