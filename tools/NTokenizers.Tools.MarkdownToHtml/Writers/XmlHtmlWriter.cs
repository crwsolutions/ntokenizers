using NTokenizers.Xml;

namespace NTokenizers.Tools.MarkdownToHtml.Writers;

/// <summary>
/// Writer for XML tokens.
/// </summary>
internal sealed class XmlHtmlWriter : AbstractTokenToHtmlWriter<XmlToken>
{
    internal override void WriteHtml(XmlToken token, TextWriter writer)
    {
        switch (token.TokenType)
        {
            case XmlTokenType.None:
                WriteValue(writer, token.Value, null, inPreBlock: true);
                break;

            case XmlTokenType.ElementName:
                WriteValue(writer, token.Value, "tok-ml-element", inPreBlock: true);
                break;

            case XmlTokenType.Text:
                WriteValue(writer, token.Value, null, inPreBlock: true);
                break;

            case XmlTokenType.Comment:
                WriteValue(writer, token.Value, "tok-comment", inPreBlock: true);
                break;

            case XmlTokenType.ProcessingInstruction:
                WriteValue(writer, token.Value, "tok-ml-processing", inPreBlock: true);
                break;

            case XmlTokenType.DocumentTypeDeclaration:
                WriteValue(writer, token.Value, "tok-ml-doctype", inPreBlock: true);
                break;

            case XmlTokenType.CData:
                WriteValue(writer, token.Value, "tok-ml-cdata", inPreBlock: true);
                break;

            case XmlTokenType.Whitespace:
                writer.Write(token.Value);
                break;

            case XmlTokenType.OpeningAngleBracket:
            case XmlTokenType.ClosingAngleBracket:
            case XmlTokenType.SelfClosingSlash:
            case XmlTokenType.AttributeEquals:
            case XmlTokenType.AttributeQuote:
                WriteValue(writer, token.Value, "tok-punctuation", inPreBlock: true);
                break;

            case XmlTokenType.AttributeName:
                WriteValue(writer, token.Value, "tok-ml-attribute", inPreBlock: true);
                break;

            case XmlTokenType.AttributeValue:
                WriteValue(writer, token.Value, "tok-ml-attr-value", inPreBlock: true);
                break;

            default:
                WriteValue(writer, token.Value, null, inPreBlock: true);
                break;
        }
    }
}
