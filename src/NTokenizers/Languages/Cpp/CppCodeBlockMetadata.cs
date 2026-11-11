using System.Text;
using NTokenizers.Core;

namespace NTokenizers.Cpp;

/// <summary>
/// Metadata for C++ code block tokens with syntax highlighting support.
/// </summary>
public sealed class CppCodeBlockMetadata(string language, TextReader reader, StringBuilder capture, Queue<char> lookahead, string stopDelimiter) : CodeBlockMetadata<CppToken>(language, reader, capture, lookahead, stopDelimiter)
{
    internal override BaseSubTokenizer<CppToken> CreateTokenizer() => CppTokenizer.Create();
}
