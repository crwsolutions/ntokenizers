using NTokenizers.Java;

namespace NTokenizers.ToHtml.Writers;

/// <summary>
/// Writer for Java tokens.
/// </summary>
internal sealed class JavaHtmlWriter : AbstractTokenToHtmlWriter<JavaToken>
{
    internal override void WriteHtml(JavaToken token, TextWriter writer)
    {
        switch (token.TokenType)
        {
            case JavaTokenType.NotDefined:
                WriteValue(writer, token.Value, null, inPreBlock: true);
                break;

            case JavaTokenType.Operator:
                WriteValue(writer, token.Value, "tok-operator", inPreBlock: true);
                break;

            case JavaTokenType.OpenParenthesis:
            case JavaTokenType.CloseParenthesis:
            case JavaTokenType.OpenBrace:
            case JavaTokenType.CloseBrace:
            case JavaTokenType.OpenBracket:
            case JavaTokenType.CloseBracket:
            case JavaTokenType.Comma:
            case JavaTokenType.Dot:
            case JavaTokenType.SequenceTerminator:
            case JavaTokenType.Colon:
            case JavaTokenType.DoubleColon:
            case JavaTokenType.QuestionMark:
                WriteValue(writer, token.Value, "tok-punctuation", inPreBlock: true);
                break;

            case JavaTokenType.StringValue:
                WriteValue(writer, token.Value, "tok-string", inPreBlock: true);
                break;

            case JavaTokenType.CharValue:
                WriteValue(writer, token.Value, "tok-char", inPreBlock: true);
                break;

            case JavaTokenType.Number:
                WriteValue(writer, token.Value, "tok-number", inPreBlock: true);
                break;

            case JavaTokenType.Boolean:
                WriteValue(writer, token.Value, "tok-boolean", inPreBlock: true);
                break;

            case JavaTokenType.Null:
                WriteValue(writer, token.Value, "tok-null", inPreBlock: true);
                break;

            case JavaTokenType.Identifier:
                WriteValue(writer, token.Value, "tok-identifier", inPreBlock: true);
                break;

            case JavaTokenType.Keyword:
                WriteValue(writer, token.Value, "tok-keyword", inPreBlock: true);
                break;

            case JavaTokenType.Comment:
                WriteValue(writer, token.Value, "tok-comment", inPreBlock: true);
                break;

            case JavaTokenType.Whitespace:
                writer.Write(token.Value);
                break;

            default:
                WriteValue(writer, token.Value, null, inPreBlock: true);
                break;
        }
    }
}
