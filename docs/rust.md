---
title: Rust Tokenizer
---

# Rust Tokenizer

The Rust tokenizer provides streaming, character-by-character parsing of Rust source code. It supports modern Rust features including async/await, lifetimes, pattern matching, and macros.

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
| `FatArrow` | `=>` |
| `SequenceTerminator` | `;` |
| `Colon` / `DoubleColon` | `:` and `::` |
| `At` | `@` |
| `Pound` | `#` |
| `QuestionMark` | `?` |
| `StringValue` | String literals including raw strings |
| `CharValue` | Character literals |
| `Number` | Integer, float, hex, octal, binary with suffixes |
| `Boolean` | `true`, `false` |
| `Identifier` | Variable and function names |
| `Keyword` | Rust keywords |
| `Macro` | Macro invocations |
| `Lifetime` | `'a`, `'static` |
| `Comment` | Line and block comments |
| `Whitespace` | Whitespace characters |

## Usage

```csharp
using NTokenizers.Rust;

var tokenizer = RustTokenizer.Create();
var tokens = await tokenizer.Parse(code);

foreach (var token in tokens)
{
    Console.WriteLine($"[{token.TokenType}] {token.Value}");
}
```

## Features

- **Streaming parsing**: Processes input character by character
- **Lifetimes**: Full support for lifetime annotations
- **Async/await**: Modern Rust async syntax
- **Pattern matching**: `match` expressions with arms
- **Macros**: `println!()`, `vec![]`, `macro_rules!`
- **Generics**: With trait bounds and lifetime parameters
- **Modern Rust**: Rust 2021 edition features

## Markdown Integration

```csharp
using NTokenizers.Markdown;

var tokenizer = MarkdownTokenizer.Create();
var tokens = await tokenizer.Parse(markdownWithRustCode);
```

The Rust tokenizer is automatically used for code blocks marked with `rust` or `rs`.
