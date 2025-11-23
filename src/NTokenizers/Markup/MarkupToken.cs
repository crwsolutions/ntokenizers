using System.Diagnostics;

namespace NTokenizers.Markup;

/// <summary>
/// Base class for markup token metadata.
/// </summary>
public abstract class MarkupMetadata;

/// <summary>
/// Metadata for heading tokens, containing the heading level.
/// </summary>
/// <param name="Level">The heading level (1-6).</param>
public class HeadingMetadata(int Level) : MarkupMetadata
{
    /// <summary>
    /// Gets the current level of the heading (1-6).
    /// </summary>
    public int Level { get; } = Level;

    /// <summary>
    /// Optional callback to stream inline tokens (bold, italic, etc.) within the heading.
    /// When set, the tokenizer will parse inline content and emit tokens via this callback.
    /// </summary>
    public Action<MarkupToken>? OnInlineToken { get; set; }
}

/// <summary>
/// Metadata for code block tokens, containing the language identifier.
/// </summary>
/// <typeparam name="TToken">The type of tokens emitted by the language-specific tokenizer.</typeparam>
/// <param name="Language">The language identifier (e.g., "xml", "json", "csharp").</param>
public abstract class CodeBlockMetadata<TToken>(string Language) : MarkupMetadata
{
    /// <summary>
    /// Gets the language code representing the current language setting.
    /// </summary>
    public string Language { get; } = Language;

    /// <summary>
    /// Optional callback to stream syntax-highlighted tokens from the code block.
    /// When set, the tokenizer will delegate to language-specific tokenizers and emit tokens via this callback.
    /// </summary>
    public Action<TToken>? OnInlineToken { get; set; }
}

/// <summary>
/// Represents metadata for a generic code block, including its associated code content.
/// </summary>
/// <remarks>This class is a specialized implementation of <see cref="CodeBlockMetadata{MarkupToken}"/> designed
/// to handle generic code blocks. It provides functionality to manage and process the code content associated with the
/// block.</remarks>
/// <param name="language">The code content associated with the generic code block.</param>
public class GenericCodeBlockMetadata(string language) : CodeBlockMetadata<MarkupToken>(language);

/// <summary>
/// Metadata for C# code block tokens with syntax highlighting support.
/// </summary>
public class CSharpCodeBlockMetadata(string language) : CodeBlockMetadata<CSharp.CSharpToken>(language);

/// <summary>
/// Metadata for JSON code block tokens with syntax highlighting support.
/// </summary>
public class JsonCodeBlockMetadata(string language) : CodeBlockMetadata<Json.JsonToken>(language);

/// <summary>
/// Metadata for XML code block tokens with syntax highlighting support.
/// </summary>
public class XmlCodeBlockMetadata(string language) : CodeBlockMetadata<Xml.XmlToken>(language);

/// <summary>
/// Metadata for SQL code block tokens with syntax highlighting support.
/// </summary>
public class SqlCodeBlockMetadata(string language) : CodeBlockMetadata<Sql.SqlToken>(language);

/// <summary>
/// Metadata for TypeScript code block tokens with syntax highlighting support.
/// </summary>
public class TypeScriptCodeBlockMetadata(string language) : CodeBlockMetadata<Typescript.TypescriptToken>(language);

/// <summary>
/// Metadata for link and image tokens, containing URL and optional title.
/// </summary>
/// <param name="Url">The URL.</param>
/// <param name="Title">Optional title.</param>
public class LinkMetadata(string Url, string? Title = null) : MarkupMetadata
{
    /// <summary>
    /// Gets the URL associated with the link or image.
    /// </summary>
    public string Url { get; } = Url;
    /// <summary>
    /// Gets the optional title associated with the link or image.
    /// </summary>
    public string? Title { get; } = Title;
}

/// <summary>
/// Metadata for blockquote tokens.
/// </summary>
public class BlockquoteMetadata() : MarkupMetadata
{
    /// <summary>
    /// Optional callback to stream inline tokens (bold, italic, etc.) within the blockquote.
    /// When set, the tokenizer will parse inline content and emit tokens via this callback.
    /// </summary>
    public Action<MarkupToken>? OnInlineToken { get; set; }
}

