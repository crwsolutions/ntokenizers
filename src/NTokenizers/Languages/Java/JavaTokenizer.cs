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
                await EmitToken(JavaTokenType.Whitespace);
                ClearState(state);
                await ProcessChar(c, state);
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
                (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F') || c == '_' ||
                c == 'L' || c == 'l' || c == 'F' || c == 'f' || c == 'D' || c == 'd')
            {
                _buffer.Append(c);
                return;
            }
            else
            {
                await EmitToken(JavaTokenType.Number);
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
            await EmitToken(JavaTokenType.OpenParenthesis, "(");
        }
        else if (c == ')')
        {
            await EmitToken(JavaTokenType.CloseParenthesis, ")");
        }
        else if (c == '{')
        {
            await EmitToken(JavaTokenType.OpenBrace, "{");
        }
        else if (c == '}')
        {
            await EmitToken(JavaTokenType.CloseBrace, "}");
        }
        else if (c == '[')
        {
            await EmitToken(JavaTokenType.OpenBracket, "[");
        }
        else if (c == ']')
        {
            await EmitToken(JavaTokenType.CloseBracket, "]");
        }
        else if (c == ',')
        {
            await EmitToken(JavaTokenType.Comma, ",");
        }
        else if (c == ';')
        {
            await EmitToken(JavaTokenType.SequenceTerminator, ";");
        }
        else if (c == ':')
        {
            await EmitToken(JavaTokenType.Colon, ":");
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
                await EmitToken(JavaTokenType.Dot, ".");
            }
        }
        else if (IsOperatorChar(c))
        {
            state.InOperator = true;
            _buffer.Append(c);
        }
        else
        {
            await EmitToken(JavaTokenType.NotDefined, c.ToString());
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
            await EmitToken(JavaTokenType.StringValue);
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
            await EmitToken(JavaTokenType.CharValue);
            ClearState(state);
        }
    }

    private async Task ProcessInCommentLine(char c, State state)
    {
        if (c == '\n' || c == '\r')
        {
            await EmitToken(JavaTokenType.Comment);
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
                await EmitToken(JavaTokenType.Comment);
                ClearState(state);
                return;
            }
        }
        // If not closing, continue accumulating
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
            await EmitToken(JavaTokenType.Operator);
            _buffer.Clear();
            ClearState(state);
        }
        else if (IsOperatorChar(c))
        {
            await EmitToken(JavaTokenType.Operator, current);
            _buffer.Clear();
            _buffer.Append(c);
        }
        else
        {
            await EmitToken(JavaTokenType.Operator, current);
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

    private async Task EmitIdentifierOrKeyword(string value)
    {
        if (Keywords.Contains(value))
        {
            if (value == "true" || value == "false")
            {
                await EmitToken(JavaTokenType.Boolean, value);
            }
            else if (value == "null")
            {
                await EmitToken(JavaTokenType.Null, value);
            }
            else
            {
                await EmitToken(JavaTokenType.Keyword, value);
            }
        }
        else
        {
            await EmitToken(JavaTokenType.Identifier, value);
        }
    }

    private async Task EmitToken(JavaTokenType type, string value)
    {
        await _onToken(new JavaToken(type, value));
    }

    private async Task EmitToken(JavaTokenType type)
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
                await EmitToken(JavaTokenType.Whitespace);
            }
            else if (state.InIdentifier)
            {
                await EmitIdentifierOrKeyword(_buffer.ToString());
                _buffer.Clear();
            }
            else if (state.InNumber)
            {
                await EmitToken(JavaTokenType.Number);
            }
            else if (state.InString)
            {
                await EmitToken(JavaTokenType.StringValue);
            }
            else if (state.InChar)
            {
                await EmitToken(JavaTokenType.CharValue);
            }
            else if (state.InCommentLine || state.InCommentBlock)
            {
                await EmitToken(JavaTokenType.Comment);
            }
            else if (state.InOperator)
            {
                await EmitToken(JavaTokenType.Operator);
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
