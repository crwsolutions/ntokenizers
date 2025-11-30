namespace NTokenizers.Markup.Metadata;

/// <summary>
/// Metadata for table tokens, containing column information.
/// </summary>
public sealed class TableMetadata : InlineMarkupMetadata<MarkupToken>
{
    /// <summary>
    /// List of text alignments for each column in the table.
    /// </summary>
    public List<Justify>? Alignments { get; internal set; }
}
