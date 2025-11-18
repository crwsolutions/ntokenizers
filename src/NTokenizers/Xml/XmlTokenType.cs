namespace NTokenizers.Xml;
public enum XmlTokenType
{
    None,           // Represents no specific kind
    ElementName,    // Represents an XML element name. eg <element>
    Text,           // Represents text content within an XML element
    Comment,        // Represents an XML comment. Everything including <!-- ... -->
    ProcessingInstruction, // Represents an XML processing instruction. e.g. <?xml version="1.0"?>
    DocumentTypeDeclaration, // A document type declaration, or DOCTYPE, is an instruction that associates a particular XML or SGML document (for example, a web page) with a document type definition (DTD)
    CData,          // Represents a CDATA section
    Whitespace,     // Represents whitespace
    EndElement,     // Represents the end of an XML element (e.g. <element/> )
    OpeningAngleBracket, // <
    ClosingAngleBracket, // >
    AttributeName,   // Name of the attribute
    AttributeEquals, // =
    AttributeValue,  // Represents the value of an XML attribute
    AttributeQuote,  // " or '
    SelfClosingSlash  // / => Represents a self-closing tag (e.g., <element/>)
}
