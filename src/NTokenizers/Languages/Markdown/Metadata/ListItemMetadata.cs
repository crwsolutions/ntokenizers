using NTokenizers.Core;

namespace NTokenizers.Markdown.Metadata;

/// <summary>
/// Metadata for unordered list item tokens
/// </summary>
/// <param name="Marker">The marker of the list: <c>+</c>, <c>-</c> or <c>*</c>.</param>
public class ListItemMetadata(char Marker) : ListMarkerBase
{
    /// <summary>
    /// Gets the marker of the list.
    /// </summary>
    public override char Marker { get; } = Marker;
}
