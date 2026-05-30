using NTokenizers.Python;
using Spectre.Console;

var pythonCode = """
    def greet(name: str) -> str:
        # Greet a person by name
        return f"Hello, {name}!"

    @decorator
    async def fetch_data(url: str) -> dict:
        # Fetch data from URL
        response = await http.get(url)
        return response.json()

    class Person:
        def __init__(self, name: str, age: int):
            self.name = name
            self.age = age

        def __str__(self) -> str:
            return f"{self.name} ({self.age})"

    # Usage
    people = [Person("Alice", 30), Person("Bob", 25)]
    for p in people:
        print(greet(p.name))
    """;

var tokenizer = PythonTokenizer.Create();
var tokens = tokenizer.Parse(pythonCode).ToList();

foreach (var token in tokens)
{
    var value = Markup.Escape(token.Value);
    var colored = token.TokenType switch
    {
        PythonTokenType.Keyword => new Markup($"[blue]{value}[/]"),
        PythonTokenType.Identifier => new Markup($"[cyan]{value}[/]"),
        PythonTokenType.StringValue => new Markup($"[white]{value}[/]"),
        PythonTokenType.Number => new Markup($"[magenta]{value}[/]"),
        PythonTokenType.Operator => new Markup($"[yellow]{value}[/]"),
        PythonTokenType.Comment => new Markup($"[green]{value}[/]"),
        PythonTokenType.Whitespace => new Markup($"[grey]{value}[/]"),
        _ => new Markup(value)
    };
    AnsiConsole.Write(colored);
}

Console.WriteLine();
Console.WriteLine("Done.");
