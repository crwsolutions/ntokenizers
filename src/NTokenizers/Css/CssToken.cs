using NTokenizers.Core;
using System.Diagnostics;

namespace NTokenizers.Css;

/// <summary>
/// Represents a CSS token with its type and value.
/// </summary>
[DebuggerDisplay("CssToken: {TokenType} '{Value}'")]
public class CssToken : IToken<CssTokenType>
{
    /// <summary>
    /// Gets the type of the current CSS token.
    /// </summary>
    public CssTokenType TokenType { get; }

    /// <summary>
    /// Gets the current value as a string.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CssToken"/> class with the specified token type and value.
    /// </summary>
    /// <param name="tokenType">The type of the CSS token.</param>
    /// <param name="value">The string representation of the CSS token's value.</param>
    public CssToken(CssTokenType tokenType, string value)
    {
        TokenType = tokenType;
        Value = value;
    }
}