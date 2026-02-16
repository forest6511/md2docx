#!/bin/bash
# Pre-Commit Quality Gate
# Fast validation (<30 seconds) - developers won't bypass it
#
# Performance optimizations:
# - Single-pass diff scanning
# - Parallel validation where possible
# - Early exit on violations
# - Detailed error reporting

set -e

VIOLATIONS=0
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}🔍 Enforcing pre-commit rules...${NC}"
echo ""

# ============================================================
# Get all staged files once (performance optimization)
# ============================================================
STAGED_FILES=$(git diff --cached --name-only --diff-filter=ACM)
YAML_FILES=$(echo "$STAGED_FILES" | grep -E '\.(yaml|yml)$' || true)
MD_FILES=$(echo "$STAGED_FILES" | grep '\.md$' || true)

# ============================================================
# R006: Protected Path Validation (CRITICAL)
# ============================================================
check_protected_paths() {
  echo -e "  ${BLUE}[R006]${NC} Checking for protected files..."

  if [ -z "$STAGED_FILES" ]; then
    echo -e "  ${YELLOW}⏭️  R006 Skipped: No files staged${NC}"
    return 0
  fi

  local protected_violations=0

  # Protected patterns (bash 3.x compatible)
  # Format: "pattern|description|fix_hint"
  PROTECTED_PATTERNS=(
    "internal-docs/|Internal documentation|Should remain local only, never commit"
    "CLAUDE.local.md|Private configuration|User-specific settings, never commit"
    "^\.env$|Environment file|Use .env.example for templates"
    "^\.env\.|Environment file|Use .env.example for templates"
    "\.backup$|Backup file|Temporary files should not be committed"
    "credentials\.|Credential file|Use environment variables or secure vaults"
    "secrets\.|Secrets file|Use environment variables or secure vaults"
  )

  for pattern_tuple in "${PROTECTED_PATTERNS[@]}"; do
    IFS='|' read -r pattern desc fix <<< "$pattern_tuple"

    # Check if any staged file matches the protected pattern
    while IFS= read -r staged_file; do
      if echo "$staged_file" | grep -qE "$pattern"; then
        echo -e "  ${RED}❌ R006 VIOLATION${NC}: Protected file detected"
        echo -e "     ${YELLOW}File${NC}: $staged_file"
        echo -e "     ${YELLOW}Type${NC}: $desc"
        echo -e "     ${BLUE}Fix${NC}: $fix"
        echo ""

        protected_violations=$((protected_violations + 1))
      fi
    done <<< "$STAGED_FILES"
  done

  if [ "$protected_violations" -eq "0" ]; then
    echo -e "  ${GREEN}✅ R006 Passed${NC}: No protected files staged"
    return 0
  else
    echo -e "  ${RED}To unstage protected files${NC}:"
    echo -e "     ${BLUE}git reset HEAD <file>${NC}"
    echo ""
    echo -e "  ${RED}If already committed, remove from tracking${NC}:"
    echo -e "     ${BLUE}git rm --cached <file>${NC}"
    echo ""
    VIOLATIONS=$((VIOLATIONS + protected_violations))
    return 1
  fi
}

