using NTokenizers.Core;
using System.Diagnostics;

namespace NTokenizers.Java;

/// <summary>
/// Represents a Java token with its type and value.
/// </summary>
[DebuggerDisplay("JavaToken: {TokenType} '{Value}'")]
public class JavaToken : IToken<JavaTokenType>
{
    /// <summary>
    /// Gets the type of the Java token represented by this instance.
    /// </summary>
    public JavaTokenType TokenType { get; }

    /// <summary>
    /// Gets the string representation of the current value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="JavaToken"/> class with the specified token type and value.
    /// </summary>
    /// <param name="tokenType">The type of the Java token.</param>
    /// <param name="value">The string representation of the token's value.</param>
    public JavaToken(JavaTokenType tokenType, string value)
    {
        TokenType = tokenType;
        Value = value;
    }
}
