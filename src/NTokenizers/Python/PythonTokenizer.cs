using NTokenizers.Core;

namespace NTokenizers.Python;

/// <summary>
/// Provides functionality for tokenizing Python source code text.
/// </summary>
public sealed class PythonTokenizer : BaseSubTokenizer<PythonToken>
{
    /// <summary>
    /// Creates a new instance of the <see cref="PythonTokenizer"/> class.
    /// </summary>
    public static PythonTokenizer Create() => new();

    private static readonly HashSet<string> Keywords = new(StringComparer.Ordinal)
    {
        // Flow control
        "False", "None", "True", "and", "as", "assert", "async", "await", "break",
        "class", "continue", "def", "del", "elif", "else", "except", "finally",
        "for", "from", "global", "if", "import", "in", "is", "lambda", "nonlocal",
        "not", "or", "pass", "raise", "return", "try", "while", "with", "yield",
    };

    private sealed class State
    {
        public bool InWhitespace;
        public bool InIdentifier;
        public bool InNumber;
        public bool InString;
        public bool InTripleString;
        public bool InComment;
        public bool InOperator;
    }

    /// <inheritdoc/>
    internal protected override Task ParseAsync(CancellationToken ct)
    {
        var state = new State();
        char? stringDelimiter = null;
        bool escape = false;

        TokenizeCharacters(ct, (c) => ProcessChar(c, state, ref stringDelimiter, ref escape));

        EmitPending(state);

        return Task.CompletedTask;
    }

