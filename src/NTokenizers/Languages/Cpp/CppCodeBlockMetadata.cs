using NTokenizers.Core;

namespace NTokenizers.Cpp;

/// <summary>
/// Metadata for C++ code block tokens with syntax highlighting support.
/// </summary>
public sealed class CppCodeBlockMetadata(string language) : CodeBlockMetadata<CppToken>(language)
{
    internal override BaseSubTokenizer<CppToken> CreateTokenizer() => CppTokenizer.Create();
}
