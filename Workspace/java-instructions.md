# Java Tokenizer Implementatie

Implementeer een **streaming Java tokenizer** in C# die volledig **karakter-voor-karakter** werkt met een **state machine**, en die kan stoppen bij een optionele `stopDelimiter`. Reuse code from BaseSubTokenizer.

> **Doel:** Goed genoeg voor syntax highlighting, niet perfecte parsing. Best-effort, no strict validation.

---

## Public API

The public API is handled by `BaseSubTokenizer<TJavaToken>`, so make sure to inherit from this class:

```csharp
public sealed class JavaTokenizer : BaseSubTokenizer<JavaToken>
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

Neem `CSharpTokenizer` als voorbeeld - Java deelt veel syntaxis met C#.

### Doel:
- Fully streaming Java tokenizer
- Matches the style of the CSharp tokenizer in the repo
- Goed genoeg voor syntax highlighting

### Token types

```csharp
public enum JavaTokenType
{
    NotDefined,
    
    // Operators
    Operator,     // &&, ||, !, ==, !=, >, <, >=, <=, +, -, *, /, %, +=, -=, *=, /=, %=, <<, >>, >>>, ++, --, ?:, ->, &, |, ^, ~
    
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
    StringValue,  // "..."
    CharValue,    // '...'
    Number,       // integer, float, scientific notation, hex, octal, binary
    Boolean,      // true, false
    Null,         // null
    
    // Identifiers
    Identifier,   // variable names, method names, etc.
    Keyword,      // Java keywords
    
    // Comments
    Comment,      // // or /* */
    
    // Whitespace
    Whitespace,
}
```

### Java keywords (voor Keyword detectie)

```csharp
private static readonly HashSet<string> Keywords = new(StringComparer.Ordinal)
{
    // Control flow
    "abstract", "assert", "break", "case", "catch", "continue", "default", "do",
    "else", "finally", "for", "if", "instanceof", "new", "return", "switch",
    "throw", "throws", "try", "while",
    
    // Types
    "boolean", "byte", "char", "double", "float", "int", "long", "short", "void",
    
    // Modifiers
    "class", "enum", "extends", "final", "finally", "implements", "import",
    "interface", "native", "package", "private", "protected", "public",
    "static", "strictfp", "super", "synchronized", "this", "transient", "volatile",
    
    // Literals
    "false", "null", "true",
    
    // Others
    "const", "goto", // reserved but unused
    "var", // local variable type inference (Java 10+)
    "record", "sealed", "non-sealed", "permits", "yield", "when", // newer Java
};
```

### Specifieke Java features om te ondersteunen

> **Belangrijk:** Geen gebruik van regular expressions in de tokenizer - alles moet met karakter-voor-karakter state machine parsing werken.

1. **Strings:** `"..."` met escape characters (`\n`, `\t`, `\\`, `\"`, `\'`, `\uXXXX`)
2. **Chars:** `'...'` met escape characters
3. **Comments:** `//` line comments, `/* */` block comments, `/** */` Javadoc comments (zelfde token type)
4. **Numbers:** decimal, hex (`0x`), octal (`0`), binary (`0b`), scientific notation, suffixes (`L`, `F`, `D`)
5. **Operators:** inclusief `+=`, `-=`, `*=`, `/=`, `%=` , `<<`, `>>`, `>>>`, `++`, `--`, `?:`, `->` (lambdas)
6. **Generics:** `<T, U extends Comparable<? super T>>` - hoekhaakjes als operator

### Bestanden aanmaken

1. `src/NTokenizers/Java/JavaTokenType.cs`
2. `src/NTokenizers/Java/JavaToken.cs`
3. `src/NTokenizers/Java/JavaTokenizer.cs`
4. `src/NTokenizers/Java/JavaCodeBlockMetadata.cs`

---

## MarkdownTokenizer update

**BELANGRIJK:** Update de `ParseCodeInlines(string language)` methode in `src/NTokenizers/Markdown/MarkdownTokenizer.cs`:

Voeg een case toe aan de switch-expression (lijn 308-320):

```csharp
"java" => await ParseCodeInlines(new JavaCodeBlockMetadata(language)),
```

Zorg ook voor de `using` statement bovenaan het bestand:

```csharp
using NTokenizers.Java;
```

---

## xUnit tests

- Plaats tests in `tests/NTokenizers.Tests/JavaTokenizerTests.cs`
- **Elke token type uit de enum moet minstens 1 keer getest worden**
- **Minimaal 2 complexe multi-regel tests (5-7 regels code)**
- **Test of MarkdownTokenizer de Java subtokenizer correct gebruikt** (code fence met ` ```java `)
- Alles moet compileren en alle tests moeten slagen

Test cases:
  - Simple class declaration
  - Method with parameters
  - String literals with escapes
  - Char literals
  - Numbers (decimal, hex, binary, octal, scientific)
  - Comments (line, block, javadoc)
  - Operators (arithmetic, comparison, logical, assignment)
  - Lambda expressions
  - Generics
  - Annotations (`@Override`, `@SuppressWarnings`)
  - Keywords vs identifiers
  - **Complex test 1:** Multi-regel class met method, generics, en annotations
  - **Complex test 2:** Lambda expression met streams en method references
  - **MarkdownTokenizer test:** Parse markdown met Java code block, verify tokens via subtokenizer

---

## Showcase project

- `tests/NTokenizers.ShowCase.Java/Program.cs` - Simple Java example met syntax highlighting
- `tests/NTokenizers.ShowCase.Java/NTokenizers.ShowCase.Java.csproj`

---

## Doc file

- `docs/java.md` - Documentatie van de Java tokenizer

---

## csproj update

Update `src/NTokenizers/NTokenizers.csproj`:
- Description field: "java" toevoegen aan de lijst
- PackageTags: "java" toevoegen
