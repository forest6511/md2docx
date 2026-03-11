namespace MarkdownToDocx.Core.Models;

/// <summary>
/// Represents a single cell in a table row
/// </summary>
public sealed record TableCellData
{
    /// <summary>
    /// Inline runs of text content with formatting
    /// </summary>
    public IReadOnlyList<InlineRun> Runs { get; init; } = [];

    /// <summary>
    /// Horizontal alignment: "left", "center", or "right"
    /// </summary>
    public string Alignment { get; init; } = "left";
}

/// <summary>
/// Represents a single row in a table
/// </summary>
public sealed record TableRowData
{
    /// <summary>
    /// Cells contained in this row
    /// </summary>
    public IReadOnlyList<TableCellData> Cells { get; init; } = [];

    /// <summary>
    /// Whether this row is a header row
    /// </summary>
    public bool IsHeader { get; init; } = false;
}

/// <summary>
/// Represents a complete table extracted from a Markdown document
/// </summary>
public sealed record TableData
{
    /// <summary>
    /// All rows in the table (header rows first, then body rows)
    /// </summary>
    public IReadOnlyList<TableRowData> Rows { get; init; } = [];

    /// <summary>
    /// Number of columns in the table
    /// </summary>
    public int ColumnCount { get; init; } = 0;
}
