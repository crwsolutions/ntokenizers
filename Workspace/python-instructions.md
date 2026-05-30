# Python Tokenizer Implementatie

Implementeer een **streaming Python tokenizer** in C# die volledig **karakter-voor-karakter** werkt met een **state machine**, en die kan stoppen bij een optionele `stopDelimiter`. Reuse code from BaseSubTokenizer.

> **Doel:** Goed genoeg voor syntax highlighting, niet perfecte parsing. Best-effort, no strict validation.

---

## Public API

The public API is handled by `BaseSubTokenizer<PythonToken>`, so make sure to inherit from this class:

```csharp
public sealed class PythonTokenizer : BaseSubTokenizer<PythonToken>
```

The BaseSubTokenizer contains an abstract method, so make sure to override that one:

```csharp
internal protected override Task ParseAsync(CancellationToken ct)
```

There is also already a method available that handles the stopDelimiter logic. So override as follows:

```csharp
internal protected override Task ParseAsync(CancellationToken ct)
{
    var state = new State();
    /* define additional state variables here (e.g., char? stringDelimiter) */

    TokenizeCharacters(ct, (c) => ProcessChar(c, state, ref /* extra state variables */));

    EmitPending(state);

    return Task.CompletedTask;
}
```

> **Belangrijk:** Gebruik een `private sealed class State` (niet een enum!) voor alle state tracking. Dit is een object met boolean properties - geen losse boolean variabelen op tokenizer niveau, en geen enum. Voorbeeld:
> ```csharp
> private sealed class State
> {
>     public bool InWhitespace;
>     public bool InIdentifier;
>     public bool InNumber;
>     public bool InString;
>     public bool InComment;
>     public bool InOperator;
> }
> ```

---

## Gedrag

1. Lees de input karakter-voor-karakter van een `TextReader`.
2. Herken en emiteer tokens direct via `_onToken` zodra ze volledig herkend zijn.
3. Lees alleen vooruit als het echt nodig is.
4. Parse best-effort, no strict validation.

---

## Implementatie details

Neem `CSharpTokenizer` als voorbeeld - Python deelt veel syntaxis concepten.

### Doel:
- Fully streaming Python tokenizer
- Matches the style of the CSharp tokenizer in the repo
- Goed genoeg voor syntax highlighting

### Token types

```csharp
public enum PythonTokenType
{
    NotDefined,
    
    // Operators
    Operator,     // +, -, *, /, //, %, **, =, +=, -=, *=, /=, //=, %=, **=, ==, !=, <, >, <=, >=, and, or, not, in, is, :=, @, &, |, ^, ~, <<, >>, &=, |=, ^=, <<=, >>=
    
    // Punctuation
    OpenParenthesis,    // (
    CloseParenthesis,   // )
    OpenBrace,          // {
    CloseBrace,         // }
    OpenBracket,        // [
    CloseBracket,       // ]
    Comma,              // ,
    Dot,                // .
    Colon,              // :
    Semicolon,          // ;
    Hash,               // @ (decorator)
    
    // Literals
    StringValue,  // '...', "...", '''...''', """...""", f'...', f"...", r'...', b"...", etc.
    Number,       // integers, floats, hex (0x), binary (0b), octal (0o), complex (1+2j)
    
    // Identifiers
    Identifier,   // variable names, function names, etc.
    Keyword,      // Python keywords
    
    // Comments
    Comment,      // # line comments
    
    // Whitespace
    Whitespace,
}
```

### Python keywords (voor Keyword detectie)

```csharp
private static readonly HashSet<string> Keywords = new(StringComparer.Ordinal)
{
    // Flow control
    "False", "None", "True", "and", "as", "assert", "async", "await", "break",
    "class", "continue", "def", "del", "elif", "else", "except", "finally",
    "for", "from", "global", "if", "import", "in", "is", "lambda", "nonlocal",
    "not", "or", "pass", "raise", "return", "try", "while", "with", "yield",
};
```

### Specifieke Python features om te ondersteunen

> **Belangrijk:** Geen gebruik van regular expressions in de tokenizer - alles moet met karakter-voor-karakter state machine parsing werken.

1. **Strings:** 
   - Single: `'...'`
   - Double: `"..."`
   - Triple single: `'''...'''`
   - Triple double: `"""..."""`
   - Raw strings: `r'...'`, `r"..."`
   - Byte strings: `b'...'`, `b"..."`
   - F-strings: `f'...'`, `f"..."` (met `{expressies}`)
   - Escape characters: `\n`, `\t`, `\\`, `\"`, `\'`, `\0`, `\xHH`, `\uXXXX`, `\UXXXXXXXX`

