using NTokenizers.Core;
using System.Diagnostics;

namespace NTokenizers.Python;

/// <summary>
/// Represents a Python token with its type and value.
/// </summary>
[DebuggerDisplay("PythonToken: {TokenType} '{Value}'")]
public class PythonToken : IToken<PythonTokenType>
{
    /// <summary>
    /// Gets the type of the Python token represented by this instance.
    /// </summary>
    public PythonTokenType TokenType { get; }
    
    /// <summary>
    /// Gets the string representation of the current value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PythonToken"/> class with the specified token type and value.
    /// </summary>
    /// <param name="tokenType">The type of the Python token.</param>
    /// <param name="value">The string representation of the token's value.</param>
    public PythonToken(PythonTokenType tokenType, string value)
    {
        TokenType = tokenType;
        Value = value;
    }
}
