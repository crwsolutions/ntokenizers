using System.Text;
using NTokenizers.Core;

namespace NTokenizers.Sql;

/// <summary>
/// Metadata for SQL code block tokens with syntax highlighting support.
/// </summary>
public sealed class SqlCodeBlockMetadata(string language, TextReader reader, StringBuilder capture, Queue<char> lookahead, string stopDelimiter) : CodeBlockMetadata<SqlToken>(language, reader, capture, lookahead, stopDelimiter)
{
    internal override BaseSubTokenizer<SqlToken> CreateTokenizer() => SqlTokenizer.Create();
}
