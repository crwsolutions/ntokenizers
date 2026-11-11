using NTokenizers.Java;

namespace NTokenizers.ToHtml.Writers;

/// <summary>
/// Writer for Java tokens.
/// </summary>
internal sealed class JavaHtmlWriter : AbstractTokenToHtmlWriter<JavaToken>
{
    internal override async Task WriteHtmlAsync(JavaToken token, TextWriter writer, CancellationToken ct = default)
    {
        switch (token.TokenType)
        {
            case JavaTokenType.NotDefined:
                await WriteValueAsync(writer, token.Value, null, inPreBlock: true);
                break;

            case JavaTokenType.Operator:
                await WriteValueAsync(writer, token.Value, "tok-operator", inPreBlock: true);
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
                await WriteValueAsync(writer, token.Value, "tok-punctuation", inPreBlock: true);
                break;

            case JavaTokenType.StringValue:
                await WriteValueAsync(writer, token.Value, "tok-string", inPreBlock: true);
                break;

            case JavaTokenType.CharValue:
                await WriteValueAsync(writer, token.Value, "tok-char", inPreBlock: true);
                break;

            case JavaTokenType.Number:
                await WriteValueAsync(writer, token.Value, "tok-number", inPreBlock: true);
                break;

            case JavaTokenType.Boolean:
                await WriteValueAsync(writer, token.Value, "tok-boolean", inPreBlock: true);
                break;

            case JavaTokenType.Null:
                await WriteValueAsync(writer, token.Value, "tok-null", inPreBlock: true);
                break;

            case JavaTokenType.Identifier:
                await WriteValueAsync(writer, token.Value, "tok-identifier", inPreBlock: true);
                break;

            case JavaTokenType.Keyword:
                await WriteValueAsync(writer, token.Value, "tok-keyword", inPreBlock: true);
                break;

            case JavaTokenType.Comment:
                await WriteValueAsync(writer, token.Value, "tok-comment", inPreBlock: true);
                break;

            case JavaTokenType.Whitespace:
                await writer.WriteAsync(token.Value);
                break;

            default:
                await WriteValueAsync(writer, token.Value, null, inPreBlock: true);
                break;
        }
    }
}
