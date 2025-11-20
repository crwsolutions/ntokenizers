using System.Text;

namespace NTokenizers.Markup;

/// <summary>
/// Provides functionality for tokenizing Markdown/markup text sources using a state machine.
/// </summary>
public static class MarkupTokenizer
{
    private enum State
    {
        Text,                      // Reading plain text
        LineStart,                 // At the beginning of a line
        InHeading,                 // Collecting heading markers (#)
        InBold,                    // Inside bold text (**)
        InItalic,                  // Inside italic text (*)
        InCodeInline,              // Inside inline code (`)
        InCodeFence,               // Inside code fence block
        InCodeFenceLanguage,       // Reading language after ```
        InLinkText,                // Inside [link text]
        InLinkUrl,                 // Inside (url)
        InLinkTitle,               // Inside "title" within link
        InImageAlt,                // Inside ![alt text]
        InImageUrl,                // Inside (url) for image
        InImageTitle,              // Inside "title" for image
        InEmoji,                   // Inside :emoji:
        InBlockquote,              // Processing blockquote line
        InHorizontalRule,          // Processing horizontal rule
        InUnorderedList,           // Processing unordered list item
        InOrderedList,             // Processing ordered list item
        InTable,                   // Inside table row
        InSubscript,               // Inside ^subscript^
        InSuperscript,             // Inside ~superscript~
        InInserted,                // Inside ++inserted++
        InMarked,                  // Inside ==marked==
        InFootnoteRef,             // Inside [^footnote]
        InFootnoteDef,             // Inside [^footnote]: definition
        InCustomContainer,         // Inside ::: container
        InHtmlTag,                 // Inside <html> tag
        InAbbreviation             // Inside *[abbreviation]: definition
    }

    /// <summary>
    /// Parses markup/Markdown content from the given <see cref="Stream"/> and
    /// produces a sequence of <see cref="MarkupToken"/> objects.
    /// </summary>
    /// <param name="stream">
    /// The input stream containing the text to tokenize.
    /// The stream is read as UTF-8.
    /// </param>
    /// <param name="onToken">
    /// A callback invoked for each <see cref="MarkupToken"/> produced during parsing.
    /// This delegate must not be <c>null</c>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="stream"/> or <paramref name="onToken"/> is <c>null</c>.
    /// </exception>
    public static void Parse(Stream stream, Action<MarkupToken> onToken)
    {
        if (stream == null) throw new ArgumentNullException(nameof(stream));
        if (onToken == null) throw new ArgumentNullException(nameof(onToken));

        using var reader = new StreamReader(stream, Encoding.UTF8);
        ParseInternal(reader, onToken);
    }

    private static void ParseInternal(TextReader reader, Action<MarkupToken> onToken)
    {
        var sb = new StringBuilder();
        var state = State.LineStart;
        var prevChar = '\0';
        var lineStartPos = true;
        var headingLevel = 0;
        var codeFenceLanguage = string.Empty;
        var inCodeFence = false;
        var codeFenceDelimiter = string.Empty;
        var linkTextDepth = 0;
        var imageMode = false;
        
        // Create a buffered reader for lookahead
        var bufferedReader = new BufferedCharReader(reader);

        while (true)
        {
            int ic = bufferedReader.Read();
            if (ic == -1)
            {
                EmitPending(sb, state, onToken, headingLevel, codeFenceLanguage);
                break;
            }

            char c = (char)ic;
            
            // Track if we're at the start of a line
            if (prevChar == '\n' || prevChar == '\0')
            {
                lineStartPos = true;
            }

            ProcessChar(c, ref state, sb, onToken, ref prevChar, ref lineStartPos, 
                       ref headingLevel, ref codeFenceLanguage, ref inCodeFence,
                       ref codeFenceDelimiter, ref linkTextDepth, ref imageMode, 
                       bufferedReader);

            prevChar = c;
            
            if (c == '\n')
            {
                lineStartPos = true;
            }
            else if (!char.IsWhiteSpace(c))
            {
                lineStartPos = false;
            }
        }
    }

    // Helper class to provide lookahead capability while reading character-by-character
    private class BufferedCharReader
    {
        private readonly TextReader _reader;
        private readonly Queue<char> _buffer = new Queue<char>();

        public BufferedCharReader(TextReader reader)
        {
            _reader = reader;
        }

        public int Read()
        {
            if (_buffer.Count > 0)
            {
                return _buffer.Dequeue();
            }
            return _reader.Read();
        }

