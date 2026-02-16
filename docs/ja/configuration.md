---
layout: default
title: 設定ガイド
lang: ja
---

# 設定ガイド

YAML設定ファイルを通じてドキュメントのスタイリングをカスタマイズする方法を学びます。

## 設定ファイルの構造

md2docxは、ドキュメントのスタイリングを定義するためにYAMLファイルを使用します。すべての設定はスキーマバージョン2.0に従います。

### 基本構造

```yaml
SchemaVersion: "2.0"

Metadata:
  Name: "カスタムスタイル"
  Description: "私のドキュメント用のカスタムスタイリング"
  Author: "あなたの名前"
  Version: "1.0.0"

TextDirection: "Horizontal"  # または縦書きの場合は "Vertical"

PageLayout:
  Width: 21.0      # A4幅（cm）
  Height: 29.7     # A4高さ（cm）
  # ... 余白設定

Fonts:
  Ascii: "Noto Serif"
  EastAsia: "Noto Serif CJK JP"
  DefaultSize: 11

Styles:
  H1:
    Size: 26
    Bold: true
    Color: "2c3e50"
    # ... 追加のプロパティ
  # ... その他の要素のスタイル
```

## 設定セクション

### メタデータ

プリセットに関する説明情報：

```yaml
Metadata:
  Name: "マイスタイル"           # 表示名
  Description: "このスタイルの簡単な説明"
  Author: "あなたの名前"         # 作成者名
  Version: "1.0.0"              # セマンティックバージョン
```

### テキスト方向

ドキュメント内のテキストの流れを制御：

```yaml
TextDirection: "Horizontal"  # 左から右、上から下
# または
TextDirection: "Vertical"    # 上から下、右から左（日本語縦書き）
```

**重要**：縦書きテキストは日本語ドキュメントと小説用に最適化されています。

### ページレイアウト

ページの寸法と余白を定義（すべての値はセンチメートル）：

```yaml
PageLayout:
  Width: 21.0              # ページ幅
  Height: 29.7             # ページ高さ
  MarginTop: 2.54          # 上余白
  MarginBottom: 2.54       # 下余白
  MarginLeft: 3.17         # 左余白
  MarginRight: 3.17        # 右余白
  MarginHeader: 1.27       # ヘッダー余白（端からの距離）
  MarginFooter: 1.27       # フッター余白（端からの距離）
  MarginGutter: 0          # のど（製本用）
```

**一般的なページサイズ**：
- **A4**: 21.0 × 29.7 cm
- **レター**: 21.59 × 27.94 cm
- **A5**: 14.8 × 21.0 cm（縦書き小説で一般的）

### フォント

フォントファミリーとデフォルトサイズを指定：

```yaml
Fonts:
  Ascii: "Noto Serif"              # ASCII文字用フォント
  EastAsia: "Noto Serif CJK JP"    # 日本語/中国語/韓国語用フォント
  DefaultSize: 11                   # デフォルトフォントサイズ（ポイント）
```

**利用可能なフォント**（Dockerイメージ内）：
- **セリフ**: Noto Serif、Noto Serif CJK JP
- **サンセリフ**: Noto Sans、Noto Sans CJK JP
- **等幅**: Noto Sans Mono、Noto Sans Mono CJK JP

**注意**：.NET CLIを使用する場合は、システムにフォントがインストールされていることを確認するか、一貫したフォント処理のためにDockerを使用してください。

### スタイル

各Markdown要素のスタイリングを定義します。

#### 見出しスタイル（H1-H6）

```yaml
Styles:
  H1:
    Size: 26                    # フォントサイズ（ポイント）
    Bold: true                  # 太字
    Italic: false               # 斜体（オプション）
    Color: "2c3e50"            # 16進数カラー（#なし）
    ShowBorder: true            # 枠線を表示
    BorderColor: "3498db"       # 枠線の色（16進数）
    BorderSize: 6               # 枠線の太さ（1/8ポイント）
    BorderPosition: "bottom"    # "top"、"bottom"、"left"、"right"
    SpaceBefore: "600"          # 前の間隔（twips、1/20ポイント）
    SpaceAfter: "300"           # 後の間隔（twips）
```

