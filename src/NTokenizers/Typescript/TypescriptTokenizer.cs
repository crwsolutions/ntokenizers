using NTokenizers.Core;
using NTokenizers.Extensions;

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
    internal protected override Task ParseAsync()
    {
        var state = State.Start;
        char? stringDelimiter = null;
        string delimiter = _stopDelimiter ?? string.Empty;
        int delLength = delimiter.Length;

        if (delLength == 0)
        {
            while (true)
            {
                int ic = _reader.Read();
                if (ic == -1)
                {
                    EmitPending(state);
                    break;
                }
                char c = (char)ic;
                ProcessChar(c, ref state, ref stringDelimiter);
            }
        }
        else
        {
            var delQueue = new Queue<char>();
            bool stoppedByDelimiter = false;
            while (true)
            {
                int ic = _reader.Read();
                if (ic == -1)
                {
                    break;
                }
                char c = (char)ic;
                delQueue.Enqueue(c);
                if (delQueue.Count > delLength)
                {
                    char toProcess = delQueue.Dequeue();
                    ProcessChar(toProcess, ref state, ref stringDelimiter);
                }

                if (delQueue.IsEqualTo(delimiter))
                {
                    stoppedByDelimiter = true;
                    break;
                }
            }

            if (!stoppedByDelimiter)
            {
                while (delQueue.Count > 0)
                {
                    char toProcess = delQueue.Dequeue();
                    ProcessChar(toProcess, ref state, ref stringDelimiter);
                }
            }

            if (stoppedByDelimiter)
            {
                StripFinalLineFeed();
            }

            EmitPending(state);
        }

        return Task.CompletedTask;
    }

    private void ProcessChar(char c, ref State state, ref char? stringDelimiter)
    {
        switch (state)
        {
            case State.Start:
                ProcessStart(c, ref state, ref stringDelimiter);
                break;
            case State.InWhitespace:
                ProcessWhitespace(c, ref state, ref stringDelimiter);
                break;
            case State.InIdentifier:
                ProcessIdentifier(c, ref state, ref stringDelimiter);
                break;
            case State.InNumber:
                ProcessNumber(c, ref state, ref stringDelimiter);
                break;
            case State.InString:
                ProcessString(c, ref state, ref stringDelimiter);
                break;
            case State.InStringEscape:
                ProcessStringEscape(c, ref state);
                break;
            case State.InCommentLine:
                ProcessCommentLine(c, ref state);
                break;
            case State.InCommentBlock:
                ProcessCommentBlock(c, ref state);
                break;
            case State.InCommentBlockStar:
                ProcessCommentBlockStar(c, ref state);
                break;
            case State.InOperator:
                ProcessOperator(c, ref state, ref stringDelimiter);
                break;
        }
    }

    private void ProcessStart(char c, ref State state, ref char? stringDelimiter)
    {
        if (char.IsWhiteSpace(c))
        {
            _buffer.Append(c);
            state = State.InWhitespace;
        }
        else if (c == '\'' || c == '"' || c == '`')
        {
            stringDelimiter = c;
            _buffer.Append(c);
            state = State.InString;
        }
        else if (char.IsLetter(c) || c == '_' || c == '$')
        {
            _buffer.Append(c);
            state = State.InIdentifier;
        }
        else if (char.IsDigit(c))
        {
            _buffer.Append(c);
            state = State.InNumber;
        }
        else if (c == '/' || IsOperatorChar(c))
        {
            _buffer.Append(c);
            state = State.InOperator;
        }
        else if (c == '(')
        {
            _onToken(new TypescriptToken(TypescriptTokenType.OpenParenthesis, "("));
        }
        else if (c == ')')
        {
            _onToken(new TypescriptToken(TypescriptTokenType.CloseParenthesis, ")"));
        }
        else if (c == ',')
        {
            _onToken(new TypescriptToken(TypescriptTokenType.Comma, ","));
        }
        else if (c == '.')
        {
            // Dot could be part of ?. operator, but here it's standalone
            _onToken(new TypescriptToken(TypescriptTokenType.Dot, "."));
        }
        else if (c == ';')
        {
            _onToken(new TypescriptToken(TypescriptTokenType.SequenceTerminator, ";"));
        }
        else if (c == '{' || c == '}' || c == '[' || c == ']' || c == ':')
        {
            // Other punctuation - emit as operator or notdefined
            _onToken(new TypescriptToken(TypescriptTokenType.Operator, c.ToString()));
        }
        else
        {
            _onToken(new TypescriptToken(TypescriptTokenType.NotDefined, c.ToString()));
        }
    }

    private void ProcessWhitespace(char c, ref State state, ref char? stringDelimiter)
    {
        if (char.IsWhiteSpace(c))
        {
            _buffer.Append(c);
        }
        else
        {
            _onToken(new TypescriptToken(TypescriptTokenType.Whitespace, _buffer.ToString()));
            _buffer.Clear();
            state = State.Start;
            ProcessStart(c, ref state, ref stringDelimiter);
        }
    }

    private void ProcessIdentifier(char c, ref State state, ref char? stringDelimiter)
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
                _onToken(new TypescriptToken(TypescriptTokenType.Keyword, identifier));
            }
            else
            {
                _onToken(new TypescriptToken(TypescriptTokenType.Identifier, identifier));
            }

            state = State.Start;
            ProcessStart(c, ref state, ref stringDelimiter);
        }
    }

    private void ProcessNumber(char c, ref State state, ref char? stringDelimiter)
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

            _onToken(new TypescriptToken(tokenType, number));
            state = State.Start;
            ProcessStart(c, ref state, ref stringDelimiter);
        }
    }

    private void ProcessString(char c, ref State state, ref char? stringDelimiter)
    {
        _buffer.Append(c);

        if (c == '\\')
        {
            state = State.InStringEscape;
        }
        else if (c == stringDelimiter)
        {
            _onToken(new TypescriptToken(TypescriptTokenType.StringValue, _buffer.ToString()));
            _buffer.Clear();
            stringDelimiter = null;
            state = State.Start;
        }
    }

    private void ProcessStringEscape(char c, ref State state)
    {
        _buffer.Append(c);
        state = State.InString;
    }

    private void ProcessCommentLine(char c, ref State state)
    {
        if (c == '\n' || c == '\r')
        {
            _onToken(new TypescriptToken(TypescriptTokenType.Comment, _buffer.ToString()));
            _buffer.Clear();
            _buffer.Append(c);
            state = State.InWhitespace;
        }
        else
        {
            _buffer.Append(c);
        }
    }

    private void ProcessCommentBlock(char c, ref State state)
    {
        _buffer.Append(c);
        if (c == '*')
        {
            state = State.InCommentBlockStar;
        }
    }

    private void ProcessCommentBlockStar(char c, ref State state)
    {
        _buffer.Append(c);
        if (c == '/')
        {
            _onToken(new TypescriptToken(TypescriptTokenType.Comment, _buffer.ToString()));
            _buffer.Clear();
            state = State.Start;
        }
        else if (c != '*')
        {
            state = State.InCommentBlock;
        }
    }

    private void ProcessOperator(char c, ref State state, ref char? stringDelimiter)
    {
        string current = _buffer.ToString();

        // Check for comment start
        if (current == "/" && c == '/')
        {
            _buffer.Append(c);
            state = State.InCommentLine;
            return;
        }
        else if (current == "/" && c == '*')
        {
            _buffer.Append(c);
            state = State.InCommentBlock;
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
        EmitOperator(current);
        _buffer.Clear();
        state = State.Start;
        ProcessStart(c, ref state, ref stringDelimiter);
    }

    private void EmitOperator(string op)
    {
        TypescriptTokenType tokenType = op switch
        {
            "&&" => TypescriptTokenType.And,
            "||" => TypescriptTokenType.Or,
            "==" or "===" => TypescriptTokenType.Equals,
            "!=" or "!==" => TypescriptTokenType.NotEquals,
            _ => TypescriptTokenType.Operator
        };

        _onToken(new TypescriptToken(tokenType, op));
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

    private void EmitPending(State state)
    {
        if (_buffer.Length > 0)
        {
            switch (state)
            {
                case State.InWhitespace:
                    _onToken(new TypescriptToken(TypescriptTokenType.Whitespace, _buffer.ToString()));
                    break;
                case State.InIdentifier:
                    string identifier = _buffer.ToString();
                    if (Keywords.Contains(identifier))
                    {
                        _onToken(new TypescriptToken(TypescriptTokenType.Keyword, identifier));
                    }
                    else
                    {
                        _onToken(new TypescriptToken(TypescriptTokenType.Identifier, identifier));
                    }
                    break;
                case State.InNumber:
                    string number = _buffer.ToString();
                    TypescriptTokenType tokenType = TypescriptTokenType.Number;
                    if (number.Contains('T') || number.Count(ch => ch == ':') >= 2 ||
                        (number.Count(ch => ch == '-') >= 2 && !number.StartsWith("-")))
                    {
                        tokenType = TypescriptTokenType.DateTimeValue;
                    }
                    _onToken(new TypescriptToken(tokenType, number));
                    break;
                case State.InString:
                    // Incomplete string - emit as string value
                    _onToken(new TypescriptToken(TypescriptTokenType.StringValue, _buffer.ToString()));
                    break;
                case State.InCommentLine:
                case State.InCommentBlock:
                case State.InCommentBlockStar:
                    _onToken(new TypescriptToken(TypescriptTokenType.Comment, _buffer.ToString()));
                    break;
                case State.InOperator:
                    EmitOperator(_buffer.ToString());
                    break;
            }
            _buffer.Clear();
        }
    }

    private enum State
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
}
