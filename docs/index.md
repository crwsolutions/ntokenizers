---
layout: default
title: "Home"
---

# NTokenizers Documentation

Welcome to the documentation for the `NTokenizers` library. This library provides a collection of **stream-capable** tokenizers for XML, JSON, Markup, TypeScript, C# and SQL processing.

## Overview

NTokenizers is a .NET library written in C# that provides tokenizers for processing structured text formats like Markup, JSON, XML, SQL, Typescript and CSharp. The `Tokenize` method is the core functionality that breaks down structured text into meaningful components (tokens) for processing. Its key feature is **stream processing capability** - it can handle data as it arrives in real-time, making it ideal for processing large files or streaming data without loading everything into memory at once.


> [!WARNING]
>
> These tokenizers are **not validation-based** and are primarily intended for **prettifying**, **formatting**, or **visualizing** structured text. They do not perform strict validation of the input format, so they may produce unexpected results when processing malformed or invalid XML, JSON, or HTML. Use them with caution when dealing with untrusted or poorly formatted input.

## Markup Example

Here's a simple example showing how to use the `MarkupTokenizer` with a `stream` containing some markup text and json inline code blocks:

```csharp
await MarkupTokenizer.Create().ParseAsync(reader, onToken: async token =>
{
    //Handle json code fence
    if (token.Metadata is JsonCodeBlockMetadata jsonMetadata)
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

    if (token.Metadata is InlineMarkupMetadata)
    {
        AnsiConsole.WriteLine();
    }
});
```

## Json Example

Here's a simple example showing how to use the `JsonTokenizer` with a `stream` containing json-data:

```csharp
await JsonTokenizer.Create().ParseAsync(stream, onToken: token =>
{
    var value = Markup.Escape(token.Value);
    var colored = token.TokenType switch
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
```

These examples will output the JSON content with colored tokens, demonstrating how the tokenizer processes structured data in real-time.