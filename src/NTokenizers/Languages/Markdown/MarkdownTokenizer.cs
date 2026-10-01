using System.Diagnostics;
using System.Text;
using NTokenizers.C;
using NTokenizers.Core;
using NTokenizers.Cpp;
using NTokenizers.CSharp;
using NTokenizers.Css;
using NTokenizers.Generic;
using NTokenizers.Go;
using NTokenizers.Html;
using NTokenizers.Java;
using NTokenizers.Json;
using NTokenizers.Kotlin;
using NTokenizers.Markdown.Metadata;
using NTokenizers.Python;
using NTokenizers.Rust;
using NTokenizers.Sql;
using NTokenizers.Swift;
using NTokenizers.Toml;
using NTokenizers.Typescript;
using NTokenizers.Xml;
using NTokenizers.Yaml;

namespace NTokenizers.Markdown;

/// <summary>
/// A streaming tokenizer for Markdown/markdown constructs using a character-by-character state machine.
/// </summary>
/// <remarks>
/// The public <see cref="Create"/> factory returns the top-level tokenizer (depth 0). The
/// content of a blockquote is streamed by a nested instance at one deeper level (see the
/// internal constructor): at each line start the blockquote decision table runs first,
/// assigning the line's blockquote markers by depth; the remaining content is parsed as a
/// full markdown sub-document (paragraphs, headings, lists, code blocks, tables, and
/// nested blockquotes).
/// </remarks>
public sealed class MarkdownTokenizer : BaseMarkdownTokenizer
{
    /// <summary>
    /// Create a new top-level instance of MarkdownTokenizer.
    /// </summary>
    /// <returns></returns>
    public static MarkdownTokenizer Create() => new();

    /// <summary>
    /// Initializes a new top-level MarkdownTokenizer instance.
    /// </summary>
    public MarkdownTokenizer() : this(depth: 0)
    {
    }

    /// <summary>
    /// Initializes a MarkdownTokenizer instance. A depth of 0 is the top-level document; a
    /// depth of N means this tokenizer runs as the content of the N-th nested blockquote.
    /// Each line start is first offered to the blockquote decision table, which assigns the
    /// line's blockquote markers by depth (marker K of a line belongs to nesting level K);
    /// the remaining content is parsed as a full markdown sub-document.
    /// </summary>
    /// <param name="depth">The blockquote nesting depth (0 for the top-level document).</param>
    internal MarkdownTokenizer(int depth)
    {
        _depth = depth;
    }

    // The blockquote nesting depth: 0 for the top-level document, N for the content of the
    // N-th nested blockquote. Marker K of an input line belongs to nesting level K.
    private readonly int _depth;

    // One marker chain is stripped per input line, at each nesting level. It starts true so
    // the trigger line's remainder goes straight to the normal line-start grammar, and it is
    // reset to false every time this level processes a '\n' (or a line-start construct left
    // it at a line start), so the next line's start is re-offered to the decision table.
    private bool _prefixConsumedThisLine = true;

    private bool _atLineStart = true;
    private BlockContext _block = BlockContext.None;
    private bool _pendingSoftBreak;

    // The open block context. None and Paragraph are mutually exclusive, so a single enum
    // tracks them instead of several booleans. An indented code block is not tracked here:
    // its content is consumed by the IndentedCodeBlockContentTokenizer.
    private enum BlockContext
    {
        None,
        Paragraph
    }

    /// <summary>
    /// Parses the input stream and emits markdown tokens via the OnToken callback.
    /// </summary>
    internal protected override async Task ParseAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            int peek = Peek();
            if (peek == -1) break;

            char c = (char)peek;

