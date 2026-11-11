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

    private enum ParseState
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

    private sealed class State
    {
        public ParseState Current = ParseState.Text;
        public char? QuoteChar;
        public bool InsideTag;
        public bool SeenElementName;
        public int Depth;
        public bool IsClosingTag;
    }

    /// <summary>
    /// Parses XML or XML-like content from the given <see cref="TextReader"/> and
    /// produces a sequence of <see cref="XmlToken"/> objects.
    /// </summary>
    internal protected override async Task ParseAsync(CancellationToken ct)
    {
        var state = new State();

        await TokenizeCharactersAsync(ct, async (c) => await ProcessChar(c, state));

        await EmitPending(state);
    }

    private async Task ProcessChar(char c, State state)
    {
        switch (state.Current)
        {
            case ParseState.Text:
                if (c == '<')
                {
                    if (_buffer.Length > 0)
                    {
                        await _onToken(new XmlToken(XmlTokenType.Text, _buffer.ToString()));
                        _buffer.Clear();
                    }
                    _buffer.Append(c);
                    state.Current = ParseState.TagStart;
                }
                else if (IsWhitespace(c) && _buffer.Length == 0 && state.Depth == 0)
                {
                    // Whitespace at document level (between elements, not inside)
                    _buffer.Append(c);
                    state.Current = ParseState.InWhitespace;
                }
                else
                {
                    _buffer.Append(c);
                }
                break;

            case ParseState.TagStart:
                _buffer.Append(c);
                if (_buffer.ToString() == "<?")
                {
                    state.Current = ParseState.InProcessingInstruction;
                }
                else if (_buffer.ToString() == "<!-")
                {
                    // Continue reading for comment
                }
                else if (_buffer.ToString() == "<!--")
                {
                    state.Current = ParseState.InComment;
                }
                else if (_buffer.ToString().StartsWith("<!["))
                {
                    if (_buffer.ToString() == "<![CDATA[")
                    {
                        state.Current = ParseState.InCData;
                    }
                    // else continue reading
                }
                else if (_buffer.Length > 2 && _buffer[1] == '!' && char.IsLetter(_buffer[2]))
                {
                    // DOCTYPE
                    state.Current = ParseState.InDocType;
                }
                else if (_buffer.Length == 2)
                {
                    if (c == '/')
                    {
                        // End tag: </
                        await _onToken(new XmlToken(XmlTokenType.OpeningAngleBracket, "<"));
                        await _onToken(new XmlToken(XmlTokenType.SelfClosingSlash, "/"));
                        _buffer.Clear();
                        state.InsideTag = true;
                        state.SeenElementName = false;
                        state.IsClosingTag = true;
                        state.Current = ParseState.InTagName;
                    }
                    else if (IsWhitespace(c))
                    {
                        // < followed by whitespace
                        await _onToken(new XmlToken(XmlTokenType.OpeningAngleBracket, "<"));
                        _buffer.Remove(0, 1); // remove the '<'
                        state.InsideTag = true;
                        state.SeenElementName = false;
                        state.IsClosingTag = false;
                        state.Current = ParseState.InWhitespace;
                    }
                    else if (IsNameStartChar(c))
                    {
                        // Regular opening tag: <name
                        await _onToken(new XmlToken(XmlTokenType.OpeningAngleBracket, "<"));
                        _buffer.Remove(0, 1); // remove the '<'
                        state.InsideTag = true;
                        state.SeenElementName = false;
                        state.IsClosingTag = false;
                        state.Current = ParseState.InTagName;
                    }
                }
                break;

            case ParseState.InTagName:
                if (IsNameChar(c))
                {
                    _buffer.Append(c);
                }
                else
                {
                    if (_buffer.Length > 0)
                    {
                        await _onToken(new XmlToken(XmlTokenType.ElementName, _buffer.ToString()));
                        state.SeenElementName = true;
                        _buffer.Clear();
                    }

                    if (c == '>')
                    {
                        await _onToken(new XmlToken(XmlTokenType.ClosingAngleBracket, ">"));
                        state.InsideTag = false;
                        state.SeenElementName = false;
                        if (state.IsClosingTag)
                        {
                            state.Depth--;
                            state.IsClosingTag = false;
                        }
                        else
                        {
                            // Opening tag - increase depth
                            state.Depth++;
                        }
                        state.Current = ParseState.Text;
                    }
                    else if (c == '/')
                    {
                        await _onToken(new XmlToken(XmlTokenType.SelfClosingSlash, "/"));
                        state.Current = ParseState.AfterTagName;
                    }
                    else if (IsWhitespace(c))
                    {
                        _buffer.Append(c);
                        state.Current = ParseState.InWhitespace;
                    }
                }
                break;

            case ParseState.InWhitespace:
                if (IsWhitespace(c))
                {
                    _buffer.Append(c);
                }
                else
                {
                    if (_buffer.Length > 0)
                    {
                        await _onToken(new XmlToken(XmlTokenType.Whitespace, _buffer.ToString()));
                        _buffer.Clear();
                    }

                    if (c == '<')
                    {
                        _buffer.Append(c);
                        state.Current = ParseState.TagStart;
                    }
                    else if (c == '>')
                    {
                        await _onToken(new XmlToken(XmlTokenType.ClosingAngleBracket, ">"));
                        state.InsideTag = false;
                        state.SeenElementName = false;
                        if (state.IsClosingTag)
                        {
                            state.Depth--;
                            state.IsClosingTag = false;
                        }
                        else
                        {
                            state.Depth++;
                        }
                        state.Current = ParseState.Text;
                    }
                    else if (c == '/')
                    {
                        await _onToken(new XmlToken(XmlTokenType.SelfClosingSlash, "/"));
                        state.Current = ParseState.AfterTagName;
                    }
                    else if (c == '=')
                    {
                        await _onToken(new XmlToken(XmlTokenType.AttributeEquals, "="));
                        state.Current = ParseState.AfterEquals;
                    }
                    else if (c == '"' || c == '\'')
                    {
                        state.QuoteChar = c;
                        await _onToken(new XmlToken(XmlTokenType.AttributeQuote, c.ToString()));
                        state.Current = ParseState.InAttributeValue;
                    }
                    else if (IsNameStartChar(c))
                    {
                        _buffer.Append(c);
                        if (!state.SeenElementName && state.InsideTag)
                        {
                            state.Current = ParseState.InTagName;
                        }
                        else
                        {
                            state.Current = ParseState.InAttributeName;
                        }
                    }
                }
                break;

            case ParseState.AfterTagName:
                if (c == '>')
                {
                    await _onToken(new XmlToken(XmlTokenType.ClosingAngleBracket, ">"));
                    state.InsideTag = false;
                    state.SeenElementName = false;
                    // Self-closing tag or closing tag after attributes - don't change depth for self-closing
                    if (state.IsClosingTag)
                    {
                        state.Depth--;
                        state.IsClosingTag = false;
                    }
                    // Note: for self-closing tags (<element/>), we don't change depth
                    state.Current = ParseState.Text;
                }
                else if (c == '/')
                {
                    await _onToken(new XmlToken(XmlTokenType.SelfClosingSlash, "/"));
                    // Stay in AfterTagName to handle the closing '>'
                }
                else if (IsWhitespace(c))
                {
                    _buffer.Append(c);
                    state.Current = ParseState.InWhitespace;
                }
                break;

            case ParseState.InAttributeName:
                if (IsNameChar(c))
                {
                    _buffer.Append(c);
                }
                else
                {
                    if (_buffer.Length > 0)
                    {
                        await _onToken(new XmlToken(XmlTokenType.AttributeName, _buffer.ToString()));
                        _buffer.Clear();
                    }

                    if (c == '=')
                    {
                        await _onToken(new XmlToken(XmlTokenType.AttributeEquals, "="));
                        state.Current = ParseState.AfterEquals;
                    }
                    else if (IsWhitespace(c))
                    {
                        _buffer.Append(c);
                        state.Current = ParseState.InWhitespace;
                    }
                }
                break;

            case ParseState.AfterEquals:
                if (c == '"' || c == '\'')
                {
                    state.QuoteChar = c;
                    await _onToken(new XmlToken(XmlTokenType.AttributeQuote, c.ToString()));
                    state.Current = ParseState.InAttributeValue;
                }
                else if (IsWhitespace(c))
                {
                    _buffer.Append(c);
                    state.Current = ParseState.InWhitespace;
                }
                break;

            case ParseState.InAttributeValue:
                if (c == state.QuoteChar)
                {
                    await _onToken(new XmlToken(XmlTokenType.AttributeValue, _buffer.ToString()));
                    _buffer.Clear();
                    await _onToken(new XmlToken(XmlTokenType.AttributeQuote, c.ToString()));
                    state.QuoteChar = null;
                    state.Current = ParseState.AfterTagName;
                }
                else
                {
                    _buffer.Append(c);
                }
                break;

            case ParseState.InComment:
                _buffer.Append(c);
                if (_buffer.ToString().EndsWith("-->"))
                {
                    await _onToken(new XmlToken(XmlTokenType.Comment, _buffer.ToString()));
                    _buffer.Clear();
                    state.Current = ParseState.Text;
                }
                break;

            case ParseState.InProcessingInstruction:
                _buffer.Append(c);
                if (_buffer.ToString().EndsWith("?>"))
                {
                    await _onToken(new XmlToken(XmlTokenType.ProcessingInstruction, _buffer.ToString()));
                    _buffer.Clear();
                    state.Current = ParseState.Text;
                }
                break;

            case ParseState.InDocType:
                _buffer.Append(c);
                if (c == '>')
                {
                    await _onToken(new XmlToken(XmlTokenType.DocumentTypeDeclaration, _buffer.ToString()));
                    _buffer.Clear();
                    state.Current = ParseState.Text;
                }
                break;

            case ParseState.InCData:
                _buffer.Append(c);
                if (_buffer.ToString().EndsWith("]]>"))
                {
                    await _onToken(new XmlToken(XmlTokenType.CData, _buffer.ToString()));
                    _buffer.Clear();
                    state.Current = ParseState.Text;
                }
                break;
        }
    }

    private async Task EmitPending(State state)
    {
        if (_buffer.Length > 0)
        {
            switch (state.Current)
            {
                case ParseState.Text:
                    await _onToken(new XmlToken(XmlTokenType.Text, _buffer.ToString()));
                    break;
                case ParseState.InWhitespace:
                    await _onToken(new XmlToken(XmlTokenType.Whitespace, _buffer.ToString()));
                    break;
                case ParseState.InTagName:
                    await _onToken(new XmlToken(XmlTokenType.ElementName, _buffer.ToString()));
                    break;
                case ParseState.InAttributeName:
                    await _onToken(new XmlToken(XmlTokenType.AttributeName, _buffer.ToString()));
                    break;
            }
            _buffer.Clear();
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