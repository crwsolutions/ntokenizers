using NTokenizers.Core;

namespace NTokenizers.Swift;

/// <summary>
/// Metadata for Swift code block tokens with syntax highlighting support.
/// </summary>
public sealed class SwiftCodeBlockMetadata(string language) : CodeBlockMetadata<SwiftToken>(language)
{
    internal override BaseSubTokenizer<SwiftToken> CreateTokenizer() => SwiftTokenizer.Create();
}
