using NTokenizers.CSharp;

namespace NTokenizers.ToHtml.Writers;

/// <summary>
/// Writer for C# tokens.
/// </summary>
internal sealed class CSharpHtmlWriter : AbstractTokenToHtmlWriter<CSharpToken>
{
    internal override async Task WriteHtmlAsync(CSharpToken token, TextWriter writer, CancellationToken ct = default)
    {
        switch (token.TokenType)
        {
            case CSharpTokenType.NotDefined:
                await WriteValueAsync(writer, token.Value, null, inPreBlock: true);
                break;

            case CSharpTokenType.And:
            case CSharpTokenType.Or:
            case CSharpTokenType.Not:
                await WriteValueAsync(writer, token.Value, "tok-keyword", inPreBlock: true);
                break;

            case CSharpTokenType.Equals:
            case CSharpTokenType.NotEquals:
            case CSharpTokenType.GreaterThan:
            case CSharpTokenType.LessThan:
            case CSharpTokenType.GreaterThanOrEqual:
            case CSharpTokenType.LessThanOrEqual:
            case CSharpTokenType.Plus:
            case CSharpTokenType.Minus:
            case CSharpTokenType.Multiply:
            case CSharpTokenType.Divide:
            case CSharpTokenType.Modulo:
                await WriteValueAsync(writer, token.Value, "tok-operator", inPreBlock: true);
                break;

            case CSharpTokenType.OpenParenthesis:
            case CSharpTokenType.CloseParenthesis:
            case CSharpTokenType.Comma:
            case CSharpTokenType.Dot:
            case CSharpTokenType.SequenceTerminator:
                await WriteValueAsync(writer, token.Value, "tok-punctuation", inPreBlock: true);
                break;

            case CSharpTokenType.StringValue:
                await WriteValueAsync(writer, token.Value, "tok-string", inPreBlock: true);
                break;

            case CSharpTokenType.Number:
                await WriteValueAsync(writer, token.Value, "tok-number", inPreBlock: true);
                break;

            case CSharpTokenType.Identifier:
                await WriteValueAsync(writer, token.Value, "tok-identifier", inPreBlock: true);
                break;

            case CSharpTokenType.Keyword:
                await WriteValueAsync(writer, token.Value, "tok-keyword", inPreBlock: true);
                break;

            case CSharpTokenType.Comment:
                await WriteValueAsync(writer, token.Value, "tok-comment", inPreBlock: true);
                break;

            case CSharpTokenType.Operator:
                await WriteValueAsync(writer, token.Value, "tok-operator", inPreBlock: true);
                break;

            case CSharpTokenType.Whitespace:
                await writer.WriteAsync(token.Value);
                break;

            default:
                await WriteValueAsync(writer, token.Value, null, inPreBlock: true);
                break;
        }
    }
}
