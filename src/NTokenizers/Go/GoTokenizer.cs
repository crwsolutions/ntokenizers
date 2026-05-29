using NTokenizers.Core;

namespace NTokenizers.Go;

/// <summary>
/// Provides functionality for tokenizing Go source code text.
/// </summary>
public sealed class GoTokenizer : BaseSubTokenizer<GoToken>
{
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
    }

    internal protected override Task ParseAsync(CancellationToken ct)
    {
        var state = new State();
        bool escape = false;

        TokenizeCharacters(ct, (c) => ProcessChar(c, state, ref escape));

        EmitPending(state);

        return Task.CompletedTask;
    }

    private void ProcessChar(char c, State state, ref bool escape)
    {
        if (state.InString)
        {
            ProcessInString(c, state, ref escape);
            return;
        }

        if (state.InRawString)
        {
            ProcessInRawString(c, state);
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
                EmitToken(GoTokenType.Whitespace);
                ClearState(state);
                ProcessChar(c, state, ref escape);
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
                ProcessChar(c, state, ref escape);
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
                EmitToken(GoTokenType.Number);
                _buffer.Clear();
                ClearState(state);
                ProcessChar(c, state, ref escape);
                return;
            }
        }

        if (state.InOperator)
        {
            ProcessInOperator(c, state);
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
            EmitToken(GoTokenType.OpenParenthesis, "(");
        }
        else if (c == ')')
        {
            EmitToken(GoTokenType.CloseParenthesis, ")");
        }
        else if (c == '{')
        {
            EmitToken(GoTokenType.OpenBrace, "{");
        }
        else if (c == '}')
        {
            EmitToken(GoTokenType.CloseBrace, "}");
        }
        else if (c == '[')
        {
            EmitToken(GoTokenType.OpenBracket, "[");
        }
        else if (c == ']')
        {
            EmitToken(GoTokenType.CloseBracket, "]");
        }
        else if (c == ',')
        {
            EmitToken(GoTokenType.Comma, ",");
        }
        else if (c == ';')
        {
            EmitToken(GoTokenType.SequenceTerminator, ";");
        }
        else if (c == ':')
        {
            int next = Peek();
            if (next == ':')
            {
                Read();
                EmitToken(GoTokenType.DoubleColon, "::");
            }
            else if (next == '=')
            {
                Read();
                EmitToken(GoTokenType.Operator, ":=");
            }
            else
            {
                EmitToken(GoTokenType.Colon, ":");
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
                EmitToken(GoTokenType.Dot, ".");
            }
        }
        else if (c == '@')
        {
            EmitToken(GoTokenType.At, "@");
        }
        else if (c == '#')
        {
            EmitToken(GoTokenType.Pound, "#");
        }
        else if (c == '?')
        {
            EmitToken(GoTokenType.QuestionMark, "?");
        }
        else if (IsOperatorChar(c))
        {
            state.InOperator = true;
            _buffer.Append(c);
        }
        else
        {
            EmitToken(GoTokenType.NotDefined, c.ToString());
        }
    }

    private void ProcessInString(char c, State state, ref bool escape)
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
            EmitToken(GoTokenType.StringValue);
            ClearState(state);
        }
    }

    private void ProcessInRawString(char c, State state)
    {
        _buffer.Append(c);
        if (c == '`')
        {
            EmitToken(GoTokenType.StringValue);
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
            EmitToken(GoTokenType.CharValue);
            ClearState(state);
        }
    }

    private void ProcessInCommentLine(char c, State state)
    {
        if (c == '\n' || c == '\r')
        {
            EmitToken(GoTokenType.Comment);
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
                EmitToken(GoTokenType.Comment);
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
            int next = Peek();
            if (next != -1)
            {
                string further = combined + (char)next;
                if (IsMultiCharOperator(further))
                {
                    return;
                }
            }
            EmitToken(GoTokenType.Operator);
            _buffer.Clear();
            ClearState(state);
        }
        else if (IsOperatorChar(c))
        {
            EmitToken(GoTokenType.Operator, current);
            _buffer.Clear();
            _buffer.Append(c);
        }
        else
        {
            EmitToken(GoTokenType.Operator, current);
            _buffer.Clear();
            ClearState(state);
            bool tempEscape = false;
            ProcessChar(c, state, ref tempEscape);
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

    private void EmitIdentifierOrKeyword(string value)
    {
        if (Booleans.Contains(value))
        {
            EmitToken(GoTokenType.Boolean, value);
        }
        else if (Nulls.Contains(value))
        {
            EmitToken(GoTokenType.Null, value);
        }
        else if (Keywords.Contains(value))
        {
            EmitToken(GoTokenType.Keyword, value);
        }
        else
        {
            EmitToken(GoTokenType.Identifier, value);
        }
    }

    private void EmitToken(GoTokenType type, string value)
    {
        _onToken(new GoToken(type, value));
    }

    private void EmitToken(GoTokenType type)
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
                EmitToken(GoTokenType.Whitespace);
            }
            else if (state.InIdentifier)
            {
                EmitIdentifierOrKeyword(_buffer.ToString());
                _buffer.Clear();
            }
            else if (state.InNumber)
            {
                EmitToken(GoTokenType.Number);
            }
            else if (state.InString)
            {
                EmitToken(GoTokenType.StringValue);
            }
            else if (state.InRawString)
            {
                EmitToken(GoTokenType.StringValue);
            }
            else if (state.InChar)
            {
                EmitToken(GoTokenType.CharValue);
            }
            else if (state.InCommentLine || state.InCommentBlock)
            {
                EmitToken(GoTokenType.Comment);
            }
            else if (state.InOperator)
            {
                EmitToken(GoTokenType.Operator);
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
