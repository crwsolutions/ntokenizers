using NTokenizers.Core;

namespace NTokenizers.Go;

/// <summary>
/// Provides functionality for tokenizing Go source code text.
/// </summary>
public sealed class GoTokenizer : BaseSubTokenizer<GoToken>
{
    /// <summary>
    /// Creates a new instance of the <see cref="GoTokenizer"/> class.
    /// </summary>
    /// <returns>A new Go tokenizer instance.</returns>
    public static GoTokenizer Create() => new();

    private static readonly HashSet<string> Keywords = new(StringComparer.Ordinal)
    {
        "break", "case", "chan", "const", "continue", "default", "defer", "else",
        "fallthrough", "for", "func", "go", "goto", "if", "import", "interface",
        "map", "package", "range", "return", "select", "struct", "switch", "type", "var",
    };

    private static readonly HashSet<string> Booleans = new(StringComparer.Ordinal)
    {
        "true", "false",
    };

    private static readonly HashSet<string> Nulls = new(StringComparer.Ordinal)
    {
        "nil",
    };

    private sealed class State
    {
        public bool InWhitespace;
        public bool InIdentifier;
        public bool InNumber;
        public bool InString;
        public bool InRawString;
        public bool InChar;
        public bool InCommentLine;
        public bool InCommentBlock;
        public bool InOperator;
        public bool Escape;
    }

    /// <inheritdoc/>
    internal protected override async Task ParseAsync(CancellationToken ct)
    {
        var state = new State();

        await TokenizeCharactersAsync(ct, async (c) => await ProcessChar(c, state));

        await EmitPending(state);
    }

