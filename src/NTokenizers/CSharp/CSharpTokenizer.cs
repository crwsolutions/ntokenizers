using System.Text;

namespace NTokenizers.CSharp;

/// <summary>
/// Provides functionality for tokenizing C# source code text.
/// </summary>
public static class CSharpTokenizer
{
    private static readonly HashSet<string> Keywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "abstract", "as", "async", "await", "base", "bool", "break", "byte", "case", "catch", "char",
        "checked", "class", "const", "continue", "decimal", "default", "delegate",
        "do", "double", "else", "enum", "event", "explicit", "extern", "false",
        "finally", "fixed", "float", "for", "foreach", "goto", "if", "implicit",
        "in", "int", "interface", "internal", "is", "lock", "long", "namespace",
        "new", "null", "object", "operator", "out", "override", "params", "private",
        "protected", "public", "readonly", "ref", "return", "sbyte", "sealed",
        "short", "sizeof", "stackalloc", "static", "string", "struct", "switch",
        "this", "throw", "true", "try", "typeof", "uint", "ulong", "unchecked",
        "unsafe", "ushort", "using", "virtual", "void", "volatile", "while"
    };

    /// <summary>
    /// Parses C# source code from the given <see cref="Stream"/> and
    /// produces a sequence of <see cref="CSharpToken"/> objects.
    /// </summary>
    /// <param name="stream">
    /// The input stream containing the text to tokenize.  
    /// The stream is read as UTF-8.
    /// </param>
    /// <param name="stopDelimiter">
    /// An optional string that, when encountered in the input, instructs the tokenizer
    /// to stop parsing and return control to the caller.  
    /// If <c>null</c>, the tokenizer parses until the end of the stream.
    /// </param>
    /// <param name="onToken">
    /// A callback invoked for each <see cref="CSharpToken"/> produced during parsing.
    /// This delegate must not be <c>null</c>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="stream"/> or <paramref name="onToken"/> is <c>null</c>.
    /// </exception>
    public static void Parse(Stream stream, string? stopDelimiter, Action<CSharpToken> onToken)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8);
        ParseInternal(reader, stopDelimiter, onToken);
    }

    private enum State
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

    private static void ParseInternal(TextReader reader, string? stopDelimiter, Action<CSharpToken> onToken)
    {
        var sb = new StringBuilder();
        State state = State.Start;
        bool escape = false;
        string delimiter = stopDelimiter ?? string.Empty;
        int delLength = delimiter.Length;

        if (delLength == 0)
        {
            while (true)
            {
                int ic = reader.Read();
                if (ic == -1)
                {
                    EmitPending(sb, ref state, onToken);
                    break;
                }
                char c = (char)ic;
                ProcessChar(c, ref state, ref escape, sb, onToken, reader);
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
                    ProcessChar(toProcess, ref state, ref escape, sb, onToken, reader);
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
                    ProcessChar(toProcess, ref state, ref escape, sb, onToken, reader);
                }
            }
            EmitPending(sb, ref state, onToken);
        }
    }

    private static void ProcessChar(char c, ref State state, ref bool escape, StringBuilder sb, Action<CSharpToken> onToken, TextReader reader)
    {
        switch (state)
        {
            case State.InString:
                ProcessInString(c, ref state, ref escape, sb, onToken);
                return;

            case State.InVerbatimString:
                ProcessInVerbatimString(c, ref state, sb, onToken, reader);
                return;

            case State.InCommentLine:
                ProcessInCommentLine(c, ref state, sb, onToken);
                return;

            case State.InCommentBlock:
                ProcessInCommentBlock(c, ref state, sb, onToken, reader);
                return;

            case State.InWhitespace:
                if (char.IsWhiteSpace(c))
                {
                    sb.Append(c);
                    return;
                }
                else
                {
                    EmitToken(CSharpTokenType.Whitespace, sb.ToString(), onToken);
                    sb.Clear();
                    state = State.Start;
                    // Fall through to process current character
                }
                break;

            case State.InIdentifier:
                if (char.IsLetterOrDigit(c) || c == '_')
                {
                    sb.Append(c);
                    return;
                }
                else
                {
                    EmitIdentifierOrKeyword(sb.ToString(), onToken);
                    sb.Clear();
                    state = State.Start;
                    // Fall through to process current character
                }
                break;

            case State.InNumber:
                if (char.IsDigit(c) || c == '.' || c == 'e' || c == 'E' || c == '-' || c == '+' || c == 'x' || c == 'X' || 
                    (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F') || c == '_')
                {
                    sb.Append(c);
                    return;
                }
                else
                {
                    EmitToken(CSharpTokenType.Number, sb.ToString(), onToken);
                    sb.Clear();
                    state = State.Start;
                    // Fall through to process current character
                }
                break;

            case State.InOperator:
                ProcessInOperator(c, ref state, sb, onToken, reader);
                return;
        }

        // State.Start - process new character
        if (char.IsWhiteSpace(c))
        {
            state = State.InWhitespace;
            sb.Append(c);
        }
        else if (c == '"')
        {
            state = State.InString;
            sb.Append(c);
        }
        else if (c == '@')
        {
            // Check if next character is a quote for verbatim string
            int next = reader.Peek();
            if (next == '"')
            {
                sb.Append(c);
                sb.Append((char)reader.Read());
                state = State.InVerbatimString;
            }
            else
            {
                // @ can be part of an identifier
                state = State.InIdentifier;
                sb.Append(c);
            }
        }
        else if (char.IsLetter(c) || c == '_')
        {
            state = State.InIdentifier;
            sb.Append(c);
        }
        else if (char.IsDigit(c))
        {
            state = State.InNumber;
            sb.Append(c);
        }
        else if (c == '/')
        {
            int next = reader.Peek();
            if (next == '/')
            {
                reader.Read(); // consume second /
                state = State.InCommentLine;
                sb.Append("//");
            }
            else if (next == '*')
            {
                reader.Read(); // consume *
                state = State.InCommentBlock;
                sb.Append("/*");
            }
            else
            {
                state = State.InOperator;
                sb.Append(c);
            }
        }
        else if (c == '(' || c == ')' || c == ',' || c == ';')
        {
            EmitPunctuation(c, onToken);
        }
        else if (c == '.')
        {
            // Check if it's part of a number or a dot operator
            int next = reader.Peek();
            if (next != -1 && char.IsDigit((char)next))
            {
                state = State.InNumber;
                sb.Append(c);
            }
            else
            {
                EmitToken(CSharpTokenType.Dot, ".", onToken);
            }
        }
        else if (IsOperatorChar(c))
        {
            state = State.InOperator;
            sb.Append(c);
        }
        else
        {
            // Unknown character - emit as NotDefined
            EmitToken(CSharpTokenType.NotDefined, c.ToString(), onToken);
        }
    }

    private static void ProcessInString(char c, ref State state, ref bool escape, StringBuilder sb, Action<CSharpToken> onToken)
    {
        sb.Append(c);
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
            EmitToken(CSharpTokenType.StringValue, sb.ToString(), onToken);
            sb.Clear();
            state = State.Start;
        }
    }

    private static void ProcessInVerbatimString(char c, ref State state, StringBuilder sb, Action<CSharpToken> onToken, TextReader reader)
    {
        sb.Append(c);
        if (c == '"')
        {
            // Check if it's an escaped quote (double quote)
            int next = reader.Peek();
            if (next == '"')
            {
                sb.Append((char)reader.Read());
            }
            else
            {
                // End of verbatim string
                EmitToken(CSharpTokenType.StringValue, sb.ToString(), onToken);
                sb.Clear();
                state = State.Start;
            }
        }
    }

    private static void ProcessInCommentLine(char c, ref State state, StringBuilder sb, Action<CSharpToken> onToken)
    {
        if (c == '\n' || c == '\r')
        {
            EmitToken(CSharpTokenType.Comment, sb.ToString(), onToken);
            sb.Clear();
            state = State.InWhitespace;
            sb.Append(c);
        }
        else
        {
            sb.Append(c);
        }
    }

    private static void ProcessInCommentBlock(char c, ref State state, StringBuilder sb, Action<CSharpToken> onToken, TextReader reader)
    {
        sb.Append(c);
        if (c == '*')
        {
            int next = reader.Peek();
            if (next == '/')
            {
                sb.Append((char)reader.Read());
                EmitToken(CSharpTokenType.Comment, sb.ToString(), onToken);
                sb.Clear();
                state = State.Start;
            }
        }
    }

    private static void ProcessInOperator(char c, ref State state, StringBuilder sb, Action<CSharpToken> onToken, TextReader reader)
    {
        string current = sb.ToString();
        string combined = current + c;

        // Check if the combined string forms a valid multi-character operator
        if (IsMultiCharOperator(combined))
        {
            sb.Append(c);
            // Check if we can form an even longer operator
            int next = reader.Peek();
            if (next != -1)
            {
                string further = combined + (char)next;
                if (IsMultiCharOperator(further))
                {
                    return; // Continue building the operator
                }
            }
            // Emit the operator
            EmitOperatorToken(sb.ToString(), onToken);
            sb.Clear();
            state = State.Start;
        }
        else if (IsOperatorChar(c))
        {
            // Emit the current operator and start a new one
            EmitOperatorToken(current, onToken);
            sb.Clear();
            sb.Append(c);
        }
        else
        {
            // Emit the current operator and process this character
            EmitOperatorToken(current, onToken);
            sb.Clear();
            state = State.Start;
            bool tempEscape = false;
            ProcessChar(c, ref state, ref tempEscape, sb, onToken, reader);
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

    private static void EmitOperatorToken(string value, Action<CSharpToken> onToken)
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
        EmitToken(type, value, onToken);
    }

    private static void EmitPunctuation(char c, Action<CSharpToken> onToken)
    {
        CSharpTokenType type = c switch
        {
            '(' => CSharpTokenType.OpenParenthesis,
            ')' => CSharpTokenType.CloseParenthesis,
            ',' => CSharpTokenType.Comma,
            ';' => CSharpTokenType.SequenceTerminator,
            _ => CSharpTokenType.NotDefined
        };
        EmitToken(type, c.ToString(), onToken);
    }

    private static void EmitIdentifierOrKeyword(string value, Action<CSharpToken> onToken)
    {
        if (Keywords.Contains(value))
        {
            EmitToken(CSharpTokenType.Keyword, value, onToken);
        }
        else
        {
            EmitToken(CSharpTokenType.Identifier, value, onToken);
        }
    }

    private static void EmitToken(CSharpTokenType type, string value, Action<CSharpToken> onToken)
    {
        onToken(new CSharpToken(type, value));
    }

    private static void EmitPending(StringBuilder sb, ref State state, Action<CSharpToken> onToken)
    {
        if (sb.Length > 0)
        {
            switch (state)
            {
                case State.InWhitespace:
                    EmitToken(CSharpTokenType.Whitespace, sb.ToString(), onToken);
                    break;
                case State.InIdentifier:
                    EmitIdentifierOrKeyword(sb.ToString(), onToken);
                    break;
                case State.InNumber:
                    EmitToken(CSharpTokenType.Number, sb.ToString(), onToken);
                    break;
                case State.InString:
                case State.InVerbatimString:
                    // Incomplete string - could emit as NotDefined or StringValue
                    EmitToken(CSharpTokenType.StringValue, sb.ToString(), onToken);
                    break;
                case State.InCommentLine:
                case State.InCommentBlock:
                    EmitToken(CSharpTokenType.Comment, sb.ToString(), onToken);
                    break;
                case State.InOperator:
                    EmitOperatorToken(sb.ToString(), onToken);
                    break;
            }
            sb.Clear();
        }
        state = State.Start;
    }
}
