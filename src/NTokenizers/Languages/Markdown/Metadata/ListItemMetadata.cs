using System.Text;
using NTokenizers.Core;

namespace NTokenizers.Markdown.Metadata;

/// <summary>
/// Metadata for unordered list item tokens
/// </summary>
/// <param name="Marker">The marker of the list: <c>+</c>, <c>-</c> or <c>*</c>.</param>
/// <param name="createContentTokenizer">Creates the tokenizer that parses the item's content.</param>
/// <param name="reader">The section reader the sub-tokenizer reads from.</param>
/// <param name="capture">The capture builder that records every character of the stream.</param>
/// <param name="lookahead">The lookahead queue the sub-tokenizer reads from.</param>
public class ListItemMetadata(char Marker, Func<BaseTokenizer<MarkdownToken>> createContentTokenizer, TextReader reader, StringBuilder capture, Queue<char> lookahead)
    : ListMarkerBase(createContentTokenizer, reader, capture, lookahead)
{
    /// <summary>
    /// Gets the marker of the list.
    /// </summary>
    public override char Marker { get; } = Marker;
}
