namespace NTokenizers.Typescript;

/// <summary>
/// Represents a TypeScript token with its type and value.
/// </summary>
public class TypescriptToken : IToken<TypescriptTokenType>
{
    /// <summary>
    /// Gets the type of the TypeScript token.
    /// </summary>
    public TypescriptTokenType TokenType { get; }

    /// <summary>
    /// Gets the string representation of the current value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TypescriptToken"/> class with the specified token type and value.
    /// </summary>
    /// <param name="tokenType">The type of the token, indicating its role in the TypeScript syntax.</param>
    /// <param name="value">The string representation of the token's value.</param>
    public TypescriptToken(TypescriptTokenType tokenType, string value)
    {
        TokenType = tokenType;
        Value = value;
    }
}
