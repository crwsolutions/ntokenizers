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
    private bool _stop = false;
    private string delimiter = string.Empty;
    private int delLength = 0;

    /// <summary>
    /// Parses the input text reader and invokes the onToken action for each token found,
    /// stopping when the specified delimiter is encountered.
    /// </summary>
    /// <param name="reader">The text reader to parse.</param>
    /// <param name="stopDelimiter">The delimiter that stops parsing, or null to parse until end of stream.</param>
    /// <param name="onToken">The action to invoke for each token found.</param>
    public async Task<string> ParseAsync(TextReader reader, string? stopDelimiter, Action<TToken> onToken)
    {
        SetDelimiter(stopDelimiter);
        return await ParseAsync(reader, new StringBuilder(), _lookaheadBuffer, onToken);
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
        SetDelimiter(stopDelimiter);
        return await ParseAsync(reader, new StringBuilder(), _lookaheadBuffer, cancellationToken, onToken);
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
        SetDelimiter(stopDelimiter);
        await ParseAsync(reader, stringBuilder, _lookaheadBuffer, onToken);
    }

    internal async Task<string> ParseAsync(TextReader reader, StringBuilder stringBuilder, Queue<char> lookaheadBuffer, string? stopDelimiter, CancellationToken ct, Action<TToken> onToken)
    {
        SetDelimiter(stopDelimiter);
        return await ParseAsync(reader, new StringBuilder(), lookaheadBuffer, ct, onToken);
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
        SetDelimiter(stopDelimiter);
        await ParseAsync(reader, stringBuilder, _lookaheadBuffer, cancellationToken, onToken);
    }

    /// <summary>
    /// Asynchronously tokenizes characters from the input stream, processing each character using the provided async delegate.
    /// This method ensures sequential execution of the character processing tasks and handles cancellation through the provided token.
    /// </summary>
    /// <param name="ct">The cancellation token to monitor for cancellation requests.</param>
    /// <param name="processCharAsync">The asynchronous function to invoke for each character during tokenization.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    internal protected async Task TokenizeCharactersAsync(CancellationToken ct, Func<char, Task> processCharAsync)
    {
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
                await processCharAsync(c);
            }
        }
        else
        {
            while (!ct.IsCancellationRequested)
            {
                int ic = Read();
                if (ic == -1)
                {
                    break;
                }

                if (_stop)
                {
                    break;
                }

                await processCharAsync((char)ic);
            }

            if (!_stop)
            {
                while (_lookaheadBuffer.Count > 0)
                {
                    char toProcess = _lookaheadBuffer.Dequeue();
                    await processCharAsync(toProcess);
                }
            }

            if (_stop)
            {
                StripFinalLineFeed();
            }
        }
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
    internal protected void TokenizeCharacters(CancellationToken ct, Action<char> processChar)
    {
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
            while (!ct.IsCancellationRequested)
            {
                int ic = Read();
                if (ic == -1)
                {
                    break;
                }

                if (_stop)
                {
                    break;
                }

                processChar((char)ic);
            }

            if (!_stop)
            {
                while (_lookaheadBuffer.Count > 0)
                {
                    char toProcess = _lookaheadBuffer.Dequeue();
                    processChar(toProcess);
                }
            }

            if (_stop)
            {
                StripFinalLineFeed();
            }
        }
    }

    internal override int Read()
    {
        if (delLength == 0)
        {
            var ic = base.Read();
            return ic;
        }

        for (var i = _lookaheadBuffer.Count; _lookaheadBuffer.Count < delLength; i++)
        {
            var p = PeekAhead(i);
            if (p == '\0')
            {
                return base.Read();
            }
        }

        if (_lookaheadBuffer.IsEqualTo(delimiter))
        {
            _stop = true;
            _lookaheadBuffer.Clear();
            return '\0';
        }
        return base.Read();
    }

    private void SetDelimiter(string? stopDelimiter)
    {
        if (stopDelimiter is null)
        {
            return;
        }

        delimiter = stopDelimiter;
        delLength = delimiter.Length;
    }

    private void StripFinalLineFeed()
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
