namespace NTokenizers.Core;

/// <summary>
/// Markdown metadata with inline tokenization support.
/// </summary>
public abstract class InlineMetadata<TToken>() : InlineMetadata where TToken : IToken
{
    private readonly TaskCompletionSource<Action<TToken>> _onInlineTokenTcs = new();

    private Action<TToken>? _onInlineToken;

    // Awaitable getter for the handler (parser uses this)
    internal Task<Action<TToken>> GetInlineTokenHandlerAsync() => _onInlineTokenTcs.Task;

    /// <summary>
    /// Client calls this to register the handler and signal; returns processing Task for awaiting
    /// </summary>
    public Task<bool> RegisterInlineTokenHandler(Action<TToken> handler)
    {
        _onInlineToken = handler ?? throw new ArgumentNullException(nameof(handler));
        _onInlineTokenTcs.TrySetResult(handler);
        return _processingTcs.Task;
    }
}

/// <summary>
/// Metadata with inline tokenization support.
/// </summary>
public abstract class InlineMetadata : Metadata
{
    /// <summary>
    /// Task that completes when processing is done
    /// </summary>
    internal protected readonly TaskCompletionSource<bool> _processingTcs = new();

    // Parser calls this to signal completion
    internal void CompleteProcessing(Exception? ex = null)
    {
        if (ex != null)
        {
            _processingTcs.TrySetException(ex);
        }
        else
        {
            _processingTcs.TrySetResult(true);
        }
    }
}