# ToHtml Realistic-Behavior Tests

These tests are a **regression baseline + working-backlog** for `MarkdownConverter.ToHtml()` (in `NTokenizers.ToHtml`). They cover all **651 examples** from the [CommonMark Spec 0.31.2](https://spec.commonmark.org/0.31.2/), but they **do not assert CommonMark conformance**.

They live alongside `NTokenizers.Tests.ToHtml.CommonMark.Compliance`, which keeps the strict spec-expecteds (and mostly fails). This project keeps the same 651 inputs and `Example_NNN` numbering, so every test traces 1-to-1 to a spec example.

## What "realistic" means here

`NTokenizers` is a **streaming** tokenizer that is also used for **console** rendering, where whitespace matters. It therefore **does not strip** leading/trailing whitespace the way the CommonMark spec requires, and it **does not collapse** whitespace. Browsers ignore most of that whitespace in HTML, so the difference is cosmetic rather than incorrect.

A **realistic assert** for a test is:

- **Whitespace / line breaks** are asserted **exactly as the library currently emits them** (retained, not stripped/collapsed).
- **Structure** (tags, blocks, nesting) follows CommonMark.

Because the library does not yet emit several block structures, most tests **fail by design** — the failure is the backlog. Each failing test is a concrete, reproducible case to fix and then update.

## Newline normalization

`MarkdownConverter` writes block separators with `TextWriter.WriteLine`, which emits `\r\n` on Windows and `\n` on Linux. To keep the asserts platform-independent, **every test normalizes the result**:

```csharp
var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
Assert.Equal("<expected>", html);
```

All expected strings therefore use `\n` only. Inline `\r` that currently flows through from `\r\n` input is asserted as it comes out of the library today.

## Test pattern

```csharp
[Fact]
public void Example_221()
{
    var input = "  aaa\n bbb";
    var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
    // Structure follows CommonMark (<p> with soft-break), whitespace as the library emits it.
    Assert.Equal("<p>  aaa<br/> bbb</p>", html);
}
```

## Current status (2026-09-15)

**651 total — 24 pass / 627 fail.** The 24 passing tests are the ones where the library's output already matches CommonMark structure (differing at most in whitespace). **Every remaining failure is structural** — none is a whitespace/newline mismatch (verified: after stripping `<br/>` and all whitespace from both sides, expected and actual still differ).

Dominant failure categories:

| Category | ~count |
|----------|-------|
| `<p>` (paragraph) not emitted | 502 |
| other structure / inline handling | 91 |
| `<pre><code>` (indented / fenced code) not emitted | 34 |

### Per-file pass / fail

| File | Total | Pass | Fail |
|------|------:|-----:|-----:|
| AtxHeadingsTests | 18 | 7 | 11 |
| AutolinksTests | 19 | 0 | 19 |
| BackslashEscapesTests | 13 | 1 | 12 |
| BlankLinesTests | 1 | 0 | 1 |
| BlockQuotesTests | 25 | 1 | 24 |
| CodeSpansTests | 22 | 0 | 22 |
| EmphasisTests | 132 | 0 | 132 |
| EntityReferencesTests | 17 | 1 | 16 |
| FencedCodeBlocksTests | 29 | 0 | 29 |
| HardLineBreaksTests | 15 | 2 | 13 |
| HtmlBlocksTests | 44 | 5 | 39 |
| ImagesTests | 22 | 0 | 22 |
| IndentedCodeBlocksTests | 12 | 0 | 12 |
| LinkReferenceDefinitionsTests | 27 | 0 | 27 |
| LinksTests | 90 | 0 | 90 |
| ListItemsTests | 48 | 0 | 48 |
| ListsTests | 27 | 1 | 26 |
| ParagraphsTests | 8 | 0 | 8 |
| PrecedenceTests | 1 | 0 | 1 |
| RawHtmlTests | 20 | 0 | 20 |
| SetextHeadingsTests | 27 | 3 | 24 |
| SoftLineBreaksTests | 2 | 0 | 2 |
| TabsTests | 11 | 1 | 10 |
| TextualContentTests | 2 | 0 | 2 |
| ThematicBreaksTests | 19 | 2 | 17 |
| **Total** | **651** | **24** | **627** |

## How to use

Run this project alone:

```bash
dotnet test tests/NTokenizers.Tests.ToHtml/NTokenizers.Tests.ToHtml.csproj
```

Run a single section:

```bash
dotnet test tests/NTokenizers.Tests.ToHtml/NTokenizers.Tests.ToHtml.csproj --filter "FullyQualifiedName~ParagraphsTests"
```

Run a single example:

```bash
dotnet test tests/NTokenizers.Tests.ToHtml/NTokenizers.Tests.ToHtml.csproj --filter "FullyQualifiedName~Example_221"
```

## Working with the failures

When a defect is fixed in the library (e.g. paragraphs are now emitted), the corresponding tests start failing against the old "realistic" expected. At that point:

1. Re-run the affected section.
2. Re-read the now-correct output and update the expected to the **new realistic** value (structure per CommonMark, whitespace as the library emits it).
3. The test flips back to passing and the expected becomes the new baseline.

This keeps the suite as a living regression baseline: green = behavior is stable, red = a deliberate behavior change landed and the expectation needs to move with it.

## Notes

- The `→` character in the spec represents tab characters (`\t`).
- The expected string is **not** a blind copy of today's output — it keeps the CommonMark structure (e.g. `<p>`, `<pre><code>`, nesting) so the failure clearly names the missing behavior, while the whitespace is written realistically.
- This project intentionally does **not** chase full CommonMark conformance; see `NTokenizers.Tests.ToHtml.CommonMark.Compliance` for the strict-spec counterpart.
