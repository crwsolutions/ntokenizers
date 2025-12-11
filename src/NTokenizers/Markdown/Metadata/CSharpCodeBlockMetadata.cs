using NTokenizers.Core;
using NTokenizers.CSharp;

namespace NTokenizers.Markdown.Metadata;

/// <summary>
/// Metadata for C# code block tokens with syntax highlighting support.
/// </summary>
public sealed class CSharpCodeBlockMetadata(string language) : CodeBlockMetadata<CSharpToken>(language)
{
    internal override BaseSubTokenizer<CSharpToken> CreateTokenizer() => CSharpTokenizer.Create();
}
