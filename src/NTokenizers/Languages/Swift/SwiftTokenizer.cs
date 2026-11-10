using NTokenizers.Core;

namespace NTokenizers.Swift;

/// <summary>
/// Provides functionality for tokenizing Swift source code text.
/// </summary>
public sealed class SwiftTokenizer : BaseSubTokenizer<SwiftToken>
{
    /// <summary>
    /// Creates a new instance of the <see cref="SwiftTokenizer"/> class.
    /// </summary>
    /// <returns>A new Swift tokenizer instance.</returns>
    public static SwiftTokenizer Create() => new();

    private static readonly HashSet<string> Keywords = new(StringComparer.Ordinal)
    {
        "associatedtype", "class", "deinit", "enum", "extension", "fileprivate", "func", "import",
        "init", "internal", "let", "mutating", "nonmutating", "open", "operator", "postfix",
        "private", "prefix", "protocol", "public", "static", "struct", "subscript", "typealias", "var",
        "break", "case", "continue", "default", "defer", "do", "else", "fallthrough", "for", "guard",
        "if", "in", "repeat", "return", "switch", "where", "while",
        "as", "catch", "throw", "throws", "rethrows", "try",
        "true", "false", "nil", "self", "Self", "super", "is", "any", "some", "Type",
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
                await EmitToken(SwiftTokenType.Whitespace);
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
                await EmitToken(SwiftTokenType.Number);
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
            await EmitToken(SwiftTokenType.OpenParenthesis, "(");
        }
        else if (c == ')')
        {
            await EmitToken(SwiftTokenType.CloseParenthesis, ")");
        }
        else if (c == '{')
        {
            await EmitToken(SwiftTokenType.OpenBrace, "{");
        }
        else if (c == '}')
        {
            await EmitToken(SwiftTokenType.CloseBrace, "}");
        }
        else if (c == '[')
        {
            await EmitToken(SwiftTokenType.OpenBracket, "[");
        }
        else if (c == ']')
        {
            await EmitToken(SwiftTokenType.CloseBracket, "]");
        }
        else if (c == ',')
        {
            await EmitToken(SwiftTokenType.Comma, ",");
        }
        else if (c == ';')
        {
            await EmitToken(SwiftTokenType.SequenceTerminator, ";");
        }
        else if (c == ':')
        {
            int next = Peek();
            if (next == ':')
            {
                Read();
                await EmitToken(SwiftTokenType.DoubleColon, "::");
            }
            else
            {
                await EmitToken(SwiftTokenType.Colon, ":");
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
                await EmitToken(SwiftTokenType.Dot, ".");
            }
        }
        else if (c == '@')
        {
            await EmitToken(SwiftTokenType.At, "@");
        }
        else if (c == '#')
        {
            await EmitToken(SwiftTokenType.Pound, "#");
        }
        else if (c == '?')
        {
            await EmitToken(SwiftTokenType.QuestionMark, "?");
        }
        else if (IsOperatorChar(c))
        {
            state.InOperator = true;
            _buffer.Append(c);
        }
        else
        {
            await EmitToken(SwiftTokenType.NotDefined, c.ToString());
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
            await EmitToken(SwiftTokenType.StringValue);
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
            await EmitToken(SwiftTokenType.CharValue);
            ClearState(state);
        }
    }

    private async Task ProcessInCommentLine(char c, State state)
    {
        if (c == '\n' || c == '\r')
        {
            await EmitToken(SwiftTokenType.Comment);
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
                await EmitToken(SwiftTokenType.Comment);
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
            await EmitToken(SwiftTokenType.Operator);
            _buffer.Clear();
            ClearState(state);
        }
        else if (IsOperatorChar(c))
        {
            await EmitToken(SwiftTokenType.Operator, current);
            _buffer.Clear();
            _buffer.Append(c);
        }
        else
        {
            await EmitToken(SwiftTokenType.Operator, current);
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
               op == "->" || op == "?." || op == "??" ||
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
                await EmitToken(SwiftTokenType.Boolean, value);
            }
            else if (value == "nil")
            {
                await EmitToken(SwiftTokenType.Null, value);
            }
            else
            {
                await EmitToken(SwiftTokenType.Keyword, value);
            }
        }
        else
        {
            await EmitToken(SwiftTokenType.Identifier, value);
        }
    }

    private async Task EmitToken(SwiftTokenType type, string value)
    {
        await _onToken(new SwiftToken(type, value));
    }

    private async Task EmitToken(SwiftTokenType type)
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
                await EmitToken(SwiftTokenType.Whitespace);
            }
            else if (state.InIdentifier)
            {
                await EmitIdentifierOrKeyword(_buffer.ToString());
                _buffer.Clear();
            }
            else if (state.InNumber)
            {
                await EmitToken(SwiftTokenType.Number);
            }
            else if (state.InString)
            {
                await EmitToken(SwiftTokenType.StringValue);
            }
            else if (state.InChar)
            {
                await EmitToken(SwiftTokenType.CharValue);
            }
            else if (state.InCommentLine || state.InCommentBlock)
            {
                await EmitToken(SwiftTokenType.Comment);
            }
            else if (state.InOperator)
            {
                await EmitToken(SwiftTokenType.Operator);
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
