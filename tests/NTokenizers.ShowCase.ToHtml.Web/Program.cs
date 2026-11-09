using NTokenizers.ToHtml;
using NTokenizers.ShowCase.ToHtml.Web;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MinRequestBodyDataRate = null;
    serverOptions.Limits.MinResponseDataRate = null;
});

var app = builder.Build();

// Enable static files serving
app.UseDefaultFiles();
app.UseStaticFiles();

var injectedCss = MarkdownConverter.GetCss();

app.MapGet("/", (HttpContext context) =>
{
    var html = HtmlBuilder.GenerateIndexHtml(injectedCss);
    context.Response.ContentType = "text/html; charset=utf-8";
    return Results.Text(html);
});

app.MapPost("/stream-html", async (HttpContext context) =>
{
    context.Response.ContentType = "text/html; charset=utf-8";

    try
    {
        using var reader = new StreamReader(context.Request.Body);
        var body = await reader.ReadToEndAsync();
        var request = JsonSerializer.Deserialize<MarkdownRequest>(body) ?? 
            throw new ArgumentException("Invalid request");

        // Write HTML header + CSS immediately (no delay)
        var headerBytes = Encoding.UTF8.GetBytes("<!DOCTYPE html>\n<html>\n<head>\n<style>\n" + injectedCss + "\n</style>\n</head>\n<body>\n");
        await context.Response.Body.WriteAsync(headerBytes, 0, headerBytes.Length);
        await context.Response.Body.FlushAsync();

        // Now stream the markdown content; the ToHtml pipeline writes to the
        // writer only through WriteAsync/FlushAsync, chunk by chunk.
        using var markdownStream = new MemoryStream(Encoding.UTF8.GetBytes(request.Markdown));
        using var streamingWriter = new StreamingTextWriter(context.Response.Body);

        await MarkdownConverter.WriteHtmlAsync(markdownStream, streamingWriter);

        // Close HTML document
        await streamingWriter.WriteAsync("\n</body>\n</html>");
    }
    catch (Exception ex)
    {
        // Response may have already started, so we cannot set status code
        // Just log the error
        Console.WriteLine($"Error in /stream-html: {ex.Message}\n{ex.StackTrace}");
        throw;
    }
});

app.Run();

class MarkdownRequest
{
    public required string Markdown { get; set; }
}

/// <summary>
/// A TextWriter that writes async chunks directly to the response body, one
/// WriteAsync + FlushAsync per chunk. The ToHtml pipeline only ever calls
/// WriteAsync/FlushAsync on it (its sync members throw, like Kestrel's guard
/// does on the response stream). A short per-chunk delay keeps the progressive
/// rendering visible in the browser.
/// </summary>
class StreamingTextWriter : TextWriter
{
    private readonly Stream _responseBody;
    private readonly Random _rng = new();

    public override Encoding Encoding => Encoding.UTF8;

    public StreamingTextWriter(Stream responseBody)
    {
        _responseBody = responseBody ?? throw new ArgumentNullException(nameof(responseBody));
    }

    public override void Write(char value) => throw new NotSupportedException("Use async writes; Kestrel disallows synchronous IO on the response stream.");
    public override void Write(string? value) => throw new NotSupportedException("Use async writes; Kestrel disallows synchronous IO on the response stream.");
    public override void Flush() => throw new NotSupportedException("Use FlushAsync.");

    public override Task WriteAsync(string? value)
        => string.IsNullOrEmpty(value) ? Task.CompletedTask : WriteChunkAsync(value);

    public override Task FlushAsync() => _responseBody.FlushAsync();

    private async Task WriteChunkAsync(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        await _responseBody.WriteAsync(bytes, 0, bytes.Length);
        await _responseBody.FlushAsync();
        // Short delay per chunk keeps the progressive rendering visible in the browser.
        await Task.Delay(_rng.Next(50, 101));
    }
}
