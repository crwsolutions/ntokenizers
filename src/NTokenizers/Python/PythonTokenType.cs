namespace NTokenizers.Python;

/// <summary>
/// Represents the type of a Python token.
/// </summary>
public enum PythonTokenType
{
    /// <summary>
    /// Represents a token that does not fit any defined category.
    /// </summary>
    NotDefined,

    /// <summary>
    /// Represents an operator (+, -, *, /, //, %, **, =, ==, !=, &lt;, &gt;, &lt;=, &gt;=, :=, @, etc.).
    /// </summary>
    Operator,

    /// <summary>
    /// Represents an open parenthesis (().
    /// </summary>
    OpenParenthesis,

    /// <summary>
    /// Represents a close parenthesis ()).
    /// </summary>
    CloseParenthesis,

    /// <summary>
    /// Represents an open brace ({).
    /// </summary>
    OpenBrace,

    /// <summary>
    /// Represents a close brace (}).
    /// </summary>
    CloseBrace,

    /// <summary>
    /// Represents an open bracket ([).
    /// </summary>
    OpenBracket,

    /// <summary>
    /// Represents a close bracket (]).
    /// </summary>
    CloseBracket,

    /// <summary>
    /// Represents a comma (,).
    /// </summary>
    Comma,

    /// <summary>
    /// Represents a dot (.).
    /// </summary>
    Dot,

    /// <summary>
    /// Represents a colon (:).
    /// </summary>
    Colon,

    /// <summary>
    /// Represents a semicolon (;).
    /// </summary>
    Semicolon,

    /// <summary>
    /// Represents a decorator at sign (@).
    /// </summary>
    Hash,

    /// <summary>
    /// Represents a string value (single, double, triple quotes, f-strings, raw strings, byte strings).
    /// </summary>
    StringValue,

    /// <summary>
    /// Represents a numeric value (integer, float, hex, binary, octal, complex).
    /// </summary>
    Number,

    /// <summary>
    /// Represents an identifier (variable name, function name, etc.).
    /// </summary>
    Identifier,

    /// <summary>
    /// Represents a Python keyword (def, class, if, for, return, etc.).
    /// </summary>
    Keyword,

    /// <summary>
    /// Represents a comment (# line comment).
    /// </summary>
    Comment,

    /// <summary>
    /// Represents whitespace characters (spaces, tabs, newlines).
    /// </summary>
    Whitespace,
}
