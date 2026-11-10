using NTokenizers.Css;
using NTokenizers.Html;
using NTokenizers.Typescript;

namespace NTokenizers.ToHtml.Writers;

/// <summary>
/// Writer for HTML tokens. Delegates ScriptElement to TypeScriptHtmlWriter and StyleElement to CssHtmlWriter.
/// </summary>
internal sealed class HtmlHtmlWriter : BaseHtmlWriter
{
    internal async Task WriteHtmlAsync(HtmlToken token, TextWriter writer, CancellationToken ct = default)
    {
        switch (token.TokenType)
        {
            case HtmlTokenType.None:
                await WriteValueAsync(writer, token.Value, null, inPreBlock: true);
                break;

            case HtmlTokenType.ElementName:
                await WriteValueAsync(writer, token.Value, "tok-ml-element", inPreBlock: true);
                break;

            case HtmlTokenType.Text:
                await WriteValueAsync(writer, token.Value, null, inPreBlock: true);
                break;

            case HtmlTokenType.Comment:
                await WriteValueAsync(writer, token.Value, "tok-comment", inPreBlock: true);
                break;

            case HtmlTokenType.DocumentTypeDeclaration:
                await WriteValueAsync(writer, token.Value, "tok-ml-doctype", inPreBlock: true);
                break;

            case HtmlTokenType.Whitespace:
                await writer.WriteAsync(token.Value);
                break;

            case HtmlTokenType.OpeningAngleBracket:
            case HtmlTokenType.ClosingAngleBracket:
            case HtmlTokenType.SelfClosingSlash:
                await WriteValueAsync(writer, token.Value, "tok-punctuation", inPreBlock: true);
                break;

            case HtmlTokenType.AttributeName:
                await WriteValueAsync(writer, token.Value, "tok-ml-attribute", inPreBlock: true);
                break;

            case HtmlTokenType.AttributeEquals:
                await WriteValueAsync(writer, token.Value, "tok-punctuation", inPreBlock: true);
                break;

            case HtmlTokenType.AttributeValue:
                await WriteValueAsync(writer, token.Value, "tok-ml-attr-value", inPreBlock: true);
                break;

            case HtmlTokenType.AttributeQuote:
                await WriteValueAsync(writer, token.Value, "tok-punctuation", inPreBlock: true);
                break;

            case HtmlTokenType.ScriptElement:
                if (token.Metadata is TypeScriptCodeBlockMetadata tsMeta)
                {
                    var tsWriter = new TypeScriptHtmlWriter();
                    await tsMeta.ParseInlinesAsync(async t => await tsWriter.WriteHtmlAsync(t, writer, ct));
                }
                break;

            case HtmlTokenType.StyleElement:
                if (token.Metadata is CssCodeBlockMetadata cssMeta)
                {
                    var cssWriter = new CssHtmlWriter();
                    await cssMeta.ParseInlinesAsync(async t => await cssWriter.WriteHtmlAsync(t, writer, ct));
                }
                break;

            default:
                await WriteValueAsync(writer, token.Value, null, inPreBlock: true);
                break;
        }
    }
}
