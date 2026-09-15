using NTokenizers.Core;

namespace NTokenizers.Toml;

/// <summary>
/// Provides functionality for tokenizing TOML or TOML-like text sources.
/// </summary>
public sealed class TomlTokenizer : BaseSubTokenizer<TomlToken>
{
    /// <summary>
    /// Creates a new instance of the <see cref="TomlTokenizer"/>.
    /// </summary>
    public static TomlTokenizer Create() => new();

    /// <summary>
    /// Parses TOML or TOML-like content from the given <see cref="TextReader"/> and
    /// produces a sequence of <see cref="TomlToken"/> objects.
    /// </summary>
    internal protected override Task ParseAsync(CancellationToken ct)
    {
        var state = new State();

        TokenizeCharacters(ct, c => ProcessChar(c, state));

        EmitPending(state);

        return Task.CompletedTask;
    }

    private void ProcessChar(char c, State state)
    {
        // --- Comment: # to end of line ---
        if (state.ParseState == ParseState.Comment)
        {
            _buffer.Append(c);
            if (c == '\n')
            {
                _onToken(new TomlToken(TomlTokenType.Comment, _buffer.ToString()));
                _buffer.Clear();
                state.ParseState = ParseState.None;
            }
            return;
        }

        // --- Basic single-line string: "..." ---
        if (state.ParseState == ParseState.BasicString)
        {
            if (state.Escape)
            {
                state.Escape = false;
                _buffer.Append(c);
            }
            else if (c == '\\')
            {
                state.Escape = true;
                _buffer.Append(c);
            }
            else if (c == '"')
            {
                _onToken(new TomlToken(TomlTokenType.StringValue, _buffer.ToString()));
                _buffer.Clear();
                _onToken(new TomlToken(TomlTokenType.StringQuote, "\""));
                state.ParseState = ParseState.None;
            }
            else
            {
                _buffer.Append(c);
            }
            return;
        }

        // --- Literal single-line string: '...' ---
        if (state.ParseState == ParseState.LiteralString)
        {
            if (c == '\'')
            {
                _onToken(new TomlToken(TomlTokenType.StringValue, _buffer.ToString()));
                _buffer.Clear();
                _onToken(new TomlToken(TomlTokenType.StringQuote, "'"));
                state.ParseState = ParseState.None;
            }
            else
            {
                _buffer.Append(c);
            }
            return;
        }

        // --- Multiline basic string: """...""" ---
        if (state.ParseState == ParseState.MultilineBasic)
        {
            if (state.Escape)
            {
                state.Escape = false;
                _buffer.Append(c);
            }
            else if (c == '\\')
            {
                state.Escape = true;
                _buffer.Append(c);
            }
            else if (c == '"')
            {
                state.MlQuoteCount++;
                if (state.MlQuoteCount >= 3)
                {
                    while (_buffer.Length > 0 && _buffer[_buffer.Length - 1] == '"')
                    {
                        _buffer.Length--;
                    }
                    _onToken(new TomlToken(TomlTokenType.StringValue, _buffer.ToString()));
                    _buffer.Clear();
                    _onToken(new TomlToken(TomlTokenType.StringQuote, "\"\"\""));
                    state.ParseState = ParseState.None;
                    state.MlQuoteCount = 0;
                }
                else
                {
                    _buffer.Append(c);
                }
            }
            else
            {
                state.MlQuoteCount = 0;
                _buffer.Append(c);
            }
            return;
        }

        // --- Multiline literal string: '''...''' ---
        if (state.ParseState == ParseState.MultilineLiteral)
        {
            if (c == '\'')
            {
                state.MlQuoteCount++;
                if (state.MlQuoteCount >= 3)
                {
                    while (_buffer.Length > 0 && _buffer[_buffer.Length - 1] == '\'')
                    {
                        _buffer.Length--;
                    }
                    _onToken(new TomlToken(TomlTokenType.StringValue, _buffer.ToString()));
                    _buffer.Clear();
                    _onToken(new TomlToken(TomlTokenType.StringQuote, "'''"));
                    state.ParseState = ParseState.None;
                    state.MlQuoteCount = 0;
                }
                else
                {
                    _buffer.Append(c);
                }
            }
            else
            {
                state.MlQuoteCount = 0;
                _buffer.Append(c);
            }
            return;
        }

        // --- Number ---
        if (state.ParseState == ParseState.Number)
        {
            if (IsNumberChar(c))
            {
                _buffer.Append(c);
                return;
            }

            // End of number
            EmitNumber();
            state.ParseState = ParseState.None;
            state.AfterEquals = false;
            // Fall through to process current c
        }

        // --- Number or Date ---
        if (state.ParseState == ParseState.NumberOrDate)
        {
            // Commit to Number on hex/oct/bin prefix or exponent
            if (c == 'x' || c == 'X' || c == 'o' || c == 'O' || c == 'b' || c == 'B' || c == 'e' || c == 'E')
            {
                _buffer.Append(c);
                state.ParseState = ParseState.Number;
                return;
            }

            // Commit to DateTime on - or :
            if (c == '-' || c == ':')
            {
                _buffer.Append(c);
                state.ParseState = ParseState.DateTime;
                return;
            }

            // Still ambiguous — keep accumulating
            if (IsDigit(c) || c == '.' || c == '_' || c == '+' || c == '-')
            {
                _buffer.Append(c);
                return;
            }

            // End of token — still ambiguous, emit as Number
            EmitNumber();
            state.ParseState = ParseState.None;
            state.AfterEquals = false;
            // Fall through to process current c
        }

        // --- DateTime ---
        if (state.ParseState == ParseState.DateTime)
        {
            if (IsDateTimeChar(c))
            {
                _buffer.Append(c);
                return;
            }

            EmitDateTime();
            state.ParseState = ParseState.None;
            state.AfterEquals = false;
            // Fall through to process current c
        }

        // --- Identifier / bare key ---
        if (state.ParseState == ParseState.Identifier)
        {
            if (IsIdentifierChar(c))
            {
                _buffer.Append(c);
                return;
            }

            EmitIdentifierToken(state);
            state.ParseState = ParseState.None;
            state.AfterEquals = false;
            // Fall through to process current c
        }

        // --- Whitespace ---
        if (char.IsWhiteSpace(c))
        {
            _buffer.Append(c);
            return;
        }

        // --- Emit any pending whitespace ---
        if (_buffer.Length > 0)
        {
            _onToken(new TomlToken(TomlTokenType.Whitespace, _buffer.ToString()));
            _buffer.Clear();
        }

        // --- Structural / value characters ---
        switch (c)
        {
            case '#':
                _buffer.Append(c);
                state.ParseState = ParseState.Comment;
                break;

            case '"':
                if (PeekAhead(0) == '"' && PeekAhead(1) == '"')
                {
                    Read(); // consume second "
                    Read(); // consume third "
                    _onToken(new TomlToken(TomlTokenType.StringQuote, "\"\"\""));
                    state.ParseState = ParseState.MultilineBasic;
                    state.MlQuoteCount = 0;
                }
                else
                {
                    _onToken(new TomlToken(TomlTokenType.StringQuote, "\""));
                    state.ParseState = ParseState.BasicString;
                }
                break;

            case '\'':
                if (PeekAhead(0) == '\'' && PeekAhead(1) == '\'')
                {
                    Read(); // consume second '
                    Read(); // consume third '
                    _onToken(new TomlToken(TomlTokenType.StringQuote, "'''"));
                    state.ParseState = ParseState.MultilineLiteral;
                    state.MlQuoteCount = 0;
                }
                else
                {
                    _onToken(new TomlToken(TomlTokenType.StringQuote, "'"));
                    state.ParseState = ParseState.LiteralString;
                }
                break;

            case '.':
                _onToken(new TomlToken(TomlTokenType.Dot, "."));
                break;

            case '=':
                _onToken(new TomlToken(TomlTokenType.Equal, "="));
                state.AfterEquals = true;
                break;

            case ',':
                _onToken(new TomlToken(TomlTokenType.Comma, ","));
                break;

            case '[':
                _onToken(new TomlToken(TomlTokenType.OpenBracket, "["));
                break;

            case ']':
                _onToken(new TomlToken(TomlTokenType.CloseBracket, "]"));
                break;

            case '{':
                _onToken(new TomlToken(TomlTokenType.OpenBrace, "{"));
                break;

            case '}':
                _onToken(new TomlToken(TomlTokenType.CloseBrace, "}"));
                break;

            case '-':
            case '+':
                HandleSign(c, state);
                break;

            default:
                if (char.IsDigit(c))
                {
                    _buffer.Append(c);
                    state.ParseState = ParseState.NumberOrDate;
                }
                else if (char.IsLetter(c))
                {
                    _buffer.Append(c);
                    state.ParseState = ParseState.Identifier;
                }
                break;
        }
    }

