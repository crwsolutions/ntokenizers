using NTokenizers.Swift;
using Spectre.Console;

var swiftCode = """
    import Foundation

    struct User {
        let id: Int
        var name: String
        var email: String

        init(id: Int, name: String, email: String) {
            self.id = id
            self.name = name
            self.email = email
        }

        func description() -> String {
            return "User(id: \\(id), name: \\(name))"
        }
    }

    extension User {
        static func from(id: Int, name: String, email: String) -> User {
            return User(id: id, name: name, email: email)
        }
    }

    var users: [User] = [
        .from(id: 1, name: "Alice", email: "alice@example.com"),
        .from(id: 2, name: "Bob", email: "bob@example.com"),
    ]

    for user in users {
        print(user.description())
    }
    """;

var tokenizer = SwiftTokenizer.Create();
var tokens = tokenizer.Parse(swiftCode).ToList();

foreach (var token in tokens)
{
    var value = Markup.Escape(token.Value);
    var colored = token.TokenType switch
    {
        SwiftTokenType.Keyword => new Markup($"[blue]{value}[/]"),
        SwiftTokenType.Identifier => new Markup($"[cyan]{value}[/]"),
        SwiftTokenType.StringValue => new Markup($"[green]{value}[/]"),
        SwiftTokenType.Number => new Markup($"[magenta]{value}[/]"),
        SwiftTokenType.Operator => new Markup($"[yellow]{value}[/]"),
        SwiftTokenType.Comment => new Markup($"[grey]{value}[/]"),
        SwiftTokenType.Boolean => new Markup($"[magenta]{value}[/]"),
        SwiftTokenType.Null => new Markup($"[magenta]{value}[/]"),
        SwiftTokenType.Whitespace => new Markup($"[grey]{value}[/]"),
        _ => new Markup(value)
    };
    AnsiConsole.Write(colored);
}

Console.WriteLine();
Console.WriteLine("Done.");
