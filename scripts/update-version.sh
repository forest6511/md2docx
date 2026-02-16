#!/usr/bin/env bash
# Version Update Script
# Updates version numbers across all project files

set -e

VERSION=$1

if [ -z "$VERSION" ]; then
  echo "❌ Error: Version number required"
  echo "Usage: ./scripts/update-version.sh 0.2.0"
  exit 1
fi

# Validate version format (semantic versioning)
if ! [[ $VERSION =~ ^[0-9]+\.[0-9]+\.[0-9]+$ ]]; then
  echo "❌ Error: Invalid version format. Use semantic versioning (e.g., 0.2.0)"
  exit 1
fi

echo "🔄 Updating version to $VERSION..."
echo ""

# 1. README.md - Version badge
if [ -f README.md ]; then
  sed -i '' "s/version-[0-9.]*-blue/version-$VERSION-blue/" README.md
  echo "✅ Updated README.md version badge"
fi

# 2. docs/_config.yml - Jekyll site version
if [ -f docs/_config.yml ]; then
  # Add version field if not exists
  if ! grep -q "^version:" docs/_config.yml; then
    echo "version: \"$VERSION\"" >> docs/_config.yml
    echo "✅ Added version to docs/_config.yml"
  else
    sed -i '' "s/^version: \"[0-9.]*\"/version: \"$VERSION\"/" docs/_config.yml
    echo "✅ Updated docs/_config.yml version"
  fi
fi

# 3. docs/index.md - Version badge
if [ -f docs/index.md ]; then
  sed -i '' "s/version-[0-9.]*-blue/version-$VERSION-blue/" docs/index.md
  echo "✅ Updated docs/index.md version badge"
fi

# 4. .csproj files - NuGet package version
CSPROJ_COUNT=0
while IFS= read -r -d '' file; do
  if grep -q "<Version>" "$file"; then
    sed -i '' "s/<Version>[0-9.]*<\/Version>/<Version>$VERSION<\/Version>/" "$file"
    echo "✅ Updated $file"
    ((CSPROJ_COUNT++))
  fi
done < <(find csharp-version -name "*.csproj" -print0 2>/dev/null)

if [ $CSPROJ_COUNT -eq 0 ]; then
  echo "⚠️  Warning: No .csproj files with <Version> tag found"
fi

# 5. CHANGELOG.md - Add new version section (if exists)
if [ -f CHANGELOG.md ]; then
  # Check if version already exists
  if grep -q "## \[$VERSION\]" CHANGELOG.md; then
    echo "ℹ️  Version $VERSION already exists in CHANGELOG.md"
  else
    # Insert new version section after the first line (# Changelog)
    sed -i '' "1 a\\
\\
## [$VERSION] - $(date +%Y-%m-%d)\\
\\
### Added\\
- \\
\\
### Changed\\
- \\
\\
### Fixed\\
- \\
" CHANGELOG.md
    echo "✅ Added version section to CHANGELOG.md"
  fi
fi

echo ""
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "✅ Version update complete!"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo ""
echo "📋 Next steps:"
echo "1. Review changes: git diff"
echo "2. Update CHANGELOG.md with actual changes"
echo "3. Commit: git add -A && git commit -m 'chore: bump version to $VERSION'"
echo "4. Create tag: git tag v$VERSION"
echo "5. Push: git push && git push --tags"
