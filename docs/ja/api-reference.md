---
layout: default
title: APIリファレンス
lang: ja
---

# APIリファレンス

.NETアプリケーションでmd2docxをライブラリとして使用します。

## インストール

プロジェクトにmd2docxを追加：

```bash
# パッケージ参照を追加（NuGetに公開後）
dotnet add package MarkdownToDocx

# またはプロジェクトを直接参照
dotnet add reference path/to/MarkdownToDocx.Core.csproj
```

## クイックスタート

### 基本的な変換

```csharp
using MarkdownToDocx.Core;
using MarkdownToDocx.Styling;

// YAML設定を読み込む
var config = YamlConfigLoader.LoadFromFile("config/presets/default.yaml");

// MarkdownをDOCXに変換
var converter = new MarkdownConverter(config);
converter.ConvertFile("input.md", "output.docx");
```

### プリセットを使用

```csharp
using MarkdownToDocx.Styling;

// ビルトインプリセットを読み込む
var config = YamlConfigLoader.LoadPreset("default", "/app/config/presets");

// プリセットで変換
var converter = new MarkdownConverter(config);
converter.ConvertFile("input.md", "output.docx");
```

## コアコンポーネント

### MarkdownConverter

変換プロセスを統括するメインコンバータークラス。

**名前空間**：`MarkdownToDocx.Core`

#### コンストラクタ

```csharp
public MarkdownConverter(StyleConfig config)
```

**パラメータ**：
- `config` - YAMLから読み込まれたスタイル設定

**例**：

```csharp
var config = YamlConfigLoader.LoadFromFile("config.yaml");
var converter = new MarkdownConverter(config);
```

#### メソッド

##### ConvertFile

```csharp
public void ConvertFile(string inputPath, string outputPath)
```

MarkdownファイルをDOCXに変換します。

**パラメータ**：
- `inputPath` - 入力Markdownファイルへのパス
- `outputPath` - 出力DOCXファイルへのパス

**例外**：
- `FileNotFoundException` - 入力ファイルが見つからない
- `InvalidMarkdownException` - 無効なMarkdown構文
- `ConversionException` - 変換エラー

**例**：

```csharp
try
{
    converter.ConvertFile("document.md", "document.docx");
    Console.WriteLine("変換が成功しました！");
}
catch (FileNotFoundException ex)
{
    Console.Error.WriteLine($"ファイルが見つかりません: {ex.Message}");
}
catch (ConversionException ex)
{
    Console.Error.WriteLine($"変換に失敗しました: {ex.Message}");
}
```

##### ConvertString

```csharp
public void ConvertString(string markdown, string outputPath)
```

Markdown文字列をDOCXに変換します。

**パラメータ**：
- `markdown` - 文字列としてのMarkdownコンテンツ
- `outputPath` - 出力DOCXファイルへのパス

**例**：

```csharp
var markdown = @"
# 私のドキュメント

これは**太字**テキストを含む段落です。
";

converter.ConvertString(markdown, "output.docx");
```

##### ConvertToStream

```csharp
public Stream ConvertToStream(string markdown)
```

MarkdownをDOCXストリームに変換します（Webアプリケーション用）。

**パラメータ**：
- `markdown` - 文字列としてのMarkdownコンテンツ

**戻り値**：`Stream` - メモリストリームとしてのDOCXコンテンツ

**例**：

```csharp
var markdown = File.ReadAllText("input.md");
var stream = converter.ConvertToStream(markdown);

// HTTPレスポンスとして返す（ASP.NET Core）
return File(stream, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "document.docx");
```

---

### YamlConfigLoader

YAML設定ファイルを読み込んで解析します。

**名前空間**：`MarkdownToDocx.Styling`

#### メソッド

##### LoadFromFile

```csharp
public static StyleConfig LoadFromFile(string yamlPath)
```

YAMLファイルから設定を読み込みます。

**パラメータ**：
- `yamlPath` - YAML設定ファイルへのパス

**戻り値**：`StyleConfig` - 解析された設定

**例外**：
- `FileNotFoundException` - YAMLファイルが見つからない
- `YamlException` - 無効なYAML構文
- `ConfigValidationException` - 無効な設定スキーマ

