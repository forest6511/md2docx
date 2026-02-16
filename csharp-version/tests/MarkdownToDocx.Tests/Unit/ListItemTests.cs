using FluentAssertions;
using MarkdownToDocx.Core.Models;
using Xunit;

namespace MarkdownToDocx.Tests.Unit;

/// <summary>
/// Unit tests for ListItem model
/// </summary>
public class ListItemTests
{
    [Fact]
    public void ListItem_WithTextOnly_ShouldCreateSuccessfully()
    {
        // Arrange & Act
        var item = new ListItem { Text = "Simple item" };

        // Assert
        item.Text.Should().Be("Simple item");
        item.SubItems.Should().BeNull();
    }

    [Fact]
    public void ListItem_WithSubItems_ShouldCreateSuccessfully()
    {
        // Arrange
        var subItems = new List<ListItem>
        {
            new() { Text = "Sub item 1" },
            new() { Text = "Sub item 2" }
        };

        // Act
        var item = new ListItem
        {
            Text = "Parent item",
            SubItems = subItems
        };

        // Assert
        item.Text.Should().Be("Parent item");
        item.SubItems.Should().NotBeNull();
        item.SubItems.Should().HaveCount(2);
        item.SubItems![0].Text.Should().Be("Sub item 1");
        item.SubItems[1].Text.Should().Be("Sub item 2");
    }

    [Fact]
    public void ListItem_WithEmptySubItemsList_ShouldCreateSuccessfully()
    {
        // Arrange & Act
        var item = new ListItem
        {
            Text = "Item with empty sub-items",
            SubItems = new List<ListItem>()
        };

        // Assert
        item.Text.Should().Be("Item with empty sub-items");
        item.SubItems.Should().NotBeNull();
        item.SubItems.Should().BeEmpty();
    }

    [Fact]
    public void ListItem_WithNestedSubItems_ShouldCreateSuccessfully()
    {
        // Arrange
        var deeplyNestedItem = new ListItem
        {
            Text = "Level 3",
            SubItems = null
        };

        var nestedItem = new ListItem
        {
            Text = "Level 2",
            SubItems = new List<ListItem> { deeplyNestedItem }
        };

        // Act
        var rootItem = new ListItem
        {
            Text = "Level 1",
            SubItems = new List<ListItem> { nestedItem }
        };

        // Assert
        rootItem.Text.Should().Be("Level 1");
        rootItem.SubItems.Should().HaveCount(1);
        rootItem.SubItems![0].Text.Should().Be("Level 2");
        rootItem.SubItems[0].SubItems.Should().HaveCount(1);
        rootItem.SubItems[0].SubItems![0].Text.Should().Be("Level 3");
    }

    [Fact]
    public void ListItem_RecordEquality_ShouldWorkCorrectly()
    {
        // Arrange
        var item1 = new ListItem { Text = "Same text" };
        var item2 = new ListItem { Text = "Same text" };
        var item3 = new ListItem { Text = "Different text" };

        // Act & Assert
        item1.Should().Be(item2); // Records with same values are equal
        item1.Should().NotBe(item3);
    }

    [Fact]
    public void ListItem_WithComplexText_ShouldHandleCorrectly()
    {
        // Arrange & Act
        var item = new ListItem
        {
            Text = "Text with **bold** and *italic* and `code`"
        };

        // Assert
        item.Text.Should().Contain("**bold**");
        item.Text.Should().Contain("*italic*");
        item.Text.Should().Contain("`code`");
    }
}
