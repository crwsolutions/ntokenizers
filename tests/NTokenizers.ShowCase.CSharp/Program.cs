using NTokenizers.CSharp;
using Spectre.Console;
using System.Text;

string csharp = """
    var user = new { Name = "Laura Smith",
        Active = true };
    """;

using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csharp));
await CSharpTokenizer.Create().ParseAsync(stream, onToken: token =>
{
    var value = Markup.Escape(token.Value);
    var colored = token.TokenType switch
    {
        CSharpTokenType.Keyword => new Markup($"[blue]{value}[/]"),
        CSharpTokenType.Identifier => new Markup($"[cyan]{value}[/]"),
        CSharpTokenType.StringValue => new Markup($"[green]{value}[/]"),
        CSharpTokenType.Number => new Markup($"[magenta]{value}[/]"),
        CSharpTokenType.Operator => new Markup($"[yellow]{value}[/]"),
        CSharpTokenType.Comment => new Markup($"[grey]{value}[/]"),
        CSharpTokenType.Whitespace => new Markup($"[grey]{value}[/]"),
        _ => new Markup(value)
    };
    AnsiConsole.Write(colored);
});

Console.WriteLine();
Console.WriteLine("Done.");
