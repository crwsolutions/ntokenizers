---
layout: default
title: "Migrating from v8 to v9"
---

# Migrating from v8 to v9

Version 9.0.0 is a **breaking** release with one core change: **token emission is now async end-to-end**. The token handler you pass to a tokenizer is no longer a synchronous callback; it is an async function the tokenizer *awaits for every token it produces*. That await is the producer/consumer boundary: the tokenizer paces itself to your code, and ordering, completion, and error propagation hold by construction.

```csharp
// 8.x — the handler is an Action<CSharpToken>
string text = csharpTokenizer.Parse(input, token => Render(token));

// 9.0 — the handler becomes a Func<CSharpToken, Task>
string text = csharpTokenizer.Parse(input, async token => await RenderAsync(token));
```

Everything else in the release is a consequence of that change.

## Summary of breaking changes

| 8.x | 9.0 | Where |
|-----|-----|-------|
| Handler delegate `Action<TToken>` | `Func<TToken, Task>` | Every `Parse(...)` / `ParseAsync(...)` entry point of every tokenizer |
| `metadata.RegisterInlineTokenHandler(handler, onCompleted?)` | `await metadata.ParseInlinesAsync(handler)` | Markdown inline sections (headings, blockquotes, list items, tables, code blocks) |
| Fire-and-forget inline rendezvous with a 2000 ms timeout | Awaited inline sub-parse | `InlineMetadata<T>` |
| `onInlinesCompleted` completion callback | Code after the `await` | — |

## Upgrade checklist

Work through this top to bottom; each section below gives the details and code.

- [ ] Bump the `NTokenizers` package reference to **9.0.0**
- [ ] Change every token handler from `Action<TToken>` to `Func<TToken, Task>` (the compiler finds them all: every `Parse` / `ParseAsync` call)
- [ ] Replace every `RegisterInlineTokenHandler(...)` call with `await ...ParseInlinesAsync(...)`
- [ ] Remove any `onInlinesCompleted` callback — the code after the `await` now runs when the section is fully consumed
- [ ] Make sure the surrounding handler methods are `async` and the awaits are chained (in particular: **no un-awaited `ParseInlinesAsync`**)
- [ ] Build, run your test suite, and verify token output order visually (streamed render, not just the final buffer)

Useful greps to find everything:

```text
RegisterInlineTokenHandler   →   ParseInlinesAsync
onInlinesCompleted           →   (code after the await)
Action<MarkdownToken>        →   Func<MarkdownToken, Task>   (and one per token type you handle)
Task.Run.*Async.*GetAwaiter  →   direct sync-over-async     (avoid Task.Run for sync wrappers)
ContinueWith                 →   await directly             (avoid ContinueWith for completion work)
```

## 1. Token handlers become async functions

Every entry point that took a handler now takes a `Func<TToken, Task>`. If your handler body is synchronous, an `async` lambda (or an explicit `Task.CompletedTask`) is all you need:

```csharp
// 8.x
string text = csharpTokenizer.Parse(input, token =>
{
    Render(token);                       // synchronous Action<CSharpToken>
});

// 9.0
string text = csharpTokenizer.Parse(input, async token =>
{
    await RenderAsync(token);            // Func<CSharpToken, Task>
});
```

The in-memory convenience overloads are unchanged in shape: `Parse(string)`, `Parse(Stream, ...)` and `ParseAsync(Stream, ...)` all still exist — only the handler delegate type moved. The `Parse(Stream, ...)` / `Parse(string)` sync overloads remain safe *for in-memory input* (sync-over-async on a memory stream, no real IO).

Because the parse loop now **awaits your handler for every token**, the tokenizer paces itself to your consumer:

- Slow handler → slow token stream. There is no internal token buffer.
- An exception thrown in your handler propagates out of the awaited `ParseAsync` — no more swallowed callback errors.
- Cancellation tokens behave as documented: the loop stops at the next await boundary.

## 2. Inline sub-parsing: `RegisterInlineTokenHandler` → `ParseInlinesAsync`

This is the change that affects Markdown consumers most. In 8.x, when a container token (blockquote, heading, list item, table, code block) arrived, you *registered* a handler for its inlines; the tokenizer picked it up through an internal rendezvous (a `TaskCompletionSource` with a 2000 ms timeout) and the inlines streamed in while your parent handler moved on. The `await` you often wrote on `RegisterInlineTokenHandler` only guaranteed the handler was *picked up* — not that the inlines were consumed.

In 9.0 the call runs the sub-parse where you call it, and the `await` genuinely completes when the section is fully consumed:

