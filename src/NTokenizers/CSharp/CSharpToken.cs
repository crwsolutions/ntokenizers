using NTokenizers.Core;

namespace NTokenizers.CSharp;

/// <summary>
/// Represents a C# token with its type and value.
/// </summary>
public class CSharpToken : IToken<CSharpTokenType>
{
    /// <summary>
    /// Gets the type of the C# token represented by this instance.
    /// </summary>
    public CSharpTokenType TokenType { get; }
    
    /// <summary>
    /// Gets the string representation of the current value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CSharpToken"/> class with the specified token type and value.
    /// </summary>
    /// <param name="tokenType">The type of the C# token.</param>
    /// <param name="value">The string representation of the token's value.</param>
    public CSharpToken(CSharpTokenType tokenType, string value)
    {
        TokenType = tokenType;
        Value = value;
    }
}
