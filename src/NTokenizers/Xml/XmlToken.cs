namespace NTokenizers.Xml;

/// <summary>
/// Represents an XML token with its type and value.
/// </summary>
/// <param name="TokenType">The type of the XML token.</param>
/// <param name="Value">The string value of the XML token.</param>
public class XmlToken(XmlTokenType TokenType, string Value)
{
    /// <summary>
    /// Gets the type of the XML token.
    /// </summary>
    public XmlTokenType TokenType { get; } = TokenType;

    /// <summary>
    /// Gets the value associated with this instance.
    /// </summary>
    public string Value { get; } = Value;
}
