using System.Text;

namespace NTokenizers.Xml;

/// <summary>
/// Provides functionality for tokenizing XML or XML-like text sources.
/// </summary>
public static class XmlTokenizer
{
    private enum State
    {
        Text,              // Reading text content
        TagStart,          // Just saw '<'
        InTagName,         // Reading element name after '<'
        AfterTagName,      // After element name, before '>' or attributes
        InAttributeName,   // Reading attribute name
        AfterAttributeName, // After attribute name, before '='
        AfterEquals,       // After '=', before quote
        InAttributeValue,  // Inside quoted attribute value
        InComment,         // Inside <!--...-->
        InProcessingInstruction, // Inside <?...?>
        InDocType,         // Inside <!DOCTYPE...>
        InCData,           // Inside <![CDATA[...]]>
        InWhitespace       // Collecting whitespace between structural tokens
    }

    /// <summary>
    /// Parses XML or XML-like content from the given <see cref="Stream"/> and
    /// produces a sequence of <see cref="XmlToken"/> objects.
    /// </summary>
    /// <param name="stream">
    /// The input stream containing the text to tokenize.  
    /// The stream is read as UTF-8.
    /// </param>
    /// <param name="onToken">
    /// A callback invoked for each <see cref="XmlToken"/> produced during parsing.
    /// This delegate must not be <c>null</c>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="stream"/> or <paramref name="onToken"/> is <c>null</c>.
    /// </exception>
    /// <example>
    /// The following example demonstrates how to parse a simple XML snippet:
    /// <code>
    /// var xml = "&lt;root&gt;Hello&lt;/root&gt;";
    /// using var ms = new MemoryStream(Encoding.UTF8.GetBytes(xml));
    ///
    /// XmlTokenizer.Parse(ms, token =>
    /// {
    ///     Console.WriteLine($"{token.Kind}: {token.Value}");
    /// });
    /// </code>
    /// </example>
    public static void Parse(Stream stream, Action<XmlToken> onToken)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8);
        Parse(reader, null, onToken);
    }

    /// <summary>
    /// Parses XML or XML-like content from the given <see cref="TextReader"/> and
    /// produces a sequence of <see cref="XmlToken"/> objects.
    /// </summary>
    /// <param name="reader">
    /// The input reader containing the text to tokenize.
    /// </param>
    /// <param name="stopDelimiter">
    /// An optional string that, when encountered in the input, instructs the tokenizer
    /// to stop parsing and return control to the caller.  
    /// This is typically used when the tokenizer is operating as a sub-tokenizer
    /// inside another language (e.g., XML embedded in Markdown), where parsing should
    /// stop when reaching a delimiter such as a Markdown code fence (<c>```</c>).
    /// If <c>null</c>, the tokenizer parses until the end of the stream.
    /// </param>
    /// <param name="onToken">
    /// A callback invoked for each <see cref="XmlToken"/> produced during parsing.
    /// This delegate must not be <c>null</c>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="reader"/> or <paramref name="onToken"/> is <c>null</c>.
    /// </exception>
    /// <example>
    /// The following example demonstrates how to parse a simple XML snippet:
    /// <code>
    /// var xml = "&lt;root&gt;Hello&lt;/root&gt;";
    /// using var sr = new StringReader(xml);
    ///
    /// XmlTokenizer.Parse(sr, null, token =>
    /// {
    ///     Console.WriteLine($"{token.Kind}: {token.Value}");
    /// });
    /// </code>
    ///
    /// The next example shows parsing XML that is embedded inside Markdown.
    /// The tokenizer stops when the code fence delimiter is reached:
    /// <code>
    /// var markdown = "```xml\n&lt;root&gt;Hi&lt;/root&gt;\n``` more text";
    ///
    /// using var sr = new StringReader(markdown);
    ///
    /// // We want to parse only until the closing code fence appears.
    /// XmlTokenizer.Parse(sr, "```", token =>
    /// {
    ///     Console.WriteLine($"{token.Kind}: {token.Value}");
    /// });
    /// </code>
    /// </example>
    public static void Parse(TextReader reader, string? stopDelimiter, Action<XmlToken> onToken)
    {
        var sb = new StringBuilder();
        var state = State.Text;
        string delimiter = stopDelimiter ?? string.Empty;
        int delLength = delimiter.Length;
        char? quoteChar = null;
        bool insideTag = false;
        bool seenElementName = false;
        int depth = 0; // Track element nesting depth
        bool isClosingTag = false; // Track if current tag is a closing tag </...>

        if (delLength == 0)
        {
            while (true)
            {
                int ic = reader.Read();
                if (ic == -1)
                {
                    EmitPending(sb, state, onToken);
                    break;
                }
                char c = (char)ic;
                ProcessChar(c, ref state, sb, onToken, ref quoteChar, ref insideTag, ref seenElementName, ref depth, ref isClosingTag);
            }
        }
        else
        {
            var delQueue = new Queue<char>();
            bool stoppedByDelimiter = false;

            while (true)
            {
                int ic = reader.Read();
                if (ic == -1)
                {
                    break;
                }
                char c = (char)ic;
                delQueue.Enqueue(c);

                if (delQueue.Count > delLength)
                {
                    char toProcess = delQueue.Dequeue();
                    ProcessChar(toProcess, ref state, sb, onToken, ref quoteChar, ref insideTag, ref seenElementName, ref depth, ref isClosingTag);
                }

                if (delQueue.Count == delLength && new string(delQueue.ToArray()) == delimiter)
                {
                    stoppedByDelimiter = true;
                    break;
                }
            }

            if (!stoppedByDelimiter)
            {
                while (delQueue.Count > 0)
                {
                    char toProcess = delQueue.Dequeue();
                    ProcessChar(toProcess, ref state, sb, onToken, ref quoteChar, ref insideTag, ref seenElementName, ref depth, ref isClosingTag);
                }
            }

            EmitPending(sb, state, onToken);
        }
    }

    private static void ProcessChar(char c, ref State state, StringBuilder sb, Action<XmlToken> onToken, 
                                    ref char? quoteChar, ref bool insideTag, ref bool seenElementName, ref int depth, ref bool isClosingTag)
    {
        switch (state)
        {
            case State.Text:
                if (c == '<')
                {
                    if (sb.Length > 0)
                    {
                        onToken(new XmlToken(XmlTokenType.Text, sb.ToString()));
                        sb.Clear();
                    }
                    sb.Append(c);
                    state = State.TagStart;
                }
                else if (IsWhitespace(c) && sb.Length == 0 && depth == 0)
                {
                    // Whitespace at document level (between elements, not inside)
                    sb.Append(c);
                    state = State.InWhitespace;
                }
                else
                {
                    sb.Append(c);
                }
                break;

            case State.TagStart:
                sb.Append(c);
                if (sb.ToString() == "<?")
                {
                    state = State.InProcessingInstruction;
                }
                else if (sb.ToString() == "<!-")
                {
                    // Continue reading for comment
                }
                else if (sb.ToString() == "<!--")
                {
                    state = State.InComment;
                }
                else if (sb.ToString().StartsWith("<!["))
                {
                    if (sb.ToString() == "<![CDATA[")
                    {
                        state = State.InCData;
                    }
                    // else continue reading
                }
                else if (sb.Length > 2 && sb[1] == '!' && char.IsLetter(sb[2]))
                {
                    // DOCTYPE
                    state = State.InDocType;
                }
                else if (sb.Length == 2)
                {
                    if (c == '/')
                    {
                        // End tag: </
                        onToken(new XmlToken(XmlTokenType.OpeningAngleBracket, "<"));
                        onToken(new XmlToken(XmlTokenType.SelfClosingSlash, "/"));
                        sb.Clear();
                        insideTag = true;
                        seenElementName = false;
                        isClosingTag = true;
                        state = State.InTagName;
                    }
                    else if (IsWhitespace(c))
                    {
                        // < followed by whitespace
                        onToken(new XmlToken(XmlTokenType.OpeningAngleBracket, "<"));
                        sb.Remove(0, 1); // remove the '<'
                        insideTag = true;
                        seenElementName = false;
                        isClosingTag = false;
                        state = State.InWhitespace;
                    }
                    else if (IsNameStartChar(c))
                    {
                        // Regular opening tag: <name
                        onToken(new XmlToken(XmlTokenType.OpeningAngleBracket, "<"));
                        sb.Remove(0, 1); // remove the '<'
                        insideTag = true;
                        seenElementName = false;
                        isClosingTag = false;
                        state = State.InTagName;
                    }
                }
                break;

            case State.InTagName:
                if (IsNameChar(c))
                {
                    sb.Append(c);
                }
                else
                {
                    if (sb.Length > 0)
                    {
                        onToken(new XmlToken(XmlTokenType.ElementName, sb.ToString()));
                        seenElementName = true;
                        sb.Clear();
                    }
                    
                    if (c == '>')
                    {
                        onToken(new XmlToken(XmlTokenType.ClosingAngleBracket, ">"));
                        insideTag = false;
                        seenElementName = false;
                        if (isClosingTag)
                        {
                            depth--;
                            isClosingTag = false;
                        }
                        else
                        {
                            // Opening tag - increase depth
                            depth++;
                        }
                        state = State.Text;
                    }
                    else if (c == '/')
                    {
                        onToken(new XmlToken(XmlTokenType.SelfClosingSlash, "/"));
                        state = State.AfterTagName;
                    }
                    else if (IsWhitespace(c))
                    {
                        sb.Append(c);
                        state = State.InWhitespace;
                    }
                }
                break;

            case State.InWhitespace:
                if (IsWhitespace(c))
                {
                    sb.Append(c);
                }
                else
                {
                    if (sb.Length > 0)
                    {
                        onToken(new XmlToken(XmlTokenType.Whitespace, sb.ToString()));
                        sb.Clear();
                    }

                    if (c == '<')
                    {
                        sb.Append(c);
                        state = State.TagStart;
                    }
                    else if (c == '>')
                    {
                        onToken(new XmlToken(XmlTokenType.ClosingAngleBracket, ">"));
                        insideTag = false;
                        seenElementName = false;
                        if (isClosingTag)
                        {
                            depth--;
                            isClosingTag = false;
                        }
                        else
                        {
                            depth++;
                        }
                        state = State.Text;
                    }
                    else if (c == '/')
                    {
                        onToken(new XmlToken(XmlTokenType.SelfClosingSlash, "/"));
                        state = State.AfterTagName;
                    }
                    else if (c == '=')
                    {
                        onToken(new XmlToken(XmlTokenType.AttributeEquals, "="));
                        state = State.AfterEquals;
                    }
                    else if (c == '"' || c == '\'')
                    {
                        quoteChar = c;
                        onToken(new XmlToken(XmlTokenType.AttributeQuote, c.ToString()));
                        state = State.InAttributeValue;
                    }
                    else if (IsNameStartChar(c))
                    {
                        sb.Append(c);
                        if (!seenElementName && insideTag)
                        {
                            state = State.InTagName;
                        }
                        else
                        {
                            state = State.InAttributeName;
                        }
                    }
                }
                break;

            case State.AfterTagName:
                if (c == '>')
                {
                    onToken(new XmlToken(XmlTokenType.ClosingAngleBracket, ">"));
                    insideTag = false;
                    seenElementName = false;
                    // Self-closing tag or closing tag after attributes - don't change depth for self-closing
                    if (isClosingTag)
                    {
                        depth--;
                        isClosingTag = false;
                    }
                    // Note: for self-closing tags (<element/>), we don't change depth
                    state = State.Text;
                }
                else if (c == '/')
                {
                    onToken(new XmlToken(XmlTokenType.SelfClosingSlash, "/"));
                    // Stay in AfterTagName to handle the closing '>'
                }
                else if (IsWhitespace(c))
                {
                    sb.Append(c);
                    state = State.InWhitespace;
                }
                break;

            case State.InAttributeName:
                if (IsNameChar(c))
                {
                    sb.Append(c);
                }
                else
                {
                    if (sb.Length > 0)
                    {
                        onToken(new XmlToken(XmlTokenType.AttributeName, sb.ToString()));
                        sb.Clear();
                    }

                    if (c == '=')
                    {
                        onToken(new XmlToken(XmlTokenType.AttributeEquals, "="));
                        state = State.AfterEquals;
                    }
                    else if (IsWhitespace(c))
                    {
                        sb.Append(c);
                        state = State.InWhitespace;
                    }
                }
                break;

            case State.AfterEquals:
                if (c == '"' || c == '\'')
                {
                    quoteChar = c;
                    onToken(new XmlToken(XmlTokenType.AttributeQuote, c.ToString()));
                    state = State.InAttributeValue;
                }
                else if (IsWhitespace(c))
                {
                    sb.Append(c);
                    state = State.InWhitespace;
                }
                break;

            case State.InAttributeValue:
                if (c == quoteChar)
                {
                    onToken(new XmlToken(XmlTokenType.AttributeValue, sb.ToString()));
                    sb.Clear();
                    onToken(new XmlToken(XmlTokenType.AttributeQuote, c.ToString()));
                    quoteChar = null;
                    state = State.AfterTagName;
                }
                else
                {
                    sb.Append(c);
                }
                break;

            case State.InComment:
                sb.Append(c);
                if (sb.ToString().EndsWith("-->"))
                {
                    onToken(new XmlToken(XmlTokenType.Comment, sb.ToString()));
                    sb.Clear();
                    state = State.Text;
                }
                break;

            case State.InProcessingInstruction:
                sb.Append(c);
                if (sb.ToString().EndsWith("?>"))
                {
                    onToken(new XmlToken(XmlTokenType.ProcessingInstruction, sb.ToString()));
                    sb.Clear();
                    state = State.Text;
                }
                break;

            case State.InDocType:
                sb.Append(c);
                if (c == '>')
                {
                    onToken(new XmlToken(XmlTokenType.DocumentTypeDeclaration, sb.ToString()));
                    sb.Clear();
                    state = State.Text;
                }
                break;

            case State.InCData:
                sb.Append(c);
                if (sb.ToString().EndsWith("]]>"))
                {
                    onToken(new XmlToken(XmlTokenType.CData, sb.ToString()));
                    sb.Clear();
                    state = State.Text;
                }
                break;
        }
    }

    private static void EmitPending(StringBuilder sb, State state, Action<XmlToken> onToken)
    {
        if (sb.Length > 0)
        {
            switch (state)
            {
                case State.Text:
                    onToken(new XmlToken(XmlTokenType.Text, sb.ToString()));
                    break;
                case State.InWhitespace:
                    onToken(new XmlToken(XmlTokenType.Whitespace, sb.ToString()));
                    break;
                case State.InTagName:
                    onToken(new XmlToken(XmlTokenType.ElementName, sb.ToString()));
                    break;
                case State.InAttributeName:
                    onToken(new XmlToken(XmlTokenType.AttributeName, sb.ToString()));
                    break;
            }
            sb.Clear();
        }
    }

    private static bool IsWhitespace(char c)
    {
        return c == ' ' || c == '\t' || c == '\n' || c == '\r';
    }

    private static bool IsNameStartChar(char c)
    {
        return char.IsLetter(c) || c == '_' || c == ':';
    }

    private static bool IsNameChar(char c)
    {
        return IsNameStartChar(c) || char.IsDigit(c) || c == '-' || c == '.';
    }
}