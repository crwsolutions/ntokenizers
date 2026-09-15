namespace NTokenizers.Json;

/// <summary>
/// Represents the type of a JSON token.
/// </summary>
public enum JsonTokenType
{
    /// <summary>
    /// Represents the start of a JSON object ({).
    /// </summary>
    StartObject,   // {
    /// <summary>
    /// Represents the end of a JSON object (}).
    /// </summary>
    EndObject,     // }
    /// <summary>
    /// Represents the start of a JSON array ([).
    /// </summary>
    StartArray,    // [
    /// <summary>
    /// Represents the end of a JSON array (]).
    /// </summary>
    EndArray,      // ]
    /// <summary>
    /// Represents a property name in a JSON object.
    /// </summary>
    PropertyName,  // string used as object key
    /// <summary>
    /// Represents a string value in JSON.
    /// </summary>
    StringValue,   // string value
    /// <summary>
    /// Represents a number value in JSON (integer, float, scientific notation).
    /// </summary>
    Number,        // integer, float, scientific notation
    /// <summary>
    /// Represents the boolean value true.
    /// </summary>
    True,
    /// <summary>
    /// Represents the boolean value false.
    /// </summary>
    False,
    /// <summary>
    /// Represents the null value in JSON.
    /// </summary>
    Null,          // null
    /// <summary>
    /// Represents a colon (:) used to separate keys and values in JSON objects.
    /// </summary>
    Colon,         // :
    /// <summary>
    /// Represents a comma (,) used to separate elements in JSON arrays and objects.
    /// </summary>
    Comma,         // ,
    /// <summary>
    /// Represents whitespace characters (spaces, tabs, newlines, carriage returns) between tokens.
    /// </summary>
    Whitespace     // sequences of ' ', '	', ' ', ' ' between tokens
}