**間隔の値**（twips = 1/20ポイント）：
- `"240"` = 12pt間隔
- `"360"` = 18pt間隔
- `"480"` = 24pt間隔
- `"600"` = 30pt間隔

#### 段落スタイル

```yaml
Styles:
  Paragraph:
    Size: 11                    # フォントサイズ
    Color: "2c3e50"            # テキストカラー
    LineSpacing: "360"          # 行間（twips）
    FirstLineIndent: "0"        # 1行目のインデント（twips）
    LeftIndent: "0"             # 左インデント（twips）
    SpaceBefore: "0"            # 前の間隔
    SpaceAfter: "180"           # 後の間隔
```

**行間の例**：
- `"240"` = シングルスペース（1.0）
- `"360"` = 1.5スペース
- `"480"` = ダブルスペース（2.0）

**インデントの例**：
- `"0"` = インデントなし
- `"360"` = 0.5cmインデント
- `"720"` = 1cmインデント

#### リストスタイル

```yaml
Styles:
  List:
    Size: 11                    # フォントサイズ
    Color: "2c3e50"            # テキストカラー
    LeftIndent: "720"           # 左余白（twips）
    HangingIndent: "360"        # 箇条書き/番号のぶら下げインデント
    SpaceBefore: "60"           # 各項目の前の間隔
    SpaceAfter: "60"            # 各項目の後の間隔
```

#### コードブロックスタイル

```yaml
Styles:
  CodeBlock:
    Size: 10                              # フォントサイズ
    Color: "2c3e50"                      # テキストカラー
    BackgroundColor: "ecf0f1"            # 背景色
    BorderColor: "bdc3c7"                # 枠線の色
    MonospaceFontAscii: "Noto Sans Mono"           # 等幅フォント（ASCII）
    MonospaceFontEastAsia: "Noto Sans Mono CJK JP" # 等幅フォント（CJK）
    LineSpacing: "280"                   # 行間
    SpaceBefore: "300"                   # 前の間隔
    SpaceAfter: "300"                    # 後の間隔
    ShowBorder: true                     # 枠線を表示
    BorderSize: 4                        # 枠線の太さ
```

#### 引用ブロックスタイル

```yaml
Styles:
  Quote:
    Size: 11                    # フォントサイズ
    Color: "7f8c8d"            # テキストカラー
    Italic: true                # 斜体
    ShowBorder: true            # 左枠線を表示
    BorderColor: "3498db"       # 枠線の色
    BorderSize: 16              # 枠線の太さ
    BorderPosition: "left"      # 枠線の位置
    LeftIndent: "720"           # 左インデント
    BackgroundColor: "f8f9fa"   # 背景色
    SpaceBefore: "300"          # 前の間隔
    SpaceAfter: "300"           # 後の間隔
```

## カスタムプリセットの作成

### ステップ1：既存のプリセットをコピー

```bash
cp config/presets/default.yaml config/custom/my-style.yaml
```

### ステップ2：設定を編集

希望のスタイリングでYAMLファイルを変更：

```yaml
SchemaVersion: "2.0"

Metadata:
  Name: "企業スタイル"
  Description: "ビジネス文書用のプロフェッショナルスタイリング"
  Author: "あなたの会社"
  Version: "1.0.0"

TextDirection: "Horizontal"

PageLayout:
  Width: 21.0
  Height: 29.7
  # ... 余白をカスタマイズ

Fonts:
  Ascii: "Noto Sans"
  EastAsia: "Noto Sans CJK JP"
  DefaultSize: 10

Styles:
  H1:
    Size: 22
    Bold: true
    Color: "003366"  # コーポレートブルー
    ShowBorder: true
    BorderColor: "003366"
    # ... 追加のプロパティ
```

### ステップ3：カスタムプリセットを使用

**カスタムYAMLファイルを使用：**

```bash
docker run --rm -v $(pwd):/workspace forest6511/md2docx:latest \
  input.md -o output.docx -c config/custom/my-style.yaml
```

