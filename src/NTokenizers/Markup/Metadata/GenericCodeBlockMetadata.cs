using NTokenizers.Core;
using NTokenizers.Generic;

namespace NTokenizers.Markup.Metadata;

/// <summary>
/// Represents metadata for a generic code block, including its associated code content.
/// </summary>
/// <remarks>This class is a specialized implementation of <see cref="CodeBlockMetadata{MarkupToken}"/> designed
/// to handle generic code blocks. It provides functionality to manage and process the code content associated with the
/// block.</remarks>
/// <param name="language">The code content associated with the generic code block.</param>
public sealed class GenericCodeBlockMetadata(string language) : CodeBlockMetadata<MarkupToken>(language), ICodeBlockMetadata
{
    internal override BaseSubTokenizer<MarkupToken> CreateTokenizer() => GenericTokenizer.Create();
}
