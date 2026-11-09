using System.Collections.Concurrent;
using System.Text;

namespace NTokenizers.ToHtml;

/// <summary>
/// A <see cref="TextWriter"/> that performs no IO. The synchronous <c>Write</c>
/// calls issued by the HTML writers (from the tokenizers' synchronous token
/// callbacks) enqueue the text as in-memory chunks, and <see cref="Flush"/> is
/// a no-op. A single drain loop (<see cref="DrainAsync(TextWriter)"/>, started
/// by <c>MarkdownConverter.WriteHtmlAsync</c>) moves the queued chunks to the
/// caller's writer using only <c>WriteAsync</c> and <c>FlushAsync</c>, which
/// keeps the output path safe for streams that disallow synchronous IO (such
/// as a Kestrel response body) while preserving per-token progressive
/// streaming.
/// </summary>
internal sealed class BufferedHtmlTextWriter : TextWriter
{
    private readonly ConcurrentQueue<string> _queue = new();
    private readonly SemaphoreSlim _chunkAvailable = new(0);
    private volatile bool _completed;

    /// <inheritdoc/>
    public override Encoding Encoding => Encoding.UTF8;

    /// <inheritdoc/>
    public override void Write(char value)
    {
        Enqueue(value.ToString());
    }

    /// <inheritdoc/>
    public override void Write(string? value)
    {
        if (value is not null && value.Length > 0)
        {
            Enqueue(value);
        }
    }

    /// <summary>
    /// No IO to flush; flushing the caller's writer is owned by the drain loop.
    /// </summary>
    public override void Flush()
    {
        // Intentionally empty.
    }

    /// <summary>
    /// Signals that no more chunks will be enqueued and wakes the drain loop so
    /// it can finish (final flush) even when the queue is empty.
    /// </summary>
    public void Complete()
    {
        _completed = true;
        _chunkAvailable.Release();
    }

    /// <summary>
    /// Writes all queued chunks to <paramref name="writer"/> in FIFO order
    /// using only <c>WriteAsync</c>, and finishes with a single
    /// <c>FlushAsync</c>. Completes when <see cref="Complete"/> has been called
    /// and the queue is empty.
    /// </summary>
    internal async Task DrainAsync(TextWriter writer)
    {
        if (writer is null)
            throw new ArgumentNullException(nameof(writer));

        while (true)
        {
            await _chunkAvailable.WaitAsync().ConfigureAwait(false);

            while (_queue.TryDequeue(out var chunk))
            {
                await writer.WriteAsync(chunk).ConfigureAwait(false);
            }

            if (_completed)
            {
                // Complete() sets _completed after all enqueues, so once it is
                // observed as true the queue can only shrink. The final sweep
                // catches chunks enqueued just after the empty observation
                // above, then performs the single final flush.
                while (_queue.TryDequeue(out var chunk))
                {
                    await writer.WriteAsync(chunk).ConfigureAwait(false);
                }

                await writer.FlushAsync().ConfigureAwait(false);
                return;
            }
        }
    }

    private void Enqueue(string chunk)
    {
        _queue.Enqueue(chunk);
        _chunkAvailable.Release();
    }
}
