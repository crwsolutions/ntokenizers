using NTokenizers.Css;
using NTokenizers.Html;
using NTokenizers.Typescript;

namespace NTokenizers.ToHtml.Writers;

/// <summary>
/// Writer for HTML tokens. Delegates ScriptElement to TypeScriptHtmlWriter and StyleElement to CssHtmlWriter.
/// </summary>
internal sealed class HtmlHtmlWriter : BaseHtmlWriter
{
    internal void WriteHtml(HtmlToken token, TextWriter writer)
    {
        switch (token.TokenType)
        {
            case HtmlTokenType.None:
                WriteValue(writer, token.Value, null, inPreBlock: true);
                break;

            case HtmlTokenType.ElementName:
                WriteValue(writer, token.Value, "tok-ml-element", inPreBlock: true);
                break;

            case HtmlTokenType.Text:
                WriteValue(writer, token.Value, null, inPreBlock: true);
                break;

            case HtmlTokenType.Comment:
                WriteValue(writer, token.Value, "tok-comment", inPreBlock: true);
                break;

            case HtmlTokenType.DocumentTypeDeclaration:
                WriteValue(writer, token.Value, "tok-ml-doctype", inPreBlock: true);
                break;

            case HtmlTokenType.Whitespace:
                writer.Write(token.Value);
                break;

            case HtmlTokenType.OpeningAngleBracket:
            case HtmlTokenType.ClosingAngleBracket:
            case HtmlTokenType.SelfClosingSlash:
                WriteValue(writer, token.Value, "tok-punctuation", inPreBlock: true);
                break;

            case HtmlTokenType.AttributeName:
                WriteValue(writer, token.Value, "tok-ml-attribute", inPreBlock: true);
                break;

            case HtmlTokenType.AttributeEquals:
                WriteValue(writer, token.Value, "tok-punctuation", inPreBlock: true);
                break;

            case HtmlTokenType.AttributeValue:
                WriteValue(writer, token.Value, "tok-ml-attr-value", inPreBlock: true);
                break;

            case HtmlTokenType.AttributeQuote:
                WriteValue(writer, token.Value, "tok-punctuation", inPreBlock: true);
                break;

            case HtmlTokenType.ScriptElement:
                if (token.Metadata is TypeScriptCodeBlockMetadata tsMeta)
                {
                    var tsWriter = new TypeScriptHtmlWriter();
                    _ = tsMeta.RegisterInlineTokenHandler(t => tsWriter.WriteHtml(t, writer));
                }
                break;

            case HtmlTokenType.StyleElement:
                if (token.Metadata is CssCodeBlockMetadata cssMeta)
                {
                    var cssWriter = new CssHtmlWriter();
                    _ = cssMeta.RegisterInlineTokenHandler(t => cssWriter.WriteHtml(t, writer));
                }
                break;

            default:
                WriteValue(writer, token.Value, null, inPreBlock: true);
                break;
        }
    }
}
