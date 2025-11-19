//using NTokenizers.Json;
//using Spectre.Console;
//using System.IO.Pipes;
//using System.Text;

//class Program
//{
//    static async Task Main()
//    {
//        string json = """
//        {
//          "user": {
//            "id": 4821,
//            "name": "Laura Smith",
//            "addresses": [
//              {
//                "type": "home",
//                "street": "221B Baker Street",
//                "city": "London",
//                "postalCode": "NW1 6XE",
//                "coordinates": { "lat": 51.5237, "lng": -0.1586 }
//              },
//              {
//                "type": "office",
//                "street": "18 King William Street",
//                "city": "London",
//                "postalCode": "EC4N 7BP",
//                "floor": 5
//              }
//            ],
//            "active": true
//          }
//        }
//        """;

//        // Create a pair of connected streams (writer -> reader)
//        using var pipe = new AnonymousPipeServerStream(PipeDirection.Out);
//        using var reader = new AnonymousPipeClientStream(PipeDirection.In, pipe.ClientSafePipeHandle);

//        // Start the slow writer
//        var writerTask = EmitSlowlyAsync(json, pipe);

//        // Start parsing from the reader stream
//        JsonTokenizer.Parse(reader, stopDelimiter: null, onToken: token =>
//        {
//            var value = Markup.Escape(token.Value);

//            var coloredToken = token.TokenType switch
//            {
//                JsonTokenType.StartObject => new Markup($"[blue]{value}[/]"),
//                JsonTokenType.EndObject => new Markup($"[blue]{value}[/]"),
//                JsonTokenType.StartArray => new Markup($"[blue]{value}[/]"),
//                JsonTokenType.EndArray => new Markup($"[blue]{value}[/]"),
//                JsonTokenType.Colon => new Markup($"[yellow]{value}[/]"),
//                JsonTokenType.Comma => new Markup($"[yellow]{value}[/]"),
//                JsonTokenType.PropertyName => new Markup($"[cyan]{value}[/]"),
//                JsonTokenType.StringValue => new Markup($"[green]{value}[/]"),
//                JsonTokenType.Number => new Markup($"[magenta]{value}[/]"),
//                JsonTokenType.True => new Markup($"[orange1]{value}[/]"),
//                JsonTokenType.False => new Markup($"[orange1]{value}[/]"),
//                JsonTokenType.Null => new Markup($"[orange1]{value}[/]"),
//                JsonTokenType.Whitespace => new Markup($"[grey]{value}[/]"),
//                _ => new Markup(token.Value ?? string.Empty)
//            };

//            AnsiConsole.Write(coloredToken);
//        });

//        await writerTask;

//        Console.WriteLine();
//        Console.WriteLine("Done.");
//    }

//    static async Task EmitSlowlyAsync(string json, Stream output)
//    {
//        var rng = new Random();

//        byte[] bytes = Encoding.UTF8.GetBytes(json);

//        foreach (var b in bytes)
//        {
//            await output.WriteAsync(new[] { b }.AsMemory(0, 1));
//            await output.FlushAsync();

//            await Task.Delay(rng.Next(10, 60));
//        }
//        output.Close(); // signal EOF
//    }
//}

//using NTokenizers.Xml;  // <--- XML tokenizer
//using Spectre.Console;
//using System.IO.Pipes;
//using System.Text;

//class Program
//{
//    static async Task Main()
//    {
//        string xml = """
//        <?xml version="1.0" encoding="utf-8"?>
//        <user id="4821" active="true">
//            <name>Laura Smith</name>
//            <addresses>
//                <address type="home">
//                    <street>221B Baker Street</street>
//                    <city>London</city>
//                    <postalCode>NW1 6XE</postalCode>
//                    <coordinates lat="51.5237" lng="-0.1586" />
//                </address>
//                <address type="office" floor="5">
//                    <street>18 King William Street</street>
//                    <city>London</city>
//                    <postalCode>EC4N 7BP</postalCode>
//                </address>
//            </addresses>
//        </user>
//        """;

//        // Create connected streams
//        using var pipe = new AnonymousPipeServerStream(PipeDirection.Out);
//        using var reader = new AnonymousPipeClientStream(PipeDirection.In, pipe.ClientSafePipeHandle);

//        // Start slow writer
//        var writerTask = EmitSlowlyAsync(xml, pipe);

//        // Start parsing XML
//        XmlTokenizer.Parse(reader, null,onToken: token =>
//        {
//            var value = Markup.Escape(token.Value);

//            var colored = token.TokenType switch
//            {
//                XmlTokenType.ElementName => new Markup($"[blue]{value}[/]"),
//                XmlTokenType.EndElement => new Markup($"[blue]{value}[/]"),

