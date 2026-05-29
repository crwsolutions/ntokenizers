using NTokenizers.Kotlin;
using Spectre.Console;

var kotlinCode = """
    data class Config(
        val host: String,
        val port: Int,
        val enabled: Boolean = true
    ) {
        companion object {
            val DEFAULT = Config("localhost", 8080, true)
        }
    }

    fun main() {
        val config = Config("127.0.0.1", 3000, false)
        println("Host: ${config.host}")
        println("Port: ${config.port}")
        println("Enabled: ${config.enabled}")
    }
    """;

var tokenizer = KotlinTokenizer.Create();
var tokens = tokenizer.Parse(kotlinCode).ToList();

foreach (var token in tokens)
{
    var value = Markup.Escape(token.Value);
    var colored = token.TokenType switch
    {
        KotlinTokenType.Keyword => new Markup($"[blue]{value}[/]"),
        KotlinTokenType.Identifier => new Markup($"[cyan]{value}[/]"),
        KotlinTokenType.StringValue => new Markup($"[green]{value}[/]"),
        KotlinTokenType.Number => new Markup($"[magenta]{value}[/]"),
        KotlinTokenType.Operator => new Markup($"[yellow]{value}[/]"),
        KotlinTokenType.Comment => new Markup($"[grey]{value}[/]"),
        KotlinTokenType.Boolean => new Markup($"[magenta]{value}[/]"),
        KotlinTokenType.Null => new Markup($"[magenta]{value}[/]"),
        KotlinTokenType.Whitespace => new Markup($"[grey]{value}[/]"),
        _ => new Markup(value)
    };
    AnsiConsole.Write(colored);
}

Console.WriteLine();
Console.WriteLine("Done.");
