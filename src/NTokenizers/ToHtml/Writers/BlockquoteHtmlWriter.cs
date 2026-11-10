using System.Text;
using NTokenizers.Markdown.Metadata;

namespace NTokenizers.ToHtml.Writers;

/// <summary>
/// Writer for blockquote tokens. Writes the <c>&lt;blockquote&gt;</c> container and dispatches
/// the quoted content through the shared <see cref="MarkdownBlockTokenDispatcher"/>, so the
/// content (a full markdown sub-document: paragraphs, block constructs, and nested
/// blockquotes) renders exactly like top-level content.
/// </summary>
internal sealed class BlockquoteHtmlWriter : AbstractMetadataToHtmlWriter<BlockquoteMetadata>
{
    internal override void WriteAdditionalCss(StringBuilder css)
    {
    }

    internal override async Task WriteContentAsync(BlockquoteMetadata metadata, TextWriter writer, CancellationToken ct = default)
    {
        var dispatcher = new MarkdownBlockTokenDispatcher(inlineListItems: true);
        await writer.WriteAsync("<blockquote>\n");

        // Content tokens arrive in stream order as the quote is tokenized; each dispatch
        // is awaited before the next token arrives.
        await metadata.ParseInlinesAsync(async token =>
        {
            await dispatcher.WriteTokenAsync(token, writer, ct);
        });

        // A closed paragraph inside the quote is separated from the closing tag by a
        // line break; at end of content no trailing line break is emitted.
        await dispatcher.FlushPendingBlockBreak(writer, ct);
        await writer.WriteAsync("</blockquote>");
    }
}
