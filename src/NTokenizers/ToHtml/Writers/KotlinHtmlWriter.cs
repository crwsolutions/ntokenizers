using NTokenizers.Kotlin;

namespace NTokenizers.ToHtml.Writers;

/// <summary>
/// Writer for Kotlin tokens.
/// </summary>
internal sealed class KotlinHtmlWriter : AbstractTokenToHtmlWriter<KotlinToken>
{
    internal override async Task WriteHtmlAsync(KotlinToken token, TextWriter writer, CancellationToken ct = default)
    {
        switch (token.TokenType)
        {
            case KotlinTokenType.NotDefined:
                await WriteValueAsync(writer, token.Value, null, inPreBlock: true);
                break;

            case KotlinTokenType.Operator:
                await WriteValueAsync(writer, token.Value, "tok-operator", inPreBlock: true);
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
                await WriteValueAsync(writer, token.Value, "tok-punctuation", inPreBlock: true);
                break;

            case KotlinTokenType.StringValue:
                await WriteValueAsync(writer, token.Value, "tok-string", inPreBlock: true);
                break;

            case KotlinTokenType.CharValue:
                await WriteValueAsync(writer, token.Value, "tok-char", inPreBlock: true);
                break;

            case KotlinTokenType.Number:
                await WriteValueAsync(writer, token.Value, "tok-number", inPreBlock: true);
                break;

            case KotlinTokenType.Boolean:
                await WriteValueAsync(writer, token.Value, "tok-boolean", inPreBlock: true);
                break;

            case KotlinTokenType.Null:
                await WriteValueAsync(writer, token.Value, "tok-null", inPreBlock: true);
                break;

            case KotlinTokenType.Identifier:
                await WriteValueAsync(writer, token.Value, "tok-identifier", inPreBlock: true);
                break;

            case KotlinTokenType.Keyword:
                await WriteValueAsync(writer, token.Value, "tok-keyword", inPreBlock: true);
                break;

            case KotlinTokenType.Comment:
                await WriteValueAsync(writer, token.Value, "tok-comment", inPreBlock: true);
                break;

            case KotlinTokenType.Whitespace:
                await writer.WriteAsync(token.Value);
                break;

            default:
                await WriteValueAsync(writer, token.Value, null, inPreBlock: true);
                break;
        }
    }
}
