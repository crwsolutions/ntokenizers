using System.Text;
using NTokenizers.Core;

namespace NTokenizers.Markdown.Metadata;

/// <summary>
/// Metadata for table tokens, containing column information.
/// </summary>
/// <param name="reader">The section reader the sub-tokenizer reads from.</param>
/// <param name="capture">The capture builder that records every character of the stream.</param>
/// <param name="lookahead">The lookahead queue the sub-tokenizer reads from.</param>
public sealed class TableMetadata(TextReader reader, StringBuilder capture, Queue<char> lookahead)
    : InlineMetadata<MarkdownToken>(reader, capture, lookahead)
{
    /// <summary>
    /// List of text alignments for each column in the table.
    /// </summary>
    public List<Justify>? Alignments { get; internal set; }

    internal override Task RunInlinesAsync(Func<MarkdownToken, Task> onToken, CancellationToken ct) =>
        new TableMarkdownTokenizer(this).ParseAsync(Reader, Capture, Lookahead, ct, onToken);
}
