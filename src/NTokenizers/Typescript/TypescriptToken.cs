namespace NTokenizers.Typescript;

/// <summary>
/// Represents a TypeScript token with its type and value.
/// </summary>
/// <param name="TokenType">The type of the TypeScript token.</param>
/// <param name="Value">The string value of the TypeScript token.</param>
public record TypescriptToken(TypescriptTokenType TokenType, string Value);