        public bool Peek(int ahead, out char[] result)
        {
            // Fill buffer to have enough characters
            while (_buffer.Count < ahead)
            {
                int ic = _reader.Read();
                if (ic == -1)
                {
                    result = _buffer.ToArray();
                    return false;
                }
                _buffer.Enqueue((char)ic);
            }

            result = _buffer.Take(ahead).ToArray();
            return _buffer.Count >= ahead;
        }

        public void Consume(int count)
        {
            for (int i = 0; i < count && _buffer.Count > 0; i++)
            {
                _buffer.Dequeue();
            }
        }
    }

    private static void ProcessChar(char c, ref State state, StringBuilder sb, 
                                    Action<MarkupToken> onToken, ref char prevChar,
                                    ref bool lineStartPos, ref int headingLevel,
                                    ref string codeFenceLanguage, ref bool inCodeFence,
                                    ref string codeFenceDelimiter, ref int linkTextDepth,
                                    ref bool imageMode, BufferedCharReader reader)
    {
        switch (state)
        {
            case State.Text:
                HandleTextState(c, ref state, sb, onToken, ref prevChar, ref lineStartPos, 
                               ref headingLevel, ref linkTextDepth, ref imageMode);
                break;

            case State.LineStart:
                HandleLineStartState(c, ref state, sb, onToken, ref lineStartPos, 
                                    ref headingLevel, ref inCodeFence, ref codeFenceDelimiter,
                                    ref codeFenceLanguage, reader);
                break;

            case State.InHeading:
                HandleHeadingState(c, ref state, sb, onToken, ref headingLevel);
                break;

            case State.InBold:
                HandleBoldState(c, ref state, sb, onToken, ref prevChar);
                break;

            case State.InItalic:
                HandleItalicState(c, ref state, sb, onToken);
                break;

            case State.InCodeInline:
                HandleCodeInlineState(c, ref state, sb, onToken);
                break;

            case State.InCodeFence:
                HandleCodeFenceState(c, ref state, sb, onToken, ref inCodeFence, 
                                    ref codeFenceDelimiter, ref codeFenceLanguage, ref prevChar);
                break;

            case State.InCodeFenceLanguage:
                HandleCodeFenceLanguageState(c, ref state, sb, onToken, ref codeFenceLanguage,
                                            ref inCodeFence, ref codeFenceDelimiter);
                break;

            case State.InLinkText:
                HandleLinkState(c, ref state, sb, onToken, ref linkTextDepth, ref imageMode);
                break;

            case State.InLinkUrl:
                HandleLinkState(c, ref state, sb, onToken, ref imageMode);
                break;

            case State.InImageAlt:
                HandleImageState(c, ref state, sb, onToken, ref linkTextDepth);
                break;

            case State.InImageUrl:
                HandleImageState(c, ref state, sb, onToken);
                break;

            case State.InEmoji:
                HandleEmojiState(c, ref state, sb, onToken);
                break;

            case State.InBlockquote:
                HandleBlockquoteState(c, ref state, sb, onToken, ref lineStartPos);
                break;

            case State.InHtmlTag:
                HandleHtmlTagState(c, ref state, sb, onToken);
                break;

            case State.InSubscript:
                HandleSubscriptState(c, ref state, sb, onToken);
                break;

            case State.InSuperscript:
                HandleSuperscriptState(c, ref state, sb, onToken);
                break;

            case State.InInserted:
                HandleInsertedState(c, ref state, sb, onToken, ref prevChar);
                break;

            case State.InMarked:
                HandleMarkedState(c, ref state, sb, onToken, ref prevChar);
                break;

            case State.InFootnoteRef:
                HandleFootnoteRefState(c, ref state, sb, onToken);
                break;

            case State.InCustomContainer:
                HandleCustomContainerState(c, ref state, sb, onToken);
                break;

            case State.InUnorderedList:
                HandleUnorderedListState(c, ref state, sb, onToken);
                break;

            case State.InOrderedList:
                HandleOrderedListState(c, ref state, sb, onToken, ref headingLevel);
                break;

            case State.InTable:
                HandleTableState(c, ref state, sb, onToken);
                break;

            default:
                // For other states, just collect characters
                sb.Append(c);
                if (c == '\n')
                {
                    state = State.LineStart;
                }
                break;
        }
    }

