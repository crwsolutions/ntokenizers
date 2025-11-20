namespace NTokenizers.Markup;

/// <summary>
/// Represents the type of a Markup token.
/// </summary>
public enum MarkupTokenType
{
    /// <summary>
    /// Represents plain text content.
    /// </summary>
    Text,
    
    /// <summary>
    /// Represents bold text. Value contains only the text content without ** markers.
    /// </summary>
    Bold,
    
    /// <summary>
    /// Represents italic text. Value contains only the text content without * markers.
    /// </summary>
    Italic,
    
    /// <summary>
    /// Represents a heading. Value contains only the heading text without # markers. Level is stored in Metadata.
    /// </summary>
    Heading,
    
    /// <summary>
    /// Represents a horizontal rule (--- or ***).
    /// </summary>
    HorizontalRule,
    
    /// <summary>
    /// Represents a typographic replacement ((c), (r), (tm), +-).
    /// </summary>
    TypographicReplacement,
    
    /// <summary>
    /// Represents a generic emphasis marker.
    /// </summary>
    Emphasis,
    
    /// <summary>
    /// Represents a blockquote. Value contains the quoted text without > marker.
    /// </summary>
    Blockquote,
    
    /// <summary>
    /// Represents an unordered list item. Value contains the item text without +, -, * markers.
    /// </summary>
    UnorderedListItem,
    
    /// <summary>
    /// Represents an ordered list item. Value contains the item text without number prefix. Item number is stored in Metadata.
    /// </summary>
    OrderedListItem,
    
    /// <summary>
    /// Represents inline code. Value contains the code content without ` markers.
    /// </summary>
    CodeInline,
    
    /// <summary>
    /// Represents a code block. Value contains the code content without ``` markers. Language is stored in Metadata.
    /// </summary>
    CodeBlock,
    
    /// <summary>
    /// Represents a table cell. Value contains the cell content without | delimiters. Position and alignment stored in Metadata.
    /// </summary>
    TableCell,
    
    /// <summary>
    /// Represents a link. Value contains the link text without [ ] markers. URL and title stored in Metadata.
    /// </summary>
    Link,
    
    /// <summary>
    /// Represents an image. Value contains the alt text without ![ ] markers. URL and title stored in Metadata.
    /// </summary>
    Image,
    
    /// <summary>
    /// Represents an emoji. Value contains the emoji name without : markers, stored in Metadata as well.
    /// </summary>
    Emoji,
    
    /// <summary>
    /// Represents subscript text. Value contains only the subscript content without ^ markers.
    /// </summary>
    Subscript,
    
    /// <summary>
    /// Represents superscript text. Value contains only the superscript content without ~ markers.
    /// </summary>
    Superscript,
    
    /// <summary>
    /// Represents inserted text. Value contains only the content without ++ markers.
    /// </summary>
    InsertedText,
    
    /// <summary>
    /// Represents marked text. Value contains only the content without == markers.
    /// </summary>
    MarkedText,
    
    /// <summary>
    /// Represents a footnote reference. Value contains the reference ID without [^ ] markers.
    /// </summary>
    FootnoteReference,
    
    /// <summary>
    /// Represents a footnote definition. Value contains the definition content. ID stored in Metadata.
    /// </summary>
    FootnoteDefinition,
    
    /// <summary>
    /// Represents a definition term.
    /// </summary>
    DefinitionTerm,
    
    /// <summary>
    /// Represents a definition description. Value contains the description without : marker.
    /// </summary>
    DefinitionDescription,
    
    /// <summary>
    /// Represents an abbreviation. Value contains the definition. Abbreviation stored in Metadata.
    /// </summary>
    Abbreviation,
    
    /// <summary>
    /// Represents a custom container. Value contains the container type/name without ::: markers.
    /// </summary>
    CustomContainer,
    
    /// <summary>
    /// Represents an HTML tag. Value contains the complete tag including &lt; &gt; markers as they are part of HTML syntax.
    /// </summary>
    HtmlTag
}
