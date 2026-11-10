using NTokenizers.Xml;

namespace NTokenizers.ToHtml.Writers;

/// <summary>
/// Writer for XML tokens.
/// </summary>
internal sealed class XmlHtmlWriter : AbstractTokenToHtmlWriter<XmlToken>
{
    internal override async Task WriteHtmlAsync(XmlToken token, TextWriter writer, CancellationToken ct = default)
    {
        switch (token.TokenType)
        {
            case XmlTokenType.None:
                await WriteValueAsync(writer, token.Value, null, inPreBlock: true);
                break;

            case XmlTokenType.ElementName:
                await WriteValueAsync(writer, token.Value, "tok-ml-element", inPreBlock: true);
                break;

            case XmlTokenType.Text:
                await WriteValueAsync(writer, token.Value, null, inPreBlock: true);
                break;

            case XmlTokenType.Comment:
                await WriteValueAsync(writer, token.Value, "tok-comment", inPreBlock: true);
                break;

            case XmlTokenType.ProcessingInstruction:
                await WriteValueAsync(writer, token.Value, "tok-ml-processing", inPreBlock: true);
                break;

            case XmlTokenType.DocumentTypeDeclaration:
                await WriteValueAsync(writer, token.Value, "tok-ml-doctype", inPreBlock: true);
                break;

            case XmlTokenType.CData:
                await WriteValueAsync(writer, token.Value, "tok-ml-cdata", inPreBlock: true);
                break;

            case XmlTokenType.Whitespace:
                await writer.WriteAsync(token.Value);
                break;

            case XmlTokenType.OpeningAngleBracket:
            case XmlTokenType.ClosingAngleBracket:
            case XmlTokenType.SelfClosingSlash:
            case XmlTokenType.AttributeEquals:
            case XmlTokenType.AttributeQuote:
                await WriteValueAsync(writer, token.Value, "tok-punctuation", inPreBlock: true);
                break;

            case XmlTokenType.AttributeName:
                await WriteValueAsync(writer, token.Value, "tok-ml-attribute", inPreBlock: true);
                break;

            case XmlTokenType.AttributeValue:
                await WriteValueAsync(writer, token.Value, "tok-ml-attr-value", inPreBlock: true);
                break;

            default:
                await WriteValueAsync(writer, token.Value, null, inPreBlock: true);
                break;
        }
    }
}
