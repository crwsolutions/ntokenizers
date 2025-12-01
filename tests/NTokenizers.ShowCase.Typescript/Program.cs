using NTokenizers.Typescript;
using Spectre.Console;
using System.Text;

string typescript = """
    const user = { 
        name: "Laura Smith",
        active: true
    };
    """;

using var stream = new MemoryStream(Encoding.UTF8.GetBytes(typescript));
await TypescriptTokenizer.Create().ParseAsync(stream, onToken: token =>
{
    var value = Markup.Escape(token.Value);
    var colored = token.TokenType switch
    {
        TypescriptTokenType.Identifier => new Markup($"[cyan]{value}[/]"),
        TypescriptTokenType.Keyword => new Markup($"[blue]{value}[/]"),
        TypescriptTokenType.StringValue => new Markup($"[green]{value}[/]"),
        TypescriptTokenType.Number => new Markup($"[magenta]{value}[/]"),
        TypescriptTokenType.Operator => new Markup($"[yellow]{value}[/]"),
        TypescriptTokenType.Comment => new Markup($"[grey]{value}[/]"),
        TypescriptTokenType.Whitespace => new Markup($"[grey]{value}[/]"),
        _ => new Markup(value)
    };
    AnsiConsole.Write(colored);
});

Console.WriteLine();
Console.WriteLine("Done.");
