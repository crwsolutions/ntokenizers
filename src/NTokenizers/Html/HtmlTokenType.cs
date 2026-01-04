namespace NTokenizers.Html;

/// <summary>
/// Represents the type of an HTML token.
/// </summary>
public enum HtmlTokenType
{
    /// <summary>
    /// Represents no specific kind.
    /// </summary>
    None,
    /// <summary>
    /// Represents an HTML element name. eg &lt;element&gt;
    /// </summary>
    ElementName,
    /// <summary>
    /// Represents text content within an HTML element.
    /// </summary>
    Text,
    /// <summary>
    /// Represents an HTML comment. Everything including &lt;!-- ... --&gt;
    /// </summary>
    Comment,
    /// <summary>
    /// A document type declaration, or DOCTYPE.
    /// </summary>
    DocumentTypeDeclaration,
    /// <summary>
    /// Represents whitespace.
    /// </summary>
    Whitespace,
    /// <summary>
    /// Represents an opening angle bracket (&lt;).
    /// </summary>
    OpeningAngleBracket,
    /// <summary>
    /// Represents a closing angle bracket (&gt;).
    /// </summary>
    ClosingAngleBracket,
    /// <summary>
    /// Name of the attribute.
    /// </summary>
    AttributeName,
    /// <summary>
    /// Represents an equals sign (=) used to separate attribute names and values.
    /// </summary>
    AttributeEquals,
    /// <summary>
    /// Represents the value of an HTML attribute.
    /// </summary>
    AttributeValue,
    /// <summary>
    /// Represents the quote character (&quot; or ') used to wrap attribute values.
    /// </summary>
    AttributeQuote,
    /// <summary>
    /// Represents a self-closing slash (/) used in self-closing tags (e.g., &lt;br/&gt;).
    /// </summary>
    SelfClosingSlash
}
