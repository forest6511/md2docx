using Markdig;
using Markdig.Syntax;
using MarkdownToDocx.Core.Interfaces;

namespace MarkdownToDocx.Core.Markdown;

/// <summary>
/// Markdown parser implementation using Markdig library
/// </summary>
/// <param name="pipeline">Markdig pipeline configuration for parsing</param>
public sealed class MarkdigParser(MarkdownPipeline? pipeline = null) : IMarkdownParser
{
    private readonly MarkdownPipeline _pipeline = pipeline ?? new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .Build();

    /// <inheritdoc/>
    public MarkdownDocument Parse(string markdown)
    {
        ArgumentNullException.ThrowIfNull(markdown);
        return Markdig.Markdown.Parse(markdown, _pipeline);
    }
}
