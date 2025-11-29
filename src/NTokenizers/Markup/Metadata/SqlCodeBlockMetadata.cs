namespace NTokenizers.Markup.Metadata;

/// <summary>
/// Metadata for SQL code block tokens with syntax highlighting support.
/// </summary>
public sealed class SqlCodeBlockMetadata(string language) : CodeBlockMetadata<Sql.SqlToken>(language);