    private void HandleSign(char c, State state)
    {
        char next = PeekAhead(0);
        if (next >= '0' && next <= '9')
        {
            _buffer.Append(c);
            state.ParseState = ParseState.NumberOrDate;
        }
        else if (next == 'i' || next == 'n')
        {
            _buffer.Append(c);
            state.ParseState = ParseState.Identifier;
        }
        else
        {
            _buffer.Append(c);
            state.ParseState = ParseState.Number;
        }
    }

    private void EmitPending(State state)
    {
        if (_buffer.Length > 0)
        {
            switch (state.ParseState)
            {
                case ParseState.Number:
                case ParseState.NumberOrDate:
                    EmitNumber();
                    break;
                case ParseState.DateTime:
                    EmitDateTime();
                    break;
                case ParseState.Identifier:
                    EmitIdentifierToken(state);
                    break;
                case ParseState.Comment:
                    _onToken(new TomlToken(TomlTokenType.Comment, _buffer.ToString()));
                    break;
                case ParseState.BasicString:
                case ParseState.LiteralString:
                case ParseState.MultilineBasic:
                case ParseState.MultilineLiteral:
                    // Incomplete string, emit content as-is
                    _onToken(new TomlToken(TomlTokenType.StringValue, _buffer.ToString()));
                    break;
                default:
                    _onToken(new TomlToken(TomlTokenType.Whitespace, _buffer.ToString()));
                    break;
            }
            _buffer.Clear();
        }
    }

