---
layout: default
title: "ToHtml"
---

# ToHtml API

The ToHtml API provides a simple way to convert Markdown content to HTML output. It is built on top of the `MarkdownTokenizer` and is **fully stream-capable**: it consumes a Markdown *stream* and writes the HTML *stream* incrementally — each token is written to the output `TextWriter` as soon as it is parsed, so a live AI chat response or a large file can be converted in real time without buffering the whole document.

## Overview

The `MarkdownConverter` class offers two conversion modes:

- **Fragment** — Converts Markdown to an HTML fragment (body content only, no document wrapper).
- **Document** — Converts Markdown to a complete HTML document with `DOCTYPE`, `<head>`, embedded CSS, and a copy-to-clipboard script for code blocks.

## Public API

### Fragment Methods

| Method | Description |
|--------|-------------|
| `ToHtml(string input)` | Synchronously converts a Markdown string to an HTML fragment. |
| `ToHtmlAsync(Stream inputStream)` | Asynchronously converts a Markdown stream to an HTML fragment. |
| `WriteHtmlAsync(Stream inputStream, TextWriter writer)` | Asynchronously converts a Markdown stream and writes the fragment directly to a `TextWriter`. |
| `WriteHtmlAsync(TextReader reader, TextWriter writer)` | Asynchronously converts a Markdown `TextReader` and writes the fragment directly to a `TextWriter`. |

### Document Methods

| Method | Description |
|--------|-------------|
| `ToHtmlDocument(string input)` | Synchronously converts a Markdown string to a full HTML document. |
| `ToHtmlDocumentAsync(Stream inputStream)` | Asynchronously converts a Markdown stream to a full HTML document. |
| `WriteHtmlDocumentAsync(Stream inputStream, TextWriter writer)` | Asynchronously converts a Markdown stream and writes the full document directly to a `TextWriter`. |
| `WriteHtmlDocumentAsync(TextReader reader, TextWriter writer)` | Asynchronously converts a Markdown `TextReader` and writes the full document directly to a `TextWriter`. |

### Utility Methods

| Method | Description |
|--------|-------------|
| `GetCss()` | Returns the default CSS required for rendering Markdown HTML output. |

## Usage Examples

### Basic String Conversion

```csharp
using NTokenizers.ToHtml;

string markdown = "# Hello\n\nThis is **bold** text.";

// Fragment (body-only)
string html = MarkdownConverter.ToHtml(markdown);

// Full HTML document
string document = MarkdownConverter.ToHtmlDocument(markdown);
```

### Stream-to-Stream Conversion

The key strength of the ToHtml API is its **stream-capable** design, allowing efficient processing of large files:

```csharp
using NTokenizers.ToHtml;

string inputPath = "input.md";
string outputPath = "output.html";

using var inputStream = new FileStream(inputPath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true);
using var outputStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true);
using var writer = new StreamWriter(outputStream, leaveOpen: false);

await MarkdownConverter.WriteHtmlDocumentAsync(inputStream, writer);
```

### Getting the Default CSS

If you need to apply the default styling in your own HTML template:

```csharp
using NTokenizers.ToHtml;

string css = MarkdownConverter.GetCss();
```

## Full HTML Document Features

When using the document methods (`ToHtmlDocument*`), the output includes:

- `<!DOCTYPE html>` declaration
- `<html lang="en">` root element with a fixed `<title>Converted Markdown</title>`
- `<head>` with charset, viewport meta tags
- Embedded CSS styles for headings, code blocks, tables, blockquotes, and more
- A JavaScript `copyCode()` function for copy-to-clipboard support on code blocks; the button text switches to `Copied!` for 1.5 seconds after a successful copy
- `<body>` wrapping the converted Markdown content

## CommonMark Compliance

The conversion follows the [CommonMark 0.31.2](https://spec.commonmark.org/0.31.2/) reference. The output differs from the reference implementation in a small number of documented places — the differences, and the features that are intentionally not supported, are listed on the [CommonMark Compliance](/ntokenizers/commonmark) page.

## See Also

- [Markdown Tokenizer](/ntokenizers/markdown)
- [Home](/ntokenizers/)
