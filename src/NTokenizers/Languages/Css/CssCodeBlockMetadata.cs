using System.Text;
using NTokenizers.Core;

namespace NTokenizers.Css;

/// <summary>
/// Metadata for CSS code block tokens with syntax highlighting support.
/// </summary>
public sealed class CssCodeBlockMetadata(string language, TextReader reader, StringBuilder capture, Queue<char> lookahead, string stopDelimiter) : CodeBlockMetadata<CssToken>(language, reader, capture, lookahead, stopDelimiter)
{
    internal override BaseSubTokenizer<CssToken> CreateTokenizer() => CssTokenizer.Create();
}
