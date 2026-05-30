# Python Tokenizer

The Python tokenizer provides character-by-character streaming tokenization for Python source code.

## Supported Token Types

- **Keywords**: `def`, `class`, `if`, `for`, `while`, `return`, `import`, `from`, `async`, `await`, `try`, `except`, `finally`, `with`, `yield`, `lambda`, `pass`, `break`, `continue`, `raise`, `del`, `global`, `nonlocal`, `assert`, `is`, `in`, `not`, `and`, `or`, `as`, `elif`, `else`, `True`, `False`, `None`
- **Operators**: `+`, `-`, `*`, `/`, `//`, `%`, `**`, `=`, `==`, `!=`, `<`, `>`, `<=`, `>=`, `:=`, `+=`, `-=`, `*=`, `/=`, `//=`, `%=`, `**=`, `&`, `|`, `^`, `~`, `<<`, `>>`, `&=`, `|=`, `^=`, `<<=`, `>>=`, `@`
- **Punctuation**: `(`, `)`, `{`, `}`, `[`, `]`, `,`, `.`, `:`, `;`
- **Strings**: Single quotes `'...'`, double quotes `"..."`, triple quotes `'''...'''`, `"""..."""`, f-strings, raw strings, byte strings
- **Numbers**: Integers, floats, hex (`0x`), binary (`0b`), octal (`0o`), complex (`3j`), scientific notation, underscores (`1_000_000`)
- **Comments**: `#` line comments
- **Decorators**: `@decorator` syntax
- **Type hints**: `def func(x: int) -> str:`

## Usage

```csharp
using NTokenizers.Python;

var tokenizer = PythonTokenizer.Create();
var tokens = tokenizer.Parse("def greet(name: str) -> str: return f\"Hello, {name}!\"");

foreach (var token in tokens)
{
    Console.WriteLine($"{token.TokenType}: {token.Value}");
}
```

## Streaming

```csharp
await PythonTokenizer.Create().ParseAsync(stream, onToken: token =>
{
    // Handle tokens as they arrive
});
```

## Markdown Integration

Python code blocks are automatically recognized in Markdown:

````markdown
```python
def greet(name: str) -> str:
    return f"Hello, {name}!"
```
````

The MarkdownTokenizer will use the Python tokenizer to tokenize the code block content.

## Limitations

- F-strings with nested expressions are tokenized best-effort
- Complex numbers like `1+2j` may be split into multiple tokens
- String prefixes (`r`, `b`, `f`, `u`) are tokenized as separate identifiers
