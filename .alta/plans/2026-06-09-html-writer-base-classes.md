# Refactor HTML Writers into Abstract Base Classes

- Status: Done
- Plan file: `.alta/plans/2026-06-09-html-writer-base-classes.md`
- Created: 2026-06-09
- Task: Introduce abstract base classes and an interface for the HTML writer hierarchy in `NTokenizers.Tools.MarkdownToHtml`.
- Git: not ignored (commit this plan with related work)

## Objective
- Extract shared writer logic into two abstract base classes and one interface, reducing duplication and making the writer contract explicit.
- Non-goals: no new abstractions beyond the two bases + interface; no factory/registry layer; no behavior changes.

## Context and evidence
- **Token writers** (16 language writers): `tools/.../Writers/{C,CSharp,Css,Cpp,Go,Html,Java,Json,Kotlin,Python,Rust,Sql,Swift,Toml,TypeScript,Xml,Yaml}HtmlWriter.cs` — all have `void WriteHtml(TToken, TextWriter)` (except `CssHtmlWriter` which has `WriteContent`) and `void WriteAdditionalCss(StringBuilder)`.
- **Metadata writers** (4 simple): `HeadingHtmlWriter`, `BlockquoteHtmlWriter`, `ListItemHtmlWriter`, `OrderedListItemHtmlWriter` — all have `Task WriteContentAsync(TMetadata, TextWriter)` and delegate to `InlineMarkdownTokenWriter.WriteToken`.
- **Standalone writers**: `TableHtmlWriter` (complex state), `HtmlHtmlWriter` (constructor dep on `HtmlDocumentBuilder`), `MarkdownHtmlWriter` (top-level orchestrator).
- **Static utilities**: `BaseHtmlWriter` (escaping, `WriteValue`, `WriteCommonCss`), `InlineMarkdownTokenWriter` (shared inline token handler).
- All token types implement `IToken<TTokenType>` which extends `IToken`.

## Assumptions and open decisions
- `InlineMarkdownTokenWriter` stays static for now; future state needs (e.g., wrapping text in `<p>` tags) may require converting it to an instance class extending `AbstractTokenToHtmlWriter<MarkdownToken>`. The current design accommodates this cleanly — just change static calls to instance calls and add a constructor parameter to the metadata writers.
- No decisions remain unresolved.

## Design notes

### New types

```csharp
internal interface IAdditionalCssWriter
{
    void WriteAdditionalCss(StringBuilder css);
}

internal abstract class AbstractTokenToHtmlWriter<TToken> : IAdditionalCssWriter
    where TToken : IToken
{
    protected static string EscapeHtml(string value) { /* from BaseHtmlWriter */ }
    protected static void WriteValue(TextWriter writer, string value, string? cssClass, bool inPreBlock = false) { /* from BaseHtmlWriter */ }
    internal virtual void WriteAdditionalCss(StringBuilder css) { }
    internal abstract void WriteHtml(TToken token, TextWriter writer);
}

internal abstract class AbstractMetadataToHtmlWriter<TMetadata> : IAdditionalCssWriter
    where TMetadata : InlineMetadata
{
    internal virtual void WriteAdditionalCss(StringBuilder css) { }
    internal abstract Task WriteContentAsync(TMetadata metadata, TextWriter writer);
}
```

### What moves where

| From | To |
|---|---|
| `BaseHtmlWriter.EscapeHtml` | `AbstractTokenToHtmlWriter<T>.EscapeHtml` (protected static) |
| `BaseHtmlWriter.WriteValue` + privates | `AbstractTokenToHtmlWriter<T>.WriteValue` (protected static) |
| `BaseHtmlWriter.WriteCommonCss` | stays in `BaseHtmlWriter` (static utility) |

### Writer classification

