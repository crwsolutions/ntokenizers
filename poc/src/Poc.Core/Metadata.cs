namespace Poc.Core;

/// <summary>
/// Base class for the block-structure metadata attached to tokens.
/// </summary>
public abstract class Metadata
{
}

/// <summary>
/// Metadata for a block whose content is parsed by a sub-tokenizer.
/// <para>
/// This is the heart of the architecture: the client drives the inline parse.
/// It calls <see cref="ParseInlinesAsync"/> from inside its handler for the
/// container token; the sub-tokenizer runs there and the inline handler is
/// awaited for every sub-token. The call completes when the section is fully
/// consumed, so the container handler can do its closing work right after
/// the await. There is no registration window and no rendezvous: the await
/// inside the container handler is the coordination.
/// </para>
/// </summary>
public abstract class InlineMetadata<TToken> : Metadata where TToken : IToken
{
    private readonly TextReader _section;
    private readonly Func<BaseTokenizer<TToken>> _createTokenizer;

    protected InlineMetadata(TextReader section, Func<BaseTokenizer<TToken>> createTokenizer)
    {
        _section = section;
        _createTokenizer = createTokenizer;
    }

    /// <summary>
    /// Runs the sub-tokenizer over the section, awaiting <paramref name="onToken"/>
    /// for every sub-token. Must be called from the container token's handler.
    /// Completes when the section is fully consumed.
    /// </summary>
    public async Task ParseInlinesAsync(Func<TToken, Task> onToken)
    {
        var tokenizer = _createTokenizer();
        await tokenizer.ParseAsync(_section, onToken);
    }
}

/// <summary>
/// Metadata for a fenced code block. The section is the block content;
/// the sub-tokenizer is a <see cref="CodeTokenizer"/>.
/// </summary>
public sealed class CodeBlockMetadata : InlineMetadata<CodeToken>
{
    public CodeBlockMetadata(string language, TextReader section)
        : base(section, () => new CodeTokenizer())
    {
        Language = language;
    }

    /// <summary>The fence info string (language).</summary>
    public string Language { get; }
}

/// <summary>
/// Metadata for a blockquote. The section is the quote content (markers
/// stripped); the sub-tokenizer is a nested <see cref="MarkdownTokenizer"/>,
/// so the inline tokens are <see cref="MarkdownToken"/> and the client
/// handler recurses with the same method.
/// </summary>
public sealed class BlockquoteMetadata : InlineMetadata<MarkdownToken>
{
    public BlockquoteMetadata(TextReader section)
        : base(section, () => new MarkdownTokenizer())
    {
    }
}
