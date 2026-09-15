using NTokenizers.Core;
using System.Diagnostics;

namespace NTokenizers.Xml;

/// <summary>
/// Represents an XML token with its type and value.
/// </summary>
/// <param name="TokenType">The type of the XML token.</param>
/// <param name="Value">The string value of the XML token.</param>
[DebuggerDisplay("XmlTokenType: {TokenType} '{Value}'")]
public class XmlToken(XmlTokenType TokenType, string Value) : IToken<XmlTokenType>
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
