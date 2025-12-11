using NTokenizers.Markdown;

namespace NTokenizers.Markdown.Metadata;

/// <summary>
/// Metadata for list item tokens, containing the item number for ordered lists.
/// </summary>
/// <param name="Number">The item number for ordered lists.</param>
public sealed class OrderedListItemMetadata(int Number) : InlineMarkdownMetadata<MarkdownToken>
{
    /// <summary>
    /// Gets the current number value within the list.
    /// </summary>
    public int Number { get; } = Number;
}
