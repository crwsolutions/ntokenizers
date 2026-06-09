using NTokenizers.Cpp;

namespace NTokenizers.Tools.MarkdownToHtml.Writers;

/// <summary>
/// Writer for C++ tokens.
/// </summary>
internal sealed class CppHtmlWriter : AbstractTokenToHtmlWriter<CppToken>
{
    internal override void WriteHtml(CppToken token, TextWriter writer)
    {
        switch (token.TokenType)
        {
            case CppTokenType.NotDefined:
                WriteValue(writer, token.Value, null, inPreBlock: true);
                break;

            case CppTokenType.Operator:
                WriteValue(writer, token.Value, "tok-operator", inPreBlock: true);
                break;

            case CppTokenType.OpenParenthesis:
            case CppTokenType.CloseParenthesis:
            case CppTokenType.OpenBrace:
            case CppTokenType.CloseBrace:
            case CppTokenType.OpenBracket:
            case CppTokenType.CloseBracket:
            case CppTokenType.Comma:
            case CppTokenType.Dot:
            case CppTokenType.Arrow:
            case CppTokenType.SequenceTerminator:
            case CppTokenType.Colon:
            case CppTokenType.DoubleColon:
            case CppTokenType.QuestionMark:
                WriteValue(writer, token.Value, "tok-punctuation", inPreBlock: true);
                break;

            case CppTokenType.StringValue:
                WriteValue(writer, token.Value, "tok-string", inPreBlock: true);
                break;

            case CppTokenType.CharValue:
                WriteValue(writer, token.Value, "tok-char", inPreBlock: true);
                break;

            case CppTokenType.Number:
                WriteValue(writer, token.Value, "tok-number", inPreBlock: true);
                break;

            case CppTokenType.Boolean:
                WriteValue(writer, token.Value, "tok-boolean", inPreBlock: true);
                break;

            case CppTokenType.Null:
                WriteValue(writer, token.Value, "tok-null", inPreBlock: true);
                break;

            case CppTokenType.Identifier:
                WriteValue(writer, token.Value, "tok-identifier", inPreBlock: true);
                break;

            case CppTokenType.Keyword:
                WriteValue(writer, token.Value, "tok-keyword", inPreBlock: true);
                break;

            case CppTokenType.Comment:
                WriteValue(writer, token.Value, "tok-comment", inPreBlock: true);
                break;

            case CppTokenType.Whitespace:
                writer.Write(token.Value);
                break;

            default:
                WriteValue(writer, token.Value, null, inPreBlock: true);
                break;
        }
    }
}
