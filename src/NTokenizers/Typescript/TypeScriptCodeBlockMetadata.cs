using NTokenizers.Core;

namespace NTokenizers.Typescript;

/// <summary>
/// Metadata for TypeScript code block tokens with syntax highlighting support.
/// </summary>
public sealed class TypeScriptCodeBlockMetadata(string language) : CodeBlockMetadata<TypescriptToken>(language)
{
    internal override BaseSubTokenizer<TypescriptToken> CreateTokenizer() => TypescriptTokenizer.Create();
}
