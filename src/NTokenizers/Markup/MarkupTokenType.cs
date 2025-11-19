namespace NTokenizers.Markup;

/// <summary>
/// Represents the type of a Markup token.
/// </summary>
public enum MarkupTokenType
{
    /// <summary>
    /// Represents plain text content between delimiters.
    /// </summary>
    Text,
    
    /// <summary>
    /// Represents a bold delimiter (**).
    /// </summary>
    BoldDelimiter,
    
    /// <summary>
    /// Represents an italic delimiter (*).
    /// </summary>
    ItalicDelimiter,
    
    /// <summary>
    /// Represents a heading delimiter (#, ##, ### ... ######). Level is stored in Metadata.
    /// </summary>
    HeadingDelimiter,
    
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
    /// Represents a blockquote delimiter (&gt;).
    /// </summary>
    BlockquoteDelimiter,
    
    /// <summary>
    /// Represents an unordered list delimiter (+, -, *).
    /// </summary>
    UnorderedListDelimiter,
    
    /// <summary>
    /// Represents an ordered list delimiter (1., 2., etc.).
    /// </summary>
    OrderedListDelimiter,
    
    /// <summary>
    /// Represents inline code (`code`).
    /// </summary>
    CodeInline,
    
    /// <summary>
    /// Represents the start of a code block fence (```language).
    /// </summary>
    CodeBlockFenceStart,
    
    /// <summary>
    /// Represents the end of a code block fence (```).
    /// </summary>
    CodeBlockFenceEnd,
    
    /// <summary>
    /// Represents content within a code block.
    /// </summary>
    CodeBlockContent,
    
    /// <summary>
    /// Represents a table delimiter (|).
    /// </summary>
    TableDelimiter,
    
    /// <summary>
    /// Represents link text ([link text]).
    /// </summary>
    LinkText,
    
    /// <summary>
    /// Represents a link URL ((http://...)).
    /// </summary>
    LinkUrl,
    
    /// <summary>
    /// Represents a link title ("title text").
    /// </summary>
    LinkTitle,
    
    /// <summary>
    /// Represents image alt text (![alt]).
    /// </summary>
    ImageAlt,
    
    /// <summary>
    /// Represents an image URL ((url)).
    /// </summary>
    ImageUrl,
    
    /// <summary>
    /// Represents an image title ("title").
    /// </summary>
    ImageTitle,
    
    /// <summary>
    /// Represents an emoji (:wink:).
    /// </summary>
    Emoji,
    
    /// <summary>
    /// Represents subscript text (^th^).
    /// </summary>
    Subscript,
    
    /// <summary>
    /// Represents superscript text (~2~).
    /// </summary>
    Superscript,
    
    /// <summary>
    /// Represents inserted text (++Inserted++).
    /// </summary>
    InsertedText,
    
    /// <summary>
    /// Represents marked text (==Marked==).
    /// </summary>
    MarkedText,
    
    /// <summary>
    /// Represents a footnote reference ([^id]).
    /// </summary>
    FootnoteReference,
    
    /// <summary>
    /// Represents a footnote definition ([^id]: ...).
    /// </summary>
    FootnoteDefinition,
    
    /// <summary>
    /// Represents a definition term.
    /// </summary>
    DefinitionTerm,
    
    /// <summary>
    /// Represents a definition description (: Definition).
    /// </summary>
    DefinitionDescription,
    
    /// <summary>
    /// Represents an abbreviation (*[HTML]: ...).
    /// </summary>
    Abbreviation,
    
    /// <summary>
    /// Represents a custom container (::: warning).
    /// </summary>
    CustomContainer,
    
    /// <summary>
    /// Represents an HTML tag (&lt;html&gt;, &lt;/html&gt;, etc.).
    /// </summary>
    HtmlTag
}