    private static void HandleTextState(char c, ref State state, StringBuilder sb,
                                       Action<MarkupToken> onToken, ref char prevChar,
                                       ref bool lineStartPos, ref int headingLevel,
                                       ref int linkTextDepth, ref bool imageMode)
    {
        if (c == '\n')
        {
            if (sb.Length > 0)
            {
                onToken(new MarkupToken(MarkupTokenType.Text, sb.ToString()));
                sb.Clear();
            }
            onToken(new MarkupToken(MarkupTokenType.Text, "\n"));
            state = State.LineStart;
            return;
        }

        // Check for inline code
        if (c == '`')
        {
            if (sb.Length > 0)
            {
                onToken(new MarkupToken(MarkupTokenType.Text, sb.ToString()));
                sb.Clear();
            }
            state = State.InCodeInline;
            return;
        }

        // Check for bold delimiter **
        if (c == '*' && prevChar == '*')
        {
            // Remove the previous * from text
            if (sb.Length > 0 && sb[sb.Length - 1] == '*')
            {
                sb.Length--;
            }
            if (sb.Length > 0)
            {
                onToken(new MarkupToken(MarkupTokenType.Text, sb.ToString()));
                sb.Clear();
            }
            // Transition to bold state instead of emitting delimiter
            state = State.InBold;
            return;
        }

        // Check for italic delimiter * (only if not followed by another *)
        if (c == '*')
        {
            // We need to check if this is italic or bold
            // For now, just append and let the next iteration decide
            sb.Append(c);
            return;
        }

        // Check for links [text](url)
        if (c == '[')
        {
            if (prevChar == '!')
            {
                // Image: ![alt](url)
                if (sb.Length > 0 && sb[sb.Length - 1] == '!')
                {
                    sb.Length--;
                }
                if (sb.Length > 0)
                {
                    onToken(new MarkupToken(MarkupTokenType.Text, sb.ToString()));
                    sb.Clear();
                }
                imageMode = true;
                state = State.InImageAlt;
                return;
            }
            else
            {
                if (sb.Length > 0)
                {
                    onToken(new MarkupToken(MarkupTokenType.Text, sb.ToString()));
                    sb.Clear();
                }
                linkTextDepth = 1;
                state = State.InLinkText;
                return;
            }
        }

        // Check for HTML tags
        if (c == '<')
        {
            if (sb.Length > 0)
            {
                onToken(new MarkupToken(MarkupTokenType.Text, sb.ToString()));
                sb.Clear();
            }
            sb.Append(c);
            state = State.InHtmlTag;
            return;
        }

        // Check for subscript ^text^
        if (c == '^')
        {
            if (sb.Length > 0)
            {
                onToken(new MarkupToken(MarkupTokenType.Text, sb.ToString()));
                sb.Clear();
            }
            state = State.InSubscript;
            return;
        }

        // Check for superscript ~text~
        if (c == '~')
        {
            if (sb.Length > 0)
            {
                onToken(new MarkupToken(MarkupTokenType.Text, sb.ToString()));
                sb.Clear();
            }
            state = State.InSuperscript;
            return;
        }

        // Check for inserted ++text++
        if (c == '+' && prevChar == '+')
        {
            if (sb.Length > 0 && sb[sb.Length - 1] == '+')
            {
                sb.Length--;
            }
            if (sb.Length > 0)
            {
                onToken(new MarkupToken(MarkupTokenType.Text, sb.ToString()));
                sb.Clear();
            }
            state = State.InInserted;
            return;
        }

        // Check for marked ==text==
        if (c == '=' && prevChar == '=')
        {
            if (sb.Length > 0 && sb[sb.Length - 1] == '=')
            {
                sb.Length--;
            }
            if (sb.Length > 0)
            {
                onToken(new MarkupToken(MarkupTokenType.Text, sb.ToString()));
                sb.Clear();
            }
            state = State.InMarked;
            return;
        }

        // Check for emoji :name: only if the previous character was whitespace or start
        if (c == ':' && (prevChar == '\0' || char.IsWhiteSpace(prevChar) || prevChar == '\n'))
        {
            if (sb.Length > 0)
            {
                onToken(new MarkupToken(MarkupTokenType.Text, sb.ToString()));
                sb.Clear();
            }
            state = State.InEmoji;
            return;
        }

        sb.Append(c);
    }

