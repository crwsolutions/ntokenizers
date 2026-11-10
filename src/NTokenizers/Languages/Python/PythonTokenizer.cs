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
        public bool Escape; public char? StringDelimiter;
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
        // Handle specific states first
        if (state.InString)
        {
            await ProcessInString(c, state);
            return;
        }

        if (state.InTripleString)
        {
            await ProcessInTripleString(c, state);
            return;
        }

        if (state.InComment)
        {
            await ProcessInComment(c, state);
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
                await EmitToken(PythonTokenType.Whitespace);
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
                await EmitIdentifierOrKeyword(_buffer.ToString());
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
                await EmitToken(PythonTokenType.Number);
                ClearState(state);
                // Fall through to process current character
            }
        }
        else if (state.InOperator)
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
                state.StringDelimiter = c;
                _buffer.Append(c);
                _buffer.Append(c);
                _buffer.Append(c);
            }
            else
            {
                state.InString = true;
                state.StringDelimiter = c;
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
            await EmitToken(PythonTokenType.OpenParenthesis, "(");
        }
        else if (c == ')')
        {
            await EmitToken(PythonTokenType.CloseParenthesis, ")");
        }
        else if (c == '{')
        {
            await EmitToken(PythonTokenType.OpenBrace, "{");
        }
        else if (c == '}')
        {
            await EmitToken(PythonTokenType.CloseBrace, "}");
        }
        else if (c == '[')
        {
            await EmitToken(PythonTokenType.OpenBracket, "[");
        }
        else if (c == ']')
        {
            await EmitToken(PythonTokenType.CloseBracket, "]");
        }
        else if (c == ',')
        {
            await EmitToken(PythonTokenType.Comma, ",");
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
                await EmitToken(PythonTokenType.Dot, ".");
            }
        }
        else if (c == ':')
        {
            // Check for walrus operator :=
            int next = Peek();
            if (next == '=')
            {
                Read(); // consume =
                await EmitToken(PythonTokenType.Operator, ":=");
            }
            else
            {
                await EmitToken(PythonTokenType.Colon, ":");
            }
        }
        else if (c == ';')
        {
            await EmitToken(PythonTokenType.Semicolon, ";");
        }
        else if (c == '@')
        {
            await EmitToken(PythonTokenType.Hash, "@");
        }
        else if (IsOperatorChar(c))
        {
            state.InOperator = true;
            _buffer.Append(c);
        }
        else
        {
            // Unknown character - emit as NotDefined
            await EmitToken(PythonTokenType.NotDefined, c.ToString());
        }
    }

    private async Task ProcessInString(char c, State state)
    {
        _buffer.Append(c);

        if (state.Escape)
        {
            state.Escape = false;
            return;
        }

        if (c == '\\')
        {
            state.Escape = true;
            return;
        }

        if (c == state.StringDelimiter)
        {
            await EmitToken(PythonTokenType.StringValue);
            ClearState(state);
            state.StringDelimiter = null;
        }
    }

    private async Task ProcessInTripleString(char c, State state)
    {
        _buffer.Append(c);

        if (state.Escape)
        {
            state.Escape = false;
            return;
        }

        if (c == '\\')
        {
            state.Escape = true;
            return;
        }

        if (c == state.StringDelimiter)
        {
            // Check for triple quote end
            int next = Peek();
            int next2 = PeekAhead(1);
            if (next == state.StringDelimiter && next2 == state.StringDelimiter)
            {
                Read(); // consume second quote
                Read(); // consume third quote
                _buffer.Append(state.StringDelimiter.Value);
                _buffer.Append(state.StringDelimiter.Value);
                await EmitToken(PythonTokenType.StringValue);
                ClearState(state);
                state.StringDelimiter = null;
            }
        }
    }

    private async Task ProcessInComment(char c, State state)
    {
        if (c == '\n' || c == '\r')
        {
            await EmitToken(PythonTokenType.Comment);
            _buffer.Clear();
            ClearState(state);
            // Re-process the newline as a fresh character
            await ProcessChar(c, state);
        }
        else
        {
            _buffer.Append(c);
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
            await EmitToken(PythonTokenType.Operator, combined);
            _buffer.Clear();
            ClearState(state);
        }
        else if (IsOperatorChar(c))
        {
            // Emit the current operator and start a new one
            await EmitToken(PythonTokenType.Operator, current);
            _buffer.Clear();
            _buffer.Append(c);
        }
        else
        {
            // Emit the current operator and process this character
            await EmitToken(PythonTokenType.Operator, current);
            _buffer.Clear();
            ClearState(state);
            state.StringDelimiter = null;
            state.Escape = false;
            await ProcessChar(c, state);
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

    private async Task EmitIdentifierOrKeyword(string value)
    {
        if (Keywords.Contains(value))
        {
            await EmitToken(PythonTokenType.Keyword, value);
        }
        else
        {
            await EmitToken(PythonTokenType.Identifier, value);
        }
    }

    private async Task EmitToken(PythonTokenType type, string value)
    {
        await _onToken(new PythonToken(type, value));
    }

    private async Task EmitToken(PythonTokenType type)
    {
        await EmitToken(type, _buffer.ToString());
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

    private async Task EmitPending(State state)
    {
        if (_buffer.Length > 0)
        {
            if (state.InWhitespace)
            {
                await EmitToken(PythonTokenType.Whitespace);
            }
            else if (state.InIdentifier)
            {
                await EmitIdentifierOrKeyword(_buffer.ToString());
                _buffer.Clear();
            }
            else if (state.InNumber)
            {
                await EmitToken(PythonTokenType.Number);
            }
            else if (state.InString || state.InTripleString)
            {
                // Incomplete string - emit as StringValue
                await EmitToken(PythonTokenType.StringValue);
            }
            else if (state.InComment)
            {
                await EmitToken(PythonTokenType.Comment);
            }
            else if (state.InOperator)
            {
                await EmitToken(PythonTokenType.Operator, _buffer.ToString());
                _buffer.Clear();
            }
        }
    }
}
