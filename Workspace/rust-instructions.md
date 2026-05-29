# Rust Tokenizer Implementatie

Implementeer een **streaming Rust tokenizer** in C# die volledig **karakter-voor-karakter** werkt met een **state machine**, en die kan stoppen bij een optionele `stopDelimiter`. Reuse code from BaseSubTokenizer.

> **Doel:** Goed genoeg voor syntax highlighting, niet perfecte parsing. Best-effort, no strict validation.

---

## Public API

The public API is handled by `BaseSubTokenizer<RustToken>`, so make sure to inherit from this class:

```csharp
public sealed class RustTokenizer : BaseSubTokenizer<RustToken>
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
>     public bool InLifetime;
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

Neem `CSharpTokenizer` als voorbeeld - Rust deelt veel syntaxis met C-family talen.

### Doel:
- Fully streaming Rust tokenizer
- Matches the style of the CSharp tokenizer in the repo
- Goed genoeg voor syntax highlighting

### Token types

```csharp
public enum RustTokenType
{
    NotDefined,
    
    // Operators
    Operator,     // &&, ||, !, ==, !=, >, <, >=, <=, +, -, *, /, %, &=, |=, ^=, <<, >>, +=, -=, *=, /=, %=, =>, ::, &, |, ^
    
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
    FatArrow,           // =>
    SequenceTerminator, // ;
    Colon,              // :
    DoubleColon,        // ::
    At,                 // @
    Pound,              // #
    QuestionMark,       // ?
    
    // Literals
    StringValue,  // "..." or r#"..."# raw strings
    CharValue,    // '...'
    Number,       // integer, float, hex, octal, binary
    Boolean,      // true, false
    
    // Identifiers
    Identifier,   // variable names, function names, etc.
    Keyword,      // Rust keywords
    Macro,        // macro!() or macro_rules!
    Lifetime,     // 'a, 'static
    
    // Comments
    Comment,      // // or /* */ or /// or /** */
    
    // Whitespace
    Whitespace,
    
    // Generic operator fallback
    Operator,
}
```

### Rust keywords (voor Keyword detectie)

```csharp
private static readonly HashSet<string> Keywords = new(StringComparer.Ordinal)
{
    // Keywords
    "as", "async", "await", "break", "const", "continue", "crate", "dyn",
    "else", "enum", "extern", "false", "fn", "for", "if", "impl", "in",
    "let", "loop", "match", "mod", "move", "mut", "pub", "ref", "return",
    "self", "Self", "static", "struct", "super", "trait", "true", "type",
    "unsafe", "use", "where", "while", "yield",
    
    // Edition 2018+
    "async", "await", "dyn",
    
    // Edition 2021+
    "const", "async",
    
    // Strict keywords (cannot be used as identifiers)
    "abstract", "become", "box", "do", "final", "macro", "override",
    "priv", "typeof", "unsized", "virtual", "yield",
    
    // Weak keywords (context-dependent)
    "try", "impl", "dyn",
};
```

### Specifieke Rust features om te ondersteunen

> **Belangrijk:** Geen gebruik van regular expressions in de tokenizer - alles moet met karakter-voor-karakter state machine parsing werken.

1. **Strings:** `"..."` met escape characters, raw strings `r"..."`, `r#"..."#`, `r##"..."##`
2. **Chars:** `'...'` met escape characters, `\\'` escaped
3. **Lifetimes:** `'a`, `'static`, `'life_time`
4. **Comments:** `//` line, `///` doc, `/* */` block, `/** */` doc block
5. **Macros:** `macro_name!()`, `macro_rules!`, `#[attr]`, `#![attr]`
6. **Numbers:** decimal, hex (`0x`), octal (`0o`), binary (`0b`), suffixes (`u8`, `i32`, `f64`, `usize`)
7. **Operators:** inclusief `+=`, `-=`, `*=`, `/=`, `%=` , `&=`, `|=`, `^=`, `<<=`, `>>=`, `=>` (match arms), `::` (path separator), `->` (return type)
8. **Pattern matching:** `match`, `if let`, `while let`
9. **Generics:** `<T: Trait + 'lifetime>`

### Bestanden aanmaken

1. `src/NTokenizers/Rust/RustTokenType.cs`
2. `src/NTokenizers/Rust/RustToken.cs`
3. `src/NTokenizers/Rust/RustTokenizer.cs`
4. `src/NTokenizers/Rust/RustCodeBlockMetadata.cs`

---

## MarkdownTokenizer update

**BELANGRIJK:** Update de `ParseCodeInlines(string language)` methode in `src/NTokenizers/Markdown/MarkdownTokenizer.cs`:

Voeg een case toe aan de switch-expression (lijn 308-320):

```csharp
"rust" or "rs" => await ParseCodeInlines(new RustCodeBlockMetadata(language)),
```

Zorg ook voor de `using` statement bovenaan het bestand:

```csharp
using NTokenizers.Rust;
```

---

## xUnit tests

- Plaats tests in `tests/NTokenizers.Tests/RustTokenizerTests.cs`
- **Elke token type uit de enum moet minstens 1 keer getest worden**
- **Minimaal 2 complexe multi-regel tests (5-7 regels code)**
- **Test of MarkdownTokenizer de Rust subtokenizer correct gebruikt** (code fence met ` ```rust `)
- Alles moet compileren en alle tests moeten slagen

Test cases:
  - Simple function declaration
  - Struct with lifetime
  - Enum with variants
  - String literals (regular, raw)
  - Char literals
  - Lifetimes ('a, 'static)
  - Numbers (decimal, hex, binary, suffixes)
  - Comments (line, doc, block)
  - Macro invocations
  - Operators (arithmetic, bitwise, comparison, logical)
  - Pattern matching
  - Generics with trait bounds
  - Keywords vs identifiers
  - **Complex test 1:** Async function met generics, lifetimes, en match expression
  - **Complex test 2:** Struct implementation met multiple methods en trait bounds
  - **MarkdownTokenizer test:** Parse markdown met Rust code block, verify tokens via subtokenizer

---

## Showcase project

- `tests/NTokenizers.ShowCase.Rust/Program.cs` - Simple Rust example met syntax highlighting
- `tests/NTokenizers.ShowCase.Rust/NTokenizers.ShowCase.Rust.csproj`

---

## Doc file

- `docs/rust.md` - Documentatie van de Rust tokenizer

---

## csproj update

Update `src/NTokenizers/NTokenizers.csproj`:
- Description field: "Rust" toevoegen aan de lijst
- PackageTags: "rust" toevoegen
