using System.Text;
using NTokenizers.Core;

namespace NTokenizers.Json;

/// <summary>
/// Metadata for JSON code block tokens with syntax highlighting support.
/// </summary>
public sealed class JsonCodeBlockMetadata(string language, TextReader reader, StringBuilder capture, Queue<char> lookahead, string stopDelimiter) : CodeBlockMetadata<JsonToken>(language, reader, capture, lookahead, stopDelimiter)
{
    internal override BaseSubTokenizer<JsonToken> CreateTokenizer() => JsonTokenizer.Create();
}
