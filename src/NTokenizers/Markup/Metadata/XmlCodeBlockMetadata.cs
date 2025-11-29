namespace NTokenizers.Markup.Metadata;

/// <summary>
/// Metadata for XML code block tokens with syntax highlighting support.
/// </summary>
public sealed class XmlCodeBlockMetadata(string language) : CodeBlockMetadata<Xml.XmlToken>(language);
