using System.Text;
using NTokenizers.Core;

namespace NTokenizers.Markdown.Metadata;

/// <summary>
/// Metadata for list item tokens, containing the item number for ordered lists.
/// </summary>
/// <param name="Number">The item number for ordered lists.</param>
/// <param name="Marker">The ordered list delimiter: <c>.</c> or <c>)</c>.</param>
/// <param name="createContentTokenizer">Creates the tokenizer that parses the item's content.</param>
/// <param name="reader">The section reader the sub-tokenizer reads from.</param>
/// <param name="capture">The capture builder that records every character of the stream.</param>
/// <param name="lookahead">The lookahead queue the sub-tokenizer reads from.</param>
public class OrderedListItemMetadata(int Number, char Marker, Func<BaseTokenizer<MarkdownToken>> createContentTokenizer, TextReader reader, StringBuilder capture, Queue<char> lookahead)
    : ListMarkerBase(createContentTokenizer, reader, capture, lookahead)
{
    /// <summary>
    /// Gets the current number value within the list.
    /// </summary>
    public int Number { get; } = Number;

    /// <summary>
    /// Gets the ordered list delimiter: <c>.</c> or <c>)</c>.
    /// </summary>
    public override char Marker { get; } = Marker;
}
