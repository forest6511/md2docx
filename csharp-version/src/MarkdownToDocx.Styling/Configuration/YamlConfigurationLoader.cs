using MarkdownToDocx.Styling.Interfaces;
using MarkdownToDocx.Styling.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MarkdownToDocx.Styling.Configuration;

/// <summary>
/// Loads conversion configuration from YAML files
/// Uses YamlDotNet for deserialization with PascalCase naming convention
/// </summary>
/// <param name="presetDirectory">Directory containing preset YAML files</param>
/// <exception cref="ArgumentNullException">Thrown when presetDirectory is null</exception>
public sealed class YamlConfigurationLoader(string presetDirectory) : IConfigurationLoader
{
    private readonly string _presetDirectory = presetDirectory
        ?? throw new ArgumentNullException(nameof(presetDirectory));

    private readonly IDeserializer _deserializer = new DeserializerBuilder()
        .WithNamingConvention(PascalCaseNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    /// <inheritdoc/>
    public ConversionConfiguration Load(string configPath)
    {
        ArgumentNullException.ThrowIfNull(configPath);

        if (!File.Exists(configPath))
        {
            throw new FileNotFoundException($"Configuration file not found: {configPath}", configPath);
        }

        try
        {
            var yaml = File.ReadAllText(configPath);
            var config = _deserializer.Deserialize<ConversionConfiguration>(yaml);

            if (config == null)
            {
                throw new InvalidDataException($"Failed to deserialize configuration from: {configPath}");
            }

            ValidateConfiguration(config);
            return config;
        }
        catch (YamlDotNet.Core.YamlException ex)
        {
            throw new InvalidDataException($"Invalid YAML format in {configPath}: {ex.Message}", ex);
        }
    }

    /// <inheritdoc/>
    public ConversionConfiguration LoadPreset(string presetName)
    {
        ArgumentNullException.ThrowIfNull(presetName);

        var path = Path.Combine(_presetDirectory, $"{presetName}.yaml");

        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Preset configuration not found: {presetName}", path);
        }

        return Load(path);
    }

    /// <summary>
    /// Validates loaded configuration for required fields and valid values
    /// </summary>
    private static void ValidateConfiguration(ConversionConfiguration config)
    {
        // Schema version validation
        if (string.IsNullOrWhiteSpace(config.SchemaVersion))
        {
            throw new InvalidDataException("Schema version is required");
        }

        // Metadata validation
        if (string.IsNullOrWhiteSpace(config.Metadata.Name))
        {
            throw new InvalidDataException("Metadata name is required");
        }

        if (string.IsNullOrWhiteSpace(config.Metadata.Description))
        {
            throw new InvalidDataException("Metadata description is required");
        }

        // Page layout validation
        if (config.PageLayout.Width <= 0)
        {
            throw new InvalidDataException("Page width must be greater than 0");
        }

        if (config.PageLayout.Height <= 0)
        {
            throw new InvalidDataException("Page height must be greater than 0");
        }

        // Font validation
        if (string.IsNullOrWhiteSpace(config.Fonts.Ascii))
        {
            throw new InvalidDataException("ASCII font name is required");
        }

        if (string.IsNullOrWhiteSpace(config.Fonts.EastAsia))
        {
            throw new InvalidDataException("East Asia font name is required");
        }

        if (config.Fonts.DefaultSize <= 0)
        {
            throw new InvalidDataException("Default font size must be greater than 0");
        }
    }
}
