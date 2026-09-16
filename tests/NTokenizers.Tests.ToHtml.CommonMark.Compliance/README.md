# CommonMark Spec Compliance Tests

These tests verify that `MarkdownConverter.ToHtml()` conforms to the [CommonMark Spec 0.31.2](https://spec.commonmark.org/0.31.2/).

## Overview

651 test cases from the spec, organized by section:

### Preliminaries (41 tests)

| File | Tests |
|------|-------|
| TabsTests.cs | 11 |
| BackslashEscapesTests.cs | 13 |
| EntityReferencesTests.cs | 17 |

### Leaf blocks (185 tests)

| File | Tests |
|------|-------|
| ThematicBreaksTests.cs | 19 |
| AtxHeadingsTests.cs | 18 |
| SetextHeadingsTests.cs | 27 |
| IndentedCodeBlocksTests.cs | 12 |
| FencedCodeBlocksTests.cs | 29 |
| HtmlBlocksTests.cs | 44 |
| LinkReferenceDefinitionsTests.cs | 27 |
| ParagraphsTests.cs | 8 |
| BlankLinesTests.cs | 1 |

### Container blocks (100 tests)

| File | Tests |
|------|-------|
| BlockQuotesTests.cs | 25 |
| ListItemsTests.cs | 48 |
| ListsTests.cs | 27 |

### Inlines (325 tests)

| File | Tests |
|------|-------|
| CodeSpansTests.cs | 22 |
| EmphasisTests.cs | 132 |
| LinksTests.cs | 90 |
| ImagesTests.cs | 22 |
| AutolinksTests.cs | 19 |
| RawHtmlTests.cs | 20 |
| HardLineBreaksTests.cs | 15 |
| SoftLineBreaksTests.cs | 2 |
| TextualContentTests.cs | 2 |

### Other

| File | Tests |
|------|-------|
| PrecedenceTests.cs | 1 |

## How to use

Run all tests:
```bash
dotnet test tests/NTokenizers.Tests.ToHtml.CommonMark.Compliance
```

Run a specific section:
```bash
dotnet test tests/NTokenizers.Tests.ToHtml.CommonMark.Compliance --filter "FullyQualifiedName~ThematicBreaksTests"
```

## Notes

- The `→` character in the spec represents tab characters (`\t`)
- HTML entities like `&quot;` and `&amp;` are preserved as-is in expected output
- Each test compares `MarkdownConverter.ToHtml(input)` with the spec's expected HTML
