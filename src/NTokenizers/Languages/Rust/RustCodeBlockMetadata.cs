using System.Text;
using NTokenizers.Core;

namespace NTokenizers.Rust;

/// <summary>
/// Metadata for Rust code block tokens with syntax highlighting support.
/// </summary>
public sealed class RustCodeBlockMetadata(string language, TextReader reader, StringBuilder capture, Queue<char> lookahead, string stopDelimiter) : CodeBlockMetadata<RustToken>(language, reader, capture, lookahead, stopDelimiter)
{
    internal override BaseSubTokenizer<RustToken> CreateTokenizer() => RustTokenizer.Create();
}
