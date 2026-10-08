namespace NTokenizers.Markdown;

/// <summary>
/// Parses the inline content of an ATX heading line. Like the inline tokenizer it stops
/// at the end of the line, but it additionally strips the optional closing sequence of
/// '#' characters. Per the CommonMark spec a closing sequence is a run of unescaped '#'
/// characters preceded by a space or tab (or starting the content) and followed only by
/// spaces or tabs before the end of the line. The closing sequence and the spaces or
/// tabs around it are syntax, not content, so they are removed from the heading. Any
/// other '#' characters are heading content. The line ending itself is left for the
/// caller to consume.
/// </summary>
internal sealed class HeadingMarkdownTokenizer : BaseMarkdownTokenizer
{
    private char _lastReadChar = '\0';
    private bool _hasReadChar;

    internal static HeadingMarkdownTokenizer Create() => new();

    internal protected override Task ParseAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            var ch = Peek();
            if (ch == -1 || ch == '\n' || ch == '\r')
            {
                break;
            }

            if (TryParseInlineConstruct((char)ch))
            {
                continue;
            }

            // A run of '#' characters is a candidate closing sequence when it starts the
            // content or is preceded by a space or tab in the stream (the last character
            // consumed, tracked in Read; inline constructs may have flushed the buffer).
            if ((char)ch == '#' && (!_hasReadChar || _lastReadChar == ' ' || _lastReadChar == '\t'))
            {
                // Count the run of '#' characters starting at the current position.
                int run = 0;
                while (PeekAhead(run) == '#')
                {
                    run++;
                }

                // The run is the closing sequence only when it is followed by nothing but
                // spaces or tabs before the end of the line (or end of stream). The
                // heading is a single line, so this lookahead is bounded.
                int end = run;
                bool closing = true;
                while (true)
                {
                    char after = PeekAhead(end);
                    if (after == '\0' || after == '\r' || after == '\n')
                    {
                        break;
                    }
                    if (after != ' ' && after != '\t')
                    {
                        closing = false;
                        break;
                    }
                    end++;
                }

                if (closing)
                {
                    // The closing sequence and the spaces/tabs around it are syntax, not
                    // content: consume the run of '#' characters and the spaces/tabs
                    // after it, and strip the trailing spaces/tabs of the content that
                    // precede the run. The line ending stays in the stream for the
                    // caller.
                    while (_buffer.Length > 0 &&
                        (_buffer[_buffer.Length - 1] == ' ' || _buffer[_buffer.Length - 1] == '\t'))
                    {
                        _buffer.Length--;
                    }
                    for (int i = 0; i < run; i++)
                    {
                        Read();
                    }
                    while (Peek() == ' ' || Peek() == '\t')
                    {
                        Read();
                    }
                    EmitText();
                    return Task.CompletedTask;
                }
            }

            // Regular character
            _buffer.Append((char)Read());
        }

        EmitText();

        return Task.CompletedTask;
    }

    /// <summary>
    /// Tracks the last character consumed from the stream, so the closing-sequence check
    /// knows what precedes a '#' run even after inline constructs flushed the buffer.
    /// </summary>
    internal override int Read()
    {
        int c = base.Read();
        if (c != -1)
        {
            _lastReadChar = (char)c;
            _hasReadChar = true;
        }
        return c;
    }
}
