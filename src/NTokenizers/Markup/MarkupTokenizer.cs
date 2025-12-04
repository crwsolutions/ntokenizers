using NTokenizers.Core;
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

    private async Task<bool> TryParseLineStartConstructAsync() => true switch
    {
        true when await TryParseHeadingAsync() => true,
        true when TryParseHorizontalRule() => true,
        true when await TryParseBlockquoteAsync() => true,
        true when await TryParseListItemAsync() => true,
        true when await TryParseCodeFence() => true,
        true when TryParseCustomContainer() => true,
        true when await TryParseTableAsync() => true,
        _ => false,
    };

    private bool TryParseInlineConstruct(char ch) => ch switch
    {
        '*' when TryParseBoldOrItalic() => true,
        '_' when TryParseBoldOrItalic() => true,
        '`' when TryParseInlineCode() => true,
        '[' when TryParseLink() => true,
        '!' when PeekAhead(1) == '[' && TryParseImage() => true,
        ':' when TryParseEmoji() => true,
        '^' when TryParseSubscript() => true,
        '~' when TryParseSuperscript() => true,
        '+' when PeekAhead(1) == '+' && TryParseInsertedText() => true,
        '=' when PeekAhead(1) == '=' && TryParseMarkedText() => true,
        '<' when TryParseHtmlTag() => true,
        _ => false
    };

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

        await ParseInlines(MarkupTokenType.Heading, new HeadingMetadata(level), InlineMarkupTokenizer.Create());

        return true;
    }

    private async Task ParseInlinesCommon<TToken>(MarkupTokenType tokenType, InlineMarkupMetadata<TToken> metadata, Func<Action<TToken>, Task> parseAsync) where TToken : IToken
    {
        // Emit heading token with empty value (client can set OnInlineToken to parse inline content)
        var emitTask = Task.Run(() =>
            _onToken(new MarkupToken(tokenType, string.Empty, metadata)));

        // Await the client registering the handler
        var inlineTokenHandler = await metadata.GetInlineTokenHandlerAsync();

        // Run the tokenizer
        await parseAsync(inlineTokenHandler);

        // Ensure the heading token emission is complete
        await emitTask;

        metadata.CompleteProcessing();
    }


    private Task ParseInlines<TToken>(MarkupTokenType tokenType, InlineMarkupMetadata<TToken> metadata, BaseTokenizer<TToken> tokenizer) where TToken : IToken =>
        ParseInlinesCommon(tokenType, metadata, handler => tokenizer.ParseAsync(Reader, Bob, handler));

    private Task ParseCodeInlines<TToken>(MarkupTokenType tokenType, InlineMarkupMetadata<TToken> metadata, BaseSubTokenizer<TToken> tokenizer) where TToken : IToken =>
        ParseInlinesCommon(tokenType, metadata, handler => tokenizer.ParseAsync(Reader, "```", handler));

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

        await ParseInlines(MarkupTokenType.Blockquote, new BlockquoteMetadata(), InlineMarkupTokenizer.Create());

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

            await ParseInlines(MarkupTokenType.UnorderedListItem, new ListItemMetadata(marker), InlineMarkupTokenizer.Create());

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

                await ParseInlines(MarkupTokenType.OrderedListItem, new OrderedListItemMetadata(number), InlineMarkupTokenizer.Create());

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

        await DelegateToLanguageTokenizerAsync(metadata);

        return true;
    }

    /// <summary>
    /// Delegates code block content to the appropriate language tokenizer.
    /// Emits language-specific tokens (e.g., CSharpToken, JsonToken) via the OnInlineToken callback.
    /// </summary>
    private async Task DelegateToLanguageTokenizerAsync(MarkupMetadata metadata)
    {
        switch (metadata)
        {
            case CSharpCodeBlockMetadata csharpMeta:
                await ParseCodeInlines(MarkupTokenType.CodeBlock, csharpMeta, CSharpTokenizer.Create());
                break;
            case JsonCodeBlockMetadata jsonMeta:
                await ParseCodeInlines(MarkupTokenType.CodeBlock, jsonMeta, Json.JsonTokenizer.Create());
                break;
            case XmlCodeBlockMetadata xmlMeta:
                await ParseCodeInlines(MarkupTokenType.CodeBlock, xmlMeta, Xml.XmlTokenizer.Create());
                break;
            case SqlCodeBlockMetadata sqlMeta:
                await ParseCodeInlines(MarkupTokenType.CodeBlock, sqlMeta, Sql.SqlTokenizer.Create());
                break;
            case TypeScriptCodeBlockMetadata tsMeta:
                await ParseCodeInlines(MarkupTokenType.CodeBlock, tsMeta, Typescript.TypescriptTokenizer.Create());
                break;
            case GenericCodeBlockMetadata gMeta:
                await ParseCodeInlines(MarkupTokenType.CodeBlock, gMeta, Generic.GenericTokenizer.Create());
                break;
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
        await ParseInlines(MarkupTokenType.Table, metadata, new TableMarkupTokenizer(metadata));

        if (Peek() == '|')
        {
            Read();
        }

        return true;
    }
}
