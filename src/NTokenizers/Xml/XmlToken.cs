namespace NTokenizers.Xml;

/// <summary>
/// Represents an XML token with its type and value.
/// </summary>
/// <param name="TokenType">The type of the XML token.</param>
/// <param name="Value">The string value of the XML token.</param>
public record XmlToken(XmlTokenType TokenType, string Value);
