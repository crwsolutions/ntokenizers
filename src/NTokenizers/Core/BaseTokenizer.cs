using System.Text;

namespace NTokenizers.Core;

/// <summary>
/// Abstract base class for tokenizers that provides common functionality for parsing text streams.
/// </summary>
/// <typeparam name="TToken">The type of token to be produced by the tokenizer.</typeparam>
public abstract class BaseTokenizer<TToken> where TToken : IToken
{
    /// <summary>
    /// Buffer for characters that have been read ahead
    /// </summary>
    internal protected readonly Queue<char> _lookaheadBuffer = new();

    private TextReader _reader = default!;
    private StringBuilder _stringBuilder = default!;

    /// <summary>
    /// Stringbuilder that captures all characters from the stream
    /// </summary>
    internal protected StringBuilder Bob => _stringBuilder;

    /// <summary>
    /// The text reader for the input stream.
    /// </summary>
    internal protected TextReader Reader => _reader;

    /// <summary>
    /// The action to invoke for each token found.
    /// </summary>
    internal protected Action<TToken> _onToken = default!;

    /// <summary>
    /// The internal buffer for accumulating characters.
    /// </summary>
    internal protected readonly StringBuilder _buffer = new();

    /// <summary>
    /// Parses the input stream and invokes the onToken action for each token found.
    /// </summary>
    /// <param name="stream">The input stream to parse.</param>
    /// <param name="onToken">The action to invoke for each token found.</param>
    /// <returns>The input stream as string</returns>
    public async Task<string> ParseAsync(Stream stream, Action<TToken> onToken) =>
        await ParseAsync(new StreamReader(stream), new StringBuilder(), onToken);

    /// <summary>
    /// Parses the input stream and invokes the onToken action for each token found.
    /// </summary>
    /// <param name="stream">The input stream to parse.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="onToken">The action to invoke for each token found.</param>
    /// <returns>The input stream as string</returns>
    public async Task<string> ParseAsync(Stream stream, CancellationToken cancellationToken, Action<TToken> onToken) =>
        await ParseAsync(new StreamReader(stream), new StringBuilder(), cancellationToken, onToken);

    /// <summary>
    /// Parses the input stream and invokes the onToken action for each token found.
    /// </summary>
    /// <param name="stream">The input stream to parse.</param>
    /// <param name="encoding">The encoding to use when reading the stream.</param>
    /// <param name="onToken">The action to invoke for each token found.</param>
    /// <returns>The input stream as string</returns>
    public async Task<string> ParseAsync(Stream stream, Encoding encoding, Action<TToken> onToken) =>
        await ParseAsync(new StreamReader(stream, encoding), new StringBuilder(), onToken);

    /// <summary>
    /// Parses the input stream and invokes the onToken action for each token found.
    /// </summary>
    /// <param name="stream">The input stream to parse.</param>
    /// <param name="encoding">The encoding to use when reading the stream.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="onToken">The action to invoke for each token found.</param>
    /// <returns>The input stream as string</returns>
    public async Task<string> ParseAsync(Stream stream, Encoding encoding, CancellationToken cancellationToken, Action<TToken> onToken) =>
        await ParseAsync(new StreamReader(stream, encoding), new StringBuilder(), cancellationToken, onToken);

    /// <summary>
    /// Parses the input stream and invokes the onToken action for each token found.
    /// </summary>
    /// <param name="stream">The input stream to parse.</param>
    /// <param name="onToken">The action to invoke for each token found.</param>
    /// <returns>The input stream as string</returns>
    public string Parse(Stream stream, Action<TToken> onToken) =>
        ParseAsync(stream, onToken).GetAwaiter().GetResult();

    /// <summary>
    /// Parses the input stream and invokes the onToken action for each token found.
    /// </summary>
    /// <param name="stream">The input stream to parse.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="onToken">The action to invoke for each token found.</param>
    /// <returns>The input stream as string</returns>
    public string Parse(Stream stream, CancellationToken cancellationToken, Action<TToken> onToken) =>
        ParseAsync(stream, cancellationToken, onToken).GetAwaiter().GetResult();

