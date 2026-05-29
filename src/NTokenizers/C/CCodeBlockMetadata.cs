using NTokenizers.Core;

namespace NTokenizers.C;

/// <summary>
/// Metadata for C code block tokens with syntax highlighting support.
/// </summary>
public sealed class CCodeBlockMetadata(string language) : CodeBlockMetadata<CToken>(language)
{
    internal override BaseSubTokenizer<CToken> CreateTokenizer() => CTokenizer.Create();
}