2. **Numbers:** 
   - Decimal: `42`, `3.14`
   - Hex: `0xFF`
   - Binary: `0b1010`
   - Octal: `0o77`
   - Complex: `1+2j`, `3j`
   - Underscores: `1_000_000`

3. **Comments:** `#` line comments only (Python heeft geen block comments)

4. **Operators:** 
   - Arithmetic: `+`, `-`, `*`, `/`, `//`, `%`, `**`
   - Assignment: `=`, `+=`, `-=`, `*=`, `/=`, `//=`, `%=`, `**=`, `&=`, `|=`, `^=`, `<<=`, `>>=`
   - Comparison: `==`, `!=`, `<`, `>`, `<=`, `>=`
   - Logical: `and`, `or`, `not`
   - Membership: `in`, `not in`
   - Identity: `is`, `is not`
   - Walrus: `:=`
   - Bitwise: `&`, `|`, `^`, `~`, `<<`, `>>`
   - Matrix multiplication: `@`

5. **Decorators:** `@decorator` syntax

6. **Type hints:** `def func(x: int) -> str:`

### Bestanden aanmaken

1. `src/NTokenizers/Python/PythonTokenType.cs`
2. `src/NTokenizers/Python/PythonToken.cs`
3. `src/NTokenizers/Python/PythonTokenizer.cs`
4. `src/NTokenizers/Python/PythonCodeBlockMetadata.cs`

---

## MarkdownTokenizer update

**BELANGRIJK:** Update de `ParseCodeInlines(string language)` methode in `src/NTokenizers/Markdown/MarkdownTokenizer.cs`:

Voeg een case toe aan de switch-expression:

```csharp
"python" or "py" => await ParseCodeInlines(new PythonCodeBlockMetadata(language)),
```

Zorg ook voor de `using` statement bovenaan het bestand:

```csharp
using NTokenizers.Python;
```

---

## xUnit tests

- Plaats tests in `tests/NTokenizers.Tests/PythonTokenizerTests.cs`
- **Elke token type uit de enum moet minstens 1 keer getest worden**
- **Minimaal 2 complexe multi-regel tests (5-7 regels code)**
- **Test of MarkdownTokenizer de Python subtokenizer correct gebruikt** (code fence met ` ```python `)
- Alles moet compileren en alle tests moeten slagen

Test cases:
  - Simple function definition
  - Class definition with type hints
  - String literals (single, double, triple quotes)
  - F-strings with expressions
  - Numbers (decimal, hex, binary, octal, complex)
  - Comments (line only)
  - Operators (arithmetic, assignment, comparison, logical)
  - Decorators (@decorator)
  - Type hints (def func(x: int) -> str:)
  - Keywords vs identifiers
  - **Complex test 1:** Multi-regel class met f-strings en type hints
  - **Complex test 2:** Function with decorators, type hints, and complex return statement
  - **MarkdownTokenizer test:** Parse markdown met Python code block, verify tokens via subtokenizer
  - **Newline preservation test:** Test dat newlines correct als whitespace tokens worden doorgegeven

---

## Showcase project

- `tests/NTokenizers.ShowCase.Python/Program.cs` - Simple Python example met syntax highlighting
- `tests/NTokenizers.ShowCase.Python/NTokenizers.ShowCase.Python.csproj`

Volg het C# showcase patroon met Spectre.Console:
- Keywords: blauw
- Identifiers: cyaan
- Strings: groen
- Numbers: magenta
- Operators: geel
- Comments: grijs
- Whitespace: grijs

---

## Doc file

- `docs/python.md` - Documentatie van de Python tokenizer

---

## csproj update

Update `src/NTokenizers/NTokenizers.csproj`:
- Description field: "Python" toevoegen aan de lijst
- PackageTags: "python" toevoegen

---

## Markdown showcase update

Update `tests/NTokenizers.ShowCase.Markdown/Program.cs`:
- Voeg een Python code block toe met 3-6 regels voorbeeld code
- Voeg een token handler toe voor PythonCodeBlockMetadata

Voorbeeld code block:
```python
def greet(name: str) -> str:
    return f"Hello, {name}!"

# Usage
message = greet("World")
```

---

## Common Pitfalls voor Python

1. **Triple quotes:** `'''` en `"""` zijn multi-line strings - zorg dat je correct herkent wanneer ze eindigen
2. **F-strings:** `f"..."` bevatten `{expressies}` die geen strings zijn
3. **Raw strings:** `r'...'` behandelen backslashes niet als escape characters
4. **Complex numbers:** `1+2j` is één token, niet drie
5. **Walrus operator:** `:=` is één operator, niet twee
6. **Newlines:** Python is newline-sensitive - zorg dat newlines correct als whitespace tokens worden doorgegeven
7. **Indentation:** Voor syntax highlighting hoeven we indentation niet te valideren, maar het moet wel als whitespace worden behandeld
