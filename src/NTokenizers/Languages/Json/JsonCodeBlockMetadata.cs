using NTokenizers.Core;

namespace NTokenizers.Json;

/// <summary>
/// Metadata for JSON code block tokens with syntax highlighting support.
/// </summary>
public sealed class JsonCodeBlockMetadata(string language) : CodeBlockMetadata<JsonToken>(language)
{
    internal override BaseSubTokenizer<JsonToken> CreateTokenizer() => JsonTokenizer.Create();
}
