# C++ Tokenizer Implementatie

Implementeer een **streaming C++ tokenizer** in C# die volledig **karakter-voor-karakter** werkt met een **state machine**, en die kan stoppen bij een optionele `stopDelimiter`. Reuse code from BaseSubTokenizer.

> **Doel:** Goed genoeg voor syntax highlighting, niet perfecte parsing. Best-effort, no strict validation.

---

## Public API

The public API is handled by `BaseSubTokenizer<CppToken>`, so make sure to inherit from this class:

```csharp
public sealed class CppTokenizer : BaseSubTokenizer<CppToken>
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

Neem `CSharpTokenizer` als voorbeeld - C++ deelt veel syntaxis met C#.

### Doel:
- Fully streaming C++ tokenizer
- Matches the style of the CSharp tokenizer in the repo
- Goed genoeg voor syntax highlighting

### Token types

```csharp
public enum CppTokenType
{
    NotDefined,
    
    // Operators
    Operator,     // &&, ||, !, ==, !=, ===, !==, >, <, >=, <=, +, -, *, /, %, &=, |=, ^=, <<, >>, +=, -=, *=, /=, %=, ++, --, ?:, ->, ::, .*, ->*, &, |, ^, ~
    
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
    QuestionMark,       // ?
    
    // Literals
    StringValue,  // "..." or R"(...)" raw strings
    CharValue,    // '...'
    Number,       // integer, float, scientific notation, hex, octal, binary
    Boolean,      // true, false
    Null,         // nullptr, NULL
    
    // Identifiers
    Identifier,   // variable names, function names, etc.
    Keyword,      // C++ keywords
    
    // Comments
    Comment,      // // or /* */
    
    // Whitespace
    Whitespace,
}
```

### C++ keywords (voor Keyword detectie)

```csharp
private static readonly HashSet<string> Keywords = new(StringComparer.Ordinal)
{
    // C keywords (C++ inherits all of these)
    "auto", "break", "case", "char", "const", "continue", "default", "do",
    "double", "else", "enum", "extern", "float", "for", "goto", "if",
    "int", "long", "register", "return", "short", "signed", "sizeof",
    "static", "struct", "switch", "typedef", "union", "unsigned", "void",
    "volatile", "while",
    
    // C++ specific
    "alignas", "alignof", "and", "and_eq", "asm", "bitand", "bitor",
    "bool", "catch", "char8_t", "char16_t", "char32_t", "class", "compl",
    "concept", "consteval", "constexpr", "constinit", "const_cast",
    "co_await", "co_return", "co_yield", "decltype", "delete", "dynamic_cast",
    "explicit", "export", "false", "final", "float", "friend", "inline",
    "mutable", "namespace", "new", "noexcept", "not", "not_eq", "nullptr",
    "operator", "or", "or_eq", "override", "private", "protected", "public",
    "reinterpret_cast", "requires", "static_assert", "static_cast",
    "template", "this", "thread_local", "throw", "true", "try", "typeid",
    "typename", "using", "virtual", "wchar_t", "xor", "xor_eq",
    
    // C++20/23
    "co_await", "co_return", "co_yield", "concept", "consteval", "constexpr",
    "constinit", "coroutine", "requires", "module", "import", "export",
    "exportable", "transitive",
};
```

### Specifieke C++ features om te ondersteunen

> **Belangrijk:** Geen gebruik van regular expressions in de tokenizer - alles moet met karakter-voor-karakter state machine parsing werken.

1. **Strings:** `"..."` met escape characters, raw strings `R"delim(...)delim"`, wide strings `L"..."`, `u"..."`, `U"..."`, `u8"..."`
2. **Chars:** `'...'`, `L'...'`, `u'...'`, `U'...'`, `u8'...'`
3. **Comments:** `//` line comments, `/* */` block comments
4. **Preprocessor:** `#include`, `#define`, `#ifdef`, `#ifndef`, `#endif`, `#elif`, `#else`, `#undef`, `#pragma`
5. **Numbers:** decimal, hex (`0x`), octal (`0`), binary (`0b`), scientific notation, suffixes (`U`, `L`, `UL`, `ULL`, `F`, `LF`, `LLF`)
6. **Operators:** inclusief `+=`, `-=`, `*=`, `/=`, `%=` , `<<=`, `>>=`, `&=`, `|=`, `^=`, `++`, `--`, `?:`, `->`, `::`, `.*`, `->*`
7. **Templates:** `<T, U>` - hoekhaakjes als template delimiter
8. **Namespaces:** `namespace foo { }`
9. **Lambda:** `[capture](params) -> return_type { body }`

### Bestanden aanmaken

1. `src/NTokenizers/Cpp/CppTokenType.cs`
2. `src/NTokenizers/Cpp/CppToken.cs`
3. `src/NTokenizers/Cpp/CppTokenizer.cs`
4. `src/NTokenizers/Cpp/CppCodeBlockMetadata.cs`

---

## MarkdownTokenizer update

**BELANGRIJK:** Update de `ParseCodeInlines(string language)` methode in `src/NTokenizers/Markdown/MarkdownTokenizer.cs`:

Voeg een case toe aan de switch-expression (lijn 308-320):

```csharp
"cpp" or "c++" => await ParseCodeInlines(new CppCodeBlockMetadata(language)),
```

Zorg ook voor de `using` statement bovenaan het bestand:

```csharp
using NTokenizers.Cpp;
```

---

## xUnit tests

- Plaats tests in `tests/NTokenizers.Tests/CppTokenizerTests.cs`
- **Elke token type uit de enum moet minstens 1 keer getest worden**
- **Minimaal 2 complexe multi-regel tests (5-7 regels code)**
- **Test of MarkdownTokenizer de C++ subtokenizer correct gebruikt** (code fence met ` ```cpp `)
- Alles moet compileren en alle tests moeten slagen

Test cases:
  - Simple class declaration
  - Template syntax
  - Namespace declaration
  - Lambda expressions
  - String literals (regular, raw, wide)
  - Char literals
  - Numbers (decimal, hex, binary, suffixes)
  - Comments (line, block)
  - Preprocessor directives
  - Operators (arithmetic, bitwise, comparison, logical, scope resolution)
  - Smart pointers (unique_ptr, shared_ptr)
  - Range-based for loop
  - Keywords vs identifiers
  - **Complex test 1:** Template class met method en lambda
  - **Complex test 2:** Namespace met class, constructor, en smart pointers
  - **MarkdownTokenizer test:** Parse markdown met C++ code block, verify tokens via subtokenizer

---

## Showcase project

- `tests/NTokenizers.ShowCase.Cpp/Program.cs` - Simple C++ example met syntax highlighting
- `tests/NTokenizers.ShowCase.Cpp/NTokenizers.ShowCase.Cpp.csproj`

---

## Doc file

- `docs/cpp.md` - Documentatie van de C++ tokenizer

---

## csproj update

Update `src/NTokenizers/NTokenizers.csproj`:
- Description field: "C++" toevoegen aan de lijst
- PackageTags: "cpp;c++" toevoegen
