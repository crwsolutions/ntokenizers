using NTokenizers.Xml;
using Spectre.Console;
using System.Text;

string xml = """
    <user id="4821" active="true">
        <name>Laura Smith</name>
    </user>
    """;

using var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
await XmlTokenizer.Create().ParseAsync(stream, onToken: token =>
{
    var value = Markup.Escape(token.Value);
    var colored = token.TokenType switch
    {
        XmlTokenType.ElementName => new Markup($"[blue]{value}[/]"),
        XmlTokenType.EndElement => new Markup($"[blue]{value}[/]"),
        XmlTokenType.OpeningAngleBracket => new Markup($"[yellow]{value}[/]"),
        XmlTokenType.ClosingAngleBracket => new Markup($"[yellow]{value}[/]"),
        XmlTokenType.SelfClosingSlash => new Markup($"[yellow]{value}[/]"),
        XmlTokenType.AttributeName => new Markup($"[cyan]{value}[/]"),
        XmlTokenType.AttributeEquals => new Markup($"[yellow]{value}[/]"),
        XmlTokenType.AttributeQuote => new Markup($"[grey]{value}[/]"),
        XmlTokenType.AttributeValue => new Markup($"[green]{value}[/]"),
        XmlTokenType.Text => new Markup($"[white]{value}[/]"),
        XmlTokenType.Whitespace => new Markup($"[grey]{value}[/]"),
        _ => new Markup(value)
    };
    AnsiConsole.Write(colored);
});

Console.WriteLine();
Console.WriteLine("Done.");
