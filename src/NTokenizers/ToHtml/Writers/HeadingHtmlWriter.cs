using NTokenizers.Markdown.Metadata;
using System.Text;

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
        writer.Write($"<h{metadata.Level}>");
        return metadata.RegisterInlineTokenHandler(token =>
        {
            _inlineWriter.WriteToken(token, writer);
        },
        () =>
        {
            writer.WriteLine($"</h{metadata.Level}>");
        });
    }
}
