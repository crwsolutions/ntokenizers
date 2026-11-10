using System.Diagnostics;
using NTokenizers.Core;
using NTokenizers.Css;
using NTokenizers.Typescript;

namespace NTokenizers.Html;

/// <summary>
/// Provides functionality for tokenizing HTML text sources.
/// </summary>
public sealed class HtmlTokenizer : BaseSubTokenizer<HtmlToken>
{
    /// <summary>
    /// Creates a new instance of the <see cref="HtmlTokenizer"/> class.
    /// </summary>
    public static HtmlTokenizer Create() => new();

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
        InDocType,         // Inside <!DOCTYPE...>
        InWhitespace       // Collecting whitespace between structural tokens
    }

    [DebuggerDisplay("ParseState: {CurrentState} {Depth}")]
    private sealed class ParseState
    {
        public State CurrentState { get; set; } = State.Text;
        public char? QuoteChar { get; set; }
        public bool InsideTag { get; set; }
        public bool SeenElementName { get; set; }
        public int Depth { get; set; } = 0; // Track element nesting depth
        public bool IsClosingTag { get; set; } = false; // Track if current tag is a closing tag </...>
        public string? CurrentElementName { get; set; } = null; // Track the current element name for style/script detection
    }

    /// <summary>
    /// Parses HTML content from the given <see cref="TextReader"/> and
    /// produces a sequence of <see cref="HtmlToken"/> objects.
    /// </summary>
    internal protected override async Task ParseAsync(CancellationToken ct)
    {
        var state = new ParseState();

        await TokenizeCharactersAsync(ct, async (c) => await ProcessCharAsync(c, state, ct));

        await EmitPending(state);
    }

    private async Task ProcessCharAsync(char c, ParseState state, CancellationToken ct)
    {
        switch (state.CurrentState)
        {
            case State.Text:
                if (c == '<')
                {
                    if (_buffer.Length > 0)
                    {
                        await _onToken(new HtmlToken(HtmlTokenType.Text, _buffer.ToString()));
                        _buffer.Clear();
                    }
                    _buffer.Append(c);
                    state.CurrentState = State.TagStart;
                }
                else if (IsWhitespace(c) && _buffer.Length == 0 && state.Depth == 0)
                {
                    // Whitespace at document level (between elements, not inside)
                    _buffer.Append(c);
                    state.CurrentState = State.InWhitespace;
                }
                else
                {
                    _buffer.Append(c);
                }
                break;

            case State.TagStart:
                _buffer.Append(c);
                if (_buffer.ToString() == "<!-")
                {
                    // Continue reading for comment
                }
                else if (_buffer.ToString() == "<!--")
                {
                    state.CurrentState = State.InComment;
                }
                else if (_buffer.Length > 2 && _buffer[1] == '!' && char.IsLetter(_buffer[2]))
                {
                    // DOCTYPE
                    state.CurrentState = State.InDocType;
                }
                else if (_buffer.Length == 2)
                {
                    if (c == '/')
                    {
                        // End tag: </
                        await _onToken(new HtmlToken(HtmlTokenType.OpeningAngleBracket, "<"));
                        await _onToken(new HtmlToken(HtmlTokenType.SelfClosingSlash, "/"));
                        _buffer.Clear();
                        state.InsideTag = true;
                        state.SeenElementName = false;
                        state.IsClosingTag = true;
                        state.CurrentState = State.InTagName;
                    }
                    else if (IsWhitespace(c))
                    {
                        // < followed by whitespace
                        await _onToken(new HtmlToken(HtmlTokenType.OpeningAngleBracket, "<"));
                        _buffer.Remove(0, 1); // remove the '<'
                        state.InsideTag = true;
                        state.SeenElementName = false;
                        state.IsClosingTag = false;
                        state.CurrentState = State.InWhitespace;
                    }
                    else if (IsNameStartChar(c))
                    {
                        // Regular opening tag: <name
                        await _onToken(new HtmlToken(HtmlTokenType.OpeningAngleBracket, "<"));
                        _buffer.Remove(0, 1); // remove the '<'
                        state.InsideTag = true;
                        state.SeenElementName = false;
                        state.IsClosingTag = false;
                        state.CurrentState = State.InTagName;
                    }
                }
                break;

            case State.InTagName:
                if (IsNameChar(c))
                {
                    _buffer.Append(c);
                }
                else
                {
                    if (_buffer.Length > 0)
                    {
                        var elementName = _buffer.ToString();
                        await _onToken(new HtmlToken(HtmlTokenType.ElementName, elementName));
                        state.SeenElementName = true;

                        // Track element name for special handling
                        if (!state.IsClosingTag)
                        {
                            state.CurrentElementName = elementName.ToLowerInvariant();
                        }

                        _buffer.Clear();
                    }

                    if (c == '>')
                    {
                        await _onToken(new HtmlToken(HtmlTokenType.ClosingAngleBracket, ">"));
                        state.InsideTag = false;
                        state.SeenElementName = false;

                        if (state.IsClosingTag)
                        {
                            state.Depth--;
                            state.IsClosingTag = false;
                            state.CurrentElementName = null;
                        }
                        else
                        {
                            // Opening tag - increase depth
                            state.Depth++;

                            // Check if we need to delegate to a sub-tokenizer
                            if (state.CurrentElementName == "style")
                            {
                                // Emit the style container token with the metadata that drives the
                                // CSS sub-parse. The section is streamed when the client handler
                                // calls metadata.ParseInlinesAsync; the post-emit work (the closing
                                // tag, the depth fix-up) runs right after the awaited emit.
                                await _onToken(new HtmlToken(HtmlTokenType.StyleElement, string.Empty,
                                    new CssCodeBlockMetadata("css", Reader, Bob, _lookaheadBuffer, "</style>")));
                                await EmitClosingTag("style");
                                state.Depth--;
                                state.CurrentElementName = null;
                            }
                            else if (state.CurrentElementName == "script")
                            {
                                await _onToken(new HtmlToken(HtmlTokenType.ScriptElement, string.Empty,
                                    new TypeScriptCodeBlockMetadata("javascript", Reader, Bob, _lookaheadBuffer, "</script>")));
                                await EmitClosingTag("script");
                                state.Depth--;
                                state.CurrentElementName = null;
                            }
                        }

                        state.CurrentState = State.Text;
                    }
                    else if (c == '/')
                    {
                        await _onToken(new HtmlToken(HtmlTokenType.SelfClosingSlash, "/"));
                        state.CurrentElementName = null; // Self-closing tag
                        state.CurrentState = State.AfterTagName;
                    }
                    else if (IsWhitespace(c))
                    {
                        _buffer.Append(c);
                        state.CurrentState = State.InWhitespace;
                    }
                }
                break;

            case State.InWhitespace:
                if (IsWhitespace(c))
                {
                    _buffer.Append(c);
                }
                else
                {
                    if (_buffer.Length > 0)
                    {
                        await _onToken(new HtmlToken(HtmlTokenType.Whitespace, _buffer.ToString()));
                        _buffer.Clear();
                    }

                    if (c == '<')
                    {
                        _buffer.Append(c);
                        state.CurrentState = State.TagStart;
                    }
                    else if (c == '>')
                    {
                        await _onToken(new HtmlToken(HtmlTokenType.ClosingAngleBracket, ">"));
                        state.InsideTag = false;
                        state.SeenElementName = false;

                        if (state.IsClosingTag)
                        {
                            state.Depth--;
                            state.IsClosingTag = false;
                            state.CurrentElementName = null;
                        }
                        else
                        {
                            state.Depth++;

                            // Check if we need to delegate to a sub-tokenizer
                            if (state.CurrentElementName == "style")
                            {
                                // Emit the style container token with the metadata that drives the
                                // CSS sub-parse. The section is streamed when the client handler
                                // calls metadata.ParseInlinesAsync; the post-emit work (the closing
                                // tag, the depth fix-up) runs right after the awaited emit.
                                await _onToken(new HtmlToken(HtmlTokenType.StyleElement, string.Empty,
                                    new CssCodeBlockMetadata("css", Reader, Bob, _lookaheadBuffer, "</style>")));
                                await EmitClosingTag("style");
                                state.Depth--;
                                state.CurrentElementName = null;
                            }
                            else if (state.CurrentElementName == "script")
                            {
                                await _onToken(new HtmlToken(HtmlTokenType.ScriptElement, string.Empty,
                                    new TypeScriptCodeBlockMetadata("javascript", Reader, Bob, _lookaheadBuffer, "</script>")));
                                await EmitClosingTag("script");
                                state.Depth--;
                                state.CurrentElementName = null;
                            }
                        }

                        state.CurrentState = State.Text;
                    }
                    else if (c == '/')
                    {
                        await _onToken(new HtmlToken(HtmlTokenType.SelfClosingSlash, "/"));
                        state.CurrentElementName = null; // Self-closing tag
                        state.CurrentState = State.AfterTagName;
                    }
                    else if (c == '=')
                    {
                        await _onToken(new HtmlToken(HtmlTokenType.AttributeEquals, "="));
                        state.CurrentState = State.AfterEquals;
                    }
                    else if (c == '"' || c == '\'')
                    {
                        state.QuoteChar = c;
                        await _onToken(new HtmlToken(HtmlTokenType.AttributeQuote, c.ToString()));
                        state.CurrentState = State.InAttributeValue;
                    }
                    else if (IsNameStartChar(c))
                    {
                        _buffer.Append(c);
                        if (!state.SeenElementName && state.InsideTag)
                        {
                            state.CurrentState = State.InTagName;
                        }
                        else
                        {
                            state.CurrentState = State.InAttributeName;
                        }
                    }
                }
                break;

            case State.AfterTagName:
                if (c == '>')
                {
                    await _onToken(new HtmlToken(HtmlTokenType.ClosingAngleBracket, ">"));
                    state.InsideTag = false;
                    state.SeenElementName = false;
                    // Self-closing tag - don't change depth
                    if (state.IsClosingTag)
                    {
                        state.Depth--;
                        state.IsClosingTag = false;
                    }
                    state.CurrentElementName = null;
                    state.CurrentState = State.Text;
                }
                else if (c == '/')
                {
                    await _onToken(new HtmlToken(HtmlTokenType.SelfClosingSlash, "/"));
                    // Stay in AfterTagName to handle the closing '>'
                }
                else if (IsWhitespace(c))
                {
                    _buffer.Append(c);
                    state.CurrentState = State.InWhitespace;
                }
                break;

            case State.InAttributeName:
                if (IsNameChar(c))
                {
                    _buffer.Append(c);
                }
                else
                {
                    if (_buffer.Length > 0)
                    {
                        await _onToken(new HtmlToken(HtmlTokenType.AttributeName, _buffer.ToString()));
                        _buffer.Clear();
                    }

                    if (c == '=')
                    {
                        await _onToken(new HtmlToken(HtmlTokenType.AttributeEquals, "="));
                        state.CurrentState = State.AfterEquals;
                    }
                    else if (IsWhitespace(c))
                    {
                        _buffer.Append(c);
                        state.CurrentState = State.InWhitespace;
                    }
                }
                break;

            case State.AfterEquals:
                if (c == '"' || c == '\'')
                {
                    state.QuoteChar = c;
                    await _onToken(new HtmlToken(HtmlTokenType.AttributeQuote, c.ToString()));
                    state.CurrentState = State.InAttributeValue;
                }
                else if (IsWhitespace(c))
                {
                    _buffer.Append(c);
                    state.CurrentState = State.InWhitespace;
                }
                break;

            case State.InAttributeValue:
                if (c == state.QuoteChar)
                {
                    await _onToken(new HtmlToken(HtmlTokenType.AttributeValue, _buffer.ToString()));
                    _buffer.Clear();
                    await _onToken(new HtmlToken(HtmlTokenType.AttributeQuote, c.ToString()));
                    state.QuoteChar = null;
                    state.CurrentState = State.AfterTagName;
                }
                else
                {
                    _buffer.Append(c);
                }
                break;

            case State.InComment:
                _buffer.Append(c);
                if (_buffer.ToString().EndsWith("-->"))
                {
                    await _onToken(new HtmlToken(HtmlTokenType.Comment, _buffer.ToString()));
                    _buffer.Clear();
                    state.CurrentState = State.Text;
                }
                break;

            case State.InDocType:
                _buffer.Append(c);
                if (c == '>')
                {
                    await _onToken(new HtmlToken(HtmlTokenType.DocumentTypeDeclaration, _buffer.ToString()));
                    _buffer.Clear();
                    state.CurrentState = State.Text;
                }
                break;
        }
    }

    private async Task EmitClosingTag(string elementName)
    {
        // Emit </elementName>
        await _onToken(new HtmlToken(HtmlTokenType.OpeningAngleBracket, "<"));
        await _onToken(new HtmlToken(HtmlTokenType.SelfClosingSlash, "/"));
        await _onToken(new HtmlToken(HtmlTokenType.ElementName, elementName));
        await _onToken(new HtmlToken(HtmlTokenType.ClosingAngleBracket, ">"));
    }

    private async Task EmitPending(ParseState state)
    {
        if (_buffer.Length > 0)
        {
            switch (state.CurrentState)
            {
                case State.Text:
                    await _onToken(new HtmlToken(HtmlTokenType.Text, _buffer.ToString()));
                    break;
                case State.InWhitespace:
                    await _onToken(new HtmlToken(HtmlTokenType.Whitespace, _buffer.ToString()));
                    break;
                case State.InTagName:
                    await _onToken(new HtmlToken(HtmlTokenType.ElementName, _buffer.ToString()));
                    break;
                case State.InAttributeName:
                    await _onToken(new HtmlToken(HtmlTokenType.AttributeName, _buffer.ToString()));
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