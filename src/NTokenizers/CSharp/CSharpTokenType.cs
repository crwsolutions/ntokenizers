namespace NTokenizers.CSharp;

/// <summary>
/// Represents the type of a C# token.
/// </summary>
public enum CSharpTokenType
{
    /// <summary>
    /// Represents a token that does not fit any defined category.
    /// </summary>
    NotDefined,

    /// <summary>
    /// Represents the logical AND operator (&amp;&amp;).
    /// </summary>
    And,

    /// <summary>
    /// Represents the logical OR operator (||).
    /// </summary>
    Or,

    /// <summary>
    /// Represents the logical NOT operator (!).
    /// </summary>
    Not,

    /// <summary>
    /// Represents the equality operator (==).
    /// </summary>
    Equals,

    /// <summary>
    /// Represents the inequality operator (!=).
    /// </summary>
    NotEquals,

    /// <summary>
    /// Represents the greater than operator (>).
    /// </summary>
    GreaterThan,

    /// <summary>
    /// Represents the less than operator (&lt;).
    /// </summary>
    LessThan,

    /// <summary>
    /// Represents the greater than or equal operator (&gt;=).
    /// </summary>
    GreaterThanOrEqual,

    /// <summary>
    /// Represents the less than or equal operator (&lt;=).
    /// </summary>
    LessThanOrEqual,

    /// <summary>
    /// Represents the addition operator (+).
    /// </summary>
    Plus,

    /// <summary>
    /// Represents the subtraction operator (-).
    /// </summary>
    Minus,

    /// <summary>
    /// Represents the multiplication operator (*).
    /// </summary>
    Multiply,

    /// <summary>
    /// Represents the division operator (/).
    /// </summary>
    Divide,

    /// <summary>
    /// Represents the modulo operator (%).
    /// </summary>
    Modulo,

    /// <summary>
    /// Represents an open parenthesis (().
    /// </summary>
    OpenParenthesis,

    /// <summary>
    /// Represents a close parenthesis ()).
    /// </summary>
    CloseParenthesis,

    /// <summary>
    /// Represents a comma (,).
    /// </summary>
    Comma,

    /// <summary>
    /// Represents a dot (.).
    /// </summary>
    Dot,

    /// <summary>
    /// Represents a semicolon (;) - the statement terminator.
    /// </summary>
    SequenceTerminator,

    /// <summary>
    /// Represents a string value enclosed in double quotes or verbatim string.
    /// </summary>
    StringValue,

    /// <summary>
    /// Represents a numeric value (integer, decimal, or scientific notation).
    /// </summary>
    Number,

    /// <summary>
    /// Represents an identifier (variable name, method name, etc.).
    /// </summary>
    Identifier,

    /// <summary>
    /// Represents a C# keyword (case-insensitive).
    /// </summary>
    Keyword,

    /// <summary>
    /// Represents a comment (inline // or block /* */).
    /// </summary>
    Comment,

    /// <summary>
    /// Represents an operator not covered by specific operator types.
    /// </summary>
    Operator,

    /// <summary>
    /// Represents whitespace characters (spaces, tabs, newlines).
    /// </summary>
    Whitespace
}
