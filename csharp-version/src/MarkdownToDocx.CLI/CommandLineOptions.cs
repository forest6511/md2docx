namespace MarkdownToDocx.CLI;

public sealed class CommandLineOptions
{
    public required string InputPath { get; init; }
    public required string OutputPath { get; init; }
    public string PresetName { get; init; } = "minimal";
    public string? ConfigPath { get; init; }
    public string PresetDirectory { get; init; } = "config/presets";
    public string? CoverImagePath { get; init; }
    public bool Verbose { get; init; }
    public bool ShowHelp { get; init; }
}
