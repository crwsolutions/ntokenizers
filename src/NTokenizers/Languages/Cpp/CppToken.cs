using NTokenizers.Core;
using System.Diagnostics;

namespace NTokenizers.Cpp;

/// <summary>
/// Represents a C++ token with its type and value.
/// </summary>
[DebuggerDisplay("CppToken: {TokenType} '{Value}'")]
public class CppToken : IToken<CppTokenType>
{
    /// <summary>
    /// Gets the type of the C++ token represented by this instance.
    /// </summary>
    public CppTokenType TokenType { get; }

    /// <summary>
    /// Gets the string representation of the current value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CppToken"/> class with the specified token type and value.
    /// </summary>
    /// <param name="tokenType">The type of the C++ token.</param>
    /// <param name="value">The string representation of the token's value.</param>
    public CppToken(CppTokenType tokenType, string value)
    {
        TokenType = tokenType;
        Value = value;
    }
}
