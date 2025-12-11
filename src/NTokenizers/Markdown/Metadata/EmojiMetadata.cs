namespace NTokenizers.Markdown.Metadata;

/// <summary>
/// Metadata for emoji tokens, containing the emoji name.
/// </summary>
/// <param name="Name">The emoji name (e.g., "wink").</param>
public sealed class EmojiMetadata(string Name) : MarkdownMetadata
{
    /// <summary>
    /// Gets the emoji name.
    /// </summary>
    public string Name { get; } = Name;
}
