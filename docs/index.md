---
layout: default
title: "Home"
---

# NTokenizers

Lightweight **Stream Tokenizers** for syntax highlighting and formatting. Perfect building block for **chat applications**, and **AI response rendering**. Tokenize streaming AI responses in real-time for beautiful syntax-highlighted output.

NTokenizers sits in the middle of a tokenization pipeline — it takes raw source code or markup as a stream and emits a sequence of typed tokens that a downstream renderer can consume:

```
 ┌────────┐    stream     ┌──────────────┐   tokens    ┌────────────┐
 │ source │ ────────────► │ NTokenizers  │ ──────────► │  Renderer  │ ──► styled output
 └────────┘               └──────────────┘             └────────────┘
```

This separation of concerns means NTokenizers stays format-focused while rendering is delegated to the consumer — whether that is a console UI, a web component, or a custom formatter. The stream-first design ensures low memory usage and real-time compatibility with AI chat outputs, CI logs, or any scenario where data arrives incrementally.

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
        await codeBlock.RegisterInlineTokenHandler(inlineToken =>
        {
            // Handle code block tokens with syntax highlighting
        });
    }
    // Handle regular markdown tokens
});
```

## Overview

NTokenizers is a .NET library written in C# that provides tokenizers for processing structured text formats like Markdown, JSON, XML, HTML, YAML, TOML, SQL, TypeScript, CSS, C#, C, C++, Go, Java, Kotlin, Rust, Swift and Python. The `Tokenize` method is the core functionality that breaks down structured text into meaningful components (tokens) for processing. Its key feature is **stream processing capability** — it can handle data as it arrives in real-time, making it ideal for processing large files or streaming data without loading everything into memory at once.

<blockquote class="warning">
  <b>Warning</b><br/><br/>
  These tokenizers are <b>not validation-based</b> and are primarily intended for <b>prettifying</b>, <b>formatting</b>, or <b>visualizing</b> structured text. They do not perform strict validation of the input format, so they may produce unexpected results when processing malformed or invalid XML, JSON, or HTML. Use them with caution when dealing with untrusted or poorly formatted input.
</blockquote>

## String output

```csharp
var result = await MarkdownTokenizer.Create().ParseAsync(stream, onToken: async token => { /* handle tokens here */ });
```

In addition to streaming tokens, the original input is returned for convenience.

## Code specific Tokenizers

Individual tokenizers are available for each supported format:

| **Language** | **Page** |
|---|---|
| Markdown | [Markdown Tokenizer](/ntokenizers/markdown) |
| HTML | [HTML Tokenizer](/ntokenizers/html) |
| JSON | [JSON Tokenizer](/ntokenizers/json) |
| YAML | [YAML Tokenizer](/ntokenizers/yaml) |
| TOML | [TOML Tokenizer](/ntokenizers/toml) |
| XML | [XML Tokenizer](/ntokenizers/xml) |
| C# | [CSharp Tokenizer](/ntokenizers/csharp) |
| C | [C Tokenizer](/ntokenizers/c) |
| C++ | [C++ Tokenizer](/ntokenizers/cpp) |
| Go | [Go Tokenizer](/ntokenizers/go) |
| Java | [Java Tokenizer](/ntokenizers/java) |
| Kotlin | [Kotlin Tokenizer](/ntokenizers/kotlin) |
| Python | [Python Tokenizer](/ntokenizers/python) |
| Rust | [Rust Tokenizer](/ntokenizers/rust) |
| SQL | [SQL Tokenizer](/ntokenizers/sql) |
| Swift | [Swift Tokenizer](/ntokenizers/swift) |
| TypeScript | [TypeScript Tokenizer](/ntokenizers/typescript) |
| CSS | [CSS Tokenizer](/ntokenizers/css) |

## Features

- **Stream Processing**: Can handle large files or real-time data streams without loading everything into memory
- **Real-time Parsing**: Processes tokens as they are encountered
- **Flexible Input**: Supports various input sources including streams, readers, and strings
- **Rich Token Information**: Provides detailed token type information for precise handling

> **Especially suitable for parsing AI chat streams**, NTokenizers excels at processing real-time tokenized data from AI models, enabling efficient handling of streaming responses and chat conversations without buffering entire responses.