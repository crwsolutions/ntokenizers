namespace NTokenizers.Markup.Metadata;

/// <summary>
/// Markup metadata with inline tokenization support.
/// </summary>
public abstract class InlineMarkupMetadata<TToken>() : MarkupMetadata, IInlineMarkupMedata
{
    /// <intheritdoc/>
    public bool IsProcessing { get; set; } = true;

    private readonly ManualResetEventSlim _callbackReady = new();

    /// <summary>
    /// Callback to stream syntax-highlighted tokens from the code block.
    /// When set, the tokenizer will delegate to language-specific tokenizers and emit tokens via this callback.
    /// </summary>
    public Action<TToken>? OnInlineToken
    {
        get => _onInlineToken;
        set
        {
            _onInlineToken = value;
            if (value is not null)
            {
                _callbackReady.Set();
            }
        }
    }
    private Action<TToken>? _onInlineToken;

    /// <intheritdoc/>
    public void WaitForCallbackClient()
    {
        if (_onInlineToken is not null)
        {
            return;
        }

        if (!_callbackReady.Wait(1000))
        {
            //throw new TimeoutException("Callback client was never assigned.");
        }
    }
}
