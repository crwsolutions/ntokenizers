using NTokenizers.Core;

namespace NTokenizers.Rust;

public sealed class RustTokenizer : BaseSubTokenizer<RustToken>
{
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
    }

    internal protected override Task ParseAsync(CancellationToken ct)
    {
        var state = new State();
        bool escape = false;
        int rawHashCount = 0;

        TokenizeCharacters(ct, (c) => ProcessChar(c, state, ref escape, ref rawHashCount));

        EmitPending(state);

        return Task.CompletedTask;
    }

    private void ProcessChar(char c, State state, ref bool escape, ref int rawHashCount)
    {
        if (state.InString)
        {
            ProcessInString(c, state, ref escape, ref rawHashCount);
            return;
        }

        if (state.InChar)
        {
            ProcessInChar(c, state, ref escape);
            return;
        }

        if (state.InCommentLine)
        {
            ProcessInCommentLine(c, state);
            return;
        }

        if (state.InCommentBlock)
        {
            ProcessInCommentBlock(c, state);
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
                EmitToken(RustTokenType.Whitespace);
                ClearState(state);
                ProcessChar(c, state, ref escape, ref rawHashCount);
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
                EmitIdentifierOrKeyword(_buffer.ToString());
                _buffer.Clear();
                ClearState(state);
                ProcessChar(c, state, ref escape, ref rawHashCount);
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
                EmitToken(RustTokenType.Number);
                _buffer.Clear();
                ClearState(state);
                ProcessChar(c, state, ref escape, ref rawHashCount);
                return;
            }
        }

        if (state.InOperator)
        {
            ProcessInOperator(c, state);
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
                EmitToken(RustTokenType.Lifetime);
                _buffer.Clear();
                ClearState(state);
                ProcessChar(c, state, ref escape, ref rawHashCount);
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
            EmitToken(RustTokenType.OpenParenthesis, "(");
        }
        else if (c == ')')
        {
            EmitToken(RustTokenType.CloseParenthesis, ")");
        }
        else if (c == '{')
        {
            EmitToken(RustTokenType.OpenBrace, "{");
        }
        else if (c == '}')
        {
            EmitToken(RustTokenType.CloseBrace, "}");
        }
        else if (c == '[')
        {
            EmitToken(RustTokenType.OpenBracket, "[");
        }
        else if (c == ']')
        {
            EmitToken(RustTokenType.CloseBracket, "]");
        }
        else if (c == ',')
        {
            EmitToken(RustTokenType.Comma, ",");
        }
        else if (c == ';')
        {
            EmitToken(RustTokenType.SequenceTerminator, ";");
        }
        else if (c == ':')
        {
            int next = Peek();
            if (next == ':')
            {
                Read();
                EmitToken(RustTokenType.DoubleColon, "::");
            }
            else
            {
                EmitToken(RustTokenType.Colon, ":");
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
                EmitToken(RustTokenType.Dot, ".");
            }
        }
        else if (c == '#')
        {
            EmitToken(RustTokenType.Pound, "#");
        }
        else if (c == '@')
        {
            EmitToken(RustTokenType.At, "@");
        }
        else if (c == '?')
        {
            EmitToken(RustTokenType.QuestionMark, "?");
        }
        else if (c == '=')
        {
            int next = Peek();
            if (next == '>')
            {
                Read();
                EmitToken(RustTokenType.FatArrow, "=>");
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
                EmitToken(RustTokenType.Arrow, "->");
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
            EmitToken(RustTokenType.NotDefined, c.ToString());
        }
    }

    private void ProcessInString(char c, State state, ref bool escape, ref int rawHashCount)
    {
        _buffer.Append(c);
        if (escape)
        {
            escape = false;
        }
        else if (c == '\\')
        {
            escape = true;
        }
        else if (c == '"')
        {
            EmitToken(RustTokenType.StringValue);
            ClearState(state);
        }
    }

    private void ProcessInChar(char c, State state, ref bool escape)
    {
        _buffer.Append(c);
        if (escape)
        {
            escape = false;
        }
        else if (c == '\\')
        {
            escape = true;
        }
        else if (c == '\'')
        {
            EmitToken(RustTokenType.CharValue);
            ClearState(state);
        }
    }

    private void ProcessInCommentLine(char c, State state)
    {
        if (c == '\n' || c == '\r')
        {
            EmitToken(RustTokenType.Comment);
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

    private void ProcessInCommentBlock(char c, State state)
    {
        _buffer.Append(c);
        if (c == '*')
        {
            int next = Peek();
            if (next == '/')
            {
                _buffer.Append((char)Read());
                EmitToken(RustTokenType.Comment);
                _buffer.Clear();
                ClearState(state);
                return;
            }
        }
    }

    private void ProcessInOperator(char c, State state)
    {
        string current = _buffer.ToString();
        string combined = current + c;

        if (IsMultiCharOperator(combined))
        {
            _buffer.Append(c);
            EmitToken(RustTokenType.Operator);
            _buffer.Clear();
            ClearState(state);
        }
        else if (IsOperatorChar(c))
        {
            EmitToken(RustTokenType.Operator, current);
            _buffer.Clear();
            _buffer.Append(c);
        }
        else
        {
            EmitToken(RustTokenType.Operator, current);
            _buffer.Clear();
            ClearState(state);
            bool tempEscape = false;
            int tempRaw = 0;
            ProcessChar(c, state, ref tempEscape, ref tempRaw);
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

    private void EmitIdentifierOrKeyword(string value)
    {
        if (Keywords.Contains(value))
        {
            if (value == "true" || value == "false")
            {
                EmitToken(RustTokenType.Boolean, value);
            }
            else
            {
                EmitToken(RustTokenType.Keyword, value);
            }
        }
        else
        {
            EmitToken(RustTokenType.Identifier, value);
        }
    }

    private void EmitToken(RustTokenType type, string value)
    {
        _onToken(new RustToken(type, value));
    }

    private void EmitToken(RustTokenType type)
    {
        EmitToken(type, _buffer.ToString());
        _buffer.Clear();
    }

    private void EmitPending(State state)
    {
        if (_buffer.Length > 0)
        {
            if (state.InWhitespace)
            {
                EmitToken(RustTokenType.Whitespace);
            }
            else if (state.InIdentifier)
            {
                EmitIdentifierOrKeyword(_buffer.ToString());
                _buffer.Clear();
            }
            else if (state.InNumber)
            {
                EmitToken(RustTokenType.Number);
            }
            else if (state.InString)
            {
                EmitToken(RustTokenType.StringValue);
            }
            else if (state.InChar)
            {
                EmitToken(RustTokenType.CharValue);
            }
            else if (state.InCommentLine || state.InCommentBlock)
            {
                EmitToken(RustTokenType.Comment);
            }
            else if (state.InLifetime)
            {
                EmitToken(RustTokenType.Lifetime);
                _buffer.Clear();
            }
            else if (state.InOperator)
            {
                EmitToken(RustTokenType.Operator);
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
