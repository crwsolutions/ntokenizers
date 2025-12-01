using NTokenizers.Core;
using NTokenizers.Extensions;

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

    /// <inheritdoc/>
    internal protected override Task ParseAsync()
    {
        State state = State.Start;
        bool escape = false;
        string delimiter = _stopDelimiter ?? string.Empty;
        int delLength = delimiter.Length;

        if (delLength == 0)
        {
            while (true)
            {
                int ic = _reader.Read();
                if (ic == -1)
                {
                    EmitPending(ref state);
                    break;
                }
                char c = (char)ic;
                ProcessChar(c, ref state, ref escape);
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
                    ProcessChar(toProcess, ref state, ref escape);
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
                    ProcessChar(toProcess, ref state, ref escape);
                }
            }

            if (stoppedByDelimiter)
            {
                StripFinalLineFeed();
            }

            EmitPending(ref state);
        }

        return Task.CompletedTask;
    }

    private void ProcessChar(char c, ref State state, ref bool escape)
    {
        switch (state)
        {
            case State.InString:
                ProcessInString(c, ref state, ref escape);
                return;

            case State.InVerbatimString:
                ProcessInVerbatimString(c, ref state);
                return;

            case State.InCommentLine:
                ProcessInCommentLine(c, ref state);
                return;

            case State.InCommentBlock:
                ProcessInCommentBlock(c, ref state);
                return;

            case State.InWhitespace:
                if (char.IsWhiteSpace(c))
                {
                    _buffer.Append(c);
                    return;
                }
                else
                {
                    EmitToken(CSharpTokenType.Whitespace);
                    state = State.Start;
                    // Fall through to process current character
                }
                break;

            case State.InIdentifier:
                if (char.IsLetterOrDigit(c) || c == '_')
                {
                    _buffer.Append(c);
                    return;
                }
                else
                {
                    EmitIdentifierOrKeyword(_buffer.ToString());
                    _buffer.Clear();
                    state = State.Start;
                    // Fall through to process current character
                }
                break;

            case State.InNumber:
                if (char.IsDigit(c) || c == '.' || c == 'e' || c == 'E' || c == '-' || c == '+' || c == 'x' || c == 'X' ||
                    (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F') || c == '_')
                {
                    _buffer.Append(c);
                    return;
                }
                else
                {
                    EmitToken(CSharpTokenType.Number);
                    state = State.Start;
                    // Fall through to process current character
                }
                break;

            case State.InOperator:
                ProcessInOperator(c, ref state);
                return;
        }

        // State.Start - process new character
        if (char.IsWhiteSpace(c))
        {
            state = State.InWhitespace;
            _buffer.Append(c);
        }
        else if (c == '"')
        {
            state = State.InString;
            _buffer.Append(c);
        }
        else if (c == '@')
        {
            // Check if next character is a quote for verbatim string
            int next = _reader.Peek();
            if (next == '"')
            {
                _buffer.Append(c);
                _buffer.Append((char)_reader.Read());
                state = State.InVerbatimString;
            }
            else
            {
                // @ can be part of an identifier
                state = State.InIdentifier;
                _buffer.Append(c);
            }
        }
        else if (char.IsLetter(c) || c == '_')
        {
            state = State.InIdentifier;
            _buffer.Append(c);
        }
        else if (char.IsDigit(c))
        {
            state = State.InNumber;
            _buffer.Append(c);
        }
        else if (c == '/')
        {
            int next = _reader.Peek();
            if (next == '/')
            {
                _reader.Read(); // consume second /
                state = State.InCommentLine;
                _buffer.Append("//");
            }
            else if (next == '*')
            {
                _reader.Read(); // consume *
                state = State.InCommentBlock;
                _buffer.Append("/*");
            }
            else
            {
                state = State.InOperator;
                _buffer.Append(c);
            }
        }
        else if (c == '(' || c == ')' || c == ',' || c == ';')
        {
            EmitPunctuation(c);
        }
        else if (c == '.')
        {
            // Check if it's part of a number or a dot operator
            int next = _reader.Peek();
            if (next != -1 && char.IsDigit((char)next))
            {
                state = State.InNumber;
                _buffer.Append(c);
            }
            else
            {
                EmitToken(CSharpTokenType.Dot, ".");
            }
        }
        else if (IsOperatorChar(c))
        {
            state = State.InOperator;
            _buffer.Append(c);
        }
        else
        {
            // Unknown character - emit as NotDefined
            EmitToken(CSharpTokenType.NotDefined, c.ToString());
        }
    }

    private void ProcessInString(char c, ref State state, ref bool escape)
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
            EmitToken(CSharpTokenType.StringValue);
            state = State.Start;
        }
    }

    private void ProcessInVerbatimString(char c, ref State state)
    {
        _buffer.Append(c);
        if (c == '"')
        {
            // Check if it's an escaped quote (double quote)
            int next = _reader.Peek();
            if (next == '"')
            {
                _buffer.Append((char)_reader.Read());
            }
            else
            {
                // End of verbatim string
                EmitToken(CSharpTokenType.StringValue);
                state = State.Start;
            }
        }
    }

    private void ProcessInCommentLine(char c, ref State state)
    {
        if (c == '\n' || c == '\r')
        {
            EmitToken(CSharpTokenType.Comment);
            state = State.InWhitespace;
            _buffer.Append(c);
        }
        else
        {
            _buffer.Append(c);
        }
    }

    private void ProcessInCommentBlock(char c, ref State state)
    {
        _buffer.Append(c);
        if (c == '*')
        {
            int next = _reader.Peek();
            if (next == '/')
            {
                _buffer.Append((char)_reader.Read());
                EmitToken(CSharpTokenType.Comment);
                state = State.Start;
            }
        }
    }

    private void ProcessInOperator(char c, ref State state)
    {
        string current = _buffer.ToString();
        string combined = current + c;

        // Check if the combined string forms a valid multi-character operator
        if (IsMultiCharOperator(combined))
        {
            _buffer.Append(c);
            // Check if we can form an even longer operator
            int next = _reader.Peek();
            if (next != -1)
            {
                string further = combined + (char)next;
                if (IsMultiCharOperator(further))
                {
                    return; // Continue building the operator
                }
            }
            // Emit the operator
            EmitOperatorToken(_buffer.ToString());
            _buffer.Clear();
            state = State.Start;
        }
        else if (IsOperatorChar(c))
        {
            // Emit the current operator and start a new one
            EmitOperatorToken(current);
            _buffer.Clear();
            _buffer.Append(c);
        }
        else
        {
            // Emit the current operator and process this character
            EmitOperatorToken(current);
            _buffer.Clear();
            state = State.Start;
            bool tempEscape = false;
            ProcessChar(c, ref state, ref tempEscape);
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

    private void EmitOperatorToken(string value)
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
        EmitToken(type, value);
    }

    private void EmitPunctuation(char c)
    {
        CSharpTokenType type = c switch
        {
            '(' => CSharpTokenType.OpenParenthesis,
            ')' => CSharpTokenType.CloseParenthesis,
            ',' => CSharpTokenType.Comma,
            ';' => CSharpTokenType.SequenceTerminator,
            _ => CSharpTokenType.NotDefined
        };
        EmitToken(type, c.ToString());
    }

    private void EmitIdentifierOrKeyword(string value)
    {
        if (Keywords.Contains(value))
        {
            EmitToken(CSharpTokenType.Keyword, value);
        }
        else
        {
            EmitToken(CSharpTokenType.Identifier, value);
        }
    }

    private void EmitToken(CSharpTokenType type, string value)
    {
        _onToken(new CSharpToken(type, value));
    }

    private void EmitToken(CSharpTokenType type)
    {
        EmitToken(type, _buffer.ToString());
        _buffer.Clear();
    }

    private void EmitPending(ref State state)
    {
        if (_buffer.Length > 0)
        {
            switch (state)
            {
                case State.InWhitespace:
                    EmitToken(CSharpTokenType.Whitespace);
                    break;
                case State.InIdentifier:
                    EmitIdentifierOrKeyword(_buffer.ToString());
                    _buffer.Clear();
                    break;
                case State.InNumber:
                    EmitToken(CSharpTokenType.Number);
                    break;
                case State.InString:
                case State.InVerbatimString:
                    // Incomplete string - could emit as NotDefined or StringValue
                    EmitToken(CSharpTokenType.StringValue);
                    break;
                case State.InCommentLine:
                case State.InCommentBlock:
                    EmitToken(CSharpTokenType.Comment);
                    break;
                case State.InOperator:
                    EmitOperatorToken(_buffer.ToString());
                    _buffer.Clear();
                    break;
            }
        }
        state = State.Start;
    }
}
