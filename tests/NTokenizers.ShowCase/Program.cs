using NTokenizers.Json;
using Spectre.Console;
using System.IO.Pipes;
using System.Text;

class Program
{
    static async Task Main()
    {
        string json = """
        {
          "user": {
            "id": 4821,
            "name": "Laura Smith",
            "addresses": [
              {
                "type": "home",
                "street": "221B Baker Street",
                "city": "London",
                "postalCode": "NW1 6XE",
                "coordinates": { "lat": 51.5237, "lng": -0.1586 }
              },
              {
                "type": "office",
                "street": "18 King William Street",
                "city": "London",
                "postalCode": "EC4N 7BP",
                "floor": 5
              }
            ],
            "active": true
          }
        }
        """;

        // Create a pair of connected streams (writer -> reader)
        using var pipe = new AnonymousPipeServerStream(PipeDirection.Out);
        using var reader = new AnonymousPipeClientStream(PipeDirection.In, pipe.ClientSafePipeHandle);

        // Start the slow writer
        var writerTask = EmitSlowlyAsync(json, pipe);

        // Start parsing from the reader stream
        JsonTokenizer.Parse(reader, stopDelimiter: null, onToken: token =>
        {
            var value = Markup.Escape(token.Value);

            var coloredToken = token.TokenType switch
            {
                JsonTokenType.StartObject => new Markup($"[blue]{value}[/]"),
                JsonTokenType.EndObject => new Markup($"[blue]{value}[/]"),
                JsonTokenType.StartArray => new Markup($"[blue]{value}[/]"),
                JsonTokenType.EndArray => new Markup($"[blue]{value}[/]"),
                JsonTokenType.Colon => new Markup($"[yellow]{value}[/]"),
                JsonTokenType.Comma => new Markup($"[yellow]{value}[/]"),
                JsonTokenType.PropertyName => new Markup($"[cyan]{value}[/]"),
                JsonTokenType.StringValue => new Markup($"[green]{value}[/]"),
                JsonTokenType.Number => new Markup($"[magenta]{value}[/]"),
                JsonTokenType.True => new Markup($"[orange1]{value}[/]"),
                JsonTokenType.False => new Markup($"[orange1]{value}[/]"),
                JsonTokenType.Null => new Markup($"[orange1]{value}[/]"),
                JsonTokenType.Whitespace => new Markup($"[grey]{value}[/]"),
                _ => new Markup(token.Value ?? string.Empty)
            };

            AnsiConsole.Write(coloredToken);
        });

        await writerTask;

        Console.WriteLine();
        Console.WriteLine("Done.");
    }

    static async Task EmitSlowlyAsync(string json, Stream output)
    {
        var rng = new Random();

        byte[] bytes = Encoding.UTF8.GetBytes(json);

        foreach (var b in bytes)
        {
            await output.WriteAsync(new[] { b }.AsMemory(0, 1));
            await output.FlushAsync();

            await Task.Delay(rng.Next(10, 60));
        }
        output.Close(); // signal EOF
    }
}
