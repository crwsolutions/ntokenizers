using NTokenizers.Core;

namespace NTokenizers.Html;

/// <summary>
/// Metadata for HTML code block tokens with syntax highlighting support.
/// </summary>
public sealed class HtmlCodeBlockMetadata(string language) : CodeBlockMetadata<HtmlToken>(language)
{
    internal override BaseSubTokenizer<HtmlToken> CreateTokenizer() => HtmlTokenizer.Create();
}
