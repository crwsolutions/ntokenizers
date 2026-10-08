using NTokenizers.Core;

namespace NTokenizers.Markdown.Metadata;

/// <summary>
/// Metadata for list start and list end tokens.
/// </summary>
/// <param name="IsOrdered">True when the list is ordered; false for an unordered list.</param>
public sealed class ListMetadata(bool IsOrdered) : InlineMetadata<MarkdownToken>
{
    /// <summary>
    /// Gets a value indicating whether the list is ordered.
    /// </summary>
    public bool IsOrdered { get; } = IsOrdered;
}
