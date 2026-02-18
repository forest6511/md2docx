---
layout: default
title: はじめに
lang: ja
---

# はじめに

このガイドでは、md2docxのインストールと初めての使用方法を説明します。

## インストール

### Docker（推奨）

Dockerは、すべてのプラットフォームで最も簡単で一貫した体験を提供します。

```bash
# 最新のイメージをプル
docker pull forest6511/md2docx:latest

# インストールの確認
docker run --rm forest6511/md2docx:latest --help
```

**Dockerの利点：**
- .NETのインストール不要
- フォント埋め込み（Noto CJK）で一貫した出力
- Windows、Mac、Linuxで同じように動作
- 依存関係の競合なし

### .NET CLI

開発用、またはDockerを使用したくない場合：

**要件：**
- .NET 8.0 SDK以降

```bash
# リポジトリをクローン
git clone https://github.com/forest6511/md2docx.git
cd md2docx

# プロジェクトをビルド
cd csharp-version
dotnet build src/MarkdownToDocx.sln

# 実行
dotnet run --project src/MarkdownToDocx.CLI -- --help
```

## 初めての変換

### ステップ1：Markdownファイルを作成

`example.md`という名前のファイルを作成：

```markdown
# 初めてのドキュメント

これは**太字**と*斜体*のテキストを含む段落です。

## セクション1

- 項目1
- 項目2
- 項目3

## コード例

\```python
def hello_world():
    print("Hello, World!")
\```

> これは引用ブロックです。
```

### ステップ2：DOCXに変換

**Dockerを使用：**

```bash
docker run --rm -v $(pwd):/workspace forest6511/md2docx:latest \
  example.md -o example.docx -p default --preset-dir /app/config/presets
```

**.NET CLIを使用：**

```bash
dotnet run --project src/MarkdownToDocx.CLI -- \
  example.md -o example.docx -p default --preset-dir ../config/presets
```

### ステップ3：結果を開く

Microsoft Word、LibreOffice、またはその他の互換性のあるワードプロセッサで`example.docx`を開きます。

## 利用可能なプリセット

md2docxには、いくつかのビルトインプリセットが付属しています：

- **minimal** - 最小限のスタイリング、白黒
- **default** - バランスの取れた汎用フォーマット
- **technical** - 技術ドキュメント用に最適化
- **vertical-novel** - 小説用の日本語縦書き

さまざまなプリセットを試す：

```bash
# ミニマルスタイル
docker run --rm -v $(pwd):/workspace forest6511/md2docx:latest \
  example.md -o minimal.docx -p minimal --preset-dir /app/config/presets

# テクニカルスタイル
docker run --rm -v $(pwd):/workspace forest6511/md2docx:latest \
  example.md -o technical.docx -p technical --preset-dir /app/config/presets
```

## コマンドラインオプション

```
使用法: md2docx [オプション] <入力ファイル>

引数:
  <入力ファイル>    入力Markdownファイル

オプション:
  -o, --output <ファイル>         出力DOCXファイル (デフォルト: input.docx)
  -p, --preset <名前>             プリセット名 (minimal, default, technical)
  -c, --config <ファイル>         カスタムYAML設定ファイル
  --cover-image <ファイル>        タイトルページ用カバー画像
  --preset-dir <ディレクトリ>     プリセットディレクトリのパス
  -v, --verbose                   詳細な出力
  -h, --help                      ヘルプ情報を表示
```

## シェルエイリアス（オプション）

便利なように、シェルエイリアスを作成：

**Bash/Zsh (~/.bashrc または ~/.zshrc):**

```bash
alias md2docx='docker run --rm -v $(pwd):/workspace forest6511/md2docx:latest'
```

**PowerShell:**

```powershell
function md2docx { docker run --rm -v ${PWD}:/workspace forest6511/md2docx:latest @args }
```

エイリアスを追加した後、次のように使用できます：

```bash
md2docx example.md -o output.docx -p default --preset-dir /app/config/presets
```

## 次のステップ

- [設定ガイド](./configuration) - スタイリングのカスタマイズ方法を学ぶ
- [プリセットリファレンス](./presets) - すべての利用可能なプリセットを探索
- [APIドキュメント](./api-reference) - md2docxをライブラリとして使用

## トラブルシューティング

### Dockerパーミッションエラー

出力ファイルの書き込み時にパーミッションエラーが発生した場合：

```bash
# ユーザーパーミッションで実行
docker run --user $(id -u):$(id -g) \
  --rm -v $(pwd):/workspace forest6511/md2docx:latest \
  input.md -o output.docx
```

### フォントが見つからない

Dockerイメージには Noto フォントが含まれています。.NET CLIを使用していて異なるフォントが表示される場合：

1. 一貫したフォント処理のためにDockerを使用
2. またはシステムに Noto フォントをインストール
3. またはYAML設定でシステムフォントを指定

### 変換が失敗する

確認事項：
1. 入力ファイルが存在し、有効なMarkdownである
2. 出力ディレクトリが書き込み可能である
3. （カスタム設定を使用している場合）YAML設定が有効である

詳細なヘルプ：
- [GitHub Issues](https://github.com/forest6511/md2docx/issues)
- [GitHub Discussions](https://github.com/forest6511/md2docx/discussions)

---

**最終更新**: 2026-02-17
