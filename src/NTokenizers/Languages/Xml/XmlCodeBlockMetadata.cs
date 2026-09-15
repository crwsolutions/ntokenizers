using NTokenizers.Core;

namespace NTokenizers.Xml;

/// <summary>
/// Metadata for XML code block tokens with syntax highlighting support.
/// </summary>
public sealed class XmlCodeBlockMetadata (string language) : CodeBlockMetadata<XmlToken>(language)
{
    internal override BaseSubTokenizer<XmlToken> CreateTokenizer() => XmlTokenizer.Create();
}
