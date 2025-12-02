using System.Text;

namespace NTokenizers.Core;

/// <summary>
/// Abstract base class for tokenizers that provides common functionality for parsing text streams.
/// </summary>
/// <typeparam name="TToken">The type of token to be produced by the tokenizer.</typeparam>
public abstract class BaseTokenizer<TToken> where TToken : IToken
{
    private readonly Queue<char> _lookaheadBuffer = new();

    private TextReader _reader = default!;
    private readonly StringBuilder _stringBuilder = new();

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
    public async Task<string> ParseAsync(Stream stream, Action<TToken> onToken)
    {
        _reader = new StreamReader(stream);
        _onToken = onToken;
        await ParseAsync();
        return _stringBuilder.ToString();
    }

    /// <summary>
    /// Parses the input stream and invokes the onToken action for each token found.
    /// </summary>
    public string Parse(Stream stream, Action<TToken> onToken)
    {
        // Call the async method but block in a safe way
        return ParseAsync(stream, onToken).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Parses the input string and returns a list of tokens found.
    /// </summary>
    /// <param name="input">The input string to parse.</param>
    /// <returns>A list of tokens found in the input string.</returns>
    public List<TToken> Parse(string input)
    {
        var tokens = new List<TToken>();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        ParseAsync(stream, tokens.Add).GetAwaiter().GetResult();
        return tokens;
    }

    /// <summary>
    /// Parses the input text reader and invokes the onToken action for each token found.
    /// </summary>
    /// <param name="reader">The text reader to parse.</param>
    /// <param name="onToken">The action to invoke for each token found.</param>
    internal async Task<string> ParseAsync(TextReader reader, Action<TToken> onToken)
    {
        _reader = reader;
        _onToken = onToken;

        await ParseAsync();

        return _stringBuilder.ToString();
    }

    /// <summary>
    /// Performs the actual parsing logic. This method must be implemented by derived classes.
    /// </summary>
    internal protected abstract Task ParseAsync();

    internal int Peek()
    {
        if (_lookaheadBuffer.Count > 0)
            return _lookaheadBuffer.Peek();
        return _reader.Peek();
    }

    internal int Read()
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
    public async Task ParseAsync(TextReader reader, string? stopDelimiter, Action<TToken> onToken)
    {
        _stopDelimiter = stopDelimiter;
        await ParseAsync(reader, onToken);
    }

    /// <summary>
    /// Removes a final line feed (CR, LF, or CRLF) from the internal StringBuilder, if present. 
    /// </summary>
    internal protected void StripFinalLineFeed()
    {
        if (_buffer.Length > 0)
        {
            if (_buffer[_buffer.Length - 1] == '\n')
            {
                _buffer.Length -= 1;
                if (_buffer.Length > 0 && _buffer[_buffer.Length - 1] == '\r')
                {
                    _buffer.Length -= 1;
                }
            }
            else if (_buffer[_buffer.Length - 1] == '\r')
            {
                _buffer.Length -= 1;
            }
        }
    }
}
