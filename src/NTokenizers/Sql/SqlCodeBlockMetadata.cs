using NTokenizers.Core;

namespace NTokenizers.Sql;

/// <summary>
/// Metadata for SQL code block tokens with syntax highlighting support.
/// </summary>
public sealed class SqlCodeBlockMetadata(string language) : CodeBlockMetadata<SqlToken>(language)
{
    internal override BaseSubTokenizer<SqlToken> CreateTokenizer() => SqlTokenizer.Create();
}
