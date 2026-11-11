using System.Text;
using NTokenizers.Core;

namespace NTokenizers.Yaml;

/// <summary>
/// Metadata for YAML code block tokens with syntax highlighting support.
/// </summary>
public sealed class YamlCodeBlockMetadata(string language, TextReader reader, StringBuilder capture, Queue<char> lookahead, string stopDelimiter) : CodeBlockMetadata<YamlToken>(language, reader, capture, lookahead, stopDelimiter)
{
    internal override BaseSubTokenizer<YamlToken> CreateTokenizer() => YamlTokenizer.Create();
}
