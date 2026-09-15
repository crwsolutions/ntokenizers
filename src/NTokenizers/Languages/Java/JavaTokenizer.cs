using NTokenizers.Core;

namespace NTokenizers.Java;

/// <summary>
/// Provides functionality for tokenizing Java source code text.
/// </summary>
public sealed class JavaTokenizer : BaseSubTokenizer<JavaToken>
{
    /// <summary>
    /// Creates a new instance of the <see cref="JavaTokenizer"/> class.
    /// </summary>
    /// <returns>A new Java tokenizer instance.</returns>
    public static JavaTokenizer Create() => new();

    private static readonly HashSet<string> Keywords = new(StringComparer.Ordinal)
    {
        // Control flow
        "abstract", "assert", "break", "case", "catch", "continue", "default", "do",
        "else", "finally", "for", "if", "instanceof", "new", "return", "switch",
        "throw", "throws", "try", "while",

        // Types
        "boolean", "byte", "char", "double", "float", "int", "long", "short", "void",

        // Modifiers
        "class", "enum", "extends", "final", "implements", "import",
        "interface", "native", "package", "private", "protected", "public",
        "static", "strictfp", "super", "synchronized", "this", "transient", "volatile",

        // Literals
        "false", "null", "true",

        // Others
        "const", "goto",
        "var",
        "record", "sealed", "permits", "yield", "non-sealed",
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

        if (state.InWhitespace)
        {
            if (char.IsWhiteSpace(c))
            {
                _buffer.Append(c);
                return;
            }
            else
            {
                EmitToken(JavaTokenType.Whitespace);
                ClearState(state);
                ProcessChar(c, state, ref escape);
                return;
            }
        }

        if (state.InIdentifier)
        {
            if (char.IsLetterOrDigit(c) || c == '_' || c == '$')
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
                c == 'L' || c == 'l' || c == 'F' || c == 'f' || c == 'D' || c == 'd')
            {
                _buffer.Append(c);
                return;
            }
            else
            {
                EmitToken(JavaTokenType.Number);
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
        else if (char.IsLetter(c) || c == '_' || c == '$')
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
            EmitToken(JavaTokenType.OpenParenthesis, "(");
        }
        else if (c == ')')
        {
            EmitToken(JavaTokenType.CloseParenthesis, ")");
        }
        else if (c == '{')
        {
            EmitToken(JavaTokenType.OpenBrace, "{");
        }
        else if (c == '}')
        {
            EmitToken(JavaTokenType.CloseBrace, "}");
        }
        else if (c == '[')
        {
            EmitToken(JavaTokenType.OpenBracket, "[");
        }
        else if (c == ']')
        {
            EmitToken(JavaTokenType.CloseBracket, "]");
        }
        else if (c == ',')
        {
            EmitToken(JavaTokenType.Comma, ",");
        }
        else if (c == ';')
        {
            EmitToken(JavaTokenType.SequenceTerminator, ";");
        }
        else if (c == ':')
        {
            EmitToken(JavaTokenType.Colon, ":");
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
                EmitToken(JavaTokenType.Dot, ".");
            }
        }
        else if (IsOperatorChar(c))
        {
            state.InOperator = true;
            _buffer.Append(c);
        }
        else
        {
            EmitToken(JavaTokenType.NotDefined, c.ToString());
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
            EmitToken(JavaTokenType.StringValue);
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
            EmitToken(JavaTokenType.CharValue);
            ClearState(state);
        }
    }

    private void ProcessInCommentLine(char c, State state)
    {
        if (c == '\n' || c == '\r')
        {
            EmitToken(JavaTokenType.Comment);
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
                EmitToken(JavaTokenType.Comment);
                ClearState(state);
                return;
            }
        }
        // If not closing, continue accumulating
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
            EmitToken(JavaTokenType.Operator);
            _buffer.Clear();
            ClearState(state);
        }
        else if (IsOperatorChar(c))
        {
            EmitToken(JavaTokenType.Operator, current);
            _buffer.Clear();
            _buffer.Append(c);
        }
        else
        {
            EmitToken(JavaTokenType.Operator, current);
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
               c == '~' || c == '?' || c == ':' || c == '@';
    }

    private static bool IsMultiCharOperator(string op)
    {
        return op == "==" || op == "!=" || op == "<=" || op == ">=" || op == "&&" || op == "||" ||
               op == "++" || op == "--" || op == "+=" || op == "-=" || op == "*=" || op == "/=" ||
               op == "%=" || op == "&=" || op == "|=" || op == "^=" || op == "<<" || op == ">>" ||
               op == ">>>" || op == "?: " || op == "->" || op == "?." || op == "?.[" ||
               op == "!" || op == "&" || op == "|" || op == "^" || op == "~" ||
               op == "+" || op == "-" || op == "*" || op == "/" || op == "%" ||
               op == "<" || op == ">" || op == "=" || op == "@";
    }

    private void EmitIdentifierOrKeyword(string value)
    {
        if (Keywords.Contains(value))
        {
            if (value == "true" || value == "false")
            {
                EmitToken(JavaTokenType.Boolean, value);
            }
            else if (value == "null")
            {
                EmitToken(JavaTokenType.Null, value);
            }
            else
            {
                EmitToken(JavaTokenType.Keyword, value);
            }
        }
        else
        {
            EmitToken(JavaTokenType.Identifier, value);
        }
    }

    private void EmitToken(JavaTokenType type, string value)
    {
        _onToken(new JavaToken(type, value));
    }

    private void EmitToken(JavaTokenType type)
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
                EmitToken(JavaTokenType.Whitespace);
            }
            else if (state.InIdentifier)
            {
                EmitIdentifierOrKeyword(_buffer.ToString());
                _buffer.Clear();
            }
            else if (state.InNumber)
            {
                EmitToken(JavaTokenType.Number);
            }
            else if (state.InString)
            {
                EmitToken(JavaTokenType.StringValue);
            }
            else if (state.InChar)
            {
                EmitToken(JavaTokenType.CharValue);
            }
            else if (state.InCommentLine || state.InCommentBlock)
            {
                EmitToken(JavaTokenType.Comment);
            }
            else if (state.InOperator)
            {
                EmitToken(JavaTokenType.Operator);
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
