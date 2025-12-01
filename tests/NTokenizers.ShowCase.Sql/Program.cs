using NTokenizers.Sql;
using Spectre.Console;
using System.Text;

string sql = """
    SELECT name, active FROM users
    WHERE id = 4821;
    """;

using var stream = new MemoryStream(Encoding.UTF8.GetBytes(sql));
await SqlTokenizer.Create().ParseAsync(stream, onToken: token =>
{
    var value = Markup.Escape(token.Value);
    var colored = token.TokenType switch
    {
        SqlTokenType.Keyword => new Markup($"[blue]{value}[/]"),
        SqlTokenType.Identifier => new Markup($"[cyan]{value}[/]"),
        SqlTokenType.StringValue => new Markup($"[green]{value}[/]"),
        SqlTokenType.Number => new Markup($"[magenta]{value}[/]"),
        SqlTokenType.Operator => new Markup($"[yellow]{value}[/]"),
        SqlTokenType.Comment => new Markup($"[grey]{value}[/]"),
        SqlTokenType.Whitespace => new Markup($"[grey]{value}[/]"),
        _ => new Markup(value)
    };
    AnsiConsole.Write(colored);
});

Console.WriteLine();
Console.WriteLine("Done.");
