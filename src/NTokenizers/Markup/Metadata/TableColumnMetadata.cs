namespace NTokenizers.Markup.Metadata;

/// <summary>
/// Metadata for individual table columns.
/// </summary>
/// <param name="Index">The column index.</param>
/// <param name="Alignment">The column text alignment.</param>
/// <param name="IsHeader">Whether this column is in the header row.</param>
public sealed class TableColumnMetadata(
    int Index,
    Justify Alignment,
    bool IsHeader
)
{
    /// <summary>
    /// Gets the column index (0-based).
    /// </summary>
    public int Index { get; } = Index;
    /// <summary>
    /// Gets the text alignment for the column.
    /// </summary>
    public Justify Alignment { get; } = Alignment;
    /// <summary>
    /// Gets a value indicating whether this column is part of the header row.
    /// </summary>
    public bool IsHeader { get; } = IsHeader;
}
