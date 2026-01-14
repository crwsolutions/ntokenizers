using NTokenizers.Core;
using System.Diagnostics;

namespace NTokenizers.Html;

/// <summary>
/// Represents an HTML token with its type and value.
/// </summary>
/// <param name="TokenType">The type of the HTML token.</param>
/// <param name="Value">The string value of the HTML token.</param>
/// <param name="Metadata">Optional metadata associated with the token.</param>
[DebuggerDisplay("HtmlTokenType: {TokenType} '{Value}'")]
public class HtmlToken(
    HtmlTokenType TokenType,
    string Value,
    Core.Metadata? Metadata = null
    ) : IToken<HtmlTokenType>
{
    /// <summary>
    /// Gets the type of the HTML token.
    /// </summary>
    public HtmlTokenType TokenType { get; } = TokenType;

    /// <summary>
    /// Gets the value associated with this instance.
    /// </summary>
    public string Value { get; } = Value;
    /// <summary>
    /// Gets the optional metadata associated with the token.
    /// </summary>
    public Core.Metadata? Metadata { get; } = Metadata;
}
