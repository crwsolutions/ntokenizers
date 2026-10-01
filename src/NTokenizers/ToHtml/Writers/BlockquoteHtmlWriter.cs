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

    internal override Task WriteContentAsync(BlockquoteMetadata metadata, TextWriter writer)
    {
        var dispatcher = new MarkdownBlockTokenDispatcher();
        writer.Write("<blockquote>\n");
        return metadata.RegisterInlineTokenHandler(
            token =>
            {
                // The token dispatch is fire-and-forget from this synchronous handler: the
                // returned Task completes when the whole quote content has been processed,
                // which the parser already awaits through the registration's processing Task.
                _ = dispatcher.WriteTokenAsync(token, writer);
            },
            () =>
            {
                // A closed paragraph inside the quote is separated from the closing tag by a
                // line break; at end of content no trailing line break is emitted.
                dispatcher.FlushPendingBlockBreak(writer);
                writer.Write("</blockquote>");
            });
    }
}
