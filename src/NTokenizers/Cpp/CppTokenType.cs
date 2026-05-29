namespace NTokenizers.Cpp;

/// <summary>
/// Represents the type of a C++ token.
/// </summary>
public enum CppTokenType
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