**例**：

```csharp
var config = YamlConfigLoader.LoadFromFile("config/custom/my-style.yaml");
```

##### LoadPreset

```csharp
public static StyleConfig LoadPreset(string presetName, string presetDir)
```

名前でビルトインプリセットを読み込みます。

**パラメータ**：
- `presetName` - プリセット名（`.yaml`拡張子なし）
- `presetDir` - プリセットを含むディレクトリ

**戻り値**：`StyleConfig` - プリセット設定

**例**：

```csharp
var config = YamlConfigLoader.LoadPreset("default", "/app/config/presets");
```

##### Validate

```csharp
public static bool Validate(StyleConfig config, out List<string> errors)
```

設定オブジェクトを検証します。

**パラメータ**：
- `config` - 検証する設定
- `errors` - 検証エラーの出力リスト

**戻り値**：`bool` - 有効な場合は`true`、そうでない場合は`false`

**例**：

```csharp
var config = YamlConfigLoader.LoadFromFile("config.yaml");
if (!YamlConfigLoader.Validate(config, out var errors))
{
    foreach (var error in errors)
    {
        Console.Error.WriteLine($"検証エラー: {error}");
    }
}
```

---

### StyleConfig

YAML構造を表す設定モデル。

**名前空間**：`MarkdownToDocx.Styling.Models`

#### プロパティ

```csharp
public class StyleConfig
{
    public string SchemaVersion { get; set; }
    public Metadata Metadata { get; set; }
    public string TextDirection { get; set; }
    public PageLayout PageLayout { get; set; }
    public Fonts Fonts { get; set; }
    public Styles Styles { get; set; }
}
```

#### ネストされたクラス

##### Metadata

```csharp
public class Metadata
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Author { get; set; }
    public string Version { get; set; }
}
```

##### PageLayout

```csharp
public class PageLayout
{
    public double Width { get; set; }
    public double Height { get; set; }
    public double MarginTop { get; set; }
    public double MarginBottom { get; set; }
    public double MarginLeft { get; set; }
    public double MarginRight { get; set; }
    public double MarginHeader { get; set; }
    public double MarginFooter { get; set; }
    public double MarginGutter { get; set; }
}
```

##### Fonts

```csharp
public class Fonts
{
    public string Ascii { get; set; }
    public string EastAsia { get; set; }
    public int DefaultSize { get; set; }
}
```

##### Styles

```csharp
public class Styles
{
    public HeadingStyle H1 { get; set; }
    public HeadingStyle H2 { get; set; }
    public HeadingStyle H3 { get; set; }
    public HeadingStyle H4 { get; set; }
    public HeadingStyle H5 { get; set; }
    public HeadingStyle H6 { get; set; }
    public ParagraphStyle Paragraph { get; set; }
    public ListStyle List { get; set; }
    public CodeBlockStyle CodeBlock { get; set; }
    public QuoteStyle Quote { get; set; }
}
```

---

## 高度な使用法

### コードでカスタム設定

YAMLなしでプログラム的に設定を作成：

```csharp
var config = new StyleConfig
{
    SchemaVersion = "2.0",
    Metadata = new Metadata
    {
        Name = "カスタムスタイル",
        Description = "プログラム的に作成されたスタイル",
        Author = "あなたのアプリ",
        Version = "1.0.0"
    },
    TextDirection = "Horizontal",
    PageLayout = new PageLayout
    {
        Width = 21.0,
        Height = 29.7,
        MarginTop = 2.54,
        MarginBottom = 2.54,
        MarginLeft = 3.17,
        MarginRight = 3.17,
        MarginHeader = 1.27,
        MarginFooter = 1.27,
        MarginGutter = 0
    },
    Fonts = new Fonts
    {
        Ascii = "Noto Serif",
        EastAsia = "Noto Serif CJK JP",
        DefaultSize = 11
    },
    Styles = new Styles
    {
        H1 = new HeadingStyle
        {
            Size = 26,
            Bold = true,
            Color = "2c3e50",
            ShowBorder = true,
            BorderColor = "3498db",
            BorderSize = 6,
            BorderPosition = "bottom",
            SpaceBefore = "600",
            SpaceAfter = "300"
        },
        Paragraph = new ParagraphStyle
        {
            Size = 11,
            Color = "2c3e50",
            LineSpacing = "360",
            FirstLineIndent = "0",
            LeftIndent = "0",
            SpaceBefore = "0",
            SpaceAfter = "180"
        }
        // ... その他のスタイル
    }
};

var converter = new MarkdownConverter(config);
converter.ConvertFile("input.md", "output.docx");
```

