namespace NTokenizers.CSharp;

/// <summary>
/// Represents a C# token with its type and value.
/// </summary>
/// <param name="TokenType">The type of the C# token.</param>
/// <param name="Value">The string value of the C# token.</param>
public record CSharpToken(CSharpTokenType TokenType, string Value);
