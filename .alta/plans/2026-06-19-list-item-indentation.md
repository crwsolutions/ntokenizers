# Capture list item indentation in MarkdownTokenizer

- Status: Done
- Plan file: `.alta/plans/2026-06-19-list-item-indentation.md`
- Created: 2026-06-19
- Task: Extract leading whitespace before list markers in `TryParseListItemAsync` and include it in the emitted list item token value and metadata.
- Git: not ignored; commit this plan with related work.

## Objective
- When parsing unordered or ordered list items, capture the leading whitespace (indentation) that appears before the marker (`+`, `-`, `*`, or `1.` etc.).
- Include that whitespace in the emitted `ListItem` / `OrderedListItem` token's `Value` field.
- Store the raw indentation string in the metadata so consumers can determine nesting depth.
- **Non-goal:** No change to how other constructs (headings, blockquotes, etc.) handle indentation.

## Context and evidence
- `MarkdownTokenizer.cs` (line 236): `TryParseListItemAsync` is called from `TryParseLineStartConstructAsync` (line 106), which is only invoked when `_atLineStart && !char.IsWhiteSpace(c)`.
- The main loop (line 52) skips line-start constructs when the current char is whitespace, meaning leading spaces are accumulated into `_buffer` before the marker is reached.
- However, `_atLineStart` stays `true` while whitespace is consumed (line 86–88: `_atLineStart` only becomes `false` on a non-whitespace char). So when the loop encounters the marker, `_buffer` contains the leading indentation.
- Currently `EmitText()` (line 249, 271) fires the indentation as a plain `Text` token, losing it from the list item context.
- `ListItemMetadata` (line 9) has a `Marker` property; `OrderedListItemMetadata` (line 9) has a `Number` property — both need an `Indentation` property added.

## Assumptions and open decisions
- The indentation is always spaces (not tabs) — common in Markdown. If tabs appear, they are preserved as-is in the string.
- No open decisions remain; user chose **string (raw whitespace)** for representation and **include in Value** for emission.

## Design notes
- Before calling `EmitText()` in `TryParseListItemAsync`, extract the leading whitespace from `_buffer` into a `string indentation`.
- Clear `_buffer` instead of calling `EmitText()`, so the whitespace is not emitted as a separate `Text` token.
- Add `Indentation` property (type `string`) to both `ListItemMetadata` and `OrderedListItemMetadata`.
- Prepend the `indentation` to the token value in `ParseInlines` — however, `ParseInlines` currently emits an empty string value for the list item token, and the inline content is handled separately. The cleanest approach: prepend `indentation` to the value passed to the `MarkdownToken` constructor.
- Rejected: using `int` for depth — loses fidelity (mixed spaces/tabs, varying depth conventions).

## Risks and challenges
- **Inline content value:** The list item token is emitted with `string.Empty` value in `ParseInlines`; the actual inline content is handled via `OnInlineToken`. The indentation will be the token's value, which is consistent with the user's intent to "issue these spaces in the value when the listitem token is emitted."
- **Existing tests:** Any tests that assert the indentation is emitted as a separate `Text` token will need updating.
- **Nested lists within inline parsing:** The inline tokenizer handles nested constructs; indentation is only relevant at the top-level list item, not for nested items parsed inline.

## Implementation checklist
- [ ] Add `Indentation` property (type `string`, default `string.Empty`) to `ListItemMetadata` in `src/NTokenizers/Markdown/Metadata/ListItemMetadata.cs`
- [ ] Add `Indentation` property (type `string`, default `string.Empty`) to `OrderedListItemMetadata` in `src/NTokenizers/Markdown/Metadata/OrderedListItemMetadata.cs`
- [ ] In `TryParseListItemAsync` (unordered branch, line 249): extract leading whitespace from `_buffer` into `indentation`, clear `_buffer` instead of calling `EmitText()`, pass `indentation` to `ListItemMetadata`
- [ ] In `TryParseListItemAsync` (ordered branch, line 271): same extraction for `OrderedListItemMetadata`
- [ ] In `ParseInlines` (line 146–150): use `metadata.Indentation` as the token value when constructing the `MarkdownToken` (instead of `string.Empty`)
- [ ] Update any existing tests that expect indentation as a separate `Text` token

## Verification checklist
- [ ] `dotnet test NTokenizers.slnx` — all tests pass
- [ ] Verify nested list item tokens contain correct indentation in their value
- [ ] Verify top-level list items have empty indentation string
- [ ] Verify no separate `Text` token is emitted for the indentation

## Handoff notes
- The change is small and localized to 3 files: the two metadata classes and `MarkdownTokenizer.cs`.
- The key mechanism: `_buffer` already holds the leading whitespace when `TryParseListItemAsync` is called; we just need to extract it before clearing the buffer instead of emitting it.
