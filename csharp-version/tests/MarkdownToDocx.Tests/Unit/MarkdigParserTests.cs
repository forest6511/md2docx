using FluentAssertions;
using Markdig.Syntax;
using MarkdownToDocx.Core.Markdown;
using Xunit;

namespace MarkdownToDocx.Tests.Unit;

/// <summary>
/// Unit tests for MarkdigParser
/// </summary>
public class MarkdigParserTests
{
    private readonly MarkdigParser _parser = new();

    [Fact]
    public void Parse_WithNullInput_ShouldThrowArgumentNullException()
    {
        // Arrange
        string? markdown = null;

        // Act
        Action act = () => _parser.Parse(markdown!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("markdown");
    }

    [Fact]
    public void Parse_WithEmptyString_ShouldReturnEmptyDocument()
    {
        // Arrange
        var markdown = string.Empty;

        // Act
        var result = _parser.Parse(markdown);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<MarkdownDocument>();
        result.Count.Should().Be(0);
    }

    [Fact]
    public void Parse_WithHeading_ShouldParseCorrectly()
    {
        // Arrange
        var markdown = "# Heading 1\n\n## Heading 2";

        // Act
        var result = _parser.Parse(markdown);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCountGreaterOrEqualTo(2);

        // Filter only heading blocks
        var headings = result.OfType<HeadingBlock>().ToList();
        headings.Should().HaveCount(2);

        headings[0].Level.Should().Be(1);
        headings[1].Level.Should().Be(2);
    }

    [Fact]
    public void Parse_WithParagraph_ShouldParseCorrectly()
    {
        // Arrange
        var markdown = "This is a paragraph.";

        // Act
        var result = _parser.Parse(markdown);

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(1);
        result[0].Should().BeOfType<ParagraphBlock>();
    }

    [Fact]
    public void Parse_WithList_ShouldParseCorrectly()
    {
        // Arrange
        var markdown = "- Item 1\n- Item 2\n- Item 3";

        // Act
        var result = _parser.Parse(markdown);

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(1);
        result[0].Should().BeOfType<ListBlock>();

        var list = result[0] as ListBlock;
        list!.Count.Should().Be(3);
        list.IsOrdered.Should().BeFalse();
    }

    [Fact]
    public void Parse_WithOrderedList_ShouldParseCorrectly()
    {
        // Arrange
        var markdown = "1. First\n2. Second\n3. Third";

        // Act
        var result = _parser.Parse(markdown);

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(1);
        result[0].Should().BeOfType<ListBlock>();

        var list = result[0] as ListBlock;
        list!.Count.Should().Be(3);
        list.IsOrdered.Should().BeTrue();
    }

    [Fact]
    public void Parse_WithCodeBlock_ShouldParseCorrectly()
    {
        // Arrange
        var markdown = "```csharp\nvar x = 10;\n```";

        // Act
        var result = _parser.Parse(markdown);

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(1);
        result[0].Should().BeOfType<FencedCodeBlock>();

        var codeBlock = result[0] as FencedCodeBlock;
        codeBlock!.Info.Should().Be("csharp");
    }

    [Fact]
    public void Parse_WithQuote_ShouldParseCorrectly()
    {
        // Arrange
        var markdown = "> This is a quote";

        // Act
        var result = _parser.Parse(markdown);

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(1);
        result[0].Should().BeOfType<QuoteBlock>();
    }

    [Fact]
    public void Parse_WithComplexMarkdown_ShouldParseCorrectly()
    {
        // Arrange
        var markdown = @"# Title

This is a paragraph.

## Section

- Item 1
- Item 2

```python
print('Hello')
```

> A quote";

        // Act
        var result = _parser.Parse(markdown);

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().BeGreaterThan(0);

        // Verify different block types exist
        result.Should().Contain(b => b is HeadingBlock);
        result.Should().Contain(b => b is ParagraphBlock);
        result.Should().Contain(b => b is ListBlock);
        result.Should().Contain(b => b is FencedCodeBlock);
        result.Should().Contain(b => b is QuoteBlock);
    }
}
