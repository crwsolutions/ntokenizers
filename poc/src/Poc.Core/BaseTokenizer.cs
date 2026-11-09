namespace Poc.Core;

/// <summary>
/// Base class for the PoC tokenizers.
/// <para>
/// The whole architecture in one line: the parse loop awaits the handler for
/// every token. The handler may do arbitrary async work (I/O, rendering,
/// running a sub-tokenizer); the producer paces itself to the consumer.
/// Ordering, completion and error propagation hold by construction, because
/// every token goes through <see cref="EmitAsync"/>.
/// </para>
/// </summary>
public abstract class BaseTokenizer<TToken> where TToken : IToken
{
    private TextReader _reader = default!;
    private Func<TToken, Task> _onToken = default!;

    /// <summary>The input stream.</summary>
    protected TextReader Reader => _reader;

    /// <summary>
    /// Parses the input stream and awaits the handler for every token.
    /// Completes only after the last token's handler has completed.
    /// </summary>
    public async Task ParseAsync(TextReader reader, Func<TToken, Task> onToken, CancellationToken ct = default)
    {
        _reader = reader;
        _onToken = onToken;
        await ParseCoreAsync(ct);
    }

    /// <summary>The tokenizer's parse loop, implemented by derived classes.</summary>
    protected abstract Task ParseCoreAsync(CancellationToken ct);

    /// <summary>
    /// Emits a token: awaits the client handler. This await is the
    /// producer/consumer boundary.
    /// </summary>
    protected Task EmitAsync(TToken token) => _onToken(token);
}
