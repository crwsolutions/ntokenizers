namespace NTokenizers.Sql;

/// <summary>
/// Represents a SQL token with its type and value.
/// </summary>
/// <param name="TokenType">The type of the SQL token.</param>
/// <param name="Value">The string value of the SQL token.</param>
public record SqlToken(SqlTokenType TokenType, string Value);
