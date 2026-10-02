using System.Text;

namespace NTokenizers.Markdown;

/// <summary>
/// Strips a list item's content indentation and returns sibling list markers to the parent
/// tokenizer without consuming them as item content.
/// </summary>
internal sealed class ListContentReader : TextReader
{
    private readonly TextReader _inner;
    private readonly int _contentOffset;
    private readonly StringBuilder _faithful;
    private readonly Queue<ReadyCharacter> _ready = new();
    private readonly List<char> _handoff = new();
    private readonly Queue<char> _sourceReplay = new();
    private bool _lineStart;
    private bool _afterCarriageReturn;
    private bool _scopeEnded;

    internal ListContentReader(TextReader inner, int contentOffset, StringBuilder faithful, bool startsAtLineStart = false)
    {
        _inner = inner;
        _contentOffset = contentOffset;
        _faithful = faithful;
        _lineStart = startsAtLineStart;
    }

    /// <inheritdoc/>
    public override int Read()
    {
        if (_ready.Count == 0 && _lineStart && !_scopeEnded)
        {
            PrepareLine();
        }

        if (_ready.Count > 0)
        {
            return ReadReady();
        }

        if (_scopeEnded)
        {
            return -1;
        }

        int value = _inner.Read();
        UpdateLineState(value);
        return value;
    }

    /// <inheritdoc/>
    public override int Peek()
    {
        if (_ready.Count == 0 && _lineStart && !_scopeEnded)
        {
            PrepareLine();
        }

        if (_ready.Count > 0)
        {
            while (_ready.Peek().IsStripped)
            {
                _faithful.Append(_ready.Dequeue().Value);
            }

            return _ready.Count > 0 ? _ready.Peek().Value : (_scopeEnded ? -1 : _inner.Peek());
        }

        return _scopeEnded ? -1 : _inner.Peek();
    }

    /// <summary>
    /// Returns scope-ending blank lines and the sibling marker to the parent tokenizer's
    /// lookahead buffer after the item sub-document has completed.
    /// </summary>
    internal void Handoff(Queue<char> destination)
    {
        foreach (char c in _handoff)
        {
            _faithful.Append(c);
            destination.Enqueue(c);
        }
        _handoff.Clear();

        while (_sourceReplay.Count > 0)
        {
            destination.Enqueue(_sourceReplay.Dequeue());
        }
    }

    private int ReadReady()
    {
        while (_ready.Peek().IsStripped)
        {
            _faithful.Append(_ready.Dequeue().Value);
        }

        int value = _ready.Dequeue().Value;
        UpdateLineState(value);
        return value;
    }

    private void UpdateLineState(int value)
    {
        if (value == '\r')
        {
            _lineStart = true;
            _afterCarriageReturn = true;
        }
        else if (value == '\n')
        {
            _lineStart = true;
            _afterCarriageReturn = false;
        }
        else if (value != -1)
        {
            _lineStart = false;
            _afterCarriageReturn = false;
        }
    }

    private void PrepareLine()
    {
        if (_afterCarriageReturn)
        {
            _afterCarriageReturn = false;
            if (_inner.Peek() == '\n')
            {
                _ready.Enqueue(new ReadyCharacter((char)_inner.Read(), isStripped: false));
                return;
            }
        }

        var blankLines = new List<List<char>>();
        LinePrefix line;
        do
        {
            line = ReadLinePrefix();
            if (line.IsBlank)
            {
                blankLines.Add(line.Characters);
            }
        }
        while (line.IsBlank && !line.IsEof);

        if (line.IsEof)
        {
            ScopeEnd(blankLines, null);
            return;
        }

        if (line.IsListMarker && line.IndentColumns < _contentOffset && line.IndentColumns < 4)
        {
            ScopeEnd(blankLines, line.Characters);
            return;
        }

        if (line.IsListMarker && line.IndentColumns >= 4 && line.IndentColumns < _contentOffset)
        {
            foreach (char c in line.Characters)
            {
                _ready.Enqueue(new ReadyCharacter(c, isStripped: false));
            }
            _lineStart = false;
            return;
        }

        if (line.IsListMarker && line.IndentColumns < _contentOffset)
        {
            ScopeEnd(blankLines, line.Characters);
            return;
        }

        if (line.IsLineStartConstruct && line.IndentColumns < _contentOffset
            && IsThematicBreak(line.Characters, line.IndentColumns))
        {
            foreach (var blankLine in blankLines)
            {
                _handoff.AddRange(blankLine);
            }
            foreach (char c in line.Characters)
            {
                _sourceReplay.Enqueue(c);
            }
            _scopeEnded = true;
            return;
        }

        if (blankLines.Count > 0 && line.IndentColumns < _contentOffset && !line.IsLineStartConstruct)
        {
            ScopeEnd(blankLines, line.Characters);
            return;
        }

        foreach (var blankLine in blankLines)
        {
            QueueStrippedLine(blankLine);
        }

        QueueStrippedLine(line.Characters);
        _lineStart = false;
    }