    private static void HandleLineStartState(char c, ref State state, StringBuilder sb,
                                            Action<MarkupToken> onToken, ref bool lineStartPos,
                                            ref int headingLevel, ref bool inCodeFence,
                                            ref string codeFenceDelimiter, ref string codeFenceLanguage,
                                            BufferedCharReader reader)
    {
        // Handle newline - stay in LineStart
        if (c == '\n')
        {
            if (sb.Length > 0)
            {
                onToken(new MarkupToken(MarkupTokenType.Text, sb.ToString()));
                sb.Clear();
            }
            onToken(new MarkupToken(MarkupTokenType.Text, "\n"));
            // Stay in LineStart state
            return;
        }

        // Check for heading #
        if (c == '#')
        {
            headingLevel = 1;
            state = State.InHeading;
            return;
        }

        // Check for blockquote >
        if (c == '>')
        {
            // Don't emit delimiter, just transition to blockquote state to collect content
            state = State.InBlockquote;
            return;
        }

        // Check for code fence ```
        if (c == '`')
        {
            sb.Append(c);
            // Check for ``` pattern by peeking ahead
            if (reader.Peek(2, out var next2) && next2.Length == 2 && next2[0] == '`' && next2[1] == '`')
            {
                // Consume the next two backticks
                reader.Consume(2);
                
                if (inCodeFence)
                {
                    // End of code fence
                    onToken(new MarkupToken(MarkupTokenType.CodeBlock, "```"));
                    inCodeFence = false;
                    codeFenceDelimiter = string.Empty;
                    codeFenceLanguage = string.Empty;
                    sb.Clear();
                    state = State.Text;
                }
                else
                {
                    // Start of code fence
                    codeFenceDelimiter = "```";
                    state = State.InCodeFenceLanguage;
                    sb.Clear();
                }
                return;
            }
            else
            {
                state = State.Text;
                return;
            }
        }

        // Check for horizontal rule --- or ***
        if (c == '-' || c == '*')
        {
            sb.Append(c);
            if (reader.Peek(2, out var next2) && next2.Length == 2 && next2[0] == c && next2[1] == c)
            {
                reader.Consume(2);
                onToken(new MarkupToken(MarkupTokenType.HorizontalRule, new string(c, 3)));
                sb.Clear();
                state = State.Text;
                return;
            }
            // Check if it's a list item (- or * followed by space)
            else if (reader.Peek(1, out var next1) && next1.Length == 1 && char.IsWhiteSpace(next1[0]))
            {
                // Consume the space and transition to collecting list item content
                sb.Clear();
                state = State.InUnorderedList;
                return;
            }
            else
            {
                state = State.Text;
                return;
            }
        }

        // Check for unordered list + (separate check for +)
        if (c == '+')
        {
            if (reader.Peek(1, out var next1) && next1.Length == 1 && char.IsWhiteSpace(next1[0]))
            {
                // Transition to collecting list item content
                sb.Clear();
                state = State.InUnorderedList;
                return;
            }
        }

        // Check for ordered list 1., 2., etc.
        if (char.IsDigit(c))
        {
            sb.Append(c);
            int digitCount = 1;
            
            // Peek ahead to find more digits followed by '.' and space
            while (digitCount < 10)
            {
                if (reader.Peek(digitCount, out var peek) && peek.Length == digitCount && char.IsDigit(peek[digitCount - 1]))
                {
                    digitCount++;
                }
                else
                {
                    break;
                }
            }
            
            if (reader.Peek(digitCount, out var peekFinal) && peekFinal.Length == digitCount && peekFinal[digitCount - 1] == '.')
            {
                // Check if there's a space after the dot
                if (reader.Peek(digitCount + 1, out var peekSpace) && peekSpace.Length == digitCount + 1 && char.IsWhiteSpace(peekSpace[digitCount]))
                {
                    // Extract the list item number
                    int listItemNumber = int.Parse(sb.ToString());
                    
                    // Consume all the digits and the dot
                    for (int i = 0; i < digitCount; i++)
                    {
                        reader.Read();
                    }
                    
                    sb.Clear();
                    // Store the item number in headingLevel temporarily (we'll use a better approach later)
                    headingLevel = listItemNumber;
                    state = State.InOrderedList;
                    return;
                }
            }
            
            state = State.Text;
            return;
        }

        // Check for custom container :::
        if (c == ':')
        {
            if (reader.Peek(2, out var next2) && next2.Length == 2 && next2[0] == ':' && next2[1] == ':')
            {
                reader.Consume(2);
                state = State.InCustomContainer;
                return;
            }
        }

        // Check for table |
        if (c == '|')
        {
            // Don't emit delimiter, transition to table state to collect cell content
            state = State.InTable;
            return;
        }

        // Default to text
        state = State.Text;
        sb.Append(c);
    }

