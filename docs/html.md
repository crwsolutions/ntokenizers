---
layout: default
title: "Html"
---

# HTML Tokenizer

The HTML tokenizer is designed to parse HTML code and break it down into meaningful components (tokens) for processing. It provides stream-capable functionality for handling large HTML files or real-time HTML data analysis.

## Overview

The HTML tokenizer is part of the NTokenizers library and provides a stream-capable approach to parsing HTML code. It can process HTML source code in real-time, making it suitable for large files or streaming scenarios where loading everything into memory at once is impractical.

The HTML tokenizer also supports embedded CSS and JavaScript by delegating to the CSS and TypeScript tokenizers respectively when encountering `<style>` and `<script>` elements.

## Public API

The HTML tokenizer inherits from `BaseSubTokenizer<HtmlToken>` and provides the following key methods:

- `ParseAsync(Stream stream, Action<HtmlToken> onToken)` - Asynchronously parses a stream of HTML code
- `Parse(Stream stream, Action<HtmlToken> onToken)` - Synchronously parses a stream of HTML code
- `Parse(string input)` - Parses a string of HTML code and returns a list of tokens
- `ParseAsync(TextReader reader, Action<HtmlToken> onToken)` - Asynchronously parses from a TextReader

## Usage Examples

### Basic Usage with Stream

```csharp
using NTokenizers.Html;
using Spectre.Console;
using System.Text;

string htmlCode = """
    <!DOCTYPE html>
    <html>
    <head>
        <title>Sample Page</title>
    </head>
    <body>
        <h1>Hello World</h1>
        <p>This is a paragraph.</p>
    </body>
    </html>
    """;

using var stream = new MemoryStream(Encoding.UTF8.GetBytes(htmlCode));
await HtmlTokenizer.Create().ParseAsync(stream, onToken: token =>
{
    var value = Markup.Escape(token.Value);
    var colored = token.TokenType switch
    {
        HtmlTokenType.ElementName => new Markup($"[blue]{value}[/]"),
        HtmlTokenType.OpeningAngleBracket => new Markup($"[yellow]{value}[/]"),
        HtmlTokenType.ClosingAngleBracket => new Markup($"[yellow]{value}[/]"),
        HtmlTokenType.SelfClosingSlash => new Markup($"[yellow]{value}[/]"),
        HtmlTokenType.AttributeName => new Markup($"[cyan]{value}[/]"),
        HtmlTokenType.AttributeEquals => new Markup($"[yellow]{value}[/]"),
        HtmlTokenType.AttributeQuote => new Markup($"[grey]{value}[/]"),
        HtmlTokenType.AttributeValue => new Markup($"[green]{value}[/]"),
        HtmlTokenType.Text => new Markup($"[white]{value}[/]"),
        HtmlTokenType.Comment => new Markup($"[grey]{value}[/]"),
        HtmlTokenType.DocumentTypeDeclaration => new Markup($"[magenta]{value}[/]"),
        HtmlTokenType.Whitespace => new Markup($"[grey]{value}[/]"),
        _ => new Markup(value)
    };
    AnsiConsole.Write(colored);
});
```

### Using with TextReader

```csharp
using NTokenizers.Html;
using System.IO;

string htmlCode = """<div class="container"><p>Hello</p></div>""";
using var reader = new StringReader(htmlCode);
await HtmlTokenizer.Create().ParseAsync(reader, onToken: token =>
{
    Console.WriteLine($"Token: {token.TokenType} = '{token.Value}'");
});
```

### Parsing String Directly

```csharp
using NTokenizers.Html;

string htmlCode = """<a href="https://example.com">Link</a>""";
var tokens = HtmlTokenizer.Create().Parse(htmlCode);
foreach (var token in tokens)
{
    Console.WriteLine($"Token: {token.TokenType} = '{token.Value}'");
}
```

### HTML with Embedded CSS and JavaScript

The HTML tokenizer automatically delegates to specialized tokenizers when it encounters `<style>` and `<script>` elements:

```csharp
using NTokenizers.Html;
using System.Text;

string htmlCode = """
    <html>
    <head>
        <style>
            body { font-family: Arial, sans-serif; }
            .container { max-width: 600px; }
        </style>
    </head>
    <body>
        <div class="container">Content</div>
        <script>
            console.log('Hello, World!');
        </script>
    </body>
    </html>
    """;

using var stream = new MemoryStream(Encoding.UTF8.GetBytes(htmlCode));
await HtmlTokenizer.Create().ParseAsync(stream, onToken: token =>
{
    Console.WriteLine($"Token: {token.TokenType} = '{token.Value}'");
});
```

## Token Types

The HTML tokenizer produces tokens of type `HtmlTokenType` with the following token types:

- `None` - Represents no specific kind
- `ElementName` - Represents an HTML element name (e.g. `<div>`, `<p>`)
- `Text` - Represents text content within an HTML element
- `Comment` - Represents an HTML comment (including `<!-- ... -->`)
- `DocumentTypeDeclaration` - A document type declaration (DOCTYPE)
- `Whitespace` - Represents whitespace
- `OpeningAngleBracket` - Represents an opening angle bracket (`<`)
- `ClosingAngleBracket` - Represents a closing angle bracket (`>`)
- `AttributeName` - Name of the attribute
- `AttributeEquals` - Represents an equals sign (`=`)
- `AttributeValue` - Represents the value of an HTML attribute
- `AttributeQuote` - Represents the quote character (`"` or `'`)
- `SelfClosingSlash` - Represents a self-closing slash (`/`) in self-closing tags (e.g., `<br/>`)

More info: [HtmlTokenType.cs](https://github.com/crwsolutions/ntokenizers/blob/main/src/NTokenizers/Html/HtmlTokenType.cs)

## Special Features

### CSS and JavaScript Integration

The HTML tokenizer provides seamless integration with CSS and TypeScript tokenizers:

- **Style Elements**: When a `<style>` tag is encountered, the tokenizer delegates the CSS content to the `CssTokenizer` until it reaches `</style>`.
- **Script Elements**: When a `<script>` tag is encountered, the tokenizer delegates the JavaScript content to the `TypescriptTokenizer` until it reaches `</script>`.

This allows for proper tokenization of embedded CSS and JavaScript code within HTML documents while maintaining the streaming architecture.

## See Also

- [Xml Tokenizer](/ntokenizers/xml)
- [CSS Tokenizer](/ntokenizers/css)
- [TypeScript Tokenizer](/ntokenizers/typescript)
- [Home](/ntokenizers/)
