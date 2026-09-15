using NTokenizers.Core;
using System.Diagnostics;

namespace NTokenizers.Toml;

/// <summary>
/// Represents a TOML token with its type and value.
/// </summary>
[DebuggerDisplay("TomlToken: {TokenType} '{Value}'")]
public class TomlToken : IToken<TomlTokenType>
{
    /// <summary>
    /// Gets the type of the current TOML token.
    /// </summary>
    public TomlTokenType TokenType { get; }

    /// <summary>
    /// Gets the current value as a string.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TomlToken"/> class with the specified token type and value.
    /// </summary>
    /// <param name="tokenType">The type of the TOML token.</param>
    /// <param name="value">The string representation of the TOML token's value.</param>
    public TomlToken(TomlTokenType tokenType, string value)
    {
        TokenType = tokenType;
        Value = value;
    }
}