    private static void HandleHeadingState(char c, ref State state, StringBuilder sb,
                                          Action<MarkupToken> onToken, ref int headingLevel)
    {
        if (c == '#' && headingLevel < 6 && sb.Length == 0)
        {
            // Still counting heading markers
            headingLevel++;
        }
        else if (c == '\n')
        {
            // End of heading - emit the heading token with accumulated text
            string headingText = sb.ToString().TrimStart(); // Remove leading whitespace after #
            onToken(new MarkupToken(
                MarkupTokenType.Heading,
                headingText,
                new HeadingMetadata(headingLevel)
            ));
            sb.Clear();
            headingLevel = 0;
            state = State.LineStart;
        }
        else
        {
            // Accumulate heading text
            sb.Append(c);
        }
    }

    private static void HandleBoldState(char c, ref State state, StringBuilder sb,
                                       Action<MarkupToken> onToken, ref char prevChar)
    {
        if (c == '*' && prevChar == '*')
        {
            // Remove the previous * from bold text
            if (sb.Length > 0 && sb[sb.Length - 1] == '*')
            {
                sb.Length--;
            }
            // Emit bold token with accumulated text
            onToken(new MarkupToken(MarkupTokenType.Bold, sb.ToString()));
            sb.Clear();
            state = State.Text;
        }
        else
        {
            sb.Append(c);
        }
    }

    private static void HandleItalicState(char c, ref State state, StringBuilder sb,
                                         Action<MarkupToken> onToken)
    {
        if (c == '*')
        {
            // End of italic - emit token with accumulated text
            onToken(new MarkupToken(MarkupTokenType.Italic, sb.ToString()));
            sb.Clear();
            state = State.Text;
        }
        else
        {
            sb.Append(c);
        }
    }

    private static void HandleCodeInlineState(char c, ref State state, StringBuilder sb,
                                             Action<MarkupToken> onToken)
    {
        if (c == '`')
        {
            onToken(new MarkupToken(MarkupTokenType.CodeInline, sb.ToString()));
            sb.Clear();
            state = State.Text;
        }
        else
        {
            sb.Append(c);
        }
    }

    private static void HandleCodeFenceState(char c, ref State state, StringBuilder sb,
                                            Action<MarkupToken> onToken, ref bool inCodeFence,
                                            ref string codeFenceDelimiter, ref string codeFenceLanguage,
                                            ref char prevChar)
    {
        sb.Append(c);
        
        // Check if we hit the closing fence
        if (c == '`' && prevChar == '`')
        {
            string current = sb.ToString();
            if (current.EndsWith("```"))
            {
                // Extract content before the fence
                string content = current.Substring(0, current.Length - 3);
                if (content.Length > 0)
                {
                    // If language specified, delegate parsing
                    if (!string.IsNullOrEmpty(codeFenceLanguage))
                    {
                        DelegateCodeBlockParsing(content, codeFenceLanguage, onToken);
                    }
                    else
                    {
                        onToken(new MarkupToken(MarkupTokenType.CodeBlock, content));
                    }
                }
                onToken(new MarkupToken(MarkupTokenType.CodeBlock, "```"));
                sb.Clear();
                inCodeFence = false;
                codeFenceDelimiter = string.Empty;
                codeFenceLanguage = string.Empty;
                state = State.Text;
            }
        }
    }

    private static void HandleCodeFenceLanguageState(char c, ref State state, StringBuilder sb,
                                                     Action<MarkupToken> onToken, ref string codeFenceLanguage,
                                                     ref bool inCodeFence, ref string codeFenceDelimiter)
    {
        if (c == '\n')
        {
            codeFenceLanguage = sb.ToString().Trim();
            onToken(new MarkupToken(
                MarkupTokenType.CodeBlock,
                "```" + (codeFenceLanguage.Length > 0 ? codeFenceLanguage : ""),
                string.IsNullOrEmpty(codeFenceLanguage) ? null : new CodeBlockMetadata(codeFenceLanguage)
            ));
            sb.Clear();
            inCodeFence = true;
            state = State.InCodeFence;
        }
        else
        {
            sb.Append(c);
        }
    }

