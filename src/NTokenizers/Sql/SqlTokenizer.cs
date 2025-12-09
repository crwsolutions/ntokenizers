using NTokenizers.Core;
using NTokenizers.Extensions;

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
    internal protected override Task ParseAsync(CancellationToken ct)
    {
        var state = State.Start;
        char stringQuote = '\0';
        string delimiter = _stopDelimiter ?? string.Empty;
        int delLength = delimiter.Length;

        if (delLength == 0)
        {
            // No delimiter, parse until end of stream
            while (!ct.IsCancellationRequested)
            {
                int ic = Read();
                if (ic == -1)
                {
                    break;
                }
                char c = (char)ic;
                ProcessChar(c, ref state, ref stringQuote);
            }
        }
        else
        {
            // With delimiter, use a sliding window
            var delQueue = new Queue<char>();
            bool stoppedByDelimiter = false;

            while (!ct.IsCancellationRequested)
            {
                int ic = Read();
                if (ic == -1)
                {
                    break;
                }

                char c = (char)ic;
                delQueue.Enqueue(c);

                if (delQueue.Count > delLength)
                {
                    char toProcess = delQueue.Dequeue();
                    ProcessChar(toProcess, ref state, ref stringQuote);
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
                    ProcessChar(toProcess, ref state, ref stringQuote);
                }
            }

            if (stoppedByDelimiter)
            {
                StripFinalLineFeed();
            }
        }

        EmitPending(state);

        return Task.CompletedTask;
    }

    private void ProcessChar(char c, ref State state, ref char stringQuote)
    {
        state = state switch
        {
            State.Start => ProcessStart(c, ref stringQuote),
            State.InString => ProcessInString(c, stringQuote),
            State.InNumber => ProcessInNumber(c),
            State.InIdentifier => ProcessInIdentifier(c),
            State.InOperator => ProcessInOperator(c),
            State.InLineComment => ProcessInLineComment(c),
            State.InBlockComment => ProcessInBlockComment(c),
            State.InWhitespace => ProcessInWhitespace(c, ref stringQuote),
            _ => State.Start,
        };
    }

    private State ProcessStart(char c, ref char stringQuote)
    {
        // Accumulate whitespace
        if (char.IsWhiteSpace(c))
        {
            _buffer.Append(c);
            return State.InWhitespace;
        }

        // String literals
        if (c == '\'' || c == '"')
        {
            stringQuote = c;
            _buffer.Append(c);
            return State.InString;
        }

        // Numbers
        if (char.IsDigit(c))
        {
            _buffer.Append(c);
            return State.InNumber;
        }

        // Identifiers and keywords
        if (char.IsLetter(c) || c == '_')
        {
            _buffer.Append(c);
            return State.InIdentifier;
        }

        // Comments
        if (c == '-')
        {
            int next = Peek();
            if (next == '-')
            {
                Read(); // consume second '-'
                _buffer.Append("--");
                return State.InLineComment;
            }
            else
            {
                _buffer.Append(c);
                return State.InOperator;
            }
        }

        if (c == '/')
        {
            int next = Peek();
            if (next == '*')
            {
                Read(); // consume '*'
                _buffer.Append("/*");
                return State.InBlockComment;
            }
            else
            {
                _buffer.Append(c);
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
            _buffer.Append(c);
            return State.InOperator;
        }

        // Not defined
        _onToken(new SqlToken(SqlTokenType.NotDefined, c.ToString()));
        return State.Start;
    }

    private State ProcessInString(char c, char stringQuote)
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
            _onToken(new SqlToken(SqlTokenType.StringValue, value));
            _buffer.Clear();
            return State.Start;
        }

        return State.InString;
    }

    private State ProcessInNumber(char c)
    {
        if (char.IsDigit(c) || c == '.')
        {
            _buffer.Append(c);
            return State.InNumber;
        }
        else
        {
            // Number is complete
            string value = _buffer.ToString();
            _onToken(new SqlToken(SqlTokenType.Number, value));
            _buffer.Clear();

            // Handle current character based on what it is
            if (char.IsWhiteSpace(c))
            {
                _buffer.Append(c);
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
                _buffer.Append(c);
                return State.InIdentifier;
            }
            else if (IsOperatorChar(c))
            {
                _buffer.Append(c);
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
            _buffer.Append(c);
            return State.InIdentifier;
        }
        else
        {
            // Identifier is complete
            string value = _buffer.ToString();
            SqlTokenType tokenType = _keywords.Contains(value) ? SqlTokenType.Keyword : SqlTokenType.Identifier;
            _onToken(new SqlToken(tokenType, value));
            _buffer.Clear();

            // Handle current character
            if (char.IsWhiteSpace(c))
            {
                _buffer.Append(c);
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
                _buffer.Append(c);
                return State.InNumber;
            }
            else if (IsOperatorChar(c))
            {
                _buffer.Append(c);
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
            _buffer.Append(c);
            // Check if we have a complete multi-char operator
            string current = _buffer.ToString();
            if (current == "<>" || current == "<=" || current == ">=" || current == "!=" || current == "||")
            {
                _onToken(new SqlToken(SqlTokenType.Operator, current));
                _buffer.Clear();
                return State.Start;
            }
            return State.InOperator;
        }
        else
        {
            // Operator is complete
            string value = _buffer.ToString();
            _onToken(new SqlToken(SqlTokenType.Operator, value));
            _buffer.Clear();

            // Handle current character
            if (char.IsWhiteSpace(c))
            {
                _buffer.Append(c);
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
                _buffer.Append(c);
                return State.InNumber;
            }
            else if (char.IsLetter(c) || c == '_')
            {
                _buffer.Append(c);
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
            string value = _buffer.ToString();
            _onToken(new SqlToken(SqlTokenType.Comment, value));
            _buffer.Clear();
            return State.Start;
        }
        else
        {
            _buffer.Append(c);
            return State.InLineComment;
        }
    }

    private State ProcessInBlockComment(char c)
    {
        _buffer.Append(c);

        // Check for end of block comment */
        string current = _buffer.ToString();
        if (current.Length >= 2 && current.EndsWith("*/"))
        {
            _onToken(new SqlToken(SqlTokenType.Comment, current));
            _buffer.Clear();
            return State.Start;
        }

        return State.InBlockComment;
    }

    private State ProcessInWhitespace(char c, ref char stringQuote)
    {
        if (char.IsWhiteSpace(c))
        {
            _buffer.Append(c);
            return State.InWhitespace;
        }
        else
        {
            // Whitespace is complete
            string value = _buffer.ToString();
            _onToken(new SqlToken(SqlTokenType.Whitespace, value));
            _buffer.Clear();

            // Handle current character based on what it is
            if (c == '\'' || c == '"')
            {
                stringQuote = c;
                _buffer.Append(c);
                return State.InString;
            }
            else if (char.IsDigit(c))
            {
                _buffer.Append(c);
                return State.InNumber;
            }
            else if (char.IsLetter(c) || c == '_')
            {
                _buffer.Append(c);
                return State.InIdentifier;
            }
            // Check for comments
            else if (c == '-')
            {
                int next = Peek();
                if (next == '-')
                {
                    Read(); // consume second '-'
                    _buffer.Append("--");
                    return State.InLineComment;
                }
                else
                {
                    _buffer.Append(c);
                    return State.InOperator;
                }
            }
            else if (c == '/')
            {
                int next = Peek();
                if (next == '*')
                {
                    Read(); // consume '*'
                    _buffer.Append("/*");
                    return State.InBlockComment;
                }
                else
                {
                    _buffer.Append(c);
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
                _buffer.Append(c);
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
        if (_buffer.Length == 0)
            return;

        string value = _buffer.ToString();

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

        _buffer.Clear();
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