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
    /// Client calls this to register the handler and signal; returns processing Task for awaiting.
    /// </summary>
    /// <param name="handler">The inline token handler.</param>
    /// <param name="onInlinesCompleted">Optional callback invoked after inline parsing completes, before the parser continues.</param>
    public Task<bool> RegisterInlineTokenHandler(Action<TToken> handler, Action? onInlinesCompleted = null)
    {
        _onInlineToken = handler ?? throw new ArgumentNullException(nameof(handler));
        _onInlinesCompleted = onInlinesCompleted;
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

    /// <summary>
    /// Optional callback invoked after inline parsing completes, before the parser continues.
    /// </summary>
    internal protected Action? _onInlinesCompleted;

    // Parser calls this to signal completion
    internal void CompleteProcessing(Exception? ex = null)
    {
        _onInlinesCompleted?.Invoke();

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