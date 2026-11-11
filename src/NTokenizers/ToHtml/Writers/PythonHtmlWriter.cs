using NTokenizers.Python;

namespace NTokenizers.ToHtml.Writers;

/// <summary>
/// Writer for Python tokens.
/// </summary>
internal sealed class PythonHtmlWriter : AbstractTokenToHtmlWriter<PythonToken>
{
    internal override async Task WriteHtmlAsync(PythonToken token, TextWriter writer, CancellationToken ct = default)
    {
        switch (token.TokenType)
        {
            case PythonTokenType.NotDefined:
                await WriteValueAsync(writer, token.Value, null, inPreBlock: true);
                break;

            case PythonTokenType.Operator:
                await WriteValueAsync(writer, token.Value, "tok-operator", inPreBlock: true);
                break;

            case PythonTokenType.OpenParenthesis:
            case PythonTokenType.CloseParenthesis:
            case PythonTokenType.OpenBrace:
            case PythonTokenType.CloseBrace:
            case PythonTokenType.OpenBracket:
            case PythonTokenType.CloseBracket:
            case PythonTokenType.Comma:
            case PythonTokenType.Dot:
            case PythonTokenType.Colon:
            case PythonTokenType.Semicolon:
            case PythonTokenType.Hash:
                await WriteValueAsync(writer, token.Value, "tok-punctuation", inPreBlock: true);
                break;

            case PythonTokenType.StringValue:
                await WriteValueAsync(writer, token.Value, "tok-string", inPreBlock: true);
                break;

            case PythonTokenType.Number:
                await WriteValueAsync(writer, token.Value, "tok-number", inPreBlock: true);
                break;

            case PythonTokenType.Identifier:
                await WriteValueAsync(writer, token.Value, "tok-identifier", inPreBlock: true);
                break;

            case PythonTokenType.Keyword:
                await WriteValueAsync(writer, token.Value, "tok-keyword", inPreBlock: true);
                break;

            case PythonTokenType.Comment:
                await WriteValueAsync(writer, token.Value, "tok-comment", inPreBlock: true);
                break;

            case PythonTokenType.Whitespace:
                await writer.WriteAsync(token.Value);
                break;

            default:
                await WriteValueAsync(writer, token.Value, null, inPreBlock: true);
                break;
        }
    }
}
