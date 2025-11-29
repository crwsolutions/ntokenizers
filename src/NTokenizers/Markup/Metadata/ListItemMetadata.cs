namespace NTokenizers.Markup.Metadata;

/// <summary>
/// Metadata for unordered list item tokens
/// </summary>
/// <param name="Marker">The marker of the lists.</param>
public sealed class ListItemMetadata(char Marker) : InlineMarkupMetadata<MarkupToken>
{
    /// <summary>
    /// Gets the marker of the list.
    /// </summary>
    public char Marker { get; } = Marker;
}
