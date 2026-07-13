using NTokenizers.Swift;

namespace NTokenizers.ToHtml.Writers;

/// <summary>
/// Writer for Swift tokens.
/// </summary>
internal sealed class SwiftHtmlWriter : AbstractTokenToHtmlWriter<SwiftToken>
{
    internal override void WriteHtml(SwiftToken token, TextWriter writer)
    {
        switch (token.TokenType)
        {
            case SwiftTokenType.NotDefined:
                WriteValue(writer, token.Value, null, inPreBlock: true);
                break;

            case SwiftTokenType.Operator:
                WriteValue(writer, token.Value, "tok-operator", inPreBlock: true);
                break;

            case SwiftTokenType.OpenParenthesis:
            case SwiftTokenType.CloseParenthesis:
            case SwiftTokenType.OpenBrace:
            case SwiftTokenType.CloseBrace:
            case SwiftTokenType.OpenBracket:
            case SwiftTokenType.CloseBracket:
            case SwiftTokenType.Comma:
            case SwiftTokenType.Dot:
            case SwiftTokenType.SequenceTerminator:
            case SwiftTokenType.Colon:
            case SwiftTokenType.DoubleColon:
            case SwiftTokenType.At:
            case SwiftTokenType.Pound:
            case SwiftTokenType.QuestionMark:
                WriteValue(writer, token.Value, "tok-punctuation", inPreBlock: true);
                break;

            case SwiftTokenType.StringValue:
                WriteValue(writer, token.Value, "tok-string", inPreBlock: true);
                break;

            case SwiftTokenType.CharValue:
                WriteValue(writer, token.Value, "tok-char", inPreBlock: true);
                break;

            case SwiftTokenType.Number:
                WriteValue(writer, token.Value, "tok-number", inPreBlock: true);
                break;

            case SwiftTokenType.Boolean:
                WriteValue(writer, token.Value, "tok-boolean", inPreBlock: true);
                break;

            case SwiftTokenType.Null:
                WriteValue(writer, token.Value, "tok-null", inPreBlock: true);
                break;

            case SwiftTokenType.Identifier:
                WriteValue(writer, token.Value, "tok-identifier", inPreBlock: true);
                break;

            case SwiftTokenType.Keyword:
                WriteValue(writer, token.Value, "tok-keyword", inPreBlock: true);
                break;

            case SwiftTokenType.Comment:
                WriteValue(writer, token.Value, "tok-comment", inPreBlock: true);
                break;

            case SwiftTokenType.Whitespace:
                writer.Write(token.Value);
                break;

            default:
                WriteValue(writer, token.Value, null, inPreBlock: true);
                break;
        }
    }
}
