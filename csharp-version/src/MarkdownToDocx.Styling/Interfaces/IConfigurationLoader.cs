using MarkdownToDocx.Styling.Models;

namespace MarkdownToDocx.Styling.Interfaces;

/// <summary>
/// Interface for loading conversion configuration from various sources
/// </summary>
public interface IConfigurationLoader
{
    /// <summary>
    /// Load configuration from a file path
    /// </summary>
    /// <param name="configPath">Path to configuration file</param>
    /// <returns>Conversion configuration</returns>
    /// <exception cref="ArgumentNullException">Thrown when configPath is null</exception>
    /// <exception cref="FileNotFoundException">Thrown when file does not exist</exception>
    /// <exception cref="InvalidDataException">Thrown when configuration format is invalid</exception>
    ConversionConfiguration Load(string configPath);

    /// <summary>
    /// Load configuration from a preset name
    /// </summary>
    /// <param name="presetName">Name of the preset (without extension)</param>
    /// <returns>Conversion configuration</returns>
    /// <exception cref="ArgumentNullException">Thrown when presetName is null</exception>
    /// <exception cref="FileNotFoundException">Thrown when preset does not exist</exception>
    ConversionConfiguration LoadPreset(string presetName);
}
