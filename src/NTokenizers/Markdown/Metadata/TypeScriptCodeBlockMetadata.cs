using NTokenizers.Core;
using NTokenizers.Typescript;

namespace NTokenizers.Markdown.Metadata;

/// <summary>
/// Metadata for TypeScript code block tokens with syntax highlighting support.
/// </summary>
public sealed class TypeScriptCodeBlockMetadata(string language) : CodeBlockMetadata<Typescript.TypescriptToken>(language)
{
    internal override BaseSubTokenizer<TypescriptToken> CreateTokenizer() => TypescriptTokenizer.Create();
}
