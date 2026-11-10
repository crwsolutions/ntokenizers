using System.Text;

namespace NTokenizers.Core;

/// <summary>
/// Metadata for a block whose content is parsed by a sub-tokenizer.
/// </summary>
/// <remarks>
/// The client drives the inline parse: it calls <see cref="ParseInlinesAsync"/> from inside
/// its handler for the container token; the sub-tokenizer runs there and the inline handler
/// is awaited for every sub-token. The call completes when the section is fully consumed, so
/// the container handler can do its closing work right after the await. There is no
/// registration window and no rendezvous: the await inside the container handler is the
/// coordination.
/// </remarks>
/// <typeparam name="TToken">The type of tokens emitted by the sub-tokenizer.</typeparam>
public abstract class InlineMetadata<TToken> : Metadata where TToken : IToken
{
    /// <summary>
    /// Initializes the metadata with the section plumbing the sub-tokenizer reads from:
    /// the section reader, the shared capture builder (all characters, incl. the section)
    /// and the lookahead queue (the parent's shared buffer, or a private one).
    /// </summary>
    internal protected InlineMetadata(TextReader reader, StringBuilder capture, Queue<char> lookahead)
    {
        Reader = reader;
        Capture = capture;
        Lookahead = lookahead;
    }

    /// <summary>The section reader the sub-tokenizer reads from.</summary>
    internal protected TextReader Reader { get; }

    /// <summary>The capture builder that records every character of the stream.</summary>
    internal protected StringBuilder Capture { get; }

    /// <summary>The lookahead queue the sub-tokenizer reads from (shared with the parent).</summary>
    internal protected Queue<char> Lookahead { get; }

    /// <summary>
    /// Runs the block's sub-tokenizer, awaiting <paramref name="onToken"/> for every
    /// sub-token. Must be called from the container token's handler. Completes when the
    /// section is fully consumed, so the handler can do its closing work right after the
    /// await.
    /// </summary>
    /// <param name="onToken">The function to await for each sub-token.</param>
    /// <param name="ct">A token to monitor for cancellation requests.</param>
    public Task ParseInlinesAsync(Func<TToken, Task> onToken, CancellationToken ct = default)
    {
        if (onToken is null) throw new ArgumentNullException(nameof(onToken));
        return RunInlinesAsync(onToken, ct);
    }

    /// <summary>
    /// Runs the block's sub-tokenizer over its section. Implemented by each metadata type,
    /// which carries the section reader, capture builder, lookahead queue and (for code
    /// blocks) the stop delimiter, plus the sub-tokenizer it runs.
    /// </summary>
    internal abstract Task RunInlinesAsync(Func<TToken, Task> onToken, CancellationToken ct);
}
