---
layout: default
title: "Json"
---

# JSON Tokenizer

The JSON tokenizer is designed to parse JSON code and break it down into meaningful components (tokens) for processing. It provides stream-capable functionality for handling large JSON files or real-time JSON data analysis.

## Overview

The JSON tokenizer is part of the NTokenizers library and provides a stream-capable approach to parsing JSON code. It can process JSON source code in real-time, making it suitable for large files or streaming scenarios where loading everything into memory at once is impractical.

## Public API

The JSON tokenizer inherits from `BaseSubTokenizer<JsonToken>` and provides the following key methods:

- `ParseAsync(Stream stream, Action<JsonToken> onToken)` - Asynchronously parses a stream of JSON code
- `Parse(Stream stream, Action<JsonToken> onToken)` - Synchronously parses a stream of JSON code
- `Parse(string input)` - Parses a string of JSON code and returns a list of tokens
- `ParseAsync(TextReader reader, Action<JsonToken> onToken)` - Asynchronously parses from a TextReader

## Usage Examples

### Basic Usage with Stream

```csharp
using NTokenizers.Json;
using Spectre.Console;
using System.Text;

string jsonCode = """
    {
        "name": "Laura Smith",
        "active": true,
        "age": 42
    }
    """;

using var stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonCode));
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

### Using with TextReader

```csharp
using NTokenizers.Json;
using System.IO;

string jsonCode = """{"name":"John","age":30}""";
using var reader = new StringReader(jsonCode);
await JsonTokenizer.Create().ParseAsync(reader, onToken: token =>
{
    Console.WriteLine($"Token: {token.TokenType} = '{token.Value}'");
});
```

### Parsing String Directly

```csharp
using NTokenizers.Json;

string jsonCode = """{"active":true,"count":5}""";
var tokens = JsonTokenizer.Create().Parse(jsonCode);
foreach (var token in tokens)
{
    Console.WriteLine($"Token: {token.TokenType} = '{token.Value}'");
}
```

### Use Processed stream as string

```csharp
using NTokenizers.Json;
using System.Text;

string jsonCode = """{"name":"Laura","active":true}""";
var processedString = await JsonTokenizer.Create().ParseAsync(jsonCode, token =>
{
    return token.TokenType switch
    {
        JsonTokenType.StartObject => $"[yellow]{token.Value}[/]",
        JsonTokenType.EndObject => $"[yellow]{token.Value}[/]",
        JsonTokenType.PropertyName => $"[cyan]{token.Value}[/]",
        JsonTokenType.StringValue => $"[green]{token.Value}[/]",
        JsonTokenType.Number => $"[magenta]{token.Value}[/]",
        JsonTokenType.True => $"[orange1]{token.Value}[/]",
        JsonTokenType.False => $"[orange1]{token.Value}[/]",
        JsonTokenType.Null => $"[grey]{token.Value}[/]",
        _ => token.Value
    };
});
Console.WriteLine(processedString);
```

## Token Types

The JSON tokenizer produces tokens of type `JsonTokenType` with the following token types:

- `StartObject` - Represents the start of a JSON object (`{`)
- `EndObject` - Represents the end of a JSON object (`}`)
- `StartArray` - Represents the start of a JSON array (`[`)
- `EndArray` - Represents the end of a JSON array (`]`)
- `PropertyName` - Represents a property name in a JSON object
- `StringValue` - Represents a string value in JSON
- `Number` - Represents a number value in JSON (integer, float, scientific notation)
- `True` - Represents the boolean value `true`
- `False` - Represents the boolean value `false`
- `Null` - Represents the `null` value in JSON
- `Colon` - Represents a colon (`:`) used to separate keys and values
- `Comma` - Represents a comma (`,`) used to separate elements
- `Whitespace` - Represents whitespace characters between tokens

More info: [JsonTokenType.cs](https://github.com/crwsolutions/ntokenizers/blob/main/src/NTokenizers/Json/JsonTokenType.cs)

## See Also

- [Xml Tokenizer](/ntokenizers/xml)
- [Home](/ntokenizers/)