**プリセット名を使用**（プリセットディレクトリに配置した場合）：

```bash
docker run --rm -v $(pwd):/workspace forest6511/md2docx:latest \
  input.md -o output.docx -p my-style --preset-dir /workspace/config/custom
```

## カラーリファレンス

カラーは`#`プレフィックスなしの16進数値で指定します。

### 一般的なカラー

| カラー名 | 16進数値 | 例 |
|----------|---------|-----|
| 黒 | `000000` | <span style="color: #000000">■</span> |
| ダークグレー | `333333` | <span style="color: #333333">■</span> |
| ミディアムグレー | `7f8c8d` | <span style="color: #7f8c8d">■</span> |
| ライトグレー | `95a5a6` | <span style="color: #95a5a6">■</span> |
| ネイビーブルー | `2c3e50` | <span style="color: #2c3e50">■</span> |
| ダークブルー | `34495e` | <span style="color: #34495e">■</span> |
| ブルー | `3498db` | <span style="color: #3498db">■</span> |
| ライトブルー | `0066cc` | <span style="color: #0066cc">■</span> |

## ベストプラクティス

### 1. 一貫性を維持

- ドキュメント全体で同じフォントファミリーを使用
- カラーパレットを限定（2-3色のメインカラー）
- 一貫した間隔パターン

### 2. 読みやすさ優先

- **行間**：段落には少なくとも1.5倍の間隔（`"360"`）を使用
- **フォントサイズ**：本文テキストは10-12pt、見出しはより大きく
- **コントラスト**：テキストと背景の間に十分なコントラストを確保

### 3. プロフェッショナルな外観

- 過度な枠線や色を避ける
- 適切な余白を使用（最小2-3cm）
- 一貫した見出し階層

### 4. ターゲットプラットフォームでテスト

- サンプルドキュメントを生成してWord/LibreOfficeでテスト
- フォントが正しくレンダリングされることを確認
- 必要に応じて印刷出力でページレイアウトを確認

## 検証

使用前にYAML設定を検証：

```bash
# Pythonを使用
python3 -c "import yaml; yaml.safe_load(open('config/custom/my-style.yaml'))"

# Rubyを使用
ruby -e "require 'yaml'; YAML.load_file('config/custom/my-style.yaml')"
```

エラーが表示されなければ、YAMLは有効です。

## トラブルシューティング

### フォントが見つからない

**問題**：生成されたドキュメントが指定と異なるフォントを使用している

**解決方法**：
1. Dockerイメージを使用（フォント埋め込み済み）
2. システムにNotoフォントをインストール
3. フォント名が正確に一致することを確認（大文字小文字を区別）

### 枠線が表示されない

**問題**：枠線設定が出力に表示されない

**確認事項**：
1. `ShowBorder: true`が設定されている
2. `BorderSize`が十分である（`6`以上を試す）
3. `BorderColor`に有効な16進数値がある

### 間隔の問題

**問題**：間隔が正しく表示されない

**確認事項**：
1. 値がtwips（1/20ポイント）である
2. フォントサイズに対して行間が十分である
3. 負の値を使用していない

### YAML構文エラー

**問題**：設定ファイルが読み込まれない

**一般的な問題**：
- カラー値の引用符が欠けている
- 不正なインデント（2スペースを使用）
- キーの後のコロンが欠けている
- 無効なYAML構造

**検証**：YAML検証コマンドを実行（上記参照）

## 次のステップ

- [プリセットリファレンス](./presets) - ビルトインプリセットを探索
- [APIドキュメント](./api-reference) - md2docxをライブラリとして使用
- [GitHub Issues](https://github.com/forest6511/md2docx/issues) - 問題を報告

## リファレンス

### 完全な例

完全で詳細にコメントされた設定例については、`config/presets/default.yaml`を参照してください。

### スキーマバージョン履歴

- **2.0**（現在）：C#マッピング用のPascalCaseキー、改善された構造
- **1.0**：初期スキーマ（非推奨）
