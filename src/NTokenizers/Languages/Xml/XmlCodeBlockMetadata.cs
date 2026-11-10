using System.Text;
using NTokenizers.Core;

namespace NTokenizers.Xml;

/// <summary>
/// Metadata for XML code block tokens with syntax highlighting support.
/// </summary>
public sealed class XmlCodeBlockMetadata(string language, TextReader reader, StringBuilder capture, Queue<char> lookahead, string stopDelimiter) : CodeBlockMetadata<XmlToken>(language, reader, capture, lookahead, stopDelimiter)
{
    internal override BaseSubTokenizer<XmlToken> CreateTokenizer() => XmlTokenizer.Create();
}
