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

    private sealed class State
    {
        public ParseState Current = ParseState.None;
        public char StringDelimiter = '\0';
        public bool Escape;
        public bool IsExpectingValue;
        public bool InSelector = true;
    }

    /// <summary>
    /// Parses the input text and generates CSS tokens.
    /// </summary>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    internal protected override async Task ParseAsync(CancellationToken ct)
    {
        var stack = new Stack<ContainerType>();
        var state = new State();

        await TokenizeCharactersAsync(ct, async (c) => await ProcessChar(c, state, stack));

        await EmitPending(state);
    }

    private async Task ProcessChar(char c, State state, Stack<ContainerType> stack)
    {
        // Handle comment
        if (state.Current == ParseState.Comment)
        {
            _buffer.Append(c);
            if (c == '*' && Peek() == '/')
            {
                _buffer.Append((char)Read());
                await _onToken(new CssToken(CssTokenType.Comment, _buffer.ToString()));
                _buffer.Clear();
                state.Current = ParseState.None;
            }
            return;
        }

        // Handle string
        if (state.Current == ParseState.String)
        {
            if (state.Escape)
            {
                _buffer.Append(c);
                state.Escape = false;
            }
            else if (c == '\\')
            {
                _buffer.Append(c);
                state.Escape = true;
            }
            else if (c == state.StringDelimiter)
            {
                await _onToken(new CssToken(CssTokenType.StringValue, _buffer.ToString()));
                _buffer.Clear();
                state.Current = ParseState.None;
                await _onToken(new CssToken(CssTokenType.Quote, c.ToString()));
            }
            else
            {
                _buffer.Append(c);
            }
            return;
        }

        // Handle number
        if (state.Current == ParseState.Number)
        {
            if (char.IsDigit(c) || c == '.' || c == 'E' || c == '-' || c == '+')
            {
                _buffer.Append(c);
                return;
            }
            await _onToken(new CssToken(CssTokenType.Number, _buffer.ToString()));
            _buffer.Clear();
            if (char.IsLetter(c))
            {
                _buffer.Append(c);
                state.Current = ParseState.Unit;
                return;
            }

            state.Current = ParseState.None;
        }

        // Handle unit
        if (state.Current == ParseState.Unit)
        {
            if (char.IsLetter(c))
            {
                _buffer.Append(c);
                return;
            }
            await _onToken(new CssToken(CssTokenType.Unit, _buffer.ToString()));
            _buffer.Clear();
            state.Current = ParseState.None;
        }

        // Handle whitespace
        if (char.IsWhiteSpace(c))
        {
            if (state.Current != ParseState.InWhiteSpace)
            {
                await EmitPending(state);
            }
            state.Current = ParseState.InWhiteSpace;
            _buffer.Append(c);
            return;
        }
        else if (state.Current == ParseState.InWhiteSpace)
        {
            await _onToken(new CssToken(CssTokenType.Whitespace, _buffer.ToString()));
            _buffer.Clear();
            state.Current = ParseState.None;
        }

        // Structural and special characters
        switch (c)
        {
            case '{':
                await EmitPending(state);
                await _onToken(new CssToken(CssTokenType.StartRuleSet, "{"));
                stack.Push(ContainerType.RuleSet);
                state.InSelector = false;
                break;

            case '}':
                if (stack.Count > 0 && stack.Peek() == ContainerType.RuleSet)
                    stack.Pop();
                await EmitPending(state);
                await _onToken(new CssToken(CssTokenType.EndRuleSet, "}"));
                break;

            case ':':
                await EmitPending(state);
                if (Peek() == ':')
                {
                    Read();
                    state.Current = ParseState.PseudoElement;
                    _buffer.Append("::");
                    return;
                }
                else
                {
                    await _onToken(new CssToken(CssTokenType.Colon, ":"));
                    if (!state.InSelector) state.IsExpectingValue = true;
                    state.InSelector = false;
                }
                break;

            case '-':
                if (_buffer.Length == 0 && Peek() == '-')
                {
                    _buffer.Append("-");
                    _buffer.Append((char)Read());
                    state.Current = ParseState.CustomProperty;
                    return;
                }
                else if (state.Current == ParseState.Identifier || state.Current == ParseState.Number || state.Current == ParseState.CustomProperty || state.Current == ParseState.PseudoElement)
                    _buffer.Append(c);
                else
                    await _onToken(new CssToken(CssTokenType.Operator, "-"));
                break;

            case ';':
                await EmitPending(state);
                await _onToken(new CssToken(CssTokenType.Semicolon, ";"));
                state.InSelector = false;
                break;

            case ',':
                await EmitPending(state);
                await _onToken(new CssToken(CssTokenType.Comma, ","));
                break;

            case '(':
                if (state.IsExpectingValue && _buffer.Length > 0)
                {
                    await _onToken(new CssToken(CssTokenType.Function, _buffer.ToString()));
                    _buffer.Clear();
                }
                await EmitPending(state);
                await _onToken(new CssToken(CssTokenType.OpenParen, "("));
                break;

            case ')':
                await EmitPending(state);
                await _onToken(new CssToken(CssTokenType.CloseParen, ")"));
                break;

            case '"':
            case '\'':
                // Emit the opening quote as a separate token
                await _onToken(new CssToken(CssTokenType.Quote, c.ToString()));
                state.Current = ParseState.String;
                state.StringDelimiter = c;
                break;

            case '@':
                await EmitPending(state);
                await _onToken(new CssToken(CssTokenType.AtRule, "@"));
                state.Current = ParseState.Identifier;
                break;

            case '#':
                await EmitPending(state);
                _buffer.Append(c);
                state.Current = ParseState.Identifier;
                if (state.InSelector || !state.IsExpectingValue)
                {
                    state.InSelector = true;
                }
                else
                {
                    state.InSelector = false;
                }

                break;

            case '.':
                await EmitPending(state);
                await _onToken(new CssToken(CssTokenType.DotClass, "."));
                state.Current = ParseState.Identifier;
                state.InSelector = true;
                break;

            case '[':
                await EmitPending(state);
                await _onToken(new CssToken(CssTokenType.LeftBracket, "["));
                break;

            case ']':
                await EmitPending(state);
                await _onToken(new CssToken(CssTokenType.RightBracket, "]"));
                break;

            case '=':
                await EmitPending(state);
                await _onToken(new CssToken(CssTokenType.Equals, "="));
                break;

            case '+':
            case '*':
                if (state.Current == ParseState.Number) _buffer.Append(c);
                else await _onToken(new CssToken(CssTokenType.Operator, c.ToString()));
                break;

            case '/':
                if (Peek() == '*')
                {
                    _buffer.Append(c);
                    _buffer.Append((char)Read());
                    state.Current = ParseState.Comment;
                }
                else await _onToken(new CssToken(CssTokenType.Operator, "/"));
                break;

            default:
                if (state.Current == ParseState.None && char.IsDigit(c))
                {
                    state.Current = ParseState.Number;
                    _buffer.Append(c);
                }
                else if (char.IsLetter(c) || c == '_' || c == '-')
                {
                    if (state.Current == ParseState.PseudoElement || state.Current == ParseState.CustomProperty)
                        _buffer.Append(c);
                    else
                    {
                        state.Current = ParseState.Identifier;
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
        if ((state.Current == ParseState.PseudoElement || state.Current == ParseState.CustomProperty) &&
            !char.IsLetterOrDigit(c) && c != '-')
        {
            if (_buffer.Length > 0)
            {
                if (state.Current == ParseState.PseudoElement)
                    await _onToken(new CssToken(CssTokenType.PseudoElement, _buffer.ToString()));
                else
                    await _onToken(new CssToken(CssTokenType.PropertyName, _buffer.ToString()));

                _buffer.Clear();
                state.Current = ParseState.None;
            }
        }
    }

    private async Task EmitPending(State state)
    {
        if (_buffer.Length == 0) return;

        switch (state.Current)
        {
            case ParseState.String:
                await _onToken(new CssToken(CssTokenType.StringValue, _buffer.ToString()));
                break;
            case ParseState.Number:
                await _onToken(new CssToken(CssTokenType.Number, _buffer.ToString()));
                break;
            case ParseState.Unit:
                await _onToken(new CssToken(CssTokenType.Unit, _buffer.ToString()));
                break;
            case ParseState.Identifier:
                if (state.InSelector)
                    await _onToken(new CssToken(CssTokenType.Selector, _buffer.ToString()));
                else
                    await _onToken(new CssToken(CssTokenType.Identifier, _buffer.ToString()));
                break;
            case ParseState.Comment:
                await _onToken(new CssToken(CssTokenType.Comment, _buffer.ToString()));
                break;
            case ParseState.PseudoElement:
                await _onToken(new CssToken(CssTokenType.PseudoElement, _buffer.ToString()));
                break;
            case ParseState.CustomProperty:
                await _onToken(new CssToken(CssTokenType.PropertyName, _buffer.ToString()));
                break;
            default:
                await _onToken(new CssToken(CssTokenType.Whitespace, _buffer.ToString()));
                break;
        }

        _buffer.Clear();
        state.Current = ParseState.None;
    }
}