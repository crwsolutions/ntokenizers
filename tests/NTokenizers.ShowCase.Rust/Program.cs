using NTokenizers.Rust;
using Spectre.Console;

var rustCode = """
    use std::collections::HashMap;

    struct User {
        id: u32,
        name: String,
        email: String,
    }

    impl User {
        fn new(id: u32, name: &str, email: &str) -> Self {
            Self {
                id,
                name: name.to_string(),
                email: email.to_string(),
            }
        }

        fn display(&self) -> String {
            format!("User {}: {} ({})", self.id, self.name, self.email)
        }
    }

    fn main() {
        let users = vec![
            User::new(1, "Alice", "alice@example.com"),
            User::new(2, "Bob", "bob@example.com"),
        ];

        for user in &users {
            println!("{}", user.display());
        }
    }
    """;

var tokenizer = RustTokenizer.Create();
var tokens = tokenizer.Parse(rustCode).ToList();

foreach (var token in tokens)
{
    var value = Markup.Escape(token.Value);
    var colored = token.TokenType switch
    {
        RustTokenType.Keyword => new Markup($"[blue]{value}[/]"),
        RustTokenType.Identifier => new Markup($"[cyan]{value}[/]"),
        RustTokenType.StringValue => new Markup($"[green]{value}[/]"),
        RustTokenType.Number => new Markup($"[magenta]{value}[/]"),
        RustTokenType.Operator => new Markup($"[yellow]{value}[/]"),
        RustTokenType.Comment => new Markup($"[grey]{value}[/]"),
        RustTokenType.Boolean => new Markup($"[magenta]{value}[/]"),
        RustTokenType.Lifetime => new Markup($"[magenta]{value}[/]"),
        RustTokenType.Whitespace => new Markup($"[grey]{value}[/]"),
        _ => new Markup(value)
    };
    AnsiConsole.Write(colored);
}

Console.WriteLine();
Console.WriteLine("Done.");
