using System.Collections.Concurrent;
using System.Net;
using System.Text;
using Poc.Core;
using Poc.Web;

var builder = WebApplication.CreateBuilder(args);

// The whole point of the PoC: Kestrel with synchronous I/O disallowed.
// Any sync write to response.Body would throw
// "Synchronous operations are disallowed for this Stream". The pipeline
// must stay fully async.
//builder.WebHost.ConfigureKestrel(options =>
//{
//    options.AllowSynchronousIO = false;
//});

var app = builder.Build();

// --- Self-check (in-memory, not through Kestrel) -------------------
app.MapGet("/self-check", async () =>
{
    var records = new List<(string Path, string Type, string Value)>();

    // Per-path tracking: the invariant that proves the core awaits each
    // handler is that sibling handlers for the same path are never in
    // flight together. Nesting across paths is recursion, not parallelism.
    var perPathActive = new ConcurrentDictionary<string, int>();
    var entries = new ConcurrentQueue<(string Path, int Depth)>();

    int Enter(string path)
    {
        var depth = perPathActive.AddOrUpdate(path, 1, (_, v) => v + 1);
        entries.Enqueue((path, depth));
        return depth;
    }

    void Exit(string path) => perPathActive.AddOrUpdate(path, 1, (_, v) => v - 1);

    async Task RenderMd(MarkdownToken token, string path)
    {
        Enter(path);
        try
        {
            records.Add((path, token.Type.ToString(), token.Value));

            if (token.Metadata is CodeBlockMetadata codeMeta)
            {
                records.Add((path, nameof(MarkdownTokenType.CodeBlock), codeMeta.Language));
                await codeMeta.ParseInlinesAsync(code => RenderCode(code, $"{path}/code:{codeMeta.Language}"));
            }
            else if (token.Metadata is BlockquoteMetadata quoteMeta)
            {
                records.Add((path, nameof(MarkdownTokenType.Blockquote), string.Empty));
                await quoteMeta.ParseInlinesAsync(inner => RenderMd(inner, $"{path}/quote"));
            }
        }
        finally
        {
            Exit(path);
        }
    }

    async Task RenderCode(CodeToken token, string path)
    {
        Enter(path);
        try
        {
            records.Add((path, token.Type.ToString(), token.Value));
        }
        finally
        {
            Exit(path);
        }
    }

    await MarkdownTokenizer.Create()
        .ParseAsync(new StringReader(PocSample.Markdown), token => RenderMd(token, "root"));

    var maxDepth = entries.Max(e => e.Depth);

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

    var checks = new (string Id, string Name, bool Pass, string Detail)[]
    {
        ("S1", "handler awaited per token (sibling handlers never concurrent)", entries.All(e => e.Depth == 1), $"max nesting depth = {maxDepth} (expected 3)"),
        ("S2", "code sub-tokenizer emitted tokens", records.Any(r => r.Path == "root/code:csharp"), string.Empty),
        ("S3", "recursion (nested markdown + nested code block)", records.Any(r => r.Path == "root/quote/code:js"), string.Empty),
        ("S4", "completion (nothing in flight after ParseAsync)", perPathActive.Values.All(v => v == 0), $"in flight = {perPathActive.Values.Sum()}"),
        ("S5", "handler fault propagates through ParseAsync", fault is InvalidOperationException, fault?.Message ?? "no fault observed"),
    };

    var allPassed = checks.All(c => c.Pass);
    app.Logger.LogInformation("Self-check: {Outcome} ({Passed}/{Total})",
        allPassed ? "PASSED" : "FAILED", checks.Count(c => c.Pass), checks.Length);

    return Results.Json(new
    {
        allPassed,
        checks = checks.Select(c => new { id = c.Id, name = c.Name, pass = c.Pass, detail = c.Detail }),
    });
});

// --- /stream-html: the real Kestrel path ----------------------------
app.MapGet("/stream-html", async (HttpContext context, int? delay) =>
{
    var response = context.Response;
    response.ContentType = "text/html; charset=utf-8";

    // Writes through WriteAsync only; any sync write throws (see the writer).
    var writer = new AsyncOnlyTextWriter(response.Body, Encoding.UTF8);

    await writer.WriteAsync(
        "<!doctype html><html><head><meta charset=\"utf-8\"><title>PoC</title></head><body>\n");

    await MarkdownTokenizer.Create()
        .ParseAsync(new StringReader(PocSample.Markdown),
            token => RenderHtmlAsync(token, writer, delay ?? 0));

    await writer.WriteAsync("</body></html>\n");
});

app.Run();

// --- Renderers (local functions) ------------------------------------

async Task RenderHtmlAsync(MarkdownToken token, TextWriter writer, int delayMs)
{
    if (delayMs > 0)
    {
        // Simulate slow production so progressive delivery is visible.
        await Task.Delay(delayMs);
    }

    switch (token.Type)
    {
        case MarkdownTokenType.Heading:
            await writer.WriteAsync($"<h1>{WebUtility.HtmlEncode(token.Value)}</h1>\n");
            break;

        case MarkdownTokenType.Text:
            await writer.WriteAsync($"<p>{WebUtility.HtmlEncode(token.Value)}</p>\n");
            break;

        case MarkdownTokenType.CodeBlock when token.Metadata is CodeBlockMetadata codeMeta:
            await writer.WriteAsync($"<div class=\"code-block\" data-lang=\"{WebUtility.HtmlEncode(codeMeta.Language)}\">\n");
            await codeMeta.ParseInlinesAsync(code => WriteCodeTokenAsync(code, writer, delayMs));
            await writer.WriteAsync("</div>\n");
            break;

        case MarkdownTokenType.Blockquote when token.Metadata is BlockquoteMetadata quoteMeta:
            await writer.WriteAsync("<blockquote>\n");

            // Recursion: the same handler for the nested markdown, same pacing.
            await quoteMeta.ParseInlinesAsync(inner => RenderHtmlAsync(inner, writer, delayMs));

            await writer.WriteAsync("</blockquote>\n");
            break;
    }
}

async Task WriteCodeTokenAsync(CodeToken code, TextWriter writer, int delayMs)
{
    if (delayMs > 0)
    {
        // Same pacing as the top-level handler: the sub-tokenizer's tokens
        // arrive one by one too, so the streaming is visible in the block.
        await Task.Delay(delayMs);
    }

    // Whitespace (incl. newlines) is written raw: layout is preserved.
    var html = code.Type switch
    {
        CodeTokenType.Whitespace => code.Value,
        _ => $"<span class=\"tok-{code.Type.ToString().ToLowerInvariant()}\">{WebUtility.HtmlEncode(code.Value)}</span>",
    };

    await writer.WriteAsync(html);
}
