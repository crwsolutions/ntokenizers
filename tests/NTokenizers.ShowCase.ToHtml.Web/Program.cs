using NTokenizers.ToHtml;
using NTokenizers.ShowCase.ToHtml.Web;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

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

        // Now stream the markdown content with delays
        using var markdownStream = new MemoryStream(Encoding.UTF8.GetBytes(request.Markdown));
        using var streamingWriter = new StreamingTextWriter(context.Response.Body);

        // Start the background drain task that reads from channel and writes to response
        var drainTask = streamingWriter.StartDrainAsync();

        // Stream the markdown HTML fragment (uses sync Write internally, goes through channel)
        await MarkdownConverter.WriteHtmlAsync(markdownStream, streamingWriter);

        // Close HTML document
        await streamingWriter.WriteAsync("\n</body>\n</html>");

        // Signal completion and wait for drain
        streamingWriter.Complete();
        await drainTask;
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
/// A TextWriter that uses a Channel to bridge sync and async writes.
/// Sync writes (from HTML writers) push data into the channel.
/// A background task reads from the channel and writes to the response body with delays.
/// This avoids sync IO issues with Kestrel while providing a realistic streaming effect.
/// </summary>
class StreamingTextWriter : TextWriter
{
    private readonly Stream _responseBody;
    private readonly Channel<byte[]> _channel;
    private readonly Random _rng = new();
    private readonly MemoryStream _buffer = new();
    private Task? _drainTask;
    private const int FlushInterval = 30; // Flush channel every N characters
    private int _charCount = 0;

    public override Encoding Encoding => Encoding.UTF8;

    public StreamingTextWriter(Stream responseBody)
    {
        _responseBody = responseBody ?? throw new ArgumentNullException(nameof(responseBody));
        _channel = Channel.CreateBounded<byte[]>(new BoundedChannelOptions(100)
        {
            FullMode = BoundedChannelFullMode.Wait,
        });
    }

    /// <summary>
    /// Starts the background task that drains the channel and writes to the response body.
    /// </summary>
    public Task StartDrainAsync()
    {
        _drainTask = Task.Run(async () =>
        {
            await foreach (var chunk in _channel.Reader.ReadAllAsync())
            {
                if (chunk != null)
                {
                    await _responseBody.WriteAsync(chunk, 0, chunk.Length);
                    await _responseBody.FlushAsync();
                }
                // Small delay between chunks for streaming effect
                await Task.Delay(_rng.Next(125, 200));
            }
        });
        return _drainTask;
    }

    private bool _completed;

    /// <summary>
    /// Signals that no more data will be written.
    /// </summary>
    public void Complete()
    {
        if (_completed) return;
        _completed = true;

        // Flush remaining buffer
        if (_buffer.Length > 0)
        {
            var data = _buffer.ToArray();
            _buffer.SetLength(0);
            _channel.Writer.TryWrite(data);
        }
        _channel.Writer.Complete();
    }

    // Sync Write - writes to in-memory buffer, flushes to channel periodically
    public override void Write(char value)
    {
        var bytes = Encoding.UTF8.GetBytes(new[] { value });
        _buffer.Write(bytes, 0, bytes.Length);
        _charCount++;
        if (_charCount % FlushInterval == 0)
            FlushBufferToChannel();
    }

    // Sync Write string - writes to in-memory buffer, flushes to channel periodically
    public override void Write(string? value)
    {
        if (value == null) return;
        foreach (var c in value)
        {
            Write(c);
        }
    }

    // Async Write char - writes to buffer with delay, then flushes
    public override async Task WriteAsync(char value)
    {
        var bytes = Encoding.UTF8.GetBytes(new[] { value });
        _buffer.Write(bytes, 0, bytes.Length);
        _charCount++;
        await Task.Delay(_rng.Next(0, 2));
        if (_charCount % FlushInterval == 0)
            FlushBufferToChannel();
    }

    // Async Write string - with per-character delay
    public override async Task WriteAsync(string? value)
    {
        if (value == null) return;
        foreach (var c in value)
        {
            await WriteAsync(c);
        }
    }

    public override async Task WriteLineAsync(string? value)
    {
        if (value != null)
        {
            foreach (var c in value)
                await WriteAsync(c);
        }
        await WriteAsync('\n');
    }

    private void FlushBufferToChannel()
    {
        if (_buffer.Length > 0)
        {
            var data = _buffer.ToArray();
            _buffer.SetLength(0);
            // Use Task.Run to avoid sync-over-async deadlock with Kestrel
            Task.Run(async () => await _channel.Writer.WriteAsync(data)).Wait();
        }
    }

    public override void Flush()
    {
        FlushBufferToChannel();
    }

    public override Task FlushAsync()
    {
        FlushBufferToChannel();
        return Task.CompletedTask;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Complete();
            _buffer.Dispose();
        }
        base.Dispose(disposing);
    }
}