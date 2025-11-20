namespace NTokenizers.Markup;

/// <summary>
/// Base class for markup token metadata.
/// </summary>
public abstract record MarkupMetadata;

/// <summary>
/// Metadata for heading tokens, containing the heading level.
/// </summary>
/// <param name="Level">The heading level (1-6).</param>
public record HeadingMetadata(int Level) : MarkupMetadata
{
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
public record CodeBlockMetadata<TToken>(string Language) : MarkupMetadata
{
    /// <summary>
    /// Optional callback to stream syntax-highlighted tokens from the code block.
    /// When set, the tokenizer will delegate to language-specific tokenizers and emit tokens via this callback.
    /// </summary>
    public Action<TToken>? OnInlineToken { get; set; }
}

/// <summary>
/// Metadata for C# code block tokens with syntax highlighting support.
/// </summary>
public record CSharpCodeBlockMetadata() : CodeBlockMetadata<CSharp.CSharpToken>("csharp");

/// <summary>
/// Metadata for JSON code block tokens with syntax highlighting support.
/// </summary>
public record JsonCodeBlockMetadata() : CodeBlockMetadata<Json.JsonToken>("json");

/// <summary>
/// Metadata for XML code block tokens with syntax highlighting support.
/// </summary>
public record XmlCodeBlockMetadata() : CodeBlockMetadata<Xml.XmlToken>("xml");

/// <summary>
/// Metadata for SQL code block tokens with syntax highlighting support.
/// </summary>
public record SqlCodeBlockMetadata() : CodeBlockMetadata<Sql.SqlToken>("sql");

/// <summary>
/// Metadata for TypeScript code block tokens with syntax highlighting support.
/// </summary>
public record TypeScriptCodeBlockMetadata() : CodeBlockMetadata<Typescript.TypescriptToken>("typescript");

/// <summary>
/// Metadata for link and image tokens, containing URL and optional title.
/// </summary>
/// <param name="Url">The URL.</param>
/// <param name="Title">Optional title.</param>
public record LinkMetadata(string Url, string? Title = null) : MarkupMetadata;

/// <summary>
/// Metadata for blockquote tokens.
/// </summary>
public record BlockquoteMetadata() : MarkupMetadata
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
public record FootnoteMetadata(string Id) : MarkupMetadata;

/// <summary>
/// Metadata for emoji tokens, containing the emoji name.
/// </summary>
/// <param name="Name">The emoji name (e.g., "wink").</param>
public record EmojiMetadata(string Name) : MarkupMetadata;

/// <summary>
/// Metadata for list item tokens, containing the item number for ordered lists.
/// </summary>
/// <param name="Number">The item number for ordered lists.</param>
public record ListItemMetadata(int Number) : MarkupMetadata
{
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
public record TableMetadata(
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
public record TableColumnMetadata(
    int Index,
    Justify Alignment,
    bool IsHeader
);

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
public record MarkupToken(
    MarkupTokenType TokenType,
    string Value,
    MarkupMetadata? Metadata = null
);