    /// <summary>
    /// Parses the input stream and invokes the onToken action for each token found.
    /// </summary>
    /// <param name="stream">The input stream to parse.</param>
    /// <param name="encoding">The encoding to use when reading the stream.</param>
    /// <param name="onToken">The action to invoke for each token found.</param>
    /// <returns>The input stream as string</returns>
    public string Parse(Stream stream, Encoding encoding, Action<TToken> onToken) =>
        ParseAsync(stream, encoding, onToken).GetAwaiter().GetResult();

    /// <summary>
    /// Parses the input stream and invokes the onToken action for each token found.
    /// </summary>
    /// <param name="stream">The input stream to parse.</param>
    /// <param name="encoding">The encoding to use when reading the stream.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="onToken">The action to invoke for each token found.</param>
    /// <returns>The input stream as string</returns>
    public string Parse(Stream stream, Encoding encoding, CancellationToken cancellationToken, Action<TToken> onToken) =>
        ParseAsync(stream, encoding, cancellationToken, onToken).GetAwaiter().GetResult();

    /// <summary>
    /// Parses the input string and returns a list of tokens found.
    /// </summary>
    /// <param name="input">The input string to parse.</param>
    /// <returns>A list of tokens found in the input string.</returns>
    public List<TToken> Parse(string input)
    {
        _stringBuilder = new StringBuilder();
        var tokens = new List<TToken>();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        ParseAsync(stream, tokens.Add).GetAwaiter().GetResult();
        return tokens;
    }

    /// <summary>
    /// Parses the input text reader and invokes the onToken action for each token found.
    /// </summary>
    /// <param name="reader">The text reader to parse.</param>
    /// <param name="stringBuilder">Stringbuilder that captures all characters from the stream</param>
    /// <param name="onToken">The action to invoke for each token found.</param>
    /// <returns>The input stream as string</returns>
    internal async Task<string> ParseAsync(TextReader reader, StringBuilder stringBuilder, Action<TToken> onToken)
    {
        _reader = reader;
        _onToken = onToken;
        _stringBuilder = stringBuilder;

        await ParseAsync(CancellationToken.None);

        return _stringBuilder.ToString();
    }

    /// <summary>
    /// Parses the input text reader and invokes the onToken action for each token found.
    /// </summary>
    /// <param name="reader">The text reader to parse.</param>
    /// <param name="stringBuilder">Stringbuilder that captures all characters from the stream</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="onToken">The action to invoke for each token found.</param>
    /// <returns>The input stream as string</returns>
    internal async Task<string> ParseAsync(TextReader reader, StringBuilder stringBuilder, CancellationToken cancellationToken, Action<TToken> onToken)
    {
        _reader = reader;
        _onToken = onToken;
        _stringBuilder = stringBuilder;

        await ParseAsync(cancellationToken);

        return _stringBuilder.ToString();
    }

    /// <summary>
    /// Performs the actual parsing logic. This method must be implemented by derived classes.
    /// </summary>
    /// <param name="ct">A token to monitor for cancellation requests.</param>
    internal protected abstract Task ParseAsync(CancellationToken ct);

    internal int Peek()
    {
        if (_lookaheadBuffer.Count > 0)
            return _lookaheadBuffer.Peek();
        return _reader.Peek();
    }

    internal virtual int Read()
    {
        if (_lookaheadBuffer.Count > 0)
            return _lookaheadBuffer.Dequeue();
        var c = _reader.Read();
        if (c != -1)
        {
            _stringBuilder.Append((char)c);
        }
        return c;
    }

    internal char PeekAhead(int offset)
    {
        while (_lookaheadBuffer.Count <= offset)
        {
            int next = _reader.Read();
            if (next == -1) return '\0';
            _lookaheadBuffer.Enqueue((char)next);
            _stringBuilder.Append((char)next);
        }
        return _lookaheadBuffer.ElementAt(offset);
    }
}
