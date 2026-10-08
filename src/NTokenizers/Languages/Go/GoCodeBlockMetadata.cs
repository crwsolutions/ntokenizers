using NTokenizers.Core;

namespace NTokenizers.Go;

/// <summary>
/// Metadata for Go code block tokens with syntax highlighting support.
/// </summary>
public sealed class GoCodeBlockMetadata(string language) : CodeBlockMetadata<GoToken>(language)
{
    internal override BaseSubTokenizer<GoToken> CreateTokenizer() => GoTokenizer.Create();
}
