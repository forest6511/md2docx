---
layout: default
title: md2docx ドキュメント
lang: ja
---

# md2docx ドキュメント

**md2docx**の公式ドキュメントへようこそ。柔軟なMarkdownからWord（DOCX）への変換ツールです。

## ナビゲーション

- [はじめに](./getting-started) - インストールと初回変換
- [設定ガイド](./configuration) - YAML設定ガイド
- [プリセット](./presets) - ビルトインとカスタムプリセット
- [APIリファレンス](./api-reference) - C# APIドキュメント

## md2docxとは？

md2docxは、MarkdownドキュメントをプロフェッショナルにフォーマットされたWord（DOCX）ファイルに変換するコマンドラインツールです。他のコンバーターとは異なり、md2docxはシンプルなYAML設定ファイルを通じてスタイリングを完全にコントロールできます。

## 主な機能

### 🎨 YAMLベースのスタイリング
シンプルなYAMLファイルでドキュメントスタイルを定義：

```yaml
Styles:
  H1:
    Size: 26
    Bold: true
    Color: "2c3e50"
    ShowBorder: true

  Paragraph:
    Size: 11
    LineSpacing: "360"
```

### 📝 豊富なMarkdownサポート
- 見出し（H1-H6）
- インラインフォーマット付き段落
- 順序付き・順序なしリスト
- フェンスコードブロック
- 引用ブロック
- 水平線

### 🌐 クロスプラットフォーム
すべてのプラットフォームで一貫した結果を提供するため、フォントを埋め込んだDockerイメージとして配布。

### 🎌 日本語サポート
小説や伝統的な文書のための日本語縦書き（縦書き）を完全サポート。

## クイック例

```bash
# Dockerを使用
docker run --rm -v $(pwd):/workspace forest6511/md2docx:latest \
  input.md -o output.docx -p default --preset-dir /app/config/presets

# .NET CLIを使用
dotnet run --project src/MarkdownToDocx.CLI -- input.md -o output.docx
```

## 使用例

- **技術ドキュメント** - APIドキュメント、ソフトウェアマニュアル、READMEファイル
- **ビジネス文書** - 提案書、報告書、議事録
- **出版** - カスタムフォーマットの書籍原稿
- **日本語縦書き** - 小説や伝統的な文書

## サポート

- [GitHub Issues](https://github.com/forest6511/md2docx/issues) - バグ報告
- [GitHub Discussions](https://github.com/forest6511/md2docx/discussions) - 質問
- [コントリビューションガイド](https://github.com/forest6511/md2docx/blob/main/CONTRIBUTING.md) - プロジェクトへの貢献

## ライセンス

md2docxは[MITライセンス](https://opensource.org/licenses/MIT)の下でライセンスされたオープンソースソフトウェアです。
