# C-like Tokenizers Workspace

Doel: tokenizers toevoegen voor populaire C-like talen, goed genoeg voor syntax highlighting.

> **Belangrijk:** Deze tokenizers zijn **niet validatie-gebaseerd** en zijn primair bedoeld voor **prettifying, formatting, of visualizing** van gestructureerde tekst. Ze doen geen strikte validatie van de input, dus ze kunnen onverwachte resultaten produceren bij malformed input. Het gaat om "goed genoeg" voor syntax highlighting, niet om perfecte parsing.

## Taal tokenizers te implementeren

- [x] **Java** (`java-instructions.md`)
- [x] **C** (`c-instructions.md`)
- [x] **C++** (`cpp-instructions.md`)
- [x] **Rust** (`rust-instructions.md`)
- [x] **Kotlin** (`kotlin-instructions.md`)
- [x] **Go** (`go-instructions.md`)
- [x] **Swift** (`swift-instructions.md`)

## Per tokenizer zijn dit de deliverables

1. **4 source files** in `src/NTokenizers/[Language]/`:
   - `[Language]TokenType.cs` - Enum met token types
   - `[Language]Token.cs` - Token class
   - `[Language]Tokenizer.cs` - De tokenizer (erft van `BaseSubTokenizer`)
   - `[Language]CodeBlockMetadata.cs` - Metadata class

2. **1 test file** in `tests/NTokenizers.Tests/`:
   - `[Language]TokenizerTests.cs`

3. **1 showcase project** in `tests/NTokenizers.ShowCase.[Language]/`:
   - `Program.cs` + `.csproj`

4. **1 doc file** in `docs/`:
   - `[language].md`

5. **MarkdownTokenizer update** in `src/NTokenizers/Markdown/MarkdownTokenizer.cs`:
   - `ParseCodeInlines(string language)` switch-case uitbreiden met taal aliases

6. **csproj update** in `src/NTokenizers/NTokenizers.csproj`:
   - Description en PackageTags updaten

## Referenties

- Bestaande tokenizers als voorbeeld: `CSharpTokenizer`, `TypescriptTokenizer`
- Issue template: https://github.com/crwsolutions/ntokenizers/issues/44
- Base class: `BaseSubTokenizer<TToken>` in `src/NTokenizers/Core/`
- Pattern: state machine + `TokenizeCharacters()` + `EmitPending()`
