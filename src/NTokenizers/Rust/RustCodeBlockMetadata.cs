using NTokenizers.Core;

namespace NTokenizers.Rust;

public sealed class RustCodeBlockMetadata(string language) : CodeBlockMetadata<RustToken>(language)
{
    internal override BaseSubTokenizer<RustToken> CreateTokenizer() => RustTokenizer.Create();
}
