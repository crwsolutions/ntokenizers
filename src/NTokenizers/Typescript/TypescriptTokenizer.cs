using System.Text;

namespace NTokenizers.Typescript;

/// <summary>
/// Provides functionality for tokenizing TypeScript text sources character by character using a state machine.
/// </summary>
public static class TypescriptTokenizer
{
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
    /// Parses TypeScript content from the given <see cref="Stream"/> and produces a sequence of <see cref="TypescriptToken"/> objects.
    /// </summary>
    /// <param name="stream">The input stream containing the text to tokenize. The stream is read as UTF-8.</param>
    /// <param name="onToken">A callback invoked for each <see cref="TypescriptToken"/> produced during parsing.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="stream"/> or <paramref name="onToken"/> is <c>null</c>.</exception>
    public static void Parse(Stream stream, Action<TypescriptToken> onToken)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8);
        Parse(reader, null, onToken);
    }

    /// <summary>
    /// Parses TypeScript content from the given <see cref="TextReader"/> and produces a sequence of <see cref="TypescriptToken"/> objects.
    /// </summary>
    /// <param name="reader">The input reader containing the text to tokenize.</param>
    /// <param name="stopDelimiter">
    /// An optional string that, when encountered in the input, instructs the tokenizer to stop parsing and return control to the caller.
    /// If <c>null</c>, the tokenizer parses until the end of the reader.
    /// </param>
    /// <param name="onToken">A callback invoked for each <see cref="TypescriptToken"/> produced during parsing.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="reader"/> or <paramref name="onToken"/> is <c>null</c>.</exception>
    public static void Parse(TextReader reader, string? stopDelimiter, Action<TypescriptToken> onToken)
    {
        var sb = new StringBuilder();
        var state = State.Start;
        char? stringDelimiter = null;
        string delimiter = stopDelimiter ?? string.Empty;
        int delLength = delimiter.Length;

        if (delLength == 0)
        {
            while (true)
            {
                int ic = reader.Read();
                if (ic == -1)
                {
                    EmitPending(sb, state, onToken);
                    break;
                }
                char c = (char)ic;
                ProcessChar(c, ref state, ref stringDelimiter, sb, onToken);
            }
        }
        else
        {
            var delQueue = new Queue<char>();
            bool stoppedByDelimiter = false;
            while (true)
            {
                int ic = reader.Read();
                if (ic == -1)
                {
                    break;
                }
                char c = (char)ic;
                delQueue.Enqueue(c);
                if (delQueue.Count > delLength)
                {
                    char toProcess = delQueue.Dequeue();
                    ProcessChar(toProcess, ref state, ref stringDelimiter, sb, onToken);
                }
                if (delQueue.Count == delLength && new string(delQueue.ToArray()) == delimiter)
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
                    ProcessChar(toProcess, ref state, ref stringDelimiter, sb, onToken);
                }
            }
            EmitPending(sb, state, onToken);
        }
    }

    private static void ProcessChar(char c, ref State state, ref char? stringDelimiter, StringBuilder sb, Action<TypescriptToken> onToken)
    {
        switch (state)
        {
            case State.Start:
                ProcessStart(c, ref state, ref stringDelimiter, sb, onToken);
                break;
            case State.InWhitespace:
                ProcessWhitespace(c, ref state, ref stringDelimiter, sb, onToken);
                break;
            case State.InIdentifier:
                ProcessIdentifier(c, ref state, ref stringDelimiter, sb, onToken);
                break;
            case State.InNumber:
                ProcessNumber(c, ref state, ref stringDelimiter, sb, onToken);
                break;
            case State.InString:
                ProcessString(c, ref state, ref stringDelimiter, sb, onToken);
                break;
            case State.InStringEscape:
                ProcessStringEscape(c, ref state, sb);
                break;
            case State.InCommentLine:
                ProcessCommentLine(c, ref state, sb, onToken);
                break;
            case State.InCommentBlock:
                ProcessCommentBlock(c, ref state, sb);
                break;
            case State.InCommentBlockStar:
                ProcessCommentBlockStar(c, ref state, sb, onToken);
                break;
            case State.InOperator:
                ProcessOperator(c, ref state, ref stringDelimiter, sb, onToken);
                break;
        }
    }

    private static void ProcessStart(char c, ref State state, ref char? stringDelimiter, StringBuilder sb, Action<TypescriptToken> onToken)
    {
        if (char.IsWhiteSpace(c))
        {
            sb.Append(c);
            state = State.InWhitespace;
        }
        else if (c == '\'' || c == '"' || c == '`')
        {
            stringDelimiter = c;
            sb.Append(c);
            state = State.InString;
        }
        else if (char.IsLetter(c) || c == '_' || c == '$')
        {
            sb.Append(c);
            state = State.InIdentifier;
        }
        else if (char.IsDigit(c))
        {
            sb.Append(c);
            state = State.InNumber;
        }
        else if (c == '/' || IsOperatorChar(c))
        {
            sb.Append(c);
            state = State.InOperator;
        }
        else if (c == '(')
        {
            onToken(new TypescriptToken(TypescriptTokenType.OpenParenthesis, "("));
        }
        else if (c == ')')
        {
            onToken(new TypescriptToken(TypescriptTokenType.CloseParenthesis, ")"));
        }
        else if (c == ',')
        {
            onToken(new TypescriptToken(TypescriptTokenType.Comma, ","));
        }
        else if (c == '.')
        {
            // Dot could be part of ?. operator, but here it's standalone
            onToken(new TypescriptToken(TypescriptTokenType.Dot, "."));
        }
        else if (c == ';')
        {
            onToken(new TypescriptToken(TypescriptTokenType.SequenceTerminator, ";"));
        }
        else if (c == '{' || c == '}' || c == '[' || c == ']' || c == ':')
        {
            // Other punctuation - emit as operator or notdefined
            onToken(new TypescriptToken(TypescriptTokenType.Operator, c.ToString()));
        }
        else
        {
            onToken(new TypescriptToken(TypescriptTokenType.NotDefined, c.ToString()));
        }
    }

    private static void ProcessWhitespace(char c, ref State state, ref char? stringDelimiter, StringBuilder sb, Action<TypescriptToken> onToken)
    {
        if (char.IsWhiteSpace(c))
        {
            sb.Append(c);
        }
        else
        {
            onToken(new TypescriptToken(TypescriptTokenType.Whitespace, sb.ToString()));
            sb.Clear();
            state = State.Start;
            ProcessStart(c, ref state, ref stringDelimiter, sb, onToken);
        }
    }

    private static void ProcessIdentifier(char c, ref State state, ref char? stringDelimiter, StringBuilder sb, Action<TypescriptToken> onToken)
    {
        if (char.IsLetterOrDigit(c) || c == '_' || c == '$')
        {
            sb.Append(c);
        }
        else
        {
            string identifier = sb.ToString();
            sb.Clear();
            
            // Check if it's a keyword (case-insensitive)
            if (Keywords.Contains(identifier))
            {
                onToken(new TypescriptToken(TypescriptTokenType.Keyword, identifier));
            }
            else
            {
                onToken(new TypescriptToken(TypescriptTokenType.Identifier, identifier));
            }
            
            state = State.Start;
            ProcessStart(c, ref state, ref stringDelimiter, sb, onToken);
        }
    }

    private static void ProcessNumber(char c, ref State state, ref char? stringDelimiter, StringBuilder sb, Action<TypescriptToken> onToken)
    {
        if (char.IsDigit(c) || c == '.' || c == 'e' || c == 'E' || c == '-' || c == '+' || c == 'x' || c == 'X' || 
            (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F') || c == 'T' || c == ':')
        {
            sb.Append(c);
            
            // Check if this looks like a datetime (simple heuristic: contains 'T' or multiple colons)
            string current = sb.ToString();
            if (current.Contains('T') || current.Count(ch => ch == ':') >= 2 || current.Count(ch => ch == '-') >= 2)
            {
                // Continue building as potential datetime
            }
        }
        else
        {
            string number = sb.ToString();
            sb.Clear();
            
            // Determine if it's a datetime or a number
            TypescriptTokenType tokenType = TypescriptTokenType.Number;
            if (number.Contains('T') || number.Count(ch => ch == ':') >= 2 || 
                (number.Count(ch => ch == '-') >= 2 && !number.StartsWith("-")))
            {
                tokenType = TypescriptTokenType.DateTimeValue;
            }
            
            onToken(new TypescriptToken(tokenType, number));
            state = State.Start;
            ProcessStart(c, ref state, ref stringDelimiter, sb, onToken);
        }
    }

    private static void ProcessString(char c, ref State state, ref char? stringDelimiter, StringBuilder sb, Action<TypescriptToken> onToken)
    {
        sb.Append(c);
        
        if (c == '\\')
        {
            state = State.InStringEscape;
        }
        else if (c == stringDelimiter)
        {
            onToken(new TypescriptToken(TypescriptTokenType.StringValue, sb.ToString()));
            sb.Clear();
            stringDelimiter = null;
            state = State.Start;
        }
    }

    private static void ProcessStringEscape(char c, ref State state, StringBuilder sb)
    {
        sb.Append(c);
        state = State.InString;
    }

    private static void ProcessCommentLine(char c, ref State state, StringBuilder sb, Action<TypescriptToken> onToken)
    {
        if (c == '\n' || c == '\r')
        {
            onToken(new TypescriptToken(TypescriptTokenType.Comment, sb.ToString()));
            sb.Clear();
            sb.Append(c);
            state = State.InWhitespace;
        }
        else
        {
            sb.Append(c);
        }
    }

    private static void ProcessCommentBlock(char c, ref State state, StringBuilder sb)
    {
        sb.Append(c);
        if (c == '*')
        {
            state = State.InCommentBlockStar;
        }
    }

    private static void ProcessCommentBlockStar(char c, ref State state, StringBuilder sb, Action<TypescriptToken> onToken)
    {
        sb.Append(c);
        if (c == '/')
        {
            onToken(new TypescriptToken(TypescriptTokenType.Comment, sb.ToString()));
            sb.Clear();
            state = State.Start;
        }
        else if (c != '*')
        {
            state = State.InCommentBlock;
        }
    }

    private static void ProcessOperator(char c, ref State state, ref char? stringDelimiter, StringBuilder sb, Action<TypescriptToken> onToken)
    {
        string current = sb.ToString();
        
        // Check for comment start
        if (current == "/" && c == '/')
        {
            sb.Append(c);
            state = State.InCommentLine;
            return;
        }
        else if (current == "/" && c == '*')
        {
            sb.Append(c);
            state = State.InCommentBlock;
            return;
        }
        
        // Check for ?. operator
        if (current == "?" && c == '.')
        {
            sb.Append(c);
            return;
        }
        
        // Try to build multi-character operators
        if (IsOperatorChar(c))
        {
            string potential = current + c;
            if (IsValidOperator(potential))
            {
                sb.Append(c);
                return;
            }
        }
        
        // Emit current operator and process next character
        EmitOperator(current, onToken);
        sb.Clear();
        state = State.Start;
        ProcessStart(c, ref state, ref stringDelimiter, sb, onToken);
    }

    private static void EmitOperator(string op, Action<TypescriptToken> onToken)
    {
        TypescriptTokenType tokenType = op switch
        {
            "&&" => TypescriptTokenType.And,
            "||" => TypescriptTokenType.Or,
            "==" or "===" => TypescriptTokenType.Equals,
            "!=" or "!==" => TypescriptTokenType.NotEquals,
            _ => TypescriptTokenType.Operator
        };
        
        onToken(new TypescriptToken(tokenType, op));
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

    private static void EmitPending(StringBuilder sb, State state, Action<TypescriptToken> onToken)
    {
        if (sb.Length > 0)
        {
            switch (state)
            {
                case State.InWhitespace:
                    onToken(new TypescriptToken(TypescriptTokenType.Whitespace, sb.ToString()));
                    break;
                case State.InIdentifier:
                    string identifier = sb.ToString();
                    if (Keywords.Contains(identifier))
                    {
                        onToken(new TypescriptToken(TypescriptTokenType.Keyword, identifier));
                    }
                    else
                    {
                        onToken(new TypescriptToken(TypescriptTokenType.Identifier, identifier));
                    }
                    break;
                case State.InNumber:
                    string number = sb.ToString();
                    TypescriptTokenType tokenType = TypescriptTokenType.Number;
                    if (number.Contains('T') || number.Count(ch => ch == ':') >= 2 || 
                        (number.Count(ch => ch == '-') >= 2 && !number.StartsWith("-")))
                    {
                        tokenType = TypescriptTokenType.DateTimeValue;
                    }
                    onToken(new TypescriptToken(tokenType, number));
                    break;
                case State.InString:
                    // Incomplete string - emit as string value
                    onToken(new TypescriptToken(TypescriptTokenType.StringValue, sb.ToString()));
                    break;
                case State.InCommentLine:
                case State.InCommentBlock:
                case State.InCommentBlockStar:
                    onToken(new TypescriptToken(TypescriptTokenType.Comment, sb.ToString()));
                    break;
                case State.InOperator:
                    EmitOperator(sb.ToString(), onToken);
                    break;
            }
            sb.Clear();
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
