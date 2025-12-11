using NTokenizers.Core;
using NTokenizers.Sql;

namespace NTokenizers.Markdown.Metadata;

/// <summary>
/// Metadata for SQL code block tokens with syntax highlighting support.
/// </summary>
public sealed class SqlCodeBlockMetadata(string language) : CodeBlockMetadata<Sql.SqlToken>(language)
{
    internal override BaseSubTokenizer<SqlToken> CreateTokenizer() => SqlTokenizer.Create();
}