/// <summary>
/// Metadata for footnote tokens, containing the footnote identifier.
/// </summary>
/// <param name="Id">The footnote identifier.</param>
public class FootnoteMetadata(string Id) : MarkupMetadata
{
    /// <summary>
    /// Gets the footnote identifier.
    /// </summary>
    public string Id { get; } = Id;
}

/// <summary>
/// Metadata for emoji tokens, containing the emoji name.
/// </summary>
/// <param name="Name">The emoji name (e.g., "wink").</param>
public class EmojiMetadata(string Name) : MarkupMetadata
{
    /// <summary>
    /// Gets the emoji name.
    /// </summary>
    public string Name { get; } = Name;
}

/// <summary>
/// Metadata for unordered list item tokens
/// </summary>
/// <param name="Marker">The marker of the lists.</param>
public class ListItemMetadata(char Marker) : MarkupMetadata
{
    /// <summary>
    /// Gets the marker of the list.
    /// </summary>
    public char Marker { get; } = Marker;
    /// <summary>
    /// Optional callback to stream inline tokens (bold, italic, etc.) within the list item.
    /// When set, the tokenizer will parse inline content and emit tokens via this callback.
    /// </summary>
    public Action<MarkupToken>? OnInlineToken { get; set; }
}

/// <summary>
/// Metadata for list item tokens, containing the item number for ordered lists.
/// </summary>
/// <param name="Number">The item number for ordered lists.</param>
public class OrderedListItemMetadata(int Number) : MarkupMetadata
{
    /// <summary>
    /// Gets the current number value within the list.
    /// </summary>
    public int Number { get; } = Number;
    /// <summary>
    /// Optional callback to stream inline tokens (bold, italic, etc.) within the list item.
    /// When set, the tokenizer will parse inline content and emit tokens via this callback.
    /// </summary>
    public Action<MarkupToken>? OnInlineToken { get; set; }
}

/// <summary>
/// Metadata for table tokens, containing column information.
/// </summary>
/// <param name="Columns">List of column metadata.</param>
public class TableMetadata(
    IReadOnlyList<TableColumnMetadata> Columns
) : MarkupMetadata
{
    /// <summary>
    /// Optional callback to stream inline tokens (bold, italic, etc.) within table cells.
    /// When set, the tokenizer will parse inline content and emit tokens via this callback.
    /// </summary>
    public Action<MarkupToken>? OnInlineToken { get; set; }
}

/// <summary>
/// Metadata for individual table columns.
/// </summary>
/// <param name="Index">The column index.</param>
/// <param name="Alignment">The column text alignment.</param>
/// <param name="IsHeader">Whether this column is in the header row.</param>
public class TableColumnMetadata(
    int Index,
    Justify Alignment,
    bool IsHeader
)
    {
    /// <summary>
    /// Gets the column index (0-based).
    /// </summary>
    public int Index { get; } = Index;
    /// <summary>
    /// Gets the text alignment for the column.
    /// </summary>
    public Justify Alignment { get; } = Alignment;
    /// <summary>
    /// Gets a value indicating whether this column is part of the header row.
    /// </summary>
    public bool IsHeader { get; } = IsHeader;
}

/// <summary>
/// Text alignment/justification options for table columns.
/// </summary>
public enum Justify
{
    /// <summary>
    /// Left-aligned text.
    /// </summary>
    Left,
    
    /// <summary>
    /// Center-aligned text.
    /// </summary>
    Center,
    
    /// <summary>
    /// Right-aligned text.
    /// </summary>
    Right
}

/// <summary>
/// Represents a markup token with its type, value, and optional metadata.
/// </summary>
/// <param name="TokenType">The type of the markup token.</param>
/// <param name="Value">The string value of the markup token (renderable content only, no syntax markers).</param>
/// <param name="Metadata">Optional metadata associated with the token.</param>
[DebuggerDisplay("MarkupToken: {TokenType} {Value}")]
public class MarkupToken(
    MarkupTokenType TokenType,
    string Value,
    MarkupMetadata? Metadata = null
)
{
    /// <summary>
    /// Gets the type of the markup token.
    /// </summary>
    public MarkupTokenType TokenType { get; } = TokenType;
    /// <summary>
    /// Gets the string value of the markup token (renderable content only, no syntax markers).
    /// </summary>
    public string Value { get; } = Value;
    /// <summary>
    /// Gets the optional metadata associated with the token.
    /// </summary>
    public MarkupMetadata? Metadata { get; } = Metadata;
}
