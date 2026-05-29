using NTokenizers.Core;

namespace NTokenizers.C;

/// <summary>
/// Provides functionality for tokenizing C source code text.
/// </summary>
public sealed class CTokenizer : BaseSubTokenizer<CToken>
{
    /// <summary>
    /// Creates a new instance of the <see cref="CTokenizer"/> class.
    /// </summary>
    public static CTokenizer Create() => new();

    private static readonly HashSet<string> Keywords = new(StringComparer.Ordinal)
    {
        // Types and control flow
        "auto", "break", "case", "char", "const", "continue", "default", "do",
        "double", "else", "enum", "extern", "float", "for", "goto", "if",
        "inline", "int", "long", "register", "restrict", "return", "short",
        "signed", "sizeof", "static", "struct", "switch", "typedef", "union",
        "unsigned", "void", "volatile", "while",
        
        // C99
        "_Bool", "_Complex", "_Imaginary", "_Alignas", "_Alignof",
        "_Atomic", "_Noreturn", "_Static_assert", "_Thread_local",
        "bool", "complex", "imaginary",
        
        // C11
        "_Generic", "_Pragma",
        
        // C23
        "_BitInt", "_Bitalign", "_Bitwidth", "_Decimal128", "_Decimal32", "_Decimal64",
        "_Decimal128x", "_Decimal32x", "_Decimal64x", "_Float128", "_Float16",
        "_Float32", "_Float32x", "_Float64", "_Float64x", "_Float80",
        "_Float8", "_Float80x", "_Float16x", "_Math", "_Prior",
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
                EmitToken(CTokenType.Whitespace);
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
                EmitToken(CTokenType.Number);
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

        // Start state - process new character
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
                Read(); // consume second /
                state.InCommentLine = true;
                _buffer.Append("//");
            }
            else if (next == '*')
            {
                Read(); // consume *
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
            EmitToken(CTokenType.OpenParenthesis, "(");
        }
        else if (c == ')')
        {
            EmitToken(CTokenType.CloseParenthesis, ")");
        }
        else if (c == '{')
        {
            EmitToken(CTokenType.OpenBrace, "{");
        }
        else if (c == '}')
        {
            EmitToken(CTokenType.CloseBrace, "}");
        }
        else if (c == '[')
        {
            EmitToken(CTokenType.OpenBracket, "[");
        }
        else if (c == ']')
        {
            EmitToken(CTokenType.CloseBracket, "]");
        }
        else if (c == ',')
        {
            EmitToken(CTokenType.Comma, ",");
        }
        else if (c == ';')
        {
            EmitToken(CTokenType.SequenceTerminator, ";");
        }
        else if (c == ':')
        {
            EmitToken(CTokenType.Colon, ":");
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
                EmitToken(CTokenType.Dot, ".");
            }
        }
        else if (IsOperatorChar(c))
        {
            state.InOperator = true;
            _buffer.Append(c);
        }
        else
        {
            EmitToken(CTokenType.NotDefined, c.ToString());
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
            EmitToken(CTokenType.StringValue);
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
            EmitToken(CTokenType.CharValue);
            ClearState(state);
        }
    }

    private void ProcessInCommentLine(char c, State state)
    {
        if (c == '\n' || c == '\r')
        {
            EmitToken(CTokenType.Comment);
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
                EmitToken(CTokenType.Comment);
                ClearState(state);
                return;
            }
        }
    }

    private void ProcessInPreprocessor(char c, State state)
    {
        if (c == '\n' || c == '\r')
        {
            EmitToken(CTokenType.Preprocessor);
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
            EmitToken(CTokenType.Operator);
            _buffer.Clear();
            ClearState(state);
        }
        else if (IsOperatorChar(c))
        {
            EmitToken(CTokenType.Operator, current);
            _buffer.Clear();
            _buffer.Append(c);
        }
        else
        {
            EmitToken(CTokenType.Operator, current);
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
               c == '~' || c == '?' || c == ':';
    }

    private static bool IsMultiCharOperator(string op)
    {
        return op == "==" || op == "!=" || op == "<=" || op == ">=" || op == "&&" || op == "||" ||
               op == "++" || op == "--" || op == "+=" || op == "-=" || op == "*=" || op == "/=" ||
               op == "%=" || op == "&=" || op == "|=" || op == "^=" || op == "<<" || op == ">>" ||
               op == "?: " || op == "->" || op == "?." ||
               op == "!" || op == "&" || op == "|" || op == "^" || op == "~" ||
               op == "+" || op == "-" || op == "*" || op == "/" || op == "%" ||
               op == "<" || op == ">" || op == "=";
    }

    private void EmitIdentifierOrKeyword(string value)
    {
        if (Keywords.Contains(value))
        {
            EmitToken(CTokenType.Keyword, value);
        }
        else
        {
            EmitToken(CTokenType.Identifier, value);
        }
    }

    private void EmitToken(CTokenType type, string value)
    {
        _onToken(new CToken(type, value));
    }

    private void EmitToken(CTokenType type)
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
                EmitToken(CTokenType.Whitespace);
            }
            else if (state.InIdentifier)
            {
                EmitIdentifierOrKeyword(_buffer.ToString());
                _buffer.Clear();
            }
            else if (state.InNumber)
            {
                EmitToken(CTokenType.Number);
            }
            else if (state.InString)
            {
                EmitToken(CTokenType.StringValue);
            }
            else if (state.InChar)
            {
                EmitToken(CTokenType.CharValue);
            }
            else if (state.InCommentLine || state.InCommentBlock)
            {
                EmitToken(CTokenType.Comment);
            }
            else if (state.InPreprocessor)
            {
                EmitToken(CTokenType.Preprocessor);
            }
            else if (state.InOperator)
            {
                EmitToken(CTokenType.Operator);
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
