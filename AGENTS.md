# NTokenizers - Agent Instructions for Adding New Languages

This file contains all the information needed to add a new language tokenizer to NTokenizers.

## Overview

NTokenizers is a .NET library that provides **stream-capable** tokenizers for syntax highlighting. These tokenizers are **not validation-based** and are primarily intended for **prettifying, formatting, or visualizing** structured text. The **ToHtml API** (`MarkdownConverter`) is feature complete and release-ready (package version 7.0): it converts a Markdown stream to HTML stream-to-stream (per-token writes to the caller's `TextWriter`, no whole-document buffering).

## Deviations from the CommonMark Standard

The NTokenizers and ToHtml libraries intentionally deviate from the CommonMark standard (0.31.2). The behavior is not identical to CommonMark: ToHtml is not the only handler of the token stream, and the processing is streaming.

Design-level deviations (library-wide):

- **Streaming with bounded lookahead.** Tokenizers process input character-by-character and must keep emitting tokens as a stream while input arrives. Lookahead (`PeekAhead`) is therefore limited to what the stream can buffer: in practice tokenizers look ahead 1-4 characters, at most ~20. This is not a fixed limit, but a tokenizer must never require unbounded future input to decide on a token.
- **Whitespace is preserved.** CommonMark removes or normalizes some whitespace and newlines, but this library must keep whitespace in the token stream (almost always). Dropping whitespace breaks the layout, because the token stream is also used to generate console output.
- **Paragraph tokens are not mandatory.** Emitting paragraph tokens is not a requirement of the tokenizer. For example, paragraph tokens are not emitted inside lists.

ToHtml output deviations (each is annotated with a `// Deviation:` comment next to the assertion in `tests/NTokenizers.Tests.ToHtml`):

- **Rendering**
  - Fenced code blocks render as a decorated container (`<div class="code-block-container">` with a language header and Copy button) instead of a bare `<pre><code>`.
  - Inline (soft) line breaks inside a paragraph render as `<br/>` instead of being folded.
  - List item content is wrapped in `<p>` (loose/tight semantics differ from the spec).
- **Content fidelity**
  - Fenced code content keeps its full line indentation (the spec strips the common indent).
  - The fence info string is not unescaped and not entity-decoded (leading space and backslashes are kept).
  - An unclosed fence reads its content raw to the end of the stream.
  - Indented closing fences (`  ````, `    ```) are not recognized as closing the block.
  - Code spans preserve surrounding whitespace (no leading/trailing space trim).
- **Raw HTML**
  - Inline pass-through only: `<` + letter/`/`/`!`/`?` is emitted verbatim up to the first `>`; no tag validation, Markdown inside attribute text still parses, and newlines inside a paragraph become `<br/>`.
  - Angle-bracketed spans that are not a valid URI (scheme ≥ 2 chars + non-empty remainder) or email are left raw (autolink fall-through).
- **Entity and character references**
  - Named, decimal and hex references are not decoded; they render as literal text with `&` escaped to `&amp;` (body text, code blocks, indented code, and href/title attributes — the latter double-escaped).
- **Links**
  - Newlines inside link URLs are not rejected.
  - Some bracketed-URL edge cases (embedded `)`, escaped `]`, nested parens) are not fully supported.
- **Blockquotes**
  - Four leading spaces at top level are an indented code block, not a blockquote.
  - Indented code inside quotes holds more/fewer lines than the spec in a few cases.

### Unsupported CommonMark features

The following CommonMark features are intentionally not supported due to the streaming architecture:

- **Link reference definitions.** `[foo]: /url "title"` definitions and their resolution to `[foo]` / `[foo][bar]` / `![foo]` references require buffering the entire document to build a lookup table, which is incompatible with streaming. Such definitions render as plain paragraphs.
- **HTML blocks** (spec block types 1-7). Block-level raw HTML requires deciding from following lines where a block starts and ends; raw HTML is handled as inline pass-through only.

The public documentation of these differences lives on the [CommonMark Compliance](docs/commonmark.md) page.

## Quick Checklist for New Language

- [ ] Create 4 source files in `src/NTokenizers/Languages/[Language]/`
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

Create these 4 files in `src/NTokenizers/Languages/[Language]/`:

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

Update `ParseCodeInlines(string language)` in `src/NTokenizers/Languages/Markdown/MarkdownTokenizer.cs`:

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

- Best existing tokenizer: `CSharpTokenizer` in `src/NTokenizers/Languages/CSharp/`
- Base class: `BaseSubTokenizer<TToken>` in `src/NTokenizers/Core/`
- Pattern: state machine + `TokenizeCharacters()` + `EmitPending()`
- Showcase example: `tests/NTokenizers.ShowCase.CSharp/`

## Test Projects

The repository has three distinct test projects with different purposes and rules:

- **`tests/NTokenizers.Tests`** — Core tokenization. Verifies the token stream each tokenizer produces (token types, values, metadata, order). Update these when tokenizer behavior changes.
- **`tests/NTokenizers.Tests.ToHtml`** — Desired HTML output. Verifies `MarkdownConverter.ToHtml()` against the *intended* output. These tests define the expected behavior and are updated as that behavior changes (for example, when new tokens are introduced).
- **`tests/NTokenizers.Tests.ToHtml.CommonMark.Compliance`** — CommonMark spec 0.31.2 compliance monitor. **Read-only: must never be modified.** It tracks how far the converter is from full CommonMark conformance; failing tests here are expected until the corresponding feature is implemented. As of this release, 176 of the 652 spec examples produce byte-identical output.

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
