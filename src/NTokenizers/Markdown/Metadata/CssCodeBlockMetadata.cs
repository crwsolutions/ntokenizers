using NTokenizers.Core;
using NTokenizers.Css;

namespace NTokenizers.Markdown.Metadata;

/// <summary>
/// Metadata for CSS code block tokens with syntax highlighting support.
/// </summary>
public sealed class CssCodeBlockMetadata(string language) : CodeBlockMetadata<CssToken>(language)
{
    internal override BaseSubTokenizer<CssToken> CreateTokenizer() => CssTokenizer.Create();
}
