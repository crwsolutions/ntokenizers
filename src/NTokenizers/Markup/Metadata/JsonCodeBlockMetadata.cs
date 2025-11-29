namespace NTokenizers.Markup.Metadata;

/// <summary>
/// Metadata for JSON code block tokens with syntax highlighting support.
/// </summary>
public sealed class JsonCodeBlockMetadata(string language) : CodeBlockMetadata<Json.JsonToken>(language);
