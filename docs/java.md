---
layout: default
title: Java Tokenizer
nav: true
order: 10
---

# Java Tokenizer

A fully streaming Java tokenizer that processes source code character-by-character using a state machine approach. Designed for syntax highlighting applications.

## Features

- **Streaming processing**: Character-by-character parsing with no regular expressions
- **Complete token coverage**: All Java token types including operators, keywords, literals, and comments
- **String and char literals**: Full support for escape sequences including `\\n`, `\\t`, `\\\\`, `\\\"`, `\\'`, `\\uXXXX`
- **Comments**: Line comments (`//`), block comments (`/* */`), and Javadoc comments (`/** */`)
- **Numbers**: Decimal, hexadecimal (`0x`), octal (`0`), binary (`0b`), scientific notation, and suffixes (`L`, `F`, `D`)
- **Operators**: All Java operators including `+=`, `-=`, `*=`, `/=`, `%=` , `<<`, `>>`, `>>>`, `++`, `--`, `?:`, `->` (lambdas)
- **Generics**: Full support for generic type parameters
- **Annotations**: Support for Java annotations (`@Override`, `@SuppressWarnings`, etc.)

## Token Types

The Java tokenizer recognizes the following token types:

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
| `SequenceTerminator` | Semicolon | `;` |
| `Colon` | Colon | `:` |
| `StringValue` | String literal | `"Hello, World!"` |
| `CharValue` | Character literal | `'a'` |
| `Number` | Numeric literal | `42`, `3.14`, `0xFF`, `0b1010` |
| `Boolean` | Boolean literal | `true`, `false` |
| `Null` | Null literal | `null` |
| `Identifier` | Variable/method name | `myVariable`, `main` |
| `Keyword` | Java keyword | `public`, `class`, `static`, `void` |
| `Comment` | Comment | `// line`, `/* block */` |
| `Whitespace` | Whitespace | ` `, `\t`, `\n` |

## Usage

```csharp
using NTokenizers.Java;

var tokenizer = JavaTokenizer.Create();
var tokens = tokenizer.Tokenize("public class Main { }").ToList();

foreach (var token in tokens)
{
    Console.WriteLine($"{token.TokenType}: {token.Value}");
}
```

## Markdown Integration

The Java tokenizer is integrated with the Markdown tokenizer. Java code blocks in Markdown are automatically tokenized:

```csharp
using NTokenizers.Markdown;

var markdown = "# Java Example\n\n```java\npublic class Main { }\n```";
var tokens = await MarkdownTokenizer.Create().TokenizeAsync(markdown);
```

## Java Keywords

The tokenizer recognizes all standard Java keywords:

- **Control flow**: `abstract`, `assert`, `break`, `case`, `catch`, `continue`, `default`, `do`, `else`, `finally`, `for`, `if`, `instanceof`, `new`, `return`, `switch`, `throw`, `throws`, `try`, `while`
- **Types**: `boolean`, `byte`, `char`, `double`, `float`, `int`, `long`, `short`, `void`
- **Modifiers**: `class`, `enum`, `extends`, `final`, `implements`, `import`, `interface`, `native`, `package`, `private`, `protected`, `public`, `static`, `strictfp`, `super`, `synchronized`, `this`, `transient`, `volatile`
- **Literals**: `false`, `null`, `true`
- **Modern Java**: `var`, `record`, `sealed`, `permits`, `yield`
