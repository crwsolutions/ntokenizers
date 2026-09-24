using System.Text;
using NTokenizers.Core;

namespace NTokenizers.Markdown;

/// <summary>
/// Parses the plain-text content of an indented code block, line by line. The content is
/// emitted as Text tokens using the same word rule as the main MarkdownTokenizer loop (a
/// word is emitted at every space that follows more than one buffered character, and line
/// breaks stay literal). No inline syntax is parsed: the content is code, not markdown.
/// The block ends at a non-indented, non-blank line (which is left untouched in the stream
/// for the main tokenizer) or at end of stream. Only the start of the next line (indentation
/// plus first character) ever needs to be looked ahead, so the lookahead stays bounded.
/// </summary>
internal sealed class IndentedCodeBlockContentTokenizer : BaseMarkdownTokenizer
{
    internal static IndentedCodeBlockContentTokenizer Create() => new();

    // Line-start tracking for the word emission rule, per content line.
    private bool _atLineStart = true;

    // Blank lines inside the open indented code block that are not yet known to belong to
    // the block. Each buffered blank line contributes its (indentation-removed) content plus
    // "\n". They are flushed when a code content line follows, and dropped when the block
    // ends first.
    private readonly StringBuilder _pendingBlankContent = new();

    internal protected override Task ParseAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            // Classify the line at the current position without consuming it.
            LineStart kind = PeekCurrentLineKind(this);

            if (kind == LineStart.IndentedContent)
            {
                // Code content: flush the pending blank lines that precede it, then emit
                // this line (its first four columns removed). The line ending is included
                // only when the input has one, so the token stream stays faithful to the
                // input.
                (string line, bool hasLineEnding) = ReadLineText(this);
                EmitText();
                _pendingBlankContent.Append(RemoveLeadingIndent(line));
                if (hasLineEnding)
                {
                    _pendingBlankContent.Append('\n');
                }
                EmitContent(_pendingBlankContent.ToString());
                _pendingBlankContent.Clear();
                continue;
            }

            if (kind == LineStart.Blank)
            {
                // A blank line may be followed by code content: buffer it tentatively. Its
                // first four columns are removed too, so an indented blank line keeps any
                // whitespace beyond those columns. A blank line always ends the line in
                // the input, so its line ending is buffered as well.
                (string line, _) = ReadLineText(this);
                EmitText();
                _pendingBlankContent.Append(RemoveLeadingIndent(line)).Append('\n');
                continue;
            }

            // A non-indented, non-blank line ends the block. It is left untouched so the
            // main tokenizer processes it as a normal line; the pending blank lines before
            // it are dropped. End of stream also ends the block.
            break;
        }

        _pendingBlankContent.Clear(); // Trailing blank lines do not belong to the block.
        EmitText();

        return Task.CompletedTask;
    }

    /// <summary>
    /// Emits the given (indentation-removed) content line as Text tokens, using the same
    /// word rule as the main MarkdownTokenizer loop: a word is emitted at every space that
    /// follows more than one buffered character (the emitted token includes the triggering
    /// space). Line breaks stay literal in the content; leading indentation of a line does
    /// not emit by itself.
    /// </summary>
    private void EmitContent(string content)
    {
        _atLineStart = true; // start of a content line
        foreach (char c in content)
        {
            if (c != '\n' && _atLineStart && !char.IsWhiteSpace(c))
            {
                _atLineStart = false; // first content character of the line
            }

            _buffer.Append(c);

            if (c == '\n')
            {
                _atLineStart = true; // next line; line breaks stay literal
            }
            else if (!_atLineStart && c == ' ' && _buffer.Length > 1)
            {
                EmitText(); // word rule of the main loop, applied after the append
            }
        }
    }

    /// <summary>
    /// What kind of line starts at the current position of the given tokenizer.
    /// </summary>
    internal enum LineStart
    {
        // End of stream.
        End,

        // A blank line (no characters, or only spaces/tabs).
        Blank,

        // A line with at least four columns of indentation followed by non-blank content.
        IndentedContent,

        // Any other line (e.g. a paragraph line, a heading, or a construct with up to three
        // columns of indentation).
        Other
    }

    /// <summary>
    /// Classifies the line at the current position of the given tokenizer, which must be a
    /// line start. Only the leading spaces/tabs and the first content character are
    /// examined, so the lookahead stays bounded. Nothing is consumed; the position is left
    /// at the start of the line. Shared with the main tokenizer, which uses it for the
    /// block start detection before the content tokenizer is wired to the stream.
    /// </summary>
    internal static LineStart PeekCurrentLineKind(BaseTokenizer<MarkdownToken> tokenizer)
    {
        int columns = 0;
        int pos = 0;
        while (true)
        {
            char c = tokenizer.PeekAhead(pos);
            if (c != ' ' && c != '\t')
            {
                break;
            }
            // A tab counts as the number of spaces to the next 4-column tab stop.
            columns += c == '\t' ? 4 - (columns % 4) : 1;
            pos++;
        }

        char ch = tokenizer.PeekAhead(pos);
        if (ch == '\0')
        {
            return LineStart.End;
        }
        if (ch == ' ' || ch == '\t' || ch == '\r' || ch == '\n')
        {
            return LineStart.Blank;
        }
        return columns >= 4 ? LineStart.IndentedContent : LineStart.Other;
    }

    /// <summary>
    /// Reads one line from the stream of the given tokenizer: the line content (with any
    /// trailing \r removed; an empty string at end of stream) and whether a line ending was
    /// present. The line ending is consumed as well, leaving the position at the start of
    /// the next line. The caller decides whether to include the line ending in its output.
    /// </summary>
    internal static (string line, bool hasLineEnding) ReadLineText(BaseTokenizer<MarkdownToken> tokenizer)
    {
        var line = new StringBuilder();
        while (true)
        {
            int c = tokenizer.Peek();
            if (c == -1 || c == '\n')
            {
                break;
            }
            line.Append((char)tokenizer.Read());
        }
        if (line.Length > 0 && line[line.Length - 1] == '\r')
        {
            line.Length--;
        }
        bool hasLineEnding = tokenizer.Peek() == '\n';
        if (hasLineEnding)
        {
            tokenizer.Read();
        }
        return (line.Length > 0 ? line.ToString() : string.Empty, hasLineEnding);
    }

    /// <summary>
    /// Removes up to the first four columns of indentation (spaces or tabs) from the given
    /// line, preserving any additional indentation beyond the first four columns. A line with
    /// fewer than four columns of indentation is reduced to its remaining (non-whitespace)
    /// content, so an indented blank line never adds visible indentation to the code.
    /// </summary>
    internal static string RemoveLeadingIndent(string line)
    {
        int columns = 0;
        int i = 0;
        while (i < line.Length)
        {
            char ch = line[i];
            if (ch != ' ' && ch != '\t')
            {
                break;
            }
            // A tab counts as the number of spaces to the next 4-column tab stop.
            columns += ch == '\t' ? 4 - (columns % 4) : 1;
            i++;
            if (columns >= 4)
            {
                break;
            }
        }

        return i < line.Length ? line.Substring(i) : string.Empty;
    }
}
