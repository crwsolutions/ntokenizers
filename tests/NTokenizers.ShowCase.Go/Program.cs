using NTokenizers.Go;
using Spectre.Console;

var goCode = """
    package main

    import (
        "fmt"
        "sort"
    )

    type Person struct {
        Name string
        Age  int
    }

    func sortByAge(people []Person) {
        sort.Slice(people, func(i, j int) bool {
            return people[i].Age < people[j].Age
        })
    }

    func main() {
        people := []Person{
            {"Alice", 30},
            {"Bob", 25},
            {"Charlie", 35},
        }

        sortByAge(people)

        for _, p := range people {
            fmt.Printf("%s is %d years old\n", p.Name, p.Age)
        }
    }
    """;

var tokenizer = GoTokenizer.Create();
var tokens = tokenizer.Parse(goCode).ToList();

foreach (var token in tokens)
{
    var value = Markup.Escape(token.Value);
    var colored = token.TokenType switch
    {
        GoTokenType.Keyword => new Markup($"[blue]{value}[/]"),
        GoTokenType.Identifier => new Markup($"[cyan]{value}[/]"),
        GoTokenType.StringValue => new Markup($"[green]{value}[/]"),
        GoTokenType.Number => new Markup($"[magenta]{value}[/]"),
        GoTokenType.Operator => new Markup($"[yellow]{value}[/]"),
        GoTokenType.Comment => new Markup($"[grey]{value}[/]"),
        GoTokenType.Boolean => new Markup($"[magenta]{value}[/]"),
        GoTokenType.Null => new Markup($"[magenta]{value}[/]"),
        GoTokenType.Whitespace => new Markup($"[grey]{value}[/]"),
        _ => new Markup(value)
    };
    AnsiConsole.Write(colored);
}

Console.WriteLine();
Console.WriteLine("Done.");
