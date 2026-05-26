using NTokenizers.Toml;
using Spectre.Console;
using System.Text;

string toml = @"
# Comment over het bestand
title = ""TOML demo""
active = true
debug = false
name = 'literal string'
multiline = """"""multi
line string""""""
lit_multi = '''literal multi line'''
int_dec = 42
int_neg = -17
float = 3.14
float_exp = 1e6
float_exp_neg = -2.5e-3
hex = 0xDEADBEEF
oct = 0o755
bin = 0b101010
inf = inf
ninf = -inf
nan = nan
date = 2026-05-26
time = 13:45:30
datetime = 2026-05-26T13:45:30Z
datetime_offset = 2026-05-26T13:45:30+02:00
array = [1, 2, 3, ""four"", true, 2026-01-01T00:00:00Z]
point = {x = 10, y = 20}
a.b.c = ""dotted key""

[table]
key = ""value""

[[array_table]]
id = 1

[[array_table]]
id = 2
";

using var stream = new MemoryStream(Encoding.UTF8.GetBytes(toml));
await TomlTokenizer.Create().ParseAsync(stream, onToken: token =>
{
    var value = Markup.Escape(token.Value);
    var colored = token.TokenType switch
    {
        TomlTokenType.Comment => new Markup($"[dim]{value}[/]"),
        TomlTokenType.Identifier => new Markup($"[cyan]{value}[/]"),
        TomlTokenType.StringValue => new Markup($"[green]{value}[/]"),
        TomlTokenType.StringQuote => new Markup($"[green]{value}[/]"),
        TomlTokenType.Number => new Markup($"[magenta]{value}[/]"),
        TomlTokenType.Boolean => new Markup($"[orange1]{value}[/]"),
        TomlTokenType.DateTime => new Markup($"[blue]{value}[/]"),
        TomlTokenType.OpenBracket or TomlTokenType.CloseBracket => new Markup($"[yellow]{value}[/]"),
        TomlTokenType.OpenBrace or TomlTokenType.CloseBrace => new Markup($"[yellow]{value}[/]"),
        TomlTokenType.Equal => new Markup($"[grey]{value}[/]"),
        TomlTokenType.Comma => new Markup($"[grey]{value}[/]"),
        TomlTokenType.Dot => new Markup($"[grey]{value}[/]"),
        TomlTokenType.Whitespace => new Markup($"[dim]{value}[/]"),
        _ => new Markup(value)
    };
    AnsiConsole.Write(colored);
});

Console.WriteLine();
Console.WriteLine("Done.");