    private void EmitNumber()
    {
        _onToken(new TomlToken(TomlTokenType.Number, _buffer.ToString()));
        _buffer.Clear();
    }

    private void EmitDateTime()
    {
        _onToken(new TomlToken(TomlTokenType.DateTime, _buffer.ToString()));
        _buffer.Clear();
    }

    private void EmitIdentifierToken(State state)
    {
        string value = _buffer.ToString();
        _buffer.Clear();

        TomlTokenType type = value switch
        {
            "true" or "false" => TomlTokenType.Boolean,
            "inf" or "-inf" or "+inf" or "nan" or "-nan" or "+nan" when state.AfterEquals => TomlTokenType.Number,
            _ => TomlTokenType.Identifier
        };

        _onToken(new TomlToken(type, value));
    }

    private static bool IsNumberChar(char c)
    {
        if (char.IsDigit(c)) return true;
        if (c == '.' || c == 'e' || c == 'E' || c == '+' || c == '-') return true;
        if (c >= 'a' && c <= 'f') return true; // hex digits
        if (c >= 'A' && c <= 'F') return true; // hex digits
        if (c == '_') return true; // TOML allows underscores in numbers
        if (c == 'x' || c == 'o' || c == 'b') return true; // hex/oct/bin prefix
        return false;
    }

    private static bool IsDateTimeChar(char c)
    {
        if (char.IsDigit(c)) return true;
        if (c == '-' || c == ':' || c == 'T' || c == 'Z' || c == '+' || c == '.') return true;
        return false;
    }

    private static bool IsIdentifierChar(char c)
    {
        if (char.IsLetterOrDigit(c)) return true;
        if (c == '-' || c == '_') return true;
        return false;
    }

    private static bool IsDigit(char c) => c >= '0' && c <= '9';

    private enum ParseState
    {
        None,
        Comment,
        BasicString,
        LiteralString,
        MultilineBasic,
        MultilineLiteral,
        Number,
        NumberOrDate,
        DateTime,
        Identifier,
    }

    private sealed class State
    {
        public ParseState ParseState;
        public bool Escape;
        public bool AfterEquals;
        public int MlQuoteCount;
    }
}
