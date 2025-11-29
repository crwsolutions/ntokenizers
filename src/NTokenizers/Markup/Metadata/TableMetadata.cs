namespace NTokenizers.Markup.Metadata;

/// <summary>
/// Metadata for table tokens, containing column information.
/// </summary>
/// <param name="Columns">List of column metadata.</param>
public sealed class TableMetadata(
    IReadOnlyList<TableColumnMetadata> Columns
) : MarkupMetadata
{
    /// <summary>
    /// Optional callback to stream inline tokens (bold, italic, etc.) within table cells.
    /// When set, the tokenizer will parse inline content and emit tokens via this callback.
    /// </summary>
    public Action<MarkupToken>? OnInlineToken { get; set; }
}
