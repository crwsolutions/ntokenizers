using System.Text;
using NTokenizers.Core;

namespace NTokenizers.Swift;

/// <summary>
/// Metadata for Swift code block tokens with syntax highlighting support.
/// </summary>
public sealed class SwiftCodeBlockMetadata(string language, TextReader reader, StringBuilder capture, Queue<char> lookahead, string stopDelimiter) : CodeBlockMetadata<SwiftToken>(language, reader, capture, lookahead, stopDelimiter)
{
    internal override BaseSubTokenizer<SwiftToken> CreateTokenizer() => SwiftTokenizer.Create();
}
