using NTokenizers.Core;

namespace NTokenizers.Cpp;

/// <summary>
/// Provides functionality for tokenizing C++ source code text.
/// </summary>
public sealed class CppTokenizer : BaseSubTokenizer<CppToken>
{
    /// <summary>
    /// Creates a new instance of the <see cref="CppTokenizer"/> class.
    /// </summary>
    /// <returns>A new C++ tokenizer instance.</returns>
    public static CppTokenizer Create() => new();

    private static readonly HashSet<string> Keywords = new(StringComparer.Ordinal)
    {
        // C keywords
        "auto", "break", "case", "char", "const", "continue", "default", "do",
        "double", "else", "enum", "extern", "float", "for", "goto", "if",
        "int", "long", "register", "return", "short", "signed", "sizeof",
        "static", "struct", "switch", "typedef", "union", "unsigned", "void",
        "volatile", "while",
        
        // C++ specific
        "alignas", "alignof", "and", "and_eq", "asm", "bitand", "bitor",
        "bool", "catch", "char8_t", "char16_t", "char32_t", "class", "compl",
        "concept", "consteval", "constexpr", "constinit", "const_cast",
        "co_await", "co_return", "co_yield", "decltype", "delete", "dynamic_cast",
        "explicit", "export", "false", "final", "friend", "inline",
        "mutable", "namespace", "new", "noexcept", "not", "not_eq", "nullptr",
        "operator", "or", "or_eq", "override", "private", "protected", "public",
        "reinterpret_cast", "requires", "static_assert", "static_cast",
        "template", "this", "thread_local", "throw", "true", "try", "typeid",
        "typename", "using", "virtual", "wchar_t", "xor", "xor_eq",
        
        // C++20/23
        "module", "import", "exportable", "transitive",
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
        public bool InPreprocessor;
    }

    /// <inheritdoc/>
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

        if (state.InPreprocessor)
        {
            ProcessInPreprocessor(c, state);
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
                EmitToken(CppTokenType.Whitespace);
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
                (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F') || c == '_' ||
                c == 'U' || c == 'L' || c == 'F')
            {
                _buffer.Append(c);
                return;
            }
            else
            {
                EmitToken(CppTokenType.Number);
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
            // Check for string/char prefix: L, u, U, u8
            int next = Peek();
            if ((c == 'L' || c == 'u' || c == 'U') && (next == '"' || next == '\''))
            {
                _buffer.Append(c);
                Read();
                if (next == '"')
                {
                    _buffer.Append('"');
                    state.InString = true;
                }
                else
                {
                    _buffer.Append('\'');
                    state.InChar = true;
                }
            }
            else if (c == 'u' && next == '8')
            {
                _buffer.Append(c);
                Read();
                int next2 = Peek();
                if (next2 == '"')
                {
                    _buffer.Append("u8\"");
                    Read();
                    state.InString = true;
                }
                else if (next2 == '\'')
                {
                    _buffer.Append("u8'");
                    Read();
                    state.InChar = true;
                }
                else
                {
                    state.InIdentifier = true;
                }
            }
            else
            {
                state.InIdentifier = true;
                _buffer.Append(c);
            }
        }
        else if (char.IsDigit(c))
        {
            state.InNumber = true;
            _buffer.Append(c);
        }
        else if (c == '#')
        {
            state.InPreprocessor = true;
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
            EmitToken(CppTokenType.OpenParenthesis, "(");
        }
        else if (c == ')')
        {
            EmitToken(CppTokenType.CloseParenthesis, ")");
        }
        else if (c == '{')
        {
            EmitToken(CppTokenType.OpenBrace, "{");
        }
        else if (c == '}')
        {
            EmitToken(CppTokenType.CloseBrace, "}");
        }
        else if (c == '[')
        {
            EmitToken(CppTokenType.OpenBracket, "[");
        }
        else if (c == ']')
        {
            EmitToken(CppTokenType.CloseBracket, "]");
        }
        else if (c == ',')
        {
            EmitToken(CppTokenType.Comma, ",");
        }
        else if (c == ';')
        {
            EmitToken(CppTokenType.SequenceTerminator, ";");
        }
        else if (c == ':')
        {
            int next = Peek();
            if (next == ':')
            {
                Read();
                EmitToken(CppTokenType.DoubleColon, "::");
            }
            else
            {
                EmitToken(CppTokenType.Colon, ":");
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
                EmitToken(CppTokenType.Dot, ".");
            }
        }
        else if (IsOperatorChar(c))
        {
            state.InOperator = true;
            _buffer.Append(c);
        }
        else
        {
            EmitToken(CppTokenType.NotDefined, c.ToString());
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
            EmitToken(CppTokenType.StringValue);
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
            EmitToken(CppTokenType.CharValue);
            ClearState(state);
        }
    }

    private void ProcessInCommentLine(char c, State state)
    {
        if (c == '\n' || c == '\r')
        {
            EmitToken(CppTokenType.Comment);
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
                EmitToken(CppTokenType.Comment);
                ClearState(state);
                return;
            }
        }
    }

    private void ProcessInPreprocessor(char c, State state)
    {
        if (c == '\n' || c == '\r')
        {
            EmitToken(CppTokenType.Keyword);
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
            EmitToken(CppTokenType.Operator);
            _buffer.Clear();
            ClearState(state);
        }
        else if (IsOperatorChar(c))
        {
            EmitToken(CppTokenType.Operator, current);
            _buffer.Clear();
            _buffer.Append(c);
        }
        else
        {
            EmitToken(CppTokenType.Operator, current);
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
               op == "?: " || op == "->" || op == "?." || op == ".*" || op == "->*" ||
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
                EmitToken(CppTokenType.Boolean, value);
            }
            else if (value == "nullptr")
            {
                EmitToken(CppTokenType.Null, value);
            }
            else
            {
                EmitToken(CppTokenType.Keyword, value);
            }
        }
        else
        {
            EmitToken(CppTokenType.Identifier, value);
        }
    }

    private void EmitToken(CppTokenType type, string value)
    {
        _onToken(new CppToken(type, value));
    }

    private void EmitToken(CppTokenType type)
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
                EmitToken(CppTokenType.Whitespace);
            }
            else if (state.InIdentifier)
            {
                EmitIdentifierOrKeyword(_buffer.ToString());
                _buffer.Clear();
            }
            else if (state.InNumber)
            {
                EmitToken(CppTokenType.Number);
            }
            else if (state.InString)
            {
                EmitToken(CppTokenType.StringValue);
            }
            else if (state.InChar)
            {
                EmitToken(CppTokenType.CharValue);
            }
            else if (state.InCommentLine || state.InCommentBlock)
            {
                EmitToken(CppTokenType.Comment);
            }
            else if (state.InPreprocessor)
            {
                EmitToken(CppTokenType.Keyword);
            }
            else if (state.InOperator)
            {
                EmitToken(CppTokenType.Operator);
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
        state.InPreprocessor = false;
    }
}
