namespace NTokenizers.Markdown.Metadata;

/// <summary>
/// Metadata for footnote tokens, containing the footnote identifier.
/// </summary>
/// <param name="Id">The footnote identifier.</param>
public sealed class FootnoteMetadata(string Id) : Core.Metadata
{
    /// <summary>
    /// Gets the footnote identifier.
    /// </summary>
    public string Id { get; } = Id;
}
