using Markdig.Syntax;
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

        // Title/cover page
        var titlePageStyle = styleApplicator.ApplyTitlePageStyle(
            config, options.InputPath, options.CoverImagePath);
        builder.AddTitlePage(titlePageStyle);

        // Table of contents
        var tocStyle = styleApplicator.ApplyTableOfContentsStyle(config);
        builder.AddTableOfContents(tocStyle);

        foreach (var block in document)
        {
            // Use pattern matching for type-safe block processing
            // Process FencedCodeBlock before ParagraphBlock (both are LeafBlock types)
            switch (block)
            {
                case HeadingBlock heading:
                    var headingText = Helpers.GetBlockText(heading);
                    var headingStyle = styleApplicator.ApplyHeadingStyle(heading.Level, config.Styles);
                    builder.AddHeading(heading.Level, headingText, headingStyle);
                    break;

                case FencedCodeBlock code:
                    var codeText = Helpers.GetCodeBlockText(code);
                    var language = Helpers.GetCodeBlockLanguage(code);
                    var codeStyle = styleApplicator.ApplyCodeBlockStyle(config.Styles);
                    builder.AddCodeBlock(codeText, language, codeStyle);
                    break;

                case ParagraphBlock paragraph:
                    var paragraphText = Helpers.GetBlockText(paragraph);
                    if (!string.IsNullOrWhiteSpace(paragraphText))
                    {
                        var paragraphStyle = styleApplicator.ApplyParagraphStyle(config.Styles);
                        builder.AddParagraph(paragraphText, paragraphStyle);
                    }
                    break;

                case ListBlock list:
                    var items = Helpers.GetListItems(list);
                    var isOrdered = list.IsOrdered;
                    var listStyle = styleApplicator.ApplyListStyle(config.Styles);
                    builder.AddList(items, isOrdered, listStyle);
                    break;

                case QuoteBlock quote:
                    var quoteText = Helpers.GetQuoteText(quote);
                    var quoteStyle = styleApplicator.ApplyQuoteStyle(config.Styles);
                    builder.AddQuote(quoteText, quoteStyle);
                    break;

                case ThematicBreakBlock:
                    builder.AddThematicBreak();
                    break;
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
