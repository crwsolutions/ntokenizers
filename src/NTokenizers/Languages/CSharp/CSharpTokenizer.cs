using NTokenizers.Core;

namespace NTokenizers.CSharp;

/// <summary>
/// Provides functionality for tokenizing C# source code text.
/// </summary>
public sealed class CSharpTokenizer : BaseSubTokenizer<CSharpToken>
{
    /// <summary>
    /// Creates a new instance of the <see cref="CSharpTokenizer"/> class.
    /// </summary>
    public static CSharpTokenizer Create() => new();

    private static readonly HashSet<string> Keywords = new(StringComparer.Ordinal)
    {
        "abstract", "as", "async", "await", "base", "bool", "break", "byte", "case", "catch", "char", "var",
        "checked", "class", "const", "continue", "decimal", "default", "delegate", "or", "and",
        "do", "double", "else", "enum", "event", "explicit", "extern", "false",
        "finally", "fixed", "float", "for", "foreach", "goto", "if", "implicit",
        "in", "int", "interface", "internal", "is", "lock", "long", "namespace",
        "new", "null", "object", "operator", "out", "override", "params", "private",
        "protected", "public", "readonly", "ref", "return", "sbyte", "sealed",
        "short", "sizeof", "stackalloc", "static", "string", "struct", "switch",
        "this", "throw", "true", "try", "typeof", "uint", "ulong", "unchecked",
        "unsafe", "ushort", "using", "virtual", "void", "volatile", "while"
    };

    private enum ParseState
    {
        Start,
        InWhitespace,
        InIdentifier,
        InNumber,
        InString,
        InVerbatimString,
        InCommentLine,
        InCommentBlock,
        InOperator
    }

    private sealed class State
    {
        public ParseState Current = ParseState.Start;
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
        switch (state.Current)
        {
            case ParseState.InString:
                await ProcessInString(c, state);
                return;

            case ParseState.InVerbatimString:
                await ProcessInVerbatimString(c, state);
                return;

            case ParseState.InCommentLine:
                await ProcessInCommentLine(c, state);
                return;

            case ParseState.InCommentBlock:
                await ProcessInCommentBlock(c, state);
                return;

            case ParseState.InWhitespace:
                if (char.IsWhiteSpace(c))
                {
                    _buffer.Append(c);
                    return;
                }
                else
                {
                    await EmitToken(CSharpTokenType.Whitespace);
                    state.Current = ParseState.Start;
                    // Fall through to process current character
                }
                break;

            case ParseState.InIdentifier:
                if (char.IsLetterOrDigit(c) || c == '_')
                {
                    _buffer.Append(c);
                    return;
                }
                else
                {
                    await EmitIdentifierOrKeyword(_buffer.ToString());
                    _buffer.Clear();
                    state.Current = ParseState.Start;
                    // Fall through to process current character
                }
                break;

            case ParseState.InNumber:
                if (char.IsDigit(c) || c == '.' || c == 'e' || c == 'E' || c == '-' || c == '+' || c == 'x' || c == 'X' ||
                    (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F') || c == '_')
                {
                    _buffer.Append(c);
                    return;
                }
                else
                {
                    await EmitToken(CSharpTokenType.Number);
                    state.Current = ParseState.Start;
                    // Fall through to process current character
                }
                break;

            case ParseState.InOperator:
                await ProcessInOperator(c, state);
                return;
        }

        // ParseState.Start - process new character
        if (char.IsWhiteSpace(c))
        {
            state.Current = ParseState.InWhitespace;
            _buffer.Append(c);
        }
        else if (c == '"')
        {
            state.Current = ParseState.InString;
            _buffer.Append(c);
        }
        else if (c == '@')
        {
            // Check if next character is a quote for verbatim string
            int next = Peek();
            if (next == '"')
            {
                _buffer.Append(c);
                _buffer.Append((char)Read());
                state.Current = ParseState.InVerbatimString;
            }
            else
            {
                // @ can be part of an identifier
                state.Current = ParseState.InIdentifier;
                _buffer.Append(c);
            }
        }
        else if (char.IsLetter(c) || c == '_')
        {
            state.Current = ParseState.InIdentifier;
            _buffer.Append(c);
        }
        else if (char.IsDigit(c))
        {
            state.Current = ParseState.InNumber;
            _buffer.Append(c);
        }
        else if (c == '/')
        {
            int next = Peek();
            if (next == '/')
            {
                Read(); // consume second /
                state.Current = ParseState.InCommentLine;
                _buffer.Append("//");
            }
            else if (next == '*')
            {
                Read(); // consume *
                state.Current = ParseState.InCommentBlock;
                _buffer.Append("/*");
            }
            else
            {
                state.Current = ParseState.InOperator;
                _buffer.Append(c);
            }
        }
        else if (c == '(' || c == ')' || c == ',' || c == ';')
        {
            await EmitPunctuation(c);
        }
        else if (c == '.')
        {
            // Check if it's part of a number or a dot operator
            int next = Peek();
            if (next != -1 && char.IsDigit((char)next))
            {
                state.Current = ParseState.InNumber;
                _buffer.Append(c);
            }
            else
            {
                await EmitToken(CSharpTokenType.Dot, ".");
            }
        }
        else if (IsOperatorChar(c))
        {
            state.Current = ParseState.InOperator;
            _buffer.Append(c);
        }
        else
        {
            // Unknown character - emit as NotDefined
            await EmitToken(CSharpTokenType.NotDefined, c.ToString());
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
            await EmitToken(CSharpTokenType.StringValue);
            state.Current = ParseState.Start;
        }
    }

    private async Task ProcessInVerbatimString(char c, State state)
    {
        _buffer.Append(c);
        if (c == '"')
        {
            // Check if it's an escaped quote (double quote)
            int next = Peek();
            if (next == '"')
            {
                _buffer.Append((char)Read());
            }
            else
            {
                // End of verbatim string
                await EmitToken(CSharpTokenType.StringValue);
                state.Current = ParseState.Start;
            }
        }
    }

    private async Task ProcessInCommentLine(char c, State state)
    {
        if (c == '\n' || c == '\r')
        {
            await EmitToken(CSharpTokenType.Comment);
            state.Current = ParseState.InWhitespace;
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
                await EmitToken(CSharpTokenType.Comment);
                state.Current = ParseState.Start;
            }
        }
    }

