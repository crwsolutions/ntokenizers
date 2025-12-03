using NTokenizers.Json;
using NTokenizers.Markup;
using NTokenizers.Markup.Metadata;
using NTokenizers.Typescript;
using NTokenizers.Xml;
using Spectre.Console;
using System.Diagnostics;
using System.IO.Pipes;
using System.Text;

class Program
{
    static async Task Main()
    {
        string markup = """
        Here is some **bold** text and some *italic* text.

        # NTokenizers Showcase
        ```json
        {
            "name": "Laura Smith",
            "active": true
        }
        ```

        **Pretty cool, huh?**
        """;

        // Create connected streams
        using var pipe = new AnonymousPipeServerStream(PipeDirection.Out);
        using var reader = new AnonymousPipeClientStream(PipeDirection.In, pipe.ClientSafePipeHandle);

        // Start slow writer
        var writerTask = EmitSlowlyAsync(markup, pipe);

        // Parse markup
        await MarkupTokenizer.Create().ParseAsync(reader, onToken: async token =>
        {
            if (token.Metadata is HeadingMetadata headingMetadata)
            {
                await headingMetadata.RegisterInlineTokenHandler( inlineToken =>
                {
                    var value = Markup.Escape(inlineToken.Value);
                    var colored = headingMetadata.Level != 1 ?
                        new Markup($"[bold GreenYellow]{value}[/]") :
                        new Markup($"[bold yellow]** {value} **[/]");
                    AnsiConsole.Write(colored);
                });
                Debug.WriteLine("Written Heading inlines");
            }
            else if (token.Metadata is JsonCodeBlockMetadata jsonMetadata)
            {
                Console.WriteLine($"code: {jsonMetadata.Language}");
                await jsonMetadata.RegisterInlineTokenHandler( inlineToken =>
                {
                    var value = Markup.Escape(inlineToken.Value);
                    var colored = inlineToken.TokenType switch
                    {
                        JsonTokenType.StartObject => new Markup($"[yellow]{value}[/]"),
                        JsonTokenType.EndObject => new Markup($"[yellow]{value}[/]"),
                        JsonTokenType.StartArray => new Markup($"[yellow]{value}[/]"),
                        JsonTokenType.EndArray => new Markup($"[yellow]{value}[/]"),
                        JsonTokenType.PropertyName => new Markup($"[cyan]{value}[/]"),
                        JsonTokenType.StringValue => new Markup($"[green]{value}[/]"),
                        JsonTokenType.Number => new Markup($"[magenta]{value}[/]"),
                        JsonTokenType.True => new Markup($"[orange1]{value}[/]"),
                        JsonTokenType.False => new Markup($"[orange1]{value}[/]"),
                        JsonTokenType.Null => new Markup($"[grey]{value}[/]"),
                        JsonTokenType.Colon => new Markup($"[yellow]{value}[/]"),
                        JsonTokenType.Comma => new Markup($"[yellow]{value}[/]"),
                        JsonTokenType.Whitespace => new Markup($"[grey]{value}[/]"),
                        _ => new Markup(value)
                    };
                    AnsiConsole.Write(colored);
                });
                AnsiConsole.WriteLine();
            }
            else
            {
                // Handle regular markup tokens
                var value = Markup.Escape(token.Value);
                var colored = token.TokenType switch
                {
                    MarkupTokenType.Text => new Markup($"{value}"),
                    MarkupTokenType.Bold => new Markup($"[bold]{value}[/]"),
                    MarkupTokenType.Italic => new Markup($"[italic]{value}[/]"),
                    _ => new Markup(value)
                };

                AnsiConsole.Write(colored);
            }
        });

        await writerTask;
    }

    static async Task EmitSlowlyAsync(string markup, Stream output)
    {
        var rng = new Random();
        byte[] bytes = Encoding.UTF8.GetBytes(markup);

        foreach (var b in bytes)
        {
            await output.WriteAsync(new[] { b }.AsMemory(0, 1));
            await output.FlushAsync();
            await Task.Delay(rng.Next(2, 8));
        }

        output.Close(); // EOF
    }
}