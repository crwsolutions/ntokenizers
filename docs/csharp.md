---
layout: default
title: "C#"
---

# C# Tokenizer

The C# tokenizer is designed to parse C# code and break it down into meaningful components (tokens) for processing. It provides stream-capable functionality for handling large code files or real-time code analysis.

## Overview

The C# tokenizer is part of the NTokenizers library and provides a stream-capable approach to parsing C# code. It can process C# source code in real-time, making it suitable for large files or streaming scenarios where loading everything into memory at once is impractical.

> [!WARNING]
>
> This tokenizer is **not validation-based** and is primarily intended for **prettifying**, **formatting**, or **visualizing** C# code. It does not perform strict validation of the C# syntax, so it may produce unexpected results when processing malformed or invalid C# code. Use it with caution when dealing with untrusted or poorly formatted input.

## Public API

The C# tokenizer inherits from `BaseTokenizer<CSharpToken>` and provides the following key methods:

- `ParseAsync(Stream stream, Action<CSharpToken> onToken)` - Asynchronously parses a stream of C# code
- `Parse(Stream stream, Action<CSharpToken> onToken)` - Synchronously parses a stream of C# code
- `Parse(string input)` - Parses a string of C# code and returns a list of tokens
- `ParseAsync(TextReader reader, Action<CSharpToken> onToken)` - Asynchronously parses from a TextReader

## Usage Examples

### Basic Usage with Stream

```csharp
using NTokenizers.CSharp;
using Spectre.Console;
using System.Text;

string csharpCode = """
    var user = new { Name = "Laura Smith",
        Active = true };
    """;

using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csharpCode));
await CSharpTokenizer.Create().ParseAsync(stream, onToken: token =>
{
    var value = Markup.Escape(token.Value);
    var colored = token.TokenType switch
    {
        CSharpTokenType.Keyword => new Markup($"[blue]{value}[/]"),
        CSharpTokenType.Identifier => new Markup($"[cyan]{value}[/]"),
        CSharpTokenType.StringValue => new Markup($"[green]{value}[/]"),
        CSharpTokenType.Number => new Markup($"[magenta]{value}[/]"),
        CSharpTokenType.Operator => new Markup($"[yellow]{value}[/]"),
        CSharpTokenType.Comment => new Markup($"[grey]{value}[/]"),
        CSharpTokenType.Whitespace => new Markup($"[grey]{value}[/]"),
        _ => new Markup(value)
    };
    AnsiConsole.Write(colored);
});
```

### Using with TextReader

```csharp
using NTokenizers.CSharp;
using System.IO;

string csharpCode = "int x = 42;";
using var reader = new StringReader(csharpCode);
await CSharpTokenizer.Create().ParseAsync(reader, onToken: token =>
{
    Console.WriteLine($"Token: {token.TokenType} = '{token.Value}'");
});
```

### Parsing String Directly

```csharp
using NTokenizers.CSharp;

string csharpCode = "string name = \"John\";";
var tokens = CSharpTokenizer.Create().Parse(csharpCode);
foreach (var token in tokens)
{
    Console.WriteLine($"Token: {token.TokenType} = '{token.Value}'");
}
```

## Token Types

The C# tokenizer produces tokens of type `CSharpTokenType` with the following token types:

- `Keyword` - C# keywords like `class`, `var`, `if`, etc.
- `Identifier` - Variable names, method names, class names, etc.
- `StringValue` - String literals
- `Number` - Numeric literals
- `Operator` - Operators like `+`, `-`, `=`, etc.
- `Comment` - Single-line or multi-line comments
- `Whitespace` - Spaces, tabs, newlines, etc.
- `Character` - Character literals
- `Comment` - Comments
- and more...

More info: [CSharpTokenType.cs](https://github.com/crwsolutions/ntokenizers/blob/main/src/NTokenizers/CSharp/CSharpTokenType.cs)

## See Also

- [Markup Tokenizer](/ntokenizers/markup)
- [Home](/ntokenizers/)