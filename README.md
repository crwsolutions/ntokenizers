# NTokenizers
Collection of **stream-capable** tokenizers for Markdown, JSON, XML, HTML, YAML, SQL, Typescript, CSS and CSharp processing.

### Kickoff token processing

```csharp
// kickoff markdown tokenizer
await MarkdownTokenizer.Create().ParseAsync(stream, onToken: async token => { /* handle markdown-tokens here */ });

// kickoff csharp tokenizer
await CSharpTokenizer.Create().ParseAsync(stream, onToken: token => { /* handle csharp-tokens here */ });

// kickoff json tokenizer
await JsonTokenizer.Create().ParseAsync(stream, onToken: token => { /* handle json-tokens here */ });

// kickoff sql tokenizer
await SqlTokenizer.Create().ParseAsync(stream, onToken: token => { /* handle sql-tokens here */ });

// kickoff typescript tokenizer
await TypescriptTokenizer.Create().ParseAsync(stream, onToken: token => { /* handle typescript-tokens here */ });

// kickoff css tokenizer
await CssTokenizer.Create().ParseAsync(stream, onToken: token => { /* handle css-tokens here */ });

// kickoff xml tokenizer
await XmlTokenizer.Create().ParseAsync(stream, onToken: token => { /* handle xml-tokens here */ });

// kickoff html tokenizer*
await HtmlTokenizer.Create().ParseAsync(stream, onToken: token => { /* handle html-tokens here */ });

// kickoff yaml tokenizer
await YamlTokenizer.Create().ParseAsync(stream, onToken: token => { /* handle yaml-tokens here */ });
```

