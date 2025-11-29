namespace NTokenizers.Markup.Metadata;

/// <summary>
/// Metadata interface for inline tokens.
/// </summary>
public interface IInlineMarkupMedata
{
    /// <summary>
    /// Is true if there are still more tokens to be processed in the code block, else false.
    /// </summary>
    bool IsProcessing { get; set; }

    /// <summary>
    /// Waits until the OnInlineToken callback is set by the client.
    /// </summary>
    void WaitForCallbackClient();
}
