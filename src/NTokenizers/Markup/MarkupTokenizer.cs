using NTokenizers.Core;
using NTokenizers.CSharp;
using NTokenizers.Markup.Metadata;
using System.Text;

namespace NTokenizers.Markup;

/// <summary>
/// A streaming tokenizer for Markdown/markup constructs using a character-by-character state machine.
/// </summary>
public sealed class MarkupTokenizer : BaseTokenizer<MarkupToken>
{
    /// <summary>
    /// Create a new instance of MarkupTokenizer.
    /// </summary>
    /// <returns></returns>
    public static MarkupTokenizer Create() => new();

    private readonly Queue<char> _lookaheadBuffer = new();
    private bool _atLineStart = true;

    /// <summary>
    /// Parses the input stream and emits markup tokens via the OnToken callback.
    /// </summary>
    internal protected override void Parse()
    {
        while (true)
        {
            int peek = Peek();
            if (peek == -1) break;

            char c = (char)peek;

            // Try to parse special constructs at line start
            if (_atLineStart && !char.IsWhiteSpace(c))
            {
                if (TryParseLineStartConstruct())
                {
                    //Eat newline after line-start construct
                    if (Peek() == '\r') 
                    {
                        Read();
                    }
                    if(Peek() == '\n')
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
            _sb.Append(c);

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

    private int Peek()
    {
        if (_lookaheadBuffer.Count > 0)
            return _lookaheadBuffer.Peek();

        return _reader.Peek();
    }

    private int Read()
    {
        if (_lookaheadBuffer.Count > 0)
            return _lookaheadBuffer.Dequeue();

        return _reader.Read();
    }

    private char PeekAhead(int offset)
    {
        // Ensure we have enough characters in the lookahead buffer
        while (_lookaheadBuffer.Count <= offset)
        {
            int c = _reader.Read();
            if (c == -1)
                return '\0';
            _lookaheadBuffer.Enqueue((char)c);
        }

        // Return the character at the offset
        return _lookaheadBuffer.ElementAt(offset);
    }

    private bool TryParseLineStartConstruct()
    {
        // Try heading
        if (TryParseHeading()) return true;

        // Try horizontal rule
        if (TryParseHorizontalRule()) return true;

        // Try blockquote
        if (TryParseBlockquote()) return true;

        // Try list items
        if (TryParseListItem()) return true;

        // Try code fence
        if (TryParseCodeFence()) return true;

        // Try custom container
        if (TryParseCustomContainer()) return true;

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

        // Try table delimiter
        if (ch == '|' && TryParseTableCell()) return true;

        return false;
    }

    private bool TryParseHeading()
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
        var t = Task.Run(() =>
            _onToken(new MarkupToken(
                MarkupTokenType.Heading,
                string.Empty,
                metadata))
        );

        // Wait until client sets OnInlineToken
        while (metadata.OnInlineToken is null)
        {
            Thread.Sleep(3);
        }

        ParseInlineTokens(metadata);

        // Ensure the heading token emission is complete
        t.Wait();

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

    private bool TryParseBlockquote()
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
        var t = Task.Run(() =>
            _onToken(new MarkupToken(MarkupTokenType.Blockquote, string.Empty, metadata))
        );
        // Check if client set OnInlineToken during the callback
        // If so, parse inline tokens and stream them
        // Wait until client sets OnInlineToken
        while (metadata.OnInlineToken is null)
        {
            Thread.Sleep(3);
        }

        ParseInlineTokens(metadata);

        t.Wait();

        return true;
    }

    private bool TryParseListItem()
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
            var t = Task.Run(() =>
                _onToken(new MarkupToken(MarkupTokenType.UnorderedListItem, string.Empty, metadata))
                );

            // Check if client set OnInlineToken during the callback
            // If so, parse inline tokens and stream them
            // Wait until client sets OnInlineToken
            while (metadata.OnInlineToken is null)
            {
                Thread.Sleep(3);
            }

            ParseInlineTokens(metadata);

            t.Wait();

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
                var t = Task.Run(() =>
                    _onToken(new MarkupToken(
                        MarkupTokenType.OrderedListItem,
                        string.Empty,
                        metadata
                    ))
                );

                // Check if client set OnInlineToken during the callback
                // If so, parse inline tokens and stream them
                // Wait until client sets OnInlineToken
                while (metadata.OnInlineToken is null)
                {
                    Thread.Sleep(3);
                }

                ParseInlineTokens(metadata);

                t.Wait();

                return true;
            }
        }

        return false;
    }

