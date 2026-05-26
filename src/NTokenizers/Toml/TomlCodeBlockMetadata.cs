using NTokenizers.Core;

namespace NTokenizers.Toml;

/// <summary>
/// Metadata for TOML code block tokens with syntax highlighting support.
/// </summary>
public sealed class TomlCodeBlockMetadata(string language) : CodeBlockMetadata<TomlToken>(language)
{
    internal override BaseSubTokenizer<TomlToken> CreateTokenizer() => TomlTokenizer.Create();
}
