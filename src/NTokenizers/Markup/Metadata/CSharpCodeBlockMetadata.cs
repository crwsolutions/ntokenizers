namespace NTokenizers.Markup.Metadata;

/// <summary>
/// Metadata for C# code block tokens with syntax highlighting support.
/// </summary>
public sealed class CSharpCodeBlockMetadata(string language) : CodeBlockMetadata<CSharp.CSharpToken>(language);
