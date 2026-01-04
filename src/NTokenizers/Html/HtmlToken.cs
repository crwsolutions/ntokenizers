using NTokenizers.Core;
using System.Diagnostics;

namespace NTokenizers.Html;

/// <summary>
/// Represents an HTML token with its type and value.
/// </summary>
/// <param name="TokenType">The type of the HTML token.</param>
/// <param name="Value">The string value of the HTML token.</param>
[DebuggerDisplay("HtmlTokenType: {TokenType} '{Value}'")]
public class HtmlToken(HtmlTokenType TokenType, string Value) : IToken<HtmlTokenType>
{
    /// <summary>
    /// Gets the type of the HTML token.
    /// </summary>
    public HtmlTokenType TokenType { get; } = TokenType;

    /// <summary>
    /// Gets the value associated with this instance.
    /// </summary>
    public string Value { get; } = Value;
}
