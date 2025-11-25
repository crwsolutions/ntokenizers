namespace NTokenizers;

/// <summary>
/// Generic interface for a token.
/// </summary>
public interface IToken
{
    /// <summary>
    /// Value of the current token as a string.
    /// </summary>
    string Value { get; }
}

/// <summary>
/// Generic interface for a token.
/// </summary>
public interface IToken<TTokentype> : IToken where TTokentype : Enum
{

    /// <summary>
    /// Token type of the current token.
    /// </summary>
    TTokentype TokenType { get; }
}