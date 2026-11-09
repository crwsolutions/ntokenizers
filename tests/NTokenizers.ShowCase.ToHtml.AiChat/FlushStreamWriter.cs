using System.Text;

namespace NTokenizers.ShowCase.ToHtml.AiChat;

/// <summary>
/// A TextWriter over a response stream that writes and flushes asynchronously:
/// every WriteAsync is followed by a FlushAsync, so content reaches the client
/// per chunk. Only async writes are supported — synchronous Write/Flush throw,
/// which mirrors what Kestrel enforces on the response body.
/// (Not a StreamWriter subclass: on .NET 8+, any StreamWriter subclass loses
/// the base class's async-aware flush and falls back to synchronous IO, which
/// Kestrel disallows.)
/// </summary>
public class FlushStreamWriter : TextWriter
{
    private readonly Stream _stream;
    private readonly UTF8Encoding _encoding = new(encoderShouldEmitUTF8Identifier: false);

    public FlushStreamWriter(Stream stream)
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
    }

    /// <inheritdoc/>
    public override Encoding Encoding => _encoding;

    public override void Write(char value)
        => throw new NotSupportedException("Use WriteAsync; Kestrel disallows synchronous IO on the response stream.");

    public override void Write(string? value)
        => throw new NotSupportedException("Use WriteAsync; Kestrel disallows synchronous IO on the response stream.");

    public override void Write(char[] buffer, int index, int count)
        => throw new NotSupportedException("Use WriteAsync; Kestrel disallows synchronous IO on the response stream.");

    public override void Flush()
        => throw new NotSupportedException("Use FlushAsync; Kestrel disallows synchronous IO on the response stream.");

    public override Task WriteAsync(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return Task.CompletedTask;
        }

        var bytes = Encoding.GetBytes(value);
        return WriteAndFlushAsync(bytes);
    }

    public override Task FlushAsync() => _stream.FlushAsync();

    private async Task WriteAndFlushAsync(byte[] bytes)
    {
        await _stream.WriteAsync(bytes.AsMemory());
        await _stream.FlushAsync();
    }
}
