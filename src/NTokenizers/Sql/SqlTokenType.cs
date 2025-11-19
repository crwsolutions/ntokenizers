namespace NTokenizers.Sql;

/// <summary>
/// Represents the type of a SQL token.
/// </summary>
public enum SqlTokenType
{
    /// <summary>
    /// Represents a string value enclosed in single or double quotes.
    /// </summary>
    StringValue,

    /// <summary>
    /// Represents a numeric value (integer or decimal).
    /// </summary>
    Number,

    /// <summary>
    /// Represents a SQL keyword (case-insensitive).
    /// </summary>
    Keyword,

    /// <summary>
    /// Represents an identifier (table name, column name, etc.).
    /// </summary>
    Identifier,

    /// <summary>
    /// Represents an operator (comparison, arithmetic, logical, etc.).
    /// </summary>
    Operator,

    /// <summary>
    /// Represents a comma (,).
    /// </summary>
    Comma,

    /// <summary>
    /// Represents a dot (.).
    /// </summary>
    Dot,

    /// <summary>
    /// Represents an opening parenthesis.
    /// </summary>
    OpenParenthesis,

    /// <summary>
    /// Represents a closing parenthesis.
    /// </summary>
    CloseParenthesis,

    /// <summary>
    /// Represents a sequence terminator (;).
    /// </summary>
    SequenceTerminator,

    /// <summary>
    /// Represents a token that doesn't fit into any other category.
    /// </summary>
    NotDefined,

    /// <summary>
    /// Represents a comment (inline -- or block /* */).
    /// </summary>
    Comment,

    /// <summary>
    /// Represents whitespace characters (spaces, tabs, newlines, carriage returns) between tokens.
    /// </summary>
    Whitespace
}
