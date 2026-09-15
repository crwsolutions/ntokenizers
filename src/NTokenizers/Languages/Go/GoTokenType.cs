namespace NTokenizers.Go;

/// <summary>
/// Represents the type of a Go token.
/// </summary>
public enum GoTokenType
{
    /// <summary>
    /// Represents a token that does not fit any defined category.
    /// </summary>
    NotDefined,

    /// <summary>
    /// Represents any operator.
    /// </summary>
    Operator,

    /// <summary>
    /// Represents an open parenthesis (.
    /// </summary>
    OpenParenthesis,

    /// <summary>
    /// Represents a close parenthesis ).
    /// </summary>
    CloseParenthesis,

    /// <summary>
    /// Represents an open brace {.
    /// </summary>
    OpenBrace,

    /// <summary>
    /// Represents a close brace }.
    /// </summary>
    CloseBrace,

    /// <summary>
    /// Represents an open bracket [.
    /// </summary>
    OpenBracket,

    /// <summary>
    /// Represents a close bracket ].
    /// </summary>
    CloseBracket,

    /// <summary>
    /// Represents a comma ,.
    /// </summary>
    Comma,

    /// <summary>
    /// Represents a dot ..
    /// </summary>
    Dot,

    /// <summary>
    /// Represents a semicolon ; - the statement terminator.
    /// </summary>
    SequenceTerminator,

    /// <summary>
    /// Represents a colon :.
    /// </summary>
    Colon,

    /// <summary>
    /// Represents a double colon ::.
    /// </summary>
    DoubleColon,

    /// <summary>
    /// Represents an at sign @.
    /// </summary>
    At,

    /// <summary>
    /// Represents a pound sign #.
    /// </summary>
    Pound,

    /// <summary>
    /// Represents a question mark ?.
    /// </summary>
    QuestionMark,

    /// <summary>
    /// Represents a string value enclosed in double quotes or backticks.
    /// </summary>
    StringValue,

    /// <summary>
    /// Represents a char value enclosed in single quotes.
    /// </summary>
    CharValue,

    /// <summary>
    /// Represents a numeric value (integer, float, scientific notation, hex, octal, binary).
    /// </summary>
    Number,

    /// <summary>
    /// Represents a boolean value (true, false).
    /// </summary>
    Boolean,

    /// <summary>
    /// Represents a null value (nil).
    /// </summary>
    Null,

    /// <summary>
    /// Represents an identifier (variable name, function name, etc.).
    /// </summary>
    Identifier,

    /// <summary>
    /// Represents a Go keyword.
    /// </summary>
    Keyword,

    /// <summary>
    /// Represents a comment (// or /* */).
    /// </summary>
    Comment,

    /// <summary>
    /// Represents whitespace characters (spaces, tabs, newlines).
    /// </summary>
    Whitespace,
}
