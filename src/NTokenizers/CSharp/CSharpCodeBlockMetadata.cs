using NTokenizers.Core;

namespace NTokenizers.CSharp;

/// <summary>
/// Metadata for C# code block tokens with syntax highlighting support.
/// </summary>
public sealed class CSharpCodeBlockMetadata(string language) : CodeBlockMetadata<CSharpToken>(language)
{
    internal override BaseSubTokenizer<CSharpToken> CreateTokenizer() => CSharpTokenizer.Create();
}