```csharp
// 8.x
await markdownTokenizer.ParseAsync(stream, token =>
{
    if (token.Type == MarkdownTokenType.BlockquoteStart)
    {
        var blockquote = (BlockquoteMetadata)token.Metadata;
        // rendezvous: returns as soon as the handler is picked up
        _ = blockquote.RegisterInlineTokenHandler(inlineToken =>
        {
            RenderInline(inlineToken);
        }, onInlinesCompleted: () => CloseBlockquote());
    }
});

// 9.0
await markdownTokenizer.ParseAsync(stream, async token =>
{
    if (token.Type == MarkdownTokenType.BlockquoteStart)
    {
        var blockquote = (BlockquoteMetadata)token.Metadata;
        await blockquote.ParseInlinesAsync(async inlineToken =>
        {
            await RenderInlineAsync(inlineToken);
        });
        CloseBlockquote();   // section fully consumed: closing work is safe here
    }
});
```

Code blocks work the same way — the language metadata (`CSharpCodeBlockMetadata`, `JsonCodeBlockMetadata`, …) all inherit `ParseInlinesAsync` from `InlineMetadata<T>`:

```csharp
// 9.0
var csharp = (CSharpCodeBlockMetadata)token.Metadata;
await csharp.ParseInlinesAsync(async csharpToken =>
{
    await RenderCSharpAsync(csharpToken);
});
```

> [!WARNING]
> **Always await `ParseInlinesAsync`.** The sub-tokenizer and the parent share the same reader and lookahead buffer. Calling `ParseInlinesAsync` without awaiting it puts two consumers on that shared reader and corrupts the token order. The rename from `Register` to `Parse…Async` exists to make the awaited contract obvious.

> [!NOTE]
> `ParseInlinesAsync` is supported by the metadata that actually carries a sub-parsed section: `HeadingMetadata`, `BlockquoteMetadata`, `ListItemMetadata`, `OrderedListItemMetadata`, `TableMetadata`, and every `CodeBlockMetadata<T>`. `ListMetadata` (the *list start/end* tokens) deliberately does **not** carry a section — list items are streamed by the tokenizer itself — and its `ParseInlinesAsync` throws `NotSupportedException` by design. Parse the list *item*, not the list marker.

> [!NOTE]
> The rename is also an honest one: the old `RegisterInlineTokenHandler` never merely registered — it started the parse. `ParseInlinesAsync` says what it does.

## 3. ToHtml

The public surface of the ToHtml API is **unchanged** — `ToHtmlAsync` / `WriteHtmlAsync` / `ToHtmlDocument*` already existed in the v8 line. What changed internally:

- `MarkdownConverter` now writes straight to your `TextWriter` through async paths only (`WriteAsync` / `FlushAsync`), so the async methods work with writers over streams that disallow synchronous IO (for example a Kestrel response body).
- `ToHtml(string)` / `ToHtmlDocument(string)` remain as in-memory convenience wrappers (sync-over-async on a `StringWriter`) — fine for string input, not for real IO.

## Pitfalls and gotchas

### Don't use `ContinueWith` for post-parse work

A common mistake is using `ContinueWith` to run completion work after `ParseInlinesAsync`:

```hemlock
// ❌ BAD: runs on a different thread, can cause race conditions and deadlocks
var t = meta.ParseInlinesAsync(handler);
t.ContinueWith(_ => context.Commit());

// ✅ GOOD: await directly and run completion work on the same thread
await meta.ParseInlinesAsync(handler);
context.Commit();
```

The old `onInlinesCompleted` callback ran on the parsing thread. With `ContinueWith`, the completion work runs on a thread pool thread, which can cause race conditions when multiple sections are parsed concurrently.

### Don't use `Task.Run` for sync-over-async

When wrapping async methods in synchronous APIs, avoid `Task.Run`:

```hemlock
// ❌ BAD: runs on a different thread, breaks Spectre.Console cursor handling
var t = Task.Run(() => WriteMarkdownAsync(...));
t.GetAwaiter().GetResult();

// ✅ GOOD: same thread, no cursor issues
return WriteMarkdownAsync(...).GetAwaiter().GetResult();
```

The `Task.Run` pattern is appropriate for true parallelism, but for sync-over-async on in-memory streams, it introduces unnecessary thread hopping and can break console cursor management.

### Spectre.Console Live regions on Windows

If you use Spectre.Console Live regions (e.g., for streaming table rendering), be aware that they have Windows-specific issues with cursor handling. This is not related to the v9 migration, but it can surface after upgrading. Workarounds include:

- Upgrading Spectre.Console to the latest version
- Avoiding Live regions for table rendering (build table first, display once)
- Using try/finally to ensure Live region cleanup

## What did NOT change

- Token types, token values, and metadata properties on every format (token streams are byte-identical to 8.x for the same input, modulo the now-awaited delivery)
- Whitespace preservation and the streaming model (bounded lookahead, per-character processing)
- Fence language aliases (`csharp` / `cs` / `c#`, `xml` / `xaml` / `svg`, …)
- `Parse(string)` and the other in-memory conveniences
- `MarkdownConverter.GetCss()` and the ToHtml output HTML