    private static void HandleLinkState(char c, ref State state, StringBuilder sb,
                                           Action<MarkupToken> onToken, ref int linkTextDepth,
                                           ref bool imageMode)
    {
        if (c == '[')
        {
            linkTextDepth++;
            sb.Append(c);
        }
        else if (c == ']')
        {
            linkTextDepth--;
            if (linkTextDepth == 0)
            {
                onToken(new MarkupToken(MarkupTokenType.Link, sb.ToString()));
                sb.Clear();
                state = State.InLinkText;
            }
            else
            {
                sb.Append(c);
            }
        }
        else
        {
            sb.Append(c);
        }
    }

    private static void HandleLinkState(char c, ref State state, StringBuilder sb,
                                          Action<MarkupToken> onToken, ref bool imageMode)
    {
        if (c == '(')
        {
            // Start collecting URL
            return;
        }
        else if (c == ')')
        {
            string url = sb.ToString().Trim();
            string? title = null;
            
            // Check for title in quotes
            int quoteIdx = url.IndexOf('"');
            if (quoteIdx >= 0)
            {
                int endQuoteIdx = url.IndexOf('"', quoteIdx + 1);
                if (endQuoteIdx > quoteIdx)
                {
                    title = url.Substring(quoteIdx + 1, endQuoteIdx - quoteIdx - 1);
                    url = url.Substring(0, quoteIdx).Trim();
                }
            }

            onToken(new MarkupToken(MarkupTokenType.Link, url, new LinkMetadata(url, title)));
            if (title != null)
            {
                onToken(new MarkupToken(MarkupTokenType.Link, title));
            }
            sb.Clear();
            imageMode = false;
            state = State.Text;
        }
        else
        {
            sb.Append(c);
        }
    }

    private static void HandleImageState(char c, ref State state, StringBuilder sb,
                                           Action<MarkupToken> onToken, ref int linkTextDepth)
    {
        if (c == ']')
        {
            onToken(new MarkupToken(MarkupTokenType.Image, sb.ToString()));
            sb.Clear();
            state = State.InImageAlt;
        }
        else
        {
            sb.Append(c);
        }
    }

    private static void HandleImageState(char c, ref State state, StringBuilder sb,
                                           Action<MarkupToken> onToken)
    {
        if (c == '(')
        {
            // Start collecting URL
            return;
        }
        else if (c == ')')
        {
            string url = sb.ToString().Trim();
            string? title = null;
            
            // Check for title in quotes
            int quoteIdx = url.IndexOf('"');
            if (quoteIdx >= 0)
            {
                int endQuoteIdx = url.IndexOf('"', quoteIdx + 1);
                if (endQuoteIdx > quoteIdx)
                {
                    title = url.Substring(quoteIdx + 1, endQuoteIdx - quoteIdx - 1);
                    url = url.Substring(0, quoteIdx).Trim();
                }
            }

            onToken(new MarkupToken(MarkupTokenType.Image, url));
            if (title != null)
            {
                onToken(new MarkupToken(MarkupTokenType.Image, title));
            }
            sb.Clear();
            state = State.Text;
        }
        else
        {
            sb.Append(c);
        }
    }

    private static void HandleEmojiState(char c, ref State state, StringBuilder sb,
                                        Action<MarkupToken> onToken)
    {
        if (c == ':')
        {
            string emojiName = sb.ToString();
            onToken(new MarkupToken(
                MarkupTokenType.Emoji,
                ":" + emojiName + ":",
                new EmojiMetadata(emojiName)
            ));
            sb.Clear();
            state = State.Text;
        }
        else if (char.IsLetterOrDigit(c) || c == '_' || c == '-')
        {
            sb.Append(c);
        }
        else
        {
            // Invalid emoji, treat as text
            sb.Insert(0, ':');
            sb.Append(c);
            onToken(new MarkupToken(MarkupTokenType.Text, sb.ToString()));
            sb.Clear();
            state = State.Text;
        }
    }

    private static void HandleBlockquoteState(char c, ref State state, StringBuilder sb,
                                             Action<MarkupToken> onToken, ref bool lineStartPos)
    {
        if (c == '\n')
        {
            // End of blockquote line - emit blockquote token with accumulated content
            string quoteText = sb.ToString().TrimStart(); // Remove leading whitespace after >
            if (quoteText.Length > 0)
            {
                onToken(new MarkupToken(MarkupTokenType.Blockquote, quoteText));
            }
            sb.Clear();
            state = State.LineStart;
        }
        else
        {
            sb.Append(c);
        }
    }

