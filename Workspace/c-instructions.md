# C Tokenizer Implementatie

Implementeer een **streaming C tokenizer** in C# die volledig **karakter-voor-karakter** werkt met een **state machine**, en die kan stoppen bij een optionele `stopDelimiter`. Reuse code from BaseSubTokenizer.

> **Doel:** Goed genoeg voor syntax highlighting, niet perfecte parsing. Best-effort, no strict validation.

---

## Public API

The public API is handled by `BaseSubTokenizer<CToken>`, so make sure to inherit from this class:

```csharp
public sealed class CTokenizer : BaseSubTokenizer<CToken>
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
>     public bool InChar;
>     public bool InCommentLine;
>     public bool InCommentBlock;
>     public bool InOperator;
>     public bool InPreprocessor;
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

Neem `CSharpTokenizer` als voorbeeld - C is de voorouder van C# en deelt veel syntaxis.

### Doel:
- Fully streaming C tokenizer
- Matches the style of the CSharp tokenizer in the repo
- Goed genoeg voor syntax highlighting

### Token types

```csharp
public enum CTokenType
{
    NotDefined,
    
    // Operators
    Operator,     // &&, ||, !, ==, !=, >, <, >=, <=, +, -, *, /, %, &=, |=, ^=, <<, >>, +=, -=, *=, /=, %=, ++, --, ?:, ->, &, |, ^, ~
    
    // Punctuation
    OpenParenthesis,    // (
    CloseParenthesis,   // )
    OpenBrace,          // {
    CloseBrace,         // }
    OpenBracket,        // [
    CloseBracket,       // ]
    Comma,              // ,
    Dot,                // .
    Arrow,              // ->
    SequenceTerminator, // ;
    Colon,              // :
    
    // Literals
    StringValue,  // "..."
    CharValue,    // '...'
    Number,       // integer, float, scientific notation, hex, octal, binary (C23)
    
    // Identifiers
    Identifier,   // variable names, function names, etc.
    Keyword,      // C keywords
    Preprocessor, // #include, #define, etc.
    
    // Comments
    Comment,      // // (C99) or /* */
    
    // Whitespace
    Whitespace,
}
```

### C keywords (voor Keyword detectie)

```csharp
private static readonly HashSet<string> Keywords = new(StringComparer.Ordinal)
{
    // Types
    "auto", "break", "case", "char", "const", "continue", "default", "do",
    "double", "else", "enum", "extern", "float", "for", "goto", "if",
    "inline", "int", "long", "register", "restrict", "return", "short",
    "signed", "sizeof", "static", "struct", "switch", "typedef", "union",
    "unsigned", "void", "volatile", "while",
    
    // C99
    "_Bool", "_Complex", "_Imaginary", "_Alignas", "_Alignof",
    "_Atomic", "_Noreturn", "_Static_assert", "_Thread_local",
    "bool", "complex", "imaginary", // stdbool.h / stdcomplex.h
    
    // C11
    "_Generic", "_Pragma", "_Static_assert", "_Thread_local",
    
    // C23
    "_BitInt", "_Bitalign", "_Bitwidth", "_Decimal128", "_Decimal32", "_Decimal64",
    "_Decimal128x", "_Decimal32x", "_Decimal64x", "_Float128", "_Float16",
    "_Float32", "_Float32x", "_Float64", "_Float64x", "_Float80",
    "_Float8", "_Float80x", "_Float16x", "_Math", "_Prior", "_Static_assert",
    "_Thread_local", "_BitInt", "_Bitalign", "_Bitwidth",
};
```

### Specifieke C features om te ondersteunen

> **Belangrijk:** Geen gebruik van regular expressions in de tokenizer - alles moet met karakter-voor-karakter state machine parsing werken.

1. **Strings:** `"..."` met escape characters (`\n`, `\t`, `\\`, `\"`, `\'`, `\0`, `\xHH`, `\uXXXX`, `\UXXXXXXXX`)
2. **Chars:** `'...'` met escape characters
3. **Comments:** `//` (C99+), `/* */` block comments
4. **Preprocessor:** `#include`, `#define`, `#ifdef`, `#ifndef`, `#endif`, `#elif`, `#else`, `#undef`, `#pragma`
5. **Numbers:** decimal, hex (`0x`), octal (`0`), binary (`0b` - C23), scientific notation, suffixes (`U`, `L`, `UL`, `ULL`, `F`, `LF`)
6. **Operators:** inclusief `+=`, `-=`, `*=`, `/=`, `%=` , `<<=`, `>>=`, `&=`, `|=`, `^=`, `++`, `--`, `?:`, `->`
7. **Struct/Union:** `struct`, `union`, `enum` declaration keywords

### Bestanden aanmaken

1. `src/NTokenizers/C/CTokenType.cs`
2. `src/NTokenizers/C/CToken.cs`
3. `src/NTokenizers/C/CTokenizer.cs`
4. `src/NTokenizers/C/CCodeBlockMetadata.cs`

---

## MarkdownTokenizer update

**BELANGRIJK:** Update de `ParseCodeInlines(string language)` methode in `src/NTokenizers/Markdown/MarkdownTokenizer.cs`:

Voeg een case toe aan de switch-expression (lijn 308-320):

```csharp
"c" => await ParseCodeInlines(new CCodeBlockMetadata(language)),
```

Zorg ook voor de `using` statement bovenaan het bestand:

```csharp
using NTokenizers.C;
```

---

## xUnit tests

- Plaats tests in `tests/NTokenizers.Tests/CTokenizerTests.cs`
- **Elke token type uit de enum moet minstens 1 keer getest worden**
- **Minimaal 2 complexe multi-regel tests (5-7 regels code)**
- **Test of MarkdownTokenizer de C subtokenizer correct gebruikt** (code fence met ` ```c `)
- Alles moet compileren en alle tests moeten slagen

Test cases:
  - Simple function declaration
  - Struct definition
  - String literals with escapes
  - Char literals
  - Numbers (decimal, hex, octal, suffixes)
  - Comments (line C99, block)
  - Preprocessor directives (#include, #define)
  - Operators (arithmetic, bitwise, comparison, logical)
  - Pointer syntax (*, ->)
  - Array access []
  - Keywords vs identifiers
  - **Complex test 1:** Multi-regel struct met function pointer en array
  - **Complex test 2:** Preprocessor conditional blocks met function implementation
  - **MarkdownTokenizer test:** Parse markdown met C code block, verify tokens via subtokenizer

---

## Showcase project

- `tests/NTokenizers.ShowCase.C/Program.cs` - Simple C example met syntax highlighting
- `tests/NTokenizers.ShowCase.C/NTokenizers.ShowCase.C.csproj`

---

## Doc file

- `docs/c.md` - Documentatie van de C tokenizer

---

## csproj update

Update `src/NTokenizers/NTokenizers.csproj`:
- Description field: "C" toevoegen aan de lijst
- PackageTags: "c" toevoegen
