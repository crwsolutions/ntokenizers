# Go Tokenizer Implementatie

Implementeer een **streaming Go tokenizer** in C# die volledig **karakter-voor-karakter** werkt met een **state machine**, en die kan stoppen bij een optionele `stopDelimiter`. Reuse code from BaseSubTokenizer.

> **Doel:** Goed genoeg voor syntax highlighting, niet perfecte parsing. Best-effort, no strict validation.

---

## Public API

The public API is handled by `BaseSubTokenizer<GoToken>`, so make sure to inherit from this class:

```csharp
public sealed class GoTokenizer : BaseSubTokenizer<GoToken>
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

> **Belangrijk:** Gebruik een `private sealed class State` (niet een enum!) voor alle state tracking. Dit is een object met boolean properties - geen losse boolean variabelen op tokenizer niveau, en geen enum. Geef het object door (geen `ref` nodig voor objects). Voorbeeld:
> ```csharp
> private sealed class State
> {
>     public bool InWhitespace;
>     public bool InIdentifier;
>     public bool InNumber;
>     public bool InString;
>     public bool InStringEscape;
>     public bool InChar;
>     public bool InCommentLine;
>     public bool InCommentBlock;
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

Neem `CSharpTokenizer` als voorbeeld - Go deelt syntaxis met C-family talen.

### Doel:
- Fully streaming Go tokenizer
- Matches the style of the CSharp tokenizer in the repo
- Goed genoeg voor syntax highlighting

### Token types

```csharp
public enum GoTokenType
{
    NotDefined,
    
    // Operators
    Operator,     // &&, ||, !, ==, !=, >, <, >=, <=, +, -, *, /, %, &=, |=, ^=, <<, >>, +=, -=, *=, /=, %=, :=, <-, &, |, ^
    
    // Punctuation
    OpenParenthesis,    // (
    CloseParenthesis,   // )
    OpenBrace,          // {
    CloseBrace,         // }
    OpenBracket,        // [
    CloseBracket,       // ]
    Comma,              // ,
    Dot,                // .
    SequenceTerminator, // ;
    Colon,              // :
    
    // Literals
    StringValue,  // "..." or `...` raw strings
    CharValue,    // '...' (rune literals)
    Number,       // integer, float, complex, hex, octal, binary
    Boolean,      // true, false
    Null,         // nil, iota
    
    // Identifiers
    Identifier,   // variable names, function names, etc.
    Keyword,      // Go keywords
    Builtin,      // built-in functions: len, cap, append, make, etc.
    
    // Comments
    Comment,      // // or /* */
    
    // Whitespace
    Whitespace,
    
    // Generic operator fallback
    Operator,
}
```

### Go keywords (voor Keyword detectie)

```csharp
private static readonly HashSet<string> Keywords = new(StringComparer.Ordinal)
{
    // Keywords
    "break", "case", "chan", "const", "continue", "default", "defer",
    "else", "fallthrough", "for", "func", "go", "goto", "if", "import",
    "interface", "map", "package", "range", "return", "select", "struct",
    "switch", "type", "var",
    
    // Predeclared identifiers (often treated as keywords)
    "append", "bool", "byte", "cap", "close", "complex", "complex64",
    "complex128", "copy", "error", "false", "float32", "float64",
    "imag", "int", "int8", "int16", "int32", "int64", "iota", "len",
    "make", "new", "nil", "panic", "print", "println", "real", "recover",
    "rune", "string", "true", "uint", "uint8", "uint16", "uint32",
    "uint64", "uintptr", "unsafe",
};
```

### Specifieke Go features om te ondersteunen

> **Belangrijk:** Geen gebruik van regular expressions in de tokenizer - alles moet met karakter-voor-karakter state machine parsing werken.

1. **Strings:** `"..."` met escape characters, raw strings `` `...` ``
2. **Runes:** `'...'` (rune literals - Go's term for char)
3. **Comments:** `//` line comments, `/* */` block comments
4. **Numbers:** decimal, hex (`0x`), octal (`0`), binary (`0b`), scientific notation, complex numbers (`1+2i`)
5. **Operators:** inclusief `+=`, `-=`, `*=`, `/=`, `%=` , `&=`, `|=`, `^=`, `<<=`, `>>=`, `:=` (short declaration)
6. **Channels:** `<-` channel operator
7. **Struct/Interface:** `struct`, `interface`, `map`, `chan` type keywords
8. **Defer/Go:** `defer`, `go` keywords voor concurrency

### Bestanden aanmaken

1. `src/NTokenizers/Go/GoTokenType.cs`
2. `src/NTokenizers/Go/GoToken.cs`
3. `src/NTokenizers/Go/GoTokenizer.cs`
4. `src/NTokenizers/Go/GoCodeBlockMetadata.cs`

---

## MarkdownTokenizer update

**BELANGRIJK:** Update de `ParseCodeInlines(string language)` methode in `src/NTokenizers/Markdown/MarkdownTokenizer.cs`:

Voeg een case toe aan de switch-expression (lijn 308-320):

```csharp
"go" or "golang" => await ParseCodeInlines(new GoCodeBlockMetadata(language)),
```

Zorg ook voor de `using` statement bovenaan het bestand:

```csharp
using NTokenizers.Go;
```

---

## xUnit tests

- Plaats tests in `tests/NTokenizers.Tests/GoTokenizerTests.cs`
- **Elke token type uit de enum moet minstens 1 keer getest worden**
- **Minimaal 2 complexe multi-regel tests (5-7 regels code)**
- **Test of MarkdownTokenizer de Go subtokenizer correct gebruikt** (code fence met ` ```go `)
- Alles moet compileren en alle tests moeten slagen

Test cases:
  - Simple function declaration
  - Struct definition
  - Interface definition
  - String literals (regular, raw)
  - Rune literals
  - Numbers (decimal, hex, binary, complex)
  - Comments (line, block)
  - Operators (arithmetic, bitwise, comparison, logical, short declaration)
  - Channel operations (<-)
  - Goroutine (go keyword)
  - Defer statement
  - Map declaration
  - Keywords vs identifiers
  - **Complex test 1:** Struct met methods en interface implementation
  - **Complex test 2:** Goroutine met channel operations en select statement
  - **MarkdownTokenizer test:** Parse markdown met Go code block, verify tokens via subtokenizer

---

## Showcase project

- `tests/NTokenizers.ShowCase.Go/Program.cs` - Simple Go example met syntax highlighting
- `tests/NTokenizers.ShowCase.Go/NTokenizers.ShowCase.Go.csproj`

---

## Doc file

- `docs/go.md` - Documentatie van de Go tokenizer

---

## csproj update

Update `src/NTokenizers/NTokenizers.csproj`:
- Description field: "Go" toevoegen aan de lijst
- PackageTags: "go;golang" toevoegen