            // Try to parse special constructs at line start (headings, lists, fences, etc.).
            // An indented code line (>= 4 spaces) is checked first because it begins with
            // whitespace, and it cannot interrupt a paragraph. Line-start constructs (which
            // allow up to 3 spaces of indentation) are checked afterwards.
            if (_atLineStart)
            {
                // Blockquote content: the line is first offered to the blockquote decision
                // table, which strips this level's marker prefix or ends the quote (line left
                // untouched for the outer tokenizer).
                if (_depth > 0 && !_prefixConsumedThisLine)
                {
                    if (!await TryHandleBlockquoteLineStartAsync())
                    {
                        break; // The quote is ended; the outer tokenizer takes over this line.
                    }

                    // The decision table consumed this line's prefix, so the character captured
                    // at the top of the loop is stale; re-sync it with the stream.
                    peek = Peek();
                    if (peek == -1)
                    {
                        break;
                    }

                    c = (char)peek;
                }

                // Indented (>= 4 spaces) code line at the top level. It cannot interrupt a
                // paragraph, so it is only started when no paragraph is open. The content is
                // streamed through the inline handler pipe by the content tokenizer.
                if (_block != BlockContext.Paragraph && await TryStartIndentedCodeBlockAsync())
                {
                    continue;
                }

                // A line-start construct allows at most three columns of indentation. When
                // the first non-whitespace character is about to be processed the leading
                // whitespace is still buffered, so the indent can be measured here. A line
                // indented by four or more columns is not a line-start construct: with a
                // paragraph open it is a lazy continuation line (handled below) and with no
                // paragraph open it simply cannot start a construct.
                if (!char.IsWhiteSpace(c) && CountLeadingIndentColumns() < 4 && await TryParseLineStartConstructAsync())
                {
                    // A line-start block construct (list, fence, ...) terminates an open
                    // paragraph. Close it after the construct; for headings the paragraph is
                    // already closed before the heading token is emitted (see
                    // TryParseHeadingAsync), so this is a no-op in that case.
                    CloseParagraph();
                    _pendingSoftBreak = false;

                    //Eat newline after line-start construct
                    bool ateNewline = false;
                    if (Peek() == '\r')
                    {
                        Read();
                    }
                    if (Peek() == '\n')
                    {
                        Read();
                        ateNewline = true;
                    }

                    _atLineStart = true;
                    // After a line-start construct the stream sits at a line start. In blockquote
                    // mode the construct may have consumed the line ending itself, in which case
                    // this level never processed a '\n'; reset the per-line prefix flag
                    // unconditionally so the next line is re-offered to the decision table.
                    if (_depth > 0 || ateNewline)
                    {
                        _prefixConsumedThisLine = false;
                    }

                    continue;
                }
            }

            // First non-whitespace character of a non-block line: opens a paragraph, or
            // continues an open one (flushing a held soft line break).
            if (_atLineStart && !char.IsWhiteSpace(c))
            {
                _atLineStart = false;

                if (_block == BlockContext.Paragraph)
                {
                    if (_pendingSoftBreak)
                    {
                        _pendingSoftBreak = false;
                        _onToken(new MarkdownToken(MarkdownTokenType.Text, "\n"));
                    }
                }
                else
                {
                    _pendingSoftBreak = false;
                    _block = BlockContext.Paragraph;
                    _onToken(new MarkdownToken(MarkdownTokenType.ParagraphBlockStart, string.Empty));
                }
            }

            // Try inline constructs
            if (TryParseInlineConstruct(c))
            {
                continue;
            }

            // Regular character - add to buffer
            Read();
            _buffer.Append(c);

            if (c == '\n')
            {
                if (_block == BlockContext.Paragraph)
                {
                    // Strip the trailing line break (and any preceding \r) from the line content.
                    _buffer.Length--;
                    if (_buffer.Length > 0 && _buffer[_buffer.Length - 1] == '\r')
                    {
                        _buffer.Length--;
                    }

                    var lineIsBlank = _atLineStart && AllWhiteSpace(_buffer.ToString());
                    if (lineIsBlank)
                    {
                        // A blank line ends the paragraph; its own content is dropped.
                        _pendingSoftBreak = false;
                        CloseParagraph();
                        _buffer.Clear();
                    }
                    else
                    {
                        // A non-blank line within the paragraph; its break is a pending soft break.
                        _pendingSoftBreak = true;
                        EmitText();
                    }
                }
                else
                {
                    // Not in a paragraph: keep the line break as plain text (baseline behavior).
                    _pendingSoftBreak = false;
                    EmitText();
                }
                _atLineStart = true;
                _prefixConsumedThisLine = false;
            }

