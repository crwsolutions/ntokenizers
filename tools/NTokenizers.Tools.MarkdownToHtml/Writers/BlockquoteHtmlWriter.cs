using NTokenizers.Markdown.Metadata;
using System.Text;

namespace NTokenizers.Tools.MarkdownToHtml.Writers;

/// <summary>
/// Writer for inline tokens inside Markdown blockquotes.
/// </summary>
internal sealed class BlockquoteHtmlWriter : AbstractMetadataToHtmlWriter<BlockquoteMetadata>
{
    private readonly InlineMarkdownTokenWriter _inlineWriter = new();

    internal override void WriteAdditionalCss(StringBuilder css)
    {
        
    }

    internal override Task WriteContentAsync(BlockquoteMetadata metadata, TextWriter writer)
    {
        writer.Write("<blockquote>\n");
        return metadata.RegisterInlineTokenHandler(token =>
        {
            _inlineWriter.WriteToken(token, writer);
        },
        () =>
        {
            writer.Write("</blockquote>");
        });
    }
}
