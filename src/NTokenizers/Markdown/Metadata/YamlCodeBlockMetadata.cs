using NTokenizers.Core;
using NTokenizers.Yaml;

namespace NTokenizers.Markdown.Metadata;

/// <summary>
/// Metadata for YAML code block tokens with syntax highlighting support.
/// </summary>
public sealed class YamlCodeBlockMetadata(string language) : CodeBlockMetadata<YamlToken>(language)
{
    internal override BaseSubTokenizer<YamlToken> CreateTokenizer() => YamlTokenizer.Create();
}
