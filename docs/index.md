---
layout: default
title: "Home"
---

# NTokenizers

Lightweight **Stream Tokenizers** for syntax highlighting and formatting. Perfect **building block** for render logic, handling **AI responses**, and **streaming Markdown to HTML** via the included [ToHtml](https://crwsolutions.github.io/ntokenizers/tohtml) converter.

NTokenizers sits in the middle of a tokenization pipeline — it takes raw source code or markup as a stream and emits a sequence of typed tokens that a downstream renderer can consume:

```
 ┌────────┐    stream     ┌──────────────┐   tokens    ┌────────────┐
 │ source │ ────────────► │ NTokenizers  │ ──────────► │  Renderer  │ ──► styled output
 └────────┘               └──────────────┘             └────────────┘
```

This separation of concerns means NTokenizers stays format-focused while rendering is delegated to the consumer — whether that is a console UI, a web component, or a custom formatter. The stream-first design ensures low memory usage and real-time compatibility with AI chat outputs, CI logs, or any scenario where data arrives incrementally.

> [!WARNING] 
>
> **Version 9.0.0 is a breaking release**: token handlers are now async (`Func<TToken, Task>` instead of `Action<TToken>`) and Markdown inline sub-parsing now uses `ParseInlinesAsync`. If you are upgrading from 8.x, read the [v8 → v9 migration guide](/ntokenizers/migrating-v8-v9) first.

### Used by

- [NTokenizers.Extensions.Spectre.Console](https://www.nuget.org/packages/NTokenizers.Extensions.Spectre.Console/) Spectre.Console rendering extensions for NTokenizers, Style-rich console syntax highlighting.

## Supported Formats

NTokenizers provides a collection of stream-capable tokenizers for processing structured text. Each tokenizer breaks down input into meaningful tokens as data arrives in real-time—ideal for large files or streaming data without loading everything into memory.

The library supports the following formats:

- **Markup languages:** Markdown, HTML
- **Data formats:** JSON, YAML, TOML, XML
- **Programming languages:** C#, C, C++, Go, Java, Kotlin, Python, Rust, SQL, Swift, TypeScript, CSS

The `MarkdownTokenizer` acts as a **composite tokenizer**, delegating code blocks to the appropriate sub-tokenizer based on the language tag. This allows seamless parsing of documents that mix multiple formats in a single pass.

## Quick Start

Initialize any tokenizer and start parsing a stream:

```csharp
// Use any tokenizer — replace [Language] with the target format
await [Language]Tokenizer.Create().ParseAsync(stream, onToken: async token =>
{
    // Handle tokens as they arrive
});
```

Example with the JSON tokenizer:

```csharp
await JsonTokenizer.Create().ParseAsync(stream, onToken: async token =>
{
    var value = Markup.Escape(token.Value);
    var colored = token.TokenType switch
    {
        JsonTokenType.PropertyName => new Markup($"[cyan]{value}[/]"),
        JsonTokenType.StringValue => new Markup($"[green]{value}[/]"),
        JsonTokenType.Number => new Markup($"[magenta]{value}[/]"),
        _ => new Markup(value)
    };
    AnsiConsole.Write(colored);
});
```

The `MarkdownTokenizer` delegates code blocks to sub-tokenizers automatically:

```csharp
await MarkdownTokenizer.Create().ParseAsync(stream, onToken: async token =>
{
    if (token.Metadata is ICodeBlockMetadata codeBlock)
    {
        await codeBlock.ParseInlinesAsync(async inlineToken =>
        {
            // Handle code block tokens with syntax highlighting
        });
    }
    // Handle regular markdown tokens
});
```

## Overview

NTokenizers is a .NET library written in C# that provides tokenizers for processing structured text formats like Markdown, JSON, XML, HTML, YAML, TOML, SQL, TypeScript, CSS, C#, C, C++, Go, Java, Kotlin, Rust, Swift and Python. The `ParseAsync` method is the core functionality that breaks down structured text into meaningful components (tokens) for processing. Its key feature is **stream processing capability** — it can handle data as it arrives in real-time, making it ideal for processing large files or streaming data without loading everything into memory at once.

<blockquote class="warning">
  <b>Warning</b><br/><br/>
  These tokenizers are <b>not validation-based</b> and are primarily intended for <b>prettifying</b>, <b>formatting</b>, or <b>visualizing</b> structured text. They do not perform strict validation of the input format, so they may produce unexpected results when processing malformed or invalid XML, JSON, or HTML. Use them with caution when dealing with untrusted or poorly formatted input.
</blockquote>

## String output

```csharp
var result = await MarkdownTokenizer.Create().ParseAsync(stream, onToken: async token => { /* handle tokens here */ });
```

In addition to streaming tokens, the original input is returned for convenience.

## Markdown to HTML

The **ToHtml API** is feature complete and stream-capable: it converts a Markdown stream to an HTML stream incrementally, writing each token to the output writer as it is parsed. It works on live streams such as AI chat responses, and offers both fragment and full HTML document output.

Quick start:

```csharp
using NTokenizers.ToHtml;

string markdown = "# Hello\n\nThis is **bold** text.";

// Fragment (body-only)
string html = MarkdownConverter.ToHtml(markdown);

// Full HTML document, stream-to-stream
using var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(markdown));
using var writer = new StringWriter();
await MarkdownConverter.WriteHtmlDocumentAsync(inputStream, writer);
```

See the [ToHtml API](/ntokenizers/tohtml) page for more details.

## Code specific Tokenizers

The library supports the following formats:

- **Markup languages:** [Markdown](/ntokenizers/markdown), [HTML](/ntokenizers/html)
- **Data formats:** [JSON](/ntokenizers/json), [YAML](/ntokenizers/yaml), [TOML](/ntokenizers/toml), [XML](/ntokenizers/xml)
- **Programming languages:** [C#](/ntokenizers/csharp), [C](/ntokenizers/c), [C++](/ntokenizers/cpp), [Go](/ntokenizers/go), [Java](/ntokenizers/java), [Kotlin](/ntokenizers/kotlin), [Python](/ntokenizers/python), [Rust](/ntokenizers/rust), [SQL](/ntokenizers/sql), [Swift](/ntokenizers/swift), [TypeScript](/ntokenizers/typescript), [CSS](/ntokenizers/css)

## Features

- **Stream Processing**: Can handle large files or real-time data streams without loading everything into memory
- **Real-time Parsing**: Processes tokens as they are encountered
- **Flexible Input**: Supports various input sources including streams, readers, and strings
- **Rich Token Information**: Provides detailed token type information for precise handling
- **Markdown to HTML**: The ToHtml API converts a Markdown stream to an HTML stream incrementally, with both fragment and full HTML document output ([ToHtml API](/ntokenizers/tohtml))

> **Especially suitable for parsing AI chat streams**, NTokenizers excels at processing real-time tokenized data from AI models, enabling efficient handling of streaming responses and chat conversations without buffering entire responses.

## Tools, examples, and showcases

The repository ships a collection of sample applications and tools that showcase the library in action — an AI chat showcase, a streaming Markdown to HTML showcase, a token debugger, and much more. See the [Tools](/ntokenizers/tools) page for details.
