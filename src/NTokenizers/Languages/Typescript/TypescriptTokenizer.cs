using NTokenizers.Core;

namespace NTokenizers.Typescript;

/// <summary>
/// Provides functionality for tokenizing TypeScript text sources character by character using a state machine.
/// </summary>
public class TypescriptTokenizer : BaseSubTokenizer<TypescriptToken>
{
    /// <summary>
    /// Creates a new instance of the <see cref="TypescriptTokenizer"/> class.
    /// </summary>
    public static TypescriptTokenizer Create() => new();

    private static readonly HashSet<string> Keywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "abstract", "arguments", "async", "await", "boolean", "break", "byte",
        "case", "catch", "char", "class", "const", "continue", "constructor",
        "debugger", "declare", "default", "delete", "do", "double", "else",
        "enum", "eval", "export", "extends", "false", "final", "finally", "float",
        "for", "from", "function", "get", "global", "goto", "if", "implements",
        "import", "in", "infer", "instanceof", "interface", "is", "keyof", "let",
        "module", "namespace", "native", "new", "null", "number", "object", "of",
        "package", "private", "protected", "public", "readonly", "require", "return",
        "set", "short", "static", "string", "super", "switch", "symbol", "satisfies",
        "this", "throw", "true", "try", "type", "typeof", "undefined", "unique",
        "unknown", "var", "void", "volatile", "while", "with", "yield",
        "never", "asserts"
    };

    /// <summary>
    /// Parses TypeScript content from the given <see cref="TextReader"/> and produces a sequence of <see cref="TypescriptToken"/> objects.
    /// </summary>
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
            case ParseState.Start:
                await ProcessStart(c, state);
                break;
            case ParseState.InWhitespace:
                await ProcessWhitespace(c, state);
                break;
            case ParseState.InIdentifier:
                await ProcessIdentifier(c, state);
                break;
            case ParseState.InNumber:
                await ProcessNumber(c, state);
                break;
            case ParseState.InString:
                await ProcessString(c, state);
                break;
            case ParseState.InStringEscape:
                await ProcessStringEscape(c, state);
                break;
            case ParseState.InCommentLine:
                await ProcessCommentLine(c, state);
                break;
            case ParseState.InCommentBlock:
                await ProcessCommentBlock(c, state);
                break;
            case ParseState.InCommentBlockStar:
                await ProcessCommentBlockStar(c, state);
                break;
            case ParseState.InOperator:
                await ProcessOperator(c, state);
                break;
        }
    }

    private async Task ProcessStart(char c, State state)
    {
        if (char.IsWhiteSpace(c))
        {
            _buffer.Append(c);
            state.Current = ParseState.InWhitespace;
        }
        else if (c == '\'' || c == '"' || c == '`')
        {
            state.StringDelimiter = c;
            _buffer.Append(c);
            state.Current = ParseState.InString;
        }
        else if (char.IsLetter(c) || c == '_' || c == '$')
        {
            _buffer.Append(c);
            state.Current = ParseState.InIdentifier;
        }
        else if (char.IsDigit(c))
        {
            _buffer.Append(c);
            state.Current = ParseState.InNumber;
        }
        else if (c == '/' || IsOperatorChar(c))
        {
            _buffer.Append(c);
            state.Current = ParseState.InOperator;
        }
        else if (c == '(')
        {
            await _onToken(new TypescriptToken(TypescriptTokenType.OpenParenthesis, "("));
        }
        else if (c == ')')
        {
            await _onToken(new TypescriptToken(TypescriptTokenType.CloseParenthesis, ")"));
        }
        else if (c == ',')
        {
            await _onToken(new TypescriptToken(TypescriptTokenType.Comma, ","));
        }
        else if (c == '.')
        {
            // Dot could be part of ?. operator, but here it's standalone
            await _onToken(new TypescriptToken(TypescriptTokenType.Dot, "."));
        }
        else if (c == ';')
        {
            await _onToken(new TypescriptToken(TypescriptTokenType.SequenceTerminator, ";"));
        }
        else if (c == '{' || c == '}' || c == '[' || c == ']' || c == ':')
        {
            // Other punctuation - emit as operator or notdefined
            await _onToken(new TypescriptToken(TypescriptTokenType.Operator, c.ToString()));
        }
        else
        {
            await _onToken(new TypescriptToken(TypescriptTokenType.NotDefined, c.ToString()));
        }
    }

    private async Task ProcessWhitespace(char c, State state)
    {
        if (char.IsWhiteSpace(c))
        {
            _buffer.Append(c);
        }
        else
        {
            await _onToken(new TypescriptToken(TypescriptTokenType.Whitespace, _buffer.ToString()));
            _buffer.Clear();
            state.Current = ParseState.Start;
            await ProcessStart(c, state);
        }
    }

    private async Task ProcessIdentifier(char c, State state)
    {
        if (char.IsLetterOrDigit(c) || c == '_' || c == '$')
        {
            _buffer.Append(c);
        }
        else
        {
            string identifier = _buffer.ToString();
            _buffer.Clear();

            // Check if it's a keyword (case-insensitive)
            if (Keywords.Contains(identifier))
            {
                await _onToken(new TypescriptToken(TypescriptTokenType.Keyword, identifier));
            }
            else
            {
                await _onToken(new TypescriptToken(TypescriptTokenType.Identifier, identifier));
            }

            state.Current = ParseState.Start;
            await ProcessStart(c, state);
        }
    }

    private async Task ProcessNumber(char c, State state)
    {
        if (char.IsDigit(c) || c == '.' || c == 'e' || c == 'E' || c == '-' || c == '+' || c == 'x' || c == 'X' ||
            (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F') || c == 'T' || c == ':')
        {
            _buffer.Append(c);

            // Check if this looks like a datetime (simple heuristic: contains 'T' or multiple colons)
            string current = _buffer.ToString();
            if (current.Contains('T') || current.Count(ch => ch == ':') >= 2 || current.Count(ch => ch == '-') >= 2)
            {
                // Continue building as potential datetime
            }
        }
        else
        {
            string number = _buffer.ToString();
            _buffer.Clear();

            // Determine if it's a datetime or a number
            TypescriptTokenType tokenType = TypescriptTokenType.Number;
            if (number.Contains('T') || number.Count(ch => ch == ':') >= 2 ||
                (number.Count(ch => ch == '-') >= 2 && !number.StartsWith("-")))
            {
                tokenType = TypescriptTokenType.DateTimeValue;
            }

            await _onToken(new TypescriptToken(tokenType, number));
            state.Current = ParseState.Start;
            await ProcessStart(c, state);
        }
    }

    private async Task ProcessString(char c, State state)
    {
        _buffer.Append(c);

        if (c == '\\')
        {
            state.Current = ParseState.InStringEscape;
        }
        else if (c == state.StringDelimiter)
        {
            await _onToken(new TypescriptToken(TypescriptTokenType.StringValue, _buffer.ToString()));
            _buffer.Clear();
            state.StringDelimiter = null;
            state.Current = ParseState.Start;
        }
    }

    private async Task ProcessStringEscape(char c, State state)
    {
        _buffer.Append(c);
        state.Current = ParseState.InString;
    }

    private async Task ProcessCommentLine(char c, State state)
    {
        if (c == '\n' || c == '\r')
        {
            await _onToken(new TypescriptToken(TypescriptTokenType.Comment, _buffer.ToString()));
            _buffer.Clear();
            _buffer.Append(c);
            state.Current = ParseState.InWhitespace;
        }
        else
        {
            _buffer.Append(c);
        }
    }

    private async Task ProcessCommentBlock(char c, State state)
    {
        _buffer.Append(c);
        if (c == '*')
        {
            state.Current = ParseState.InCommentBlockStar;
        }
    }

    private async Task ProcessCommentBlockStar(char c, State state)
    {
        _buffer.Append(c);
        if (c == '/')
        {
            await _onToken(new TypescriptToken(TypescriptTokenType.Comment, _buffer.ToString()));
            _buffer.Clear();
            state.Current = ParseState.Start;
        }
        else if (c != '*')
        {
            state.Current = ParseState.InCommentBlock;
        }
    }

    private async Task ProcessOperator(char c, State state)
    {
        string current = _buffer.ToString();

        // Check for comment start
        if (current == "/" && c == '/')
        {
            _buffer.Append(c);
            state.Current = ParseState.InCommentLine;
            return;
        }
        else if (current == "/" && c == '*')
        {
            _buffer.Append(c);
            state.Current = ParseState.InCommentBlock;
            return;
        }

        // Check for ?. operator
        if (current == "?" && c == '.')
        {
            _buffer.Append(c);
            return;
        }

        // Try to build multi-character operators
        if (IsOperatorChar(c))
        {
            string potential = current + c;
            if (IsValidOperator(potential))
            {
                _buffer.Append(c);
                return;
            }
        }

        // Emit current operator and process next character
        await EmitOperator(current);
        _buffer.Clear();
        state.Current = ParseState.Start;
        await ProcessStart(c, state);
    }

    private async Task EmitOperator(string op)
    {
        TypescriptTokenType tokenType = op switch
        {
            "&&" => TypescriptTokenType.And,
            "||" => TypescriptTokenType.Or,
            "==" or "===" => TypescriptTokenType.Equals,
            "!=" or "!==" => TypescriptTokenType.NotEquals,
            _ => TypescriptTokenType.Operator
        };

        await _onToken(new TypescriptToken(tokenType, op));
    }

    private static bool IsOperatorChar(char c)
    {
        return c is '+' or '-' or '*' or '/' or '%' or '=' or '!' or '<' or '>' or '&' or '|' or '^' or '~' or '?';
    }

    private static bool IsValidOperator(string op)
    {
        // Multi-character operators
        return op is "==" or "===" or "!=" or "!==" or "<=" or ">=" or "&&" or "||" or "++" or "--"
            or "+=" or "-=" or "*=" or "/=" or "%=" or "<<" or ">>" or ">>>" or "&=" or "|=" or "^="
            or "&&=" or "||=" or "??=" or "=>" or "**" or "**=" or "??" or "?.";
    }

    private async Task EmitPending(State state)
    {
        if (_buffer.Length > 0)
        {
            switch (state.Current)
            {
                case ParseState.InWhitespace:
                    await _onToken(new TypescriptToken(TypescriptTokenType.Whitespace, _buffer.ToString()));
                    break;
                case ParseState.InIdentifier:
                    string identifier = _buffer.ToString();
                    if (Keywords.Contains(identifier))
                    {
                        await _onToken(new TypescriptToken(TypescriptTokenType.Keyword, identifier));
                    }
                    else
                    {
                        await _onToken(new TypescriptToken(TypescriptTokenType.Identifier, identifier));
                    }
                    break;
                case ParseState.InNumber:
                    string number = _buffer.ToString();
                    TypescriptTokenType tokenType = TypescriptTokenType.Number;
                    if (number.Contains('T') || number.Count(ch => ch == ':') >= 2 ||
                        (number.Count(ch => ch == '-') >= 2 && !number.StartsWith("-")))
                    {
                        tokenType = TypescriptTokenType.DateTimeValue;
                    }
                    await _onToken(new TypescriptToken(tokenType, number));
                    break;
                case ParseState.InString:
                    // Incomplete string - emit as string value
                    await _onToken(new TypescriptToken(TypescriptTokenType.StringValue, _buffer.ToString()));
                    break;
                case ParseState.InCommentLine:
                case ParseState.InCommentBlock:
                case ParseState.InCommentBlockStar:
                    await _onToken(new TypescriptToken(TypescriptTokenType.Comment, _buffer.ToString()));
                    break;
                case ParseState.InOperator:
                    await EmitOperator(_buffer.ToString());
                    break;
            }
            _buffer.Clear();
        }
    }

    private enum ParseState
    {
        Start,
        InWhitespace,
        InIdentifier,
        InNumber,
        InString,
        InStringEscape,
        InCommentLine,
        InCommentBlock,
        InCommentBlockStar,
        InOperator
    }

    private sealed class State
    {
        public ParseState Current = ParseState.Start;
        public char? StringDelimiter;
    }
}
