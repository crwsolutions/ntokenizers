using NTokenizers.Kotlin;

namespace NTokenizers.Tools.MarkdownToHtml.Writers;

/// <summary>
/// Writer for Kotlin tokens.
/// </summary>
internal sealed class KotlinHtmlWriter : AbstractTokenToHtmlWriter<KotlinToken>
{
    internal override void WriteHtml(KotlinToken token, TextWriter writer)
    {
        switch (token.TokenType)
        {
            case KotlinTokenType.NotDefined:
                WriteValue(writer, token.Value, null, inPreBlock: true);
                break;

            case KotlinTokenType.Operator:
                WriteValue(writer, token.Value, "tok-operator", inPreBlock: true);
                break;

            case KotlinTokenType.OpenParenthesis:
            case KotlinTokenType.CloseParenthesis:
            case KotlinTokenType.OpenBrace:
            case KotlinTokenType.CloseBrace:
            case KotlinTokenType.OpenBracket:
            case KotlinTokenType.CloseBracket:
            case KotlinTokenType.Comma:
            case KotlinTokenType.Dot:
            case KotlinTokenType.SequenceTerminator:
            case KotlinTokenType.Colon:
            case KotlinTokenType.DoubleColon:
            case KotlinTokenType.QuestionMark:
            case KotlinTokenType.At:
            case KotlinTokenType.Pound:
                WriteValue(writer, token.Value, "tok-punctuation", inPreBlock: true);
                break;

            case KotlinTokenType.StringValue:
                WriteValue(writer, token.Value, "tok-string", inPreBlock: true);
                break;

            case KotlinTokenType.CharValue:
                WriteValue(writer, token.Value, "tok-char", inPreBlock: true);
                break;

            case KotlinTokenType.Number:
                WriteValue(writer, token.Value, "tok-number", inPreBlock: true);
                break;

            case KotlinTokenType.Boolean:
                WriteValue(writer, token.Value, "tok-boolean", inPreBlock: true);
                break;

            case KotlinTokenType.Null:
                WriteValue(writer, token.Value, "tok-null", inPreBlock: true);
                break;

            case KotlinTokenType.Identifier:
                WriteValue(writer, token.Value, "tok-identifier", inPreBlock: true);
                break;

            case KotlinTokenType.Keyword:
                WriteValue(writer, token.Value, "tok-keyword", inPreBlock: true);
                break;

            case KotlinTokenType.Comment:
                WriteValue(writer, token.Value, "tok-comment", inPreBlock: true);
                break;

            case KotlinTokenType.Whitespace:
                writer.Write(token.Value);
                break;

            default:
                WriteValue(writer, token.Value, null, inPreBlock: true);
                break;
        }
    }
}
