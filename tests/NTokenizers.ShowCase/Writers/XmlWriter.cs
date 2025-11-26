using NTokenizers.Xml;
using Spectre.Console;

namespace NTokenizers.ShowCase.Writers;

internal sealed class XmlWriter : BaseInlineWriter<XmlToken, XmlTokenType>
{
    protected override Style GetStyle(XmlTokenType token) => token switch
    {
        XmlTokenType.ElementName => new Style(Color.DeepSkyBlue3_1),
        XmlTokenType.Text => new Style(Color.DarkSlateGray1),
        XmlTokenType.Comment => new Style(Color.Green),
        XmlTokenType.ProcessingInstruction => new Style(Color.Turquoise2),
        XmlTokenType.DocumentTypeDeclaration => new Style(Color.Turquoise2),
        XmlTokenType.CData => new Style(Color.Magenta1),
        XmlTokenType.Whitespace => new Style(Color.Yellow),
        XmlTokenType.EndElement => new Style(Color.DeepSkyBlue3_1),
        XmlTokenType.OpeningAngleBracket => new Style(Color.DeepSkyBlue4_2),
        XmlTokenType.ClosingAngleBracket => new Style(Color.DeepSkyBlue4_2),
        XmlTokenType.AttributeName => new Style(Color.Turquoise2),
        XmlTokenType.AttributeEquals => new Style(Color.DeepSkyBlue4_2),
        XmlTokenType.AttributeValue => new Style(Color.White),
        XmlTokenType.AttributeQuote => new Style(Color.DeepSkyBlue3_1),
        XmlTokenType.SelfClosingSlash => new Style(Color.DeepSkyBlue4_2),
        _ => new Style(),
    };
}