            //Emit words to keep the streaming character of the tokenizer.
            if (!_atLineStart && c == ' ' && _buffer.Length > 1)
            {
                EmitText();
            }
        }

        // End of stream: emit any remaining text; a paragraph still open at EOF ends with
        // the last line.
        EmitText();
        CloseParagraph();
    }

    /// <summary>
    /// Ends the open paragraph (if any) by emitting a ParagraphBlockEnd token.
    /// Called when a blank line, a line-start construct, or end of stream terminates the paragraph.
    /// </summary>
    private void CloseParagraph()
    {
        if (_block == BlockContext.Paragraph)
        {
            _block = BlockContext.None;
            _onToken(new MarkdownToken(MarkdownTokenType.ParagraphBlockEnd, string.Empty));
        }
    }

    /// <summary>
    /// Attempts to start an indented code block on the current (line-start) position when the
    /// line begins with four or more columns of indentation (spaces or tabs) followed by
    /// non-blank content. Returns true when a block was started and the block content was
    /// streamed through the inline handler pipe, so the loop can continue with the line after
    /// the block. A block is never started while a paragraph is open (an indented code block
    /// cannot interrupt a paragraph) and a blank line does not open a block on its own. The
    /// line itself is left in the stream: the content tokenizer reads it as the first block
    /// line.
    /// </summary>
    private async Task<bool> TryStartIndentedCodeBlockAsync()
    {
        // Classify the line at the current position without consuming it; only an indented
        // content line starts a block.
        if (IndentedCodeBlockContentTokenizer.PeekCurrentLineKind(this) != IndentedCodeBlockContentTokenizer.LineStart.IndentedContent)
        {
            return false;
        }

        EmitText();
        _pendingSoftBreak = false;

        // Open the block: the token is emitted with its metadata and the block content is
        // streamed as plain Text tokens through the metadata's inline token handler.
        return await ParseInlines(MarkdownTokenType.IndentedCodeBlock, new IndentedCodeBlockMetadata(), IndentedCodeBlockContentTokenizer.Create());
    }

    /// <summary>
    /// The blockquote decision table, evaluated at the start of each input line while this
    /// tokenizer runs as blockquote content (see the internal constructor). The line is
    /// examined without consuming it until a decision is made:
    /// <list type="number">
    /// <item>Markers are assigned by depth: marker K belongs to nesting level K, so this
    /// level (depth <c>_depth</c>) continues only when the line carries at least <c>_depth</c>
    /// markers (each: up to three columns of spaces or tabs, then <c>&gt;</c> plus an optional
    /// single space). This level's markers are consumed; any excess stays in the remainder
    /// and opens deeper nested quotes. With fewer markers: a valid lazy continuation (a
    /// paragraph is open and the remainder is plain content) consumes them as decoration;
    /// otherwise this level ends and the line is left for the outer tokenizer.</item>
    /// <item>A blank line (no prefix) closes the paragraph, emits the whitespace plus line
    /// ending as a Text token and ends the quote.</item>
    /// <item>An indented line (four or more columns, no prefix) ends the quote when no
    /// paragraph is open, so the outer tokenizer can start an indented code block.</item>
    /// <item>A line starting a line-start construct ends the quote, leaving the line untouched.</item>
    /// <item>Any other line is a lazy continuation while a paragraph is open and ends the
    /// quote otherwise.</item>
    /// </list>
    /// </summary>
    /// <returns>true when the prefix was stripped and the main loop should continue; false when
    /// the quote is ended and the line is left untouched in the stream for the outer tokenizer.</returns>
    private async Task<bool> TryHandleBlockquoteLineStartAsync()
    {
        // Row 1: walk the marker chain without consuming it. 'stop' marks the end of this
        // level's _depth-th marker so a line carrying more markers leaves the excess in
        // the remainder to open deeper nested quotes.
        int markers = 0;
        int end = 0;
        int stop = 0;
        while (true)
        {
            int col = 0;
            int pos = end;
            while ((PeekAhead(pos) == ' ' || PeekAhead(pos) == '\t') && col < 4)
            {
                col += PeekAhead(pos) == '\t' ? 4 - (col % 4) : 1;
                pos++;
            }
            if (col >= 4 || PeekAhead(pos) != '>')
            {
                break;
            }
            pos++; // the '>'
            if (PeekAhead(pos) == ' ')
            {
                pos++; // the optional single space is part of the marker
            }
            markers++;
            end = pos;
            if (markers == _depth)
            {
                stop = end;
                break;
            }
        }

        if (markers >= _depth)
        {
            // The line belongs to this level: strip only its _depth markers; any excess
            // stays in the remainder to open deeper nested quotes.
            EmitText();
            for (int i = 0; i < stop; i++)
            {
                Read();
            }
            _prefixConsumedThisLine = true;
            return true;
        }

        // Markers < depth: the markers belong to the outer levels. A valid lazy
        // continuation consumes them as decoration; otherwise this level ends and the
        // line is left untouched for the outer tokenizer. This must not fall through to
        // rows 2-5: a blank remainder or line-start construct is not a lazy continuation,
        // so row 5 would wrongly keep the paragraph open.
        if (markers > 0)
        {
            bool lazy = _block == BlockContext.Paragraph
                && IndentedCodeBlockContentTokenizer.PeekCurrentLineKind(this, end) == IndentedCodeBlockContentTokenizer.LineStart.Other
                && !WouldStartBlockquoteBreakingConstruct(end);
            if (lazy)
            {
                EmitText();
                for (int i = 0; i < end; i++)
                {
                    Read();
                }
                _prefixConsumedThisLine = true;
                return true;
            }

            CloseParagraph();
            return false;
        }

        // Row 2: a blank line (no prefix) ends the quote. The paragraph is closed first and
        // the line is emitted as a Text token so the stream stays faithful to the input.
        if (IndentedCodeBlockContentTokenizer.PeekCurrentLineKind(this) == IndentedCodeBlockContentTokenizer.LineStart.Blank)
        {
            _pendingSoftBreak = false;
            CloseParagraph();

            (string line, bool hasLineEnding) = IndentedCodeBlockContentTokenizer.ReadLineText(this);
            EmitText();
            _onToken(new MarkdownToken(MarkdownTokenType.Text, line + (hasLineEnding ? "\n" : string.Empty)));
            return false;
        }

        // Row 3: an indented (four or more columns) content line without a prefix ends the
        // quote when no paragraph is open; the outer tokenizer starts an indented code block.
        // With a paragraph open it is a lazy continuation line (fall through to the regular
        // paragraph logic below).
        if (IndentedCodeBlockContentTokenizer.PeekCurrentLineKind(this) == IndentedCodeBlockContentTokenizer.LineStart.IndentedContent
            && _block != BlockContext.Paragraph)
        {
            return false;
        }

        // Row 4: a line that would start a line-start construct (same <4-column checks as the
        // top-level grammar) ends the quote; the line stays untouched for the outer scope.
        if (WouldStartBlockquoteBreakingConstruct())
        {
            return false;
        }

        // Row 5: any other line. A lazy continuation while a paragraph is open; otherwise the
        // quote ends and the line belongs to the outer scope. When the line is a lazy
        // continuation the decision is marked made so the table is not re-evaluated further
        // down the line (after its leading whitespace is buffered), which would otherwise
        // end the quote the moment the buffered content begins with a line-start construct.
        if (_block == BlockContext.Paragraph)
        {
            _prefixConsumedThisLine = true;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Non-consumingly reports whether the line at the current (line-start) position,
    /// optionally starting <paramref name="offset"/> characters ahead, would begin a
    /// line-start construct: an ATX heading, a thematic break, a list marker, a code
    /// fence, a ::: custom container, or a table. The same character conditions as the
    /// consumer methods in <see cref="TryParseLineStartConstructAsync"/> are replicated here
    /// without reading from the stream, so a "quote ends" decision never disturbs the line.
    /// A leading blockquote marker is not part of this check: a line starting with <c>&gt;</c>
    /// is handled by the marker row of the decision table.
    /// </summary>
    private bool WouldStartBlockquoteBreakingConstruct(int offset = 0)
    {
        char first = PeekAhead(offset);
        if (first == ' ' || first == '\t')
        {
            return false;
        }

        if (first == '#')
        {
            // 1-6 '#' followed by space, tab, line ending, or end of stream.
            int level = 0;
            while (PeekAhead(level + offset) == '#' && level < 6)
            {
                level++;
            }

            char next = PeekAhead(level + offset);
            return next == ' ' || next == '\t' || next == '\n' || next == '\0';
        }

        if (first == '-' || first == '*')
        {
            int count = 0;
            while (PeekAhead(count + offset) == first)
            {
                count++;
            }

            // A thematic break: at least three of the same character, then line ending or EOF.
            if (count >= 3 && (PeekAhead(count + offset) == '\r' || PeekAhead(count + offset) == '\n' || PeekAhead(count + offset) == '\0'))
            {
                return true;
            }

            // An unordered list marker: the character followed by a space.
            return PeekAhead(1 + offset) == ' ';
        }

        if (char.IsDigit(first))
        {
            // An ordered list marker: digits, a dot, then a space.
            int pos = 0;
            while (char.IsDigit(PeekAhead(pos + offset)))
            {
                pos++;
            }

            return PeekAhead(pos + offset) == '.' && PeekAhead(pos + 1 + offset) == ' ';
        }

        if (first == '`')
        {
            return PeekAhead(1 + offset) == '`' && PeekAhead(2 + offset) == '`';
        }

        if (first == ':')
        {
            return PeekAhead(1 + offset) == ':' && PeekAhead(2 + offset) == ':';
        }

        return first == '|';
    }

    /// <summary>
    /// Counts the leading indentation of the buffered leading whitespace in columns
    /// (a tab counts as the number of spaces to the next 4-column tab stop), stopping at
    /// the first non-whitespace character. The buffer holds exactly the leading whitespace
    /// of a line at the moment the first content character is processed.
    /// </summary>
    private int CountLeadingIndentColumns()
    {
        int columns = 0;
        for (int i = 0; i < _buffer.Length; i++)
        {
            char ch = _buffer[i];
            if (ch != ' ' && ch != '\t')
            {
                break;
            }
            columns += ch == '\t' ? 4 - (columns % 4) : 1;
        }
        return columns;
    }

    private static bool AllWhiteSpace(string s)
    {
        foreach (var ch in s)
        {
            if (!char.IsWhiteSpace(ch))
            {
                return false;
            }
        }
        return true;
    }

    private async Task<bool> TryParseLineStartConstructAsync()
    {
        if (await TryParseHeadingAsync()) return true;
        if (TryParseHorizontalRule()) return true;
        if (await TryParseBlockquoteAsync()) return true;
        if (await TryParseListItemAsync()) return true;
        if (await TryParseCodeFence()) return true;
        if (TryParseCustomContainer()) return true;
        if (await TryParseTableAsync()) return true;
        return false;
    }

    private async Task<bool> TryParseHeadingAsync()
    {
        int level = 0;
        int pos = 0;

        // Count # characters
        while (PeekAhead(pos) == '#' && level < 6)
        {
            level++;
            pos++;
        }

        if (level == 0) return false;

        // Must be followed by space or newline
        char next = PeekAhead(pos);
        if (next != ' ' && next != '\t' && next != '\n' && next != '\0')
            return false;

        // An ATX heading interrupts a paragraph. Close any open paragraph before the
        // heading is emitted, so that the paragraph's end token precedes the heading's
        // tokens in the stream rather than wrapping it.
        CloseParagraph();
        _pendingSoftBreak = false;

        // Emit any pending text
        EmitText();

        // Consume the # characters
        for (int i = 0; i < level; i++)
            Read();

        // Skip whitespace after #
        while (char.IsWhiteSpace((char)Peek()) && Peek() != '\n')
            Read();

        // Headings parse their content with heading-specific rules (the optional
        // closing sequence of '#' characters is stripped), so the dedicated heading
        // inline tokenizer is used instead of the generic inline tokenizer.
        return await ParseInlines(MarkdownTokenType.Heading, new HeadingMetadata(level), HeadingMarkdownTokenizer.Create());
    }

    private async Task<bool> ParseInlines<TToken>(MarkdownTokenType tokenType, InlineMetadata<TToken> metadata, Func<Action<TToken>, Task> parseAsync, string? value = null) where TToken : IToken
    {
        // Emit token with value (client can set OnInlineToken to parse inline content)
        var emitTask = Task.Run(() =>
            _onToken(new MarkdownToken(tokenType, value ?? string.Empty, metadata)));

        // Await the client registering the handler
        var handlerTask = metadata.GetInlineTokenHandlerAsync();

        // Ensure the heading token emission is complete
        await emitTask;

        using var cts = new CancellationTokenSource();
        var delayTask = Task.Delay(2000, cts.Token);

        var completedTask = await Task.WhenAny(handlerTask, delayTask);

        if (completedTask == handlerTask)
        {
            cts.Cancel();
            Debug.WriteLine("Canceling wait token");
        }

        if (completedTask != handlerTask || handlerTask.Result == null)
        {
            return false;
        }

        // Run the tokenizer
        await parseAsync(handlerTask.Result);

        metadata.CompleteProcessing();
        return true;
    }

    private Task<bool> ParseInlines<TToken>(MarkdownTokenType tokenType, InlineMetadata<TToken> metadata, BaseTokenizer<TToken> tokenizer, string? value = null) where TToken : IToken =>
        ParseInlines(tokenType, metadata, handler => tokenizer.ParseAsync(Reader, Bob, _lookaheadBuffer, handler), value);

    private Task<bool> ParseCodeInlines<TToken>(CodeBlockMetadata<TToken> metadata) where TToken : IToken
    {
        // Fence content is read straight from the stream, bypassing the blockquote decision
        // table. Inside a quote that would leak the '>' markers into the code (and a
        // prefixed closing fence would go unrecognized), so the reader is wrapped to strip
        // this level's markers while keeping them in the faithful text.
        TextReader reader = _depth > 0
            ? new FilteredBlockquoteReader(Reader, _depth, Bob)
            : Reader;

        return ParseInlines(MarkdownTokenType.CodeBlock, metadata, handler => metadata.CreateTokenizer().ParseAsync(reader, Bob, "```", handler));
    }

    private bool TryParseHorizontalRule()
    {
        char c = PeekAhead(0);
        if (c != '-' && c != '*') return false;

        // Check for at least 3 of the same character
        int count = 0;
        int pos = 0;
        while (PeekAhead(pos) == c)
        {
            count++;
            pos++;
        }

        if (count < 3) return false;

        // Must be followed by newline or end of stream
        char next = PeekAhead(pos);
        if (next != '\r' && next != '\n' && next != '\0')
            return false;

        EmitText();

        // Consume the characters
        for (int i = 0; i < count; i++)
            Read();

        _onToken(new MarkdownToken(MarkdownTokenType.HorizontalRule, new string(c, count)));

        return true;
    }

    /// <summary>
    /// Attempts to parse a blockquote at the current (line-start) position. Consumes the
    /// blockquote marker ('>' + optional single space) and streams the blockquote content
    /// through the inline handler pipe.
    /// </summary>
    /// <remarks>
    /// The content is parsed by a nested <see cref="MarkdownTokenizer"/> at one deeper
    /// level, so the content is a full markdown sub-document: the content tokenizer reads
    /// until the grammar no longer fits a quoted line, then the call stack unwinds and the
    /// outer tokenizer's main loop takes the initiative back. Nesting is recursive through
    /// the same metadata pipe; a nested Blockquote token carries its own BlockquoteMetadata.
    /// <para>Documented simplifications (deviations from CommonMark):</para>
    /// <list type="alpha">
    /// <item>Setext headings are not supported (existing limitation).</item>
    /// <item>An indented line (>= 4 spaces) without a prefix and without an open
    /// sub-paragraph ends the quote; the outer scope turns it into an indented code block.</item>
    /// <item>A blank line without a prefix ends the quote; a blank quote line (prefix +
    /// whitespace only) is paragraph separation and keeps the quote open.</item>
    /// </list>
    /// </remarks>
    private async Task<bool> TryParseBlockquoteAsync()
    {
        if (Peek() != '>') return false;

        // A blockquote interrupts a paragraph ("foo\n> bar" is a closed paragraph followed
        // by a blockquote), so the paragraph's end token precedes the blockquote in the
        // stream - the same rule as for ATX headings.
        CloseParagraph();
        _pendingSoftBreak = false;

        EmitText();
        Read(); // Consume >

        // Skip whitespace after >
        if (Peek() == ' ')
            Read();

        // The content is a full markdown sub-document parsed by a nested tokenizer at one
        // deeper level, so the quote it opens is level _depth + 1.
        await ParseInlines(MarkdownTokenType.Blockquote, new BlockquoteMetadata(), new MarkdownTokenizer(_depth + 1));

        return true;
    }

    private async Task<bool> TryParseListItemAsync()
    {
        char c = PeekAhead(0);

        // Unordered list
        if (c == '+' || c == '-' || c == '*')
        {
            // Check if followed by space
            if (PeekAhead(1) != ' ')
                return false;

            var marker = c;

            // Extract indentation from buffer (leading whitespace before marker)
            var indentation = _buffer.ToString();
            _buffer.Clear();

            Read(); // Consume marker
            Read(); // Consume space

            await ParseInlines(MarkdownTokenType.UnorderedListItem, new ListItemMetadata(marker), InlineMarkdownTokenizer.Create(), indentation);

            return true;
        }

        // Ordered list
        if (char.IsDigit(c))
        {
            int pos = 0;
            int number = 0;
            while (char.IsDigit(PeekAhead(pos)))
            {
                number = number * 10 + (PeekAhead(pos) - '0');
                pos++;
            }

            if (PeekAhead(pos) == '.' && PeekAhead(pos + 1) == ' ')
            {
                // Extract indentation from buffer (leading whitespace before marker)
                var indentation = _buffer.ToString();
                _buffer.Clear();

                // Consume number and dot
                for (int i = 0; i <= pos; i++)
                    Read();
                Read(); // Consume space

                await ParseInlines(MarkdownTokenType.OrderedListItem, new OrderedListItemMetadata(number), InlineMarkdownTokenizer.Create(), indentation);

                return true;
            }
        }

        return false;
    }

    private async Task<bool> TryParseCodeFence()
    {
        if (PeekAhead(0) != '`' || PeekAhead(1) != '`' || PeekAhead(2) != '`')
            return false;

        EmitText();

        // Consume ```
        Read();
        Read();
        Read();

        // Read language identifier
        var lang = new StringBuilder();
        while (Peek() != -1 && Peek() != '\r' && Peek() != '\n')
        {
            lang.Append((char)Read());
        }

        if (Peek() == '\r')
            Read();

        if (Peek() == '\n')
            Read();

        // Create appropriate metadata based on language
        return await ParseCodeInlines(lang.ToString());
    }

    private async Task<bool> ParseCodeInlines(string language) => language.Trim().ToLowerInvariant() switch
    {
        "csharp" or "cs" or "c#" => await ParseCodeInlines(new CSharpCodeBlockMetadata(language)),
        "json" => await ParseCodeInlines(new JsonCodeBlockMetadata(language)),
        "xml" or "xaml" or "svg" => await ParseCodeInlines(new XmlCodeBlockMetadata(language)),
        "html" => await ParseCodeInlines(new HtmlCodeBlockMetadata(language)),
        "yaml" => await ParseCodeInlines(new YamlCodeBlockMetadata(language)),
        "sql" => await ParseCodeInlines(new SqlCodeBlockMetadata(language)),
        "typescript" or "ts" or "javascript" or "js" => await ParseCodeInlines(new TypeScriptCodeBlockMetadata(language)),
        "css" => await ParseCodeInlines(new CssCodeBlockMetadata(language)),
        "toml" => await ParseCodeInlines(new TomlCodeBlockMetadata(language)),
        "java" => await ParseCodeInlines(new JavaCodeBlockMetadata(language)),
        "c" => await ParseCodeInlines(new CCodeBlockMetadata(language)),
        "cpp" or "c++" => await ParseCodeInlines(new CppCodeBlockMetadata(language)),
        "rust" or "rs" => await ParseCodeInlines(new RustCodeBlockMetadata(language)),
        "kotlin" or "kt" => await ParseCodeInlines(new KotlinCodeBlockMetadata(language)),
        "go" or "golang" => await ParseCodeInlines(new GoCodeBlockMetadata(language)),
        "swift" => await ParseCodeInlines(new SwiftCodeBlockMetadata(language)),
        "python" or "py" => await ParseCodeInlines(new PythonCodeBlockMetadata(language)),
        _ => await ParseCodeInlines(new GenericCodeBlockMetadata(language))
    };

    private bool TryParseCustomContainer()
    {
        if (PeekAhead(0) != ':' || PeekAhead(1) != ':' || PeekAhead(2) != ':')
            return false;

        EmitText();

        // Consume :::
        Read();
        Read();
        Read();

        // Skip whitespace
        while (Peek() == ' ')
            Read();

        // Read container type
        var containerType = new StringBuilder();
        while (Peek() != -1 && Peek() != '\n')
        {
            containerType.Append((char)Read());
        }

        _onToken(new MarkdownToken(MarkdownTokenType.CustomContainer, containerType.ToString().Trim()));

        return true;
    }

    private async Task<bool> TryParseTableAsync()
    {
        if (Peek() != '|') return false;

        EmitText();
        Read(); // Consume |

        if (Peek() == '|') //this is invalid
        {
            _buffer.Append('|'); //put that pipe back
            return false;
        }

        var metadata = new TableMetadata();
        await ParseInlines(MarkdownTokenType.Table, metadata, new TableMarkdownTokenizer(metadata));

        if (Peek() == '|')
        {
            Read();
        }

        return true;
    }
}
