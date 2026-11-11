using System.Text;
using NTokenizers.ToHtml;

namespace ToHtml;

/// <summary>
/// Verifies that the ToHtml output path only issues WriteAsync/FlushAsync to the
/// caller's writer, so it is safe for streams that disallow synchronous IO (such
/// as a Kestrel response body), and that the buffered output stays byte-identical
/// to the fragment/document conversion.
/// </summary>
public class ToHtmlAsyncSafetyTests
{
    /// <summary>
    /// A rich document exercising headings, inline formatting, lists, blockquotes,
    /// a table, fenced code blocks in several languages, indented code, and a
    /// thematic break.
    /// </summary>
    private const string RichMarkdown = """
        # Heading One

        Some text with **bold**, *italic*, `inline code` and a [link](https://example.com).

        - first item
        - second item
          - nested item

        1. one
        2. two

        > quoted first line
        > quoted second line

        | Name | Value |
        | ----- | ----- |
        | alpha | 1     |
        | beta  | 2     |

        ```csharp
        var x = 1;
        Console.WriteLine(x);
        ```

        ```python
        def double(n):
            return n * 2
        ```

        ```js
        const answer = 42;
        ```

            indented code line
            indented code line two

        ---

        Final paragraph.
        """;

    // --- WriteHtmlAsync (fragment) ---

    [Fact]
    public async Task WriteHtmlAsync_UsesOnlyAsyncWrites_AndOutputIsByteIdentical()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(RichMarkdown));
        using var writer = new SyncGuardedTextWriter();

        var markdown = await MarkdownConverter.WriteHtmlAsync(stream, writer);

        // The guarded writer throws on any synchronous Write/Flush; reaching this
        // point means the pipeline only used WriteAsync/FlushAsync.
        Assert.Equal(RichMarkdown, markdown);
        Assert.Equal(MarkdownConverter.ToHtml(RichMarkdown), writer.GetOutput());

        // The output arrived in multiple ordered chunks (progressive streaming),
        // concatenated byte-identical to the direct conversion.
        Assert.True(writer.Chunks.Count > 1, $"Expected multiple chunks, got {writer.Chunks.Count}.");
        Assert.Equal(writer.GetOutput(), string.Concat(writer.Chunks));

        // Exactly one flush, the drain's single final flush.
        Assert.Equal(1, writer.FlushCount);
    }

    [Fact]
    public async Task WriteHtmlAsync_EmptyInput_CompletesWithoutChunks()
    {
        using var stream = new MemoryStream(Array.Empty<byte>());
        using var writer = new SyncGuardedTextWriter();

        var markdown = await MarkdownConverter.WriteHtmlAsync(stream, writer);

        Assert.Equal(MarkdownConverter.ToHtml(string.Empty), markdown);
        Assert.Equal(MarkdownConverter.ToHtml(string.Empty), writer.GetOutput());
        Assert.Empty(writer.Chunks);
        Assert.Equal(1, writer.FlushCount);
    }

    // --- WriteHtmlDocumentAsync (full document) ---

    [Fact]
    public async Task WriteHtmlDocumentAsync_UsesOnlyAsyncWrites_AndOutputIsByteIdentical()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(RichMarkdown));
        using var writer = new SyncGuardedTextWriter();

        var markdown = await MarkdownConverter.WriteHtmlDocumentAsync(stream, writer);

        Assert.Equal(RichMarkdown, markdown);

        var output = writer.GetOutput();
        Assert.Equal(MarkdownConverter.ToHtmlDocument(RichMarkdown), output);
        Assert.StartsWith("<!DOCTYPE html>", output);
        Assert.EndsWith("</html>" + Environment.NewLine, output);
        Assert.Equal(1, writer.FlushCount);
    }

    // --- Failure propagation ---

    [Fact]
    public async Task WriteHtmlAsync_WriterWriteAsyncThrows_SurfacesExceptionToCaller()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(RichMarkdown));
        using var writer = new SyncGuardedTextWriter();
        var fault = new IOException("simulated client disconnect");
        writer.FaultWriteAsync(fault);

        var exception = await Assert.ThrowsAsync<IOException>(
            () => MarkdownConverter.WriteHtmlAsync(stream, writer));

        // The original exception instance is preserved, not a wrapper.
        Assert.Same(fault, exception);
    }

    // --- Guard double sanity ---

    [Fact]
    public void SyncGuardedTextWriter_ThrowsOnSynchronousWrites()
    {
        using var writer = new SyncGuardedTextWriter();

        Assert.Throws<NotSupportedException>(() => writer.Write('a'));
        Assert.Throws<NotSupportedException>(() => writer.Write("a"));
        Assert.Throws<NotSupportedException>(() => writer.Flush());
    }

    /// <summary>
    /// Simulates a writer over a Kestrel response body: synchronous Write/Flush
    /// throw (Kestrel's guard), while WriteAsync/FlushAsync record what they
    /// receive. WriteLineAsync is implemented the way an async-capable writer
    /// (e.g. StreamWriter) does it — through WriteAsync — since the base
    /// TextWriter.WriteLineAsync falls back to synchronous Write.
    /// </summary>
    private sealed class SyncGuardedTextWriter : TextWriter
    {
        private readonly StringBuilder _output = new();
        private readonly List<string> _chunks = new();
        private int _flushCount;
        private Exception? _writeFault;

        /// <summary>Gets the text received through WriteAsync, in call order.</summary>
        public string GetOutput() => _output.ToString();

        /// <summary>Gets the individual WriteAsync chunks, in call order.</summary>
        public IReadOnlyList<string> Chunks => _chunks;

        /// <summary>Gets the number of FlushAsync calls received.</summary>
        public int FlushCount => _flushCount;

        /// <summary>Makes the next WriteAsync/FlushAsync call throw the given exception.</summary>
        public void FaultWriteAsync(Exception exception) => _writeFault = exception;

        public override Encoding Encoding => Encoding.UTF8;

        public override void Write(char value) => ThrowSync();

        public override void Write(string? value) => ThrowSync();

        public override void Write(char[] buffer, int index, int count) => ThrowSync();

        public override void Flush() => ThrowSync();

        public override Task WriteAsync(string? value)
        {
            if (_writeFault is not null)
                throw _writeFault;

            if (!string.IsNullOrEmpty(value))
            {
                _chunks.Add(value);
                _output.Append(value);
            }

            return Task.CompletedTask;
        }

        public override Task FlushAsync()
        {
            if (_writeFault is not null)
                throw _writeFault;

            _flushCount++;
            return Task.CompletedTask;
        }

        public override Task WriteLineAsync() => WriteAsync(NewLine);

        public override Task WriteLineAsync(string? value)
            => value is null ? WriteAsync(NewLine) : WriteAsync(value + NewLine);

        private static void ThrowSync()
            => throw new NotSupportedException("Synchronous IO is disallowed (Kestrel guard simulation).");
    }
}
