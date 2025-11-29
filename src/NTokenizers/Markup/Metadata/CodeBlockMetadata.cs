namespace NTokenizers.Markup.Metadata;

/// <summary>
/// Metadata for code block tokens, containing the language identifier.
/// </summary>
/// <typeparam name="TToken">The type of tokens emitted by the language-specific tokenizer.</typeparam>
/// <param name="Language">The language identifier (e.g., "xml", "json", "csharp").</param>
public abstract class CodeBlockMetadata<TToken>(string Language) : InlineMarkupMetadata<TToken>, ICodeBlockMetadata
{
    /// <summary>
    /// Gets the language code representing the current language setting.
    /// </summary>
    public string Language { get; } = Language;
}
