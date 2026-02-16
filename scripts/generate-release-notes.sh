#!/usr/bin/env bash
# Release Notes Generator
# Generates release notes from git commit history

set -e

VERSION=$1

if [ -z "$VERSION" ]; then
  echo "❌ Error: Version number required"
  echo "Usage: ./scripts/generate-release-notes.sh 0.2.0"
  exit 1
fi

# Validate version format
if ! [[ $VERSION =~ ^[0-9]+\.[0-9]+\.[0-9]+$ ]]; then
  echo "❌ Error: Invalid version format. Use semantic versioning (e.g., 0.2.0)"
  exit 1
fi

echo "📝 Generating release notes for v$VERSION..."
echo ""

# Get previous version tag
PREV_VERSION=$(git describe --tags --abbrev=0 2>/dev/null || echo "")

if [ -z "$PREV_VERSION" ]; then
  echo "⚠️  Warning: No previous version tag found. Generating from all commits."
  COMMIT_RANGE="HEAD"
else
  echo "📊 Comparing $PREV_VERSION...HEAD"
  COMMIT_RANGE="$PREV_VERSION..HEAD"
fi
echo ""

# Output file
OUTPUT_FILE="docs/RELEASE_NOTES_v$VERSION.md"

# Generate release notes
cat > "$OUTPUT_FILE" <<EOF
# Release Notes v$VERSION

**Release Date**: $(date +%Y-%m-%d)
**Previous Version**: ${PREV_VERSION:-Initial Release}

---

## Summary

<!-- Add a brief summary of this release -->

---

## Changes

### ✨ Features

$(git log $COMMIT_RANGE --pretty=format:"- %s" --grep="^feat:" | sed 's/^feat: //' || echo "- No new features")

### 🐛 Bug Fixes

$(git log $COMMIT_RANGE --pretty=format:"- %s" --grep="^fix:" | sed 's/^fix: //' || echo "- No bug fixes")

### 📚 Documentation

$(git log $COMMIT_RANGE --pretty=format:"- %s" --grep="^docs:" | sed 's/^docs: //' || echo "- No documentation updates")

### 🔧 Refactoring

$(git log $COMMIT_RANGE --pretty=format:"- %s" --grep="^refactor:" | sed 's/^refactor: //' || echo "- No refactoring")

### ⚡ Performance

$(git log $COMMIT_RANGE --pretty=format:"- %s" --grep="^perf:" | sed 's/^perf: //' || echo "- No performance improvements")

### 🧪 Tests

$(git log $COMMIT_RANGE --pretty=format:"- %s" --grep="^test:" | sed 's/^test: //' || echo "- No test updates")

### 🔨 Build & CI

$(git log $COMMIT_RANGE --pretty=format:"- %s" --grep="^build:\|^ci:" | sed 's/^build: //;s/^ci: //' || echo "- No build/CI changes")

### 🎨 Style & Chore

$(git log $COMMIT_RANGE --pretty=format:"- %s" --grep="^style:\|^chore:" | sed 's/^style: //;s/^chore: //' || echo "- No style/chore updates")

---

## Breaking Changes

<!-- List any breaking changes here -->

- None

---

## Installation

### Docker (Recommended)

\`\`\`bash
docker pull forest6511/md2docx:$VERSION
\`\`\`

### .NET CLI

\`\`\`bash
dotnet tool install --global MarkdownToDocx.CLI --version $VERSION
\`\`\`

---

## Full Changelog

**All Commits**:
$(git log $COMMIT_RANGE --oneline | head -50)

$(if [ $(git log $COMMIT_RANGE --oneline | wc -l) -gt 50 ]; then echo "... and $(( $(git log $COMMIT_RANGE --oneline | wc -l) - 50 )) more commits"; fi)

---

## Contributors

$(git log $COMMIT_RANGE --pretty=format:"- %an" | sort -u)

---

**Full Diff**: https://github.com/forest6511/md2docx/compare/${PREV_VERSION}...v$VERSION

EOF

echo "✅ Release notes generated: $OUTPUT_FILE"
echo ""
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "📋 Next steps:"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "1. Review and edit: $OUTPUT_FILE"
echo "2. Add summary of the release"
echo "3. List breaking changes (if any)"
echo "4. Commit: git add $OUTPUT_FILE && git commit -m 'docs: add release notes for v$VERSION'"
echo ""

# Also generate internal release notes template
INTERNAL_FILE="internal-docs/ja/releases/RELEASE_v$VERSION.md"
mkdir -p "internal-docs/ja/releases"

cat > "$INTERNAL_FILE" <<EOF
# リリース準備: v$VERSION

**リリース予定日**: $(date +%Y-%m-%d)
**担当者**: 開発者名

---

## ✅ リリース前チェックリスト

### ドキュメント

- [ ] README.md バージョンバッジ更新
- [ ] docs/_config.yml バージョン更新
- [ ] docs/index.md バージョンバッジ更新
- [ ] CHANGELOG.md 更新
- [ ] RELEASE_NOTES_v$VERSION.md 作成・レビュー
- [ ] docs/en/ と docs/ja/ の同期確認
- [ ] API Reference 更新

### コード

- [ ] .csproj ファイルのバージョン更新
- [ ] ビルド成功確認
- [ ] テスト全パス確認
- [ ] カバレッジ ≥80% 確認

### Docker

- [ ] Dockerfile ビルド成功
- [ ] Docker イメージテスト
- [ ] Docker Hub へのpush準備

### Git

- [ ] git tag v$VERSION 作成
- [ ] git push --tags 実行
- [ ] GitHub Release 作成

---

## 📝 リリース内容サマリ

### 主な変更点

-

### 破壊的変更

- なし

### 既知の問題

- なし

---

## 🚀 リリース手順

1. \`./scripts/update-version.sh $VERSION\`
2. \`./scripts/generate-release-notes.sh $VERSION\` (既に実行済み)
3. RELEASE_NOTES_v$VERSION.md をレビュー・編集
4. CHANGELOG.md を編集
5. git commit -m "chore: prepare release v$VERSION"
6. git tag v$VERSION
7. git push && git push --tags
8. GitHub Releaseを作成
9. Docker Hubにpush

---

## 📊 テスト結果

### ビルド

\`\`\`bash
cd csharp-version
dotnet build --configuration Release
\`\`\`

結果:

### テスト

\`\`\`bash
dotnet test
\`\`\`

結果:

### Docker

\`\`\`bash
docker build -t md2docx:$VERSION .
docker run --rm md2docx:$VERSION --version
\`\`\`

結果:

---

## 📢 リリース後の作業

- [ ] Twitter/SNS でアナウンス
- [ ] GitHub Discussions に投稿
- [ ] Docker Hub のREADME更新
- [ ] NuGet.org にパッケージ公開（該当する場合）

EOF

echo "ℹ️  Internal release notes also created: $INTERNAL_FILE"
echo ""
