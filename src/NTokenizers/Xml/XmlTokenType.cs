namespace NTokenizers.Xml;

/// <summary>
/// Represents the type of an XML token.
/// </summary>
public enum XmlTokenType
{
    /// <summary>
    /// Represents no specific kind.
    /// </summary>
    None,           // Represents no specific kind
    /// <summary>
    /// Represents an XML element name. eg &lt;element&gt;
    /// </summary>
    ElementName,    // Represents an XML element name. eg <element>
    /// <summary>
    /// Represents text content within an XML element.
    /// </summary>
    Text,           // Represents text content within an XML element
    /// <summary>
    /// Represents an XML comment. Everything including &lt;!-- ... --&gt;
    /// </summary>
    Comment,        // Represents an XML comment. Everything including <!-- ... -->
    /// <summary>
    /// Represents an XML processing instruction. e.g. &lt;?xml version="1.0"?&gt;
    /// </summary>
    ProcessingInstruction, // Represents an XML processing instruction. e.g. <?xml version="1.0"?>
    /// <summary>
    /// A document type declaration, or DOCTYPE, is an instruction that associates a particular XML or SGML document (for example, a web page) with a document type definition (DTD)
    /// </summary>
    DocumentTypeDeclaration, // A document type declaration, or DOCTYPE, is an instruction that associates a particular XML or SGML document (for example, a web page) with a document type definition (DTD)
    /// <summary>
    /// Represents a CDATA section.
    /// </summary>
    CData,          // Represents a CDATA section
    /// <summary>
    /// Represents whitespace.
    /// </summary>
    Whitespace,     // Represents whitespace
    /// <summary>
    /// Represents the end of an XML element (e.g. &lt;element/&gt; ).
    /// </summary>
    EndElement,     // Represents the end of an XML element (e.g. <element/> )
    /// <summary>
    /// Represents an opening angle bracket (&lt;).
    /// </summary>
    OpeningAngleBracket, // <
    /// <summary>
    /// Represents a closing angle bracket (&gt;).
    /// </summary>
    ClosingAngleBracket, // >
    /// <summary>
    /// Name of the attribute.
    /// </summary>
    AttributeName,   // Name of the attribute
    /// <summary>
    /// Represents an equals sign (=) used to separate attribute names and values.
    /// </summary>
    AttributeEquals, // =
    /// <summary>
    /// Represents the value of an XML attribute.
    /// </summary>
    AttributeValue,  // Represents the value of an XML attribute
    /// <summary>
    /// Represents the quote character (&quot; or ') used to wrap attribute values.
    /// </summary>
    AttributeQuote,  // " or '
    /// <summary>
    /// Represents a self-closing slash (/) used in self-closing tags (e.g., &lt;element/&gt;).
    /// </summary>
    SelfClosingSlash  // / => Represents a self-closing tag (e.g., <element/>)
}