    private LinePrefix ReadLinePrefix()
    {
        var characters = new List<char>();
        int columns = 0;

        while (true)
        {
            int next = _inner.Peek();
            if (next == ' ' || next == '\t')
            {
                char whitespace = (char)_inner.Read();
                characters.Add(whitespace);
                columns += whitespace == '\t' ? 4 - (columns % 4) : 1;
                continue;
            }

            if (next == '\r' || next == '\n' || next == -1)
            {
                if (next == '\r')
                {
                    characters.Add((char)_inner.Read());
                    if (_inner.Peek() == '\n')
                    {
                        characters.Add((char)_inner.Read());
                    }
                }
                else if (next == '\n')
                {
                    characters.Add((char)_inner.Read());
                }

                return new LinePrefix(characters, columns, isBlank: true, isListMarker: false, isLineStartConstruct: false, isEof: next == -1);
            }

            char first = (char)_inner.Read();
            characters.Add(first);
            if (first is '-' or '*')
            {
                int nextCharacter = _inner.Peek();
                if (nextCharacter == first)
                {
                    while (_inner.Peek() == first)
                    {
                        characters.Add((char)_inner.Read());
                    }
                    if (_inner.Peek() == '\r')
                    {
                        characters.Add((char)_inner.Read());
                        if (_inner.Peek() == '\n')
                            characters.Add((char)_inner.Read());
                    }
                    else if (_inner.Peek() == '\n')
                    {
                        characters.Add((char)_inner.Read());
                    }
                    return new LinePrefix(characters, columns, isBlank: false, isListMarker: false, isLineStartConstruct: true, isEof: false);
                }
            }
            bool isListMarker = CaptureListMarkerSuffix(characters, characters[characters.Count - 1]);
            bool isLineStartConstruct = characters[columns] is '#' or '>' or '`' or '~' or '|' or '-' or '*';
            return new LinePrefix(characters, columns, isBlank: false, isListMarker, isLineStartConstruct, isEof: false);
        }
    }

    private bool CaptureListMarkerSuffix(List<char> characters, char first)
    {
        if (first is '+' or '-' or '*')
        {
            int afterMarker = _inner.Peek();
            if (afterMarker == ' ')
            {
                characters.Add((char)_inner.Read());
                return true;
            }
            return afterMarker is '\r' or '\n' or -1;
        }

        if (!char.IsDigit(first))
        {
            return false;
        }

        int digits = 1;
        while (digits < 9 && char.IsDigit((char)_inner.Peek()))
        {
            characters.Add((char)_inner.Read());
            digits++;
        }

        int delimiter = _inner.Peek();
        if (delimiter is not ('.' or ')'))
        {
            return false;
        }

        characters.Add((char)_inner.Read());
        int afterDelimiter = _inner.Peek();
        if (afterDelimiter == ' ')
        {
            characters.Add((char)_inner.Read());
            return true;
        }

        return afterDelimiter is '\r' or '\n' or -1;
    }

    private static bool IsThematicBreak(List<char> characters, int start)
    {
        if (start >= characters.Count || characters[start] is not ('-' or '*'))
        {
            return false;
        }

        char marker = characters[start];
        int count = 0;
        for (int i = start; i < characters.Count; i++)
        {
            if (characters[i] != marker)
            {
                return false;
            }
            count++;
        }

        return count >= 3;
    }

    private void ScopeEnd(List<List<char>> blankLines, List<char>? siblingPrefix)
    {
        foreach (var blankLine in blankLines)
        {
            _handoff.AddRange(blankLine);
        }

        if (siblingPrefix != null)
        {
            _handoff.AddRange(siblingPrefix);
        }

        _scopeEnded = true;
    }

    private void QueueStrippedLine(List<char> characters)
    {
        int columns = 0;
        foreach (char c in characters)
        {
            if (c is not (' ' or '\t'))
            {
                _ready.Enqueue(new ReadyCharacter(c, isStripped: false));
                continue;
            }

            int nextColumn = columns + (c == '\t' ? 4 - (columns % 4) : 1);
            if (columns >= _contentOffset)
            {
                _ready.Enqueue(new ReadyCharacter(c, isStripped: false));
            }
            else
            {
                _ready.Enqueue(new ReadyCharacter(c, isStripped: true));
                if (nextColumn > _contentOffset)
                {
                    for (int remainder = _contentOffset; remainder < nextColumn; remainder++)
                    {
                        _ready.Enqueue(new ReadyCharacter(' ', isStripped: false));
                    }
                }
            }
            columns = nextColumn;
        }

    }

    private sealed class LinePrefix
    {
        internal List<char> Characters { get; }
        internal int IndentColumns { get; }
        internal bool IsBlank { get; }
        internal bool IsListMarker { get; }
        internal bool IsLineStartConstruct { get; }
        internal bool IsEof { get; }

        internal LinePrefix(List<char> characters, int indentColumns, bool isBlank, bool isListMarker, bool isLineStartConstruct, bool isEof)
        {
            Characters = characters;
            IndentColumns = indentColumns;
            IsBlank = isBlank;
            IsListMarker = isListMarker;
            IsLineStartConstruct = isLineStartConstruct;
            IsEof = isEof;
        }
    }

    private sealed class ReadyCharacter
    {
        internal char Value { get; }
        internal bool IsStripped { get; }

        internal ReadyCharacter(char value, bool isStripped)
        {
            Value = value;
            IsStripped = isStripped;
        }
    }
}
