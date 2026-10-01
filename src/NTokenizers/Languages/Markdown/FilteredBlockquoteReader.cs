using System.Text;

namespace NTokenizers.Markdown;

/// <summary>
/// A <see cref="TextReader"/> wrapper that strips blockquote markers from fence code
/// content read inside a blockquote. Without it the quote's <c>&gt;</c> markers leak into
/// the code and a prefixed closing fence (<c>&gt; ```</c>) is never recognized. Stripped
/// characters are appended to the shared faithful text builder so the faithful text stays
/// identical to the input. Only the marker chain at the start of a line is removed and
/// only the current line is ever read ahead, so streaming is preserved.
/// </summary>
internal sealed class FilteredBlockquoteReader : TextReader
{
    private readonly TextReader _inner;
    private readonly int _depth;
    private readonly StringBuilder _faithful;

    // Characters the caller has not yet read.
    private readonly Queue<char> _queue = new();

    // True when the next character starts a new line of the underlying stream.
    private bool _lineStart = true;

    /// <summary>
    /// Creates a filter that strips blockquote markers from <paramref name="inner"/> while
    /// reading code content at blockquote depth <paramref name="depth"/>. Stripped marker
    /// characters are appended to <paramref name="faithful"/>.
    /// </summary>
    internal FilteredBlockquoteReader(TextReader inner, int depth, StringBuilder faithful)
    {
        _inner = inner;
        _depth = depth;
        _faithful = faithful;
    }

    /// <inheritdoc/>
    public override int Read()
    {
        if (_queue.Count > 0)
        {
            return _queue.Dequeue();
        }

        if (_lineStart)
        {
            ScanLineStart();
        }

        if (_queue.Count > 0)
        {
            return _queue.Dequeue();
        }

        return ReadFromInner();
    }

    /// <inheritdoc/>
    public override int Peek()
    {
        if (_queue.Count > 0)
        {
            return _queue.Peek();
        }

        if (_lineStart)
        {
            ScanLineStart();
        }

        if (_queue.Count > 0)
        {
            return _queue.Peek();
        }

        return _inner.Peek();
    }

    /// <summary>
    /// Reads the next character from the underlying reader. The caller's tokenizer appends it
    /// to the shared text builder, so this does not (that would double-count).
    /// </summary>
    private int ReadFromInner()
    {
        int c = _inner.Read();
        if (c == '\n' || c == '\r')
        {
            _lineStart = true;
        }

        return c;
    }

    /// <summary>
    /// At a line start, strips up to <see cref="_depth"/> markers (at most three columns of
    /// spaces or tabs, then <c>&gt;</c>, then an optional single space). Because
    /// <see cref="TextReader"/> cannot peek past the first character, the leading whitespace
    /// of a would-be marker is read temporarily and either committed to the faithful text
    /// (marker confirmed) or re-queued as code content (line not prefixed). The first content
    /// character is queued too, so the scan does not run again for it.
    /// </summary>
    private void ScanLineStart()
    {
        _lineStart = false;

        int stripped = 0;
        while (stripped < _depth)
        {
            // Read the leading whitespace of a possible marker; it is either committed to the
            // faithful text (marker confirmed) or returned to the caller (line not prefixed).
            var pending = new List<char>();
            int column = 0;
            int at = _inner.Peek();
            while (column < 4 && (at == ' ' || at == '\t'))
            {
                pending.Add((char)_inner.Read());
                column += at == '\t' ? 4 - (column % 4) : 1;
                at = _inner.Peek();
            }

            if (column >= 4 || at != '>')
            {
                // Not a marker: the whitespace is leading code indentation; return it as-is.
                foreach (var ch in pending)
                {
                    _queue.Enqueue(ch);
                }
                break;
            }

            // Marker confirmed: the whitespace, '>' and optional space are quote decoration.
            foreach (var ch in pending)
            {
                _faithful.Append(ch);
            }
            _inner.Read(); // the '>'
            _faithful.Append('>');
            stripped++;
            if (_inner.Peek() == ' ')
            {
                _inner.Read();
                _faithful.Append(' ');
            }
        }

        // Queue the first content character so the scan does not run again for it.
        int first = _inner.Peek();
        if (first != -1)
        {
            _inner.Read();
            if (first == '\n' || first == '\r')
            {
                _lineStart = true;
            }
            _queue.Enqueue((char)first);
        }
    }
}
