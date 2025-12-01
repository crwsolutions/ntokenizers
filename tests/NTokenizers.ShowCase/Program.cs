using NTokenizers.Json;
using NTokenizers.Markup;
using NTokenizers.Markup.Metadata;
using NTokenizers.Typescript;
using NTokenizers.Xml;
using Spectre.Console;
using System.IO.Pipes;
using System.Text;

class Program
{
    static async Task Main()
    {
        string markup = """
        # NTokenizers Showcase

        ## XML example
        ```xml
        <user id="4821" active="true">
            <name>Laura Smith</name>
        </user>
        ```

        ## JSON example
        ```json
        {
            "name": "Laura Smith",
            "active": true
        }
        ```

        ## TypeScript example
        ```typescript
        const user = {
            name: "Laura Smith",
            active: true
        };
        ```
        """;

        // Create connected streams
        using var pipe = new AnonymousPipeServerStream(PipeDirection.Out);
        using var reader = new AnonymousPipeClientStream(PipeDirection.In, pipe.ClientSafePipeHandle);

        // Start slow writer
        var writerTask = EmitSlowlyAsync(markup, pipe);

        // Parse markup
        MarkupTokenizer.Create().Parse(reader, onToken: async token =>
        {
            if (token.Metadata is HeadingMetadata headingMetadata)
            {
                await headingMetadata.RegisterInlineTokenHandler( inlineToken =>
                {
                    var value = Markup.Escape(inlineToken.Value);
                    var colored = headingMetadata.Level != 1 ?
                        new Markup($"[bold blue]{value}[/]") :
                        new Markup($"[bold yellow]** {value} **[/]");
                    AnsiConsole.Write(colored);
                });
            }
            else if (token.Metadata is XmlCodeBlockMetadata xmlMetadata)
            {
                await xmlMetadata.RegisterInlineTokenHandler(inlineToken =>
                {
                    var value = Markup.Escape(inlineToken.Value);
                    var colored = inlineToken.TokenType switch
                    {
                        XmlTokenType.ElementName => new Markup($"[blue]{value}[/]"),
                        XmlTokenType.EndElement => new Markup($"[blue]{value}[/]"),
                        XmlTokenType.OpeningAngleBracket => new Markup($"[yellow]{value}[/]"),
                        XmlTokenType.ClosingAngleBracket => new Markup($"[yellow]{value}[/]"),
                        XmlTokenType.SelfClosingSlash => new Markup($"[yellow]{value}[/]"),
                        XmlTokenType.AttributeName => new Markup($"[cyan]{value}[/]"),
                        XmlTokenType.AttributeEquals => new Markup($"[yellow]{value}[/]"),
                        XmlTokenType.AttributeQuote => new Markup($"[grey]{value}[/]"),
                        XmlTokenType.AttributeValue => new Markup($"[green]{value}[/]"),
                        XmlTokenType.Text => new Markup($"[white]{value}[/]"),
                        XmlTokenType.Whitespace => new Markup($"[grey]{value}[/]"),
                        _ => new Markup(value)
                    };
                    AnsiConsole.Write(colored);
                });
            }
            else if (token.Metadata is JsonCodeBlockMetadata jsonMetadata)
            {
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
            }
            else if (token.Metadata is TypeScriptCodeBlockMetadata tsMetadata)
            {
                await tsMetadata.RegisterInlineTokenHandler( inlineToken =>
                {
                    var value = Markup.Escape(inlineToken.Value);
                    var colored = inlineToken.TokenType switch
                    {
                        TypescriptTokenType.Identifier => new Markup($"[cyan]{value}[/]"),
                        TypescriptTokenType.Keyword => new Markup($"[blue]{value}[/]"),
                        TypescriptTokenType.StringValue => new Markup($"[green]{value}[/]"),
                        TypescriptTokenType.Number => new Markup($"[magenta]{value}[/]"),
                        TypescriptTokenType.Operator => new Markup($"[yellow]{value}[/]"),
                        TypescriptTokenType.Comment => new Markup($"[grey]{value}[/]"),
                        TypescriptTokenType.Whitespace => new Markup($"[grey]{value}[/]"),
                        _ => new Markup(value)
                    };
                    AnsiConsole.Write(colored);
                });
            }
            else
            {
                // Handle regular markup tokens
                var value = Markup.Escape(token.Value);
                var colored = token.TokenType switch
                {
                    MarkupTokenType.Text => new Markup($"[white]{value}[/]"),
                    MarkupTokenType.Bold => new Markup($"[bold]{value}[/]"),
                    MarkupTokenType.Italic => new Markup($"[italic]{value}[/]"),
                    MarkupTokenType.Heading => new Markup($"[bold blue]{value}[/]"),
                    MarkupTokenType.Link => new Markup($"[blue underline]{value}[/]"),
                    _ => new Markup(value)
                };

                AnsiConsole.Write(colored);
            }

            if (token.Metadata is InlineMarkupMetadata)
            {
                AnsiConsole.WriteLine();
            }
        });

        await writerTask;

        Console.WriteLine();
        Console.WriteLine("Done.");
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