    private void ProcessChar(char c, State state, ref char? stringDelimiter, ref bool escape)
    {
        // Handle specific states first
        if (state.InString)
        {
            ProcessInString(c, state, ref stringDelimiter, ref escape);
            return;
        }

        if (state.InTripleString)
        {
            ProcessInTripleString(c, state, ref stringDelimiter, ref escape);
            return;
        }

        if (state.InComment)
        {
            ProcessInComment(c, state, ref stringDelimiter, ref escape);
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
                EmitToken(PythonTokenType.Whitespace);
                ClearState(state);
                // Fall through to process current character
            }
        }
        else if (state.InIdentifier)
        {
            if (char.IsLetterOrDigit(c) || c == '_')
            {
                _buffer.Append(c);
                return;
            }
            else
            {
                EmitIdentifierOrKeyword(_buffer.ToString());
                _buffer.Clear();
                ClearState(state);
                // Fall through to process current character
            }
        }
        else if (state.InNumber)
        {
            // Continue number for: digits, decimal point, scientific notation, hex/octal/binary prefixes,
            // hex digits (a-f, A-F), complex suffix (j, J), underscores
            if (char.IsDigit(c) || c == '.' || c == 'e' || c == 'E' || c == '-' || c == '+' ||
                c == 'x' || c == 'X' || c == 'o' || c == 'O' || c == 'b' || c == 'B' ||
                c == 'j' || c == 'J' || c == '_' ||
                ((c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F')))
            {
                _buffer.Append(c);
                return;
            }
            else
            {
                EmitToken(PythonTokenType.Number);
                ClearState(state);
                // Fall through to process current character
            }
        }
        else if (state.InOperator)
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
        else if (c == '\'' || c == '"')
        {
            // Check for triple quotes
            int next = Peek();
            int next2 = PeekAhead(1);
            if (next == c && next2 == c)
            {
                // Triple quote string
                Read(); // consume second quote
                Read(); // consume third quote
                state.InTripleString = true;
                stringDelimiter = c;
                _buffer.Append(c);
                _buffer.Append(c);
                _buffer.Append(c);
            }
            else
            {
                state.InString = true;
                stringDelimiter = c;
                _buffer.Append(c);
            }
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
        else if (c == '#')
        {
            state.InComment = true;
            _buffer.Append(c);
        }
        else if (c == '(')
        {
            EmitToken(PythonTokenType.OpenParenthesis, "(");
        }
        else if (c == ')')
        {
            EmitToken(PythonTokenType.CloseParenthesis, ")");
        }
        else if (c == '{')
        {
            EmitToken(PythonTokenType.OpenBrace, "{");
        }
        else if (c == '}')
        {
            EmitToken(PythonTokenType.CloseBrace, "}");
        }
        else if (c == '[')
        {
            EmitToken(PythonTokenType.OpenBracket, "[");
        }
        else if (c == ']')
        {
            EmitToken(PythonTokenType.CloseBracket, "]");
        }
        else if (c == ',')
        {
            EmitToken(PythonTokenType.Comma, ",");
        }
        else if (c == '.')
        {
            // Check if it's part of a number
            int next = Peek();
            if (next != -1 && char.IsDigit((char)next))
            {
                state.InNumber = true;
                _buffer.Append(c);
            }
            else
            {
                EmitToken(PythonTokenType.Dot, ".");
            }
        }
        else if (c == ':')
        {
            // Check for walrus operator :=
            int next = Peek();
            if (next == '=')
            {
                Read(); // consume =
                EmitToken(PythonTokenType.Operator, ":=");
            }
            else
            {
                EmitToken(PythonTokenType.Colon, ":");
            }
        }
        else if (c == ';')
        {
            EmitToken(PythonTokenType.Semicolon, ";");
        }
        else if (c == '@')
        {
            EmitToken(PythonTokenType.Hash, "@");
        }
        else if (IsOperatorChar(c))
        {
            state.InOperator = true;
            _buffer.Append(c);
        }
        else
        {
            // Unknown character - emit as NotDefined
            EmitToken(PythonTokenType.NotDefined, c.ToString());
        }
    }

    private void ProcessInString(char c, State state, ref char? stringDelimiter, ref bool escape)
    {
        _buffer.Append(c);

        if (escape)
        {
            escape = false;
            return;
        }

        if (c == '\\')
        {
            escape = true;
            return;
        }

        if (c == stringDelimiter)
        {
            EmitToken(PythonTokenType.StringValue);
            ClearState(state);
            stringDelimiter = null;
        }
    }

    private void ProcessInTripleString(char c, State state, ref char? stringDelimiter, ref bool escape)
    {
        _buffer.Append(c);

        if (escape)
        {
            escape = false;
            return;
        }

        if (c == '\\')
        {
            escape = true;
            return;
        }

        if (c == stringDelimiter)
        {
            // Check for triple quote end
            int next = Peek();
            int next2 = PeekAhead(1);
            if (next == stringDelimiter && next2 == stringDelimiter)
            {
                Read(); // consume second quote
                Read(); // consume third quote
                _buffer.Append(stringDelimiter.Value);
                _buffer.Append(stringDelimiter.Value);
                EmitToken(PythonTokenType.StringValue);
                ClearState(state);
                stringDelimiter = null;
            }
        }
    }

    private void ProcessInComment(char c, State state, ref char? stringDelimiter, ref bool escape)
    {
        if (c == '\n' || c == '\r')
        {
            EmitToken(PythonTokenType.Comment);
            _buffer.Clear();
            ClearState(state);
            // Re-process the newline as a fresh character
            ProcessChar(c, state, ref stringDelimiter, ref escape);
        }
        else
        {
            _buffer.Append(c);
        }
    }

    private void ProcessInOperator(char c, State state)
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
            EmitToken(PythonTokenType.Operator, combined);
            _buffer.Clear();
            ClearState(state);
        }
        else if (IsOperatorChar(c))
        {
            // Emit the current operator and start a new one
            EmitToken(PythonTokenType.Operator, current);
            _buffer.Clear();
            _buffer.Append(c);
        }
        else
        {
            // Emit the current operator and process this character
            EmitToken(PythonTokenType.Operator, current);
            _buffer.Clear();
            ClearState(state);
            char? stringDelimiter = null;
            bool escape = false;
            ProcessChar(c, state, ref stringDelimiter, ref escape);
        }
    }

    private static bool IsOperatorChar(char c)
    {
        return c == '=' || c == '!' || c == '<' || c == '>' || c == '+' || c == '-' ||
               c == '*' || c == '/' || c == '%' || c == '&' || c == '|' || c == '^' ||
               c == '~';
    }

    private static bool IsMultiCharOperator(string op)
    {
        return op == "==" || op == "!=" || op == "<=" || op == ">=" || op == "+=" || op == "-=" ||
               op == "*=" || op == "/=" || op == "%=" || op == "&=" || op == "|=" || op == "^=" ||
               op == "**" || op == "//" || op == "**=" || op == "//=" || op == "<<" || op == ">>" ||
               op == "<<=" || op == ">>=";
    }

    private void EmitIdentifierOrKeyword(string value)
    {
        if (Keywords.Contains(value))
        {
            EmitToken(PythonTokenType.Keyword, value);
        }
        else
        {
            EmitToken(PythonTokenType.Identifier, value);
        }
    }

    private void EmitToken(PythonTokenType type, string value)
    {
        _onToken(new PythonToken(type, value));
    }

    private void EmitToken(PythonTokenType type)
    {
        EmitToken(type, _buffer.ToString());
        _buffer.Clear();
    }

    private static void ClearState(State state)
    {
        state.InWhitespace = false;
        state.InIdentifier = false;
        state.InNumber = false;
        state.InString = false;
        state.InTripleString = false;
        state.InComment = false;
        state.InOperator = false;
    }

    private void EmitPending(State state)
    {
        if (_buffer.Length > 0)
        {
            if (state.InWhitespace)
            {
                EmitToken(PythonTokenType.Whitespace);
            }
            else if (state.InIdentifier)
            {
                EmitIdentifierOrKeyword(_buffer.ToString());
                _buffer.Clear();
            }
            else if (state.InNumber)
            {
                EmitToken(PythonTokenType.Number);
            }
            else if (state.InString || state.InTripleString)
            {
                // Incomplete string - emit as StringValue
                EmitToken(PythonTokenType.StringValue);
            }
            else if (state.InComment)
            {
                EmitToken(PythonTokenType.Comment);
            }
            else if (state.InOperator)
            {
                EmitToken(PythonTokenType.Operator, _buffer.ToString());
                _buffer.Clear();
            }
        }
    }
}
