#!/bin/bash
# Documentation Sync Checker
# Verifies English and Japanese documentation are in sync

set -e

echo "🔍 Checking documentation synchronization..."
echo ""

ERRORS=0
WARNINGS=0

# Check if docs directories exist
if [ ! -d "docs/en" ] || [ ! -d "docs/ja" ]; then
  echo "❌ Error: docs/en or docs/ja directory not found"
  exit 1
fi

# 1. File count check
EN_COUNT=$(find docs/en -name "*.md" -type f | wc -l | tr -d ' ')
JA_COUNT=$(find docs/ja -name "*.md" -type f | wc -l | tr -d ' ')

echo "📊 File Counts:"
echo "   English: $EN_COUNT files"
echo "   Japanese: $JA_COUNT files"
echo ""

if [ "$EN_COUNT" -ne "$JA_COUNT" ]; then
  echo "⚠️  Warning: File count mismatch"
  ((WARNINGS++))

  # Find missing files
  EN_FILES=$(cd docs/en && find . -name "*.md" -type f | sort)
  JA_FILES=$(cd docs/ja && find . -name "*.md" -type f | sort)

  MISSING_IN_JA=$(comm -23 <(echo "$EN_FILES") <(echo "$JA_FILES"))
  MISSING_IN_EN=$(comm -13 <(echo "$EN_FILES") <(echo "$JA_FILES"))

  if [ -n "$MISSING_IN_JA" ]; then
    echo ""
    echo "📋 Files in en/ but missing in ja/:"
    echo "$MISSING_IN_JA" | sed 's/^/   /'
  fi

  if [ -n "$MISSING_IN_EN" ]; then
    echo ""
    echo "📋 Files in ja/ but missing in en/:"
    echo "$MISSING_IN_EN" | sed 's/^/   /'
  fi
  echo ""
else
  echo "✅ File counts match"
  echo ""
fi

# 2. File name consistency check
echo "🔍 Checking file name consistency..."
EN_FILES=$(cd docs/en && find . -name "*.md" -type f | sort)
JA_FILES=$(cd docs/ja && find . -name "*.md" -type f | sort)

# Files should have the same names (paths)
if [ "$EN_FILES" != "$JA_FILES" ]; then
  echo "⚠️  Warning: File structure mismatch"
  ((WARNINGS++))
else
  echo "✅ File structures match"
fi
echo ""

# 3. Check for "Last Updated" markers
echo "🔍 Checking 'Last Updated' markers..."
MISSING_DATE_COUNT=0

for file in docs/en/*.md docs/ja/*.md; do
  if [ -f "$file" ]; then
    if ! grep -q "Last Updated" "$file" && ! grep -q "最終更新" "$file"; then
      echo "⚠️  Missing date marker: $file"
      ((MISSING_DATE_COUNT++))
    fi
  fi
done

if [ $MISSING_DATE_COUNT -eq 0 ]; then
  echo "✅ All files have date markers"
else
  echo "⚠️  $MISSING_DATE_COUNT files missing date markers"
  ((WARNINGS++))
fi
echo ""

# 4. Check for broken internal links (basic check)
echo "🔍 Checking for potential broken links..."
BROKEN_LINKS=0

# Check for common broken link patterns
for file in docs/en/*.md docs/ja/*.md; do
  if [ -f "$file" ]; then
    # Check for links to non-existent files
    while IFS= read -r link; do
      # Extract relative path from [text](path)
      if echo "$link" | grep -qE '\]\([^)]+\)'; then
        link_path=$(echo "$link" | sed 's/.*\](\([^)]*\)).*/\1/')

        # Skip external links and anchors
        if echo "$link_path" | grep -qE '^https?://'; then
          continue
        fi
        if echo "$link_path" | grep -qE '^#'; then
          continue
        fi

        # Resolve relative path
        dir=$(dirname "$file")
        target="$dir/$link_path"

        if [ ! -f "$target" ]; then
          echo "⚠️  Broken link in $file: $link_path"
          ((BROKEN_LINKS++))
        fi
      fi
    done < <(grep -o '\[.*\](.*\.md)' "$file" 2>/dev/null || true)
  fi
done

if [ $BROKEN_LINKS -eq 0 ]; then
  echo "✅ No broken internal links detected"
else
  echo "⚠️  $BROKEN_LINKS potential broken links found"
  ((WARNINGS++))
fi
echo ""

# Summary
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
if [ $ERRORS -eq 0 ] && [ $WARNINGS -eq 0 ]; then
  echo "✅ Documentation sync check passed!"
  echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
  exit 0
elif [ $ERRORS -eq 0 ]; then
  echo "⚠️  Documentation sync check completed with $WARNINGS warning(s)"
  echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
  echo ""
  echo "💡 Recommendations:"
  echo "1. Ensure all English documents have Japanese translations"
  echo "2. Add 'Last Updated: YYYY-MM-DD' to files missing date markers"
  echo "3. Fix broken internal links"
  exit 0
else
  echo "❌ Documentation sync check failed with $ERRORS error(s) and $WARNINGS warning(s)"
  echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
  exit 1
fi
