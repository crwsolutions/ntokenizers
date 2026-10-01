using NTokenizers.Core;

namespace NTokenizers.Markdown.Metadata;

/// <summary>
/// Metadata for blockquote tokens. The blockquote marker is decoration only and carries no
/// state: the token's Value is empty and the quoted content is streamed as a full markdown
/// token stream (paragraphs, block constructs, and nested blockquotes) through the inline
/// token handler registered on this metadata.
/// </summary>
public sealed class BlockquoteMetadata() : InlineMetadata<MarkdownToken>
{
}
