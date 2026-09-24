using System.Text;
using NTokenizers.Markdown.Metadata;

namespace NTokenizers.ToHtml.Writers;

/// <summary>
/// Writer for inline tokens inside Markdown indented code blocks. The content is literal:
/// each Text token is written escaped with preserved line breaks (no br conversion, no span).
/// </summary>
internal sealed class IndentedCodeBlockHtmlWriter : BaseHtmlWriter, IAdditionalCssWriter
{
    /// <inheritdoc cref="IAdditionalCssWriter.WriteAdditionalCss"/>
    void IAdditionalCssWriter.WriteAdditionalCss(StringBuilder css)
    {
    }

    internal async Task WriteContentAsync(IndentedCodeBlockMetadata metadata, TextWriter writer)
    {
        writer.Write("<pre><code>");
        await metadata.RegisterInlineTokenHandler(
            token => WriteValue(writer, token.Value, null, inPreBlock: true),
            () =>
            {
                writer.Write("</code></pre>");
            });
    }
}
