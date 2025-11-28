using NTokenizers.Core;

namespace NTokenizers.Sql;

/// <summary>
/// Represents a SQL token with its type and value.
/// </summary>
public class SqlToken : IToken<SqlTokenType>
{
    /// <summary>
    /// Gets the type of the SQL token.
    /// </summary>
    public SqlTokenType TokenType { get; }

    /// <summary>
    /// Gets the string representation of the current value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlToken"/> class with the specified token type and value.
    /// </summary>
    /// <param name="tokenType">The type of the SQL token, indicating its role in the SQL syntax.</param>
    /// <param name="value">The string representation of the SQL token's value.</param>
    public SqlToken(SqlTokenType tokenType, string value)
    {
        TokenType = tokenType;
        Value = value;
    }
}
