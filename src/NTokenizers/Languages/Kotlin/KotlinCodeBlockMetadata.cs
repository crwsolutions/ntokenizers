using System.Text;
using NTokenizers.Core;

namespace NTokenizers.Kotlin;

/// <summary>
/// Metadata for Kotlin code block tokens with syntax highlighting support.
/// </summary>
public sealed class KotlinCodeBlockMetadata(string language, TextReader reader, StringBuilder capture, Queue<char> lookahead, string stopDelimiter) : CodeBlockMetadata<KotlinToken>(language, reader, capture, lookahead, stopDelimiter)
{
    internal override BaseSubTokenizer<KotlinToken> CreateTokenizer() => KotlinTokenizer.Create();
}
