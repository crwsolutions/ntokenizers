using System.Text;
using NTokenizers.Core;

namespace NTokenizers.C;

/// <summary>
/// Metadata for C code block tokens with syntax highlighting support.
/// </summary>
public sealed class CCodeBlockMetadata(string language, TextReader reader, StringBuilder capture, Queue<char> lookahead, string stopDelimiter) : CodeBlockMetadata<CToken>(language, reader, capture, lookahead, stopDelimiter)
{
    internal override BaseSubTokenizer<CToken> CreateTokenizer() => CTokenizer.Create();
}
