using NTokenizers.Core;
using NTokenizers.Extensions;
using NTokenizers.Markup;

namespace NTokenizers.Generic;

/// <summary>
/// A generic tokenizer that reads text until a specified stop delimiter is encountered.
/// </summary>
public sealed class GenericTokenizer : BaseSubTokenizer<MarkupToken>
{
    /// <summary>
    /// Creates a new instance of the <see cref="GenericTokenizer"/> class.
    /// </summary>
    public static GenericTokenizer Create() => new();

    /// <summary>
    /// Parses the input text reader and invokes the onToken action for each token found,
    /// </summary>
    internal protected override void Parse()
    {
        string delimiter = _stopDelimiter ?? string.Empty;
        int delLength = delimiter.Length;

        var delQueue = new Queue<char>();
        bool stoppedByDelimiter = false;
        while (true)
        {
            int ic = _reader.Read();
            if (ic == -1)
            {
                break;
            }
            char c = (char)ic;

            delQueue.Enqueue(c);

            if (delQueue.Count > delLength)
            {
                char toProcess = delQueue.Dequeue();
                _buffer.Append(toProcess);
                if (toProcess == ' ')
                {
                    EmitPending();
                }
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
                _buffer.Append(toProcess);
            }
        }

        if (stoppedByDelimiter)
        {
            StripFinalLineFeed();
        }

        EmitPending();
    }

    private void EmitPending()
    {
        if (_buffer.Length > 0)
        {
            _onToken(new MarkupToken(MarkupTokenType.Text, _buffer.ToString()));
            _buffer.Clear();
        }
    }
}
