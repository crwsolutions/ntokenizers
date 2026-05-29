---
title: Swift Tokenizer
---

# Swift Tokenizer

The Swift tokenizer provides streaming, character-by-character parsing of Swift source code. It supports Swift features including optionals, closures, protocol extensions, and pattern matching.

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
| `StringValue` | String literals including multi-line strings |
| `CharValue` | Character literals |
| `Number` | Integer, float, hex, octal, binary with suffixes |
| `Boolean` | `true`, `false` |
| `Null` | `nil` |
| `Identifier` | Variable and function names |
| `Keyword` | Swift keywords |
| `Comment` | Line and block comments |
| `Whitespace` | Whitespace characters |

## Usage

```csharp
using NTokenizers.Swift;

var tokenizer = SwiftTokenizer.Create();
var tokens = tokenizer.Parse(code);
```

## Features

- **Streaming parsing**: Processes input character by character
- **Optionals**: `Type?` and `Type!` optional type annotations
- **Closures**: Swift closure expressions
- **Pattern matching**: `switch` with pattern matching
- **Protocol extensions**: `protocol` and `extension` support
- **Guard statements**: `guard let` and `guard case`

## Markdown Integration

```csharp
using NTokenizers.Markdown;

var tokenizer = MarkdownTokenizer.Create();
var tokens = await tokenizer.Parse(markdownWithSwiftCode);
```

The Swift tokenizer is automatically used for code blocks marked with `swift`.
