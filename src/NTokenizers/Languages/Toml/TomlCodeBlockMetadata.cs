using System.Text;
using NTokenizers.Core;

namespace NTokenizers.Toml;

/// <summary>
/// Metadata for TOML code block tokens with syntax highlighting support.
/// </summary>
public sealed class TomlCodeBlockMetadata(string language, TextReader reader, StringBuilder capture, Queue<char> lookahead, string stopDelimiter) : CodeBlockMetadata<TomlToken>(language, reader, capture, lookahead, stopDelimiter)
{
    internal override BaseSubTokenizer<TomlToken> CreateTokenizer() => TomlTokenizer.Create();
}
