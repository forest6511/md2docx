#!/bin/bash
# Docker image test script

set -e

IMAGE="${1:-md2word/converter:latest}"
TEST_DIR="$(pwd)/tests"
OUTPUT_DIR="$(pwd)/output/docker-tests"

echo "🧪 Testing Docker image: ${IMAGE}"

# Create output directory
mkdir -p "${OUTPUT_DIR}"

# Test 1: Help message
echo "Test 1: Help message"
docker run --rm ${IMAGE} --help

# Test 2: Version info
echo "Test 2: Version info"
docker run --rm ${IMAGE} --version

# Test 3: Basic conversion (minimal preset)
echo "Test 3: Basic conversion (minimal)"
docker run --rm \
  -v "${TEST_DIR}:/workspace/input:ro" \
  -v "${OUTPUT_DIR}:/workspace/output" \
  ${IMAGE} \
  input/simple.md -o output/simple-minimal.docx --preset minimal

# Test 4: Default preset
echo "Test 4: Default preset"
docker run --rm \
  -v "${TEST_DIR}:/workspace/input:ro" \
  -v "${OUTPUT_DIR}:/workspace/output" \
  ${IMAGE} \
  input/sample.md -o output/sample-default.docx --preset default

# Test 5: Technical documentation
echo "Test 5: Technical documentation"
docker run --rm \
  -v "${TEST_DIR}:/workspace/input:ro" \
  -v "${OUTPUT_DIR}:/workspace/output" \
  ${IMAGE} \
  input/technical.md -o output/technical.docx --preset technical

# Test 6: KDP horizontal
echo "Test 6: KDP horizontal"
docker run --rm \
  -v "${TEST_DIR}:/workspace/input:ro" \
  -v "${OUTPUT_DIR}:/workspace/output" \
  ${IMAGE} \
  input/book-chapter.md -o output/kdp-horizontal.docx --preset publishing/kdp-6x9-horizontal

# Test 7: Vertical text
echo "Test 7: Vertical text"
docker run --rm \
  -v "${TEST_DIR}:/workspace/input:ro" \
  -v "${OUTPUT_DIR}:/workspace/output" \
  ${IMAGE} \
  input/novel.md -o output/vertical-novel.docx --preset publishing/kdp-vertical-novel

# Test 8: Font embedding check
echo "Test 8: Font embedding check"
docker run --rm \
  -v "${TEST_DIR}:/workspace/input:ro" \
  -v "${OUTPUT_DIR}:/workspace/output" \
  ${IMAGE} \
  input/font-test.md -o output/font-embedded.docx --preset publishing/kdp-6x9-horizontal

# Unzip generated file and check font embedding
if command -v unzip &> /dev/null; then
  echo "Checking font embedding..."
  mkdir -p "${OUTPUT_DIR}/font-check"
  unzip -q "${OUTPUT_DIR}/font-embedded.docx" -d "${OUTPUT_DIR}/font-check"

  if grep -q "embedRegular" "${OUTPUT_DIR}/font-check/word/fontTable.xml" 2>/dev/null; then
    echo "✅ Font embedding: PASS"
  else
    echo "❌ Font embedding: FAIL"
  fi

  rm -rf "${OUTPUT_DIR}/font-check"
fi

echo ""
echo "✅ All tests complete!"
echo "📁 Output files: ${OUTPUT_DIR}"
echo ""
echo "📋 Generated files:"
ls -lh "${OUTPUT_DIR}"/*.docx