    private static void HandleHtmlTagState(char c, ref State state, StringBuilder sb,
                                          Action<MarkupToken> onToken)
    {
        sb.Append(c);
        if (c == '>')
        {
            onToken(new MarkupToken(MarkupTokenType.HtmlTag, sb.ToString()));
            sb.Clear();
            state = State.Text;
        }
    }

    private static void HandleSubscriptState(char c, ref State state, StringBuilder sb,
                                            Action<MarkupToken> onToken)
    {
        if (c == '^')
        {
            onToken(new MarkupToken(MarkupTokenType.Subscript, sb.ToString()));
            sb.Clear();
            state = State.Text;
        }
        else
        {
            sb.Append(c);
        }
    }

    private static void HandleSuperscriptState(char c, ref State state, StringBuilder sb,
                                              Action<MarkupToken> onToken)
    {
        if (c == '~')
        {
            onToken(new MarkupToken(MarkupTokenType.Superscript, sb.ToString()));
            sb.Clear();
            state = State.Text;
        }
        else
        {
            sb.Append(c);
        }
    }

    private static void HandleInsertedState(char c, ref State state, StringBuilder sb,
                                           Action<MarkupToken> onToken, ref char prevChar)
    {
        if (c == '+' && prevChar == '+')
        {
            if (sb.Length > 0 && sb[sb.Length - 1] == '+')
            {
                sb.Length--;
            }
            onToken(new MarkupToken(MarkupTokenType.InsertedText, sb.ToString()));
            sb.Clear();
            state = State.Text;
        }
        else
        {
            sb.Append(c);
        }
    }

    private static void HandleMarkedState(char c, ref State state, StringBuilder sb,
                                         Action<MarkupToken> onToken, ref char prevChar)
    {
        if (c == '=' && prevChar == '=')
        {
            if (sb.Length > 0 && sb[sb.Length - 1] == '=')
            {
                sb.Length--;
            }
            onToken(new MarkupToken(MarkupTokenType.MarkedText, sb.ToString()));
            sb.Clear();
            state = State.Text;
        }
        else
        {
            sb.Append(c);
        }
    }

    private static void HandleFootnoteRefState(char c, ref State state, StringBuilder sb,
                                              Action<MarkupToken> onToken)
    {
        if (c == ']')
        {
            string id = sb.ToString();
            onToken(new MarkupToken(
                MarkupTokenType.FootnoteReference,
                "[^" + id + "]",
                new FootnoteMetadata(id)
            ));
            sb.Clear();
            state = State.Text;
        }
        else
        {
            sb.Append(c);
        }
    }

    private static void HandleCustomContainerState(char c, ref State state, StringBuilder sb,
                                                   Action<MarkupToken> onToken)
    {
        if (c == '\n')
        {
            // Emit custom container without ::: prefix
            string containerType = sb.ToString().Trim();
            onToken(new MarkupToken(MarkupTokenType.CustomContainer, containerType));
            sb.Clear();
            state = State.LineStart;
        }
        else
        {
            sb.Append(c);
        }
    }

    private static void HandleUnorderedListState(char c, ref State state, StringBuilder sb,
                                                  Action<MarkupToken> onToken)
    {
        if (c == '\n')
        {
            // End of list item - emit token with accumulated content
            string itemText = sb.ToString().TrimStart(); // Remove leading whitespace
            if (itemText.Length > 0)
            {
                onToken(new MarkupToken(MarkupTokenType.UnorderedListItem, itemText));
            }
            sb.Clear();
            state = State.LineStart;
        }
        else
        {
            sb.Append(c);
        }
    }

    private static void HandleOrderedListState(char c, ref State state, StringBuilder sb,
                                               Action<MarkupToken> onToken, ref int listItemNumber)
    {
        if (c == '\n')
        {
            // End of list item - emit token with accumulated content and item number in metadata
            string itemText = sb.ToString().TrimStart(); // Remove leading whitespace
            if (itemText.Length > 0)
            {
                onToken(new MarkupToken(
                    MarkupTokenType.OrderedListItem,
                    itemText,
                    new ListItemMetadata(listItemNumber)
                ));
            }
            sb.Clear();
            listItemNumber = 0;
            state = State.LineStart;
        }
        else
        {
            sb.Append(c);
        }
    }

