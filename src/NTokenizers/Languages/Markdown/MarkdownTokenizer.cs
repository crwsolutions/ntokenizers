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
public sealed class MarkdownTokenizer : BaseMarkdownTokenizer
{
    /// <summary>
    /// Create a new instance of MarkdownTokenizer.
    /// </summary>
    /// <returns></returns>
    public static MarkdownTokenizer Create() => new();

    private bool _atLineStart = true;
    private BlockContext _block = BlockContext.None;
    private bool _pendingSoftBreak;

    // Blank lines inside an open indented code block that are not yet known to belong to the
    // block. Each buffered blank line contributes its (indentation-removed) content plus "\n".
    // They are flushed when a code content line follows, and dropped when the block ends first.
    private readonly StringBuilder _pendingBlankContent = new();

    // The open block context. None, Paragraph and IndentedCodeBlock are mutually exclusive, so a
    // single enum tracks them instead of several booleans.
    private enum BlockContext
    {
        None,
        Paragraph,
        IndentedCodeBlock
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

            // Inside an indented code block, consume the lines literally (no inline parsing)
            // until the block ends. When the block ends on a non-indented line, that line is
            // left unprocessed in the stream for the normal paragraph/construct handling on
            // the next iteration. The position after ProcessCodeBlock is always at a line
            // start, so _atLineStart is set there.
            if (_block == BlockContext.IndentedCodeBlock)
            {
                ProcessCodeBlock();
                continue;
            }

            // Try to parse special constructs at line start (headings, lists, fences, etc.).
            // An indented code line (>= 4 spaces) is checked first because it begins with
            // whitespace, and it cannot interrupt a paragraph. Line-start constructs (which
            // allow up to 3 spaces of indentation) are checked afterwards.
            if (_atLineStart)
            {
                // Indented (>= 4 spaces) code line at the top level. It cannot interrupt a
                // paragraph, so it is only started when no paragraph is open.
                if (_block != BlockContext.Paragraph && TryStartIndentedCodeBlock())
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
                    if (Peek() == '\r')
                    {
                        Read();
                    }
                    if (Peek() == '\n')
                    {
                        Read();
                    }

                    _atLineStart = true;
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
            }

            if (!_atLineStart && c == ' ' && _buffer.Length > 1)
            {
                EmitText();
            }
        }

        // End of stream: close an open indented code block (its pending blank lines are
        // dropped) or a still-open paragraph, and emit any remaining text.
        if (_block == BlockContext.IndentedCodeBlock)
        {
            CloseIndentedCodeBlock();
            EmitText();
        }
        else
        {
            // Emit any remaining text; a paragraph still open at EOF ends with the last line.
            EmitText();
            CloseParagraph();
        }
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
    /// non-blank content. Returns true when a block was started and the whole first line has
    /// been consumed, so the loop can continue with the next line. A block is never started
    /// while a paragraph is open (an indented code block cannot interrupt a paragraph) and a
    /// blank line does not open a block on its own.
    /// </summary>
    private bool TryStartIndentedCodeBlock()
    {
        // Classify the line at the current position without consuming it; only an indented
        // content line starts a block, so the line is read only when a block actually starts.
        if (PeekCurrentLineKind() != LineStart.IndentedContent)
        {
            return false;
        }

        (string line, bool hasLineEnding) = ReadLineText();
        _atLineStart = true; // The whole first line (and its line ending) has been consumed.

        // Open the block and emit the first line (its leading indentation removed). The line
        // ending is emitted only when the input has one, so the token stream stays faithful
        // to the input.
        EmitText();
        _block = BlockContext.IndentedCodeBlock;
        _pendingSoftBreak = false;
        _pendingBlankContent.Clear();
        _onToken(new MarkdownToken(MarkdownTokenType.IndentedCodeBlockStart, string.Empty));
        _onToken(new MarkdownToken(MarkdownTokenType.Text, RemoveLeadingIndent(line) + (hasLineEnding ? "\n" : string.Empty)));
        return true;
    }

