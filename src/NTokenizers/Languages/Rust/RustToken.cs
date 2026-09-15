using NTokenizers.Core;
using System.Diagnostics;

namespace NTokenizers.Rust;

/// <summary>
/// Represents a Rust token with its type and value.
/// </summary>
[DebuggerDisplay("RustToken: {TokenType} '{Value}'")]
public class RustToken : IToken<RustTokenType>
{
    /// <summary>
    /// Gets the type of the Rust token represented by this instance.
    /// </summary>
    public RustTokenType TokenType { get; }

    /// <summary>
    /// Gets the string representation of the current value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RustToken"/> class with the specified token type and value.
    /// </summary>
    /// <param name="tokenType">The type of the Rust token.</param>
    /// <param name="value">The string representation of the token's value.</param>
    public RustToken(RustTokenType tokenType, string value)
    {
        TokenType = tokenType;
        Value = value;
    }
}
