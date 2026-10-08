using NTokenizers.Go;

namespace NTokenizers.ToHtml.Writers;

/// <summary>
/// Writer for Go tokens.
/// </summary>
internal sealed class GoHtmlWriter : AbstractTokenToHtmlWriter<GoToken>
{
    internal override void WriteHtml(GoToken token, TextWriter writer)
    {
        switch (token.TokenType)
        {
            case GoTokenType.NotDefined:
                WriteValue(writer, token.Value, null, inPreBlock: true);
                break;

            case GoTokenType.Operator:
                WriteValue(writer, token.Value, "tok-operator", inPreBlock: true);
                break;

            case GoTokenType.OpenParenthesis:
            case GoTokenType.CloseParenthesis:
            case GoTokenType.OpenBrace:
            case GoTokenType.CloseBrace:
            case GoTokenType.OpenBracket:
            case GoTokenType.CloseBracket:
            case GoTokenType.Comma:
            case GoTokenType.Dot:
            case GoTokenType.SequenceTerminator:
            case GoTokenType.Colon:
            case GoTokenType.DoubleColon:
            case GoTokenType.At:
            case GoTokenType.Pound:
            case GoTokenType.QuestionMark:
                WriteValue(writer, token.Value, "tok-punctuation", inPreBlock: true);
                break;

            case GoTokenType.StringValue:
                WriteValue(writer, token.Value, "tok-string", inPreBlock: true);
                break;

            case GoTokenType.CharValue:
                WriteValue(writer, token.Value, "tok-char", inPreBlock: true);
                break;

            case GoTokenType.Number:
                WriteValue(writer, token.Value, "tok-number", inPreBlock: true);
                break;

            case GoTokenType.Boolean:
                WriteValue(writer, token.Value, "tok-boolean", inPreBlock: true);
                break;

            case GoTokenType.Null:
                WriteValue(writer, token.Value, "tok-null", inPreBlock: true);
                break;

            case GoTokenType.Identifier:
                WriteValue(writer, token.Value, "tok-identifier", inPreBlock: true);
                break;

            case GoTokenType.Keyword:
                WriteValue(writer, token.Value, "tok-keyword", inPreBlock: true);
                break;

            case GoTokenType.Comment:
                WriteValue(writer, token.Value, "tok-comment", inPreBlock: true);
                break;

            case GoTokenType.Whitespace:
                writer.Write(token.Value);
                break;

            default:
                WriteValue(writer, token.Value, null, inPreBlock: true);
                break;
        }
    }
}
