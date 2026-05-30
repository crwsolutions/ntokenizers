using NTokenizers.Java;
using Spectre.Console;

var javaCode = """
    import java.util.List;
    import java.util.stream.Collectors;

    public class HelloWorld {
        public static void main(String[] args) {
            System.out.println("Hello, World!");

            List<String> names = List.of("Alice", "Bob", "Charlie");
            names.stream()
                .filter(name -> name != null)
                .map(name -> name.toUpperCase())
                .collect(Collectors.toList())
                .forEach(System.out::println);
        }
    }
    """;

var tokenizer = JavaTokenizer.Create();
var tokens = tokenizer.Parse(javaCode).ToList();

foreach (var token in tokens)
{
    var value = Markup.Escape(token.Value);
    var colored = token.TokenType switch
    {
        JavaTokenType.Keyword => new Markup($"[blue]{value}[/]"),
        JavaTokenType.Identifier => new Markup($"[cyan]{value}[/]"),
        JavaTokenType.StringValue => new Markup($"[green]{value}[/]"),
        JavaTokenType.Number => new Markup($"[magenta]{value}[/]"),
        JavaTokenType.Operator => new Markup($"[yellow]{value}[/]"),
        JavaTokenType.Comment => new Markup($"[grey]{value}[/]"),
        JavaTokenType.Boolean => new Markup($"[magenta]{value}[/]"),
        JavaTokenType.Null => new Markup($"[magenta]{value}[/]"),
        JavaTokenType.Whitespace => new Markup($"[grey]{value}[/]"),
        _ => new Markup(value)
    };
    AnsiConsole.Write(colored);
}

Console.WriteLine();
Console.WriteLine("Done.");
