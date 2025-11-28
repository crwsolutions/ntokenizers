using NTokenizers.Core;
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
    public static GenericTokenizer Create() => new GenericTokenizer();

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
                _sb.Append(toProcess);
                if (char.IsWhiteSpace(toProcess))
                {
                    EmitPending();
                }
            }

            if (delQueue.Count == delLength && new string(delQueue.ToArray()) == delimiter)
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
                _sb.Append(toProcess);
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
        if (_sb.Length > 0)
        {
            _onToken(new MarkupToken(MarkupTokenType.Text, _sb.ToString()));
            _sb.Clear();
        }
    }
}
