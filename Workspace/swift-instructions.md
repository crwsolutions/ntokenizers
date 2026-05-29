# Swift Tokenizer Implementatie

Implementeer een **streaming Swift tokenizer** in C# die volledig **karakter-voor-karakter** werkt met een **state machine**, en die kan stoppen bij een optionele `stopDelimiter`. Reuse code from BaseSubTokenizer.

> **Doel:** Goed genoeg voor syntax highlighting, niet perfecte parsing. Best-effort, no strict validation.

---

## Public API

The public API is handled by `BaseSubTokenizer<SwiftToken>`, so make sure to inherit from this class:

```csharp
public sealed class SwiftTokenizer : BaseSubTokenizer<SwiftToken>
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

Neem `CSharpTokenizer` als voorbeeld - Swift deelt veel syntaxis met C-family talen.

### Doel:
- Fully streaming Swift tokenizer
- Matches the style of the CSharp tokenizer in the repo
- Goed genoeg voor syntax highlighting

### Token types

```csharp
public enum SwiftTokenType
{
    NotDefined,
    
    // Operators
    Operator,     // &&, ||, !, ==, !=, ===, !==, >, <, >=, <=, +, -, *, /, %, +=, -=, *=, /=, %=, ++, --, ??, <.., ..., &, |, ^
    
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
    StringValue,  // "..." or """...""" multi-line strings
    CharValue,    // "..." (Swift uses double quotes for characters too in some contexts)
    Number,       // integer, float, hex, octal, binary
    Boolean,      // true, false
    Null,         // nil, nilLiteralConvertible
    
    // Identifiers
    Identifier,   // variable names, function names, etc.
    Keyword,      // Swift keywords
    
    // Comments
    Comment,      // // or /* */
    
    // Whitespace
    Whitespace,
    
    // Generic operator fallback
    Operator,
}
```

### Swift keywords (voor Keyword detectie)

```csharp
private static readonly HashSet<string> Keywords = new(StringComparer.Ordinal)
{
    // Keywords
    "associatedtype", "class", "deinit", "enum", "extension", "fileprivate",
    "func", "import", "init", "inout", "internal", "let", "mutating",
    "nonmutating", "operator", "private", "precedencegroup", "protocol",
    "public", "static", "struct", "subscript", "typealias", "var",
    
    // Control flow
    "break", "case", "continue", "default", "defer", "do", "else",
    "fallthrough", "for", "guard", "if", "in", "repeat", "return",
    "switch", "throw", "throws", "catch", "rethrows", "try",
    "where", "while", "noescape", "autoclosure",
    
    // Type-related
    "as", "dynamicType", "is", "new", "super", "self", "Self",
    "Type", "Protocol",
    
    // Constants
    "false", "nil", "true",
    
    // Special
    "available", "convenience", "dynamic", "final", "lazy", "optional",
    "override", "Postfix", "Prefix", "required", "set", "get",
    "willSet", "didSet", "weak", "unowned", "strong",
    
    // Swift 5.1+
    "some", "any", "_async", "_yield",
    
    // Swift 5.5+
    "actor", "async", "await", "asyncLet", "borrowing", "consuming",
    "sending",
};
```

### Specifieke Swift features om te ondersteunen

> **Belangrijk:** Geen gebruik van regular expressions in de tokenizer - alles moet met karakter-voor-karakter state machine parsing werken.

1. **Strings:** `"..."` met escape characters en string interpolatie `\()`, multi-line strings `"""..."""`
2. **Comments:** `//` line comments, `/* */` block comments
3. **Numbers:** decimal, hex (`0x`), octal (`0o`), binary (`0b`), scientific notation, underscores (`1_000_000`)
4. **Operators:** inclusief `+=`, `-=`, `*=`, `/=`, `%=` , `++`, `--`, `?.`, `!.` (force unwrap), `??` (nil coalescing), `..<` (half-open range), `...` (closed range), `===`/`!==` (identity)
5. **Optionals:** `Type?` voor optional, `Type!` voor implicitly unwrapped
6. **Closures:** `{ (params) -> return in body }`
7. **Attributes:** `@available`, `@objc`, `@discardableResult`, etc.
8. **Playground:** `#colorLiteral`, `#fileLiteral`, etc.

### Bestanden aanmaken

1. `src/NTokenizers/Swift/SwiftTokenType.cs`
2. `src/NTokenizers/Swift/SwiftToken.cs`
3. `src/NTokenizers/Swift/SwiftTokenizer.cs`
4. `src/NTokenizers/Swift/SwiftCodeBlockMetadata.cs`

---

## MarkdownTokenizer update

**BELANGRIJK:** Update de `ParseCodeInlines(string language)` methode in `src/NTokenizers/Markdown/MarkdownTokenizer.cs`:

Voeg een case toe aan de switch-expression (lijn 308-320):

```csharp
"swift" => await ParseCodeInlines(new SwiftCodeBlockMetadata(language)),
```

Zorg ook voor de `using` statement bovenaan het bestand:

```csharp
using NTokenizers.Swift;
```

---

## xUnit tests

- Plaats tests in `tests/NTokenizers.Tests/SwiftTokenizerTests.cs`
- **Elke token type uit de enum moet minstens 1 keer getest worden**
- **Minimaal 2 complexe multi-regel tests (5-7 regels code)**
- **Test of MarkdownTokenizer de Swift subtokenizer correct gebruikt** (code fence met ` ```swift `)
- Alles moet compileren en alle tests moeten slagen

Test cases:
  - Simple class declaration
  - Struct with properties
  - Enum with associated values
  - String literals (regular, multi-line, interpolation)
  - Numbers (decimal, hex, binary, underscores)
  - Comments (line, block)
  - Operators (arithmetic, comparison, logical, nil coalescing, identity)
  - Optional types (? and !)
  - Closure expressions
  - Range operators (..< and ...)
  - Attributes (@available, @objc)
  - Keywords vs identifiers
  - **Complex test 1:** Class met properties, initializer, en computed properties
  - **Complex test 2:** Async function met do-catch, optionals, en closures
  - **MarkdownTokenizer test:** Parse markdown met Swift code block, verify tokens via subtokenizer

---

## Showcase project

- `tests/NTokenizers.ShowCase.Swift/Program.cs` - Simple Swift example met syntax highlighting
- `tests/NTokenizers.ShowCase.Swift/NTokenizers.ShowCase.Swift.csproj`

---

## Doc file

- `docs/swift.md` - Documentatie van de Swift tokenizer

---

## csproj update

Update `src/NTokenizers/NTokenizers.csproj`:
- Description field: "Swift" toevoegen aan de lijst
- PackageTags: "swift" toevoegen
