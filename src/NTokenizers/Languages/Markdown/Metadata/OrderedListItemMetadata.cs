using NTokenizers.Core;

namespace NTokenizers.Markdown.Metadata;

/// <summary>
/// Metadata for list item tokens, containing the item number for ordered lists.
/// </summary>
/// <param name="Number">The item number for ordered lists.</param>
/// <param name="Marker">The ordered list delimiter: <c>.</c> or <c>)</c>.</param>
public class OrderedListItemMetadata(int Number, char Marker) : ListMarkerBase
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
