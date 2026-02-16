using Markdig;
using Markdig.Syntax;
using MarkdownToDocx.Core.Interfaces;

namespace MarkdownToDocx.Core.Markdown;

/// <summary>
/// Markdown parser implementation using Markdig library
/// </summary>
public sealed class MarkdigParser : IMarkdownParser
{
    private readonly MarkdownPipeline _pipeline;

    /// <summary>
    /// Initializes a new instance of <see cref="MarkdigParser"/>
    /// </summary>
    public MarkdigParser()
    {
        _pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .Build();
    }

    /// <summary>
    /// Initializes a new instance of <see cref="MarkdigParser"/> with custom pipeline
    /// </summary>
    /// <param name="pipeline">Custom Markdig pipeline configuration</param>
    public MarkdigParser(MarkdownPipeline pipeline)
    {
        _pipeline = pipeline ?? throw new ArgumentNullException(nameof(pipeline));
    }

    /// <inheritdoc/>
    public MarkdownDocument Parse(string markdown)
    {
        if (markdown == null)
        {
            throw new ArgumentNullException(nameof(markdown));
        }

        return Markdig.Markdown.Parse(markdown, _pipeline);
    }
}
