using NTokenizers.Core;
using System.Diagnostics;

namespace NTokenizers.Markdown;

/// <summary>
/// Represents a markdown token with its type, value, and optional metadata.
/// </summary>
/// <param name="TokenType">The type of the markdown token.</param>
/// <param name="Value">The string value of the markdown token (renderable content only, no syntax markers).</param>
/// <param name="Metadata">Optional metadata associated with the token.</param>
[DebuggerDisplay("MarkdownToken: {TokenType} '{Value}'")]
public sealed class MarkdownToken(
    MarkdownTokenType TokenType,
    string Value,
    Core.Metadata? Metadata = null
) : IToken<MarkdownTokenType>
{
    /// <summary>
    /// Gets the type of the markdown token.
    /// </summary>
    public MarkdownTokenType TokenType { get; } = TokenType;
    /// <summary>
    /// Gets the string value of the markdown token (renderable content only, no syntax markers).
    /// </summary>
    public string Value { get; } = Value;
    /// <summary>
    /// Gets the optional metadata associated with the token.
    /// </summary>
    public Core.Metadata? Metadata { get; } = Metadata;
}
