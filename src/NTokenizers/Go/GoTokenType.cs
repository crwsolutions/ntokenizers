namespace NTokenizers.Go;

/// <summary>
/// Represents the type of a Go token.
/// </summary>
public enum GoTokenType
{
    NotDefined,
    Operator,
    OpenParenthesis,
    CloseParenthesis,
    OpenBrace,
    CloseBrace,
    OpenBracket,
    CloseBracket,
    Comma,
    Dot,
    Arrow,
    SequenceTerminator,
    Colon,
    DoubleColon,
    At,
    Pound,
    QuestionMark,
    StringValue,
    CharValue,
    Number,
    Boolean,
    Null,
    Identifier,
    Keyword,
    Comment,
    Whitespace,
}
