using NTokenizers.Core;

namespace NTokenizers.Css;

/// <summary>
/// A CSS tokenizer that converts CSS text into a sequence of tokens.
/// </summary>
public sealed class CssTokenizer : BaseSubTokenizer<CssToken>
{
    /// <summary>
    /// Creates a new instance of the <see cref="CssTokenizer"/> class.
    /// </summary>
    /// <returns>A new <see cref="CssTokenizer"/> instance.</returns>
    public static CssTokenizer Create() => new();

    private enum ParseState
    {
        None,
        String,
        Number,
        Unit,
        Identifier,
        Comment,
        PseudoElement,
        CustomProperty,
        InWhiteSpace
    }

    private enum ContainerType
    {
        RuleSet
    }

    /// <summary>
    /// Parses the input text and generates CSS tokens.
    /// </summary>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    internal protected override Task ParseAsync(CancellationToken ct)
    {
        var stack = new Stack<ContainerType>();
        ParseState state = ParseState.None;
        char stringDelimiter = '\0';
        bool escape = false;
        bool isExpectingValue = false;
        bool inSelector = true;

        TokenizeCharacters(ct, (c) =>
            ProcessChar(c, ref state, ref stringDelimiter, ref escape,
                        ref isExpectingValue, ref inSelector, stack));

        EmitPending(ref state, inSelector);
        return Task.CompletedTask;
    }

