using Markdig;
using Markdig.Extensions.EmphasisExtras;
using Markdig.Syntax;
using MarkdownToDocx.Core.Interfaces;

namespace MarkdownToDocx.Core.Markdown;

/// <summary>
/// Markdown parser implementation using Markdig library
/// </summary>
/// <param name="pipeline">Markdig pipeline configuration for parsing</param>
public sealed class MarkdigParser(MarkdownPipeline? pipeline = null) : IMarkdownParser
{
    private readonly MarkdownPipeline _pipeline = pipeline ?? BuildDefaultPipeline();

    /// <summary>
    /// Builds the default Markdig pipeline with selective extensions.
    /// Uses explicit extension registration instead of UseAdvancedExtensions() to avoid
    /// the subscript extension (~), which causes Markdig to misparse table cell boundaries
    /// when tilde appears inside bold markers (e.g., **~$1.50**).
    /// </summary>
    private static MarkdownPipeline BuildDefaultPipeline()
    {
        return new MarkdownPipelineBuilder()
            .UseAlertBlocks()
            .UseAbbreviations()
            .UseAutoIdentifiers()
            .UseCitations()
            .UseCustomContainers()
            .UseDefinitionLists()
            .UseEmphasisExtras(EmphasisExtraOptions.Strikethrough)
            .UseFigures()
            .UseFooters()
            .UseFootnotes()
            .UseGridTables()
            .UseMathematics()
            .UseMediaLinks()
            .UsePipeTables()
            .UseListExtras()
            .UseTaskLists()
            .UseDiagrams()
            .UseAutoLinks()
            .UseGenericAttributes()
            .Build();
    }

    /// <inheritdoc/>
    public MarkdownDocument Parse(string markdown)
    {
        ArgumentNullException.ThrowIfNull(markdown);
        return Markdig.Markdown.Parse(markdown, _pipeline);
    }
}
