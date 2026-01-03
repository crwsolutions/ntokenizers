---
layout: default
title: "Css"
---

# CSS Tokenizer

The CSS tokenizer is designed to parse CSS code and break it down into meaningful components (tokens) for processing. It provides stream-capable functionality for handling large CSS files or real-time CSS data analysis.

<blockquote class="warning">
 <b>Please note:</b> Will be part of v2.2 and later
</blockquote>

## Overview

The CSS tokenizer is part of the NTokenizers library and provides a stream-capable approach to parsing CSS code. It can process CSS source code in real-time, making it suitable for large files or streaming scenarios where loading everything into memory at once is impractical.

## Public API

The CSS tokenizer inherits from `BaseSubTokenizer<CssToken>` and provides the following key methods:

- `ParseAsync(Stream stream, Action<CssToken> onToken)` - Asynchronously parses a stream of CSS code
- `Parse(Stream stream, Action<CssToken> onToken)` - Synchronously parses a stream of CSS code
- `Parse(string input)` - Parses a string of CSS code and returns a list of tokens
- `ParseAsync(TextReader reader, Action<CssToken> onToken)` - Asynchronously parses from a TextReader

## Usage Examples

### Basic Usage with Stream

```csharp
using NTokenizers.Css;
using Spectre.Console;
using System.Text;

string cssCode = """
    .header {
        color: blue;
        font-size: 16px;
    }
    """;

using var stream = new MemoryStream(Encoding.UTF8.GetBytes(cssCode));
await CssTokenizer.Create().ParseAsync(stream, onToken: token =>
{
    var value = Markup.Escape(token.Value);
    var colored = token.TokenType switch
    {
        CssTokenType.StartRuleSet => new Markup($"[yellow]{value}[/]"),
        CssTokenType.EndRuleSet => new Markup($"[yellow]{value}[/]"),
        CssTokenType.Selector => new Markup($"[cyan]{value}[/]"),
        CssTokenType.PropertyName => new Markup($"[blue]{value}[/]"),
        CssTokenType.StringValue => new Markup($"[white]{value}[/]"),
        CssTokenType.Quote => new Markup($"[yellow]{value}[/]"),
        CssTokenType.Number => new Markup($"[magenta]{value}[/]"),
        CssTokenType.Unit => new Markup($"[orange1]{value}[/]"),
        CssTokenType.Function => new Markup($"[yellow]{value}[/]"),
        CssTokenType.OpenParen => new Markup($"[yellow]{value}[/]"),
        CssTokenType.CloseParen => new Markup($"[yellow]{value}[/]"),
        CssTokenType.Comment => new Markup($"[green]{value}[/]"),
        CssTokenType.Colon => new Markup($"[yellow]{value}[/]"),
        CssTokenType.Semicolon => new Markup($"[yellow]{value}[/]"),
        CssTokenType.Comma => new Markup($"[yellow]{value}[/]"),
        CssTokenType.Whitespace => new Markup($"[grey]{value}[/]"),
        CssTokenType.AtRule => new Markup($"[orange1]{value}[/]"),
        CssTokenType.Identifier => new Markup($"[blue]{value}[/]"),
        CssTokenType.Operator => new Markup($"[red]{value}[/]"),
        _ => new Markup(value)
    };
    AnsiConsole.Write(colored);
});
```

### Using with TextReader

```csharp
using NTokenizers.Css;
using System.IO;

string cssCode = """
    .button {
        background: red;
    }
    """;
using var reader = new StringReader(cssCode);
await CssTokenizer.Create().ParseAsync(reader, onToken: token =>
{
    Console.WriteLine($"Token: {token.TokenType} = '{token.Value}'");
});
```

### Parsing String Directly

```csharp
using NTokenizers.Css;

string cssCode = """
    #main {
        width: 100%;
    }
    """;
var tokens = CssTokenizer.Create().Parse(cssCode);
foreach (var token in tokens)
{
    Console.WriteLine($"Token: {token.TokenType} = '{token.Value}'");
}
```

## Token Types

The CSS tokenizer produces tokens of type `CssTokenType` with the following token types:

- `StartRuleSet` - Represents the opening brace `{` of a rule set
- `EndRuleSet` - Represents the closing brace `}` of a rule set
- `Selector` - Represents a CSS selector (e.g., `.class`, `#id`, `tag`)
- `PropertyName` - Represents a CSS property name (e.g., `color`, `font-size`)
- `StringValue` - Represents a string value (e.g., `"Arial"`)
- `Quote` - Represents quote characters (`"` or `'`)
- `Number` - Represents numeric values (e.g., `16`, `0.5`)
- `Unit` - Represents CSS units (e.g., `px`, `em`, `%`)
- `Function` - Represents CSS functions like `url()`, `calc()`, `rgb()`
- `OpenParen` - Represents an opening parenthesis `(`
- `CloseParen` - Represents a closing parenthesis `)`
- `Comment` - Represents CSS comments (`/* ... */`)
- `Colon` - Represents the colon `:` separator between property and value
- `Semicolon` - Represents the semicolon `;` separator between declarations
- `Comma` - Represents a comma `,` separator in lists
- `Whitespace` - Represents whitespace characters (spaces, tabs, newlines)
- `AtRule` - Represents CSS at-rules like `@media`, `@import`
- `Identifier` - Represents identifiers (e.g., `red`, `bold`)
- `Operator` - Represents operators like `+`, `-`, `/`, `*`

More info: [CssTokenType.cs](https://github.com/crwsolutions/ntokenizers/blob/main/src/NTokenizers/Css/CssTokenType.cs)

## Features

- **Streaming Support**: Process CSS files of any size without loading the entire file into memory
- **Rule Set Parsing**: Recognizes rule sets with opening and closing braces
- **Selector Support**: Tokenizes various selector types including classes, IDs, and tags
- **Property/Value Parsing**: Properly handles property-value pairs
- **String Handling**: Processes quoted strings with proper escaping
- **Function Support**: Recognizes CSS functions like `url()`, `calc()`, etc.
- **Comments**: Handles `/* ... */` comments
- **At-Rules**: Supports CSS at-rules like `@media`, `@import`
- **Best-Effort Parsing**: Designed for formatting and visualization, not strict validation

## See Also

- [Json Tokenizer](/ntokenizers/json)
- [Xml Tokenizer](/ntokenizers/xml)
- [Yaml Tokenizer](/ntokenizers/yaml)
- [Home](/ntokenizers/)