| Writer | Base class | Notes |
|---|---|---|
| `CSharpHtmlWriter`, `JsonHtmlWriter`, `PythonHtmlWriter`, etc. (16) | `AbstractTokenToHtmlWriter<TToken>` | Mechanical: add `: AbstractTokenToHtmlWriter<T>`, remove empty `WriteAdditionalCss` bodies |
| `CssHtmlWriter` | `AbstractTokenToHtmlWriter<CssToken>` | Also rename `WriteContent` → `WriteHtml` (1 call site in `HtmlDocumentBuilder`) |
| `HeadingHtmlWriter`, `BlockquoteHtmlWriter`, `ListItemHtmlWriter`, `OrderedListItemHtmlWriter` | `AbstractMetadataToHtmlWriter<TMetadata>` | Remove empty `WriteAdditionalCss` bodies |
| `TableHtmlWriter` | implements `IAdditionalCssWriter` directly | Too complex for base class (internal state, constructor deps) |
| `HtmlHtmlWriter` | implements `IAdditionalCssWriter` directly | Constructor dep on `HtmlDocumentBuilder` |
| `MarkdownHtmlWriter` | implements `IAdditionalCssWriter` directly | Top-level orchestrator |
| `InlineMarkdownTokenWriter` | stays static | Shared utility. **Future**: may become instance class extending `AbstractTokenToHtmlWriter<MarkdownToken>` if state is needed (e.g., `<p>` wrapping). |
| `BaseHtmlWriter` | stays static | Only `WriteCommonCss` remains |

### Why no `AbstractHtmlWriter` common base?
The shared surface between the two families is only `WriteAdditionalCss` — a single no-op virtual method. The interface `IAdditionalCssWriter` captures this contract without an empty inheritance layer. Standalone writers can implement the interface directly without being forced into either abstract family.

## Risks and challenges
- **Blast radius**: 16+ writer files + `BaseHtmlWriter` + `HtmlDocumentBuilder` (1 call site rename). Low risk since changes are mechanical — method bodies stay identical.
- **`WriteAdditionalCss` virtual vs abstract**: Virtual with empty default is correct — many writers have no additional CSS.
- **Generic constraints**: `TToken : IToken` and `TMetadata : InlineMetadata` are satisfied by all existing types — no core library changes needed.

## Implementation checklist
- [x] Create `IAdditionalCssWriter` interface in `Writers/IAdditionalCssWriter.cs`
- [x] Create `AbstractTokenToHtmlWriter<TToken>` in `Writers/AbstractTokenToHtmlWriter.cs` — move `EscapeHtml` and `WriteValue` (and private helpers) from `BaseHtmlWriter`
- [x] Create `AbstractMetadataToHtmlWriter<TMetadata>` in `Writers/AbstractMetadataToHtmlWriter.cs`
- [x] Trim `BaseHtmlWriter.cs` to only `WriteCommonCss`
- [x] Update 16 language writers to extend `AbstractTokenToHtmlWriter<T>`: remove empty `WriteAdditionalCss` bodies, keep `WriteHtml` bodies unchanged
- [x] Rename `CssHtmlWriter.WriteContent` → `WriteHtml`; update call site in `HtmlDocumentBuilder.cs`
- [x] Update 4 metadata writers to extend `AbstractMetadataToHtmlWriter<T>`: remove empty `WriteAdditionalCss` bodies
- [x] Add `: IAdditionalCssWriter` to `TableHtmlWriter`, `HtmlHtmlWriter`, `MarkdownHtmlWriter`
- [x] Add XML documentation to all new types and members
- [x] Update `HtmlDocumentBuilder` if any type references change (only the `CssHtmlWriter.WriteContent` → `WriteHtml` rename)

## Verification checklist
- [x] `dotnet build NTokenizers.Tools.MarkdownToHtml` — zero errors
- [x] `dotnet test NTokenizers.slnx` — all tests pass
- [x] Verify generated HTML output is identical before/after (no behavioral change)
- [x] Confirm `BaseHtmlWriter` contains only `WriteCommonCss` after refactor

## Handoff notes
- All changes are mechanical inheritance additions — no logic changes except the `CssHtmlWriter.WriteContent` → `WriteHtml` rename.
- The `InlineMarkdownTokenWriter` future-state consideration is noted but not acted on — the design accommodates it cleanly when needed.
- `IAdditionalCssWriter` enables future `IEnumerable<IAdditionalCssWriter>` iteration in `HtmlDocumentBuilder.BuildCss()` if desired.
