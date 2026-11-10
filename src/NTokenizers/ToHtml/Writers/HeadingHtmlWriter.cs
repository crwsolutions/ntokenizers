using System.Text;
using NTokenizers.Markdown.Metadata;

namespace NTokenizers.ToHtml.Writers;

/// <summary>
/// Writer for inline tokens inside Markdown headings.
/// </summary>
internal sealed class HeadingHtmlWriter : AbstractMetadataToHtmlWriter<HeadingMetadata>
{
    private readonly InlineMarkdownTokenWriter _inlineWriter = new();

    internal override void WriteAdditionalCss(StringBuilder css)
    {

    }

    internal override async Task WriteContentAsync(HeadingMetadata metadata, TextWriter writer, CancellationToken ct = default)
    {
        // The heading writes its closing tag without a trailing line break. A heading is a
        // top-level block, so the separating line break is owned by the top-level Markdown
        // writer (_pendingBlockBreak), which writes it before the following block and
        // omits it at end of stream. This keeps heading output consistent with paragraphs,
        // code blocks, and thematic breaks.
        await writer.WriteAsync($"<h{metadata.Level}>");
        await metadata.ParseInlinesAsync(async token => await _inlineWriter.WriteTokenAsync(token, writer, ct));
        await writer.WriteAsync($"</h{metadata.Level}>");
    }
}
