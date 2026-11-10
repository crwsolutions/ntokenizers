using System.Text;
using NTokenizers.Core;

namespace NTokenizers.Markdown.Metadata;

/// <summary>
/// Base metadata for list item tokens. Carries the marker the item was started with and
/// the content tokenizer factory for the item's sub-document.
/// </summary>
/// <param name="createContentTokenizer">Creates the tokenizer that parses the item's content
/// (a full markdown sub-document at the top level, or an inline-only tokenizer when nested
/// in a blockquote).</param>
/// <param name="reader">The section reader the sub-tokenizer reads from.</param>
/// <param name="capture">The capture builder that records every character of the stream.</param>
/// <param name="lookahead">The lookahead queue the sub-tokenizer reads from.</param>
public abstract class ListMarkerBase(Func<BaseTokenizer<MarkdownToken>> createContentTokenizer, TextReader reader, StringBuilder capture, Queue<char> lookahead)
    : InlineMetadata<MarkdownToken>(reader, capture, lookahead)
{
    private readonly Func<BaseTokenizer<MarkdownToken>> _createContentTokenizer = createContentTokenizer;

    /// <summary>
    /// Gets the marker of the list: <c>+</c>, <c>-</c> or <c>*</c> for unordered lists;
    /// <c>.</c> or <c>)</c> for ordered lists.
    /// </summary>
    public abstract char Marker { get; }

    internal override Task RunInlinesAsync(Func<MarkdownToken, Task> onToken, CancellationToken ct) =>
        _createContentTokenizer().ParseAsync(Reader, Capture, Lookahead, ct, onToken);
}
