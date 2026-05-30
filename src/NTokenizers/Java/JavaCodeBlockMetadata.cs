using NTokenizers.Core;

namespace NTokenizers.Java;

/// <summary>
/// Metadata for Java code block tokens with syntax highlighting support.
/// </summary>
public sealed class JavaCodeBlockMetadata(string language) : CodeBlockMetadata<JavaToken>(language)
{
    internal override BaseSubTokenizer<JavaToken> CreateTokenizer() => JavaTokenizer.Create();
}
