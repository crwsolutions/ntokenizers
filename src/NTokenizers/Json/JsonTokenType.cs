namespace NTokenizers.Json;

public enum JsonTokenType
{
    StartObject,   // {
    EndObject,     // }
    StartArray,    // [
    EndArray,      // ]
    PropertyName,  // string used as object key
    StringValue,   // string value
    Number,        // integer, float, scientific notation
    True,
    False,
    Null,          // null
    Colon,         // :
    Comma,         // ,
    Whitespace     // sequences of ' ', '\t', '\n', '\r' between tokens
}
