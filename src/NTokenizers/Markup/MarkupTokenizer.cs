using NTokenizers.CSharp;
using NTokenizers.Markup.Metadata;
using System.Diagnostics;
using System.Text;

namespace NTokenizers.Markup;

/// <summary>
/// A streaming tokenizer for Markdown/markup constructs using a character-by-character state machine.
/// </summary>
public sealed class MarkupTokenizer : BaseMarkupTokenizer
{
    /// <summary>
    /// Create a new instance of MarkupTokenizer.
    /// </summary>
    /// <returns></returns>
    public static MarkupTokenizer Create() => new();

    private bool _atLineStart = true;

    /// <summary>
    /// Parses the input stream and emits markup tokens via the OnToken callback.
    /// </summary>
    internal protected override async Task ParseAsync()
    {
        while (true)
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
                        _atLineStart = true;
                    }
                    else
                    {
                        _atLineStart = false;
                    }
                    continue;
                }
            }

            // Try inline constructs
            if (TryParseInlineConstruct())
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
        }

        // Emit any remaining text
        EmitText();
    }

    private async Task<bool> TryParseLineStartConstructAsync()
    {
        // Try heading
        if (await TryParseHeadingAsync()) return true;

        // Try horizontal rule
        if (TryParseHorizontalRule()) return true;

        // Try blockquote
        if (await TryParseBlockquoteAsync()) return true;

        // Try list items
        if (await TryParseListItemAsync()) return true;

        // Try code fence
        if (await TryParseCodeFence()) return true;

        // Try custom container
        if (TryParseCustomContainer()) return true;

        // Try table
        if (await TryParseTableAsync()) return true;

        return false;
    }

    private bool TryParseInlineConstruct()
    {
        int c = Peek();
        if (c == -1) return false;

        char ch = (char)c;

        // Try bold/italic
        if (ch == '*' && TryParseBoldOrItalic()) return true;
        if (ch == '_' && TryParseBoldOrItalic()) return true;

        // Try inline code
        if (ch == '`' && TryParseInlineCode()) return true;

        // Try link or image
        if (ch == '[' && TryParseLink()) return true;
        if (ch == '!' && PeekAhead(1) == '[' && TryParseImage()) return true;

        // Try emoji
        if (ch == ':' && TryParseEmoji()) return true;

        // Try subscript
        if (ch == '^' && TryParseSubscript()) return true;

        // Try superscript
        if (ch == '~' && TryParseSuperscript()) return true;

        // Try inserted text
        if (ch == '+' && PeekAhead(1) == '+' && TryParseInsertedText()) return true;

        // Try marked text
        if (ch == '=' && PeekAhead(1) == '=' && TryParseMarkedText()) return true;

        // Try HTML tag
        if (ch == '<' && TryParseHtmlTag()) return true;

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

        // Create metadata without inline callback initially
        var metadata = new HeadingMetadata(level);

        // Emit heading token with empty value (client can set OnInlineToken to parse inline content)
        var emitTask = Task.Run(() =>
            _onToken(new MarkupToken(MarkupTokenType.Heading, string.Empty, metadata)));

        // Await the client registering the handler
        var inlineTokenHandler = await metadata.GetInlineTokenHandlerAsync();

        await InlineMarkupTokenizer.Create().ParseAsync(Reader, Bob, inlineTokenHandler);

        // Ensure the heading token emission is complete
        await emitTask;

        metadata.CompleteProcessing();

        return true;
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

        _onToken(new MarkupToken(MarkupTokenType.HorizontalRule, new string(c, count)));

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

        // Create metadata without inline callback initially
        var metadata = new BlockquoteMetadata();

        // Emit blockquote token with empty value (client can set OnInlineToken to parse inline content)
        var emitTask = Task.Run(() =>
            _onToken(new MarkupToken(MarkupTokenType.Blockquote, string.Empty, metadata))
        );

        // Await the client registering the handler
        var inlineTokenHandler = await metadata.GetInlineTokenHandlerAsync();

        await InlineMarkupTokenizer.Create().ParseAsync(Reader, Bob, inlineTokenHandler);

        await emitTask;

        metadata.CompleteProcessing();

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

            var metadata = new ListItemMetadata(marker);

            // Emit unordered list item token with empty value
            var emitTask = Task.Run(() =>
                _onToken(new MarkupToken(MarkupTokenType.UnorderedListItem, string.Empty, metadata)));

            var inlineTokenHandler = await metadata.GetInlineTokenHandlerAsync();

            await InlineMarkupTokenizer.Create().ParseAsync(Reader, Bob, inlineTokenHandler);

            await emitTask;

            metadata.CompleteProcessing();

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

                // Create metadata without inline callback initially
                var metadata = new OrderedListItemMetadata(number);

                // Emit ordered list item token with empty value
                var emitTask = Task.Run(() =>
                    _onToken(new MarkupToken(MarkupTokenType.OrderedListItem, string.Empty, metadata)));

                var inlineTokenHandler = await metadata.GetInlineTokenHandlerAsync();

                await InlineMarkupTokenizer.Create().ParseAsync(Reader, Bob, inlineTokenHandler);

                await emitTask;

                metadata.CompleteProcessing();

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

        string language = lang.ToString().Trim().ToLowerInvariant();

        // Create appropriate metadata based on language
        InlineMarkupMetadata metadata = language.ToLowerInvariant() switch
        {
            "csharp" or "cs" or "c#" => new CSharpCodeBlockMetadata(language),
            "json" => new JsonCodeBlockMetadata(language),
            "xml" or "html" => new XmlCodeBlockMetadata(language),
            "sql" => new SqlCodeBlockMetadata(language),
            "typescript" or "ts" or "javascript" or "js" => new TypeScriptCodeBlockMetadata(language),
            _ => new GenericCodeBlockMetadata(language)
        };

        // Emit code block token with empty value (client can set OnInlineToken for syntax highlighting)
        var emitTask = Task.Run(() =>
            _onToken(new MarkupToken(
                MarkupTokenType.CodeBlock,
                string.Empty,
                metadata
            ))
        );

        // Check if client set OnInlineToken during the callback
        // If so, delegate to specialized tokenizer for syntax highlighting
        await DelegateToLanguageTokenizerAsync(metadata);

        await emitTask;

        metadata.CompleteProcessing();

        return true;
    }

    /// <summary>
    /// Delegates code block content to the appropriate language tokenizer.
    /// Emits language-specific tokens (e.g., CSharpToken, JsonToken) via the OnInlineToken callback.
    /// </summary>
    private async Task DelegateToLanguageTokenizerAsync(MarkupMetadata metadata)
    {
        try
        {
            switch (metadata)
            {
                case CSharpCodeBlockMetadata csharpMeta:
                    var csharpHandler = await csharpMeta.GetInlineTokenHandlerAsync();
                    await CSharpTokenizer.Create().ParseAsync(Reader, "```", csharpHandler);
                    break;
                case JsonCodeBlockMetadata jsonMeta:
                    var jsonHandler = await jsonMeta.GetInlineTokenHandlerAsync();
                    await Json.JsonTokenizer.Create().ParseAsync(Reader, "```", jsonHandler);
                    break;
                case XmlCodeBlockMetadata xmlMeta:
                    var xmlHandler = await xmlMeta.GetInlineTokenHandlerAsync();
                    await Xml.XmlTokenizer.Create().ParseAsync(Reader, "```", xmlHandler);
                    break;
                case SqlCodeBlockMetadata sqlMeta:
                    var sqlHandler = await sqlMeta.GetInlineTokenHandlerAsync();
                    await Sql.SqlTokenizer.Create().ParseAsync(Reader, "```", sqlHandler);
                    break;
                case TypeScriptCodeBlockMetadata tsMeta:
                    var tsHandler = await tsMeta.GetInlineTokenHandlerAsync();
                    await Typescript.TypescriptTokenizer.Create().ParseAsync(Reader, "```", tsHandler);
                    break;
                case GenericCodeBlockMetadata gMeta:
                    var genHandler = await gMeta.GetInlineTokenHandlerAsync();
                    await Generic.GenericTokenizer.Create().ParseAsync(Reader, "```", genHandler);
                    break;
            }
        }
        catch
        {
            // Silently fail if delegation doesn't work
        }
    }

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

        _onToken(new MarkupToken(MarkupTokenType.CustomContainer, containerType.ToString().Trim()));

        return true;
    }

    private bool TryParseHtmlTag()
    {
        if (Peek() != '<') return false;

        // Check if it looks like an HTML tag
        char next = PeekAhead(1);

        // Must start with letter or / for closing tags
        if (!char.IsLetter(next) && next != '/')
            return false;

        EmitText();
        Read(); // Consume <

        // Read tag content until >
        var tagContent = new StringBuilder();
        tagContent.Append('<');

        while (Peek() != -1)
        {
            char c = (char)Read();
            tagContent.Append(c);

            if (c == '>')
            {
                _onToken(new MarkupToken(MarkupTokenType.HtmlTag, tagContent.ToString()));
                return true;
            }
        }

        // No closing found, treat as text
        _buffer.Append(tagContent);
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

        var emitTask = Task.Run(() =>
            _onToken(new MarkupToken(MarkupTokenType.Table, string.Empty, metadata)));

        // Await the client registering the handler
        var inlineTokenHandler = await metadata.GetInlineTokenHandlerAsync();

        var tableTokenizer = new TableMarkupTokenizer(metadata);

        Debug.WriteLine($"'{Peek()}'");

        await tableTokenizer.ParseAsync(Reader, Bob, inlineTokenHandler);

        // Ensure the table token emission is complete
        await emitTask;

        metadata.CompleteProcessing();

        if (Peek() == '|')
        {
            Read();
        }

        return true;
    }

}
