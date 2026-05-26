---
layout: default
title: "Toml"
---

# TOML Tokenizer

The TOML tokenizer is designed to parse TOML configuration files and break them down into meaningful components (tokens) for processing. It provides stream-capable functionality for handling large TOML files or real-time TOML data analysis.

## Overview

The TOML tokenizer is part of the NTokenizers library and provides a stream-capable approach to parsing TOML files. It can process TOML source code in real-time, making it suitable for large files or streaming scenarios where loading everything into memory at once is impractical.

## Public API

The TOML tokenizer inherits from `BaseSubTokenizer<TomlToken>` and provides the following key methods:

- `ParseAsync(Stream stream, Action<TomlToken> onToken)` - Asynchronously parses a stream of TOML content
- `Parse(Stream stream, Action<TomlToken> onToken)` - Synchronously parses a stream of TOML content
- `Parse(string input)` - Parses a string of TOML content and returns a list of tokens
- `ParseAsync(TextReader reader, Action<TomlToken> onToken)` - Asynchronously parses from a TextReader

## Usage Examples

### Basic Usage with Stream

```csharp
using NTokenizers.Toml;
using Spectre.Console;
using System.Text;

string tomlCode = """
    [database]
    server = "192.168.1.1"
    ports = [ 8001, 8001, 8002 ]
    connection_max = 5000
    enabled = true
    """;

using var stream = new MemoryStream(Encoding.UTF8.GetBytes(tomlCode));
await TomlTokenizer.Create().ParseAsync(stream, onToken: token =>
{
    var value = Markup.Escape(token.Value);
    var colored = token.TokenType switch
    {
        TomlTokenType.Identifier => new Markup($"[cyan]{value}[/]"),
        TomlTokenType.StringValue => new Markup($"[green]{value}[/]"),
        TomlTokenType.StringQuote => new Markup($"[green]{value}[/]"),
        TomlTokenType.Number => new Markup($"[magenta]{value}[/]"),
        TomlTokenType.Boolean => new Markup($"[orange1]{value}[/]"),
        TomlTokenType.DateTime => new Markup($"[blue]{value}[/]"),
        TomlTokenType.Comment => new Markup($"[dim]{value}[/]"),
        TomlTokenType.OpenBracket => new Markup($"[yellow]{value}[/]"),
        TomlTokenType.CloseBracket => new Markup($"[yellow]{value}[/]"),
        TomlTokenType.OpenBrace => new Markup($"[yellow]{value}[/]"),
        TomlTokenType.CloseBrace => new Markup($"[yellow]{value}[/]"),
        TomlTokenType.Equal => new Markup($"[grey]{value}[/]"),
        TomlTokenType.Comma => new Markup($"[grey]{value}[/]"),
        TomlTokenType.Dot => new Markup($"[grey]{value}[/]"),
        TomlTokenType.Whitespace => new Markup($"[dim]{value}[/]"),
        _ => new Markup(value)
    };
    AnsiConsole.Write(colored);
});
```

### Using with TextReader

```csharp
using NTokenizers.Toml;
using System.IO;

string tomlCode = """
    title = "TOML Example"
    [owner]
    name = "Tom Preston-Werner"
    """;
using var reader = new StringReader(tomlCode);
await TomlTokenizer.Create().ParseAsync(reader, onToken: token =>
{
    Console.WriteLine($"Token: {token.TokenType} = '{token.Value}'");
});
```

### Parsing String Directly

```csharp
using NTokenizers.Toml;

string tomlCode = """
    active = true
    count = 42
    """;
var tokens = TomlTokenizer.Create().Parse(tomlCode);
foreach (var token in tokens)
{
    Console.WriteLine($"Token: {token.TokenType} = '{token.Value}'");
}
```

## Token Types

The TOML tokenizer produces the following token types via `TomlTokenType`:

| Token Type | Description |
|---|---|
| `Whitespace` | Spaces, tabs, newlines |
| `Comment` | Comments starting with `#` to end of line |
| `Identifier` | Bare keys, dotted key segments |
| `Dot` | Dot separator in dotted keys (`a.b.c`) |
| `Equal` | Key-value separator (`=`) |
| `Comma` | Separator in arrays and inline tables |
| `OpenBracket` | Opening bracket (`[`) |
| `CloseBracket` | Closing bracket (`]`) |
| `OpenBrace` | Opening brace (`{`) |
| `CloseBrace` | Closing brace (`}`) |
| `StringQuote` | Opening or closing quote character |
| `StringValue` | The content between quotes |
| `Number` | Integer, float, scientific notation, hex, octal, binary, inf, nan |
| `Boolean` | `true` or `false` |
| `DateTime` | Date, time, or date-time values |

## Supported TOML Features

- **Basic strings**: `"..."` with escape sequences
- **Literal strings**: `'...'` without escape processing
- **Multiline basic strings**: `"""..."""` with escape sequences
- **Multiline literal strings**: `'''...'''` without escape processing
- **Numbers**: integers, floats, scientific notation, hex (`0x`), octal (`0o`), binary (`0b`), `inf`, `nan`
- **Booleans**: `true`, `false`
- **Dates and times**: local date, local time, local datetime, offset datetime
- **Tables**: `[table]` and `[[array_table]]`
- **Inline tables**: `{key = value}`
- **Arrays**: `[1, 2, 3]`
- **Dotted keys**: `a.b.c = "value"`
- **Comments**: `#` to end of line
