using NTokenizers.Core;

namespace NTokenizers.Rust;

/// <summary>
/// Provides functionality for tokenizing Rust source code text.
/// </summary>
public sealed class RustTokenizer : BaseSubTokenizer<RustToken>
{
    /// <summary>
    /// Creates a new instance of the <see cref="RustTokenizer"/> class.
    /// </summary>
    /// <returns>A new Rust tokenizer instance.</returns>
    public static RustTokenizer Create() => new();

    private static readonly HashSet<string> Keywords = new(StringComparer.Ordinal)
    {
        "as", "async", "await", "break", "const", "continue", "crate", "dyn",
        "else", "enum", "extern", "false", "fn", "for", "if", "impl", "in",
        "let", "loop", "match", "mod", "move", "mut", "pub", "ref", "return",
        "self", "Self", "static", "struct", "super", "trait", "true", "type",
        "unsafe", "use", "where", "while", "yield",
        "abstract", "become", "box", "do", "final", "macro", "override",
        "priv", "typeof", "unsized", "virtual",
        "try",
    };

    private sealed class State
    {
        public bool InWhitespace;
        public bool InIdentifier;
        public bool InNumber;
        public bool InString;
        public bool InChar;
        public bool InCommentLine;
        public bool InCommentBlock;
        public bool InOperator;
        public bool InLifetime;
        public bool InRawString;
        public bool Escape; public int RawHashCount;
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
                await EmitToken(RustTokenType.Whitespace);
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
            if (char.IsDigit(c) || c == '.' || c == 'e' || c == 'E' || c == 'x' || c == 'X' ||
                c == 'o' || c == 'O' || c == 'b' || c == 'B' || c == '_' ||
                (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F'))
            {
                _buffer.Append(c);
                return;
            }
            else
            {
                await EmitToken(RustTokenType.Number);
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

        if (state.InLifetime)
        {
            if (char.IsLetterOrDigit(c) || c == '_')
            {
                _buffer.Append(c);
                return;
            }
            else
            {
                await EmitToken(RustTokenType.Lifetime);
                _buffer.Clear();
                ClearState(state);
                await ProcessChar(c, state);
                return;
            }
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
        else if (c == '\'')
        {
            int next = Peek();
            if (next != -1 && char.IsLetter((char)next))
            {
                Read();
                int next2 = Peek();
                if (next2 == '\'')
                {
                    state.InChar = true;
                    _buffer.Append(c);
                    _buffer.Append((char)next);
                }
                else
                {
                    state.InLifetime = true;
                    _buffer.Append(c);
                    _buffer.Append((char)next);
                }
            }
            else
            {
                state.InChar = true;
                _buffer.Append(c);
            }
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
            await EmitToken(RustTokenType.OpenParenthesis, "(");
        }
        else if (c == ')')
        {
            await EmitToken(RustTokenType.CloseParenthesis, ")");
        }
        else if (c == '{')
        {
            await EmitToken(RustTokenType.OpenBrace, "{");
        }
        else if (c == '}')
        {
            await EmitToken(RustTokenType.CloseBrace, "}");
        }
        else if (c == '[')
        {
            await EmitToken(RustTokenType.OpenBracket, "[");
        }
        else if (c == ']')
        {
            await EmitToken(RustTokenType.CloseBracket, "]");
        }
        else if (c == ',')
        {
            await EmitToken(RustTokenType.Comma, ",");
        }
        else if (c == ';')
        {
            await EmitToken(RustTokenType.SequenceTerminator, ";");
        }
        else if (c == ':')
        {
            int next = Peek();
            if (next == ':')
            {
                Read();
                await EmitToken(RustTokenType.DoubleColon, "::");
            }
            else
            {
                await EmitToken(RustTokenType.Colon, ":");
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
                await EmitToken(RustTokenType.Dot, ".");
            }
        }
        else if (c == '#')
        {
            await EmitToken(RustTokenType.Pound, "#");
        }
        else if (c == '@')
        {
            await EmitToken(RustTokenType.At, "@");
        }
        else if (c == '?')
        {
            await EmitToken(RustTokenType.QuestionMark, "?");
        }
        else if (c == '=')
        {
            int next = Peek();
            if (next == '>')
            {
                Read();
                await EmitToken(RustTokenType.FatArrow, "=>");
            }
            else
            {
                state.InOperator = true;
                _buffer.Append(c);
            }
        }
        else if (c == '-')
        {
            int next = Peek();
            if (next == '>')
            {
                Read();
                await EmitToken(RustTokenType.Arrow, "->");
            }
            else
            {
                state.InOperator = true;
                _buffer.Append(c);
            }
        }
        else if (IsOperatorChar(c))
        {
            state.InOperator = true;
            _buffer.Append(c);
        }
        else
        {
            await EmitToken(RustTokenType.NotDefined, c.ToString());
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
            await EmitToken(RustTokenType.StringValue);
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
            await EmitToken(RustTokenType.CharValue);
            ClearState(state);
        }
    }

    private async Task ProcessInCommentLine(char c, State state)
    {
        if (c == '\n' || c == '\r')
        {
            await EmitToken(RustTokenType.Comment);
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
                await EmitToken(RustTokenType.Comment);
                _buffer.Clear();
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
            await EmitToken(RustTokenType.Operator);
            _buffer.Clear();
            ClearState(state);
        }
        else if (IsOperatorChar(c))
        {
            await EmitToken(RustTokenType.Operator, current);
            _buffer.Clear();
            _buffer.Append(c);
        }
        else
        {
            await EmitToken(RustTokenType.Operator, current);
            _buffer.Clear();
            ClearState(state);
            state.Escape = false;
            state.RawHashCount = 0;
            await ProcessChar(c, state);
        }
    }

    private static bool IsOperatorChar(char c)
    {
        return c == '=' || c == '!' || c == '<' || c == '>' || c == '+' || c == '-' ||
               c == '*' || c == '/' || c == '%' || c == '&' || c == '|' || c == '^' ||
               c == '~' || c == '?';
    }

    private static bool IsMultiCharOperator(string op)
    {
        return op == "==" || op == "!=" || op == "<=" || op == ">=" || op == "&&" || op == "||" ||
               op == "++" || op == "--" || op == "+=" || op == "-=" || op == "*=" || op == "/=" ||
               op == "%=" || op == "&=" || op == "|=" || op == "^=" || op == "<<" || op == ">>" ||
               op == "=>" || op == "->" || op == "::" ||
               op == "!" || op == "&" || op == "|" || op == "^" || op == "~" ||
               op == "+" || op == "-" || op == "*" || op == "/" || op == "%" ||
               op == "<" || op == ">" || op == "=" || op == "?";
    }

    private async Task EmitIdentifierOrKeyword(string value)
    {
        if (Keywords.Contains(value))
        {
            if (value == "true" || value == "false")
            {
                await EmitToken(RustTokenType.Boolean, value);
            }
            else
            {
                await EmitToken(RustTokenType.Keyword, value);
            }
        }
        else
        {
            await EmitToken(RustTokenType.Identifier, value);
        }
    }

    private async Task EmitToken(RustTokenType type, string value)
    {
        await _onToken(new RustToken(type, value));
    }

    private async Task EmitToken(RustTokenType type)
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
                await EmitToken(RustTokenType.Whitespace);
            }
            else if (state.InIdentifier)
            {
                await EmitIdentifierOrKeyword(_buffer.ToString());
                _buffer.Clear();
            }
            else if (state.InNumber)
            {
                await EmitToken(RustTokenType.Number);
            }
            else if (state.InString)
            {
                await EmitToken(RustTokenType.StringValue);
            }
            else if (state.InChar)
            {
                await EmitToken(RustTokenType.CharValue);
            }
            else if (state.InCommentLine || state.InCommentBlock)
            {
                await EmitToken(RustTokenType.Comment);
            }
            else if (state.InLifetime)
            {
                await EmitToken(RustTokenType.Lifetime);
                _buffer.Clear();
            }
            else if (state.InOperator)
            {
                await EmitToken(RustTokenType.Operator);
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
        state.InChar = false;
        state.InCommentLine = false;
        state.InCommentBlock = false;
        state.InOperator = false;
        state.InLifetime = false;
        state.InRawString = false;
    }
}
