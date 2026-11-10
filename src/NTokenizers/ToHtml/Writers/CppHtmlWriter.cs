using NTokenizers.Cpp;

namespace NTokenizers.ToHtml.Writers;

/// <summary>
/// Writer for C++ tokens.
/// </summary>
internal sealed class CppHtmlWriter : AbstractTokenToHtmlWriter<CppToken>
{
    internal override async Task WriteHtmlAsync(CppToken token, TextWriter writer, CancellationToken ct = default)
    {
        switch (token.TokenType)
        {
            case CppTokenType.NotDefined:
                await WriteValueAsync(writer, token.Value, null, inPreBlock: true);
                break;

            case CppTokenType.Operator:
                await WriteValueAsync(writer, token.Value, "tok-operator", inPreBlock: true);
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
                await WriteValueAsync(writer, token.Value, "tok-punctuation", inPreBlock: true);
                break;

            case CppTokenType.StringValue:
                await WriteValueAsync(writer, token.Value, "tok-string", inPreBlock: true);
                break;

            case CppTokenType.CharValue:
                await WriteValueAsync(writer, token.Value, "tok-char", inPreBlock: true);
                break;

            case CppTokenType.Number:
                await WriteValueAsync(writer, token.Value, "tok-number", inPreBlock: true);
                break;

            case CppTokenType.Boolean:
                await WriteValueAsync(writer, token.Value, "tok-boolean", inPreBlock: true);
                break;

            case CppTokenType.Null:
                await WriteValueAsync(writer, token.Value, "tok-null", inPreBlock: true);
                break;

            case CppTokenType.Identifier:
                await WriteValueAsync(writer, token.Value, "tok-identifier", inPreBlock: true);
                break;

            case CppTokenType.Keyword:
                await WriteValueAsync(writer, token.Value, "tok-keyword", inPreBlock: true);
                break;

            case CppTokenType.Comment:
                await WriteValueAsync(writer, token.Value, "tok-comment", inPreBlock: true);
                break;

            case CppTokenType.Whitespace:
                await writer.WriteAsync(token.Value);
                break;

            default:
                await WriteValueAsync(writer, token.Value, null, inPreBlock: true);
                break;
        }
    }
}
