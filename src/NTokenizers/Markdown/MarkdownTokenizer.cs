using NTokenizers.Core;
using NTokenizers.Markdown.Metadata;
using System.Diagnostics;
using System.Text;

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

            // Try to parse special constructs at line start
            if (_atLineStart && !char.IsWhiteSpace(c))
            {
                if (await TryParseLineStartConstructAsync())
                {
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
                EmitText();
                _atLineStart = true;
            }
            else if (_atLineStart && !char.IsWhiteSpace(c))
            {
                _atLineStart = false;
            }

            if (!_atLineStart && c == ' ' && _buffer.Length > 1)
            {
                EmitText();
            }
        }

        // Emit any remaining text
        EmitText();
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

        // Emit any pending text
        EmitText();

        // Consume the # characters
        for (int i = 0; i < level; i++)
            Read();

        // Skip whitespace after #
        while (char.IsWhiteSpace((char)Peek()) && Peek() != '\n')
            Read();

        return await ParseInlines(MarkdownTokenType.Heading, new HeadingMetadata(level), InlineMarkdownTokenizer.Create());
    }

    private async Task<bool> ParseInlines<TToken>(MarkdownTokenType tokenType, InlineMarkdownMetadata<TToken> metadata, Func<Action<TToken>, Task> parseAsync) where TToken : IToken
    {
        // Emit heading token with empty value (client can set OnInlineToken to parse inline content)
        var emitTask = Task.Run(() =>
            _onToken(new MarkdownToken(tokenType, string.Empty, metadata)));

        // Await the client registering the handler
        var handlerTask = metadata.GetInlineTokenHandlerAsync();

        using var cts = new CancellationTokenSource();
        var delayTask = Task.Delay(2000, cts.Token); // 2-second timeout

        var completedTask = await Task.WhenAny(handlerTask, delayTask);

        if (completedTask == handlerTask)
        {
            cts.Cancel();
            Debug.WriteLine("Canceling wait token");
        }

        if (completedTask != handlerTask || handlerTask.Result == null)
        {
            // No handler registered within timeout
            await emitTask;
            return false;
        }

        // Run the tokenizer
        await parseAsync(handlerTask.Result);

        // Ensure the heading token emission is complete
        await emitTask;

        metadata.CompleteProcessing();
        return true;
    }

    private Task<bool> ParseInlines<TToken>(MarkdownTokenType tokenType, InlineMarkdownMetadata<TToken> metadata, BaseTokenizer<TToken> tokenizer) where TToken : IToken =>
        ParseInlines(tokenType, metadata, handler => tokenizer.ParseAsync(Reader, Bob, handler));

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

            EmitText();
            Read(); // Consume marker
            Read(); // Consume space

            await ParseInlines(MarkdownTokenType.UnorderedListItem, new ListItemMetadata(marker), InlineMarkdownTokenizer.Create());

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
                EmitText();

                // Consume number and dot
                for (int i = 0; i <= pos; i++)
                    Read();
                Read(); // Consume space

                await ParseInlines(MarkdownTokenType.OrderedListItem, new OrderedListItemMetadata(number), InlineMarkdownTokenizer.Create());

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
        "xml" => await ParseCodeInlines(new XmlCodeBlockMetadata(language)),
        "html" => await ParseCodeInlines(new HtmlCodeBlockMetadata(language)),
        "yaml" => await ParseCodeInlines(new YamlCodeBlockMetadata(language)),
        "sql" => await ParseCodeInlines(new SqlCodeBlockMetadata(language)),
        "typescript" or "ts" or "javascript" or "js" => await ParseCodeInlines(new TypeScriptCodeBlockMetadata(language)),
        "css" => await ParseCodeInlines(new CssCodeBlockMetadata(language)),
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
