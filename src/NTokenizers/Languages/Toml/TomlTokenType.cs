namespace NTokenizers.Toml;

/// <summary>
/// Represents the type of a TOML token.
/// </summary>
public enum TomlTokenType
{
    /// <summary>
    /// Whitespace: spaces, tabs, newlines.
    /// </summary>
    Whitespace,

    /// <summary>
    /// Comment: starts with # and extends to end of line.
    /// </summary>
    Comment,

    /// <summary>
    /// Identifier: bare keys, dotted key segments, and keywords like true/false/inf/nan.
    /// </summary>
    Identifier,

    /// <summary>
    /// Dot separator: used in dotted keys (a.b.c).
    /// </summary>
    Dot,

    /// <summary>
    /// Equal sign: key-value separator.
    /// </summary>
    Equal,

    /// <summary>
    /// Comma: separator in arrays and inline tables.
    /// </summary>
    Comma,

    /// <summary>
    /// Opening bracket: used in arrays and table headers.
    /// </summary>
    OpenBracket,

    /// <summary>
    /// Closing bracket: used in arrays and table headers.
    /// </summary>
    CloseBracket,

    /// <summary>
    /// Opening brace: used in inline tables.
    /// </summary>
    OpenBrace,

    /// <summary>
    /// Closing brace: used in inline tables.
    /// </summary>
    CloseBrace,

    /// <summary>
    /// String quote: opening or closing quote character for basic or literal strings.
    /// </summary>
    StringQuote,

    /// <summary>
    /// String value: the content between quotes (includes surrounding quotes).
    /// </summary>
    StringValue,

    /// <summary>
    /// Number: integer, float, scientific notation, hex, octal, binary, inf, nan.
    /// </summary>
    Number,

    /// <summary>
    /// Boolean: true or false.
    /// </summary>
    Boolean,

    /// <summary>
    /// Date, time, or date-time token.
    /// </summary>
    DateTime,
}
