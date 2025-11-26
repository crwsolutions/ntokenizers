namespace NTokenizers.Json;

/// <summary>
/// Represents a JSON token with its type and value.
/// </summary>
public class JsonToken : IToken<JsonTokenType>
{
    /// <summary>
    /// Gets the type of the current JSON token.
    /// </summary>
    public JsonTokenType TokenType { get; }

    /// <summary>
    /// Gets the current value as a string.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonToken"/> class with the specified token type and value.
    /// </summary>
    /// <param name="tokenType">The type of the JSON token.</param>
    /// <param name="value">The string representation of the JSON token's value.</param>
    public JsonToken(JsonTokenType tokenType, string value)
    {
        TokenType = tokenType;
        Value = value;
    }
}
