namespace NTokenizers.Core;

/// <summary>
/// Represents metadata for a code block within markdown content.
/// </summary>
public interface ICodeBlockMetadata
{
    /// <summary>
    /// Gets the language of the code block, e.g., "csharp", "xml", or "javascript".
    /// This corresponds to the language identifier in a code fence.
    /// </summary>
    string Language { get; }
}
