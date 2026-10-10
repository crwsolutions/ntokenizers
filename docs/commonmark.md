---
layout: default
title: "CommonMark Compliance"
---

# CommonMark Compliance

The `ToHtml` API uses [CommonMark 0.31.2](https://spec.commonmark.org/0.31.2/) as its reference for converting Markdown to HTML. Because NTokenizers is built on a **streaming architecture** — it processes input character-by-character and emits output incrementally as it arrives — full byte-for-byte conformance with the CommonMark reference implementation is not possible in a number of cases.

This page lists all documented differences. The intended output is defined by the `NTokenizers.Tests.ToHtml` test suite; every intentional difference is annotated with a `// Deviation:` comment next to the corresponding assertion.

## Design-level deviations

These properties apply to the NTokenizers library as a whole, not only to the ToHtml output:

- **Streaming with bounded lookahead.** Tokenizers process input character-by-character and must keep emitting tokens while input arrives. Lookahead is limited to what the stream can buffer (typically 1-4 characters). A tokenizer never requires unbounded future input to decide on a token, which rules out whole-document analysis.
- **Whitespace is preserved.** CommonMark removes or normalizes some whitespace and newlines; this library keeps whitespace in the token stream. Dropping whitespace would break the layout, because the token stream is also used to generate console output.
- **Paragraph tokens are not mandatory.** Emitting paragraph tokens is not a requirement of the tokenizer (for example, paragraph tokens are not emitted inside lists).

## ToHtml output deviations

### Rendering

- **Fenced code blocks render as a decorated container** — a `<div class="code-block-container">` with a language header and a copy-to-clipboard button — instead of a bare `<pre><code>` element. This is the most frequent difference.
- **Soft line breaks render as `<br/>`.** Inline (soft) line breaks inside a paragraph are emitted as `<br/>` instead of being folded into a single space.
- **List items wrap their content in `<p>`.** The loose/tight list item semantics differ from the spec: list item content is wrapped in a `<p>` element.

### Content fidelity

The tokenizer preserves content exactly as it appears in the input:

- **Fenced code content keeps its full line indentation.** The spec strips the common indent of the fence content; this output keeps every line exactly as written.
- **The fence info string is not unescaped and not entity-decoded.** A leading space and backslashes are kept as-is (for example ` ``` foo\+bar ` keeps the backslash in the language name).
- **Unclosed fences read to the end of the stream.** If a fenced code block is never closed, its content is read raw until the end of the input.
- **Indented closing fences are not recognized.** A closing fence line indented with spaces (`  ````) does not close the block.
- **Code spans preserve surrounding whitespace.** CommonMark trims a single leading and trailing space from a code span when both ends have spaces; this output keeps the whitespace as written.

### Raw HTML

- **Inline pass-through only.** A `<` followed by a letter, `/`, `!` or `?` starts a raw-HTML span that is emitted verbatim up to the first `>`. Tag structure is not validated, Markdown inside attribute text still parses, and newlines inside a paragraph become `<br/>`.
- **Autolink fall-through.** An angle-bracketed span that is neither a valid URI (a scheme of at least two characters plus a non-empty remainder) nor a valid email address is left as raw text.

### Entity and character references

- **Entity and character references are not decoded.** Named, decimal, and hex references (for example `&ouml;`, `&#123;`, `&#x41;`) are rendered as literal text with the `&` escaped to `&amp;`. This applies to body text, code blocks, indented code, and `href`/`title` attributes.

### Links

- **Newlines in link URLs are not rejected.** A URL containing a newline still produces a link.
- **Some bracketed-URL edge cases are not fully supported.** Embedded `)`, an escaped `]`, and nested parentheses in a bracketed URL are not fully handled.

### Blockquotes

- **Four leading spaces at top level are an indented code block, not a blockquote.**
- **Indented code inside quotes** can hold more or fewer lines than the spec in a few edge cases.

### Lists

- **Any ordered list marker may interrupt a paragraph.** Any start number (not only `1.`) starts a list instead of lazy-continuing the open paragraph (spec example 303).
- **A list marker line ends a blockquote.** The marker interrupts the quoted paragraph, so the quote closes and the list is parsed in the outer scope.

## Unsupported CommonMark features

The following CommonMark features are intentionally not supported due to the streaming architecture:

- **Link reference definitions.** `[foo]: /url "title"` definitions and their resolution to `[foo]` / `[foo][bar]` / `![foo]` references require buffering the entire document to build a lookup table, which is incompatible with streaming. Such definitions render as plain paragraphs.
- **HTML blocks** (spec block types 1-7). Block-level raw HTML requires deciding which HTML tags start a block and where it ends based on following lines; this library handles raw HTML as inline pass-through only.

## Conformance status

As of this release, **176 of the 652** [CommonMark 0.31.2 spec examples](https://spec.commonmark.org/0.31.2/) produce byte-identical output through `MarkdownConverter.ToHtml()`.

The repository contains a read-only conformance monitor, `tests/NTokenizers.Tests.ToHtml.CommonMark.Compliance`, that tracks how far the converter is from full conformance. Failing tests there are expected until the corresponding feature is implemented; the suite must never be modified, only the converter.

## See Also

- [ToHtml API](/ntokenizers/tohtml)
- [Markdown Tokenizer](/ntokenizers/markdown)
- [Home](/ntokenizers/)
