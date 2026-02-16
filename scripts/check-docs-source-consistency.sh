#!/usr/bin/env bash
# Documentation-Source Consistency Checker
# Verifies that documentation matches actual source code and configuration

set -e

echo "🔍 Checking documentation-source consistency..."
echo ""

ERRORS=0
WARNINGS=0

# ============================================================
# 1. YAML Configuration Schema Validation
# ============================================================
echo "📋 [1/5] Checking YAML configuration consistency..."
echo ""

# Check if config examples in docs match actual preset files
if [ -d "config/presets" ] && [ -f "docs/en/configuration.md" ]; then

  # Extract YAML keys from actual preset files
  ACTUAL_YAML_KEYS=$(find config/presets -name "*.yaml" -exec grep -h "^[A-Za-z]" {} \; | cut -d: -f1 | sort -u)

  # Extract documented YAML keys from configuration.md
  DOC_YAML_KEYS=$(grep -h -E "^\s*[A-Z][a-zA-Z]+:" docs/en/configuration.md docs/ja/configuration.md 2>/dev/null | sed 's/^[[:space:]]*//' | cut -d: -f1 | sort -u || echo "")

  # Find keys in YAML but not documented
  UNDOCUMENTED_KEYS=$(comm -23 <(echo "$ACTUAL_YAML_KEYS") <(echo "$DOC_YAML_KEYS") | grep -v "^$" || true)

  if [ -n "$UNDOCUMENTED_KEYS" ]; then
    echo "⚠️  Warning: YAML keys not documented in configuration.md:"
    echo "$UNDOCUMENTED_KEYS" | sed 's/^/   - /'
    ((WARNINGS++))
    echo ""
  fi

  # Find keys documented but not in actual YAML
  DOCUMENTED_ONLY=$(comm -13 <(echo "$ACTUAL_YAML_KEYS") <(echo "$DOC_YAML_KEYS") | grep -v "^$" || true)

  if [ -n "$DOCUMENTED_ONLY" ]; then
    echo "⚠️  Warning: Documented keys not found in YAML files:"
    echo "$DOCUMENTED_ONLY" | sed 's/^/   - /'
    ((WARNINGS++))
    echo ""
  fi

  if [ -z "$UNDOCUMENTED_KEYS" ] && [ -z "$DOCUMENTED_ONLY" ]; then
    echo "✅ YAML configuration keys match documentation"
    echo ""
  fi
else
  echo "⏭️  Skipped (config/presets or docs/en/configuration.md not found)"
  echo ""
fi

# ============================================================
# 2. Preset Files Inventory Check
# ============================================================
echo "📦 [2/5] Checking preset inventory consistency..."
echo ""

if [ -d "config/presets" ] && [ -f "docs/en/presets.md" ]; then

  # Get actual preset files
  ACTUAL_PRESETS=$(find config/presets -name "*.yaml" -exec basename {} .yaml \; | sort)

  # Get documented presets (extract from markdown lists or code blocks)
  DOC_PRESETS=$(grep -oE '`[a-z-]+\.yaml`|`[a-z-]+`|\*\*`[a-z-]+`\*\*' docs/en/presets.md | sed 's/[`*]//g' | sed 's/\.yaml$//' | sort -u || echo "")

  # Find presets not documented
  UNDOCUMENTED_PRESETS=$(comm -23 <(echo "$ACTUAL_PRESETS") <(echo "$DOC_PRESETS") | grep -v "^$" || true)

  if [ -n "$UNDOCUMENTED_PRESETS" ]; then
    echo "⚠️  Warning: Preset files not documented in presets.md:"
    echo "$UNDOCUMENTED_PRESETS" | sed 's/^/   - /'
    ((WARNINGS++))
    echo ""
  fi

  # Find documented presets that don't exist
  DOCUMENTED_ONLY_PRESETS=$(comm -13 <(echo "$ACTUAL_PRESETS") <(echo "$DOC_PRESETS") | grep -v "^$" || true)

  if [ -n "$DOCUMENTED_ONLY_PRESETS" ]; then
    echo "⚠️  Warning: Documented presets not found in config/presets/:"
    echo "$DOCUMENTED_ONLY_PRESETS" | sed 's/^/   - /'
    ((WARNINGS++))
    echo ""
  fi

  if [ -z "$UNDOCUMENTED_PRESETS" ] && [ -z "$DOCUMENTED_ONLY_PRESETS" ]; then
    echo "✅ Preset inventory matches documentation"
    echo ""
  fi
else
  echo "⏭️  Skipped (config/presets or docs/en/presets.md not found)"
  echo ""
fi

# ============================================================
# 3. CLI Options Validation
# ============================================================
echo "⚙️  [3/5] Checking CLI options consistency..."
echo ""

