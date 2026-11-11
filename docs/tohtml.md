---
layout: default
title: "ToHtml"
---

# ToHtml API

```csharp
string html = await MarkdownConverter.ToHtmlAsync(markdown);
```

The ToHtml API provides a simple way to convert Markdown content to HTML output. It is built on top of the `MarkdownTokenizer` and is **fully stream-capable**: it consumes a Markdown *stream* and writes the HTML *stream* incrementally — each token is written to the output `TextWriter` as soon as it is parsed, so a live AI chat response or a large file can be converted in real time without buffering the whole document.

> [!NOTE]
> **Async first:** the `ToHtmlAsync`/`WriteHtml*` methods are the primary surface and write only through `WriteAsync`/`FlushAsync`, so they work with writers over streams that disallow synchronous IO (for example a Kestrel response body). The sync `ToHtml(string)`/`ToHtmlDocument(string)` methods are in-memory convenience wrappers (sync-over-async on a `StringWriter`) — safe for string input, but not for real IO.

## Overview

The `MarkdownConverter` class offers two conversion modes:

- **Fragment** — Converts Markdown to an HTML fragment (body content only, no document wrapper).
- **Document** — Converts Markdown to a complete HTML document with `DOCTYPE`, `<head>`, embedded CSS, and a copy-to-clipboard script for code blocks.

## Public API

> [!NOTE]
> **Naming:** the `ToHtml*` methods return the **HTML** string, while the `WriteHtml*` methods write the HTML to your `TextWriter` and return the **consumed Markdown** source text (the raw input exactly as read — for the `Stream` overloads, decoded with any BOM detected and stripped).

### Fragment Methods

| Method | Description |
|--------|-------------|
| `ToHtmlAsync(string input)` | Asynchronously converts a Markdown string to an HTML fragment. |
| `ToHtml(string input)` | In-memory convenience: synchronously converts a Markdown string to an HTML fragment (sync-over-async). |
| `ToHtmlAsync(Stream inputStream)` | Asynchronously converts a Markdown stream to an HTML fragment. |
| `WriteHtmlAsync(Stream inputStream, TextWriter writer)` | Asynchronously converts a Markdown stream, writes the fragment directly to a `TextWriter`, and returns the consumed Markdown source text. |
| `WriteHtmlAsync(TextReader reader, TextWriter writer)` | Asynchronously converts a Markdown `TextReader`, writes the fragment directly to a `TextWriter`, and returns the consumed Markdown source text. |

### Document Methods

| Method | Description |
|--------|-------------|
| `ToHtmlDocumentAsync(string input)` | Asynchronously converts a Markdown string to a full HTML document. |
| `ToHtmlDocument(string input)` | In-memory convenience: synchronously converts a Markdown string to a full HTML document (sync-over-async). |
| `ToHtmlDocumentAsync(Stream inputStream)` | Asynchronously converts a Markdown stream to a full HTML document. |
| `WriteHtmlDocumentAsync(Stream inputStream, TextWriter writer)` | Asynchronously converts a Markdown stream, writes the full document directly to a `TextWriter`, and returns the consumed Markdown source text. |
| `WriteHtmlDocumentAsync(TextReader reader, TextWriter writer)` | Asynchronously converts a Markdown `TextReader`, writes the full document directly to a `TextWriter`, and returns the consumed Markdown source text. |

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
string html = await MarkdownConverter.ToHtmlAsync(markdown);

// Full HTML document
string document = await MarkdownConverter.ToHtmlDocumentAsync(markdown);
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

string consumedMarkdown = await MarkdownConverter.WriteHtmlDocumentAsync(inputStream, writer);
```

### Capturing the Consumed Markdown

The `WriteHtml*` methods return the Markdown source text they consumed, so a caller that renders a live stream — for example an AI chat response — can keep the exact text it just rendered. This is useful for storing assistant replies in a chat history:

```csharp
using NTokenizers.ToHtml;

// markdownStream produces the Markdown as it arrives (for example, an AI chat stream)
using var writer = new StringWriter();
string markdown = await MarkdownConverter.WriteHtmlAsync(markdownStream, writer);
// writer.ToString() holds the rendered HTML fragment;
// markdown holds the same Markdown source text that was consumed.
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