    private async Task ProcessChar(char c, State state)
    {
        if (state.InString)
        {
            await ProcessInString(c, state);
            return;
        }

        if (state.InRawString)
        {
            await ProcessInRawString(c, state);
            return;
        }

        if (state.InChar)
        {
            await ProcessInChar(c, state);
            return;
        }

        if (state.InCommentLine)
        {
            await ProcessInCommentLine(c, state);
            return;
        }

        if (state.InCommentBlock)
        {
            await ProcessInCommentBlock(c, state);
            return;
        }

        if (state.InWhitespace)
        {
            if (char.IsWhiteSpace(c))
            {
                _buffer.Append(c);
                return;
            }
            else
            {
                await EmitToken(GoTokenType.Whitespace);
                ClearState(state);
                await ProcessChar(c, state);
                return;
            }
        }

        if (state.InIdentifier)
        {
            if (char.IsLetterOrDigit(c) || c == '_')
            {
                _buffer.Append(c);
                return;
            }
            else
            {
                await EmitIdentifierOrKeyword(_buffer.ToString());
                _buffer.Clear();
                ClearState(state);
                await ProcessChar(c, state);
                return;
            }
        }

        if (state.InNumber)
        {
            if (char.IsDigit(c) || c == '.' || c == 'e' || c == 'E' || c == '-' || c == '+' || c == 'x' || c == 'X' ||
                (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F') || c == '_')
            {
                _buffer.Append(c);
                return;
            }
            else
            {
                await EmitToken(GoTokenType.Number);
                _buffer.Clear();
                ClearState(state);
                await ProcessChar(c, state);
                return;
            }
        }

        if (state.InOperator)
        {
            await ProcessInOperator(c, state);
            return;
        }

        // Start state
        if (char.IsWhiteSpace(c))
        {
            state.InWhitespace = true;
            _buffer.Append(c);
        }
        else if (c == '"')
        {
            state.InString = true;
            _buffer.Append(c);
        }
        else if (c == '`')
        {
            state.InRawString = true;
            _buffer.Append(c);
        }
        else if (c == '\'')
        {
            state.InChar = true;
            _buffer.Append(c);
        }
        else if (char.IsLetter(c) || c == '_')
        {
            state.InIdentifier = true;
            _buffer.Append(c);
        }
        else if (char.IsDigit(c))
        {
            state.InNumber = true;
            _buffer.Append(c);
        }
        else if (c == '/')
        {
            int next = Peek();
            if (next == '/')
            {
                Read();
                state.InCommentLine = true;
                _buffer.Append("//");
            }
            else if (next == '*')
            {
                Read();
                state.InCommentBlock = true;
                _buffer.Append("/*");
            }
            else
            {
                state.InOperator = true;
                _buffer.Append(c);
            }
        }
        else if (c == '(')
        {
            await EmitToken(GoTokenType.OpenParenthesis, "(");
        }
        else if (c == ')')
        {
            await EmitToken(GoTokenType.CloseParenthesis, ")");
        }
        else if (c == '{')
        {
            await EmitToken(GoTokenType.OpenBrace, "{");
        }
        else if (c == '}')
        {
            await EmitToken(GoTokenType.CloseBrace, "}");
        }
        else if (c == '[')
        {
            await EmitToken(GoTokenType.OpenBracket, "[");
        }
        else if (c == ']')
        {
            await EmitToken(GoTokenType.CloseBracket, "]");
        }
        else if (c == ',')
        {
            await EmitToken(GoTokenType.Comma, ",");
        }
        else if (c == ';')
        {
            await EmitToken(GoTokenType.SequenceTerminator, ";");
        }
        else if (c == ':')
        {
            int next = Peek();
            if (next == ':')
            {
                Read();
                await EmitToken(GoTokenType.DoubleColon, "::");
            }
            else if (next == '=')
            {
                Read();
                await EmitToken(GoTokenType.Operator, ":=");
            }
            else
            {
                await EmitToken(GoTokenType.Colon, ":");
            }
        }
        else if (c == '.')
        {
            int next = Peek();
            if (next != -1 && char.IsDigit((char)next))
            {
                state.InNumber = true;
                _buffer.Append(c);
            }
            else
            {
                await EmitToken(GoTokenType.Dot, ".");
            }
        }
        else if (c == '@')
        {
            await EmitToken(GoTokenType.At, "@");
        }
        else if (c == '#')
        {
            await EmitToken(GoTokenType.Pound, "#");
        }
        else if (c == '?')
        {
            await EmitToken(GoTokenType.QuestionMark, "?");
        }
        else if (IsOperatorChar(c))
        {
            state.InOperator = true;
            _buffer.Append(c);
        }
        else
        {
            await EmitToken(GoTokenType.NotDefined, c.ToString());
        }
    }

    private async Task ProcessInString(char c, State state)
    {
        _buffer.Append(c);
        if (state.Escape)
        {
            state.Escape = false;
        }
        else if (c == '\\')
        {
            state.Escape = true;
        }
        else if (c == '"')
        {
            await EmitToken(GoTokenType.StringValue);
            ClearState(state);
        }
    }

    private async Task ProcessInRawString(char c, State state)
    {
        _buffer.Append(c);
        if (c == '`')
        {
            await EmitToken(GoTokenType.StringValue);
            ClearState(state);
        }
    }

    private async Task ProcessInChar(char c, State state)
    {
        _buffer.Append(c);
        if (state.Escape)
        {
            state.Escape = false;
        }
        else if (c == '\\')
        {
            state.Escape = true;
        }
        else if (c == '\'')
        {
            await EmitToken(GoTokenType.CharValue);
            ClearState(state);
        }
    }

    private async Task ProcessInCommentLine(char c, State state)
    {
        if (c == '\n' || c == '\r')
        {
            await EmitToken(GoTokenType.Comment);
            _buffer.Clear();
            ClearState(state);
            state.InWhitespace = true;
            _buffer.Append(c);
        }
        else
        {
            _buffer.Append(c);
        }
    }

    private async Task ProcessInCommentBlock(char c, State state)
    {
        _buffer.Append(c);
        if (c == '*')
        {
            int next = Peek();
            if (next == '/')
            {
                _buffer.Append((char)Read());
                await EmitToken(GoTokenType.Comment);
                ClearState(state);
                return;
            }
        }
    }

    private async Task ProcessInOperator(char c, State state)
    {
        string current = _buffer.ToString();
        string combined = current + c;

        if (IsMultiCharOperator(combined))
        {
            _buffer.Append(c);
            int next = Peek();
            if (next != -1)
            {
                string further = combined + (char)next;
                if (IsMultiCharOperator(further))
                {
                    return;
                }
            }
            await EmitToken(GoTokenType.Operator);
            _buffer.Clear();
            ClearState(state);
        }
        else if (IsOperatorChar(c))
        {
            await EmitToken(GoTokenType.Operator, current);
            _buffer.Clear();
            _buffer.Append(c);
        }
        else
        {
            await EmitToken(GoTokenType.Operator, current);
            _buffer.Clear();
            ClearState(state);
            state.Escape = false;
            await ProcessChar(c, state);
        }
    }

    private static bool IsOperatorChar(char c)
    {
        return c == '=' || c == '!' || c == '<' || c == '>' || c == '+' || c == '-' ||
               c == '*' || c == '/' || c == '%' || c == '&' || c == '|' || c == '^' ||
               c == '~';
    }

    private static bool IsMultiCharOperator(string op)
    {
        return op == "==" || op == "!=" || op == "<=" || op == ">=" || op == "&&" || op == "||" ||
               op == "++" || op == "--" || op == "+=" || op == "-=" || op == "*=" || op == "/=" ||
               op == "%=" || op == "&=" || op == "|=" || op == "^=" || op == "<<" || op == ">>" ||
               op == "<<=" || op == ">>=" || op == "&^=" ||
               op == "<-" ||
               op == "!" || op == "&" || op == "|" || op == "^" || op == "~" ||
               op == "+" || op == "-" || op == "*" || op == "/" || op == "%" ||
               op == "<" || op == ">" || op == "=";
    }

    private async Task EmitIdentifierOrKeyword(string value)
    {
        if (Booleans.Contains(value))
        {
            await EmitToken(GoTokenType.Boolean, value);
        }
        else if (Nulls.Contains(value))
        {
            await EmitToken(GoTokenType.Null, value);
        }
        else if (Keywords.Contains(value))
        {
            await EmitToken(GoTokenType.Keyword, value);
        }
        else
        {
            await EmitToken(GoTokenType.Identifier, value);
        }
    }

    private async Task EmitToken(GoTokenType type, string value)
    {
        await _onToken(new GoToken(type, value));
    }

    private async Task EmitToken(GoTokenType type)
    {
        await EmitToken(type, _buffer.ToString());
        _buffer.Clear();
    }

    private async Task EmitPending(State state)
    {
        if (_buffer.Length > 0)
        {
            if (state.InWhitespace)
            {
                await EmitToken(GoTokenType.Whitespace);
            }
            else if (state.InIdentifier)
            {
                await EmitIdentifierOrKeyword(_buffer.ToString());
                _buffer.Clear();
            }
            else if (state.InNumber)
            {
                await EmitToken(GoTokenType.Number);
            }
            else if (state.InString)
            {
                await EmitToken(GoTokenType.StringValue);
            }
            else if (state.InRawString)
            {
                await EmitToken(GoTokenType.StringValue);
            }
            else if (state.InChar)
            {
                await EmitToken(GoTokenType.CharValue);
            }
            else if (state.InCommentLine || state.InCommentBlock)
            {
                await EmitToken(GoTokenType.Comment);
            }
            else if (state.InOperator)
            {
                await EmitToken(GoTokenType.Operator);
                _buffer.Clear();
            }
        }
    }

    private void ClearState(State state)
    {
        state.InWhitespace = false;
        state.InIdentifier = false;
        state.InNumber = false;
        state.InString = false;
        state.InRawString = false;
        state.InChar = false;
        state.InCommentLine = false;
        state.InCommentBlock = false;
        state.InOperator = false;
    }
}