### バッチ変換

同じ設定で複数のファイルを変換：

```csharp
var config = YamlConfigLoader.LoadPreset("default", "/app/config/presets");
var converter = new MarkdownConverter(config);

var files = Directory.GetFiles("input", "*.md");
foreach (var file in files)
{
    var outputFile = Path.Combine("output",
        Path.GetFileNameWithoutExtension(file) + ".docx");

    try
    {
        converter.ConvertFile(file, outputFile);
        Console.WriteLine($"変換完了: {file} → {outputFile}");
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"{file}の変換に失敗しました: {ex.Message}");
    }
}
```

### ASP.NET Core統合

Web APIでmd2docxを使用：

```csharp
using Microsoft.AspNetCore.Mvc;
using MarkdownToDocx.Core;
using MarkdownToDocx.Styling;

[ApiController]
[Route("api/[controller]")]
public class ConvertController : ControllerBase
{
    private readonly ILogger<ConvertController> _logger;
    private readonly StyleConfig _defaultConfig;

    public ConvertController(ILogger<ConvertController> logger)
    {
        _logger = logger;
        _defaultConfig = YamlConfigLoader.LoadPreset("default", "/app/config/presets");
    }

    [HttpPost]
    public IActionResult ConvertMarkdown([FromBody] ConvertRequest request)
    {
        try
        {
            var converter = new MarkdownConverter(_defaultConfig);
            var stream = converter.ConvertToStream(request.Markdown);

            return File(stream,
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "document.docx");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "変換に失敗しました");
            return BadRequest(new { error = ex.Message });
        }
    }
}

public class ConvertRequest
{
    public string Markdown { get; set; }
    public string Preset { get; set; }
}
```

### バックグラウンド処理

バックグラウンドジョブシステムで使用（Hangfire、Quartz.NET）：

```csharp
using Hangfire;

public class DocumentService
{
    [AutomaticRetry(Attempts = 3)]
    public void ConvertDocument(int documentId)
    {
        var document = _repository.GetDocument(documentId);

        var config = YamlConfigLoader.LoadPreset(
            document.Preset ?? "default",
            "/app/config/presets"
        );

        var converter = new MarkdownConverter(config);
        var outputPath = $"output/{documentId}.docx";

        converter.ConvertString(document.MarkdownContent, outputPath);

        document.DocxPath = outputPath;
        document.Status = "完了";
        _repository.Update(document);
    }
}

// 使用法
BackgroundJob.Enqueue<DocumentService>(x => x.ConvertDocument(123));
```

## エラー処理

### 例外タイプ

#### ConversionException

変換が失敗したときにスローされます。

```csharp
try
{
    converter.ConvertFile("input.md", "output.docx");
}
catch (ConversionException ex)
{
    Console.Error.WriteLine($"変換エラー: {ex.Message}");
    Console.Error.WriteLine($"行: {ex.LineNumber}");
}
```

#### ConfigValidationException

YAML設定が無効なときにスローされます。

```csharp
try
{
    var config = YamlConfigLoader.LoadFromFile("config.yaml");
}
catch (ConfigValidationException ex)
{
    Console.Error.WriteLine("無効な設定:");
    foreach (var error in ex.ValidationErrors)
    {
        Console.Error.WriteLine($"  - {error}");
    }
}
```

#### InvalidMarkdownException

Markdown構文が無効なときにスローされます。

```csharp
try
{
    converter.ConvertFile("input.md", "output.docx");
}
catch (InvalidMarkdownException ex)
{
    Console.Error.WriteLine($"無効なMarkdown: {ex.Message}");
    Console.Error.WriteLine($"行 {ex.LineNumber}: {ex.InvalidContent}");
}
```

### ベストプラクティス

1. **使用前に常に設定を検証**：

