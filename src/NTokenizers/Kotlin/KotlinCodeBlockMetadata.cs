using NTokenizers.Core;

namespace NTokenizers.Kotlin;

/// <summary>
/// Metadata for Kotlin code block tokens with syntax highlighting support.
/// </summary>
public sealed class KotlinCodeBlockMetadata(string language) : CodeBlockMetadata<KotlinToken>(language)
{
    internal override BaseSubTokenizer<KotlinToken> CreateTokenizer() => KotlinTokenizer.Create();
}
