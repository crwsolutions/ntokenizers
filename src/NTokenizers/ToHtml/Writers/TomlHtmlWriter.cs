using NTokenizers.Toml;

namespace NTokenizers.ToHtml.Writers;

/// <summary>
/// Writer for TOML tokens.
/// </summary>
internal sealed class TomlHtmlWriter : AbstractTokenToHtmlWriter<TomlToken>
{
    internal override async Task WriteHtmlAsync(TomlToken token, TextWriter writer, CancellationToken ct = default)
    {
        switch (token.TokenType)
        {
            case TomlTokenType.Whitespace:
                await writer.WriteAsync(token.Value);
                break;

            case TomlTokenType.Comment:
                await WriteValueAsync(writer, token.Value, "tok-comment", inPreBlock: true);
                break;

            case TomlTokenType.Identifier:
                await WriteValueAsync(writer, token.Value, "tok-identifier", inPreBlock: true);
                break;

            case TomlTokenType.Dot:
            case TomlTokenType.Equal:
            case TomlTokenType.Comma:
            case TomlTokenType.OpenBracket:
            case TomlTokenType.CloseBracket:
            case TomlTokenType.OpenBrace:
            case TomlTokenType.CloseBrace:
                await WriteValueAsync(writer, token.Value, "tok-punctuation", inPreBlock: true);
                break;

            case TomlTokenType.StringQuote:
                await WriteValueAsync(writer, token.Value, "tok-punctuation", inPreBlock: true);
                break;

            case TomlTokenType.StringValue:
                await WriteValueAsync(writer, token.Value, "tok-string", inPreBlock: true);
                break;

            case TomlTokenType.Number:
                await WriteValueAsync(writer, token.Value, "tok-number", inPreBlock: true);
                break;

            case TomlTokenType.Boolean:
                await WriteValueAsync(writer, token.Value, "tok-boolean", inPreBlock: true);
                break;

            case TomlTokenType.DateTime:
                await WriteValueAsync(writer, token.Value, "tok-datetime", inPreBlock: true);
                break;

            default:
                await WriteValueAsync(writer, token.Value, null, inPreBlock: true);
                break;
        }
    }
}
