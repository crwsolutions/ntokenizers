using NTokenizers.Core;
using System.Diagnostics;

namespace NTokenizers.Go;

/// <summary>
/// Represents a Go token with its type and value.
/// </summary>
[DebuggerDisplay("GoToken: {TokenType} '{Value}'")]
public class GoToken : IToken<GoTokenType>
{
    /// <summary>
    /// Gets the type of the Go token represented by this instance.
    /// </summary>
    public GoTokenType TokenType { get; }

    /// <summary>
    /// Gets the string representation of the current value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GoToken"/> class with the specified token type and value.
    /// </summary>
    /// <param name="tokenType">The type of the Go token.</param>
    /// <param name="value">The string representation of the token's value.</param>
    public GoToken(GoTokenType tokenType, string value)
    {
        TokenType = tokenType;
        Value = value;
    }
}
