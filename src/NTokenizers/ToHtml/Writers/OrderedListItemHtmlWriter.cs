using System.Text;
using NTokenizers.Markdown.Metadata;

namespace NTokenizers.ToHtml.Writers;

/// <summary>
/// Writer for inline tokens inside Markdown ordered list items.
/// </summary>
internal sealed class OrderedListItemHtmlWriter : AbstractMetadataToHtmlWriter<OrderedListItemMetadata>
{
    private readonly InlineMarkdownTokenWriter _inlineWriter = new();

    internal override void WriteAdditionalCss(StringBuilder css)
    {

    }

    internal override Task WriteContentAsync(OrderedListItemMetadata metadata, TextWriter writer)
    {
        writer.Write("<li>");
        return metadata.RegisterInlineTokenHandler(token =>
        {
            _inlineWriter.WriteToken(token, writer);
        },
        () =>
        {
            writer.Write("</li>\n");
        });
    }
}
