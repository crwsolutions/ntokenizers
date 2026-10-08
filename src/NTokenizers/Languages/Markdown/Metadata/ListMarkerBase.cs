using NTokenizers.Core;

namespace NTokenizers.Markdown.Metadata;

/// <summary>
/// Base metadata for list item tokens. Carries the marker the item was started with.
/// </summary>
public abstract class ListMarkerBase : InlineMetadata<MarkdownToken>
{
    /// <summary>
    /// Gets the marker of the list: <c>+</c>, <c>-</c> or <c>*</c> for unordered lists;
    /// <c>.</c> or <c>)</c> for ordered lists.
    /// </summary>
    public abstract char Marker { get; }
}
