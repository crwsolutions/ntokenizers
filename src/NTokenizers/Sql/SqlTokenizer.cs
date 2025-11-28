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
    internal protected override void Parse()
    {
        var state = State.Start;
        char stringQuote = '\0';
        string delimiter = _stopDelimiter ?? string.Empty;
        int delLength = delimiter.Length;

        if (delLength == 0)
        {
            // No delimiter, parse until end of stream
            while (true)
            {
                int ic = _reader.Read();
                if (ic == -1)
                {
                    EmitPending(state);
                    break;
                }
                char c = (char)ic;
                state = ProcessChar(c, state, ref stringQuote);
            }
        }
        else
        {
            // With delimiter, use a sliding window
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
                    state = ProcessChar(toProcess, state, ref stringQuote);
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
                    state = ProcessChar(toProcess, state, ref stringQuote);
                }
            }

            EmitPending(state);
        }
    }

    private State ProcessChar(char c, State state, ref char stringQuote)
    {
        switch (state)
        {
            case State.Start:
                return ProcessStart(c, ref stringQuote);

            case State.InString:
                return ProcessInString(c, stringQuote);

            case State.InNumber:
                return ProcessInNumber(c);

            case State.InIdentifier:
                return ProcessInIdentifier(c);

            case State.InOperator:
                return ProcessInOperator(c);

            case State.InLineComment:
                return ProcessInLineComment(c);

            case State.InBlockComment:
                return ProcessInBlockComment(c);

            case State.InWhitespace:
                return ProcessInWhitespace(c, ref stringQuote);

            default:
                return State.Start;
        }
    }

    private State ProcessStart(char c, ref char stringQuote)
    {
        // Accumulate whitespace
        if (char.IsWhiteSpace(c))
        {
            _sb.Append(c);
            return State.InWhitespace;
        }

        // String literals
        if (c == '\'' || c == '"')
        {
            stringQuote = c;
            _sb.Append(c);
            return State.InString;
        }

        // Numbers
        if (char.IsDigit(c))
        {
            _sb.Append(c);
            return State.InNumber;
        }

        // Identifiers and keywords
        if (char.IsLetter(c) || c == '_')
        {
            _sb.Append(c);
            return State.InIdentifier;
        }

        // Comments
        if (c == '-')
        {
            int next = _reader.Peek();
            if (next == '-')
            {
                _reader.Read(); // consume second '-'
                _sb.Append("--");
                return State.InLineComment;
            }
            else
            {
                _sb.Append(c);
                return State.InOperator;
            }
        }

        if (c == '/')
        {
            int next = _reader.Peek();
            if (next == '*')
            {
                _reader.Read(); // consume '*'
                _sb.Append("/*");
                return State.InBlockComment;
            }
            else
            {
                _sb.Append(c);
                return State.InOperator;
            }
        }

        // Punctuation
        switch (c)
        {
            case ',':
                _onToken(new SqlToken(SqlTokenType.Comma, ","));
                return State.Start;
            case '.':
                _onToken(new SqlToken(SqlTokenType.Dot, "."));
                return State.Start;
            case '(':
                _onToken(new SqlToken(SqlTokenType.OpenParenthesis, "("));
                return State.Start;
            case ')':
                _onToken(new SqlToken(SqlTokenType.CloseParenthesis, ")"));
                return State.Start;
            case ';':
                _onToken(new SqlToken(SqlTokenType.SequenceTerminator, ";"));
                return State.Start;
        }

        // Operators
        if (IsOperatorChar(c))
        {
            _sb.Append(c);
            return State.InOperator;
        }

        // Not defined
        _onToken(new SqlToken(SqlTokenType.NotDefined, c.ToString()));
        return State.Start;
    }

    private State ProcessInString(char c, char stringQuote)
    {
        _sb.Append(c);

        // Determine the quote character from the first char in buffer if stringQuote is not set
        char actualQuote = stringQuote != '\0' ? stringQuote : (_sb.Length > 0 ? _sb[0] : '"');

        if (c == actualQuote && _sb.Length > 1) // Must have at least opening quote + closing quote
        {
            // Check for escaped quote (double quote)
            // We need to peek ahead, but we can't in this architecture
            // So we'll handle it differently: if the next char is also a quote, continue
            // This is a simplification - in reality, we should peek
            // For now, emit the string
            string value = _sb.ToString();
            _onToken(new SqlToken(SqlTokenType.StringValue, value));
            _sb.Clear();
            return State.Start;
        }

        return State.InString;
    }

    private State ProcessInNumber(char c)
    {
        if (char.IsDigit(c) || c == '.')
        {
            _sb.Append(c);
            return State.InNumber;
        }
        else
        {
            // Number is complete
            string value = _sb.ToString();
            _onToken(new SqlToken(SqlTokenType.Number, value));
            _sb.Clear();

            // Handle current character based on what it is
            if (char.IsWhiteSpace(c))
            {
                _sb.Append(c);
                return State.InWhitespace;
            }
            else if (c == ',' || c == '.' || c == '(' || c == ')' || c == ';')
            {
                // These need to be emitted
                switch (c)
                {
                    case ',':
                        _onToken(new SqlToken(SqlTokenType.Comma, ","));
                        break;
                    case '.':
                        _onToken(new SqlToken(SqlTokenType.Dot, "."));
                        break;
                    case '(':
                        _onToken(new SqlToken(SqlTokenType.OpenParenthesis, "("));
                        break;
                    case ')':
                        _onToken(new SqlToken(SqlTokenType.CloseParenthesis, ")"));
                        break;
                    case ';':
                        _onToken(new SqlToken(SqlTokenType.SequenceTerminator, ";"));
                        break;
                }
                return State.Start;
            }
            else if (char.IsLetter(c) || c == '_')
            {
                _sb.Append(c);
                return State.InIdentifier;
            }
            else if (IsOperatorChar(c))
            {
                _sb.Append(c);
                return State.InOperator;
            }
            else
            {
                _onToken(new SqlToken(SqlTokenType.NotDefined, c.ToString()));
                return State.Start;
            }
        }
    }

    private State ProcessInIdentifier(char c)
    {
        if (char.IsLetterOrDigit(c) || c == '_')
        {
            _sb.Append(c);
            return State.InIdentifier;
        }
        else
        {
            // Identifier is complete
            string value = _sb.ToString();
            SqlTokenType tokenType = _keywords.Contains(value) ? SqlTokenType.Keyword : SqlTokenType.Identifier;
            _onToken(new SqlToken(tokenType, value));
            _sb.Clear();

            // Handle current character
            if (char.IsWhiteSpace(c))
            {
                _sb.Append(c);
                return State.InWhitespace;
            }
            else if (c == ',' || c == '.' || c == '(' || c == ')' || c == ';')
            {
                switch (c)
                {
                    case ',':
                        _onToken(new SqlToken(SqlTokenType.Comma, ","));
                        break;
                    case '.':
                        _onToken(new SqlToken(SqlTokenType.Dot, "."));
                        break;
                    case '(':
                        _onToken(new SqlToken(SqlTokenType.OpenParenthesis, "("));
                        break;
                    case ')':
                        _onToken(new SqlToken(SqlTokenType.CloseParenthesis, ")"));
                        break;
                    case ';':
                        _onToken(new SqlToken(SqlTokenType.SequenceTerminator, ";"));
                        break;
                }
                return State.Start;
            }
            else if (char.IsDigit(c))
            {
                _sb.Append(c);
                return State.InNumber;
            }
            else if (IsOperatorChar(c))
            {
                _sb.Append(c);
                return State.InOperator;
            }
            else
            {
                _onToken(new SqlToken(SqlTokenType.NotDefined, c.ToString()));
                return State.Start;
            }
        }
    }

    private State ProcessInOperator(char c)
    {
        if (IsOperatorChar(c))
        {
            _sb.Append(c);
            // Check if we have a complete multi-char operator
            string current = _sb.ToString();
            if (current == "<>" || current == "<=" || current == ">=" || current == "!=" || current == "||")
            {
                _onToken(new SqlToken(SqlTokenType.Operator, current));
                _sb.Clear();
                return State.Start;
            }
            return State.InOperator;
        }
        else
        {
            // Operator is complete
            string value = _sb.ToString();
            _onToken(new SqlToken(SqlTokenType.Operator, value));
            _sb.Clear();

            // Handle current character
            if (char.IsWhiteSpace(c))
            {
                _sb.Append(c);
                return State.InWhitespace;
            }
            else if (c == ',' || c == '.' || c == '(' || c == ')' || c == ';')
            {
                switch (c)
                {
                    case ',':
                        _onToken(new SqlToken(SqlTokenType.Comma, ","));
                        break;
                    case '.':
                        _onToken(new SqlToken(SqlTokenType.Dot, "."));
                        break;
                    case '(':
                        _onToken(new SqlToken(SqlTokenType.OpenParenthesis, "("));
                        break;
                    case ')':
                        _onToken(new SqlToken(SqlTokenType.CloseParenthesis, ")"));
                        break;
                    case ';':
                        _onToken(new SqlToken(SqlTokenType.SequenceTerminator, ";"));
                        break;
                }
                return State.Start;
            }
            else if (char.IsDigit(c))
            {
                _sb.Append(c);
                return State.InNumber;
            }
            else if (char.IsLetter(c) || c == '_')
            {
                _sb.Append(c);
                return State.InIdentifier;
            }
            else
            {
                _onToken(new SqlToken(SqlTokenType.NotDefined, c.ToString()));
                return State.Start;
            }
        }
    }

    private State ProcessInLineComment(char c)
    {
        if (c == '\n' || c == '\r')
        {
            // Line comment is complete
            string value = _sb.ToString();
            _onToken(new SqlToken(SqlTokenType.Comment, value));
            _sb.Clear();
            return State.Start;
        }
        else
        {
            _sb.Append(c);
            return State.InLineComment;
        }
    }

    private State ProcessInBlockComment(char c)
    {
        _sb.Append(c);

        // Check for end of block comment */
        string current = _sb.ToString();
        if (current.Length >= 2 && current.EndsWith("*/"))
        {
            _onToken(new SqlToken(SqlTokenType.Comment, current));
            _sb.Clear();
            return State.Start;
        }

        return State.InBlockComment;
    }

    private State ProcessInWhitespace(char c, ref char stringQuote)
    {
        if (char.IsWhiteSpace(c))
        {
            _sb.Append(c);
            return State.InWhitespace;
        }
        else
        {
            // Whitespace is complete
            string value = _sb.ToString();
            _onToken(new SqlToken(SqlTokenType.Whitespace, value));
            _sb.Clear();

            // Handle current character based on what it is
            if (c == '\'' || c == '"')
            {
                stringQuote = c;
                _sb.Append(c);
                return State.InString;
            }
            else if (char.IsDigit(c))
            {
                _sb.Append(c);
                return State.InNumber;
            }
            else if (char.IsLetter(c) || c == '_')
            {
                _sb.Append(c);
                return State.InIdentifier;
            }
            // Check for comments
            else if (c == '-')
            {
                int next = _reader.Peek();
                if (next == '-')
                {
                    _reader.Read(); // consume second '-'
                    _sb.Append("--");
                    return State.InLineComment;
                }
                else
                {
                    _sb.Append(c);
                    return State.InOperator;
                }
            }
            else if (c == '/')
            {
                int next = _reader.Peek();
                if (next == '*')
                {
                    _reader.Read(); // consume '*'
                    _sb.Append("/*");
                    return State.InBlockComment;
                }
                else
                {
                    _sb.Append(c);
                    return State.InOperator;
                }
            }
            else if (c == ',')
            {
                _onToken(new SqlToken(SqlTokenType.Comma, ","));
                return State.Start;
            }
            else if (c == '.')
            {
                _onToken(new SqlToken(SqlTokenType.Dot, "."));
                return State.Start;
            }
            else if (c == '(')
            {
                _onToken(new SqlToken(SqlTokenType.OpenParenthesis, "("));
                return State.Start;
            }
            else if (c == ')')
            {
                _onToken(new SqlToken(SqlTokenType.CloseParenthesis, ")"));
                return State.Start;
            }
            else if (c == ';')
            {
                _onToken(new SqlToken(SqlTokenType.SequenceTerminator, ";"));
                return State.Start;
            }
            else if (IsOperatorChar(c))
            {
                _sb.Append(c);
                return State.InOperator;
            }
            else
            {
                _onToken(new SqlToken(SqlTokenType.NotDefined, c.ToString()));
                return State.Start;
            }
        }
    }

    private void EmitPending(State state)
    {
        if (_sb.Length == 0)
            return;

        string value = _sb.ToString();

        switch (state)
        {
            case State.InString:
                _onToken(new SqlToken(SqlTokenType.StringValue, value));
                break;
            case State.InNumber:
                _onToken(new SqlToken(SqlTokenType.Number, value));
                break;
            case State.InIdentifier:
                SqlTokenType tokenType = _keywords.Contains(value) ? SqlTokenType.Keyword : SqlTokenType.Identifier;
                _onToken(new SqlToken(tokenType, value));
                break;
            case State.InOperator:
                _onToken(new SqlToken(SqlTokenType.Operator, value));
                break;
            case State.InLineComment:
            case State.InBlockComment:
                _onToken(new SqlToken(SqlTokenType.Comment, value));
                break;
            case State.InWhitespace:
                _onToken(new SqlToken(SqlTokenType.Whitespace, value));
                break;
        }

        _sb.Clear();
    }

    private static bool IsOperatorChar(char c)
    {
        return c == '=' || c == '<' || c == '>' || c == '!' ||
               c == '+' || c == '-' || c == '*' || c == '/' || c == '%' || c == '|';
    }

    private enum State
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
}