    private bool TryParseCodeFence()
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
        MarkupMetadata metadata = language.ToLowerInvariant() switch
        {
            "csharp" or "cs" or "c#" => new CSharpCodeBlockMetadata(language),
            "json" => new JsonCodeBlockMetadata(language),
            "xml" or "html" => new XmlCodeBlockMetadata(language),
            "sql" => new SqlCodeBlockMetadata(language),
            "typescript" or "ts" or "javascript" or "js" => new TypeScriptCodeBlockMetadata(language),
            _ => new GenericCodeBlockMetadata(language)
        };

        // Emit code block token with empty value (client can set OnInlineToken for syntax highlighting)
        var t = Task.Run(() =>
            _onToken(new MarkupToken(
                MarkupTokenType.CodeBlock,
                string.Empty,
                metadata
            ))
        );

        // Check if client set OnInlineToken during the callback
        // If so, delegate to specialized tokenizer for syntax highlighting
        DelegateToLanguageTokenizer(metadata);

        t.Wait();

        return true;
    }

    /// <summary>
    /// Delegates code block content to the appropriate language tokenizer.
    /// Emits language-specific tokens (e.g., CSharpToken, JsonToken) via the OnInlineToken callback.
    /// </summary>
    private void DelegateToLanguageTokenizer(MarkupMetadata metadata)
    {
        try
        {
            if (metadata is IInlineMarkupMedata inlineMarkupMedata)
            {
                inlineMarkupMedata.WaitForCallbackClient();

                switch (metadata)
                {
                    case CSharpCodeBlockMetadata csharpMeta:
                        CSharpTokenizer.Create().Parse(_reader, "```", csharpMeta.OnInlineToken!);
                        break;
                    case JsonCodeBlockMetadata jsonMeta:
                        Json.JsonTokenizer.Create().Parse(_reader, "```", jsonMeta.OnInlineToken!);
                        break;
                    case XmlCodeBlockMetadata xmlMeta:
                        Xml.XmlTokenizer.Create().Parse(_reader, "```", xmlMeta.OnInlineToken!);
                        break;
                    case SqlCodeBlockMetadata sqlMeta:
                        Sql.SqlTokenizer.Create().Parse(_reader, "```", sqlMeta.OnInlineToken!);
                        break;
                    case TypeScriptCodeBlockMetadata tsMeta:
                        Typescript.TypescriptTokenizer.Create().Parse(_reader, "```", tsMeta.OnInlineToken!);
                        break;
                    case GenericCodeBlockMetadata gMeta:
                        Generic.GenericTokenizer.Create().Parse(_reader, "```", gMeta.OnInlineToken!);
                        break;
                }

                inlineMarkupMedata.IsProcessing = false;
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

    private bool TryParseBoldOrItalic()
    {
        // Check for **
        if (PeekAhead(0) == '*' && PeekAhead(1) == '*')
        {
            EmitText();
            Read();
            Read();

            // Read bold text until closing **
            var boldText = new StringBuilder();
            while (Peek() != -1)
            {
                if (PeekAhead(0) == '*' && PeekAhead(1) == '*')
                {
                    Read();
                    Read();
                    _onToken(new MarkupToken(MarkupTokenType.Bold, boldText.ToString()));
                    return true;
                }
                boldText.Append((char)Read());
            }

            // No closing found, treat as text
            _sb.Append("**").Append(boldText);
            return true;
        }

        // Handle __bold__ syntax
        if (PeekAhead(0) == '_' && PeekAhead(1) == '_')
        {
            EmitText();
            Read();
            Read();
            var boldText = new StringBuilder();
            while (Peek() != -1)
            {
                if (PeekAhead(0) == '_' && PeekAhead(1) == '_')
                {
                    Read(); Read();
                    _onToken(new MarkupToken(MarkupTokenType.Bold, boldText.ToString()));
                    return true;
                }
                boldText.Append((char)Read());
            }
            _sb.Append("__").Append(boldText);
            return false;
        }

        // Check for single *
        if (PeekAhead(0) == '*')
        {
            EmitText();
            Read();

            // Read italic text until closing *
            var italicText = new StringBuilder();
            while (Peek() != -1)
            {
                if (Peek() == '*')
                {
                    Read();
                    _onToken(new MarkupToken(MarkupTokenType.Italic, italicText.ToString()));
                    return true;
                }
                italicText.Append((char)Read());
            }

            // No closing found, treat as text
            _sb.Append('*').Append(italicText);
            return true;
        }

        // Handle _italic_ syntax
        if (PeekAhead(0) == '_' && PeekAhead(1) != '_')
        {
            EmitText();
            Read();
            var italicText = new StringBuilder();
            while (Peek() != -1 && Peek() != '_')
            {
                italicText.Append((char)Read());
            }
            if (Peek() == '_')
            {
                Read();
                _onToken(new MarkupToken(MarkupTokenType.Italic, italicText.ToString()));
                return true;
            }
            _sb.Append('_').Append(italicText);
            return false;
        }

        return false;
    }

    private bool TryParseInlineCode()
    {
        if (Peek() != '`') return false;

        EmitText();
        Read(); // Consume opening `

        // Read code until closing `
        var code = new StringBuilder();
        while (Peek() != -1 && Peek() != '\n')
        {
            char c = (char)Read();
            if (c == '`')
            {
                _onToken(new MarkupToken(MarkupTokenType.CodeInline, code.ToString()));
                return true;
            }
            code.Append(c);
        }

        // No closing found, treat as text
        _sb.Append('`').Append(code);
        return true;
    }

    private bool TryParseLink()
    {
        if (Peek() != '[') return false;

        // Look ahead for ]( pattern
        int pos = 1;
        while (PeekAhead(pos) != '\0' && PeekAhead(pos) != '\n' && PeekAhead(pos) != ']')
            pos++;

        if (PeekAhead(pos) != ']' || PeekAhead(pos + 1) != '(')
            return false;

        EmitText();
        Read(); // Consume [

        // Read link text
        var linkText = new StringBuilder();
        while (Peek() != -1 && Peek() != ']')
        {
            linkText.Append((char)Read());
        }

        if (Peek() != ']') return false;
        Read(); // Consume ]

        if (Peek() != '(') return false;
        Read(); // Consume (

        // Read URL
        var url = new StringBuilder();
        string? title = null;
        bool inQuote = false;
        var titleBuilder = new StringBuilder();

        while (Peek() != -1 && Peek() != ')')
        {
            char c = (char)Read();

            if (c == '"' && !inQuote)
            {
                inQuote = true;
            }
            else if (c == '"' && inQuote)
            {
                title = titleBuilder.ToString();
                inQuote = false;
            }
            else if (inQuote)
            {
                titleBuilder.Append(c);
            }
            else if (c == ' ' && url.Length > 0 && Peek() == '"')
            {
                // Space before title
            }
            else
            {
                url.Append(c);
            }
        }

        if (Peek() == ')')
            Read(); // Consume )

        _onToken(new MarkupToken(
            MarkupTokenType.Link,
            linkText.ToString(),
            new LinkMetadata(url.ToString().Trim(), title)
        ));

        return true;
    }

    private bool TryParseImage()
    {
        if (PeekAhead(0) != '!' || PeekAhead(1) != '[')
            return false;

        EmitText();
        Read(); // Consume !
        Read(); // Consume [

        // Read alt text
        var altText = new StringBuilder();
        while (Peek() != -1 && Peek() != ']')
        {
            altText.Append((char)Read());
        }

        if (Peek() != ']') return false;
        Read(); // Consume ]

        if (Peek() != '(') return false;
        Read(); // Consume (

        // Read URL
        var url = new StringBuilder();
        string? title = null;
        bool inQuote = false;
        var titleBuilder = new StringBuilder();

        while (Peek() != -1 && Peek() != ')')
        {
            char c = (char)Read();

            if (c == '"' && !inQuote)
            {
                inQuote = true;
            }
            else if (c == '"' && inQuote)
            {
                title = titleBuilder.ToString();
                inQuote = false;
            }
            else if (inQuote)
            {
                titleBuilder.Append(c);
            }
            else if (c == ' ' && url.Length > 0 && Peek() == '"')
            {
                // Space before title
            }
            else
            {
                url.Append(c);
            }
        }

        if (Peek() == ')')
            Read(); // Consume )

        _onToken(new MarkupToken(
            MarkupTokenType.Image,
            altText.ToString(),
            new LinkMetadata(url.ToString().Trim(), title)
        ));

        return true;
    }

    private bool TryParseEmoji()
    {
        if (Peek() != ':') return false;

        // Look ahead for closing :
        int pos = 1;
        while (PeekAhead(pos) != '\0' && PeekAhead(pos) != '\n' && PeekAhead(pos) != ':' && pos < 50)
        {
            if (!char.IsLetterOrDigit(PeekAhead(pos)) && PeekAhead(pos) != '_' && PeekAhead(pos) != '-')
                return false;
            pos++;
        }

        if (PeekAhead(pos) != ':' || pos == 1)
            return false;

        EmitText();
        Read(); // Consume opening :

        // Read emoji name
        var emojiName = new StringBuilder();
        while (Peek() != -1 && Peek() != ':')
        {
            emojiName.Append((char)Read());
        }

        if (Peek() == ':')
            Read(); // Consume closing :

        _onToken(new MarkupToken(
            MarkupTokenType.Emoji,
            emojiName.ToString(),
            new EmojiMetadata(emojiName.ToString())
        ));

        return true;
    }

    private bool TryParseSubscript()
    {
        if (Peek() != '^') return false;

        EmitText();
        Read(); // Consume opening ^

        // Read subscript text until closing ^
        var subText = new StringBuilder();
        while (Peek() != -1 && Peek() != '\n')
        {
            char c = (char)Read();
            if (c == '^')
            {
                _onToken(new MarkupToken(MarkupTokenType.Subscript, subText.ToString()));
                return true;
            }
            subText.Append(c);
        }

        // No closing found, treat as text
        _sb.Append('^').Append(subText);
        return true;
    }

    private bool TryParseSuperscript()
    {
        if (Peek() != '~') return false;

        EmitText();
        Read(); // Consume opening ~

        // Read superscript text until closing ~
        var supText = new StringBuilder();
        while (Peek() != -1 && Peek() != '\n')
        {
            char c = (char)Read();
            if (c == '~')
            {
                _onToken(new MarkupToken(MarkupTokenType.Superscript, supText.ToString()));
                return true;
            }
            supText.Append(c);
        }

        // No closing found, treat as text
        _sb.Append('~').Append(supText);
        return true;
    }

    private bool TryParseInsertedText()
    {
        if (PeekAhead(0) != '+' || PeekAhead(1) != '+')
            return false;

        EmitText();
        Read(); // Consume first +
        Read(); // Consume second +

        // Read inserted text until closing ++
        var insText = new StringBuilder();
        while (Peek() != -1)
        {
            if (PeekAhead(0) == '+' && PeekAhead(1) == '+')
            {
                Read();
                Read();
                _onToken(new MarkupToken(MarkupTokenType.InsertedText, insText.ToString()));
                return true;
            }
            insText.Append((char)Read());
        }

        // No closing found, treat as text
        _sb.Append("++").Append(insText);
        return true;
    }

    private bool TryParseMarkedText()
    {
        if (PeekAhead(0) != '=' || PeekAhead(1) != '=')
            return false;

        EmitText();
        Read(); // Consume first =
        Read(); // Consume second =

        // Read marked text until closing ==
        var markedText = new StringBuilder();
        while (Peek() != -1)
        {
            if (PeekAhead(0) == '=' && PeekAhead(1) == '=')
            {
                Read();
                Read();
                _onToken(new MarkupToken(MarkupTokenType.MarkedText, markedText.ToString()));
                return true;
            }
            markedText.Append((char)Read());
        }

        // No closing found, treat as text
        _sb.Append("==").Append(markedText);
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
        _sb.Append(tagContent);
        return true;
    }

    private bool TryParseTableCell()
    {
        if (Peek() != '|') return false;

        EmitText();
        Read(); // Consume |

        // Read cell content until next | or newline
        var cellContent = new StringBuilder();
        while (Peek() != -1 && Peek() != '|' && Peek() != '\n')
        {
            cellContent.Append((char)Read());
        }

        string content = cellContent.ToString().Trim();

        // Emit table cell token with empty value
        // Note: TableCell doesn't have metadata currently for OnInlineToken support
        // This could be enhanced if needed
        _onToken(new MarkupToken(MarkupTokenType.TableCell, string.Empty));

        return true;
    }

    /// <summary>
    /// Parses inline tokens from a string and invokes the callback for each token found.
    /// This enables streaming of inline markup (bold, italic, code, links, etc.) within other constructs.
    /// </summary>
    private void ParseInlineTokens(InlineMarkupMetadata<MarkupToken> inlineMarkupMetadata)
    {
        InlineParserContext.Create().Parse(_reader, inlineMarkupMetadata.OnInlineToken!);
        inlineMarkupMetadata.IsProcessing = false;
    }

    /// <summary>
    /// Specialized parser context for inline tokens only (no block-level constructs).
    /// </summary>
    private class InlineParserContext : BaseTokenizer<MarkupToken>
    {
        internal static InlineParserContext Create() => new();
        private readonly Queue<char> _lookaheadBuffer = new();

        internal protected override void Parse()
        {
            while (Peek() != -1 && Peek() != '\n' && Peek() != '\r')
            {
                // Try inline constructs
                if (TryParseBoldOrItalic()) continue;
                if (TryParseInlineCode()) continue;
                if (TryParseLink()) continue;
                if (TryParseImage()) continue;
                if (TryParseEmoji()) continue;
                if (TryParseSubscript()) continue;
                if (TryParseSuperscript()) continue;
                if (TryParseInsertedText()) continue;
                if (TryParseMarkedText()) continue;

                // Regular character
                _sb.Append((char)Read());
            }

            EmitText();
        }

        private int Peek()
        {
            if (_lookaheadBuffer.Count > 0)
                return _lookaheadBuffer.Peek();
            return _reader.Peek();
        }

        private int Read()
        {
            if (_lookaheadBuffer.Count > 0)
                return _lookaheadBuffer.Dequeue();
            return _reader.Read();
        }

        private char PeekAhead(int offset)
        {
            while (_lookaheadBuffer.Count <= offset)
            {
                int next = _reader.Read();
                if (next == -1) return '\0';
                _lookaheadBuffer.Enqueue((char)next);
            }
            return _lookaheadBuffer.ElementAt(offset);
        }

        private bool TryParseBoldOrItalic()
        {
            // Handle **bold** syntax
            if (PeekAhead(0) == '*' && PeekAhead(1) == '*')
            {
                EmitText();
                Read(); Read();
                var boldText = new StringBuilder();
                while (Peek() != -1)
                {
                    if (PeekAhead(0) == '*' && PeekAhead(1) == '*')
                    {
                        Read(); Read();
                        _onToken(new MarkupToken(MarkupTokenType.Bold, boldText.ToString()));
                        return true;
                    }
                    boldText.Append((char)Read());
                }
                _sb.Append("**").Append(boldText);
                return false;
            }

            // Handle __bold__ syntax
            if (PeekAhead(0) == '_' && PeekAhead(1) == '_')
            {
                EmitText();
                Read(); Read();
                var boldText = new StringBuilder();
                while (Peek() != -1)
                {
                    if (PeekAhead(0) == '_' && PeekAhead(1) == '_')
                    {
                        Read(); Read();
                        _onToken(new MarkupToken(MarkupTokenType.Bold, boldText.ToString()));
                        return true;
                    }
                    boldText.Append((char)Read());
                }
                _sb.Append("__").Append(boldText);
                return false;
            }

            // Handle *italic* syntax
            if (PeekAhead(0) == '*' && PeekAhead(1) != '*')
            {
                EmitText();
                Read();
                var italicText = new StringBuilder();
                while (Peek() != -1 && Peek() != '*')
                {
                    italicText.Append((char)Read());
                }
                if (Peek() == '*')
                {
                    Read();
                    _onToken(new MarkupToken(MarkupTokenType.Italic, italicText.ToString()));
                    return true;
                }
                _sb.Append('*').Append(italicText);
                return false;
            }

            // Handle _italic_ syntax
            if (PeekAhead(0) == '_' && PeekAhead(1) != '_')
            {
                EmitText();
                Read();
                var italicText = new StringBuilder();
                while (Peek() != -1 && Peek() != '_')
                {
                    italicText.Append((char)Read());
                }
                if (Peek() == '_')
                {
                    Read();
                    _onToken(new MarkupToken(MarkupTokenType.Italic, italicText.ToString()));
                    return true;
                }
                _sb.Append('_').Append(italicText);
                return false;
            }

            return false;
        }

        private bool TryParseInlineCode()
        {
            if (Peek() != '`') return false;
            EmitText();
            Read();
            var codeText = new StringBuilder();
            while (Peek() != -1 && Peek() != '`' && Peek() != '\n')
            {
                codeText.Append((char)Read());
            }
            if (Peek() == '`')
            {
                Read();
                _onToken(new MarkupToken(MarkupTokenType.CodeInline, codeText.ToString()));
                return true;
            }
            _sb.Append('`').Append(codeText);
            return false;
        }

        private bool TryParseLink()
        {
            if (Peek() != '[') return false;
            int pos = 1;
            var text = new StringBuilder();
            while (PeekAhead(pos) != ']' && PeekAhead(pos) != '\0' && PeekAhead(pos) != '\n')
            {
                text.Append(PeekAhead(pos));
                pos++;
            }
            if (PeekAhead(pos) != ']' || PeekAhead(pos + 1) != '(') return false;

            pos += 2;
            var url = new StringBuilder();
            while (PeekAhead(pos) != ')' && PeekAhead(pos) != '\0' && PeekAhead(pos) != '\n')
            {
                url.Append(PeekAhead(pos));
                pos++;
            }
            if (PeekAhead(pos) != ')') return false;

            EmitText();
            for (int i = 0; i <= pos; i++) Read();
            _onToken(new MarkupToken(MarkupTokenType.Link, text.ToString(), new LinkMetadata(url.ToString())));
            return true;
        }

        private bool TryParseImage()
        {
            if (PeekAhead(0) != '!' || PeekAhead(1) != '[') return false;
            int pos = 2;
            var alt = new StringBuilder();
            while (PeekAhead(pos) != ']' && PeekAhead(pos) != '\0')
            {
                alt.Append(PeekAhead(pos));
                pos++;
            }
            if (PeekAhead(pos) != ']' || PeekAhead(pos + 1) != '(') return false;

            pos += 2;
            var url = new StringBuilder();
            while (PeekAhead(pos) != ')' && PeekAhead(pos) != '\0')
            {
                url.Append(PeekAhead(pos));
                pos++;
            }
            if (PeekAhead(pos) != ')') return false;

            EmitText();
            for (int i = 0; i <= pos; i++) Read();
            _onToken(new MarkupToken(MarkupTokenType.Image, alt.ToString(), new LinkMetadata(url.ToString())));
            return true;
        }

        private bool TryParseEmoji()
        {
            if (Peek() != ':') return false;
            int pos = 1;
            var name = new StringBuilder();
            while (char.IsLetterOrDigit(PeekAhead(pos)) || PeekAhead(pos) == '_')
            {
                name.Append(PeekAhead(pos));
                pos++;
            }
            if (PeekAhead(pos) != ':' || name.Length == 0) return false;

            EmitText();
            for (int i = 0; i <= pos; i++) Read();
            _onToken(new MarkupToken(MarkupTokenType.Emoji, name.ToString(), new EmojiMetadata(name.ToString())));
            return true;
        }

        private bool TryParseSubscript()
        {
            if (Peek() != '~') return false;
            EmitText();
            Read();
            var text = new StringBuilder();
            while (Peek() != -1 && Peek() != '~' && Peek() != '\n')
            {
                text.Append((char)Read());
            }
            if (Peek() == '~')
            {
                Read();
                _onToken(new MarkupToken(MarkupTokenType.Subscript, text.ToString()));
                return true;
            }
            _sb.Append('~').Append(text);
            return false;
        }

        private bool TryParseSuperscript()
        {
            if (Peek() != '^') return false;
            EmitText();
            Read();
            var text = new StringBuilder();
            while (Peek() != -1 && Peek() != '^' && Peek() != '\n')
            {
                text.Append((char)Read());
            }
            if (Peek() == '^')
            {
                Read();
                _onToken(new MarkupToken(MarkupTokenType.Superscript, text.ToString()));
                return true;
            }
            _sb.Append('^').Append(text);
            return false;
        }

        private bool TryParseInsertedText()
        {
            if (PeekAhead(0) != '+' || PeekAhead(1) != '+') return false;
            EmitText();
            Read(); Read();
            var text = new StringBuilder();
            while (Peek() != -1)
            {
                if (PeekAhead(0) == '+' && PeekAhead(1) == '+')
                {
                    Read(); Read();
                    _onToken(new MarkupToken(MarkupTokenType.InsertedText, text.ToString()));
                    return true;
                }
                text.Append((char)Read());
            }
            _sb.Append("++").Append(text);
            return false;
        }

        private bool TryParseMarkedText()
        {
            if (PeekAhead(0) != '=' || PeekAhead(1) != '=') return false;
            EmitText();
            Read(); Read();
            var text = new StringBuilder();
            while (Peek() != -1)
            {
                if (PeekAhead(0) == '=' && PeekAhead(1) == '=')
                {
                    Read(); Read();
                    _onToken(new MarkupToken(MarkupTokenType.MarkedText, text.ToString()));
                    return true;
                }
                text.Append((char)Read());
            }
            _sb.Append("==").Append(text);
            return false;
        }

        private void EmitText()
        {
            if (_sb.Length > 0)
            {
                _onToken(new MarkupToken(MarkupTokenType.Text, _sb.ToString()));
                _sb.Clear();
            }
        }
    }

    private void EmitText()
    {
        if (_sb.Length > 0)
        {
            _onToken(new MarkupToken(MarkupTokenType.Text, _sb.ToString()));
            _sb.Clear();
        }
    }
}
