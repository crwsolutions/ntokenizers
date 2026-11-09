# Poc: async-handler + sub-tokenizer architecture

Proof project for the architecture we designed for NTokenizers: the tokenizer
core **awaits the client handler per token** (`Func<TToken, Task>`), and the
client **drives the inline (sub-tokenizer) runs** through
`InlineMetadata<TToken>.ParseInlinesAsync`. No `Action<TToken>`, no
`RegisterInlineTokenHandler` rendezvous, no 2000 ms window, no fire-and-forget.

## Layout

| Project | Role |
|---|---|
| `src/Poc.Core` | The architecture: `BaseTokenizer<TToken>` (await per token), `MarkdownTokenizer` (delegates), `CodeTokenizer` (char-based sub-tokenizer), `InlineMetadata<TToken>.ParseInlinesAsync`, `CodeBlockMetadata` / `BlockquoteMetadata`. |
| `apps/Poc.Console` | Console validation: renders the sample and checks C1–C6 (order, sub-tokenizer, recursion, per-token awaiting, completion, error propagation). |
| `apps/Poc.Web` | Kestrel validation: `AllowSynchronousIO = false`, writes through `WriteAsync` only (sync writes throw), `/stream-html` streams the HTML, `/self-check` runs the in-memory architecture checks. |
| `validate-web.ps1` | Client-side validation of the web app (status, first token, content markers, completeness). |

## Run

```powershell
# 1. Console validation
dotnet run --project apps/Poc.Console

# 2. Web validation (two terminals)
dotnet run --project apps/Poc.Web --urls http://localhost:5080
# terminal 2:
.\validate-web.ps1
```

## What is proven

| # | Claim |
|---|---|
| 1 | The parse loop awaits the handler per token: ordering, completion and error propagation hold by construction (C4/C5/C6, S1/S4/S5). |
| 2 | Sub-tokenizers run client-driven through `ParseInlinesAsync` — no registration window, no 2000 ms rendezvous (C2, S2). |
| 3 | Recursion works: markdown inside a blockquote reuses the same handler (C3, S3). |
| 4 | The write path is fully async: Kestrel with `AllowSynchronousIO = false` serves the stream, and the `TextWriter` throws on any sync write (W1–W4). |
| 5 | Progressive delivery: the first token arrives well before the end (W2). |

## Deliberate simplifications versus NTokenizers

- The parent collects each section into a `StringReader` instead of handing
  the child the shared reader with a stop delimiter. The architectural
  contract (client-driven, awaited per token) is identical; this keeps the
  PoC small.
- The markdown dialect is tiny: headings, paragraphs, fenced code blocks and
  blockquotes. Lists/tables would follow the code-block/quote pattern.
- No `netstandard2.0` target: the PoC targets `net10.0`. The contract itself
  (`Func<TToken, Task>`) works on any target.
