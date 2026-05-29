using NTokenizers.Core;

namespace NTokenizers.Kotlin;

public sealed class KotlinCodeBlockMetadata(string language) : CodeBlockMetadata<KotlinToken>(language)
{
    internal override BaseSubTokenizer<KotlinToken> CreateTokenizer() => KotlinTokenizer.Create();
}
