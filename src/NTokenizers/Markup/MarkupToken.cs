using NTokenizers.Core;
using NTokenizers.Markup.Metadata;
using System.Diagnostics;

namespace NTokenizers.Markup;

/// <summary>
/// Represents a markup token with its type, value, and optional metadata.
/// </summary>
/// <param name="TokenType">The type of the markup token.</param>
/// <param name="Value">The string value of the markup token (renderable content only, no syntax markers).</param>
/// <param name="Metadata">Optional metadata associated with the token.</param>
[DebuggerDisplay("MarkupToken: {TokenType} {Value}")]
public sealed class MarkupToken(
    MarkupTokenType TokenType,
    string Value,
    MarkupMetadata? Metadata = null
) : IToken<MarkupTokenType>
{
    /// <summary>
    /// Gets the type of the markup token.
    /// </summary>
    public MarkupTokenType TokenType { get; } = TokenType;
    /// <summary>
    /// Gets the string value of the markup token (renderable content only, no syntax markers).
    /// </summary>
    public string Value { get; } = Value;
    /// <summary>
    /// Gets the optional metadata associated with the token.
    /// </summary>
    public MarkupMetadata? Metadata { get; } = Metadata;
}