    private static void HandleTableState(char c, ref State state, StringBuilder sb,
                                         Action<MarkupToken> onToken)
    {
        if (c == '|')
        {
            // End of table cell - emit cell token with accumulated content
            string cellText = sb.ToString().Trim();
            if (cellText.Length > 0)
            {
                onToken(new MarkupToken(MarkupTokenType.TableCell, cellText));
            }
            sb.Clear();
            // Stay in table state for next cell
        }
        else if (c == '\n')
        {
            // End of table row
            if (sb.Length > 0)
            {
                string cellText = sb.ToString().Trim();
                if (cellText.Length > 0)
                {
                    onToken(new MarkupToken(MarkupTokenType.TableCell, cellText));
                }
            }
            sb.Clear();
            state = State.LineStart;
        }
        else
        {
            sb.Append(c);
        }
    }

    private static void EmitPending(StringBuilder sb, State state, Action<MarkupToken> onToken,
                                    int headingLevel, string codeFenceLanguage)
    {
        if (sb.Length > 0)
        {
            switch (state)
            {
                case State.Text:
                case State.LineStart:
                case State.InBlockquote:
                    onToken(new MarkupToken(MarkupTokenType.Text, sb.ToString()));
                    break;
                case State.InCodeInline:
                    onToken(new MarkupToken(MarkupTokenType.CodeInline, sb.ToString()));
                    break;
                case State.InCodeFence:
                    if (!string.IsNullOrEmpty(codeFenceLanguage))
                    {
                        DelegateCodeBlockParsing(sb.ToString(), codeFenceLanguage, onToken);
                    }
                    else
                    {
                        onToken(new MarkupToken(MarkupTokenType.CodeBlock, sb.ToString()));
                    }
                    break;
                case State.InHeading:
                    onToken(new MarkupToken(
                        MarkupTokenType.Heading,
                        new string('#', headingLevel),
                        new HeadingMetadata(headingLevel)
                    ));
                    break;
                case State.InLinkText:
                    onToken(new MarkupToken(MarkupTokenType.Link, sb.ToString()));
                    break;
                case State.InImageAlt:
                    onToken(new MarkupToken(MarkupTokenType.Image, sb.ToString()));
                    break;
                case State.InEmoji:
                    onToken(new MarkupToken(MarkupTokenType.Text, ":" + sb.ToString()));
                    break;
                case State.InHtmlTag:
                    onToken(new MarkupToken(MarkupTokenType.HtmlTag, sb.ToString()));
                    break;
                case State.InCustomContainer:
                    onToken(new MarkupToken(MarkupTokenType.CustomContainer, ":::" + sb.ToString()));
                    break;
            }
            sb.Clear();
        }
    }

    private static void DelegateCodeBlockParsing(string content, string language, Action<MarkupToken> onToken)
    {
        // Delegate to specific tokenizers based on language
        language = language.ToLowerInvariant();
        
        try
        {
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(content));
            
            switch (language)
            {
                case "xml":
                case "html":
                    Xml.XmlTokenizer.Parse(ms, "```", token =>
                    {
                        // Wrap XML tokens as code block content
                        onToken(new MarkupToken(MarkupTokenType.CodeBlock, token.Value));
                    });
                    break;

                case "json":
                    Json.JsonTokenizer.Parse(ms, "```", token =>
                    {
                        // Wrap JSON tokens as code block content
                        onToken(new MarkupToken(MarkupTokenType.CodeBlock, token.Value));
                    });
                    break;

                case "csharp":
                case "cs":
                    CSharp.CSharpTokenizer.Parse(ms, "```", token =>
                    {
                        // Wrap C# tokens as code block content
                        onToken(new MarkupToken(MarkupTokenType.CodeBlock, token.Value));
                    });
                    break;

                case "typescript":
                case "ts":
                case "javascript":
                case "js":
                    Typescript.TypescriptTokenizer.Parse(ms, "```", token =>
                    {
                        // Wrap TypeScript tokens as code block content
                        onToken(new MarkupToken(MarkupTokenType.CodeBlock, token.Value));
                    });
                    break;

                case "sql":
                    Sql.SqlTokenizer.Parse(ms, "```", token =>
                    {
                        // Wrap SQL tokens as code block content
                        onToken(new MarkupToken(MarkupTokenType.CodeBlock, token.Value));
                    });
                    break;

                default:
                    // Unknown language, just emit as plain content
                    onToken(new MarkupToken(MarkupTokenType.CodeBlock, content));
                    break;
            }
        }
        catch
        {
            // If delegation fails, emit as plain content
            onToken(new MarkupToken(MarkupTokenType.CodeBlock, content));
        }
    }
}
