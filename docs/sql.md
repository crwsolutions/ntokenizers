---
layout: default
title: "Sql"
---

# SQL Tokenizer

The SQL tokenizer is designed to parse SQL code and break it down into meaningful components (tokens) for processing. It provides stream-capable functionality for handling large SQL files or real-time SQL statement analysis.

## Overview

The SQL tokenizer is part of the NTokenizers library and provides a stream-capable approach to parsing SQL code. It can process SQL source code in real-time, making it suitable for large files or streaming scenarios where loading everything into memory at once is impractical.

## Public API

The SQL tokenizer inherits from `BaseSubTokenizer<SqlToken>` and provides the following key methods:

- `ParseAsync(Stream stream, Action<SqlToken> onToken)` - Asynchronously parses a stream of SQL code
- `Parse(Stream stream, Action<SqlToken> onToken)` - Synchronously parses a stream of SQL code
- `Parse(string input)` - Parses a string of SQL code and returns a list of tokens
- `ParseAsync(TextReader reader, Action<SqlToken> onToken)` - Asynchronously parses from a TextReader

## Usage Examples

### Basic Usage with Stream

```csharp
using NTokenizers.Sql;
using Spectre.Console;
using System.Text;

string sqlCode = """
    SELECT name, age
    FROM users
    WHERE active = true
    ORDER BY name;
    """;

using var stream = new MemoryStream(Encoding.UTF8.GetBytes(sqlCode));
await SqlTokenizer.Create().ParseAsync(stream, onToken: token =>
{
    var value = Markup.Escape(token.Value);
    var colored = token.TokenType switch
    {
        SqlTokenType.Keyword => new Markup($"[blue]{value}[/]"),
        SqlTokenType.Identifier => new Markup($"[cyan]{value}[/]"),
        SqlTokenType.StringValue => new Markup($"[green]{value}[/]"),
        SqlTokenType.Number => new Markup($"[magenta]{value}[/]"),
        SqlTokenType.Operator => new Markup($"[yellow]{value}[/]"),
        SqlTokenType.Comment => new Markup($"[grey]{value}[/]"),
        SqlTokenType.Whitespace => new Markup($"[grey]{value}[/]"),
        _ => new Markup(value)
    };
    AnsiConsole.Write(colored);
});
```

### Using with TextReader

```csharp
using NTokenizers.Sql;
using System.IO;

string sqlCode = "SELECT * FROM users WHERE id = 42;";
using var reader = new StringReader(sqlCode);
await SqlTokenizer.Create().ParseAsync(reader, onToken: token =>
{
    Console.WriteLine($"Token: {token.TokenType} = '{token.Value}'");
});
```

### Parsing String Directly

```csharp
using NTokenizers.Sql;

string sqlCode = "INSERT INTO users (name) VALUES ('John');";
var tokens = SqlTokenizer.Create().Parse(sqlCode);
foreach (var token in tokens)
{
    Console.WriteLine($"Token: {token.TokenType} = '{token.Value}'");
}
```

### Use Processed stream as string

```csharp
using NTokenizers.Sql;
using System.Text;

string sqlCode = "SELECT COUNT(*) FROM users;";
var processedString = await SqlTokenizer.Create().ParseAsync(sqlCode, token =>
{
    return token.TokenType switch
    {
        SqlTokenType.Keyword => $"[blue]{token.Value}[/]",
        SqlTokenType.Identifier => $"[cyan]{token.Value}[/]",
        SqlTokenType.StringValue => $"[green]{token.Value}[/]",
        SqlTokenType.Number => $"[magenta]{token.Value}[/]",
        SqlTokenType.Operator => $"[yellow]{token.Value}[/]",
        SqlTokenType.Comment => $"[grey]{token.Value}[/]",
        _ => token.Value
    };
});
Console.WriteLine(processedString);
```

## Token Types

The SQL tokenizer produces tokens of type `SqlTokenType` with the following token types:

- `StringValue` - Represents a string value enclosed in single or double quotes
- `Number` - Represents a numeric value (integer or decimal)
- `Keyword` - Represents a SQL keyword (case-insensitive) like SELECT, FROM, WHERE, etc.
- `Identifier` - Represents an identifier (table name, column name, etc.)
- `Operator` - Represents an operator (comparison, arithmetic, logical, etc.)
- `Comma` - Represents a comma (`,`)
- `Dot` - Represents a dot (`.`)
- `OpenParenthesis` - Represents an opening parenthesis (`(`)
- `CloseParenthesis` - Represents a closing parenthesis (`)`)
- `SequenceTerminator` - Represents a sequence terminator (`;`)
- `NotDefined` - Represents a token that doesn't fit into any other category
- `Comment` - Represents a comment (inline `--` or block `/* */`)
- `Whitespace` - Represents whitespace characters between tokens

More info: [SqlTokenType.cs](https://github.com/crwsolutions/ntokenizers/blob/main/src/NTokenizers/Sql/SqlTokenType.cs)

## See Also

- [C# Tokenizer](/ntokenizers/csharp)
- [Home](/ntokenizers/)
