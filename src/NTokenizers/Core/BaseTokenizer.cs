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
    /// Parses the input string and returns a list of tokens found.
    /// </summary>
    /// <param name="input">The input string to parse.</param>
    /// <returns>A list of tokens found in the input string.</returns>
    public List<TToken> Parse(string input)
    {
        var tokens = new List<TToken>();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        Parse(stream, tokens.Add);
        return tokens;
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

    /// <summary>
    /// Removes a final line feed (CR, LF, or CRLF) from the internal StringBuilder, if present. 
    /// </summary>
    internal protected void StripFinalLineFeed()
    {
        if (_sb.Length > 0)
        {
            if (_sb[_sb.Length - 1] == '\n')
            {
                _sb.Length -= 1;
                if (_sb.Length > 0 && _sb[_sb.Length - 1] == '\r')
                {
                    _sb.Length -= 1;
                }
            }
            else if (_sb[_sb.Length - 1] == '\r')
            {
                _sb.Length -= 1;
            }
        }
    }
}
