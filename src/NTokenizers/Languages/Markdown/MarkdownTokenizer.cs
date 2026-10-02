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
    private bool _listItemOwnsLineBreak;

    // The open list, if any. The line-start check keeps appending compatible items and
    // closes the list at the first line that does not belong to it.
    private ListState _listState = ListState.None;

    // The ordered list delimiter ('.' or ')') of the open list.
    private char _listDelimiter;

    private enum ListState
    {
        None,
        Unordered,
        Ordered
    }

    // The open block context. None and Paragraph are mutually exclusive, so a single enum
    // tracks them instead of several booleans. An indented code block is not tracked here:
    // its content is consumed by the IndentedCodeBlockContentTokenizer.
    private enum BlockContext
    {
        None,
        Paragraph
    }

    private enum ListKind
    {
        None,
        Unordered,
        Ordered
    }

    /// <summary>
    /// The result of classifying a line start as a list marker (see
    /// <see cref="PeekListItem"/>).
    /// </summary>
    private sealed class ListClassification
    {
        public ListKind Kind { get; }
        public char Marker { get; }
        public int Number { get; }
        public int Digits { get; }
        public int MarkerStart { get; }
        public int MarkerColumn { get; }

        public ListClassification(ListKind kind, char marker = '\0', int number = 0, int digits = 0, int markerStart = 0, int markerColumn = 0)
        {
            Kind = kind;
            Marker = marker;
            Number = number;
            Digits = digits;
            MarkerStart = markerStart;
            MarkerColumn = markerColumn;
        }

        public static ListClassification None => new(ListKind.None);
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

                // Sibling item of an open list: a list marker of the same kind (and, for
                // ordered lists, the same delimiter). The leading whitespace of the line (if
                // any) is already buffered; the stream sits at the first content character.
                // Any other non-whitespace line ends the open list first. A blank line does
                // not close the list — the whitespace is emitted as a Text token on the
                // parent level and the list stays open.
                if (_listState != ListState.None && !char.IsWhiteSpace(c))
                {
                    // A thematic break is a block construct, not a list item: it must not be
                    // claimed by the sibling item logic (which would read its first two
                    // characters as a nested marker). A break never belongs to the open list,
                    // so close the list first; the line-start chain then parses the break.
                    // A line indented by four or more columns is not a break (it is indented
                    // code or a lazy continuation), so the break check respects the same
                    // three-column limit as the line-start constructs.
                    if (CountLeadingIndentColumns() < 4 && TryScanThematicBreak(out _, out _, out _))
                    {
                        EmitListEnd();
                        goto ProcessLineStartConstruct;
                    }

                    bool closesListAtOutdent = _depth == 0 && CountLeadingIndentColumns() < 4;
                    if (_depth == 0 && CountLeadingIndentColumns() >= 4)
                    {
                        goto ProcessLineStartConstruct;
                    }
                    if (_depth > 0 || closesListAtOutdent)
                    {
                        bool parsedSibling = await TryParseSiblingListItemAsync();
                        if (!parsedSibling)
                        {
                            EmitListEnd();
                        }
                        else
                        {
                            // Eat the item's trailing line break, exactly as the line-start
                            // construct path does, so only real blank lines surface as Text.
                            bool ateNewline = false;
                            bool listItemOwnsLineBreak = _listItemOwnsLineBreak;
                            _listItemOwnsLineBreak = false;
                            if (!listItemOwnsLineBreak && Peek() == '\r')
                                Read();
                            if (!listItemOwnsLineBreak && Peek() == '\n')
                            {
                                Read();
                                ateNewline = true;
                            }
                            if (_depth > 0 || ateNewline)
                                _prefixConsumedThisLine = false;
                            continue;
                        }
                    }
                }

            ProcessLineStartConstruct:
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
                    bool listItemOwnsLineBreak = _listItemOwnsLineBreak;
                    _listItemOwnsLineBreak = false;
                    if (!listItemOwnsLineBreak && Peek() == '\r')
                    {
                        Read();
                    }
                    if (!listItemOwnsLineBreak && Peek() == '\n')
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
        // the last line; an open list is closed as well.
        EmitText();
        CloseParagraph();
        EmitListEnd();
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

    /// <summary>
    /// Scans the line at the current position as a potential thematic break without
    /// consuming it. A thematic break is a line consisting of three or more of the same
    /// marker character ('-', '*', or '_'), each optionally followed by any number of
    /// spaces, and nothing else before the line ending or end of stream. The leading
    /// indentation of the line is already buffered, so the scan starts at the first
    /// non-whitespace character. When the line is a break, <paramref name="marker"/>,
    /// <paramref name="markers"/> and <paramref name="lineLength"/> (the number of
    /// characters to consume, up to but not including the line ending) are set.
    /// </summary>
    private bool TryScanThematicBreak(out char marker, out int markers, out int lineLength)
    {
        marker = '\0';
        markers = 0;
        lineLength = 0;

        char c = PeekAhead(0);
        if (c is not ('-' or '*' or '_')) return false;

        int pos = 0;
        while (true)
        {
            char lookahead = PeekAhead(pos);
            if (lookahead == c)
            {
                markers++;
                pos++;
            }
            else if (lookahead == ' ')
            {
                pos++;
            }
            else
            {
                break;
            }
        }

        // The line must end (line ending or end of stream) and carry at least three markers.
        char terminator = PeekAhead(pos);
        if (markers < 3 || (terminator != '\r' && terminator != '\n' && terminator != '\0'))
            return false;

        marker = c;
        lineLength = pos;
        return true;
    }

    private bool TryParseHorizontalRule()
    {
        if (!TryScanThematicBreak(out char marker, out int markers, out int lineLength))
            return false;

        bool hadOpenParagraph = _block == BlockContext.Paragraph;

        // Flush the buffered leading whitespace (and any other buffered content) before the
        // break, matching how a list item flushes its marker indentation. Outside a
        // paragraph the HTML writer drops this text; it is only content inside a paragraph.
        EmitText();

        // Consume the break line up to (not including) the line ending; the main loop owns
        // the line ending (it re-arms _atLineStart and the blockquote decision table).
        for (int i = 0; i < lineLength; i++)
            Read();

        if (hadOpenParagraph)
        {
            CloseParagraph();
            _pendingSoftBreak = false;
        }

        _onToken(new MarkdownToken(MarkdownTokenType.HorizontalRule, new string(marker, markers)));

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

    // Classifies a line-start list marker without consuming it. Leading whitespace (up to
    // three columns) is skipped. Returns the list kind and, for ordered lists, the number
    // and delimiter ('.' or ')'); for unordered lists, the marker. Returns None when the
    // line does not start a list item. A marker must be followed by a single space (or,
    // for ordered items, by end of line).
    private ListClassification PeekListItem(int offset)
    {
        int pos = offset;
        int column = CountLeadingIndentColumns();
        if (column == 0)
        {
            while (column < 4 && (PeekAhead(pos) == ' ' || PeekAhead(pos) == '\t'))
            {
                column += PeekAhead(pos) == '\t' ? 4 - (column % 4) : 1;
                pos++;
            }
        }

        // Four or more columns of leading whitespace is indented code, not a list item.
        if (column >= 4)
        {
            return ListClassification.None;
        }

        int markerStart = pos;
        char c = PeekAhead(pos);

        if (c == '+' || c == '-' || c == '*')
        {
            char afterMarker = PeekAhead(pos + 1);
            if (afterMarker != ' ' && afterMarker != '\r' && afterMarker != '\n' && afterMarker != '\0')
                return ListClassification.None;
            if (_block == BlockContext.Paragraph && afterMarker is '\r' or '\n' or '\0')
                return ListClassification.None;
            return new ListClassification(ListKind.Unordered, marker: c, markerStart: markerStart, markerColumn: column);
        }

        if (char.IsDigit(c))
        {
            int digitStart = pos;
            int number = 0;
            // Ordered list markers are at most nine digits long; a run of ten or more
            // digits is not a list marker (the delimiter check below fails on the next
            // digit).
            while (char.IsDigit(PeekAhead(pos)) && pos - digitStart < 9)
            {
                number = number * 10 + (PeekAhead(pos) - '0');
                pos++;
            }
            int digits = pos - digitStart;

            char delimiter = PeekAhead(pos);
            if (delimiter is '.' or ')')
            {
                char after = PeekAhead(pos + 1);
                if (after == ' ' || after == '\r' || after == '\n' || after == '\0')
                {
                    if (_block == BlockContext.Paragraph && (number != 1 || after is '\r' or '\n' or '\0'))
                    {
                        return ListClassification.None;
                    }
                    return new ListClassification(ListKind.Ordered, marker: delimiter, number: number, digits: digits, markerStart: markerStart, markerColumn: column);
                }
            }
        }

        return ListClassification.None;
    }

    // Emits the ListStart for an open list that has no start token yet, remembering the
    // list kind and (for ordered lists) the delimiter.
    private void EmitListStart(ListClassification item)
    {
        if (_listState == ListState.None)
        {
            bool isOrdered = item.Kind == ListKind.Ordered;
            _listState = isOrdered ? ListState.Ordered : ListState.Unordered;
            _listDelimiter = isOrdered ? item.Marker : '\0';
            _onToken(new MarkdownToken(MarkdownTokenType.ListStart, string.Empty, new ListMetadata(isOrdered)));
        }
    }

    private void EmitListEnd()
    {
        if (_listState != ListState.None)
        {
            _onToken(new MarkdownToken(MarkdownTokenType.ListEnd, string.Empty, new ListMetadata(_listState == ListState.Ordered)));
            _listState = ListState.None;
        }
    }

    // Starts a new item in an open list. The marker must match the open list kind (and,
    // for ordered lists, its delimiter). The leading whitespace (if any) is in the buffer
    // and is flushed as a Text token; the marker plus its trailing space are consumed from
    // the stream before the item content is parsed.
    private async Task<bool> TryParseSiblingListItemAsync()
    {
        ListClassification item = PeekListItem(0);
        if (item.Kind == ListKind.None)
        {
            return false;
        }

        bool compatible = item.Kind == ListKind.Unordered
            ? _listState == ListState.Unordered
            : _listState == ListState.Ordered && item.Marker == _listDelimiter;
        if (!compatible)
        {
            return false;
        }

        EmitText();
        ConsumeMarker(item);
        await EmitListItemAsync(item);
        return true;
    }

    private async Task<bool> TryParseListItemAsync()
    {
        ListClassification item = PeekListItem(0);
        if (item.Kind == ListKind.None)
        {
            return false;
        }

        // A list is a block construct: it terminates an open paragraph. Close it before
        // emitting the list so the token order is ParagraphBlockEnd → ListStart → items.
        CloseParagraph();
        _pendingSoftBreak = false;

        EmitListStart(item);

        // Flush the leading indentation as text (decoration, not item content), then
        // consume the marker plus its trailing space, when present.
        EmitText();
        ConsumeMarker(item);

        await EmitListItemAsync(item);
        return true;
    }

    // Consumes the list marker (and its trailing space, when present) from the stream.
    // Any leading whitespace (spaces/tabs) before the marker is also consumed.
    private void ConsumeMarker(ListClassification item)
    {
        // Consume leading whitespace (if any).
        while (Peek() == ' ' || Peek() == '\t')
        {
            Read();
        }

        // Consume the marker itself.
        int markerLen = item.Kind == ListKind.Ordered ? item.Digits + 1 : 1;
        for (int i = 0; i < markerLen; i++)
        {
            Read();
        }

        // Consume one marker-padding column; any additional padding remains item content.
        // The reader uses the full original padding width for continuation classification.
        if (Peek() == ' ')
        {
            Read();
        }
    }

    // Emits a list item token and streams its (single-line) content through the inline
    // token handler pipe. The item token carries no value; the content follows as tokens.
    private async Task EmitListItemAsync(ListClassification item)
    {
        if (_depth == 0)
        {
            int contentOffset = GetListContentOffset(item);

            // A preceding line-start scan (e.g. the thematic-break probe) may have buffered
            // the tail of the marker's line, including its line ending. If that tail carries
            // no content (only whitespace up to the line ending), drop it so the
            // ListContentReader reads the first content line from the source and strips its
            // indentation: a line ending consumed from the shared buffer would otherwise
            // desync the reader's line tracking. When the tail does carry content it is left
            // in the shared buffer for the item sub-document to read.
            bool tailIsBlank = _lookaheadBuffer.Count > 0;
            foreach (char buffered in _lookaheadBuffer)
            {
                if (buffered is not (' ' or '\t' or '\r' or '\n'))
                {
                    tailIsBlank = false;
                    break;
                }
            }

            bool startsAtLineStart;
            if (tailIsBlank)
            {
                _lookaheadBuffer.Clear();
                startsAtLineStart = true;
            }
            else
            {
                startsAtLineStart = _lookaheadBuffer.Count > 0 && (_lookaheadBuffer.Peek() == '\r' || _lookaheadBuffer.Peek() == '\n');
            }

            var reader = new ListContentReader(Reader, contentOffset, Bob, startsAtLineStart);

            if (item.Kind == ListKind.Unordered)
            {
                await ParseInlines(MarkdownTokenType.UnorderedListItem, new ListItemMetadata(item.Marker),
                    handler => new MarkdownTokenizer(0).ParseAsync(reader, Bob, _lookaheadBuffer, handler));
            }
            else
            {
                await ParseInlines(MarkdownTokenType.OrderedListItem, new OrderedListItemMetadata(item.Number, item.Marker),
                    handler => new MarkdownTokenizer(0).ParseAsync(reader, Bob, _lookaheadBuffer, handler));
            }

            reader.Handoff(_lookaheadBuffer);
            _listItemOwnsLineBreak = true;
            return;
        }

        if (item.Kind == ListKind.Unordered)
        {
            await ParseInlines(MarkdownTokenType.UnorderedListItem, new ListItemMetadata(item.Marker), InlineMarkdownTokenizer.Create());
            return;
        }

        await ParseInlines(MarkdownTokenType.OrderedListItem, new OrderedListItemMetadata(item.Number, item.Marker), InlineMarkdownTokenizer.Create());
    }

    private int GetListContentOffset(ListClassification item)
    {
        int markerWidth = item.Kind == ListKind.Ordered ? item.Digits + 1 : 1;
        return item.MarkerColumn + markerWidth + 1;
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