    /// <summary>
    /// Processes an open indented code block, line by line, until the block ends:
    /// <list type="bullet">
    /// <item>A line with at least four columns of indentation and non-blank content is code
    /// content; its first four columns are removed and the rest is emitted immediately.</item>
    /// <item>A blank line (no characters, or only spaces/tabs) may be followed by code
    /// content; it is buffered tentatively, kept when code content follows, and dropped
    /// when the block ends first (including end of stream).</item>
    /// <item>Any other line ends the block.</item>
    /// </list>
    /// Only the start of the next line (indentation plus first character) ever needs to be
    /// looked ahead, so the lookahead stays bounded. The line that ends the block is left
    /// untouched in the stream so the main loop processes it as a normal line (paragraph,
    /// heading, list, ...).
    /// </summary>
    private void ProcessCodeBlock()
    {
        while (true)
        {
            // End of stream: the block ends; any pending blank lines are dropped.
            if (Peek() == -1)
            {
                break;
            }

            // Classify the line at the current position without consuming it.
            LineStart kind = PeekCurrentLineKind();

            if (kind == LineStart.IndentedContent)
            {
                // Code content: flush the pending blank lines that precede it, then emit
                // this line (its first four columns of indentation removed). The line
                // ending is appended only when the input has one, so the token stream
                // stays faithful to the input.
                (string line, bool hasLineEnding) = ReadLineText();
                _atLineStart = true; // The line (and its line ending) has been consumed.
                _pendingBlankContent.Append(RemoveLeadingIndent(line));
                if (hasLineEnding)
                {
                    _pendingBlankContent.Append('\n');
                }
                _onToken(new MarkdownToken(MarkdownTokenType.Text, _pendingBlankContent.ToString()));
                _pendingBlankContent.Clear();
                continue;
            }

            if (kind == LineStart.Blank)
            {
                // A blank line may be followed by code content: buffer it tentatively. Its
                // first four columns are removed too, so an indented blank line keeps any
                // whitespace beyond those columns. A blank line always ends the line in
                // the input, so its line ending is buffered as well.
                (string line, _) = ReadLineText();
                _atLineStart = true; // The blank line (and its line ending) has been consumed.
                _pendingBlankContent.Append(RemoveLeadingIndent(line)).Append('\n');
                continue;
            }

            // A non-indented, non-blank line ends the block. It is left untouched so the
            // main loop processes it as a normal line; the pending blank lines before it
            // are dropped.
            break;
        }

        // The position is at a line start: either at the start of the line that ends the
        // block (left untouched for the main loop) or at the end of the stream.
        _atLineStart = true;
        CloseIndentedCodeBlock();
    }

    /// <summary>
    /// Reads one line from the stream: the line content (with any trailing \r removed; an
    /// empty string at end of stream) and whether a line ending was present. The line
    /// ending is consumed as well, leaving the position at the start of the next line.
    /// The caller decides whether to include the line ending in its output.
    /// </summary>
    private (string line, bool hasLineEnding) ReadLineText()
    {
        var line = new StringBuilder();
        while (true)
        {
            int c = Peek();
            if (c == -1 || c == '\n')
            {
                break;
            }
            line.Append((char)Read());
        }
        if (line.Length > 0 && line[line.Length - 1] == '\r')
        {
            line.Length--;
        }
        bool hasLineEnding = Peek() == '\n';
        if (hasLineEnding)
        {
            Read();
        }
        return (line.Length > 0 ? line.ToString() : string.Empty, hasLineEnding);
    }

    /// <summary>
    /// What kind of line starts at the current position.
    /// </summary>
    private enum LineStart
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
    /// Classifies the line at the current position, which must be a line start. Only the leading
    /// spaces/tabs and the first content character are examined, so the lookahead stays bounded.
    /// Nothing is consumed; the position is left at the start of the line.
    /// </summary>
    private LineStart PeekCurrentLineKind()
    {
        int columns = 0;
        int pos = 0;
        while (true)
        {
            char c = PeekAhead(pos);
            if (c != ' ' && c != '\t')
            {
                break;
            }
            // A tab counts as the number of spaces to the next 4-column tab stop.
            columns += c == '\t' ? 4 - (columns % 4) : 1;
            pos++;
        }

        char ch = PeekAhead(pos);
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
    /// Removes up to the first four columns of indentation (spaces or tabs) from the given
    /// line, preserving any additional indentation beyond the first four columns. A line with
    /// fewer than four columns of indentation is reduced to its remaining (non-whitespace)
    /// content, so an indented blank line never adds visible indentation to the code.
    /// </summary>
    private static string RemoveLeadingIndent(string line)
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

    /// <summary>
    /// Closes the open indented code block by emitting an IndentedCodeBlockEnd token. Any pending
    /// (trailing blank line) content is discarded.
    /// </summary>
    private void CloseIndentedCodeBlock()
    {
        if (_block == BlockContext.IndentedCodeBlock)
        {
            _block = BlockContext.None;
            _pendingBlankContent.Clear(); // Trailing blank lines do not belong to the block.
            _onToken(new MarkdownToken(MarkdownTokenType.IndentedCodeBlockEnd, string.Empty));
        }
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

    private Task<bool> ParseCodeInlines<TToken>(CodeBlockMetadata<TToken> metadata) where TToken : IToken =>
        ParseInlines(MarkdownTokenType.CodeBlock, metadata, handler => metadata.CreateTokenizer().ParseAsync(Reader, Bob, "```", handler));

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

    private async Task<bool> TryParseBlockquoteAsync()
    {
        if (Peek() != '>') return false;

        EmitText();
        Read(); // Consume >

        // Skip whitespace after >
        if (Peek() == ' ')
            Read();

        await ParseInlines(MarkdownTokenType.Blockquote, new BlockquoteMetadata(), InlineMarkdownTokenizer.Create());

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
