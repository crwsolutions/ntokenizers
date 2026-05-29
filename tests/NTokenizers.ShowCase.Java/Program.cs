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

Console.WriteLine("=== Java Tokenizer Showcase ===");
Console.WriteLine();

foreach (var token in tokens)
{
    switch (token.TokenType)
    {
        case JavaTokenType.Keyword:
            Console.Write($"{token.Value,-20}", ConsoleColor.Red);
            break;
        case JavaTokenType.StringValue:
            Console.Write($"{token.Value,-20}", ConsoleColor.Green);
            break;
        case JavaTokenType.Number:
            Console.Write($"{token.Value,-20}", ConsoleColor.Cyan);
            break;
        case JavaTokenType.Comment:
            Console.Write($"{token.Value,-20}", ConsoleColor.DarkGray);
            break;
        case JavaTokenType.Operator:
            Console.Write($"{token.Value,-20}", ConsoleColor.Yellow);
            break;
        case JavaTokenType.Identifier:
            Console.Write($"{token.Value,-20}", ConsoleColor.White);
            break;
        default:
            Console.Write($"{token.Value,-20}");
            break;
    }
}

Console.WriteLine();
Console.WriteLine();
Console.WriteLine($"Total tokens: {tokens.Count}");
