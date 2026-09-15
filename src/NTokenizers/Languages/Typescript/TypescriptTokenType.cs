namespace NTokenizers.Typescript;

/// <summary>
/// Defines the types of tokens that can be recognized in TypeScript code.
/// </summary>
public enum TypescriptTokenType
{
    /// <summary>
    /// Token that is not defined or recognized.
    /// </summary>
    NotDefined,

    /// <summary>
    /// Logical AND operator (&amp;&amp;).
    /// </summary>
    And,

    /// <summary>
    /// Application-specific token.
    /// </summary>
    Application,

    /// <summary>
    /// Between operator or keyword.
    /// </summary>
    Between,

    /// <summary>
    /// Closing parenthesis ')'.
    /// </summary>
    CloseParenthesis,

    /// <summary>
    /// Comma separator ','.
    /// </summary>
    Comma,

    /// <summary>
    /// DateTime value literal.
    /// </summary>
    DateTimeValue,

    /// <summary>
    /// Equality operator (== or ===).
    /// </summary>
    Equals,

    /// <summary>
    /// Exception type identifier.
    /// </summary>
    ExceptionType,

    /// <summary>
    /// Fingerprint identifier.
    /// </summary>
    Fingerprint,

    /// <summary>
    /// 'in' operator or keyword.
    /// </summary>
    In,

    /// <summary>
    /// Invalid token.
    /// </summary>
    Invalid,

    /// <summary>
    /// 'like' operator.
    /// </summary>
    Like,

    /// <summary>
    /// 'limit' keyword.
    /// </summary>
    Limit,

    /// <summary>
    /// 'match' keyword.
    /// </summary>
    Match,

    /// <summary>
    /// Message identifier.
    /// </summary>
    Message,

    /// <summary>
    /// Inequality operator (!= or !==).
    /// </summary>
    NotEquals,

    /// <summary>
    /// 'not in' operator.
    /// </summary>
    NotIn,

    /// <summary>
    /// 'not like' operator.
    /// </summary>
    NotLike,

    /// <summary>
    /// Numeric literal.
    /// </summary>
    Number,

    /// <summary>
    /// Logical OR operator (||).
    /// </summary>
    Or,

    /// <summary>
    /// Opening parenthesis '('.
    /// </summary>
    OpenParenthesis,

    /// <summary>
    /// Stack frame identifier.
    /// </summary>
    StackFrame,

    /// <summary>
    /// String literal value.
    /// </summary>
    StringValue,

    /// <summary>
    /// Semicolon ';' statement terminator.
    /// </summary>
    SequenceTerminator,

    /// <summary>
    /// Identifier (variable, function name, etc.).
    /// </summary>
    Identifier,

    /// <summary>
    /// TypeScript keyword.
    /// </summary>
    Keyword,

    /// <summary>
    /// Comment (line or block).
    /// </summary>
    Comment,

    /// <summary>
    /// Operator (+, -, *, /, %, etc.).
    /// </summary>
    Operator,

    /// <summary>
    /// Dot operator '.'.
    /// </summary>
    Dot,

    /// <summary>
    /// Whitespace characters.
    /// </summary>
    Whitespace
}
