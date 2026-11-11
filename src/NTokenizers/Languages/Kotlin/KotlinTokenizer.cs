using NTokenizers.Core;

namespace NTokenizers.Kotlin;

/// <summary>
/// Provides functionality for tokenizing Kotlin source code text.
/// </summary>
public sealed class KotlinTokenizer : BaseSubTokenizer<KotlinToken>
{
    /// <summary>
    /// Creates a new instance of the <see cref="KotlinTokenizer"/> class.
    /// </summary>
    /// <returns>A new Kotlin tokenizer instance.</returns>
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
                await EmitToken(KotlinTokenType.Whitespace);
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
                await EmitToken(KotlinTokenType.Number);
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
            await EmitToken(KotlinTokenType.OpenParenthesis, "(");
        }
        else if (c == ')')
        {
            await EmitToken(KotlinTokenType.CloseParenthesis, ")");
        }
        else if (c == '{')
        {
            await EmitToken(KotlinTokenType.OpenBrace, "{");
        }
        else if (c == '}')
        {
            await EmitToken(KotlinTokenType.CloseBrace, "}");
        }
        else if (c == '[')
        {
            await EmitToken(KotlinTokenType.OpenBracket, "[");
        }
        else if (c == ']')
        {
            await EmitToken(KotlinTokenType.CloseBracket, "]");
        }
        else if (c == ',')
        {
            await EmitToken(KotlinTokenType.Comma, ",");
        }
        else if (c == ';')
        {
            await EmitToken(KotlinTokenType.SequenceTerminator, ";");
        }
        else if (c == ':')
        {
            int next = Peek();
            if (next == ':')
            {
                Read();
                await EmitToken(KotlinTokenType.DoubleColon, "::");
            }
            else
            {
                await EmitToken(KotlinTokenType.Colon, ":");
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
                await EmitToken(KotlinTokenType.Dot, ".");
            }
        }
        else if (c == '@')
        {
            await EmitToken(KotlinTokenType.At, "@");
        }
        else if (c == '#')
        {
            await EmitToken(KotlinTokenType.Pound, "#");
        }
        else if (c == '?')
        {
            await EmitToken(KotlinTokenType.QuestionMark, "?");
        }
        else if (IsOperatorChar(c))
        {
            state.InOperator = true;
            _buffer.Append(c);
        }
        else
        {
            await EmitToken(KotlinTokenType.NotDefined, c.ToString());
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
            await EmitToken(KotlinTokenType.StringValue);
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
            await EmitToken(KotlinTokenType.CharValue);
            ClearState(state);
        }
    }

    private async Task ProcessInCommentLine(char c, State state)
    {
        if (c == '\n' || c == '\r')
        {
            await EmitToken(KotlinTokenType.Comment);
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
                await EmitToken(KotlinTokenType.Comment);
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
            await EmitToken(KotlinTokenType.Operator);
            _buffer.Clear();
            ClearState(state);
        }
        else if (IsOperatorChar(c))
        {
            await EmitToken(KotlinTokenType.Operator, current);
            _buffer.Clear();
            _buffer.Append(c);
        }
        else
        {
            await EmitToken(KotlinTokenType.Operator, current);
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

    private async Task EmitIdentifierOrKeyword(string value)
    {
        if (Keywords.Contains(value))
        {
            if (value == "true" || value == "false")
            {
                await EmitToken(KotlinTokenType.Boolean, value);
            }
            else if (value == "null")
            {
                await EmitToken(KotlinTokenType.Null, value);
            }
            else
            {
                await EmitToken(KotlinTokenType.Keyword, value);
            }
        }
        else
        {
            await EmitToken(KotlinTokenType.Identifier, value);
        }
    }

    private async Task EmitToken(KotlinTokenType type, string value)
    {
        await _onToken(new KotlinToken(type, value));
    }

    private async Task EmitToken(KotlinTokenType type)
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
                await EmitToken(KotlinTokenType.Whitespace);
            }
            else if (state.InIdentifier)
            {
                await EmitIdentifierOrKeyword(_buffer.ToString());
                _buffer.Clear();
            }
            else if (state.InNumber)
            {
                await EmitToken(KotlinTokenType.Number);
            }
            else if (state.InString)
            {
                await EmitToken(KotlinTokenType.StringValue);
            }
            else if (state.InChar)
            {
                await EmitToken(KotlinTokenType.CharValue);
            }
            else if (state.InCommentLine || state.InCommentBlock)
            {
                await EmitToken(KotlinTokenType.Comment);
            }
            else if (state.InOperator)
            {
                await EmitToken(KotlinTokenType.Operator);
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
