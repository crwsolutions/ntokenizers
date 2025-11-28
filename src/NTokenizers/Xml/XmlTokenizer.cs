using NTokenizers.Core;

namespace NTokenizers.Xml;

/// <summary>
/// Provides functionality for tokenizing XML or XML-like text sources.
/// </summary>
public sealed class XmlTokenizer : BaseSubTokenizer<XmlToken>
{
    /// <summary>
    /// Creates a new instance of the <see cref="XmlTokenizer"/> class.
    /// </summary>
    public static XmlTokenizer Create() => new();

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
    /// Parses XML or XML-like content from the given <see cref="TextReader"/> and
    /// produces a sequence of <see cref="XmlToken"/> objects.
    /// </summary>
    internal protected override void Parse()
    {
        var state = State.Text;
        string delimiter = _stopDelimiter ?? string.Empty;
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
                int ic = _reader.Read();
                if (ic == -1)
                {
                    EmitPending(state);
                    break;
                }
                char c = (char)ic;
                ProcessChar(c, ref state, ref quoteChar, ref insideTag, ref seenElementName, ref depth, ref isClosingTag);
            }
        }
        else
        {
            var delQueue = new Queue<char>();
            bool stoppedByDelimiter = false;

            while (true)
            {
                int ic = _reader.Read();
                if (ic == -1)
                {
                    break;
                }
                char c = (char)ic;
                delQueue.Enqueue(c);

                if (delQueue.Count > delLength)
                {
                    char toProcess = delQueue.Dequeue();
                    ProcessChar(toProcess, ref state, ref quoteChar, ref insideTag, ref seenElementName, ref depth, ref isClosingTag);
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
                    ProcessChar(toProcess, ref state, ref quoteChar, ref insideTag, ref seenElementName, ref depth, ref isClosingTag);
                }
            }

            if (stoppedByDelimiter)
            {
                StripFinalLineFeed();
            }

            EmitPending(state);
        }
    }

    private void ProcessChar(char c, ref State state, ref char? quoteChar, ref bool insideTag,
                                    ref bool seenElementName, ref int depth, ref bool isClosingTag)
    {
        switch (state)
        {
            case State.Text:
                if (c == '<')
                {
                    if (_sb.Length > 0)
                    {
                        _onToken(new XmlToken(XmlTokenType.Text, _sb.ToString()));
                        _sb.Clear();
                    }
                    _sb.Append(c);
                    state = State.TagStart;
                }
                else if (IsWhitespace(c) && _sb.Length == 0 && depth == 0)
                {
                    // Whitespace at document level (between elements, not inside)
                    _sb.Append(c);
                    state = State.InWhitespace;
                }
                else
                {
                    _sb.Append(c);
                }
                break;

            case State.TagStart:
                _sb.Append(c);
                if (_sb.ToString() == "<?")
                {
                    state = State.InProcessingInstruction;
                }
                else if (_sb.ToString() == "<!-")
                {
                    // Continue reading for comment
                }
                else if (_sb.ToString() == "<!--")
                {
                    state = State.InComment;
                }
                else if (_sb.ToString().StartsWith("<!["))
                {
                    if (_sb.ToString() == "<![CDATA[")
                    {
                        state = State.InCData;
                    }
                    // else continue reading
                }
                else if (_sb.Length > 2 && _sb[1] == '!' && char.IsLetter(_sb[2]))
                {
                    // DOCTYPE
                    state = State.InDocType;
                }
                else if (_sb.Length == 2)
                {
                    if (c == '/')
                    {
                        // End tag: </
                        _onToken(new XmlToken(XmlTokenType.OpeningAngleBracket, "<"));
                        _onToken(new XmlToken(XmlTokenType.SelfClosingSlash, "/"));
                        _sb.Clear();
                        insideTag = true;
                        seenElementName = false;
                        isClosingTag = true;
                        state = State.InTagName;
                    }
                    else if (IsWhitespace(c))
                    {
                        // < followed by whitespace
                        _onToken(new XmlToken(XmlTokenType.OpeningAngleBracket, "<"));
                        _sb.Remove(0, 1); // remove the '<'
                        insideTag = true;
                        seenElementName = false;
                        isClosingTag = false;
                        state = State.InWhitespace;
                    }
                    else if (IsNameStartChar(c))
                    {
                        // Regular opening tag: <name
                        _onToken(new XmlToken(XmlTokenType.OpeningAngleBracket, "<"));
                        _sb.Remove(0, 1); // remove the '<'
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
                    _sb.Append(c);
                }
                else
                {
                    if (_sb.Length > 0)
                    {
                        _onToken(new XmlToken(XmlTokenType.ElementName, _sb.ToString()));
                        seenElementName = true;
                        _sb.Clear();
                    }

                    if (c == '>')
                    {
                        _onToken(new XmlToken(XmlTokenType.ClosingAngleBracket, ">"));
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
                        _onToken(new XmlToken(XmlTokenType.SelfClosingSlash, "/"));
                        state = State.AfterTagName;
                    }
                    else if (IsWhitespace(c))
                    {
                        _sb.Append(c);
                        state = State.InWhitespace;
                    }
                }
                break;

            case State.InWhitespace:
                if (IsWhitespace(c))
                {
                    _sb.Append(c);
                }
                else
                {
                    if (_sb.Length > 0)
                    {
                        _onToken(new XmlToken(XmlTokenType.Whitespace, _sb.ToString()));
                        _sb.Clear();
                    }

                    if (c == '<')
                    {
                        _sb.Append(c);
                        state = State.TagStart;
                    }
                    else if (c == '>')
                    {
                        _onToken(new XmlToken(XmlTokenType.ClosingAngleBracket, ">"));
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
                        _onToken(new XmlToken(XmlTokenType.SelfClosingSlash, "/"));
                        state = State.AfterTagName;
                    }
                    else if (c == '=')
                    {
                        _onToken(new XmlToken(XmlTokenType.AttributeEquals, "="));
                        state = State.AfterEquals;
                    }
                    else if (c == '"' || c == '\'')
                    {
                        quoteChar = c;
                        _onToken(new XmlToken(XmlTokenType.AttributeQuote, c.ToString()));
                        state = State.InAttributeValue;
                    }
                    else if (IsNameStartChar(c))
                    {
                        _sb.Append(c);
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
                    _onToken(new XmlToken(XmlTokenType.ClosingAngleBracket, ">"));
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
                    _onToken(new XmlToken(XmlTokenType.SelfClosingSlash, "/"));
                    // Stay in AfterTagName to handle the closing '>'
                }
                else if (IsWhitespace(c))
                {
                    _sb.Append(c);
                    state = State.InWhitespace;
                }
                break;

            case State.InAttributeName:
                if (IsNameChar(c))
                {
                    _sb.Append(c);
                }
                else
                {
                    if (_sb.Length > 0)
                    {
                        _onToken(new XmlToken(XmlTokenType.AttributeName, _sb.ToString()));
                        _sb.Clear();
                    }

                    if (c == '=')
                    {
                        _onToken(new XmlToken(XmlTokenType.AttributeEquals, "="));
                        state = State.AfterEquals;
                    }
                    else if (IsWhitespace(c))
                    {
                        _sb.Append(c);
                        state = State.InWhitespace;
                    }
                }
                break;

            case State.AfterEquals:
                if (c == '"' || c == '\'')
                {
                    quoteChar = c;
                    _onToken(new XmlToken(XmlTokenType.AttributeQuote, c.ToString()));
                    state = State.InAttributeValue;
                }
                else if (IsWhitespace(c))
                {
                    _sb.Append(c);
                    state = State.InWhitespace;
                }
                break;

            case State.InAttributeValue:
                if (c == quoteChar)
                {
                    _onToken(new XmlToken(XmlTokenType.AttributeValue, _sb.ToString()));
                    _sb.Clear();
                    _onToken(new XmlToken(XmlTokenType.AttributeQuote, c.ToString()));
                    quoteChar = null;
                    state = State.AfterTagName;
                }
                else
                {
                    _sb.Append(c);
                }
                break;

            case State.InComment:
                _sb.Append(c);
                if (_sb.ToString().EndsWith("-->"))
                {
                    _onToken(new XmlToken(XmlTokenType.Comment, _sb.ToString()));
                    _sb.Clear();
                    state = State.Text;
                }
                break;

            case State.InProcessingInstruction:
                _sb.Append(c);
                if (_sb.ToString().EndsWith("?>"))
                {
                    _onToken(new XmlToken(XmlTokenType.ProcessingInstruction, _sb.ToString()));
                    _sb.Clear();
                    state = State.Text;
                }
                break;

            case State.InDocType:
                _sb.Append(c);
                if (c == '>')
                {
                    _onToken(new XmlToken(XmlTokenType.DocumentTypeDeclaration, _sb.ToString()));
                    _sb.Clear();
                    state = State.Text;
                }
                break;

            case State.InCData:
                _sb.Append(c);
                if (_sb.ToString().EndsWith("]]>"))
                {
                    _onToken(new XmlToken(XmlTokenType.CData, _sb.ToString()));
                    _sb.Clear();
                    state = State.Text;
                }
                break;
        }
    }

    private void EmitPending(State state)
    {
        if (_sb.Length > 0)
        {
            switch (state)
            {
                case State.Text:
                    _onToken(new XmlToken(XmlTokenType.Text, _sb.ToString()));
                    break;
                case State.InWhitespace:
                    _onToken(new XmlToken(XmlTokenType.Whitespace, _sb.ToString()));
                    break;
                case State.InTagName:
                    _onToken(new XmlToken(XmlTokenType.ElementName, _sb.ToString()));
                    break;
                case State.InAttributeName:
                    _onToken(new XmlToken(XmlTokenType.AttributeName, _sb.ToString()));
                    break;
            }
            _sb.Clear();
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