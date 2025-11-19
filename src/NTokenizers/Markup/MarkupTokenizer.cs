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
        // Read all content into a string for easier lookahead
        string content = reader.ReadToEnd();
        
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
        int pos = 0;

        while (pos < content.Length)
        {
            char c = content[pos];
            
            // Track if we're at the start of a line
            if (prevChar == '\n' || prevChar == '\0')
            {
                lineStartPos = true;
            }

            ProcessChar(c, ref state, sb, onToken, ref prevChar, ref lineStartPos, 
                       ref headingLevel, ref codeFenceLanguage, ref inCodeFence,
                       ref codeFenceDelimiter, ref linkTextDepth, ref imageMode, 
                       content, ref pos);

            prevChar = c;
            pos++;
            
            if (c == '\n')
            {
                lineStartPos = true;
            }
            else if (!char.IsWhiteSpace(c))
            {
                lineStartPos = false;
            }
        }
        
        EmitPending(sb, state, onToken, headingLevel, codeFenceLanguage);
    }

    private static void ProcessChar(char c, ref State state, StringBuilder sb, 
                                    Action<MarkupToken> onToken, ref char prevChar,
                                    ref bool lineStartPos, ref int headingLevel,
                                    ref string codeFenceLanguage, ref bool inCodeFence,
                                    ref string codeFenceDelimiter, ref int linkTextDepth,
                                    ref bool imageMode, string content, ref int pos)
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
                                    ref codeFenceLanguage, content, ref pos);
                break;

            case State.InHeading:
                HandleHeadingState(c, ref state, sb, onToken, ref headingLevel);
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
                HandleLinkTextState(c, ref state, sb, onToken, ref linkTextDepth, ref imageMode);
                break;

            case State.InLinkUrl:
                HandleLinkUrlState(c, ref state, sb, onToken, ref imageMode);
                break;

            case State.InImageAlt:
                HandleImageAltState(c, ref state, sb, onToken, ref linkTextDepth);
                break;

            case State.InImageUrl:
                HandleImageUrlState(c, ref state, sb, onToken);
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
            onToken(new MarkupToken(MarkupTokenType.BoldDelimiter, "**"));
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
                                            string content, ref int pos)
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
            onToken(new MarkupToken(MarkupTokenType.BlockquoteDelimiter, ">"));
            state = State.InBlockquote;
            return;
        }

        // Check for code fence ```
        if (c == '`')
        {
            sb.Append(c);
            // Check for ``` pattern
            if (pos + 2 < content.Length && content[pos + 1] == '`' && content[pos + 2] == '`')
            {
                pos += 2; // skip next two `
                if (inCodeFence)
                {
                    // End of code fence
                    onToken(new MarkupToken(MarkupTokenType.CodeBlockFenceEnd, "```"));
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
            if (pos + 2 < content.Length && content[pos + 1] == c && content[pos + 2] == c)
            {
                pos += 2; // skip next two chars
                onToken(new MarkupToken(MarkupTokenType.HorizontalRule, new string(c, 3)));
                sb.Clear();
                state = State.Text;
                return;
            }
            // Check if it's a list item (- or * or + followed by space)
            else if (pos + 1 < content.Length && char.IsWhiteSpace(content[pos + 1]))
            {
                onToken(new MarkupToken(MarkupTokenType.UnorderedListDelimiter, c.ToString()));
                sb.Clear();
                state = State.Text;
                return;
            }
            else
            {
                state = State.Text;
                return;
            }
        }

        // Check for unordered list + (separate check for +)
        if (c == '+' && pos + 1 < content.Length && char.IsWhiteSpace(content[pos + 1]))
        {
            onToken(new MarkupToken(MarkupTokenType.UnorderedListDelimiter, c.ToString()));
            state = State.Text;
            return;
        }

        // Check for ordered list 1., 2., etc.
        if (char.IsDigit(c))
        {
            sb.Append(c);
            int digitCount = 1;
            int checkPos = pos + 1;
            while (checkPos < content.Length && char.IsDigit(content[checkPos]) && digitCount < 10)
            {
                sb.Append(content[checkPos]);
                checkPos++;
                digitCount++;
            }
            if (checkPos < content.Length && content[checkPos] == '.' && 
                checkPos + 1 < content.Length && char.IsWhiteSpace(content[checkPos + 1]))
            {
                sb.Append('.');
                onToken(new MarkupToken(MarkupTokenType.OrderedListDelimiter, sb.ToString()));
                sb.Clear();
                pos = checkPos; // advance position
                state = State.Text;
                return;
            }
            else
            {
                state = State.Text;
                return;
            }
        }

        // Check for custom container :::
        if (c == ':')
        {
            if (pos + 2 < content.Length && content[pos + 1] == ':' && content[pos + 2] == ':')
            {
                pos += 2; // skip next two :
                state = State.InCustomContainer;
                return;
            }
        }

        // Check for table |
        if (c == '|')
        {
            onToken(new MarkupToken(MarkupTokenType.TableDelimiter, "|"));
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
        if (c == '#' && headingLevel < 6)
        {
            headingLevel++;
        }
        else if (char.IsWhiteSpace(c) || c == '\n')
        {
            onToken(new MarkupToken(
                MarkupTokenType.HeadingDelimiter,
                new string('#', headingLevel),
                new HeadingMetadata(headingLevel)
            ));
            headingLevel = 0;
            state = c == '\n' ? State.LineStart : State.Text;
            if (c != '\n')
            {
                sb.Append(c);
            }
        }
        else
        {
            // Not a valid heading, treat as text
            sb.Append(new string('#', headingLevel));
            sb.Append(c);
            headingLevel = 0;
            state = State.Text;
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
                        onToken(new MarkupToken(MarkupTokenType.CodeBlockContent, content));
                    }
                }
                onToken(new MarkupToken(MarkupTokenType.CodeBlockFenceEnd, "```"));
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
                MarkupTokenType.CodeBlockFenceStart,
                "```" + (codeFenceLanguage.Length > 0 ? codeFenceLanguage : ""),
                string.IsNullOrEmpty(codeFenceLanguage) ? null : new CodeFenceMetadata(codeFenceLanguage)
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

    private static void HandleLinkTextState(char c, ref State state, StringBuilder sb,
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
                onToken(new MarkupToken(MarkupTokenType.LinkText, sb.ToString()));
                sb.Clear();
                state = State.InLinkUrl;
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

    private static void HandleLinkUrlState(char c, ref State state, StringBuilder sb,
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

            onToken(new MarkupToken(MarkupTokenType.LinkUrl, url, new LinkMetadata(url, title)));
            if (title != null)
            {
                onToken(new MarkupToken(MarkupTokenType.LinkTitle, title));
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

    private static void HandleImageAltState(char c, ref State state, StringBuilder sb,
                                           Action<MarkupToken> onToken, ref int linkTextDepth)
    {
        if (c == ']')
        {
            onToken(new MarkupToken(MarkupTokenType.ImageAlt, sb.ToString()));
            sb.Clear();
            state = State.InImageUrl;
        }
        else
        {
            sb.Append(c);
        }
    }

    private static void HandleImageUrlState(char c, ref State state, StringBuilder sb,
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

            onToken(new MarkupToken(MarkupTokenType.ImageUrl, url));
            if (title != null)
            {
                onToken(new MarkupToken(MarkupTokenType.ImageTitle, title));
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
            if (sb.Length > 0)
            {
                onToken(new MarkupToken(MarkupTokenType.Text, sb.ToString()));
                sb.Clear();
            }
            onToken(new MarkupToken(MarkupTokenType.Text, "\n"));
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
            onToken(new MarkupToken(MarkupTokenType.CustomContainer, ":::" + sb.ToString()));
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
                        onToken(new MarkupToken(MarkupTokenType.CodeBlockContent, sb.ToString()));
                    }
                    break;
                case State.InHeading:
                    onToken(new MarkupToken(
                        MarkupTokenType.HeadingDelimiter,
                        new string('#', headingLevel),
                        new HeadingMetadata(headingLevel)
                    ));
                    break;
                case State.InLinkText:
                    onToken(new MarkupToken(MarkupTokenType.LinkText, sb.ToString()));
                    break;
                case State.InImageAlt:
                    onToken(new MarkupToken(MarkupTokenType.ImageAlt, sb.ToString()));
                    break;
                case State.InEmoji:
                    onToken(new MarkupToken(MarkupTokenType.Text, ":" + sb.ToString()));
                    break;
                case State.InHtmlTag:
                    onToken(new MarkupToken(MarkupTokenType.HtmlTag, sb.ToString()));
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
                        onToken(new MarkupToken(MarkupTokenType.CodeBlockContent, token.Value));
                    });
                    break;

                case "json":
                    Json.JsonTokenizer.Parse(ms, "```", token =>
                    {
                        // Wrap JSON tokens as code block content
                        onToken(new MarkupToken(MarkupTokenType.CodeBlockContent, token.Value));
                    });
                    break;

                case "csharp":
                case "cs":
                    CSharp.CSharpTokenizer.Parse(ms, "```", token =>
                    {
                        // Wrap C# tokens as code block content
                        onToken(new MarkupToken(MarkupTokenType.CodeBlockContent, token.Value));
                    });
                    break;

                case "typescript":
                case "ts":
                case "javascript":
                case "js":
                    Typescript.TypescriptTokenizer.Parse(ms, "```", token =>
                    {
                        // Wrap TypeScript tokens as code block content
                        onToken(new MarkupToken(MarkupTokenType.CodeBlockContent, token.Value));
                    });
                    break;

                case "sql":
                    Sql.SqlTokenizer.Parse(ms, "```", token =>
                    {
                        // Wrap SQL tokens as code block content
                        onToken(new MarkupToken(MarkupTokenType.CodeBlockContent, token.Value));
                    });
                    break;

                default:
                    // Unknown language, just emit as plain content
                    onToken(new MarkupToken(MarkupTokenType.CodeBlockContent, content));
                    break;
            }
        }
        catch
        {
            // If delegation fails, emit as plain content
            onToken(new MarkupToken(MarkupTokenType.CodeBlockContent, content));
        }
    }
}
