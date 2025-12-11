using NTokenizers.Core;
using NTokenizers.Generic;
using NTokenizers.Markdown;

namespace NTokenizers.Markdown.Metadata;

/// <summary>
/// Represents metadata for a generic code block, including its associated code content.
/// </summary>
/// <remarks>This class is a specialized implementation of <see cref="CodeBlockMetadata{MarkdownToken}"/> designed
/// to handle generic code blocks. It provides functionality to manage and process the code content associated with the
/// block.</remarks>
/// <param name="language">The code content associated with the generic code block.</param>
public sealed class GenericCodeBlockMetadata(string language) : CodeBlockMetadata<MarkdownToken>(language), ICodeBlockMetadata
{
    internal override BaseSubTokenizer<MarkdownToken> CreateTokenizer() => GenericTokenizer.Create();
}
