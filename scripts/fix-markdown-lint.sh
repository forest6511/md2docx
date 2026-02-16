#!/bin/bash
# Markdown Lint Error Fix Script
# Efficiently fixes common markdown lint errors

set -e

echo "🔧 Fixing Markdown lint errors..."

# Find all markdown files (excluding internal docs and node_modules)
MD_FILES=$(find . -name "*.md" ! -path "./internal-docs/*" ! -path "./CLAUDE.local.md" ! -path "./node_modules/*" -type f)

for file in $MD_FILES; do
  echo "Processing: $file"

  # Create backup
  cp "$file" "$file.bak"

  # Fix MD040: Add 'text' to code blocks without language
  # This is a simple heuristic - may need manual review
  sed -i '' 's/^```$/```text/g' "$file"

  # Fix MD022: Add blank line before headings (if missing)
  # Only add if previous line is not blank and not a heading
  perl -i -pe 's/^([^#\n].+)\n(#{1,6} )/$1\n\n$2/g' "$file"

  # Fix MD022: Add blank line after headings (if missing)
  # Only add if next line is not blank
  perl -i -pe 's/^(#{1,6} .+)\n([^#\n])/$1\n\n$2/g' "$file"

  # Fix MD032: Add blank line before lists
  perl -i -pe 's/^([^-*\n].+)\n([-*] )/$1\n\n$2/g' "$file"

  # Fix MD032: Add blank line after lists
  perl -i -pe 's/^([-*] .+)\n([^-*\n])/$1\n\n$2/g' "$file"

  echo "  ✓ Fixed: $file"
done

echo ""
echo "🔄 Running markdownlint auto-fix..."
if command -v markdownlint-cli2 &> /dev/null; then
  markdownlint-cli2 --fix --config .markdownlint.jsonc "**/*.md" "!internal-docs/**" "!CLAUDE.local.md" "!node_modules/**" 2>&1 || true
else
  npx -y markdownlint-cli2 --fix --config .markdownlint.jsonc "**/*.md" "!internal-docs/**" "!CLAUDE.local.md" "!node_modules/**" 2>&1 || true
fi

echo ""
echo "✅ Done! Checking remaining errors..."
if command -v markdownlint-cli2 &> /dev/null; then
  markdownlint-cli2 --config .markdownlint.jsonc "**/*.md" "!internal-docs/**" "!CLAUDE.local.md" "!node_modules/**" 2>&1 | grep "Summary:" || echo "No errors!"
else
  npx -y markdownlint-cli2 --config .markdownlint.jsonc "**/*.md" "!internal-docs/**" "!CLAUDE.local.md" "!node_modules/**" 2>&1 | grep "Summary:" || echo "No errors!"
fi

echo ""
echo "📁 Backup files created with .bak extension"
echo "   To remove backups: find . -name '*.md.bak' -delete"
