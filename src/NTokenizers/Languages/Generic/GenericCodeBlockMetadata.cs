using System.Text;
using NTokenizers.Core;
using NTokenizers.Markdown;

namespace NTokenizers.Generic;

/// <summary>
/// Represents metadata for a generic code block, including its associated code content.
/// </summary>
/// <remarks>This class is a specialized implementation of <see cref="InlineMetadata{MarkdownToken}"/> designed
/// to handle generic code blocks. It provides functionality to manage and process the code content associated with the
/// block.</remarks>
public sealed class GenericCodeBlockMetadata(string language, TextReader reader, StringBuilder capture, Queue<char> lookahead, string stopDelimiter) : CodeBlockMetadata<MarkdownToken>(language, reader, capture, lookahead, stopDelimiter)
{
    internal override BaseSubTokenizer<MarkdownToken> CreateTokenizer() => GenericTokenizer.Create();
}
