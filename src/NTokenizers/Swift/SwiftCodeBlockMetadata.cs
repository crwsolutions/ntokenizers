using NTokenizers.Core;

namespace NTokenizers.Swift;

public sealed class SwiftCodeBlockMetadata(string language) : CodeBlockMetadata<SwiftToken>(language)
{
    internal override BaseSubTokenizer<SwiftToken> CreateTokenizer() => SwiftTokenizer.Create();
}
