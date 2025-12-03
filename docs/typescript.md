---
layout: default
title: "Typescript"
---

# TypeScript Tokenizer

The TypeScript tokenizer is designed to parse TypeScript code and break it down into meaningful components (tokens) for processing. It provides stream-capable functionality for handling large TypeScript files or real-time code analysis.

## Overview

The TypeScript tokenizer is part of the NTokenizers library and provides a stream-capable approach to parsing TypeScript code. It can process TypeScript source code in real-time, making it suitable for large files or streaming scenarios where loading everything into memory at once is impractical.

## Public API

The TypeScript tokenizer inherits from `BaseSubTokenizer<TypescriptToken>` and provides the following key methods:

- `ParseAsync(Stream stream, Action<TypescriptToken> onToken)` - Asynchronously parses a stream of TypeScript code
- `Parse(Stream stream, Action<TypescriptToken> onToken)` - Synchronously parses a stream of TypeScript code
- `Parse(string input)` - Parses a string of TypeScript code and returns a list of tokens
- `ParseAsync(TextReader reader, Action<TypescriptToken> onToken)` - Asynchronously parses from a TextReader

## Usage Examples

### Basic Usage with Stream

```csharp
using NTokenizers.Typescript;
using Spectre.Console;
using System.Text;

string tsCode = """
    const user = {
        name: "Laura Smith",
        active: true
    };
    """;

using var stream = new MemoryStream(Encoding.UTF8.GetBytes(tsCode));
await TypescriptTokenizer.Create().ParseAsync(stream, onToken: token =>
{
    var value = Markup.Escape(token.Value);
    var colored = token.TokenType switch
    {
        TypescriptTokenType.Keyword => new Markup($"[blue]{value}[/]"),
        TypescriptTokenType.Identifier => new Markup($"[cyan]{value}[/]"),
        TypescriptTokenType.StringValue => new Markup($"[green]{value}[/]"),
        TypescriptTokenType.Number => new Markup($"[magenta]{value}[/]"),
        TypescriptTokenType.Operator => new Markup($"[yellow]{value}[/]"),
        TypescriptTokenType.Comment => new Markup($"[grey]{value}[/]"),
        TypescriptTokenType.Whitespace => new Markup($"[grey]{value}[/]"),
        _ => new Markup(value)
    };
    AnsiConsole.Write(colored);
});
```

### Using with TextReader

```csharp
using NTokenizers.Typescript;
using System.IO;

string tsCode = "let x: number = 42;";
using var reader = new StringReader(tsCode);
await TypescriptTokenizer.Create().ParseAsync(reader, onToken: token =>
{
    Console.WriteLine($"Token: {token.TokenType} = '{token.Value}'");
});
```

### Parsing String Directly

```csharp
using NTokenizers.Typescript;

string tsCode = "const name: string = \"John\";";
var tokens = TypescriptTokenizer.Create().Parse(tsCode);
foreach (var token in tokens)
{
    Console.WriteLine($"Token: {token.TokenType} = '{token.Value}'");
}
```

### Use Processed stream as string

```csharp
using NTokenizers.Typescript;
using System.Text;

string tsCode = "const x = 42;";
var processedString = await TypescriptTokenizer.Create().ParseAsync(tsCode, token =>
{
    return token.TokenType switch
    {
        TypescriptTokenType.Keyword => $"[blue]{token.Value}[/]",
        TypescriptTokenType.Identifier => $"[cyan]{token.Value}[/]",
        TypescriptTokenType.StringValue => $"[green]{token.Value}[/]",
        TypescriptTokenType.Number => $"[magenta]{token.Value}[/]",
        TypescriptTokenType.Operator => $"[yellow]{token.Value}[/]",
        TypescriptTokenType.Comment => $"[grey]{token.Value}[/]",
        _ => token.Value
    };
});
Console.WriteLine(processedString);
```

## Token Types

The TypeScript tokenizer produces tokens of type `TypescriptTokenType` with the following token types:

- `NotDefined` - Token that is not defined or recognized
- `And` - Logical AND operator (`&&`)
- `Application` - Application-specific token
- `Between` - Between operator or keyword
- `CloseParenthesis` - Closing parenthesis (`)`)
- `Comma` - Comma separator (`,`)
- `DateTimeValue` - DateTime value literal
- `Equals` - Equality operator (`==` or `===`)
- `ExceptionType` - Exception type identifier
- `Fingerprint` - Fingerprint identifier
- `In` - 'in' operator or keyword
- `Invalid` - Invalid token
- `Like` - 'like' operator
- `Limit` - 'limit' keyword
- `Match` - 'match' keyword
- `Message` - Message identifier
- `NotEquals` - Inequality operator (`!=` or `!==`)
- `NotIn` - 'not in' operator
- `NotLike` - 'not like' operator
- `Number` - Numeric literal
- `Or` - Logical OR operator (`||`)
- `OpenParenthesis` - Opening parenthesis (`(`)
- `StackFrame` - Stack frame identifier
- `StringValue` - String literal value
- `SequenceTerminator` - Semicolon (`;`) statement terminator
- `Identifier` - Identifier (variable, function name, etc.)
- `Keyword` - TypeScript keyword
- `Comment` - Comment (line or block)
- `Operator` - Operator (`+`, `-`, `*`, `/`, `%`, etc.)
- `Dot` - Dot operator (`.`)
- `Whitespace` - Whitespace characters

More info: [TypescriptTokenType.cs](https://github.com/crwsolutions/ntokenizers/blob/main/src/NTokenizers/Typescript/TypescriptTokenType.cs)

## See Also

- [C# Tokenizer](/ntokenizers/csharp)
- [Home](/ntokenizers/)
