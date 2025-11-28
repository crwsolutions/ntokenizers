using System.Text;

namespace NTokenizers.Core;
/// <summary>
/// Abstract base class for tokenizers that provides common functionality for parsing text streams.
/// </summary>
/// <typeparam name="TToken">The type of token to be produced by the tokenizer.</typeparam>
public abstract class BaseTokenizer<TToken> where TToken : IToken
{
    protected TextReader _reader = default!;
    protected Action<TToken> _onToken = default!;
    protected readonly StringBuilder _sb = new();

    /// <summary>
    /// Parses the input stream and invokes the onToken action for each token found.
    /// </summary>
    /// <param name="stream">The input stream to parse.</param>
    /// <param name="onToken">The action to invoke for each token found.</param>
    public void Parse(Stream stream, Action<TToken> onToken)
    {
        _reader = new StreamReader(stream);
        _onToken = onToken;
        Parse();
    }

    /// <summary>
    /// Parses the input text reader and invokes the onToken action for each token found.
    /// </summary>
    /// <param name="reader">The text reader to parse.</param>
    /// <param name="onToken">The action to invoke for each token found.</param>
    internal void Parse(TextReader reader, Action<TToken> onToken)
    {
        _reader = reader;
        _onToken = onToken;
        Parse();
    }

    /// <summary>
    /// Performs the actual parsing logic. This method must be implemented by derived classes.
    /// </summary>
    internal protected abstract void Parse();
}

/// <summary>
/// Abstract base class for sub-tokenizers that provides functionality for parsing text streams
/// with a stop delimiter.
/// </summary>
/// <typeparam name="TToken">The type of token to be produced by the sub-tokenizer.</typeparam>
public abstract class BaseSubTokenizer<TToken> : BaseTokenizer<TToken> where TToken : IToken
{
    /// <summary>
    /// delimiter that stops parsing.
    /// </summary>
    internal protected string? _stopDelimiter;

    /// <summary>
    /// Parses the input text reader and invokes the onToken action for each token found,
    /// stopping when the specified delimiter is encountered.
    /// </summary>
    /// <param name="reader">The text reader to parse.</param>
    /// <param name="stopDelimiter">The delimiter that stops parsing, or null to parse until end of stream.</param>
    /// <param name="onToken">The action to invoke for each token found.</param>
    public void Parse(TextReader reader, string? stopDelimiter, Action<TToken> onToken)
    {
        _stopDelimiter = stopDelimiter;
        Parse(reader, onToken);
    }
}
