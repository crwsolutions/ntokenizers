using NTokenizers.Swift;

namespace NTokenizers.ToHtml.Writers;

/// <summary>
/// Writer for Swift tokens.
/// </summary>
internal sealed class SwiftHtmlWriter : AbstractTokenToHtmlWriter<SwiftToken>
{
    internal override async Task WriteHtmlAsync(SwiftToken token, TextWriter writer, CancellationToken ct = default)
    {
        switch (token.TokenType)
        {
            case SwiftTokenType.NotDefined:
                await WriteValueAsync(writer, token.Value, null, inPreBlock: true);
                break;

            case SwiftTokenType.Operator:
                await WriteValueAsync(writer, token.Value, "tok-operator", inPreBlock: true);
                break;

            case SwiftTokenType.OpenParenthesis:
            case SwiftTokenType.CloseParenthesis:
            case SwiftTokenType.OpenBrace:
            case SwiftTokenType.CloseBrace:
            case SwiftTokenType.OpenBracket:
            case SwiftTokenType.CloseBracket:
            case SwiftTokenType.Comma:
            case SwiftTokenType.Dot:
            case SwiftTokenType.SequenceTerminator:
            case SwiftTokenType.Colon:
            case SwiftTokenType.DoubleColon:
            case SwiftTokenType.At:
            case SwiftTokenType.Pound:
            case SwiftTokenType.QuestionMark:
                await WriteValueAsync(writer, token.Value, "tok-punctuation", inPreBlock: true);
                break;

            case SwiftTokenType.StringValue:
                await WriteValueAsync(writer, token.Value, "tok-string", inPreBlock: true);
                break;

            case SwiftTokenType.CharValue:
                await WriteValueAsync(writer, token.Value, "tok-char", inPreBlock: true);
                break;

            case SwiftTokenType.Number:
                await WriteValueAsync(writer, token.Value, "tok-number", inPreBlock: true);
                break;

            case SwiftTokenType.Boolean:
                await WriteValueAsync(writer, token.Value, "tok-boolean", inPreBlock: true);
                break;

            case SwiftTokenType.Null:
                await WriteValueAsync(writer, token.Value, "tok-null", inPreBlock: true);
                break;

            case SwiftTokenType.Identifier:
                await WriteValueAsync(writer, token.Value, "tok-identifier", inPreBlock: true);
                break;

            case SwiftTokenType.Keyword:
                await WriteValueAsync(writer, token.Value, "tok-keyword", inPreBlock: true);
                break;

            case SwiftTokenType.Comment:
                await WriteValueAsync(writer, token.Value, "tok-comment", inPreBlock: true);
                break;

            case SwiftTokenType.Whitespace:
                await writer.WriteAsync(token.Value);
                break;

            default:
                await WriteValueAsync(writer, token.Value, null, inPreBlock: true);
                break;
        }
    }
}
