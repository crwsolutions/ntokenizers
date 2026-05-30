# NTokenizers - Agent Instructions for Adding New Languages

This file contains all the information needed to add a new language tokenizer to NTokenizers.

## Overview

NTokenizers is a .NET library that provides **stream-capable** tokenizers for syntax highlighting. These tokenizers are **not validation-based** and are primarily intended for **prettifying, formatting, or visualizing** structured text.

## Quick Checklist for New Language

- [ ] Create 4 source files in `src/NTokenizers/[Language]/`
- [ ] Create 1 test file in `tests/NTokenizers.Tests/`
- [ ] Create 1 showcase project in `tests/NTokenizers.ShowCase.[Language]/`
- [ ] Create 1 doc file in `docs/`
- [ ] Update `MarkdownTokenizer.cs` with language aliases
- [ ] Update `NTokenizers.csproj` with description and tags
- [ ] Update `NTokenizers.slnx` with showcase project
- [ ] Update `docs/_config.yml` with sidebar navigation
- [ ] Update `README.md` with language and code example
- [ ] Add XML documentation to all enums and classes
- [ ] Add code block to Markdown showcase
- [ ] Add unit tests for all token types
- [ ] Test newline preservation in preprocessor/comments

## Architecture

Tokenizers use a **state machine** pattern with character-by-character processing. The `MarkdownTokenizer` acts as a **composite tokenizer**, delegating code blocks to sub-tokenizers.

```
         ┌─────────┐
         │ stream  │
         └─────────┘
              │  ParseAsync()
              ▼
   ┌─────────────────────┐
   │  MarkdownTokenizer  │ ───────────► fire markdown tokens
   └─────────────────────┘
              │
              ▼       ┌─────────┐
              ├──────►│  [lang] │ ───► fire [lang] tokens
              │       └─────────┘
              │
              └──────►│  etc..  │ ───► etc
                      └─────────┘
```

## Public API

All tokenizers inherit from `BaseSubTokenizer<TToken>` which handles the public API:

```csharp
public sealed class [Language]Tokenizer : BaseSubTokenizer<[Language]Token>
```

Override the abstract `ParseAsync` method:

```csharp
internal protected override Task ParseAsync(CancellationToken ct)
{
    var state = new State();
    // define additional state variables here (e.g., char? stringDelimiter)

    TokenizeCharacters(ct, (c) => ProcessChar(c, state, ref /* extra state variables */));

    EmitPending(state);

    return Task.CompletedTask;
}
```

**Important:** Use `private sealed class State` (not enum!) for state tracking - an object with boolean properties:

```csharp
private sealed class State
{
    public bool InWhitespace;
    public bool InIdentifier;
    public bool InNumber;
    public bool InString;
    public bool InCommentLine;
    public bool InCommentBlock;
    public bool InOperator;
    // ... etc
}
```

## Step-by-Step Instructions

### 1. Create Source Files

Create these 4 files in `src/NTokenizers/[Language]/`:

1. `[Language]TokenType.cs` - Enum with token types
2. `[Language]Token.cs` - Token class
3. `[Language]Tokenizer.cs` - The tokenizer (inherits from `BaseSubTokenizer`)
4. `[Language]CodeBlockMetadata.cs` - Metadata class

**Important:** Use `private sealed class State` (not enum) for state tracking with boolean properties.

### 2. Create Test File

Create `[Language]TokenizerTests.cs` in `tests/NTokenizers.Tests/`:

- Test every token type from the enum at least once
- Include at least 2 complex multi-line tests (5-7 lines of code)
- Test MarkdownTokenizer integration with code fences
- Test newline preservation in preprocessor directives and comments
- Test that output matches input (ignoring color)

### 3. Create Showcase Project

Create showcase project in `tests/NTokenizers.ShowCase.[Language]/`:

- Follow the C# showcase pattern with Spectre.Console
- Use colored output (keywords: blue, identifiers: cyan, strings: green, numbers: magenta, operators: yellow, comments: gray, whitespace: gray)
- Add Spectre.Console package reference: `<PackageReference Include="Spectre.Console" Version="0.54.0" />`

### 4. Create Documentation and Update Config

Create `[language].md` in `docs/` folder.

Update `docs/_config.yml`:
- Add sidebar navigation entry for the new language:
  ```yaml
  - title: "[Language]"
    url: "[language]"
  ```

### 5. Update MarkdownTokenizer

Update `ParseCodeInlines(string language)` in `src/NTokenizers/Markdown/MarkdownTokenizer.cs`:

```csharp
"[language]" => await ParseCodeInlines(new [Language]CodeBlockMetadata(language)),
```

Add `using NTokenizers.[Language];` at the top.

### 6. Update Project Files

Update `src/NTokenizers/NTokenizers.csproj`:
- Add language to Description field
- Add language to PackageTags

Update `NTokenizers.slnx`:
- Add showcase project to the solution file

### 7. Update README.md

Update `README.md`:
- Add language to first paragraph (list of supported formats)
- Add code example in the kickoff section
- Add language to the Overview section

### 8. Add to Markdown Showcase

Update `tests/NTokenizers.ShowCase.Markdown/Program.cs`:
- Add code block with 3-6 lines of example code
- Add token handler with syntax highlighting

### 9. Add XML Documentation

Add `/// <summary>` comments to:
- All enum values in `[Language]TokenType.cs`
- All public classes and methods
- Use `/// <inheritdoc/>` for ParseAsync methods

## Common Pitfalls

1. **Newline handling:** Ensure newlines after preprocessor directives and comments are emitted as whitespace tokens
2. **State machine:** Use `private sealed class State` with boolean properties, not enums
3. **Streaming:** Process character-by-character, don't use regex
4. **Test coverage:** Every token type must be tested at least once
5. **Showcase consistency:** All showcase projects should follow the same Spectre.Console pattern

## Reference Files

- Best existing tokenizer: `CSharpTokenizer` in `src/NTokenizers/CSharp/`
- Base class: `BaseSubTokenizer<TToken>` in `src/NTokenizers/Core/`
- Pattern: state machine + `TokenizeCharacters()` + `EmitPending()`
- Showcase example: `tests/NTokenizers.ShowCase.CSharp/`

## Testing

Run all tests with:
```bash
dotnet test NTokenizers.slnx
```

Run specific language tests:
```bash
dotnet test NTokenizers.slnx --filter "FullyQualifiedName~[Language]TokenizerTests"
```

## Commit Message Format

```
feat: add [Language] tokenizer with full test coverage

- Add [Language]TokenType, [Language]Token, [Language]Tokenizer, [Language]CodeBlockMetadata
- Add [Language]TokenizerTests with comprehensive test coverage
- Add showcase project with Spectre.Console syntax highlighting
- Add documentation and update Markdown showcase
- All tests passing
```
