# Kotlin Tokenizer Implementatie

Implementeer een **streaming Kotlin tokenizer** in C# die volledig **karakter-voor-karakter** werkt met een **state machine**, en die kan stoppen bij een optionele `stopDelimiter`. Reuse code from BaseSubTokenizer.

> **Doel:** Goed genoeg voor syntax highlighting, niet perfecte parsing. Best-effort, no strict validation.

---

## Public API

The public API is handled by `BaseSubTokenizer<KotlinToken>`, so make sure to inherit from this class:

```csharp
public sealed class KotlinTokenizer : BaseSubTokenizer<KotlinToken>
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

Neem `CSharpTokenizer` als voorbeeld - Kotlin deelt veel syntaxis met Java/C-family talen.

### Doel:
- Fully streaming Kotlin tokenizer
- Matches the style of the CSharp tokenizer in the repo
- Goed genoeg voor syntax highlighting

### Token types

```csharp
public enum KotlinTokenType
{
    NotDefined,
    
    // Operators
    Operator,     // &&, ||, !, ==, !=, >, <, >=, <=, +, -, *, /, %, +=, -=, *=, /=, %=, ++, --, ?. ?:, in, is, as
    
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
    DoubleColon,        // ::
    At,                 // @
    Pound,              // #
    QuestionMark,       // ?
    
    // Literals
    StringValue,  // "..." or """...""" raw strings
    CharValue,    // '...'
    Number,       // integer, float, hex, binary
    Boolean,      // true, false
    Null,         // null
    
    // Identifiers
    Identifier,   // variable names, function names, etc.
    Keyword,      // Kotlin keywords
    
    // Comments
    Comment,      // // or /* */
    
    // Whitespace
    Whitespace,
    
    // Generic operator fallback
    Operator,
}
```

### Kotlin keywords (voor Keyword detectie)

```csharp
private static readonly HashSet<string> Keywords = new(StringComparer.Ordinal)
{
    // Control flow
    "abstract", "actual", "as", "break", "by", "catch", "class", "companion",
    "const", "constructor", "continue", "crossinline", "data", "do", "else",
    "enum", "false", "field", "file", "final", "finally", "for", "fun",
    "get", "if", "import", "in", "infix", "init", "inner", "inline",
    "interface", "internal", "is", "it", "lateinit", "noinline",
    "null", "object", "open", "out", "override", "package", "param",
    "private", "property", "protected", "public", "receiver", "reified",
    "return", "sealed", "set", "setparam", "static", "super", "this",
    "throw", "true", "try", "typealias", "tailrec", "val", "var",
    "vararg", "when", "where", "while",
    
    // Types
    "Boolean", "Byte", "Short", "Int", "Long", "Float", "Double", "Char", "String",
    "Unit", "Nothing", "Dynamic",
};
```

### Specifieke Kotlin features om te ondersteunen

> **Belangrijk:** Geen gebruik van regular expressions in de tokenizer - alles moet met karakter-voor-karakter state machine parsing werken.

1. **Strings:** `"..."` met escape characters en string templates `$var` en `${expression}`, raw strings `"""..."""`
2. **Chars:** `'...'` met escape characters
3. **Comments:** `//` line comments, `/* */` block comments
4. **Numbers:** decimal, hex (`0x`), binary (`0b`), underscores (`1_000_000`)
5. **Operators:** inclusief `+=`, `-=`, `*=`, `/=`, `%=` , `++`, `--`, `?.`, `?:` (Elvis), `in`, `is`, `as`
6. **Lambda:** `{ params -> body }` of `{ body }`
7. **Nullability:** `Type?` voor nullable types
8. **Type aliases:** `typealias Alias = Type`

### Bestanden aanmaken

1. `src/NTokenizers/Kotlin/KotlinTokenType.cs`
2. `src/NTokenizers/Kotlin/KotlinToken.cs`
3. `src/NTokenizers/Kotlin/KotlinTokenizer.cs`
4. `src/NTokenizers/Kotlin/KotlinCodeBlockMetadata.cs`

---

## MarkdownTokenizer update

**BELANGRIJK:** Update de `ParseCodeInlines(string language)` methode in `src/NTokenizers/Markdown/MarkdownTokenizer.cs`:

Voeg een case toe aan de switch-expression (lijn 308-320):

```csharp
"kotlin" or "kt" => await ParseCodeInlines(new KotlinCodeBlockMetadata(language)),
```

Zorg ook voor de `using` statement bovenaan het bestand:

```csharp
using NTokenizers.Kotlin;
```

---

## xUnit tests

- Plaats tests in `tests/NTokenizers.Tests/KotlinTokenizerTests.cs`
- **Elke token type uit de enum moet minstens 1 keer getest worden**
- **Minimaal 2 complexe multi-regel tests (5-7 regels code)**
- **Test of MarkdownTokenizer de Kotlin subtokenizer correct gebruikt** (code fence met ` ```kotlin `)
- Alles moet compileren en alle tests moeten slagen

Test cases:
  - Simple class declaration
  - Data class with properties
  - String templates ($var, ${expression})
  - Raw strings (""")
  - Char literals
  - Numbers (decimal, hex, binary, underscores)
  - Comments (line, block)
  - Operators (arithmetic, comparison, logical, Elvis)
  - Lambda expressions
  - Nullable types
  - When expression
  - Keywords vs identifiers
  - **Complex test 1:** Data class met companion object, properties, en constructor
  - **Complex test 2:** When expression met multiple branches en lambda
  - **MarkdownTokenizer test:** Parse markdown met Kotlin code block, verify tokens via subtokenizer

---

## Showcase project

- `tests/NTokenizers.ShowCase.Kotlin/Program.cs` - Simple Kotlin example met syntax highlighting
- `tests/NTokenizers.ShowCase.Kotlin/NTokenizers.ShowCase.Kotlin.csproj`

---

## Doc file

- `docs/kotlin.md` - Documentatie van de Kotlin tokenizer

---

## csproj update

Update `src/NTokenizers/NTokenizers.csproj`:
- Description field: "Kotlin" toevoegen aan de lijst
- PackageTags: "kotlin" toevoegen
