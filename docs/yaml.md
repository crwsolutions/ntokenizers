---
layout: default
title: "Yaml"
---

# YAML Tokenizer

The YAML tokenizer is designed to parse YAML code and break it down into meaningful components (tokens) for processing. It provides stream-capable functionality for handling large YAML files or real-time YAML data analysis.

<blockquote class="warning">
 <b>Please note:</b> Will be part of v1.1 and later
</blockquote>

## Overview

The YAML tokenizer is part of the NTokenizers library and provides a stream-capable approach to parsing YAML code. It can process YAML source code in real-time, making it suitable for large files or streaming scenarios where loading everything into memory at once is impractical.

## Public API

The YAML tokenizer inherits from `BaseSubTokenizer<YamlToken>` and provides the following key methods:

- `ParseAsync(Stream stream, Action<YamlToken> onToken)` - Asynchronously parses a stream of YAML code
- `Parse(Stream stream, Action<YamlToken> onToken)` - Synchronously parses a stream of YAML code
- `Parse(string input)` - Parses a string of YAML code and returns a list of tokens
- `ParseAsync(TextReader reader, Action<YamlToken> onToken)` - Asynchronously parses from a TextReader

## Usage Examples

### Basic Usage with Stream

```csharp
using NTokenizers.Yaml;
using Spectre.Console;
using System.Text;

string yamlCode = """
    ---
    name: Alice Smith
    age: 30
    active: true
    hobbies:
      - reading
      - coding
    ...
    """;

using var stream = new MemoryStream(Encoding.UTF8.GetBytes(yamlCode));
await YamlTokenizer.Create().ParseAsync(stream, onToken: token =>
{
    var value = Markup.Escape(token.Value);
    var colored = token.TokenType switch
    {
        YamlTokenType.DocumentStart => new Markup($"[yellow]{value}[/]"),
        YamlTokenType.DocumentEnd => new Markup($"[yellow]{value}[/]"),
        YamlTokenType.Key => new Markup($"[cyan]{value}[/]"),
        YamlTokenType.Colon => new Markup($"[yellow]{value}[/]"),
        YamlTokenType.Value => new Markup($"[green]{value}[/]"),
        YamlTokenType.Comment => new Markup($"[grey]{value}[/]"),
        YamlTokenType.Quote => new Markup($"[green]{value}[/]"),
        YamlTokenType.String => new Markup($"[green]{value}[/]"),
        YamlTokenType.Anchor => new Markup($"[magenta]{value}[/]"),
        YamlTokenType.Alias => new Markup($"[magenta]{value}[/]"),
        YamlTokenType.Tag => new Markup($"[orange1]{value}[/]"),
        YamlTokenType.FlowSeqStart => new Markup($"[yellow]{value}[/]"),
        YamlTokenType.FlowSeqEnd => new Markup($"[yellow]{value}[/]"),
        YamlTokenType.FlowMapStart => new Markup($"[yellow]{value}[/]"),
        YamlTokenType.FlowMapEnd => new Markup($"[yellow]{value}[/]"),
        YamlTokenType.FlowEntry => new Markup($"[yellow]{value}[/]"),
        YamlTokenType.BlockSeqEntry => new Markup($"[yellow]{value}[/]"),
        YamlTokenType.Whitespace => new Markup($"{value}"),
        _ => new Markup(value)
    };
    AnsiConsole.Write(colored);
});
```

### Using with TextReader

```csharp
using NTokenizers.Yaml;
using System.IO;

string yamlCode = """
    name: John
    age: 30
    """;
using var reader = new StringReader(yamlCode);
await YamlTokenizer.Create().ParseAsync(reader, onToken: token =>
{
    Console.WriteLine($"Token: {token.TokenType} = '{token.Value}'");
});
```

### Parsing String Directly

```csharp
using NTokenizers.Yaml;

string yamlCode = """
    name: Alice
    active: true
    """;
var tokens = YamlTokenizer.Create().Parse(yamlCode);
foreach (var token in tokens)
{
    Console.WriteLine($"Token: {token.TokenType} = '{token.Value}'");
}
```

## Token Types

The YAML tokenizer produces tokens of type `YamlTokenType` with the following token types:

- `DocumentStart` - Represents the document start marker (`---`)
- `DocumentEnd` - Represents the document end marker (`...`)
- `Comment` - Represents a comment (`# ...`)
- `Key` - Represents a key before a colon
- `Colon` - Represents a colon (`:`) separator
- `Value` - Represents a plain value
- `Quote` - Represents a quote character (`"`)
- `String` - Represents the content between quotes
- `Anchor` - Represents an anchor (`&anchor`)
- `Alias` - Represents an alias (`*alias`)
- `Tag` - Represents a tag (`!tag` or `!!type`)
- `FlowSeqStart` - Represents the start of a flow sequence (`[`)
- `FlowSeqEnd` - Represents the end of a flow sequence (`]`)
- `FlowMapStart` - Represents the start of a flow mapping (`{`)
- `FlowMapEnd` - Represents the end of a flow mapping (`}`)
- `FlowEntry` - Represents a comma (`,`) separator in flow collections
- `BlockSeqEntry` - Represents a block sequence entry marker (`-`)
- `Whitespace` - Represents whitespace characters (spaces, tabs, newlines)

More info: [YamlTokenType.cs](https://github.com/crwsolutions/ntokenizers/blob/main/src/NTokenizers/Yaml/YamlTokenType.cs)

## Features

- **Streaming Support**: Process YAML files of any size without loading the entire file into memory
- **Document Markers**: Recognizes `---` and `...` document boundaries
- **Block and Flow Styles**: Supports both block-style and flow-style YAML
- **Anchors and Aliases**: Tokenizes `&anchor` and `*alias` references
- **Tags**: Recognizes `!tag` and `!!type` annotations
- **Comments**: Handles `#` comments
- **Quoted Strings**: Properly tokenizes quoted string values
- **Best-Effort Parsing**: Designed for formatting and visualization, not strict validation

## See Also

- [Json Tokenizer](/ntokenizers/json)
- [Xml Tokenizer](/ntokenizers/xml)
- [Home](/ntokenizers/)
