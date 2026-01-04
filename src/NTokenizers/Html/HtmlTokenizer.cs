using NTokenizers.Core;
using NTokenizers.Css;
using NTokenizers.Typescript;
using System.Text;

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

    /// <summary>
    /// Parses HTML content from the given <see cref="TextReader"/> and
    /// produces a sequence of <see cref="HtmlToken"/> objects.
    /// </summary>
    internal protected override Task ParseAsync(CancellationToken ct)
    {
        var state = State.Text;

        char? quoteChar = null;
        bool insideTag = false;
        bool seenElementName = false;
        int depth = 0; // Track element nesting depth
        bool isClosingTag = false; // Track if current tag is a closing tag </...>
        string? currentElementName = null; // Track the current element name for style/script detection

        TokenizeCharacters(ct, (c) => ProcessChar(c, ref state, ref quoteChar, ref insideTag, ref seenElementName, ref depth, ref isClosingTag, ref currentElementName, ct));

        EmitPending(state);

        return Task.CompletedTask;
    }

    private void ProcessChar(char c, ref State state, ref char? quoteChar, ref bool insideTag,
                                    ref bool seenElementName, ref int depth, ref bool isClosingTag, ref string? currentElementName, CancellationToken ct)
    {
        switch (state)
        {
            case State.Text:
                if (c == '<')
                {
                    if (_buffer.Length > 0)
                    {
                        _onToken(new HtmlToken(HtmlTokenType.Text, _buffer.ToString()));
                        _buffer.Clear();
                    }
                    _buffer.Append(c);
                    state = State.TagStart;
                }
                else if (IsWhitespace(c) && _buffer.Length == 0 && depth == 0)
                {
                    // Whitespace at document level (between elements, not inside)
                    _buffer.Append(c);
                    state = State.InWhitespace;
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
                    state = State.InComment;
                }
                else if (_buffer.Length > 2 && _buffer[1] == '!' && char.IsLetter(_buffer[2]))
                {
                    // DOCTYPE
                    state = State.InDocType;
                }
                else if (_buffer.Length == 2)
                {
                    if (c == '/')
                    {
                        // End tag: </
                        _onToken(new HtmlToken(HtmlTokenType.OpeningAngleBracket, "<"));
                        _onToken(new HtmlToken(HtmlTokenType.SelfClosingSlash, "/"));
                        _buffer.Clear();
                        insideTag = true;
                        seenElementName = false;
                        isClosingTag = true;
                        state = State.InTagName;
                    }
                    else if (IsWhitespace(c))
                    {
                        // < followed by whitespace
                        _onToken(new HtmlToken(HtmlTokenType.OpeningAngleBracket, "<"));
                        _buffer.Remove(0, 1); // remove the '<'
                        insideTag = true;
                        seenElementName = false;
                        isClosingTag = false;
                        state = State.InWhitespace;
                    }
                    else if (IsNameStartChar(c))
                    {
                        // Regular opening tag: <name
                        _onToken(new HtmlToken(HtmlTokenType.OpeningAngleBracket, "<"));
                        _buffer.Remove(0, 1); // remove the '<'
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
                    _buffer.Append(c);
                }
                else
                {
                    if (_buffer.Length > 0)
                    {
                        var elementName = _buffer.ToString();
                        _onToken(new HtmlToken(HtmlTokenType.ElementName, elementName));
                        seenElementName = true;
                        
                        // Track element name for special handling
                        if (!isClosingTag)
                        {
                            currentElementName = elementName.ToLowerInvariant();
                        }
                        
                        _buffer.Clear();
                    }

                    if (c == '>')
                    {
                        _onToken(new HtmlToken(HtmlTokenType.ClosingAngleBracket, ">"));
                        insideTag = false;
                        seenElementName = false;
                        
                        if (isClosingTag)
                        {
                            depth--;
                            isClosingTag = false;
                            currentElementName = null;
                        }
                        else
                        {
                            // Opening tag - increase depth
                            depth++;
                            
                            // Check if we need to delegate to a sub-tokenizer
                            if (currentElementName == "style")
                            {
                                HandleStyleElement(ct, ref depth);
                                currentElementName = null;
                            }
                            else if (currentElementName == "script")
                            {
                                HandleScriptElement(ct, ref depth);
                                currentElementName = null;
                            }
                        }
                        
                        state = State.Text;
                    }
                    else if (c == '/')
                    {
                        _onToken(new HtmlToken(HtmlTokenType.SelfClosingSlash, "/"));
                        currentElementName = null; // Self-closing tag
                        state = State.AfterTagName;
                    }
                    else if (IsWhitespace(c))
                    {
                        _buffer.Append(c);
                        state = State.InWhitespace;
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
                        _onToken(new HtmlToken(HtmlTokenType.Whitespace, _buffer.ToString()));
                        _buffer.Clear();
                    }

                    if (c == '<')
                    {
                        _buffer.Append(c);
                        state = State.TagStart;
                    }
                    else if (c == '>')
                    {
                        _onToken(new HtmlToken(HtmlTokenType.ClosingAngleBracket, ">"));
                        insideTag = false;
                        seenElementName = false;
                        
                        if (isClosingTag)
                        {
                            depth--;
                            isClosingTag = false;
                            currentElementName = null;
                        }
                        else
                        {
                            depth++;
                            
                            // Check if we need to delegate to a sub-tokenizer
                            if (currentElementName == "style")
                            {
                                HandleStyleElement(ct, ref depth);
                                currentElementName = null;
                            }
                            else if (currentElementName == "script")
                            {
                                HandleScriptElement(ct, ref depth);
                                currentElementName = null;
                            }
                        }
                        
                        state = State.Text;
                    }
                    else if (c == '/')
                    {
                        _onToken(new HtmlToken(HtmlTokenType.SelfClosingSlash, "/"));
                        currentElementName = null; // Self-closing tag
                        state = State.AfterTagName;
                    }
                    else if (c == '=')
                    {
                        _onToken(new HtmlToken(HtmlTokenType.AttributeEquals, "="));
                        state = State.AfterEquals;
                    }
                    else if (c == '"' || c == '\'')
                    {
                        quoteChar = c;
                        _onToken(new HtmlToken(HtmlTokenType.AttributeQuote, c.ToString()));
                        state = State.InAttributeValue;
                    }
                    else if (IsNameStartChar(c))
                    {
                        _buffer.Append(c);
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
                    _onToken(new HtmlToken(HtmlTokenType.ClosingAngleBracket, ">"));
                    insideTag = false;
                    seenElementName = false;
                    // Self-closing tag - don't change depth
                    if (isClosingTag)
                    {
                        depth--;
                        isClosingTag = false;
                    }
                    currentElementName = null;
                    state = State.Text;
                }
                else if (c == '/')
                {
                    _onToken(new HtmlToken(HtmlTokenType.SelfClosingSlash, "/"));
                    // Stay in AfterTagName to handle the closing '>'
                }
                else if (IsWhitespace(c))
                {
                    _buffer.Append(c);
                    state = State.InWhitespace;
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
                        _onToken(new HtmlToken(HtmlTokenType.AttributeName, _buffer.ToString()));
                        _buffer.Clear();
                    }

                    if (c == '=')
                    {
                        _onToken(new HtmlToken(HtmlTokenType.AttributeEquals, "="));
                        state = State.AfterEquals;
                    }
                    else if (IsWhitespace(c))
                    {
                        _buffer.Append(c);
                        state = State.InWhitespace;
                    }
                }
                break;

            case State.AfterEquals:
                if (c == '"' || c == '\'')
                {
                    quoteChar = c;
                    _onToken(new HtmlToken(HtmlTokenType.AttributeQuote, c.ToString()));
                    state = State.InAttributeValue;
                }
                else if (IsWhitespace(c))
                {
                    _buffer.Append(c);
                    state = State.InWhitespace;
                }
                break;

            case State.InAttributeValue:
                if (c == quoteChar)
                {
                    _onToken(new HtmlToken(HtmlTokenType.AttributeValue, _buffer.ToString()));
                    _buffer.Clear();
                    _onToken(new HtmlToken(HtmlTokenType.AttributeQuote, c.ToString()));
                    quoteChar = null;
                    state = State.AfterTagName;
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
                    _onToken(new HtmlToken(HtmlTokenType.Comment, _buffer.ToString()));
                    _buffer.Clear();
                    state = State.Text;
                }
                break;

            case State.InDocType:
                _buffer.Append(c);
                if (c == '>')
                {
                    _onToken(new HtmlToken(HtmlTokenType.DocumentTypeDeclaration, _buffer.ToString()));
                    _buffer.Clear();
                    state = State.Text;
                }
                break;
        }
    }

    private void HandleStyleElement(CancellationToken ct, ref int depth)
    {
        // Delegate CSS content to CssTokenizer
        var cssTokenizer = CssTokenizer.Create();
        var stopDelimiter = "</style>";
        
        cssTokenizer.ParseAsync(Reader, Bob, stopDelimiter, ct, token =>
        {
            // Forward CSS tokens wrapped as HTML tokens
            _onToken(new HtmlToken(HtmlTokenType.Text, token.Value));
        }).GetAwaiter().GetResult();
        
        // After CSS tokenizer completes, we're at the closing tag
        // Emit the closing tag tokens
        EmitClosingTag("style");
        depth--;
    }

    private void HandleScriptElement(CancellationToken ct, ref int depth)
    {
        // Delegate JavaScript/TypeScript content to TypescriptTokenizer
        var tsTokenizer = TypescriptTokenizer.Create();
        var stopDelimiter = "</script>";
        
        tsTokenizer.ParseAsync(Reader, Bob, stopDelimiter, ct, token =>
        {
            // Forward TypeScript tokens wrapped as HTML tokens
            _onToken(new HtmlToken(HtmlTokenType.Text, token.Value));
        }).GetAwaiter().GetResult();
        
        // After TypeScript tokenizer completes, we're at the closing tag
        // Emit the closing tag tokens
        EmitClosingTag("script");
        depth--;
    }

    private void EmitClosingTag(string elementName)
    {
        // Emit </elementName>
        _onToken(new HtmlToken(HtmlTokenType.OpeningAngleBracket, "<"));
        _onToken(new HtmlToken(HtmlTokenType.SelfClosingSlash, "/"));
        _onToken(new HtmlToken(HtmlTokenType.ElementName, elementName));
        _onToken(new HtmlToken(HtmlTokenType.ClosingAngleBracket, ">"));
    }

    private void EmitPending(State state)
    {
        if (_buffer.Length > 0)
        {
            switch (state)
            {
                case State.Text:
                    _onToken(new HtmlToken(HtmlTokenType.Text, _buffer.ToString()));
                    break;
                case State.InWhitespace:
                    _onToken(new HtmlToken(HtmlTokenType.Whitespace, _buffer.ToString()));
                    break;
                case State.InTagName:
                    _onToken(new HtmlToken(HtmlTokenType.ElementName, _buffer.ToString()));
                    break;
                case State.InAttributeName:
                    _onToken(new HtmlToken(HtmlTokenType.AttributeName, _buffer.ToString()));
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
