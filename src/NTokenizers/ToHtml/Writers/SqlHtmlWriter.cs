using NTokenizers.Sql;

namespace NTokenizers.ToHtml.Writers;

/// <summary>
/// Writer for SQL tokens.
/// </summary>
internal sealed class SqlHtmlWriter : AbstractTokenToHtmlWriter<SqlToken>
{
    internal override async Task WriteHtmlAsync(SqlToken token, TextWriter writer, CancellationToken ct = default)
    {
        switch (token.TokenType)
        {
            case SqlTokenType.StringValue:
                await WriteValueAsync(writer, token.Value, "tok-string", inPreBlock: true);
                break;

            case SqlTokenType.Number:
                await WriteValueAsync(writer, token.Value, "tok-number", inPreBlock: true);
                break;

            case SqlTokenType.Keyword:
                await WriteValueAsync(writer, token.Value, "tok-keyword", inPreBlock: true);
                break;

            case SqlTokenType.Identifier:
                await WriteValueAsync(writer, token.Value, "tok-identifier", inPreBlock: true);
                break;

            case SqlTokenType.Operator:
                await WriteValueAsync(writer, token.Value, "tok-operator", inPreBlock: true);
                break;

            case SqlTokenType.Comma:
            case SqlTokenType.Dot:
            case SqlTokenType.OpenParenthesis:
            case SqlTokenType.CloseParenthesis:
            case SqlTokenType.SequenceTerminator:
                await WriteValueAsync(writer, token.Value, "tok-punctuation", inPreBlock: true);
                break;

            case SqlTokenType.NotDefined:
                await WriteValueAsync(writer, token.Value, null, inPreBlock: true);
                break;

            case SqlTokenType.Comment:
                await WriteValueAsync(writer, token.Value, "tok-comment", inPreBlock: true);
                break;

            case SqlTokenType.Whitespace:
                await writer.WriteAsync(token.Value);
                break;

            default:
                await WriteValueAsync(writer, token.Value, null, inPreBlock: true);
                break;
        }
    }
}
