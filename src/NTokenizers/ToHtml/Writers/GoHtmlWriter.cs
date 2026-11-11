using NTokenizers.Go;

namespace NTokenizers.ToHtml.Writers;

/// <summary>
/// Writer for Go tokens.
/// </summary>
internal sealed class GoHtmlWriter : AbstractTokenToHtmlWriter<GoToken>
{
    internal override async Task WriteHtmlAsync(GoToken token, TextWriter writer, CancellationToken ct = default)
    {
        switch (token.TokenType)
        {
            case GoTokenType.NotDefined:
                await WriteValueAsync(writer, token.Value, null, inPreBlock: true);
                break;

            case GoTokenType.Operator:
                await WriteValueAsync(writer, token.Value, "tok-operator", inPreBlock: true);
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
                await WriteValueAsync(writer, token.Value, "tok-punctuation", inPreBlock: true);
                break;

            case GoTokenType.StringValue:
                await WriteValueAsync(writer, token.Value, "tok-string", inPreBlock: true);
                break;

            case GoTokenType.CharValue:
                await WriteValueAsync(writer, token.Value, "tok-char", inPreBlock: true);
                break;

            case GoTokenType.Number:
                await WriteValueAsync(writer, token.Value, "tok-number", inPreBlock: true);
                break;

            case GoTokenType.Boolean:
                await WriteValueAsync(writer, token.Value, "tok-boolean", inPreBlock: true);
                break;

            case GoTokenType.Null:
                await WriteValueAsync(writer, token.Value, "tok-null", inPreBlock: true);
                break;

            case GoTokenType.Identifier:
                await WriteValueAsync(writer, token.Value, "tok-identifier", inPreBlock: true);
                break;

            case GoTokenType.Keyword:
                await WriteValueAsync(writer, token.Value, "tok-keyword", inPreBlock: true);
                break;

            case GoTokenType.Comment:
                await WriteValueAsync(writer, token.Value, "tok-comment", inPreBlock: true);
                break;

            case GoTokenType.Whitespace:
                await writer.WriteAsync(token.Value);
                break;

            default:
                await WriteValueAsync(writer, token.Value, null, inPreBlock: true);
                break;
        }
    }
}
