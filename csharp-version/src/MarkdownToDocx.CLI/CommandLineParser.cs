namespace MarkdownToDocx.CLI;

public static class CommandLineParser
{
    public static CommandLineOptions? Parse(string[] args)
    {
        if (args.Length == 0 || args.Contains("-h") || args.Contains("--help"))
        {
            return new CommandLineOptions
            {
                InputPath = string.Empty,
                OutputPath = string.Empty,
                ShowHelp = true
            };
        }

        string? inputPath = null;
        string? outputPath = null;
        string? configPath = null;
        string presetName = "minimal";
        string presetDirectory = "config/presets";
        bool verbose = false;

        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];

            if (arg == "-o" || arg == "--output")
            {
                if (i + 1 < args.Length) outputPath = args[++i];
            }
            else if (arg == "-c" || arg == "--config")
            {
                if (i + 1 < args.Length) configPath = args[++i];
            }
            else if (arg == "-p" || arg == "--preset")
            {
                if (i + 1 < args.Length) presetName = args[++i];
            }
            else if (arg == "--preset-dir")
            {
                if (i + 1 < args.Length) presetDirectory = args[++i];
            }
            else if (arg == "-v" || arg == "--verbose")
            {
                verbose = true;
            }
            else if (!arg.StartsWith('-'))
            {
                inputPath ??= arg;
            }
        }

        if (inputPath == null)
        {
            Console.Error.WriteLine("Error: Input file is required");
            return null;
        }

        outputPath ??= Path.ChangeExtension(inputPath, ".docx");

        return new CommandLineOptions
        {
            InputPath = inputPath,
            OutputPath = outputPath,
            ConfigPath = configPath,
            PresetName = presetName,
            PresetDirectory = presetDirectory,
            Verbose = verbose,
            ShowHelp = false
        };
    }

    public static void ShowHelp()
    {
        Console.WriteLine("md2docx - Markdown to Word Converter");
        Console.WriteLine();
        Console.WriteLine("USAGE: md2docx <input.md> [OPTIONS]");
        Console.WriteLine();
        Console.WriteLine("OPTIONS:");
        Console.WriteLine("  -o, --output <file>    Output file (default: input.docx)");
        Console.WriteLine("  -p, --preset <name>    Preset name (default: minimal)");
        Console.WriteLine("  -c, --config <file>    Custom config file");
        Console.WriteLine("  --preset-dir <dir>     Preset directory");
        Console.WriteLine("  -v, --verbose          Verbose output");
        Console.WriteLine("  -h, --help             Show help");
    }
}
