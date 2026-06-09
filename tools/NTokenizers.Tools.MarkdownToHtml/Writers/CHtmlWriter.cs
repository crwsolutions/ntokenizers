using NTokenizers.C;
using System.Text;

namespace NTokenizers.Tools.MarkdownToHtml.Writers;

/// <summary>
/// Writer for C tokens.
/// </summary>
internal sealed class CHtmlWriter : AbstractTokenToHtmlWriter<CToken>
{
    internal override void WriteAdditionalCss(StringBuilder css)
    {
        // C-only: preprocessor directive style
        css.AppendLine(".tok-c-preprocessor { color: #A9A9A9; font-weight: bold; }");
        css.AppendLine();
    }

    internal override void WriteHtml(CToken token, TextWriter writer)
    {
        switch (token.TokenType)
        {
            case CTokenType.NotDefined:
                WriteValue(writer, token.Value, null, inPreBlock: true);
                break;

            case CTokenType.Operator:
                WriteValue(writer, token.Value, "tok-operator", inPreBlock: true);
                break;

            case CTokenType.OpenParenthesis:
            case CTokenType.CloseParenthesis:
            case CTokenType.OpenBrace:
            case CTokenType.CloseBrace:
            case CTokenType.OpenBracket:
            case CTokenType.CloseBracket:
            case CTokenType.Comma:
            case CTokenType.Dot:
            case CTokenType.Arrow:
            case CTokenType.SequenceTerminator:
            case CTokenType.Colon:
                WriteValue(writer, token.Value, "tok-punctuation", inPreBlock: true);
                break;

            case CTokenType.StringValue:
                WriteValue(writer, token.Value, "tok-string", inPreBlock: true);
                break;

            case CTokenType.CharValue:
                WriteValue(writer, token.Value, "tok-char", inPreBlock: true);
                break;

            case CTokenType.Number:
                WriteValue(writer, token.Value, "tok-number", inPreBlock: true);
                break;

            case CTokenType.Identifier:
                WriteValue(writer, token.Value, "tok-identifier", inPreBlock: true);
                break;

            case CTokenType.Keyword:
                WriteValue(writer, token.Value, "tok-keyword", inPreBlock: true);
                break;

            case CTokenType.Preprocessor:
                WriteValue(writer, token.Value, "tok-c-preprocessor", inPreBlock: true);
                break;

            case CTokenType.Comment:
                WriteValue(writer, token.Value, "tok-comment", inPreBlock: true);
                break;

            case CTokenType.Whitespace:
                writer.Write(token.Value);
                break;

            default:
                WriteValue(writer, token.Value, null, inPreBlock: true);
                break;
        }
    }
}
