using System.Text;
using NTokenizers.Core;

namespace NTokenizers.Markdown.Metadata;

/// <summary>
/// Metadata for list start and list end tokens. These tokens never carry a section: the
/// list items are streamed by the tokenizer itself, so <see cref="ParseInlinesAsync"/> is
/// not supported and throws.
/// </summary>
/// <param name="IsOrdered">True when the list is ordered; false for an unordered list.</param>
/// <param name="reader">The section reader (unused: list items are not sub-parsed).</param>
/// <param name="capture">The capture builder (unused: list items are not sub-parsed).</param>
/// <param name="lookahead">The lookahead queue (unused: list items are not sub-parsed).</param>
public sealed class ListMetadata(bool IsOrdered, TextReader reader, StringBuilder capture, Queue<char> lookahead)
    : InlineMetadata<MarkdownToken>(reader, capture, lookahead)
{
    /// <summary>
    /// Gets a value indicating whether the list is ordered.
    /// </summary>
    public bool IsOrdered { get; } = IsOrdered;

    internal override Task RunInlinesAsync(Func<MarkdownToken, Task> onToken, CancellationToken ct) =>
        throw new NotSupportedException("List tokens do not carry a sub-parsed section.");
}
