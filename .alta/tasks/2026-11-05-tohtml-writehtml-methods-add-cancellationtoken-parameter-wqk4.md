---
title: "ToHtml WriteHtml* methods: add CancellationToken parameter"
kind: gap
status: pending
created: 2026-11-05
summary: "WriteHtmlAsync/WriteHtmlDocumentAsync create an internal CancellationTokenSource that is never cancelled and expose no CancellationToken, so a caller cannot abort conversion of a stream that never ends; the console extension WriteMarkdownAsync does take a CancellationToken."
---

## Why

`MarkdownConverter.WriteHtmlAsync`/`WriteHtmlDocumentAsync` create an internal `CancellationTokenSource` that is never cancelled (src/NTokenizers/ToHtml/MarkdownConverter.cs:60-62) and expose no `CancellationToken` parameter, so a caller cannot abort conversion of a stream that never ends (e.g. a hung AI Pipe). The console equivalent does support cancellation: `AnsiConsoleMarkdownExtensions.WriteMarkdownAsync(..., CancellationToken ct)` (NTokenizers.Extensions.Spectre.Console, AnsiConsoleMarkdownExtensions.cs:23).

## What to do

Add a `CancellationToken ct = default` parameter (or overloads) to the four `WriteHtml*` methods and pass the token to the existing internal `ParseAsync` overload instead of the throw-away internal CTS. Add a test that a hung stream (e.g. a Pipe that never completes) aborts promptly and returns the consumed prefix.

## Where

src/NTokenizers/ToHtml/MarkdownConverter.cs; tests in tests/NTokenizers.Tests/ToHtml/MarkdownToHtmlTests.cs
