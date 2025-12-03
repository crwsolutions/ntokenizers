---
layout: default
title: "Home"
---

# NTokenizers Documentation

Welcome to the documentation for the `NTokenizers` library. This library provides a collection of **stream-capable** tokenizers for XML, JSON, Markup, TypeScript, C# and SQL processing.

### Kickoff token processing

```csharp
await MarkupTokenizer.Create().ParseAsync(stream, onToken: async token => { /* handle tokens here */ }
```

> **NTokenizers.Extensions.Spectre.Console**
> **Heads up:** Want to visualize your tokens in the console? Explore our companion project [NTokenizers.Extensions.Spectre.Console](https://crwsolutions.github.io/NTokenizers.Extensions.Spectre.Console/) it brings your token streams to life alongside `MarkupTokenizer`.

## Overview

NTokenizers is a .NET library written in C# that provides tokenizers for processing structured text formats like Markup, JSON, XML, SQL, Typescript and CSharp. The `Tokenize` method is the core functionality that breaks down structured text into meaningful components (tokens) for processing. Its key feature is **stream processing capability** - it can handle data as it arrives in real-time, making it ideal for processing large files or streaming data without loading everything into memory at once.

<blockquote class="warning">
 <b>Warning</b><br/><br/>
 These tokenizers are <b>not validation-based</b> and are primarily intended for <b>prettifying</b>, <b>formatting</b>, or <b>visualizing</b> structured text. They do not perform strict validation of the input format, so they may produce unexpected results when processing malformed or invalid XML, JSON, or HTML. Use them with caution when dealing with untrusted or poorly formatted input.
</blockquote>

## Markup Example

Here's a simple example showing how to use the `MarkupTokenizer` with a `stream` containing some markup text and json inline code blocks:

```csharp
await MarkupTokenizer.Create().ParseAsync(stream, onToken: async token =>
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
```

This gives the following output:

![markupexample](assets\markup_example.png)

## String output

```csharp
var result = await MarkupTokenizer.Create().ParseAsync(stream, onToken: async token => { /* handle tokens here */ }
```

In addition to streaming tokens, the original input is returned for convenience.

## Code specific Tokenizers

The Code specific tokenizers are also available see:

|**language**|**page**|
|C#|[CSharp Tokenizer](/ntokenizers/csharp)|
|Json|[Json Tokenizer](/ntokenizers/json)|
|Sql|[Sql Tokenizer](/ntokenizers/sql)|
|typescript/javascript|[TypeScript Tokenizer](/ntokenizers/typescript)|
|xml|[Xml Tokenizer](/ntokenizers/xml)|

## Features

- **Stream Processing**: Can handle large files or real-time data streams without loading everything into memory
- **Real-time Parsing**: Processes tokens as they are encountered
- **Flexible Input**: Supports various input sources including streams, readers, and strings
- **Rich Token Information**: Provides detailed token type information for precise handling

> **Especially suitable for parsing AI chat streams**, NTokenizers excels at processing real-time tokenized data from AI models, enabling efficient handling of streaming responses and chat conversations without buffering entire responses.
