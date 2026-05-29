using NTokenizers.Core;
using System.Diagnostics;

namespace NTokenizers.Swift;

/// <summary>
/// Represents a Swift token with its type and value.
/// </summary>
[DebuggerDisplay("SwiftToken: {TokenType} '{Value}'")]
public class SwiftToken : IToken<SwiftTokenType>
{
    /// <summary>
    /// Gets the type of the Swift token represented by this instance.
    /// </summary>
    public SwiftTokenType TokenType { get; }

    /// <summary>
    /// Gets the string representation of the current value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SwiftToken"/> class with the specified token type and value.
    /// </summary>
    /// <param name="tokenType">The type of the Swift token.</param>
    /// <param name="value">The string representation of the token's value.</param>
    public SwiftToken(SwiftTokenType tokenType, string value)
    {
        TokenType = tokenType;
        Value = value;
    }
}
