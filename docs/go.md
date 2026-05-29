---
title: Go Tokenizer
---

# Go Tokenizer

The Go tokenizer provides streaming, character-by-character parsing of Go source code. It supports Go features including goroutines, channels, maps, and structs.

## Supported Token Types

| Token Type | Description |
|---|---|
| `Operator` | Arithmetic, bitwise, comparison, logical, and compound assignment operators |
| `OpenParenthesis` / `CloseParenthesis` | `(` and `)` |
| `OpenBrace` / `CloseBrace` | `{` and `}` |
| `OpenBracket` / `CloseBracket` | `[` and `]` |
| `Comma` | `,` |
| `Dot` | `.` |
| `Arrow` | `->` |
| `SequenceTerminator` | `;` |
| `Colon` / `DoubleColon` | `:` and `::` |
| `At` | `@` |
| `Pound` | `#` |
| `QuestionMark` | `?` |
| `StringValue` | String literals including raw strings |
| `CharValue` | Character literals |
| `Number` | Integer, float, hex, octal, binary with suffixes |
| `Boolean` | `true`, `false` |
| `Null` | `nil` |
| `Identifier` | Variable and function names |
| `Keyword` | Go keywords |
| `Comment` | Line and block comments |
| `Whitespace` | Whitespace characters |

## Usage

```csharp
using NTokenizers.Go;

var tokenizer = GoTokenizer.Create();
var tokens = tokenizer.Parse(code);
```

## Features

- **Streaming parsing**: Processes input character by character
- **Goroutines**: `go` keyword support
- **Channels**: `chan` type and channel operations
- **Maps**: `map` type declarations
- **Structs**: Full struct support with fields
- **Short variable declarations**: `:=` operator

## Markdown Integration

```csharp
using NTokenizers.Markdown;

var tokenizer = MarkdownTokenizer.Create();
var tokens = await tokenizer.Parse(markdownWithGoCode);
```

The Go tokenizer is automatically used for code blocks marked with `go` or `golang`.
