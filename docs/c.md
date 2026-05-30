---
layout: default
title: C Tokenizer
nav: true
order: 11
---

# C Tokenizer

A fully streaming C tokenizer that processes source code character-by-character using a state machine approach. Designed for syntax highlighting applications.

## Features

- **Streaming processing**: Character-by-character parsing with no regular expressions
- **Complete token coverage**: All C token types including operators, keywords, literals, and comments
- **String and char literals**: Full support for escape sequences including `\\n`, `\\t`, `\\\\`, `\\\"`, `\\'`, `\\0`, `\\xHH`, `\\uXXXX`, `\\UXXXXXXXX`
- **Comments**: Line comments (`//` - C99+), block comments (`/* */`)
- **Preprocessor directives**: `#include`, `#define`, `#ifdef`, `#ifndef`, `#endif`, `#elif`, `#else`, `#undef`, `#pragma`
- **Numbers**: Decimal, hexadecimal (`0x`), octal (`0`), binary (`0b` - C23), scientific notation, suffixes (`U`, `L`, `UL`, `ULL`, `F`, `LF`)
- **Operators**: All C operators including `+=`, `-=`, `*=`, `/=`, `%=` , `<<`, `>>`, `&=`, `|=`, `^=`, `++`, `--`, `?:`, `->`
- **Struct/Union/Enum**: Full support for C data structure keywords

## Token Types

The C tokenizer recognizes the following token types:

| Token Type | Description | Example |
|---|---|---|
| `Operator` | Any operator | `+`, `-`, `*`, `/`, `==`, `!=`, `&&`, `||`, `->` |
| `OpenParenthesis` | Open parenthesis | `(` |
| `CloseParenthesis` | Close parenthesis | `)` |
| `OpenBrace` | Open brace | `{` |
| `CloseBrace` | Close brace | `}` |
| `OpenBracket` | Open bracket | `[` |
| `CloseBracket` | Close bracket | `]` |
| `Comma` | Comma | `,` |
| `Dot` | Dot | `.` |
| `Arrow` | Arrow | `->` |
| `SequenceTerminator` | Semicolon | `;` |
| `Colon` | Colon | `:` |
| `StringValue` | String literal | `"Hello, World!"` |
| `CharValue` | Character literal | `'a'` |
| `Number` | Numeric literal | `42`, `3.14`, `0xFF`, `077` |
| `Identifier` | Variable/function name | `myVariable`, `main` |
| `Keyword` | C keyword | `int`, `struct`, `void`, `return` |
| `Preprocessor` | Preprocessor directive | `#include`, `#define` |
| `Comment` | Comment | `// line`, `/* block */` |
| `Whitespace` | Whitespace | ` `, `\t`, `\n` |

## Usage

```csharp
using NTokenizers.C;

var tokenizer = CTokenizer.Create();
var tokens = tokenizer.Parse("int main(void) { return 0; }").ToList();

foreach (var token in tokens)
{
    Console.WriteLine($"{token.TokenType}: {token.Value}");
}
```

## Markdown Integration

The C tokenizer is integrated with the Markdown tokenizer. C code blocks in Markdown are automatically tokenized:

```csharp
using NTokenizers.Markdown;

var markdown = "# C Example\n\n```c\nint main(void) { return 0; }\n```";
var tokens = await MarkdownTokenizer.Create().TokenizeAsync(markdown);
```

## C Keywords

The tokenizer recognizes all standard C keywords:

- **Types and control flow**: `auto`, `break`, `case`, `char`, `const`, `continue`, `default`, `do`, `double`, `else`, `enum`, `extern`, `float`, `for`, `goto`, `if`, `inline`, `int`, `long`, `register`, `restrict`, `return`, `short`, `signed`, `sizeof`, `static`, `struct`, `switch`, `typedef`, `union`, `unsigned`, `void`, `volatile`, `while`
- **C99**: `_Bool`, `_Complex`, `_Imaginary`, `_Alignas`, `_Alignof`, `_Atomic`, `_Noreturn`, `_Static_assert`, `_Thread_local`, `bool`, `complex`, `imaginary`
- **C11**: `_Generic`, `_Pragma`
- **C23**: `_BitInt`, `_Bitalign`, `_Bitwidth`, `_Decimal128`, `_Decimal32`, `_Decimal64`, `_Float128`, `_Float16`, `_Float32`, `_Float64`, `_Float80`, `_Float8`, `_Math`, `_Prior`
