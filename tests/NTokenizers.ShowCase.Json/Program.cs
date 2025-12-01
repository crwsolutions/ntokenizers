using NTokenizers.Json;
using Spectre.Console;
using System.Text;

string json = """
        { 
            "name": "Laura Smith",
            "active": true 
        }
        """;

using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
await JsonTokenizer.Create().ParseAsync(stream, onToken: token =>
{
    var value = Markup.Escape(token.Value);
    var colored = token.TokenType switch
    {
        JsonTokenType.StartObject => new Markup($"[yellow]{value}[/]"),
        JsonTokenType.EndObject => new Markup($"[yellow]{value}[/]"),
        JsonTokenType.StartArray => new Markup($"[yellow]{value}[/]"),
        JsonTokenType.EndArray => new Markup($"[yellow]{value}[/]"),
        JsonTokenType.PropertyName => new Markup($"[cyan]{value}[/]"),
        JsonTokenType.StringValue => new Markup($"[green]{value}[/]"),
        JsonTokenType.Number => new Markup($"[magenta]{value}[/]"),
        JsonTokenType.True => new Markup($"[orange1]{value}[/]"),
        JsonTokenType.False => new Markup($"[orange1]{value}[/]"),
        JsonTokenType.Null => new Markup($"[grey]{value}[/]"),
        JsonTokenType.Colon => new Markup($"[yellow]{value}[/]"),
        JsonTokenType.Comma => new Markup($"[yellow]{value}[/]"),
        JsonTokenType.Whitespace => new Markup($"[grey]{value}[/]"),
        _ => new Markup(value)
    };
    AnsiConsole.Write(colored);
});

Console.WriteLine();
Console.WriteLine("Done.");
