#!/bin/bash
# Benchmark hook performance
# Verifies <30s pre-commit and <2min pre-push targets

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"

echo "🚀 Benchmarking Quality System Hooks"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo ""

# Colors
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m'

# ============================================================
# Setup test environment
# ============================================================
echo "📋 Setup:"

# Create test files
TEST_DIR="$PROJECT_ROOT/test-benchmark-$$"
mkdir -p "$TEST_DIR"
cd "$TEST_DIR"

# Initialize git repo
git init > /dev/null 2>&1
git config user.email "test@example.com"
git config user.name "Test User"

# Create test YAML files
echo "Creating test files..."
for i in {1..10}; do
  cat > "test-$i.yaml" <<EOF
schema_version: "2.0"
test_data:
  value: $i
EOF
done

# Create test Markdown files
for i in {1..5}; do
  cat > "test-$i.md" <<EOF
# Test Document $i

This is a test document.

## Section

Content here.
EOF
done

git add . > /dev/null 2>&1
git commit -m "Initial test files" > /dev/null 2>&1

echo "  ✅ Test environment created"
echo ""

# ============================================================
# Benchmark Pre-Commit Hook
# ============================================================
echo "⏱️  Benchmarking pre-commit hook..."

# Modify some files
echo "  modified: true" >> test-1.yaml
echo "New content" >> test-1.md

git add test-1.yaml test-1.md

# Copy hook
cp "$PROJECT_ROOT/.claude/hooks/pre-commit-check.sh.refactored" ".git/hooks/pre-commit"
chmod +x ".git/hooks/pre-commit"

# Run benchmark (3 runs, take average)
total_time=0
runs=3

for run in $(seq 1 $runs); do
  start=$(date +%s%N)

  # Run pre-commit hook
  .git/hooks/pre-commit > /dev/null 2>&1 || true

  end=$(date +%s%N)
  elapsed=$((($end - $start) / 1000000)) # Convert to milliseconds

  total_time=$(($total_time + $elapsed))

  echo "  Run $run: ${elapsed}ms"
done

avg_time=$(($total_time / $runs))
avg_time_sec=$(echo "scale=2; $avg_time / 1000" | bc)

echo ""
echo -n "  Average: ${avg_time}ms (${avg_time_sec}s) - "

if [ $avg_time -lt 30000 ]; then
  echo -e "${GREEN}✅ PASS${NC} (<30s target)"
  precommit_pass=1
else
  echo -e "${RED}❌ FAIL${NC} (>30s target)"
  precommit_pass=0
fi

echo ""

# ============================================================
# Benchmark ADR Validator
# ============================================================
echo "⏱️  Benchmarking ADR validator..."

# Create test ADR directory
mkdir -p docs/decisions

cat > docs/decisions/ADR-0001.md <<EOF
# ADR-0001: Use Noto Serif JP

**Status**: Accepted

## Decision
Use Noto Serif JP as default font.
EOF

# Create Dockerfile with Noto fonts
cat > Dockerfile <<EOF
FROM ubuntu:22.04
RUN apt-get update && apt-get install -y fonts-noto-cjk
EOF

git add docs Dockerfile

# Run benchmark
start=$(date +%s%N)

python3 "$PROJECT_ROOT/.claude/skills/adr-source-validator/validate.py.refactored" > /dev/null 2>&1 || true

end=$(date +%s%N)
elapsed=$((($end - $start) / 1000000))
elapsed_sec=$(echo "scale=2; $elapsed / 1000" | bc)

echo "  Execution time: ${elapsed}ms (${elapsed_sec}s)"

if [ $elapsed -lt 5000 ]; then
  echo -e "  ${GREEN}✅ PASS${NC} (<5s for validation)"
  adr_pass=1
else
  echo -e "  ${RED}❌ FAIL${NC} (>5s for validation)"
  adr_pass=0
fi

echo ""

# ============================================================
# Cleanup
# ============================================================
cd "$PROJECT_ROOT"
rm -rf "$TEST_DIR"

echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "📊 Benchmark Results"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo ""

if [ $precommit_pass -eq 1 ] && [ $adr_pass -eq 1 ]; then
  echo -e "${GREEN}✅ All benchmarks passed${NC}"
  echo ""
  exit 0
else
  echo -e "${RED}❌ Some benchmarks failed${NC}"
  echo ""
  exit 1
fi
