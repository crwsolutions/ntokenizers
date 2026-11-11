using System.Text;

namespace NTokenizers.Core;

/// <summary>
/// Metadata for code block tokens, containing the language identifier.
/// </summary>
/// <remarks>
/// The metadata carries the section plumbing the sub-tokenizer runs on: the section
/// reader (which may wrap the parent's reader, e.g. to strip blockquote markers), the
/// shared capture builder, the lookahead queue and the stop delimiter (the opening
/// fence, or a closing tag such as <c>&lt;/style&gt;</c>).
/// </remarks>
/// <typeparam name="TToken">The type of tokens emitted by the language-specific tokenizer.</typeparam>
/// <param name="Language">The language identifier (e.g., "xml", "json", "csharp").</param>
/// <param name="reader">The section reader the sub-tokenizer reads from.</param>
/// <param name="capture">The capture builder that records every character of the stream.</param>
/// <param name="lookahead">The lookahead queue the sub-tokenizer reads from.</param>
/// <param name="stopDelimiter">The delimiter that stops the sub-parse (the opening fence or a closing tag).</param>
public abstract class CodeBlockMetadata<TToken>(string Language, TextReader reader, StringBuilder capture, Queue<char> lookahead, string stopDelimiter)
    : InlineMetadata<TToken>(reader, capture, lookahead), ICodeBlockMetadata where TToken : IToken
{
    /// <summary>
    /// Gets the language code representing the current language setting.
    /// </summary>
    public string Language { get; } = Language;

    private readonly string _stopDelimiter = stopDelimiter;

    internal override Task RunInlinesAsync(Func<TToken, Task> onToken, CancellationToken ct) =>
        CreateTokenizer().ParseAsync(Reader, Capture, Lookahead, _stopDelimiter, ct, onToken);

    internal abstract BaseSubTokenizer<TToken> CreateTokenizer();
}
