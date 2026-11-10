using System.Text;
using NTokenizers.C;

namespace NTokenizers.ToHtml.Writers;

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

    internal override async Task WriteHtmlAsync(CToken token, TextWriter writer, CancellationToken ct = default)
    {
        switch (token.TokenType)
        {
            case CTokenType.NotDefined:
                await WriteValueAsync(writer, token.Value, null, inPreBlock: true);
                break;

            case CTokenType.Operator:
                await WriteValueAsync(writer, token.Value, "tok-operator", inPreBlock: true);
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
                await WriteValueAsync(writer, token.Value, "tok-punctuation", inPreBlock: true);
                break;

            case CTokenType.StringValue:
                await WriteValueAsync(writer, token.Value, "tok-string", inPreBlock: true);
                break;

            case CTokenType.CharValue:
                await WriteValueAsync(writer, token.Value, "tok-char", inPreBlock: true);
                break;

            case CTokenType.Number:
                await WriteValueAsync(writer, token.Value, "tok-number", inPreBlock: true);
                break;

            case CTokenType.Identifier:
                await WriteValueAsync(writer, token.Value, "tok-identifier", inPreBlock: true);
                break;

            case CTokenType.Keyword:
                await WriteValueAsync(writer, token.Value, "tok-keyword", inPreBlock: true);
                break;

            case CTokenType.Preprocessor:
                await WriteValueAsync(writer, token.Value, "tok-c-preprocessor", inPreBlock: true);
                break;

            case CTokenType.Comment:
                await WriteValueAsync(writer, token.Value, "tok-comment", inPreBlock: true);
                break;

            case CTokenType.Whitespace:
                await writer.WriteAsync(token.Value);
                break;

            default:
                await WriteValueAsync(writer, token.Value, null, inPreBlock: true);
                break;
        }
    }
}
