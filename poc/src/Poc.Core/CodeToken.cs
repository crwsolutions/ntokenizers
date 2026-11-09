namespace Poc.Core;

/// <summary>
/// The kind of a code token.
/// </summary>
public enum CodeTokenType
{
    /// <summary>A language keyword.</summary>
    Keyword,

    /// <summary>An identifier that is not a keyword.</summary>
    Identifier,

    /// <summary>A numeric literal.</summary>
    Number,

    /// <summary>A quoted string literal.</summary>
    StringValue,

    /// <summary>A line comment (// ...).</summary>
    Comment,

    /// <summary>Operators and punctuation, grouped into runs.</summary>
    Punctuation,

    /// <summary>Whitespace, including newlines (preserved, per the library design principle).</summary>
    Whitespace,
}

/// <summary>
/// A token produced by <see cref="CodeTokenizer"/>, the code sub-tokenizer.
/// </summary>
public sealed class CodeToken : IToken
{
    public CodeToken(CodeTokenType type, string value)
    {
        Type = type;
        Value = value;
    }

    /// <summary>The kind of this token.</summary>
    public CodeTokenType Type { get; }

    /// <summary>The raw text of the token.</summary>
    public string Value { get; }
}
