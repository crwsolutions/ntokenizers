using System.Text;
using NTokenizers.Core;

namespace NTokenizers.Go;

/// <summary>
/// Metadata for Go code block tokens with syntax highlighting support.
/// </summary>
public sealed class GoCodeBlockMetadata(string language, TextReader reader, StringBuilder capture, Queue<char> lookahead, string stopDelimiter) : CodeBlockMetadata<GoToken>(language, reader, capture, lookahead, stopDelimiter)
{
    internal override BaseSubTokenizer<GoToken> CreateTokenizer() => GoTokenizer.Create();
}
