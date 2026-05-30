---
title: C++ Tokenizer
---

# C++ Tokenizer

The C++ tokenizer provides streaming, character-by-character parsing of C++ source code. It supports modern C++ features including templates, lambdas, smart pointers, and range-based for loops.

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
| `QuestionMark` | `?` |
| `StringValue` | String literals including wide and raw strings |
| `CharValue` | Character literals |
| `Number` | Integer, float, hex, octal, binary with suffixes |
| `Boolean` | `true`, `false` |
| `Null` | `nullptr` |
| `Identifier` | Variable and function names |
| `Keyword` | C++ keywords |
| `Comment` | Line and block comments |
| `Whitespace` | Whitespace characters |

## Usage

```csharp
using NTokenizers.Cpp;

var tokenizer = CppTokenizer.Create();
var tokens = await tokenizer.Parse(code);

foreach (var token in tokens)
{
    Console.WriteLine($"[{token.TokenType}] {token.Value}");
}
```

## Features

- **Streaming parsing**: Processes input character by character
- **Templates**: Full support for template syntax
- **Lambdas**: C++ lambda expressions with captures
- **Smart pointers**: `unique_ptr`, `shared_ptr`, `make_unique`, `make_shared`
- **Range-based for loops**: Modern C++ iteration syntax
- **Preprocessor directives**: `#include`, `#define`, `#ifdef`, etc.
- **Modern C++**: C++20/23 features including `co_await`, `co_return`, `co_yield`, `concept`, `consteval`, `constexpr`

## Markdown Integration

```csharp
using NTokenizers.Markdown;

var tokenizer = MarkdownTokenizer.Create();
var tokens = await tokenizer.Parse(markdownWithCppCode);
```

The C++ tokenizer is automatically used for code blocks marked with `cpp` or `c++`.
