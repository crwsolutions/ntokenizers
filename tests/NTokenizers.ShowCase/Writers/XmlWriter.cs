using NTokenizers.Markup;
using NTokenizers.Xml;
using Spectre.Console;

internal static class XmlWriter
{
    internal static void Write(XmlCodeBlockMetadata xmlMeta)
    {
        AnsiConsole.WriteLine($"{xmlMeta.Language}:");
        xmlMeta.OnInlineToken = inlineToken =>
        {
            var inlineValue = Markup.Escape(inlineToken.Value);
            var inlineColored = inlineToken.TokenType switch
            {
                XmlTokenType.ElementName => new Markup($"[deepskyblue3_1]{inlineValue}[/]"),
                XmlTokenType.Text => new Markup($"[darkslategray1]{inlineValue}[/]"),
                XmlTokenType.Comment => new Markup($"[green]{inlineValue}[/]"),
                XmlTokenType.ProcessingInstruction => new Markup($"[turquoise2]{inlineValue}[/]"),
                XmlTokenType.DocumentTypeDeclaration => new Markup($"[turquoise2]{inlineValue}[/]"),
                XmlTokenType.CData => new Markup($"[magenta1]{inlineValue}[/]"),
                XmlTokenType.Whitespace => new Markup($"[yellow]{inlineValue}[/]"),
                XmlTokenType.EndElement => new Markup($"[deepskyblue3_1]{inlineValue}[/]"),
                XmlTokenType.OpeningAngleBracket => new Markup($"[deepskyblue4_2]{inlineValue}[/]"),
                XmlTokenType.ClosingAngleBracket => new Markup($"[deepskyblue4_2]{inlineValue}[/]"),
                XmlTokenType.AttributeName => new Markup($"[turquoise2]{inlineValue}[/]"),
                XmlTokenType.AttributeEquals => new Markup($"[deepskyblue4_2]{inlineValue}[/]"),
                XmlTokenType.AttributeValue => new Markup($"[white]{inlineValue}[/]"),
                XmlTokenType.AttributeQuote => new Markup($"[deepskyblue3_1]{inlineValue}[/]"),
                XmlTokenType.SelfClosingSlash => new Markup($"[deepskyblue4_2]{inlineValue}[/]"),
                _ => new Markup(inlineValue)
            };
            AnsiConsole.Write(inlineColored);
        };
    }
}