using System.Text;
using NTokenizers.Core;

namespace NTokenizers.Markdown.Metadata;

/// <summary>
/// Metadata for blockquote tokens. The blockquote marker is decoration only and carries no
/// state: the token's Value is empty and the quoted content is streamed as a full markdown
/// token stream (paragraphs, block constructs, and nested blockquotes) by a nested
/// MarkdownTokenizer at one deeper level, driven through <see cref="InlineMetadata{MarkdownToken}.ParseInlinesAsync"/>.
/// </summary>
/// <param name="Depth">The blockquote nesting depth of the content tokenizer (the parent's depth plus one).</param>
/// <param name="reader">The section reader the sub-tokenizer reads from.</param>
/// <param name="capture">The capture builder that records every character of the stream.</param>
/// <param name="lookahead">The lookahead queue the sub-tokenizer reads from.</param>
public sealed class BlockquoteMetadata(int Depth, TextReader reader, StringBuilder capture, Queue<char> lookahead)
    : InlineMetadata<MarkdownToken>(reader, capture, lookahead)
{
    internal override Task RunInlinesAsync(Func<MarkdownToken, Task> onToken, CancellationToken ct) =>
        new MarkdownTokenizer(Depth, dropClosingNewline: true).ParseAsync(Reader, Capture, Lookahead, ct, onToken);
}
