using NTokenizers.Core;
using NTokenizers.Markdown;

namespace NTokenizers.Generic;

/// <summary>
/// Represents metadata for a generic code block, including its associated code content.
/// </summary>
/// <remarks>This class is a specialized implementation of <see cref="InlineMetadata{MarkdownToken}"/> designed
/// to handle generic code blocks. It provides functionality to manage and process the code content associated with the
/// block.</remarks>
public sealed class GenericCodeBlockMetadata(string language) : CodeBlockMetadata<MarkdownToken>(language)
{
    internal override BaseSubTokenizer<MarkdownToken> CreateTokenizer() => GenericTokenizer.Create();
}