    private async Task ProcessInOperator(char c, State state)
    {
        string current = _buffer.ToString();
        string combined = current + c;

        // Check if the combined string forms a valid multi-character operator
        if (IsMultiCharOperator(combined))
        {
            _buffer.Append(c);
            // Check if we can form an even longer operator
            int next = Peek();
            if (next != -1)
            {
                string further = combined + (char)next;
                if (IsMultiCharOperator(further))
                {
                    return; // Continue building the operator
                }
            }
            // Emit the operator
            await EmitOperatorToken(_buffer.ToString());
            _buffer.Clear();
            state.Current = ParseState.Start;
        }
        else if (IsOperatorChar(c))
        {
            // Emit the current operator and start a new one
            await EmitOperatorToken(current);
            _buffer.Clear();
            _buffer.Append(c);
        }
        else
        {
            // Emit the current operator and process this character
            await EmitOperatorToken(current);
            _buffer.Clear();
            state.Current = ParseState.Start;
            state.Escape = false;
            await ProcessChar(c, state);
        }
    }

    private static bool IsOperatorChar(char c)
    {
        return c == '=' || c == '!' || c == '<' || c == '>' || c == '+' || c == '-' ||
               c == '*' || c == '/' || c == '%' || c == '&' || c == '|' || c == '^' ||
               c == '~' || c == '?' || c == ':' || c == '[' || c == ']' || c == '{' || c == '}';
    }

    private static bool IsMultiCharOperator(string op)
    {
        return op == "==" || op == "!=" || op == "<=" || op == ">=" || op == "&&" || op == "||" ||
               op == "++" || op == "--" || op == "+=" || op == "-=" || op == "*=" || op == "/=" ||
               op == "%=" || op == "&=" || op == "|=" || op == "^=" || op == "<<" || op == ">>" ||
               op == "??" || op == "?." || op == "?[" || op == "=>" || op == "->>" || op == "<<=" || op == ">>=";
    }

    private async Task EmitOperatorToken(string value)
    {
        CSharpTokenType type = value switch
        {
            "==" => CSharpTokenType.Equals,
            "!=" => CSharpTokenType.NotEquals,
            ">" => CSharpTokenType.GreaterThan,
            "<" => CSharpTokenType.LessThan,
            ">=" => CSharpTokenType.GreaterThanOrEqual,
            "<=" => CSharpTokenType.LessThanOrEqual,
            "&&" => CSharpTokenType.And,
            "||" => CSharpTokenType.Or,
            "!" => CSharpTokenType.Not,
            "+" => CSharpTokenType.Plus,
            "-" => CSharpTokenType.Minus,
            "*" => CSharpTokenType.Multiply,
            "/" => CSharpTokenType.Divide,
            "%" => CSharpTokenType.Modulo,
            _ => CSharpTokenType.Operator
        };
        await EmitToken(type, value);
    }

    private async Task EmitPunctuation(char c)
    {
        CSharpTokenType type = c switch
        {
            '(' => CSharpTokenType.OpenParenthesis,
            ')' => CSharpTokenType.CloseParenthesis,
            ',' => CSharpTokenType.Comma,
            ';' => CSharpTokenType.SequenceTerminator,
            _ => CSharpTokenType.NotDefined
        };
        await EmitToken(type, c.ToString());
    }

    private async Task EmitIdentifierOrKeyword(string value)
    {
        if (Keywords.Contains(value))
        {
            await EmitToken(CSharpTokenType.Keyword, value);
        }
        else
        {
            await EmitToken(CSharpTokenType.Identifier, value);
        }
    }

    private async Task EmitToken(CSharpTokenType type, string value)
    {
        await _onToken(new CSharpToken(type, value));
    }

    private async Task EmitToken(CSharpTokenType type)
    {
        await EmitToken(type, _buffer.ToString());
        _buffer.Clear();
    }

    private async Task EmitPending(State state)
    {
        if (_buffer.Length > 0)
        {
            switch (state.Current)
            {
                case ParseState.InWhitespace:
                    await EmitToken(CSharpTokenType.Whitespace);
                    break;
                case ParseState.InIdentifier:
                    await EmitIdentifierOrKeyword(_buffer.ToString());
                    _buffer.Clear();
                    break;
                case ParseState.InNumber:
                    await EmitToken(CSharpTokenType.Number);
                    break;
                case ParseState.InString:
                case ParseState.InVerbatimString:
                    // Incomplete string - could emit as NotDefined or StringValue
                    await EmitToken(CSharpTokenType.StringValue);
                    break;
                case ParseState.InCommentLine:
                case ParseState.InCommentBlock:
                    await EmitToken(CSharpTokenType.Comment);
                    break;
                case ParseState.InOperator:
                    await EmitOperatorToken(_buffer.ToString());
                    _buffer.Clear();
                    break;
            }
        }
        state.Current = ParseState.Start;
    }
}
