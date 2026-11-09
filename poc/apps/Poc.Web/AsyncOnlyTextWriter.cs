using System.Text;

namespace Poc.Web;

/// <summary>
/// A <see cref="TextWriter"/> that writes asynchronously only. Every
/// synchronous write method throws, so if anything in the pipeline attempts
/// a sync write the request fails loudly instead of silently violating
/// Kestrel's no-sync-IO rule. It owns nothing: bytes go straight to the
/// stream per call and the stream is never closed by this writer (the
/// response body belongs to Kestrel).
/// </summary>
internal sealed class AsyncOnlyTextWriter : TextWriter
{
    private readonly Stream _stream;
    private readonly Encoding _encoding;

    public AsyncOnlyTextWriter(Stream stream, Encoding encoding)
    {
        _stream = stream;
        _encoding = encoding;
    }

    public override Encoding Encoding => _encoding;

    // NewLine is not overridden: nothing in the pipeline writes it, and the
    // base default is fine.

    // --- Synchronous writes: forbidden ---------------------------------

    public override void Write(char value) => ThrowSyncWrite();

    public override void Write(string? value) => ThrowSyncWrite();

    public override void Write(char[]? buffer) => ThrowSyncWrite();

    public override void Write(char[] buffer, int index, int count) => ThrowSyncWrite();

    public override void Write(ReadOnlySpan<char> buffer) => ThrowSyncWrite();

    // The boxed/scalar overloads (Write(int), Write(object), ...) are not
    // overridden: the base implementations funnel into the string or
    // char[] overload above, so they throw as well.

    // --- Asynchronous writes: the only way out --------------------------

    // .NET 10 TextWriter virtual async surface: WriteAsync(char),
    // WriteAsync(string), WriteAsync(char[], int, int) and the
    // WriteAsync(ReadOnlyMemory<char>, CancellationToken) sink. Everything
    // is funneled into the sink so there is exactly one write path.
    public override Task WriteAsync(char value) =>
        WriteAsync(new ReadOnlyMemory<char>(new[] { value }), default);

    public override Task WriteAsync(string? value) =>
        value is null ? Task.CompletedTask : WriteAsync(value.AsMemory(), default);

    public override Task WriteAsync(char[] buffer, int index, int count) =>
        WriteAsync(new ReadOnlyMemory<char>(buffer, index, count), default);

    public override Task WriteAsync(ReadOnlyMemory<char> buffer, CancellationToken cancellationToken = default)
    {
        if (buffer.Length == 0)
        {
            return Task.CompletedTask;
        }

        var bytes = new byte[_encoding.GetByteCount(buffer.Span)];
        var written = _encoding.GetBytes(buffer.Span, bytes.AsSpan());
        return _stream.WriteAsync(bytes.AsMemory(0, written), cancellationToken).AsTask();
    }

    public override void Flush()
    {
        // No buffer of its own: every write went straight to the stream.
    }

    protected override void Dispose(bool disposing)
    {
        // Deliberately a no-op: nothing to flush and the stream is not owned.
    }

    private static void ThrowSyncWrite() =>
        throw new NotSupportedException(
            "Synchronous writes are not allowed in the PoC: the write path must be fully async.");
}