* You also have to handle the script and style Tokenizers. Check out the [docs](https://crwsolutions.github.io/ntokenizers/html) for more information.

## Overview

NTokenizers is a .NET library written in C# that provides tokenizers for processing structured text formats like Markdown, JSON, XML, HTML, YAML, SQL, Typescript, CSS and CSharp. The `Tokenize` method is the core functionality that breaks down structured text into meaningful components (tokens) for processing. Its key feature is **stream processing capability** - it can handle data as it arrives in real-time, making it ideal for processing large files or streaming data without loading everything into memory at once.

> [!WARNING] 
>
> These tokenizers are **not validation-based** and are primarily intended for **prettifying**, **formatting**, or **visualizing** structured text. They do not perform strict validation of the input format, so they may produce unexpected results when processing malformed or invalid XML, JSON, or HTML. Use them with caution when dealing with untrusted or poorly formatted input.

> [!WARNING] 
>
> MarkupTokenizer was renamed to MarkdownTokenizer in v2.


## Used by

- [NTokenizers.Extensions.Spectre.Console](https://www.nuget.org/packages/NTokenizers.Extensions.Spectre.Console/) Spectre.Console rendering extensions for NTokenizers, Style-rich console syntax highlighting.

# Architecture

Most **tokenizers**, such as json, xml, or etc..., can be used individually, depending on the specific format you want to parse.

The `MarkdownTokenizer` however is a special case. Instead of working on a single format, it acts as a **composite tokenizer**, using the other tokenizers as **subtokenizers**. When parsing a stream, MarkdownTokenizer delegates portions of the input to the appropriate subtokenizer, allowing it to handle multiple formats seamlessly in one pass.

The same principle applies to inline tokenizers such as Heading, Blockquote, ListItem, and others. However, they cannot be used individually and produce the same token types as the `MarkdownTokenizer`.

### Diagram

```
         ┌─────────┐
         │ stream  │
         └─────────┘
              │  ParseAsync()
              ▼
   ┌─────────────────────┐
   │  MarkdownTokenizer  │ ───────────► fire markdown tokens
   └─────────────────────┘
              │
              ▼       ┌─────────┐
              ├──────►│   json  │ ───► fire json tokens
              │       └─────────┘
              │
              │       ┌─────────┐
              ├──────►│ Heading │ ───► fire markdown tokens
              │       └─────────┘
              │
              │       ┌─────────┐
              ├──────►│   html  │ ───► fire html tokens
              │       └─────────┘
              │            │
              │            ▼       ┌─────────┐
              │            ├──────►│   css   │ ───► fire css tokens
              │            │       └─────────┘
              │            │
              │            │       ┌─────────┐
              │            └──────►│ script  │ ───► fire typescript tokens
              │                    └─────────┘
              │       ┌─────────┐
              └──────►│  etc..  │ ───► etc
                      └─────────┘
```

## Example

Here's a simple example showing how to use the `MarkdownTokenizer`:

```csharp
using NTokenizers.Core;
using NTokenizers.Css;
using NTokenizers.Html;
using NTokenizers.Json;
using NTokenizers.Markdown;
using NTokenizers.Markdown.Metadata;
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
        string markdown = """
        Here is some **bold** text and some *italic* text.

        # NTokenizers Showcase
        
        ## Css example
        ```css
        .user {
            color: #FFFFFF;
            active: true;
        }
        ```

        ## XML example
        ```xml
        <user id="4821" active="true">
            <name>Laura Smith</name>
        </user>
        ```

        ## HTML example
        ```html
        <html>
        <head>
            <style>
                body { font-family: Arial, sans-serif; background-color: #f0f8ff; }
                .header { color: #4682b4; text-align: center; }
                .content { margin: 20px; padding: 15px; background-color: white; border-radius: 5px; }
            </style>
        </head>
        <body>
            <p>Hello world!</p>
            <script>
                console.log("Hello from the sample script!");
                document.addEventListener('DOMContentLoaded', function() {
                    console.log("DOM is fully loaded");
                });
            </script>
        </body>
        </html>
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
        var writerTask = EmitSlowlyAsync(markdown, pipe);

        // Parse markup
        await MarkdownTokenizer.Create().ParseAsync(reader, onToken: async token =>
        {
            if (token.Metadata is ICodeBlockMetadata codeBlock)
            {
                AnsiConsole.WriteLine();
                AnsiConsole.Write(new Markup($"[bold lime]{codeBlock.Language}:[/]"));
                AnsiConsole.WriteLine();
            }

            if (token.Metadata is ListItemMetadata listMetadata)
            {
                AnsiConsole.Write(new Markup($"[bold lime]{listMetadata.Marker} [/]"));
                await listMetadata.RegisterInlineTokenHandler(inlineToken =>
                {
                    var value = Markup.Escape(inlineToken.Value);
                    AnsiConsole.Write(new Markup($"[bold red]{value}[/]"));
                });
                Debug.WriteLine("Written listItem inlines");

            }
            else if (token.Metadata is HeadingMetadata headingMetadata)
            {
                await headingMetadata.RegisterInlineTokenHandler(inlineToken =>
                {
                    var value = Markup.Escape(inlineToken.Value);
                    var colored = headingMetadata.Level != 1 ?
                        new Markup($"[bold GreenYellow]{value}[/]") :
                        new Markup($"[bold yellow]** {value} **[/]");
                    AnsiConsole.Write(colored);
                });
                Debug.WriteLine("Written Heading inlines");
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
                await jsonMetadata.RegisterInlineTokenHandler(inlineToken =>
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
            else if (token.Metadata is HtmlCodeBlockMetadata htmlMetadata)
            {
                await htmlMetadata.RegisterInlineTokenHandler(async inlineToken =>
                {
                    if (inlineToken.Metadata is TypeScriptCodeBlockMetadata tsMeta)
                    {
                        await HandleScript(tsMeta);
                    }
                    else if (inlineToken.Metadata is CssCodeBlockMetadata cssMeta)
                    {
                        await HandleCss(cssMeta);
                    }
                    else
                    {
                    var value = Markup.Escape(inlineToken.Value);
                    var colored = inlineToken.TokenType switch
                    {
                        HtmlTokenType.OpeningAngleBracket => new Markup($"[yellow]{value}[/]"),
                        HtmlTokenType.ClosingAngleBracket => new Markup($"[yellow]{value}[/]"),
                        HtmlTokenType.SelfClosingSlash => new Markup($"[yellow]{value}[/]"),
                        HtmlTokenType.AttributeName => new Markup($"[cyan]{value}[/]"),
                        HtmlTokenType.AttributeEquals => new Markup($"[yellow]{value}[/]"),
                        HtmlTokenType.AttributeQuote => new Markup($"[grey]{value}[/]"),
                        HtmlTokenType.AttributeValue => new Markup($"[green]{value}[/]"),
                        HtmlTokenType.Text => new Markup($"[white]{value}[/]"),
                        HtmlTokenType.Comment => new Markup($"[grey]{value}[/]"),
                        HtmlTokenType.Whitespace => new Markup($"[grey]{value}[/]"),
                        _ => new Markup(value)
                    };
                    AnsiConsole.Write(colored);
                    }
                });
            }
            else if (token.Metadata is TypeScriptCodeBlockMetadata tsMetadata)
            {
                await HandleScript(tsMetadata);
            }
            else if (token.Metadata is CssCodeBlockMetadata cssMetadata)
            {
                await HandleCss(cssMetadata);
            }
            else
            {
                // Handle regular markup tokens
                var value = Markup.Escape(token.Value);
                var colored = token.TokenType switch
                {
                    MarkdownTokenType.Text => new Markup($"{value}"),
                    MarkdownTokenType.Bold => new Markup($"[bold]{value}[/]"),
                    MarkdownTokenType.Italic => new Markup($"[italic]{value}[/]"),
                    _ => new Markup(value)
                };

                AnsiConsole.Write(colored);
            }

            if (token.Metadata is InlineMetadata)
            {
                AnsiConsole.WriteLine();
            }
        });

        await writerTask;

        Console.WriteLine();
        Console.WriteLine("Done.");
    }

    private static async Task HandleScript(TypeScriptCodeBlockMetadata tsMetadata)
    {
        await tsMetadata.RegisterInlineTokenHandler(inlineToken =>
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

    private static async Task HandleCss(CssCodeBlockMetadata cssMetadata)
    {
        await cssMetadata.RegisterInlineTokenHandler(inlineToken =>
        {
            var value = Markup.Escape(inlineToken.Value);
            var colored = inlineToken.TokenType switch
            {
                CssTokenType.Identifier => new Markup($"[white]{value}[/]"),
                CssTokenType.Number => new Markup($"[magenta]{value}[/]"),
                CssTokenType.Operator => new Markup($"[yellow]{value}[/]"),
                CssTokenType.Selector => new Markup($"[yellow]{value}[/]"),
                CssTokenType.Comment => new Markup($"[green]{value}[/]"),
                CssTokenType.Whitespace => new Markup($"[grey]{value}[/]"),
                _ => new Markup(value)
            };
            AnsiConsole.Write(colored);
        });
    }

    static async Task EmitSlowlyAsync(string markdown, Stream output)
    {
        var rng = new Random();
        byte[] bytes = Encoding.UTF8.GetBytes(markdown);

        foreach (var b in bytes)
        {
            await output.WriteAsync(new[] { b }.AsMemory(0, 1));
            await output.FlushAsync();
            await Task.Delay(rng.Next(0, 2));
        }

        output.Close(); // EOF
    }
}
```

For more information, check out the documentation [here](https://crwsolutions.github.io/ntokenizers/).