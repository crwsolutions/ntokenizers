namespace NTokenizers.Markdown.Metadata;

/// <summary>
/// Metadata for link and image tokens, containing URL and optional title.
/// </summary>
/// <param name="Url">The URL.</param>
/// <param name="Title">Optional title.</param>
public sealed class LinkMetadata(string Url, string? Title = null) : Core.Metadata
{
    /// <summary>
    /// Gets the URL associated with the link or image.
    /// </summary>
    public string Url { get; } = Url;
    /// <summary>
    /// Gets the optional title associated with the link or image.
    /// </summary>
    public string? Title { get; } = Title;
}