```csharp
var config = YamlConfigLoader.LoadFromFile("config.yaml");
if (!YamlConfigLoader.Validate(config, out var errors))
{
    throw new InvalidOperationException($"無効な設定: {string.Join(", ", errors)}");
}
```

2. **ファイル操作にはtry-catchを使用**：

```csharp
try
{
    converter.ConvertFile(inputPath, outputPath);
}
catch (FileNotFoundException)
{
    // ファイルが見つからない場合の処理
}
catch (UnauthorizedAccessException)
{
    // パーミッションエラーの処理
}
catch (ConversionException ex)
{
    // 変換エラーの処理
}
```

3. **ストリームを適切に破棄**：

```csharp
using var stream = converter.ConvertToStream(markdown);
// ストリームは自動的に破棄されます
```

4. **繰り返し使用のために設定をキャッシュ**：

```csharp
// 一度読み込む
private static readonly StyleConfig _defaultConfig =
    YamlConfigLoader.LoadPreset("default", "/app/config/presets");

// 再利用
public void Convert(string input, string output)
{
    var converter = new MarkdownConverter(_defaultConfig);
    converter.ConvertFile(input, output);
}
```

## パフォーマンスの考慮事項

### メモリ使用量

- 各`MarkdownConverter`インスタンスは軽量
- DOCX生成は大きなドキュメントにストリーミングを使用
- 設定オブジェクトは安全に再利用可能

### 最適化のヒント

1. **設定オブジェクトを再利用**：

```csharp
// 良い例
var config = YamlConfigLoader.LoadPreset("default", "/app/config/presets");
for (int i = 0; i < 100; i++)
{
    var converter = new MarkdownConverter(config);
    // ... 変換
}

// 避けるべき例
for (int i = 0; i < 100; i++)
{
    var config = YamlConfigLoader.LoadPreset("default", "/app/config/presets"); // 無駄
    // ...
}
```

2. **Webアプリケーションにはストリームを使用**：

```csharp
// 良い例 - レスポンスに直接ストリーミング
var stream = converter.ConvertToStream(markdown);
return File(stream, "application/...", "doc.docx");

// 避けるべき例 - 不要なファイルI/O
converter.ConvertString(markdown, "temp.docx");
var bytes = File.ReadAllBytes("temp.docx");
return File(bytes, "application/...", "doc.docx");
```

3. **バッチ処理**：

```csharp
// 複数ファイルを並列処理
Parallel.ForEach(files, file =>
{
    var converter = new MarkdownConverter(config);
    converter.ConvertFile(file, OutputPath(file));
});
```

## テスト

### ユニットテストの例

```csharp
using Xunit;
using MarkdownToDocx.Core;
using MarkdownToDocx.Styling;

public class ConverterTests
{
    [Fact]
    public void ConvertString_ValidMarkdown_CreatesDocx()
    {
        // Arrange
        var config = YamlConfigLoader.LoadPreset("minimal", "config/presets");
        var converter = new MarkdownConverter(config);
        var markdown = "# テスト\n\n段落。";
        var output = "test_output.docx";

        // Act
        converter.ConvertString(markdown, output);

        // Assert
        Assert.True(File.Exists(output));
        Assert.True(new FileInfo(output).Length > 0);

        // クリーンアップ
        File.Delete(output);
    }

    [Fact]
    public void LoadPreset_InvalidName_ThrowsException()
    {
        // Arrange & Act & Assert
        Assert.Throws<FileNotFoundException>(() =>
            YamlConfigLoader.LoadPreset("存在しない", "config/presets")
        );
    }
}
```

## 次のステップ

- [設定ガイド](./configuration) - YAML設定を学ぶ
- [プリセットリファレンス](./presets) - ビルトインプリセットを探索
- [GitHubリポジトリ](https://github.com/forest6511/md2docx) - ソースコードと例

## サポート

- [GitHub Issues](https://github.com/forest6511/md2docx/issues) - バグを報告
- [GitHub Discussions](https://github.com/forest6511/md2docx/discussions) - 質問する
- [コントリビューションガイド](https://github.com/forest6511/md2docx/blob/main/CONTRIBUTING.md) - プロジェクトに貢献

---

**最終更新**: 2026-02-17
