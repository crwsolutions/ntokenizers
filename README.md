# ntokenizers
Collection of tokenizers for XML, JSON, HTML, etc processing

## Overview

The `Tokenize` method is the core functionality of ntokenizers that breaks down structured text into meaningful components (tokens) for processing. Its key feature is **stream processing capability** - it can handle data as it arrives in real-time, making it ideal for processing large files or streaming data without loading everything into memory at once.

> [!WARNING] 
>
> These tokenizers are **not validation-based** and are primarily intended for **prettifying**, **formatting**, or **visualizing** structured text. They do not perform strict validation of the input format, so they may produce unexpected results when processing malformed or invalid XML, JSON, or HTML. Use them with caution when dealing with untrusted or poorly formatted input.
## Example

Here's a simple example showing how to use the XML tokenizer:

```csharp
```csharp
using System;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;
using NTokenizers;
using Spectre.Console;

class Program
{
    static async Task Main()
    {
        string xml = """
        <?xml version="1.0" encoding="utf-8"?>
        <user id="4821" active="true">
            <name>Laura Smith</name>
            <addresses>
                <address type="home">
                    <street>221B Baker Street</street>
                    <city>London</city>
                </address>
            </addresses>
        </user>
        """;

        // Create connected streams
        using var pipe = new AnonymousPipeServerStream(PipeDirection.Out);
        using var reader = new AnonymousPipeClientStream(PipeDirection.In, pipe.ClientSafePipeHandle);

        // Start slow writer
        var writerTask = EmitSlowlyAsync(xml, pipe);

        // Start parsing XML
        XmlTokenizer.Parse(reader, null, onToken: token =>
        {
            var value = Markup.Escape(token.Value);

            var colored = token.TokenType switch
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

                XmlTokenType.Comment => new Markup($"[grey]{value}[/]"),
                XmlTokenType.CData => new Markup($"[magenta]{value}[/]"),
                XmlTokenType.DocumentTypeDeclaration => new Markup($"[orange1]{value}[/]"),
                XmlTokenType.ProcessingInstruction => new Markup($"[orange1]{value}[/]"),

                _ => new Markup(value)
            };

            AnsiConsole.Write(colored);
        });

        await writerTask;

        Console.WriteLine();
        Console.WriteLine("Done.");
    }

    static async Task EmitSlowlyAsync(string xml, Stream output)
    {
        var rng = new Random();
        byte[] bytes = Encoding.UTF8.GetBytes(xml);

        foreach (var b in bytes)
        {
            await output.WriteAsync(new[] { b }.AsMemory(0, 1));
            await output.FlushAsync();
            await Task.Delay(rng.Next(10, 60));
        }

        output.Close(); // EOF
    }
}
```
