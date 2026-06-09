using NTokenizers.Toml;

namespace NTokenizers.Tools.MarkdownToHtml.Writers;

/// <summary>
/// Writer for TOML tokens.
/// </summary>
internal sealed class TomlHtmlWriter : AbstractTokenToHtmlWriter<TomlToken>
{
    internal override void WriteHtml(TomlToken token, TextWriter writer)
    {
        switch (token.TokenType)
        {
            case TomlTokenType.Whitespace:
                writer.Write(token.Value);
                break;

            case TomlTokenType.Comment:
                WriteValue(writer, token.Value, "tok-comment", inPreBlock: true);
                break;

            case TomlTokenType.Identifier:
                WriteValue(writer, token.Value, "tok-identifier", inPreBlock: true);
                break;

            case TomlTokenType.Dot:
            case TomlTokenType.Equal:
            case TomlTokenType.Comma:
            case TomlTokenType.OpenBracket:
            case TomlTokenType.CloseBracket:
            case TomlTokenType.OpenBrace:
            case TomlTokenType.CloseBrace:
                WriteValue(writer, token.Value, "tok-punctuation", inPreBlock: true);
                break;

            case TomlTokenType.StringQuote:
                WriteValue(writer, token.Value, "tok-punctuation", inPreBlock: true);
                break;

            case TomlTokenType.StringValue:
                WriteValue(writer, token.Value, "tok-string", inPreBlock: true);
                break;

            case TomlTokenType.Number:
                WriteValue(writer, token.Value, "tok-number", inPreBlock: true);
                break;

            case TomlTokenType.Boolean:
                WriteValue(writer, token.Value, "tok-boolean", inPreBlock: true);
                break;

            case TomlTokenType.DateTime:
                WriteValue(writer, token.Value, "tok-datetime", inPreBlock: true);
                break;

            default:
                WriteValue(writer, token.Value, null, inPreBlock: true);
                break;
        }
    }
}
