using NTokenizers.Core;

namespace NTokenizers.Markdown.Metadata;

/// <summary>
/// Metadata for heading tokens, containing the heading level.
/// </summary>
/// <param name="Level">The heading level (1-6).</param>
public sealed class HeadingMetadata(int Level) : InlineMetadata<MarkdownToken>
{
    /// <summary>
    /// Gets the current level of the heading (1-6).
    /// </summary>
    public int Level { get; } = Level;
}
