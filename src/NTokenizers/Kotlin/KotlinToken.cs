using NTokenizers.Core;
using System.Diagnostics;

namespace NTokenizers.Kotlin;

/// <summary>
/// Represents a Kotlin token with its type and value.
/// </summary>
[DebuggerDisplay("KotlinToken: {TokenType} '{Value}'")]
public class KotlinToken : IToken<KotlinTokenType>
{
    /// <summary>
    /// Gets the type of the Kotlin token represented by this instance.
    /// </summary>
    public KotlinTokenType TokenType { get; }

    /// <summary>
    /// Gets the string representation of the current value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="KotlinToken"/> class with the specified token type and value.
    /// </summary>
    /// <param name="tokenType">The type of the Kotlin token.</param>
    /// <param name="value">The string representation of the token's value.</param>
    public KotlinToken(KotlinTokenType tokenType, string value)
    {
        TokenType = tokenType;
        Value = value;
    }
}
