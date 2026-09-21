namespace NTokenizers.Markdown.Metadata;

/// <summary>
/// Metadata for link and image tokens, containing URL, optional text, and optional title.
/// </summary>
/// <param name="Url">The URL, without surrounding angle brackets when present.</param>
/// <param name="Text">Optional link text or alt text.</param>
/// <param name="Title">Optional title.</param>
/// <param name="IsBracketed">True when the destination was enclosed in angle brackets.</param>
public sealed class LinkMetadata(string Url, string? Text = null, string? Title = null, bool IsBracketed = false) : Core.Metadata
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

    /// <summary>
    /// Gets a value indicating whether the destination was enclosed in angle brackets.
    /// </summary>
    public bool IsBracketed { get; } = IsBracketed;
}