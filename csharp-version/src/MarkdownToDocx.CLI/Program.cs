using MarkdownToDocx.CLI;
using MarkdownToDocx.Core.Interfaces;
using MarkdownToDocx.Core.Markdown;
using MarkdownToDocx.Core.Models;
using MarkdownToDocx.Core.OpenXml;
using MarkdownToDocx.Core.TextDirection;
using MarkdownToDocx.Styling.Configuration;
using MarkdownToDocx.Styling.Styling;

// Parse command line arguments
var options = CommandLineParser.Parse(args);

if (options == null || options.ShowHelp)
{
    CommandLineParser.ShowHelp();
    return options?.ShowHelp == true ? 0 : 1;
}

try
{
    // Validate input file
    if (!File.Exists(options.InputPath))
    {
        Console.Error.WriteLine($"Error: Input file not found: {options.InputPath}");
        return 1;
    }

    // Load configuration
    var configLoader = new YamlConfigurationLoader(options.PresetDirectory);
    var config = options.ConfigPath != null
        ? configLoader.Load(options.ConfigPath)
        : configLoader.LoadPreset(options.PresetName);

    Console.WriteLine($"Loaded configuration: {config.Metadata.Name}");

    // Read markdown file
    var markdown = await File.ReadAllTextAsync(options.InputPath);
    Console.WriteLine($"Read markdown file: {options.InputPath}");

    // Parse markdown
    var parser = new MarkdigParser();
    var document = parser.Parse(markdown);
    Console.WriteLine("Parsed markdown document");

    // Create text direction provider
    ITextDirectionProvider textDirection = config.TextDirection == TextDirectionMode.Vertical
        ? new VerticalTextProvider()
        : new HorizontalTextProvider();

    // Create style applicator
    var styleApplicator = new StyleApplicator();

    // Convert to DOCX
    using (var stream = File.Create(options.OutputPath))
    using (var builder = new OpenXmlDocumentBuilder(stream, textDirection))
    {
        Console.WriteLine("Converting to DOCX...");

        foreach (var block in document)
        {
            var blockType = block.GetType().Name;

            if (blockType.Contains("Heading"))
            {
                var level = Helpers.GetHeadingLevel(block);
                var text = Helpers.GetBlockText(block);
                var style = styleApplicator.ApplyHeadingStyle(level, config.Styles);
                builder.AddHeading(level, text, style);
            }
            else if (blockType.Contains("Paragraph") && !blockType.Contains("Fenced"))
            {
                var text = Helpers.GetBlockText(block);
                if (!string.IsNullOrWhiteSpace(text))
                {
                    var style = styleApplicator.ApplyParagraphStyle(config.Styles);
                    builder.AddParagraph(text, style);
                }
            }
            else if (blockType.Contains("List"))
            {
                var items = Helpers.GetListItems(block);
                var isOrdered = blockType.Contains("Ordered");
                var style = styleApplicator.ApplyListStyle(config.Styles);
                builder.AddList(items, isOrdered, style);
            }
            else if (blockType.Contains("FencedCode"))
            {
                var code = Helpers.GetCodeBlockText(block);
                var language = Helpers.GetCodeBlockLanguage(block);
                var style = styleApplicator.ApplyCodeBlockStyle(config.Styles);
                builder.AddCodeBlock(code, language, style);
            }
            else if (blockType.Contains("Quote"))
            {
                var text = Helpers.GetQuoteText(block);
                var style = styleApplicator.ApplyQuoteStyle(config.Styles);
                builder.AddQuote(text, style);
            }
            else if (blockType.Contains("ThematicBreak"))
            {
                builder.AddThematicBreak();
            }
        }

        builder.Save();
    }

    Console.WriteLine($"Successfully created: {options.OutputPath}");
    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error: {ex.Message}");
    if (options.Verbose)
    {
        Console.Error.WriteLine(ex.StackTrace);
    }
    return 1;
}
