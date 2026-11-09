using System.Text;

namespace Poc.Core;

/// <summary>
/// A small character-based code tokenizer (sub-tokenizer). It demonstrates
/// a real streaming state machine that runs inside
/// <see cref="InlineMetadata{TToken}.ParseInlinesAsync"/>: it reads
/// character by character, preserves whitespace (newlines included, per the
/// library design principle) and emits <see cref="CodeToken"/> through the
/// same awaited-handler contract.
/// <para>
/// Streaming rule: a branch that reads past its token boundary must push
/// the boundary character back (single-character pushback). Losing it
/// corrupts the stream (missing whitespace, swallowed punctuation).
/// </para>
/// </summary>
public sealed class CodeTokenizer : BaseTokenizer<CodeToken>
{
    private static readonly HashSet<string> Keywords = new(StringComparer.Ordinal)
    {
        "var", "let", "const", "function", "if", "else", "return",
        "for", "while", "int", "string", "void", "new", "class",
        "async", "await",
    };

    private char? _pushback;

    protected override async Task ParseCoreAsync(CancellationToken ct)
    {
        var pending = new StringBuilder();

        int c;
        while ((c = ReadChar()) != -1)
        {
            ct.ThrowIfCancellationRequested();
            var ch = (char)c;

            if (ch == '\n' || ch == '\r')
            {
                // Newlines are preserved as whitespace tokens.
                await EmitPunctuationAsync(pending);
                await EmitAsync(new CodeToken(CodeTokenType.Whitespace, ch.ToString()));
            }
            else if (char.IsWhiteSpace(ch))
            {
                await EmitPunctuationAsync(pending);
                await EmitAsync(new CodeToken(CodeTokenType.Whitespace, ch.ToString()));
            }
            else if (ch == '/')
            {
                int next = ReadChar();
                if (next == '/')
                {
                    // Line comment: runs to the end of the line (exclusive).
                    await EmitPunctuationAsync(pending);
                    var comment = new StringBuilder("//");
                    int cc;
                    while ((cc = ReadChar()) != -1 && cc != '\n' && cc != '\r')
                    {
                        comment.Append((char)cc);
                    }

                    PushbackIfAny(cc);
                    await EmitAsync(new CodeToken(CodeTokenType.Comment, comment.ToString()));
                }
                else
                {
                    // A lone '/': punctuation, the following char is re-read.
                    PushbackIfAny(next);
                    pending.Append(ch);
                }
            }
            else if (ch == '"' || ch == '\'')
            {
                await EmitPunctuationAsync(pending);
                var literal = new StringBuilder();
                literal.Append(ch);
                int sc;
                while ((sc = ReadChar()) != -1 && sc != ch && sc != '\n' && sc != '\r')
                {
                    literal.Append((char)sc);
                }

                if (sc == ch)
                {
                    literal.Append(ch);
                }
                else
                {
                    // Unterminated at end of line: the boundary char goes back.
                    PushbackIfAny(sc);
                }

                await EmitAsync(new CodeToken(CodeTokenType.StringValue, literal.ToString()));
            }
            else if (char.IsLetter(ch) || ch == '_')
            {
                var word = new StringBuilder();
                word.Append(ch);
                int wc;
                while ((wc = ReadChar()) != -1 && (char.IsLetterOrDigit((char)wc) || wc == '_'))
                {
                    word.Append((char)wc);
                }

                // The character that ended the word (space, operator, ...)
                // is not part of it: push it back.
                PushbackIfAny(wc);

                var text = word.ToString();
                await EmitAsync(new CodeToken(
                    Keywords.Contains(text) ? CodeTokenType.Keyword : CodeTokenType.Identifier,
                    text));
            }
            else if (char.IsDigit(ch))
            {
                var number = new StringBuilder();
                number.Append(ch);
                int nc;
                while ((nc = ReadChar()) != -1 && (char.IsDigit((char)nc) || nc == '.'))
                {
                    number.Append((char)nc);
                }

                PushbackIfAny(nc);
                await EmitAsync(new CodeToken(CodeTokenType.Number, number.ToString()));
            }
            else
            {
                // Operators and punctuation accumulate into one token.
                pending.Append(ch);
            }
        }

        await EmitPunctuationAsync(pending);
    }

    /// <summary>
    /// Reads the next character, preferring the pushed-back one. This is the
    /// only way characters enter the machine, so the pushback is never
    /// bypassed (e.g. by a Peek on the raw reader).
    /// </summary>
    private int ReadChar()
    {
        if (_pushback is { } pushed)
        {
            _pushback = null;
            return pushed;
        }

        return Reader.Read();
    }

    /// <summary>Restores a boundary character for the next <see cref="ReadChar"/>.</summary>
    private void Pushback(char character) => _pushback = character;

    private void PushbackIfAny(int c)
    {
        if (c != -1)
        {
            Pushback((char)c);
        }
    }

    private async Task EmitPunctuationAsync(StringBuilder pending)
    {
        if (pending.Length == 0)
        {
            return;
        }

        var text = pending.ToString();
        pending.Clear();
        await EmitAsync(new CodeToken(CodeTokenType.Punctuation, text));
    }
}
