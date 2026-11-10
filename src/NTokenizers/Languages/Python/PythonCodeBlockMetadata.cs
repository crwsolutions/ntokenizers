using System.Text;
using NTokenizers.Core;

namespace NTokenizers.Python;

/// <summary>
/// Metadata for Python code block tokens with syntax highlighting support.
/// </summary>
public sealed class PythonCodeBlockMetadata(string language, TextReader reader, StringBuilder capture, Queue<char> lookahead, string stopDelimiter) : CodeBlockMetadata<PythonToken>(language, reader, capture, lookahead, stopDelimiter)
{
    internal override BaseSubTokenizer<PythonToken> CreateTokenizer() => PythonTokenizer.Create();
}
