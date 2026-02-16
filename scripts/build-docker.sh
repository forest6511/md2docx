#!/bin/bash
# Docker image build script

set -e

VERSION="1.0.0"
REGISTRY="md2word"  # or ghcr.io/your-org etc.

echo "🐳 Building Docker images..."

# Standard version
echo "📦 Building standard image..."
docker build -t ${REGISTRY}/converter:${VERSION} \
             -t ${REGISTRY}/converter:latest \
             -f Dockerfile .

# Slim version
echo "📦 Building slim image..."
docker build -t ${REGISTRY}/converter:${VERSION}-slim \
             -t ${REGISTRY}/converter:slim \
             -f Dockerfile.slim .

# Full version
echo "📦 Building full image..."
docker build -t ${REGISTRY}/converter:${VERSION}-full \
             -t ${REGISTRY}/converter:full \
             -f Dockerfile.full .

echo "✅ Build complete!"
echo ""
echo "📊 Image sizes:"
docker images ${REGISTRY}/converter --format "table {{.Tag}}\t{{.Size}}"

echo ""
echo "🚀 To run:"
echo "  docker run --rm -v \$(pwd):/workspace ${REGISTRY}/converter:latest input.md -o output.docx"

echo ""
echo "📤 To push to registry:"
echo "  docker push ${REGISTRY}/converter:${VERSION}"
echo "  docker push ${REGISTRY}/converter:latest"
