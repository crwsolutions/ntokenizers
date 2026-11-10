using NTokenizers.Typescript;

namespace NTokenizers.ToHtml.Writers;

/// <summary>
/// Writer for TypeScript/JavaScript tokens.
/// </summary>
internal sealed class TypeScriptHtmlWriter : AbstractTokenToHtmlWriter<TypescriptToken>
{
    internal override async Task WriteHtmlAsync(TypescriptToken token, TextWriter writer, CancellationToken ct = default)
    {
        switch (token.TokenType)
        {
            case TypescriptTokenType.NotDefined:
                await WriteValueAsync(writer, token.Value, null, inPreBlock: true);
                break;

            case TypescriptTokenType.And:
            case TypescriptTokenType.Or:
                await WriteValueAsync(writer, token.Value, "tok-keyword", inPreBlock: true);
                break;

            case TypescriptTokenType.Application:
            case TypescriptTokenType.ExceptionType:
            case TypescriptTokenType.Fingerprint:
            case TypescriptTokenType.Message:
            case TypescriptTokenType.StackFrame:
                await WriteValueAsync(writer, token.Value, "tok-identifier", inPreBlock: true);
                break;

            case TypescriptTokenType.Between:
            case TypescriptTokenType.In:
            case TypescriptTokenType.Like:
            case TypescriptTokenType.Limit:
            case TypescriptTokenType.Match:
                await WriteValueAsync(writer, token.Value, "tok-keyword", inPreBlock: true);
                break;

            case TypescriptTokenType.CloseParenthesis:
            case TypescriptTokenType.OpenParenthesis:
            case TypescriptTokenType.Comma:
            case TypescriptTokenType.SequenceTerminator:
            case TypescriptTokenType.Dot:
                await WriteValueAsync(writer, token.Value, "tok-punctuation", inPreBlock: true);
                break;

            case TypescriptTokenType.DateTimeValue:
                await WriteValueAsync(writer, token.Value, "tok-datetime", inPreBlock: true);
                break;

            case TypescriptTokenType.Equals:
            case TypescriptTokenType.NotEquals:
            case TypescriptTokenType.NotIn:
            case TypescriptTokenType.NotLike:
                await WriteValueAsync(writer, token.Value, "tok-operator", inPreBlock: true);
                break;

            case TypescriptTokenType.Invalid:
                await WriteValueAsync(writer, token.Value, null, inPreBlock: true);
                break;

            case TypescriptTokenType.Number:
                await WriteValueAsync(writer, token.Value, "tok-number", inPreBlock: true);
                break;

            case TypescriptTokenType.StringValue:
                await WriteValueAsync(writer, token.Value, "tok-string", inPreBlock: true);
                break;

            case TypescriptTokenType.Identifier:
                await WriteValueAsync(writer, token.Value, "tok-identifier", inPreBlock: true);
                break;

            case TypescriptTokenType.Keyword:
                await WriteValueAsync(writer, token.Value, "tok-keyword", inPreBlock: true);
                break;

            case TypescriptTokenType.Comment:
                await WriteValueAsync(writer, token.Value, "tok-comment", inPreBlock: true);
                break;

            case TypescriptTokenType.Operator:
                await WriteValueAsync(writer, token.Value, "tok-operator", inPreBlock: true);
                break;

            case TypescriptTokenType.Whitespace:
                await writer.WriteAsync(token.Value);
                break;

            default:
                await WriteValueAsync(writer, token.Value, null, inPreBlock: true);
                break;
        }
    }
}
