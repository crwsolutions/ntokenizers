---
title: Kotlin Tokenizer
---

# Kotlin Tokenizer

The Kotlin tokenizer provides streaming, character-by-character parsing of Kotlin source code. It supports modern Kotlin features including data classes, lambdas, nullable types, and when expressions.

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
| `Number` | Integer, float, hex, binary with suffixes |
| `Boolean` | `true`, `false` |
| `Null` | `null` |
| `Identifier` | Variable and function names |
| `Keyword` | Kotlin keywords |
| `Comment` | Line and block comments |
| `Whitespace` | Whitespace characters |

## Usage

```csharp
using NTokenizers.Kotlin;

var tokenizer = KotlinTokenizer.Create();
var tokens = tokenizer.Parse(code);
```

## Features

- **Streaming parsing**: Processes input character by character
- **Data classes**: Full support for data class syntax
- **Lambdas**: Kotlin lambda expressions
- **Nullable types**: `Type?` nullable type annotations
- **When expressions**: Pattern matching with when
- **String templates**: `$var` and `${expression}` interpolation

## Markdown Integration

```csharp
using NTokenizers.Markdown;

var tokenizer = MarkdownTokenizer.Create();
var tokens = await tokenizer.Parse(markdownWithKotlinCode);
```

The Kotlin tokenizer is automatically used for code blocks marked with `kotlin` or `kt`.