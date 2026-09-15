namespace NTokenizers.Markdown.Metadata;

/// <summary>
/// Metadata for link and image tokens, containing URL, optional text, and optional title.
/// </summary>
/// <param name="Url">The URL.</param>
/// <param name="Text">Optional link text or alt text.</param>
/// <param name="Title">Optional title.</param>
public sealed class LinkMetadata(string Url, string? Text = null, string? Title = null) : Core.Metadata
{
    /// <summary>
    /// Gets the URL associated with the link or image.
    /// </summary>
    public string Url { get; } = Url;

    /// <summary>
    /// Gets the optional link text or alt text.
    /// </summary>
    public string? Text { get; } = Text;

    /// <summary>
    /// Gets the optional title associated with the link or image.
    /// </summary>
    public string? Title { get; } = Title;
}