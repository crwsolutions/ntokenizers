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

    internal override Task WriteContentAsync(HeadingMetadata metadata, TextWriter writer)
    {
        // The heading writes its closing tag without a trailing line break. A heading is a
        // top-level block, so the separating line break is owned by the top-level Markdown
        // writer (_pendingBlockBreak), which writes it before the following block and
        // omits it at end of stream. This keeps heading output consistent with paragraphs,
        // code blocks, and thematic breaks.
        writer.Write($"<h{metadata.Level}>");
        return metadata.RegisterInlineTokenHandler(token =>
        {
            _inlineWriter.WriteToken(token, writer);
        },
        () =>
        {
            writer.Write($"</h{metadata.Level}>");
        });
    }
}