//                XmlTokenType.OpeningAngleBracket => new Markup($"[yellow]{value}[/]"),
//                XmlTokenType.ClosingAngleBracket => new Markup($"[yellow]{value}[/]"),
//                XmlTokenType.SelfClosingSlash => new Markup($"[yellow]{value}[/]"),

//                XmlTokenType.AttributeName => new Markup($"[cyan]{value}[/]"),
//                XmlTokenType.AttributeEquals => new Markup($"[yellow]{value}[/]"),
//                XmlTokenType.AttributeQuote => new Markup($"[grey]{value}[/]"),
//                XmlTokenType.AttributeValue => new Markup($"[green]{value}[/]"),

//                XmlTokenType.Text => new Markup($"[white]{value}[/]"),
//                XmlTokenType.Whitespace => new Markup($"[grey]{value}[/]"),

//                XmlTokenType.Comment => new Markup($"[grey]{value}[/]"),
//                XmlTokenType.CData => new Markup($"[magenta]{value}[/]"),
//                XmlTokenType.DocumentTypeDeclaration => new Markup($"[orange1]{value}[/]"),
//                XmlTokenType.ProcessingInstruction => new Markup($"[orange1]{value}[/]"),

//                _ => new Markup(value)
//            };

//            AnsiConsole.Write(colored);
//        });

//        await writerTask;

//        Console.WriteLine();
//        Console.WriteLine("Done.");
//    }

//    static async Task EmitSlowlyAsync(string xml, Stream output)
//    {
//        var rng = new Random();
//        byte[] bytes = Encoding.UTF8.GetBytes(xml);

//        foreach (var b in bytes)
//        {
//            await output.WriteAsync(new[] { b }.AsMemory(0, 1));
//            await output.FlushAsync();
//            await Task.Delay(rng.Next(10, 60));
//        }

//        output.Close(); // EOF
//    }
//}

using NTokenizers.Sql;   // <--- SQL tokenizer
using Spectre.Console;
using System.IO.Pipes;
using System.Text;

class Program
{
    static async Task Main()
    {
        string sql = """
        SELECT u.id, u.name, a.city, a.postal_code
        FROM users u
        INNER JOIN addresses a ON a.user_id = u.id
        WHERE u.active = TRUE
        ORDER BY u.name ASC;

        -- Fetch user with address
        SELECT *
        FROM users
        WHERE id = 42;
        """;

        // Connected streams
        using var pipe = new AnonymousPipeServerStream(PipeDirection.Out);
        using var reader = new AnonymousPipeClientStream(PipeDirection.In, pipe.ClientSafePipeHandle);

        // Start slow writer
        var writerTask = EmitSlowlyAsync(sql, pipe);

        // Parse SQL stream
        SqlTokenizer.Parse(reader, null, onToken: token =>
        {
            var value = Markup.Escape(token.Value);

            var colored = token.TokenType switch
            {
                SqlTokenType.Keyword => new Markup($"[yellow]{value}[/]"),
                SqlTokenType.Identifier => new Markup($"[blue]{value}[/]"),
                SqlTokenType.StringValue => new Markup($"[green]{value}[/]"),
                SqlTokenType.Number => new Markup($"[green]{value}[/]"),
                SqlTokenType.Operator => new Markup($"[red]{value}[/]"),

                SqlTokenType.Comma => new Markup($"[grey]{value}[/]"),
                SqlTokenType.Dot => new Markup($"[grey]{value}[/]"),

                SqlTokenType.OpenParenthesis => new Markup($"[cyan]{value}[/]"),
                SqlTokenType.CloseParenthesis => new Markup($"[cyan]{value}[/]"),

                SqlTokenType.SequenceTerminator => new Markup($"[yellow]{value}[/]"),

                SqlTokenType.Comment => new Markup($"[grey]{value}[/]"),

                SqlTokenType.NotDefined => new Markup($"[white]{value}[/]"),
                _ => new Markup(value)
            };

            AnsiConsole.Write(colored);
        });

        await writerTask;

        Console.WriteLine();
        Console.WriteLine("Done.");
    }

    static async Task EmitSlowlyAsync(string sql, Stream output)
    {
        var rng = new Random();
        byte[] bytes = Encoding.UTF8.GetBytes(sql);

        foreach (var b in bytes)
        {
            await output.WriteAsync(new[] { b }.AsMemory(0, 1));
            await output.FlushAsync();
            await Task.Delay(rng.Next(10, 60));
        }

        output.Close(); // EOF
    }
}
