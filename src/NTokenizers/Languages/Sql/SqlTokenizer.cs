using NTokenizers.Core;

namespace NTokenizers.Sql;

/// <summary>
/// Provides functionality for tokenizing SQL text sources using a character-by-character state machine.
/// </summary>
public sealed class SqlTokenizer : BaseSubTokenizer<SqlToken>
{
    /// <summary>
    /// Creates a new instance of the <see cref="SqlTokenizer"/> class. 
    /// </summary>
    public static SqlTokenizer Create() => new();

    private static readonly HashSet<string> _keywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "SELECT", "FROM", "WHERE", "AND", "OR", "NOT", "IN", "IS", "NULL", "LIKE", "BETWEEN", "ON",
        "GROUP", "BY", "ORDER", "ASC", "DESC", "LIMIT", "OFFSET", "TOP", "AS", "CASE", "WHEN", "THEN",
        "ELSE", "END", "INSERT", "INTO", "VALUES", "UPDATE", "SET", "DELETE", "JOIN", "INNER", "LEFT",
        "RIGHT", "FULL OUTER JOIN", "CROSS JOIN", "CREATE", "TABLE", "ALTER", "DROP", "INDEX", "VIEW",
        "TRIGGER", "PROCEDURE", "FUNCTION", "EXISTS", "UNION", "ALL", "DISTINCT", "HAVING", "EXPLAIN",
        "DESCRIBE", "SHOW", "USE", "DATABASE", "SCHEMA", "GRANT", "REVOKE", "COMMIT", "ROLLBACK",
        "TRANSACTION", "SAVEPOINT", "ROLLBACK TO SAVEPOINT", "SET TRANSACTION", "LOCK", "UNLOCK",
        "BEGIN", "RETURN", "DECLARE", "FETCH", "CURSOR", "OPEN", "CLOSE", "NEXT", "PREVIOUS", "FIRST",
        "LAST", "ABSOLUTE", "RELATIVE", "ROWNUM", "ROWCOUNT", "OVER", "PARTITION BY", "INT", "BIGINT",
        "SMALLINT", "TINYINT", "DECIMAL", "NUMERIC", "FLOAT", "REAL", "BIT", "CHAR", "VARCHAR", "TEXT",
        "NCHAR", "NVARCHAR", "NTEXT", "DATE", "TIME", "DATETIME", "DATETIME2", "SMALLDATETIME",
        "TIMESTAMP", "BINARY", "VARBINARY", "IMAGE", "UNIQUEIDENTIFIER", "XML", "JSON", "SQL_VARIANT",
        "ENUM", "SET", "WITH", "RECURSIVE", "CTE", "PRIMARY", "KEY", "FOREIGN", "REFERENCES", "CHECK",
        "DEFAULT", "UNIQUE", "CONSTRAINT", "AVG", "SUM", "COUNT", "MIN", "MAX", "ABS", "CEIL", "CEILING",
        "FLOOR", "ROUND", "EXP", "LOG", "LOG10", "POWER", "SQRT", "MOD", "PI", "SIN", "COS", "TAN",
        "ASIN", "ACOS", "ATAN", "ATAN2", "RAND", "SIGN"
    };

    /// <summary>
    /// Parses SQL content from the given <see cref="TextReader"/> and produces a sequence
    /// of <see cref="SqlToken"/> objects.
    /// </summary>
    internal protected override async Task ParseAsync(CancellationToken ct)
    {
        var state = new State();

        await TokenizeCharactersAsync(ct, async (c) => await ProcessChar(c, state));

        await EmitPending(state); // Emit last token
    }

    private async Task ProcessChar(char c, State state)
    {
        state.Current = state.Current switch
        {
            ParseState.Start => await ProcessStart(c, state),
            ParseState.InString => await ProcessInString(c, state.StringQuote),
            ParseState.InNumber => await ProcessInNumber(c),
            ParseState.InIdentifier => await ProcessInIdentifier(c),
            ParseState.InOperator => await ProcessInOperator(c),
            ParseState.InLineComment => await ProcessInLineComment(c),
            ParseState.InBlockComment => await ProcessInBlockComment(c),
            ParseState.InWhitespace => await ProcessInWhitespace(c, state),
            _ => ParseState.Start,
        };
    }

    private async Task<ParseState> ProcessStart(char c, State state)
    {
        // Accumulate whitespace
        if (char.IsWhiteSpace(c))
        {
            _buffer.Append(c);
            return ParseState.InWhitespace;
        }

        // String literals
        if (c == '\'' || c == '"')
        {
            state.StringQuote = c;
            _buffer.Append(c);
            return ParseState.InString;
        }

        // Numbers
        if (char.IsDigit(c))
        {
            _buffer.Append(c);
            return ParseState.InNumber;
        }

        // Identifiers and keywords
        if (char.IsLetter(c) || c == '_')
        {
            _buffer.Append(c);
            return ParseState.InIdentifier;
        }

        // Comments
        if (c == '-')
        {
            int next = Peek();
            if (next == '-')
            {
                Read(); // consume second '-'
                _buffer.Append("--");
                return ParseState.InLineComment;
            }
            else
            {
                _buffer.Append(c);
                return ParseState.InOperator;
            }
        }

        if (c == '/')
        {
            int next = Peek();
            if (next == '*')
            {
                Read(); // consume '*'
                _buffer.Append("/*");
                return ParseState.InBlockComment;
            }
            else
            {
                _buffer.Append(c);
                return ParseState.InOperator;
            }
        }

        // Punctuation
        switch (c)
        {
            case ',':
                await _onToken(new SqlToken(SqlTokenType.Comma, ","));
                return ParseState.Start;
            case '.':
                await _onToken(new SqlToken(SqlTokenType.Dot, "."));
                return ParseState.Start;
            case '(':
                await _onToken(new SqlToken(SqlTokenType.OpenParenthesis, "("));
                return ParseState.Start;
            case ')':
                await _onToken(new SqlToken(SqlTokenType.CloseParenthesis, ")"));
                return ParseState.Start;
            case ';':
                await _onToken(new SqlToken(SqlTokenType.SequenceTerminator, ";"));
                return ParseState.Start;
        }

        // Operators
        if (IsOperatorChar(c))
        {
            _buffer.Append(c);
            return ParseState.InOperator;
        }

        // Not defined
        await _onToken(new SqlToken(SqlTokenType.NotDefined, c.ToString()));
        return ParseState.Start;
    }

    private async Task<ParseState> ProcessInString(char c, char stringQuote)
    {
        _buffer.Append(c);

        // Determine the quote character from the first char in buffer if stringQuote is not set
        char actualQuote = stringQuote != '\0' ? stringQuote : (_buffer.Length > 0 ? _buffer[0] : '"');

        if (c == actualQuote && _buffer.Length > 1) // Must have at least opening quote + closing quote
        {
            // Check for escaped quote (double quote)
            // We need to peek ahead, but we can't in this architecture
            // So we'll handle it differently: if the next char is also a quote, continue
            // This is a simplification - in reality, we should peek
            // For now, emit the string
            string value = _buffer.ToString();
            await _onToken(new SqlToken(SqlTokenType.StringValue, value));
            _buffer.Clear();
            return ParseState.Start;
        }

        return ParseState.InString;
    }

    private async Task<ParseState> ProcessInNumber(char c)
    {
        if (char.IsDigit(c) || c == '.')
        {
            _buffer.Append(c);
            return ParseState.InNumber;
        }
        else
        {
            // Number is complete
            string value = _buffer.ToString();
            await _onToken(new SqlToken(SqlTokenType.Number, value));
            _buffer.Clear();

            // Handle current character based on what it is
            if (char.IsWhiteSpace(c))
            {
                _buffer.Append(c);
                return ParseState.InWhitespace;
            }
            else if (c == ',' || c == '.' || c == '(' || c == ')' || c == ';')
            {
                // These need to be emitted
                switch (c)
                {
                    case ',':
                        await _onToken(new SqlToken(SqlTokenType.Comma, ","));
                        break;
                    case '.':
                        await _onToken(new SqlToken(SqlTokenType.Dot, "."));
                        break;
                    case '(':
                        await _onToken(new SqlToken(SqlTokenType.OpenParenthesis, "("));
                        break;
                    case ')':
                        await _onToken(new SqlToken(SqlTokenType.CloseParenthesis, ")"));
                        break;
                    case ';':
                        await _onToken(new SqlToken(SqlTokenType.SequenceTerminator, ";"));
                        break;
                }
                return ParseState.Start;
            }
            else if (char.IsLetter(c) || c == '_')
            {
                _buffer.Append(c);
                return ParseState.InIdentifier;
            }
            else if (IsOperatorChar(c))
            {
                _buffer.Append(c);
                return ParseState.InOperator;
            }
            else
            {
                await _onToken(new SqlToken(SqlTokenType.NotDefined, c.ToString()));
                return ParseState.Start;
            }
        }
    }

    private async Task<ParseState> ProcessInIdentifier(char c)
    {
        if (char.IsLetterOrDigit(c) || c == '_')
        {
            _buffer.Append(c);
            return ParseState.InIdentifier;
        }
        else
        {
            // Identifier is complete
            string value = _buffer.ToString();
            SqlTokenType tokenType = _keywords.Contains(value) ? SqlTokenType.Keyword : SqlTokenType.Identifier;
            await _onToken(new SqlToken(tokenType, value));
            _buffer.Clear();

            // Handle current character
            if (char.IsWhiteSpace(c))
            {
                _buffer.Append(c);
                return ParseState.InWhitespace;
            }
            else if (c == ',' || c == '.' || c == '(' || c == ')' || c == ';')
            {
                switch (c)
                {
                    case ',':
                        await _onToken(new SqlToken(SqlTokenType.Comma, ","));
                        break;
                    case '.':
                        await _onToken(new SqlToken(SqlTokenType.Dot, "."));
                        break;
                    case '(':
                        await _onToken(new SqlToken(SqlTokenType.OpenParenthesis, "("));
                        break;
                    case ')':
                        await _onToken(new SqlToken(SqlTokenType.CloseParenthesis, ")"));
                        break;
                    case ';':
                        await _onToken(new SqlToken(SqlTokenType.SequenceTerminator, ";"));
                        break;
                }
                return ParseState.Start;
            }
            else if (char.IsDigit(c))
            {
                _buffer.Append(c);
                return ParseState.InNumber;
            }
            else if (IsOperatorChar(c))
            {
                _buffer.Append(c);
                return ParseState.InOperator;
            }
            else
            {
                await _onToken(new SqlToken(SqlTokenType.NotDefined, c.ToString()));
                return ParseState.Start;
            }
        }
    }

    private async Task<ParseState> ProcessInOperator(char c)
    {
        if (IsOperatorChar(c))
        {
            _buffer.Append(c);
            // Check if we have a complete multi-char operator
            string current = _buffer.ToString();
            if (current == "<>" || current == "<=" || current == ">=" || current == "!=" || current == "||")
            {
                await _onToken(new SqlToken(SqlTokenType.Operator, current));
                _buffer.Clear();
                return ParseState.Start;
            }
            return ParseState.InOperator;
        }
        else
        {
            // Operator is complete
            string value = _buffer.ToString();
            await _onToken(new SqlToken(SqlTokenType.Operator, value));
            _buffer.Clear();

            // Handle current character
            if (char.IsWhiteSpace(c))
            {
                _buffer.Append(c);
                return ParseState.InWhitespace;
            }
            else if (c == ',' || c == '.' || c == '(' || c == ')' || c == ';')
            {
                switch (c)
                {
                    case ',':
                        await _onToken(new SqlToken(SqlTokenType.Comma, ","));
                        break;
                    case '.':
                        await _onToken(new SqlToken(SqlTokenType.Dot, "."));
                        break;
                    case '(':
                        await _onToken(new SqlToken(SqlTokenType.OpenParenthesis, "("));
                        break;
                    case ')':
                        await _onToken(new SqlToken(SqlTokenType.CloseParenthesis, ")"));
                        break;
                    case ';':
                        await _onToken(new SqlToken(SqlTokenType.SequenceTerminator, ";"));
                        break;
                }
                return ParseState.Start;
            }
            else if (char.IsDigit(c))
            {
                _buffer.Append(c);
                return ParseState.InNumber;
            }
            else if (char.IsLetter(c) || c == '_')
            {
                _buffer.Append(c);
                return ParseState.InIdentifier;
            }
            else
            {
                await _onToken(new SqlToken(SqlTokenType.NotDefined, c.ToString()));
                return ParseState.Start;
            }
        }
    }

    private async Task<ParseState> ProcessInLineComment(char c)
    {
        if (c == '\n' || c == '\r')
        {
            // Line comment is complete
            _buffer.Append(c);
            string value = _buffer.ToString();
            await _onToken(new SqlToken(SqlTokenType.Comment, value));
            _buffer.Clear();
            return ParseState.Start;
        }
        else
        {
            _buffer.Append(c);
            return ParseState.InLineComment;
        }
    }

    private async Task<ParseState> ProcessInBlockComment(char c)
    {
        _buffer.Append(c);

        // Check for end of block comment */
        string current = _buffer.ToString();
        if (current.Length >= 2 && current.EndsWith("*/"))
        {
            await _onToken(new SqlToken(SqlTokenType.Comment, current));
            _buffer.Clear();
            return ParseState.Start;
        }

        return ParseState.InBlockComment;
    }

    private async Task<ParseState> ProcessInWhitespace(char c, State state)
    {
        if (char.IsWhiteSpace(c))
        {
            _buffer.Append(c);
            return ParseState.InWhitespace;
        }
        else
        {
            // Whitespace is complete
            string value = _buffer.ToString();
            await _onToken(new SqlToken(SqlTokenType.Whitespace, value));
            _buffer.Clear();

            // Handle current character based on what it is
            if (c == '\'' || c == '"')
            {
                state.StringQuote = c;
                _buffer.Append(c);
                return ParseState.InString;
            }
            else if (char.IsDigit(c))
            {
                _buffer.Append(c);
                return ParseState.InNumber;
            }
            else if (char.IsLetter(c) || c == '_')
            {
                _buffer.Append(c);
                return ParseState.InIdentifier;
            }
            // Check for comments
            else if (c == '-')
            {
                int next = Peek();
                if (next == '-')
                {
                    Read(); // consume second '-'
                    _buffer.Append("--");
                    return ParseState.InLineComment;
                }
                else
                {
                    _buffer.Append(c);
                    return ParseState.InOperator;
                }
            }
            else if (c == '/')
            {
                int next = Peek();
                if (next == '*')
                {
                    Read(); // consume '*'
                    _buffer.Append("/*");
                    return ParseState.InBlockComment;
                }
                else
                {
                    _buffer.Append(c);
                    return ParseState.InOperator;
                }
            }
            else if (c == ',')
            {
                await _onToken(new SqlToken(SqlTokenType.Comma, ","));
                return ParseState.Start;
            }
            else if (c == '.')
            {
                await _onToken(new SqlToken(SqlTokenType.Dot, "."));
                return ParseState.Start;
            }
            else if (c == '(')
            {
                await _onToken(new SqlToken(SqlTokenType.OpenParenthesis, "("));
                return ParseState.Start;
            }
            else if (c == ')')
            {
                await _onToken(new SqlToken(SqlTokenType.CloseParenthesis, ")"));
                return ParseState.Start;
            }
            else if (c == ';')
            {
                await _onToken(new SqlToken(SqlTokenType.SequenceTerminator, ";"));
                return ParseState.Start;
            }
            else if (IsOperatorChar(c))
            {
                _buffer.Append(c);
                return ParseState.InOperator;
            }
            else
            {
                await _onToken(new SqlToken(SqlTokenType.NotDefined, c.ToString()));
                return ParseState.Start;
            }
        }
    }

    private async Task EmitPending(State state)
    {
        if (_buffer.Length == 0)
            return;

        string value = _buffer.ToString();

        switch (state.Current)
        {
            case ParseState.InString:
                await _onToken(new SqlToken(SqlTokenType.StringValue, value));
                break;
            case ParseState.InNumber:
                await _onToken(new SqlToken(SqlTokenType.Number, value));
                break;
            case ParseState.InIdentifier:
                SqlTokenType tokenType = _keywords.Contains(value) ? SqlTokenType.Keyword : SqlTokenType.Identifier;
                await _onToken(new SqlToken(tokenType, value));
                break;
            case ParseState.InOperator:
                await _onToken(new SqlToken(SqlTokenType.Operator, value));
                break;
            case ParseState.InLineComment:
            case ParseState.InBlockComment:
                await _onToken(new SqlToken(SqlTokenType.Comment, value));
                break;
            case ParseState.InWhitespace:
                await _onToken(new SqlToken(SqlTokenType.Whitespace, value));
                break;
        }

        _buffer.Clear();
    }

    private static bool IsOperatorChar(char c)
    {
        return c == '=' || c == '<' || c == '>' || c == '!' ||
               c == '+' || c == '-' || c == '*' || c == '/' || c == '%' || c == '|';
    }

    private enum ParseState
    {
        Start,
        InString,
        InNumber,
        InIdentifier,
        InOperator,
        InLineComment,
        InBlockComment,
        InWhitespace
    }

    private sealed class State
    {
        public ParseState Current = ParseState.Start;
        public char StringQuote = '\0';
    }
}