if [ -f "csharp-version/src/MarkdownToDocx.CLI/Program.cs" ] && [ -f "docs/en/getting-started.md" ]; then

  # Extract CLI options from Program.cs (looking for Option definitions)
  ACTUAL_CLI_OPTIONS=$(grep -oE 'Option<[^>]+>\s*\(\s*"--?[a-z-]+"' csharp-version/src/MarkdownToDocx.CLI/Program.cs 2>/dev/null | grep -oE '"--?[a-z-]+"' | tr -d '"' | sort -u || echo "")

  # Extract documented CLI options from getting-started.md
  DOC_CLI_OPTIONS=$(grep -oE '\-\-[a-z-]+|\-[a-z]' docs/en/getting-started.md docs/ja/getting-started.md 2>/dev/null | sort -u || echo "")

  # Find options not documented
  UNDOCUMENTED_OPTIONS=$(comm -23 <(echo "$ACTUAL_CLI_OPTIONS") <(echo "$DOC_CLI_OPTIONS") | grep -v "^$" || true)

  if [ -n "$UNDOCUMENTED_OPTIONS" ]; then
    echo "⚠️  Warning: CLI options not documented:"
    echo "$UNDOCUMENTED_OPTIONS" | sed 's/^/   - /'
    ((WARNINGS++))
    echo ""
  fi

  # Find documented options that don't exist
  DOCUMENTED_ONLY_OPTIONS=$(comm -13 <(echo "$ACTUAL_CLI_OPTIONS") <(echo "$DOC_CLI_OPTIONS") | grep -v "^$" || true)

  if [ -n "$DOCUMENTED_ONLY_OPTIONS" ]; then
    echo "⚠️  Warning: Documented CLI options not found in code:"
    echo "$DOCUMENTED_ONLY_OPTIONS" | sed 's/^/   - /'
    ((WARNINGS++))
    echo ""
  fi

  if [ -z "$UNDOCUMENTED_OPTIONS" ] && [ -z "$DOCUMENTED_ONLY_OPTIONS" ]; then
    echo "✅ CLI options match documentation"
    echo ""
  fi
else
  echo "⏭️  Skipped (Program.cs or getting-started.md not found)"
  echo ""
fi

# ============================================================
# 4. Code Examples Validation
# ============================================================
echo "💻 [4/5] Validating code examples in documentation..."
echo ""

CODE_EXAMPLE_ERRORS=0

# Extract bash code blocks from markdown and check syntax
for doc in docs/en/*.md docs/ja/*.md; do
  if [ -f "$doc" ]; then
    # Extract bash code blocks (between ```bash and ```)
    awk '/```bash/,/```/' "$doc" | grep -v '```' > /tmp/code_example_$$.sh 2>/dev/null || true

    if [ -s /tmp/code_example_$$.sh ]; then
      # Basic bash syntax check
      if ! bash -n /tmp/code_example_$$.sh 2>/dev/null; then
        echo "⚠️  Warning: Potential syntax error in bash code example in $doc"
        ((CODE_EXAMPLE_ERRORS++))
      fi
    fi

    rm -f /tmp/code_example_$$.sh
  fi
done

if [ $CODE_EXAMPLE_ERRORS -eq 0 ]; then
  echo "✅ Code examples syntax looks valid"
  echo ""
else
  echo "⚠️  Found $CODE_EXAMPLE_ERRORS potential syntax errors in code examples"
  ((WARNINGS++))
  echo ""
fi

# ============================================================
# 5. Version Number Consistency Check
# ============================================================
echo "🔢 [5/5] Checking version number consistency..."
echo ""

VERSION_SOURCES=()
VERSIONS=()

# README.md
if [ -f "README.md" ]; then
  README_VERSION=$(grep -oE 'version-[0-9]+\.[0-9]+\.[0-9]+' README.md | head -1 | sed 's/version-//')
  if [ -n "$README_VERSION" ]; then
    VERSION_SOURCES+=("README.md")
    VERSIONS+=("$README_VERSION")
  fi
fi

# docs/_config.yml
if [ -f "docs/_config.yml" ]; then
  CONFIG_VERSION=$(grep '^version:' docs/_config.yml | sed 's/version: *"\?\([0-9.]*\)"\?/\1/')
  if [ -n "$CONFIG_VERSION" ]; then
    VERSION_SOURCES+=("docs/_config.yml")
    VERSIONS+=("$CONFIG_VERSION")
  fi
fi

# .csproj files
if [ -d "csharp-version" ]; then
  CSPROJ_VERSION=$(grep -h '<Version>' csharp-version/src/*/*.csproj 2>/dev/null | head -1 | sed 's/.*<Version>\(.*\)<\/Version>/\1/' | tr -d ' ')
  if [ -n "$CSPROJ_VERSION" ]; then
    VERSION_SOURCES+=("*.csproj")
    VERSIONS+=("$CSPROJ_VERSION")
  fi
fi

# Check if all versions match
UNIQUE_VERSIONS=$(printf '%s\n' "${VERSIONS[@]}" | sort -u | wc -l | tr -d ' ')

if [ "$UNIQUE_VERSIONS" -eq 1 ]; then
  echo "✅ Version numbers consistent across all files: ${VERSIONS[0]}"
  echo ""
elif [ "$UNIQUE_VERSIONS" -gt 1 ]; then
  echo "❌ Error: Version number mismatch!"
  for i in "${!VERSION_SOURCES[@]}"; do
    echo "   ${VERSION_SOURCES[$i]}: ${VERSIONS[$i]}"
  done
  ((ERRORS++))
  echo ""
  echo "💡 Fix: Run ./scripts/update-version.sh <version>"
  echo ""
fi

# ============================================================
# Summary
# ============================================================
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
if [ $ERRORS -eq 0 ] && [ $WARNINGS -eq 0 ]; then
  echo "✅ Documentation-source consistency check passed!"
  echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
  exit 0
elif [ $ERRORS -eq 0 ]; then
  echo "⚠️  Consistency check completed with $WARNINGS warning(s)"
  echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
  echo ""
  echo "💡 Recommendations:"
  echo "1. Document all YAML configuration keys"
  echo "2. Ensure all preset files are listed in documentation"
  echo "3. Document all CLI options"
  echo "4. Validate code examples"
  echo ""
  exit 0
else
  echo "❌ Consistency check failed with $ERRORS error(s) and $WARNINGS warning(s)"
  echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
  echo ""
  echo "🔧 Required fixes:"
  echo "1. Fix version number mismatches"
  exit 1
fi
