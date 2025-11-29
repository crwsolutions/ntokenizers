namespace NTokenizers.Markup.Metadata;

/// <summary>
/// Metadata for TypeScript code block tokens with syntax highlighting support.
/// </summary>
public sealed class TypeScriptCodeBlockMetadata(string language) : CodeBlockMetadata<Typescript.TypescriptToken>(language);
