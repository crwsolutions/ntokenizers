namespace NTokenizers.Json;

/// <summary>
/// Represents a JSON token with its type and value.
/// </summary>
/// <param name="TokenType">The type of the JSON token.</param>
/// <param name="Value">The string value of the JSON token.</param>
public record JsonToken(JsonTokenType TokenType, string Value);
