using NTokenizers.Core;
using System.Diagnostics;

namespace NTokenizers.Yaml;

/// <summary>
/// Represents a YAML token with its type and value.
/// </summary>
[DebuggerDisplay("YamlToken: {TokenType} '{Value}'")]
public class YamlToken : IToken<YamlTokenType>
{
    /// <summary>
    /// Gets the type of the current YAML token.
    /// </summary>
    public YamlTokenType TokenType { get; }

    /// <summary>
    /// Gets the current value as a string.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="YamlToken"/> class with the specified token type and value.
    /// </summary>
    /// <param name="tokenType">The type of the YAML token.</param>
    /// <param name="value">The string representation of the YAML token's value.</param>
    public YamlToken(YamlTokenType tokenType, string value)
    {
        TokenType = tokenType;
        Value = value;
    }

    /// <summary>
    /// Returns a string representation of the token.
    /// </summary>
    /// <returns>A string in the format "Type: 'Value'"</returns>
    public override string ToString()
    {
        return $"{TokenType}: '{Value}'";
    }
}
