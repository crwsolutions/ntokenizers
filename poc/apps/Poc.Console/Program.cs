using System.Collections.Concurrent;
using Poc.Core;

// ────────────────────────────────────────────────────────────────────
// PoC console validation: the async-handler + sub-tokenizer
// architecture.
//
// The client subscribes with one async handler (a local function, so it
// can recurse for the blockquote). The parse loop awaits the handler for
// every token, and the inline (sub-tokenizer) runs are client-driven
// through ParseInlinesAsync.
//
// Checks:
//   C1  top-level token order and completeness
//   C2  code sub-tokenizer emits CodeTokens (incl. preserved newlines)
//   C3  recursion: blockquote -> nested markdown -> nested code block
//   C4  per-token awaiting: sibling handlers (same path) never concurrent
//   C5  completion: nothing in flight after ParseAsync returns
//   C6  error propagation: a handler fault surfaces through ParseAsync
// ────────────────────────────────────────────────────────────────────

const int PerTokenDelayMs = 2; // any interleaving would become visible

var records = new List<(string Path, string Type, string Value)>();

// Per-path handler tracking. The invariant that proves the core awaits
// each handler: sibling handlers for the same path are never in flight at
// the same time (the next token of a level is dispatched only after the
// previous handler completed). Nesting across paths is expected and not
// parallelism: a parent handler stays active while it awaits the
// sub-parse through ParseInlinesAsync.
var perPathActive = new ConcurrentDictionary<string, int>();
var entries = new ConcurrentQueue<(string Path, int Depth)>();

int Enter(string path)
{
    var depth = perPathActive.AddOrUpdate(path, 1, (_, v) => v + 1);
    entries.Enqueue((path, depth));
    return depth;
}

void Exit(string path) => perPathActive.AddOrUpdate(path, 1, (_, v) => v - 1);

Console.WriteLine("== Render (per-token delay: {0}ms) ==", PerTokenDelayMs);
Console.WriteLine();

// The subscription: one async handler for markdown tokens. Recursion is
// possible because it is a local function.
async Task RenderMarkdownAsync(MarkdownToken token, string path)
{
    Enter(path);

    try
    {
        await Task.Delay(PerTokenDelayMs); // simulate async work per token

        switch (token.Type)
        {
            case MarkdownTokenType.Heading:
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[h1] {token.Value}");
                Console.ResetColor();
                records.Add((path, nameof(MarkdownTokenType.Heading), token.Value));
                break;

            case MarkdownTokenType.Text:
                Console.WriteLine($"[p] {token.Value}");
                records.Add((path, nameof(MarkdownTokenType.Text), token.Value));
                break;

            case MarkdownTokenType.CodeBlock when token.Metadata is CodeBlockMetadata codeMeta:
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine($"[code:{codeMeta.Language}]");
                Console.ResetColor();
                records.Add((path, nameof(MarkdownTokenType.CodeBlock), codeMeta.Language));

                // Sub-tokenizer, client-driven, awaited per token.
                await codeMeta.ParseInlinesAsync(code => RenderCodeAsync(code, $"{path}/code:{codeMeta.Language}"));

                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine("[/code]");
                Console.ResetColor();
                break;

            case MarkdownTokenType.Blockquote when token.Metadata is BlockquoteMetadata quoteMeta:
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("[quote]");
                Console.ResetColor();
                records.Add((path, nameof(MarkdownTokenType.Blockquote), string.Empty));

                // Recursion: the same handler, passed as the inline handler.
                await quoteMeta.ParseInlinesAsync(inner => RenderMarkdownAsync(inner, $"{path}/quote"));

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("[/quote]");
                Console.ResetColor();
                break;
        }
    }
    finally
    {
        Exit(path);
    }
}

