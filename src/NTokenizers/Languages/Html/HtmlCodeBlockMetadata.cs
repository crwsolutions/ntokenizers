using System.Text;
using NTokenizers.Core;

namespace NTokenizers.Html;

/// <summary>
/// Metadata for HTML code block tokens with syntax highlighting support.
/// </summary>
public sealed class HtmlCodeBlockMetadata(string language, TextReader reader, StringBuilder capture, Queue<char> lookahead, string stopDelimiter) : CodeBlockMetadata<HtmlToken>(language, reader, capture, lookahead, stopDelimiter)
{
    internal override BaseSubTokenizer<HtmlToken> CreateTokenizer() => HtmlTokenizer.Create();
}
