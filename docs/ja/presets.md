---
layout: default
title: プリセットリファレンス
lang: ja
---

# プリセットリファレンス

md2docxには、さまざまなユースケースに最適化されたいくつかのビルトインプリセットが含まれています。このガイドでは、各プリセットに関する詳細情報を提供します。

## 概要

| プリセット | ディレクトリ | 最適な用途 | テキスト方向 |
|-----------|-------------|-----------|-------------|
| [minimal](#minimal) | `config/presets/` | シンプルなドキュメント、クイック変換 | 横書き |
| [default](#default) | `config/presets/` | 汎用ドキュメント | 横書き |
| [technical](#technical) | `config/presets/` | 技術ドキュメント、APIドキュメント | 横書き |
| [vertical-novel](#vertical-novel) | `config/vertical/` | 日本語小説、伝統的な文書 | 縦書き |

## 標準プリセット

### minimal

**場所**：`config/presets/minimal.yaml`

**説明**：白黒配色の最小限のスタイリング。視覚的な装飾のない直接的なMarkdownからWordへの変換に最適。

**特徴**：
- **ページサイズ**：A4（21.0 × 29.7 cm）
- **フォント**：Noto Serif / Noto Serif CJK JP
- **基本フォントサイズ**：11pt
- **配色**：白黒のみ
- **枠線**：なし
- **背景**：なし

**スタイル詳細**：

```yaml
H1: 24pt、太字、黒
H2: 20pt、太字、黒
H3: 16pt、太字、黒
段落: 11pt、1.5行間
コードブロック: 10pt、ライトグレー背景（#f5f5f5）
引用: 11pt、斜体、左枠線（#999999）
```

**使用例**：
- クイック文書変換
- 印刷に適した出力
- 最小ファイルサイズ
- 学術論文（APA/MLAスタイル）

**使用例**：

```bash
docker run --rm -v $(pwd):/workspace forest6511/md2docx:latest \
  input.md -o output.docx -p minimal --preset-dir /app/config/presets
```

---

### default

**場所**：`config/presets/default.yaml`

**説明**：ほとんどのドキュメントに適したバランスの取れたスタイリング。控えめな色と枠線を使用したプロフェッショナルな外観。

**特徴**：
- **ページサイズ**：A4（21.0 × 29.7 cm）
- **フォント**：Noto Serif / Noto Serif CJK JP
- **基本フォントサイズ**：11pt
- **配色**：ネイビーブルー（#2c3e50）とグレートーン
- **枠線**：H1/H2の下枠線、引用の左枠線
- **背景**：コードブロックはライトグレー、引用はライトブルー

**スタイル詳細**：

```yaml
H1: 26pt、太字、ネイビー（#2c3e50）、ブルー下枠線（#3498db）
H2: 22pt、太字、ダークグレー（#34495e）、グレー下枠線（#95a5a6）
H3: 18pt、太字、ダークグレー（#34495e）
段落: 11pt、1.5行間、ネイビー
コードブロック: 10pt、等幅、ライトグレー背景（#ecf0f1）
引用: 11pt、斜体、グレー（#7f8c8d）、ブルー左枠線（#3498db）
```

**使用例**：
- ビジネス文書
- 提案書と報告書
- 議事録
- 一般的なドキュメント

**使用例**：

```bash
docker run --rm -v $(pwd):/workspace forest6511/md2docx:latest \
  input.md -o output.docx -p default --preset-dir /app/config/presets
```

---

### technical

**場所**：`config/presets/technical.yaml`

**説明**：コンパクトなレイアウトとコードに適したスタイリングで技術ドキュメント用に最適化。

**特徴**：
- **ページサイズ**：A4（21.0 × 29.7 cm）
- **フォント**：Noto Sans / Noto Sans CJK JP（読みやすさ向上のためサンセリフ）
- **基本フォントサイズ**：10pt（コンパクト）
- **配色**：ダークグレー（#1a1a1a）とブルーアクセント
- **枠線**：H1/H2の下枠線、コードブロックの枠線
- **背景**：コードブロックと引用にライトグレー

**スタイル詳細**：

```yaml
H1: 24pt、太字、ダーク（#1a1a1a）、ブルー下枠線（#0066cc）
H2: 20pt、太字、ダークグレー（#333333）、グレー下枠線（#4d4d4d）
H3: 16pt、太字、ダークグレー（#333333）
段落: 10pt、1.4行間、ダーク（#1a1a1a）
コードブロック: 9pt、等幅、ライトグレー背景（#f5f5f5）、枠線
引用: 10pt、グレー（#4d4d4d）、ブルー左枠線（#0066cc）
```

**使用例**：
- APIドキュメント
- ソフトウェアマニュアル
- 技術仕様書
- コード重視のドキュメント
- READMEファイル

**使用例**：

```bash
docker run --rm -v $(pwd):/workspace forest6511/md2docx:latest \
  README.md -o README.docx -p technical --preset-dir /app/config/presets
```

---

## 縦書きテキストプリセット

### vertical-novel

**場所**：`config/vertical/vertical-novel.yaml`

**説明**：小説フォーマット用に最適化された日本語縦書き（縦書き）。伝統的な右から左、上から下のテキストフロー。

**特徴**：
- **ページサイズ**：A5（14.8 × 21.0 cm） - 日本語小説で一般的
- **テキスト方向**：縦書き（縦書き）
- **フォント**：Noto Serif / Noto Serif CJK JP
- **基本フォントサイズ**：11pt
- **配色**：黒のみ
- **枠線**：なし（クリーンで最小限のデザイン）
- **行間**：広い（440 twips）読みやすさのため

**スタイル詳細**：

```yaml
H1: 18pt、太字、黒
H2: 16pt、太字、黒
H3: 14pt、太字、黒
段落: 11pt、広い行間（440 twips）、黒
コードブロック: 10pt、等幅、ライトグレー背景
引用: 11pt、ダークグレー（#333333）
```

**使用例**：
- 日本語小説
- 伝統的な日本語文書
- 縦書き出版物
- 日本語の文学作品

**重要な注意事項**：
- 日本語テキスト（ひらがな、カタカナ、漢字）用に最適化
- ページサイズは日本の書籍標準に従う
- 右から左への読み方向
- 長文テキスト（小説、エッセイ）に最適

**使用例**：

```bash
docker run --rm -v $(pwd):/workspace forest6511/md2docx:latest \
  novel.md -o novel.docx -p vertical-novel --preset-dir /app/config/vertical
```

---

## 比較表

### 視覚的特徴

| 機能 | minimal | default | technical | vertical-novel |
|------|---------|---------|-----------|----------------|
| **フォントファミリー** | セリフ | セリフ | サンセリフ | セリフ |
| **基本サイズ** | 11pt | 11pt | 10pt | 11pt |
| **カラー** | 白黒 | ネイビー/ブルー/グレー | ダーク/ブルー | 黒 |
| **H1枠線** | なし | 下 | 下 | なし |
| **コードブロック** | グレー背景 | グレー背景 | グレー背景+枠線 | グレー背景 |
| **引用ブロック** | 左枠線 | 左枠線+背景 | 左枠線+背景 | 枠線なし |

### レイアウト特徴

| 機能 | minimal | default | technical | vertical-novel |
|------|---------|---------|-----------|----------------|
| **ページサイズ** | A4 | A4 | A4 | A5 |
| **テキスト方向** | 横書き | 横書き | 横書き | 縦書き |
| **余白** | 標準 | 標準 | コンパクト | 標準 |
| **行間** | 1.5倍 | 1.5倍 | 1.4倍 | 広い（2.2倍） |

### ファイルサイズ（概算）

| プリセット | 一般的なファイルサイズ* |
|-----------|---------------------|
| minimal | 〜12 KB |
| default | 〜14 KB |
| technical | 〜13 KB |
| vertical-novel | 〜11 KB |

*標準的なMarkdownコンテンツを含む10ページのドキュメントの場合

## プリセットの選択

### 決定木

```
ここから開始
│
├─ 日本語縦書きが必要？
│  └─ はい → vertical-novel
│
├─ 技術ドキュメント？
│  └─ はい → technical
│
├─ プロフェッショナルなビジネス文書？
│  └─ はい → default
│
└─ シンプルで最小限のスタイリング？
   └─ はい → minimal
```

### 文書タイプ別

| 文書タイプ | 推奨プリセット |
|-----------|---------------|
| 技術ドキュメント | technical |
| APIリファレンス | technical |
| ビジネス提案書 | default |
| 議事録 | default |
| プロジェクトレポート | default |
| 学術論文 | minimal |
| クイックメモ | minimal |
| 日本語小説 | vertical-novel |
| 伝統的な日本語文書 | vertical-novel |

## プリセットのカスタマイズ

すべてのプリセットは、YAMLファイルをコピーして変更することでカスタマイズできます：

### 1. プリセットをコピー

```bash
cp config/presets/default.yaml config/custom/my-style.yaml
```

### 2. 設定を変更

色、フォント、間隔などを調整するためにYAMLファイルを編集します。詳細については、[設定ガイド](./configuration)を参照してください。

### 3. カスタムプリセットを使用

```bash
docker run --rm -v $(pwd):/workspace forest6511/md2docx:latest \
  input.md -o output.docx -c config/custom/my-style.yaml
```

## ベストプラクティス

### フォントサイズの選択

- **10pt**：コンパクト、技術文書
- **11pt**：標準的な読みやすさ
- **12pt**：読みやすさ向上、プレゼンテーション

### 行間の選択

- **1.0倍**：非常にコンパクト（推奨されません）
- **1.4倍**：コンパクトな技術文書
- **1.5倍**：標準的な読みやすさ ✓
- **2.0倍**：高い読みやすさ、アクセシビリティ

### 配色

**プロフェッショナル**：
- ネイビー/ブルー/グレー（defaultプリセット）
- 黒/ダークグレー（minimalプリセット）

**テクニカル**：
- ダークグレー/ブルーアクセント（technicalプリセット）
- 控えめなハイライトのモノクローム

**クリエイティブ**：
- カスタムカラーパレット（独自作成）

## トラブルシューティング

### プリセットが見つからない

**問題**：`Preset 'xxx' not found`

**解決方法**：
- プリセット名がファイル名（`.yaml`なし）と一致することを確認
- `--preset-dir`が正しいディレクトリを指していることを確認
- 現在のディレクトリからの絶対パスまたは相対パスを使用

**例**：

```bash
# 正しい
docker run --rm -v $(pwd):/workspace forest6511/md2docx:latest \
  input.md -o output.docx -p default --preset-dir /app/config/presets

# これも正しい
docker run --rm -v $(pwd):/workspace forest6511/md2docx:latest \
  input.md -o output.docx -c /app/config/presets/default.yaml
```

### フォントがレンダリングされない

**問題**：出力ドキュメントでフォントが異なって表示される

**解決方法**：
- Dockerイメージを使用（フォント埋め込み済み）✓
- Notoフォントをシステムにインストール（.NET CLI使用時）
- YAMLのフォント名が利用可能なフォントと一致することを確認

### 縦書きテキストの問題

**問題**：縦書きテキストが正しく表示されない

**確認事項**：
- `vertical-novel`プリセットまたはカスタム設定で`TextDirection: "Vertical"`を使用
- ドキュメントビューアが縦書きテキストをサポート（Microsoft Word、LibreOffice）
- 日本語フォントが利用可能（Noto Serif CJK JP）

## 次のステップ

- [設定ガイド](./configuration) - カスタムスタイルの作成方法を学ぶ
- [APIドキュメント](./api-reference) - md2docxをライブラリとして使用
- [GitHubリポジトリ](https://github.com/forest6511/md2docx) - ソースコードと例

## リファレンスファイル

すべてのプリセットファイルはリポジトリで利用可能です：

- `config/presets/minimal.yaml`
- `config/presets/default.yaml`
- `config/presets/technical.yaml`
- `config/vertical/vertical-novel.yaml`

詳細なスタイリング仕様については、完全なソースファイルを参照してください。
