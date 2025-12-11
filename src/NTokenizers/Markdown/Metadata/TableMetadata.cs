using NTokenizers.Markdown;

namespace NTokenizers.Markdown.Metadata;

/// <summary>
/// Metadata for table tokens, containing column information.
/// </summary>
public sealed class TableMetadata : InlineMarkdownMetadata<MarkdownToken>
{
    /// <summary>
    /// List of text alignments for each column in the table.
    /// </summary>
    public List<Justify>? Alignments { get; internal set; }
}
