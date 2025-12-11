using NTokenizers.Extensions;
using System.Text;

namespace NTokenizers.Core;

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
    public async Task<string> ParseAsync(TextReader reader, string? stopDelimiter, Action<TToken> onToken)
    {
        _stopDelimiter = stopDelimiter;
        return await ParseAsync(reader, new StringBuilder(), onToken);
    }

    /// <summary>
    /// Parses the input text reader and invokes the onToken action for each token found,
    /// stopping when the specified delimiter is encountered.
    /// </summary>
    /// <param name="reader">The text reader to parse.</param>
    /// <param name="stopDelimiter">The delimiter that stops parsing, or null to parse until end of stream.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="onToken">The action to invoke for each token found.</param>
    public async Task<string> ParseAsync(TextReader reader, string? stopDelimiter, CancellationToken cancellationToken, Action<TToken> onToken)
    {
        _stopDelimiter = stopDelimiter;
        return await ParseAsync(reader, new StringBuilder(), cancellationToken, onToken);
    }

    /// <summary>
    /// Parses the input text reader and invokes the onToken action for each token found,
    /// stopping when the specified delimiter is encountered.
    /// </summary>
    /// <param name="reader">The text reader to parse.</param>
    /// <param name="stringBuilder">Stringbuilder that captures all characters from the stream</param>
    /// <param name="stopDelimiter">The delimiter that stops parsing, or null to parse until end of stream.</param>
    /// <param name="onToken">The action to invoke for each token found.</param>
    public async Task ParseAsync(TextReader reader, StringBuilder stringBuilder, string? stopDelimiter, Action<TToken> onToken)
    {
        _stopDelimiter = stopDelimiter;
        await ParseAsync(reader, stringBuilder, onToken);
    }

    /// <summary>
    /// Parses the input text reader and invokes the onToken action for each token found,
    /// stopping when the specified delimiter is encountered.
    /// </summary>
    /// <param name="reader">The text reader to parse.</param>
    /// <param name="stringBuilder">Stringbuilder that captures all characters from the stream</param>
    /// <param name="stopDelimiter">The delimiter that stops parsing, or null to parse until end of stream.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="onToken">The action to invoke for each token found.</param>
    public async Task ParseAsync(TextReader reader, StringBuilder stringBuilder, string? stopDelimiter, CancellationToken cancellationToken, Action<TToken> onToken)
    {
        _stopDelimiter = stopDelimiter;
        await ParseAsync(reader, stringBuilder, cancellationToken, onToken);
    }

    /// <summary>
    /// Tokenizes the input stream using the specified delimiter, or parses until the end of the stream if no delimiter is specified.
    /// </summary>
    /// <param name="ct">The cancellation token to monitor for cancellation requests.</param>
    /// <param name="processChar">The action to invoke for each character during tokenization.</param>
    /// <remarks>
    /// If no delimiter is specified, the method reads the entire input stream until the end.
    /// If a delimiter is specified, the method uses a sliding window approach to process characters
    /// until the delimiter is encountered. When the delimiter is found, the method strips any final
    /// line feed character and stops processing.
    /// </remarks>
    protected void TokenizeCharacters(CancellationToken ct, Action<char> processChar)
    {
        string delimiter = _stopDelimiter ?? string.Empty;
        int delLength = delimiter.Length;

        if (delLength == 0)
        {
            // No delimiter, parse until end of stream
            while (!ct.IsCancellationRequested)
            {
                int ic = Read();
                if (ic == -1)
                {
                    break;
                }

                char c = (char)ic;
                processChar(c);
            }
        }
        else
        {
            // With delimiter, use a sliding window
            var delQueue = new Queue<char>();
            bool stoppedByDelimiter = false;

            while (!ct.IsCancellationRequested)
            {
                int ic = Read();
                if (ic == -1)
                {
                    break;
                }

                char c = (char)ic;
                delQueue.Enqueue(c);

                if (delQueue.Count > delLength)
                {
                    char toProcess = delQueue.Dequeue();
                    processChar(toProcess);
                }

                if (delQueue.IsEqualTo(delimiter))
                {
                    stoppedByDelimiter = true;
                    break;
                }
            }

            if (!stoppedByDelimiter)
            {
                while (delQueue.Count > 0)
                {
                    char toProcess = delQueue.Dequeue();
                    processChar(toProcess);
                }
            }

            if (stoppedByDelimiter)
            {
                StripFinalLineFeed();
            }
        }
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