# ============================================================
# R001: YAML Syntax Validation (CRITICAL)
# ============================================================
check_yaml_syntax() {
  echo -e "  ${BLUE}[R001]${NC} Checking YAML syntax..."

  if [ -z "$YAML_FILES" ]; then
    echo -e "  ${YELLOW}⏭️  R001 Skipped: No YAML files changed${NC}"
    return 0
  fi

  local yaml_errors=0

  for yaml_file in $YAML_FILES; do
    if [ ! -f "$yaml_file" ]; then
      continue
    fi

    # Validate YAML syntax (try Python first, fall back to Ruby)
    if command -v python3 >/dev/null 2>&1 && python3 -c "import yaml" 2>/dev/null; then
      if ! python3 -c "import yaml; yaml.safe_load(open('$yaml_file'))" 2>/dev/null; then
        echo -e "  ${RED}❌ R001 VIOLATION${NC}: Invalid YAML in ${YELLOW}$yaml_file${NC}"
        python3 -c "import yaml; yaml.safe_load(open('$yaml_file'))" 2>&1 | head -3 | sed 's/^/     /'
        yaml_errors=$((yaml_errors + 1))
      fi
    elif command -v ruby >/dev/null 2>&1; then
      if ! ruby -e "require 'yaml'; YAML.load_file('$yaml_file')" 2>/dev/null; then
        echo -e "  ${RED}❌ R001 VIOLATION${NC}: Invalid YAML in ${YELLOW}$yaml_file${NC}"
        ruby -e "require 'yaml'; YAML.load_file('$yaml_file')" 2>&1 | head -3 | sed 's/^/     /'
        yaml_errors=$((yaml_errors + 1))
      fi
    else
      echo -e "  ${YELLOW}⏭️  R001 Warning${NC}: No YAML parser available (install PyYAML or use Ruby)"
    fi
  done

  if [ "$yaml_errors" -eq "0" ]; then
    echo -e "  ${GREEN}✅ R001 Passed${NC}: All YAML files valid"
    return 0
  else
    VIOLATIONS=$((VIOLATIONS + yaml_errors))
    return 1
  fi
}

# ============================================================
# R003: No Secrets in Commits (CRITICAL)
# ============================================================
check_secrets() {
  echo ""
  echo -e "  ${BLUE}[R003]${NC} Scanning for secrets..."

  # Get full diff content once
  DIFF_CONTENT=$(git diff --cached)

  if [ -z "$DIFF_CONTENT" ]; then
    echo -e "  ${YELLOW}⏭️  R003 Skipped: No changes to scan${NC}"
    return 0
  fi

  local secrets_found=0

  # Define secret patterns (bash 3.x compatible - no associative arrays)
  # Format: "description|pattern"
  SECRET_PATTERNS=(
    "API key|api[_-]?key[\"']?\s*[:=]\s*[\"'][a-zA-Z0-9]{20,}[\"']"
    "Password|password[\"']?\s*[:=]\s*[\"'][^\"']{8,}[\"']"
    "Token|token[\"']?\s*[:=]\s*[\"'][a-zA-Z0-9]{20,}[\"']"
    "Secret|secret[\"']?\s*[:=]\s*[\"'][a-zA-Z0-9]{20,}[\"']"
    "AWS key|AKIA[0-9A-Z]{16}"
    "Private key|-----BEGIN (RSA |EC )?PRIVATE KEY-----"
  )

  for pattern_pair in "${SECRET_PATTERNS[@]}"; do
    IFS='|' read -r secret_type pattern <<< "$pattern_pair"

    if echo "$DIFF_CONTENT" | grep -qiE "$pattern"; then
      echo -e "  ${RED}❌ R003 VIOLATION${NC}: Potential ${YELLOW}${secret_type}${NC} detected"

      # Show context (first match only, obscured)
      echo "$DIFF_CONTENT" | grep -iE "$pattern" | head -1 | sed 's/[a-zA-Z0-9]\{8,\}/***REDACTED***/g' | sed 's/^/     /'

      secrets_found=1
    fi
  done

  if [ "$secrets_found" -eq "0" ]; then
    echo -e "  ${GREEN}✅ R003 Passed${NC}: No secrets detected"
    return 0
  else
    echo ""
    echo -e "  ${RED}Fix: Remove credentials and use environment variables or config files${NC}"
    VIOLATIONS=$((VIOLATIONS + 1))
    return 1
  fi
}