async Task RenderCodeAsync(CodeToken token, string path)
{
    Enter(path);

    try
    {
        await Task.Delay(PerTokenDelayMs);

        // Render inline (no newline of our own): the whitespace tokens
        // carry the layout, newlines included.
        var color = token.Type switch
        {
            CodeTokenType.Keyword => ConsoleColor.Blue,
            CodeTokenType.Identifier => ConsoleColor.Cyan,
            CodeTokenType.Number => ConsoleColor.Magenta,
            CodeTokenType.StringValue => ConsoleColor.Green,
            CodeTokenType.Comment => ConsoleColor.Gray,
            CodeTokenType.Whitespace => ConsoleColor.White,
            _ => ConsoleColor.Yellow,
        };

        Console.ForegroundColor = color;
        Console.Write(token.Value);
        Console.ResetColor();

        records.Add((path, token.Type.ToString(), token.Value));
    }
    finally
    {
        Exit(path);
    }
}

await MarkdownTokenizer.Create()
    .ParseAsync(new StringReader(PocSample.Markdown), token => RenderMarkdownAsync(token, "root"));

Console.WriteLine();
Console.WriteLine("== Validation ==");
Console.WriteLine();

var checks = new List<(string Id, string Name, bool Pass, string Detail)>();

// C1: top-level token order and completeness.
var topLevel = string.Join(",", records.Where(r => r.Path == "root").Select(r => r.Type));
checks.Add(("C1", "top-level token order and completeness",
    topLevel == "Heading,Text,CodeBlock,Blockquote,Text", topLevel));

// C2: code sub-tokenizer emitted CodeTokens, newlines preserved.
var code = records.Where(r => r.Path == "root/code:csharp").ToList();
var c2 = code.Any(r => r.Type == "Keyword" && r.Value == "var")
    && code.Any(r => r.Type == "StringValue" && r.Value.Contains("async"))
    && code.Any(r => r.Type == "Comment" && r.Value.StartsWith("//"))
    && code.Any(r => r.Type == "Whitespace" && r.Value.Contains('\n'));
checks.Add(("C2", "code sub-tokenizer (different token type), newlines preserved", c2,
    $"{code.Count} code tokens"));

// C3: recursion: blockquote -> nested markdown -> nested code block.
var quote = string.Join(",", records.Where(r => r.Path == "root/quote").Select(r => r.Type));
var nestedCode = records.Where(r => r.Path == "root/quote/code:js").ToList();
checks.Add(("C3", "recursion (blockquote -> markdown -> code block)",
    quote == "Text,CodeBlock,Text" && nestedCode.Any(r => r.Type == "Keyword" && r.Value == "let"),
    quote));

// C4: per-token awaiting: sibling handlers for the same path never run
// concurrently. The nesting depth is recursion, not parallelism: it is
// expected to be 3 here (markdown -> quoted markdown -> code).
var maxDepth = entries.Max(e => e.Depth);
checks.Add(("C4", "per-token awaiting (sibling handlers never concurrent)",
    entries.All(e => e.Depth == 1), $"max nesting depth = {maxDepth} (expected 3)"));

// C5: completion: no handler in flight after ParseAsync, and the stream
// ended at the last paragraph.
var last = records[^1];
var inFlight = perPathActive.Values.Sum();
checks.Add(("C5", "completion (nothing in flight after ParseAsync)",
    inFlight == 0 && last.Type == "Text" && last.Value == "Last paragraph.",
    $"in flight = {inFlight}, last = ({last.Path}, {last.Type}, \"{last.Value}\")"));

// C6: error propagation: a handler fault surfaces through ParseAsync.
Exception? fault = null;
try
{
    await MarkdownTokenizer.Create().ParseAsync(
        new StringReader("# boom"),
        _ => Task.FromException(new InvalidOperationException("simulated handler fault")));
}
catch (Exception ex)
{
    fault = ex;
}
checks.Add(("C6", "error propagation (handler fault surfaces through ParseAsync)",
    fault is InvalidOperationException, fault is null ? "no fault observed" : fault.Message));

var allPassed = true;
foreach (var (id, name, pass, detail) in checks)
{
    allPassed &= pass;
    Console.ForegroundColor = pass ? ConsoleColor.Green : ConsoleColor.Red;
    Console.WriteLine($"{(pass ? "PASS" : "FAIL")} {id} {name}");
    Console.ResetColor();
    if (!pass)
    {
        Console.WriteLine($"       detail: {detail}");
    }
}

Console.WriteLine();
Console.WriteLine(allPassed ? "ALL CHECKS PASSED" : "SOME CHECKS FAILED");
return allPassed ? 0 : 1;