    private void ProcessChar(char c,
        ref ParseState state,
        ref char stringDelimiter,
        ref bool escape,
        ref bool isExpectingValue,
        ref bool inSelector,
        Stack<ContainerType> stack)
    {
        // Handle comment
        if (state == ParseState.Comment)
        {
            _buffer.Append(c);
            if (c == '*' && Peek() == '/')
            {
                _buffer.Append((char)Read());
                _onToken(new CssToken(CssTokenType.Comment, _buffer.ToString()));
                _buffer.Clear();
                state = ParseState.None;
            }
            return;
        }

        // Handle string
        if (state == ParseState.String)
        {
            if (escape)
            {
                _buffer.Append(c);
                escape = false;
            }
            else if (c == '\\')
            {
                _buffer.Append(c);
                escape = true;
            }
            else if (c == stringDelimiter)
            {
                _onToken(new CssToken(CssTokenType.StringValue, _buffer.ToString()));
                _buffer.Clear();
                state = ParseState.None;
                _onToken(new CssToken(CssTokenType.Quote, c.ToString()));
            }
            else
            {
                _buffer.Append(c);
            }
            return;
        }

        // Handle number
        if (state == ParseState.Number)
        {
            if (char.IsDigit(c) || c == '.' || c == 'E' || c == '-' || c == '+')
            {
                _buffer.Append(c);
                return;
            }
            _onToken(new CssToken(CssTokenType.Number, _buffer.ToString()));
            _buffer.Clear();
            if (char.IsLetter(c))
            {
                _buffer.Append(c);
                state = ParseState.Unit;
                return;
            }

            state = ParseState.None;
        }

        // Handle unit
        if (state == ParseState.Unit)
        {
            if (char.IsLetter(c))
            {
                _buffer.Append(c);
                return;
            }
            _onToken(new CssToken(CssTokenType.Unit, _buffer.ToString()));
            _buffer.Clear();
            state = ParseState.None;
        }

        // Handle whitespace
        if (char.IsWhiteSpace(c))
        {
            if (state != ParseState.InWhiteSpace)
            {
                EmitPending(ref state, inSelector);
            }
            state = ParseState.InWhiteSpace;
            _buffer.Append(c);
            return;
        }
        else if (state == ParseState.InWhiteSpace)
        {
            _onToken(new CssToken(CssTokenType.Whitespace, _buffer.ToString()));
            _buffer.Clear();
            state = ParseState.None;
        }

        // Structural and special characters
        switch (c)
        {
            case '{':
                EmitPending(ref state, inSelector);
                _onToken(new CssToken(CssTokenType.StartRuleSet, "{"));
                stack.Push(ContainerType.RuleSet);
                inSelector = false;
                break;

            case '}':
                if (stack.Count > 0 && stack.Peek() == ContainerType.RuleSet)
                    stack.Pop();
                EmitPending(ref state, inSelector);
                _onToken(new CssToken(CssTokenType.EndRuleSet, "}"));
                break;

            case ':':
                EmitPending(ref state, inSelector);
                if (Peek() == ':')
                {
                    Read();
                    state = ParseState.PseudoElement;
                    _buffer.Append("::");
                    return;
                }
                else
                {
                    _onToken(new CssToken(CssTokenType.Colon, ":"));
                    if (!inSelector) isExpectingValue = true;
                    inSelector = false;
                }
                break;

            case '-':
                if (_buffer.Length == 0 && Peek() == '-')
                {
                    _buffer.Append("-");
                    _buffer.Append((char)Read());
                    state = ParseState.CustomProperty;
                    return;
                }
                else if (state == ParseState.Identifier || state == ParseState.Number || state == ParseState.CustomProperty || state == ParseState.PseudoElement)
                    _buffer.Append(c);
                else
                    _onToken(new CssToken(CssTokenType.Operator, "-"));
                break;

            case ';':
                EmitPending(ref state, inSelector);
                _onToken(new CssToken(CssTokenType.Semicolon, ";"));
                inSelector = false;
                break;

            case ',':
                EmitPending(ref state, inSelector);
                _onToken(new CssToken(CssTokenType.Comma, ","));
                break;

            case '(':
                if (isExpectingValue && _buffer.Length > 0)
                {
                    _onToken(new CssToken(CssTokenType.Function, _buffer.ToString()));
                    _buffer.Clear();
                }
                EmitPending(ref state, inSelector);
                _onToken(new CssToken(CssTokenType.OpenParen, "("));
                break;

            case ')':
                EmitPending(ref state, inSelector);
                _onToken(new CssToken(CssTokenType.CloseParen, ")"));
                break;

            case '"':
            case '\'' :
                // Emit the opening quote as a separate token
                _onToken(new CssToken(CssTokenType.Quote, c.ToString()));
                state = ParseState.String;
                stringDelimiter = c;
                break;

            case '@':
                EmitPending(ref state, inSelector);
                _onToken(new CssToken(CssTokenType.AtRule, "@"));
                state = ParseState.Identifier;
                break;

            case '#':
                EmitPending(ref state, inSelector);
                _buffer.Append(c);
                state = ParseState.Identifier;
                if (inSelector || !isExpectingValue)
                {
                    inSelector = true;
                }
                else
                {
                    inSelector = false;
                }

                break;

            case '.':
                EmitPending(ref state, inSelector);
                _onToken(new CssToken(CssTokenType.DotClass, "."));
                state = ParseState.Identifier;
                inSelector = true;
                break;

            case '[':
                EmitPending(ref state, inSelector);
                _onToken(new CssToken(CssTokenType.LeftBracket, "["));
                break;

            case ']':
                EmitPending(ref state, inSelector);
                _onToken(new CssToken(CssTokenType.RightBracket, "]"));
                break;

            case '=':
                EmitPending(ref state, inSelector);
                _onToken(new CssToken(CssTokenType.Equals, "="));
                break;

            case '+':
            case '*':
                if (state == ParseState.Number) _buffer.Append(c);
                else _onToken(new CssToken(CssTokenType.Operator, c.ToString()));
                break;

            case '/':
                if (Peek() == '*')
                {
                    _buffer.Append(c);
                    _buffer.Append((char)Read());
                    state = ParseState.Comment;
                }
                else _onToken(new CssToken(CssTokenType.Operator, "/"));
                break;

            default:
                if (state == ParseState.None && char.IsDigit(c))
                {
                    state = ParseState.Number;
                    _buffer.Append(c);
                }
                else if (char.IsLetter(c) || c == '_' || c == '-')
                {
                    if (state == ParseState.PseudoElement || state == ParseState.CustomProperty)
                        _buffer.Append(c);
                    else
                    {
                        state = ParseState.Identifier;
                        _buffer.Append(c);
                    }
                }
                else
                {
                    _buffer.Append(c);
                }
                break;
        }

        // Emit pseudo-element or custom property token if non-letter encountered
        if ((state == ParseState.PseudoElement || state == ParseState.CustomProperty) &&
            !char.IsLetterOrDigit(c) && c != '-')
        {
            if (_buffer.Length > 0)
            {
                if (state == ParseState.PseudoElement)
                    _onToken(new CssToken(CssTokenType.PseudoElement, _buffer.ToString()));
                else
                    _onToken(new CssToken(CssTokenType.PropertyName, _buffer.ToString()));

                _buffer.Clear();
                state = ParseState.None;
            }
        }
    }

    private void EmitPending(ref ParseState state, bool inSelector)
    {
        if (_buffer.Length == 0) return;

        switch (state)
        {
            case ParseState.String:
                _onToken(new CssToken(CssTokenType.StringValue, _buffer.ToString()));
                break;
            case ParseState.Number:
                _onToken(new CssToken(CssTokenType.Number, _buffer.ToString()));
                break;
            case ParseState.Unit:
                _onToken(new CssToken(CssTokenType.Unit, _buffer.ToString()));
                break; 
            case ParseState.Identifier:
                if (inSelector)
                    _onToken(new CssToken(CssTokenType.Selector, _buffer.ToString()));
                else
                    _onToken(new CssToken(CssTokenType.Identifier, _buffer.ToString()));
                break;
            case ParseState.Comment:
                _onToken(new CssToken(CssTokenType.Comment, _buffer.ToString()));
                break;
            case ParseState.PseudoElement:
                _onToken(new CssToken(CssTokenType.PseudoElement, _buffer.ToString()));
                break;
            case ParseState.CustomProperty:
                _onToken(new CssToken(CssTokenType.PropertyName, _buffer.ToString()));
                break;
            default:
                _onToken(new CssToken(CssTokenType.Whitespace, _buffer.ToString()));
                break;
        }

        _buffer.Clear();
        state = ParseState.None;
    }
}