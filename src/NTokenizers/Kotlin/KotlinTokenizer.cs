using NTokenizers.Core;

namespace NTokenizers.Kotlin;

public sealed class KotlinTokenizer : BaseSubTokenizer<KotlinToken>
{
    public static KotlinTokenizer Create() => new();

    private static readonly HashSet<string> Keywords = new(StringComparer.Ordinal)
    {
        "abstract", "actual", "as", "break", "by", "catch", "class", "companion",
        "const", "constructor", "continue", "crossinline", "data", "do", "else",
        "enum", "false", "field", "file", "final", "finally", "for", "fun",
        "get", "if", "import", "in", "infix", "init", "inner", "inline",
        "interface", "internal", "is", "it", "lateinit", "noinline",
        "null", "object", "open", "out", "override", "package", "param",
        "private", "property", "protected", "public", "receiver", "reified",
        "return", "sealed", "set", "setparam", "static", "super", "this",
        "throw", "true", "try", "typealias", "tailrec", "val", "var",
        "vararg", "when", "where", "while",
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
                EmitToken(KotlinTokenType.Whitespace);
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
                EmitToken(KotlinTokenType.Number);
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
            EmitToken(KotlinTokenType.OpenParenthesis, "(");
        }
        else if (c == ')')
        {
            EmitToken(KotlinTokenType.CloseParenthesis, ")");
        }
        else if (c == '{')
        {
            EmitToken(KotlinTokenType.OpenBrace, "{");
        }
        else if (c == '}')
        {
            EmitToken(KotlinTokenType.CloseBrace, "}");
        }
        else if (c == '[')
        {
            EmitToken(KotlinTokenType.OpenBracket, "[");
        }
        else if (c == ']')
        {
            EmitToken(KotlinTokenType.CloseBracket, "]");
        }
        else if (c == ',')
        {
            EmitToken(KotlinTokenType.Comma, ",");
        }
        else if (c == ';')
        {
            EmitToken(KotlinTokenType.SequenceTerminator, ";");
        }
        else if (c == ':')
        {
            int next = Peek();
            if (next == ':')
            {
                Read();
                EmitToken(KotlinTokenType.DoubleColon, "::");
            }
            else
            {
                EmitToken(KotlinTokenType.Colon, ":");
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
                EmitToken(KotlinTokenType.Dot, ".");
            }
        }
        else if (c == '@')
        {
            EmitToken(KotlinTokenType.At, "@");
        }
        else if (c == '#')
        {
            EmitToken(KotlinTokenType.Pound, "#");
        }
        else if (c == '?')
        {
            EmitToken(KotlinTokenType.QuestionMark, "?");
        }
        else if (IsOperatorChar(c))
        {
            state.InOperator = true;
            _buffer.Append(c);
        }
        else
        {
            EmitToken(KotlinTokenType.NotDefined, c.ToString());
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
            EmitToken(KotlinTokenType.StringValue);
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
            EmitToken(KotlinTokenType.CharValue);
            ClearState(state);
        }
    }

    private void ProcessInCommentLine(char c, State state)
    {
        if (c == '\n' || c == '\r')
        {
            EmitToken(KotlinTokenType.Comment);
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
                EmitToken(KotlinTokenType.Comment);
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
            EmitToken(KotlinTokenType.Operator);
            _buffer.Clear();
            ClearState(state);
        }
        else if (IsOperatorChar(c))
        {
            EmitToken(KotlinTokenType.Operator, current);
            _buffer.Clear();
            _buffer.Append(c);
        }
        else
        {
            EmitToken(KotlinTokenType.Operator, current);
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
               c == '~' || c == '?';
    }

    private static bool IsMultiCharOperator(string op)
    {
        return op == "==" || op == "!=" || op == "<=" || op == ">=" || op == "&&" || op == "||" ||
               op == "++" || op == "--" || op == "+=" || op == "-=" || op == "*=" || op == "/=" ||
               op == "%=" || op == "&=" || op == "|=" || op == "^=" || op == "<<" || op == ">>" ||
               op == "->" || op == "?." || op == "?:" ||
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
                EmitToken(KotlinTokenType.Boolean, value);
            }
            else if (value == "null")
            {
                EmitToken(KotlinTokenType.Null, value);
            }
            else
            {
                EmitToken(KotlinTokenType.Keyword, value);
            }
        }
        else
        {
            EmitToken(KotlinTokenType.Identifier, value);
        }
    }

    private void EmitToken(KotlinTokenType type, string value)
    {
        _onToken(new KotlinToken(type, value));
    }

    private void EmitToken(KotlinTokenType type)
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
                EmitToken(KotlinTokenType.Whitespace);
            }
            else if (state.InIdentifier)
            {
                EmitIdentifierOrKeyword(_buffer.ToString());
                _buffer.Clear();
            }
            else if (state.InNumber)
            {
                EmitToken(KotlinTokenType.Number);
            }
            else if (state.InString)
            {
                EmitToken(KotlinTokenType.StringValue);
            }
            else if (state.InChar)
            {
                EmitToken(KotlinTokenType.CharValue);
            }
            else if (state.InCommentLine || state.InCommentBlock)
            {
                EmitToken(KotlinTokenType.Comment);
            }
            else if (state.InOperator)
            {
                EmitToken(KotlinTokenType.Operator);
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
    }
}