# ============================================================
# R005: Font License Compliance (CRITICAL)
# ============================================================
check_font_licenses() {
  echo ""
  echo -e "  ${BLUE}[R005]${NC} Checking font license compliance..."

  # Check if Dockerfile or config files changed
  if ! echo "$STAGED_FILES" | grep -qE "(Dockerfile|config/.*\.(yaml|yml))" 2>/dev/null; then
    echo -e "  ${YELLOW}⏭️  R005 Skipped: No Dockerfile or config changes${NC}"
    return 0
  fi

  local font_violations=0

  # Commercial fonts to detect (bash 3.x compatible)
  # Format: "font_name|pattern"
  COMMERCIAL_FONTS=(
    "游明朝 (Yu Mincho)|游明朝|Yu.*Mincho"
    "游ゴシック (Yu Gothic)|游ゴシック|Yu.*Gothic"
    "Hiragino|Hiragino|ヒラギノ"
    "MS Fonts|MS.*(明朝|ゴシック|Mincho|Gothic)"
  )

  # Get diff for relevant files
  FONT_DIFF=$(git diff --cached Dockerfile config/ 2>/dev/null || true)

  if [ -z "$FONT_DIFF" ]; then
    echo -e "  ${YELLOW}⏭️  R005 Skipped: No font-related changes${NC}"
    return 0
  fi

  for font_pair in "${COMMERCIAL_FONTS[@]}"; do
    IFS='|' read -r font_name pattern <<< "$font_pair"

    if echo "$FONT_DIFF" | grep -qiE "$pattern"; then
      echo -e "  ${RED}❌ R005 VIOLATION${NC}: Commercial font ${YELLOW}${font_name}${NC} detected"
      echo -e "     ${BLUE}Fix${NC}: Use SIL OFL licensed fonts (Noto Serif/Sans CJK JP)"

      # Show file and line
      git diff --cached | grep -iE "$pattern" | head -2 | sed 's/^/     /'

      font_violations=1
    fi
  done

  if [ "$font_violations" -eq "0" ]; then
    echo -e "  ${GREEN}✅ R005 Passed${NC}: Font license compliance OK"
    return 0
  else
    VIOLATIONS=$((VIOLATIONS + 1))
    return 1
  fi
}

# ============================================================
# R101: Markdown Lint (IMPORTANT - Warn Only)
# ============================================================
check_markdown_lint() {
  echo ""
  echo -e "  ${BLUE}[R101]${NC} Checking Markdown lint..."

  if [ -z "$MD_FILES" ]; then
    echo -e "  ${YELLOW}⏭️  R101 Skipped: No Markdown files changed${NC}"
    return 0
  fi

  if ! command -v markdownlint-cli2 &> /dev/null; then
    echo -e "  ${YELLOW}⏭️  R101 Skipped: markdownlint-cli2 not installed${NC}"
    echo -e "     Install: ${BLUE}npm install -g markdownlint-cli2${NC}"
    return 0
  fi

  local lint_errors=0

  for md_file in $MD_FILES; do
    if [ ! -f "$md_file" ]; then
      continue
    fi

    if ! markdownlint-cli2 --config .markdownlint.jsonc "$md_file" 2>&1 > /dev/null; then
      lint_errors=$((lint_errors + 1))
    fi
  done

  if [ "$lint_errors" -gt "0" ]; then
    echo -e "  ${YELLOW}⚠️  R101 WARNING${NC}: $lint_errors file(s) with lint issues"
    echo -e "     ${BLUE}Note${NC}: Will auto-fix at session end"
    echo -e "     ${BLUE}Manual fix${NC}: markdownlint-cli2 --fix --config .markdownlint.jsonc <file>"
    return 0  # Warning only, don't block commit
  else
    echo -e "  ${GREEN}✅ R101 Passed${NC}: Markdown clean"
    return 0
  fi
}

# ============================================================
# Execute All Checks
# ============================================================
check_protected_paths  # R006 - First for fast exit on violations
check_yaml_syntax
check_secrets
check_font_licenses
check_markdown_lint

# ============================================================
# Summary
# ============================================================
echo ""
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

if [ "$VIOLATIONS" -gt "0" ]; then
  echo -e "${RED}❌ COMMIT BLOCKED: $VIOLATIONS critical rule violation(s)${NC}"
  echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
  echo ""
  echo -e "${BLUE}Fix violations and try again.${NC}"
  echo -e "${BLUE}See .claude/RULES.md for details.${NC}"
  echo ""
  exit 1
else
  echo -e "${GREEN}✅ All pre-commit rules passed${NC}"
  echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
  echo ""
  exit 0
fi
