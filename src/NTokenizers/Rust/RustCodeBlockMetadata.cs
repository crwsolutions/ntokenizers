using NTokenizers.Core;

namespace NTokenizers.Rust;

/// <summary>
/// Metadata for Rust code block tokens with syntax highlighting support.
/// </summary>
public sealed class RustCodeBlockMetadata(string language) : CodeBlockMetadata<RustToken>(language)
{
    internal override BaseSubTokenizer<RustToken> CreateTokenizer() => RustTokenizer.Create();
}
