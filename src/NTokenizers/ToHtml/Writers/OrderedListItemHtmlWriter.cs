using System.Text;
using NTokenizers.Markdown;
using NTokenizers.Markdown.Metadata;

namespace NTokenizers.ToHtml.Writers;

/// <summary>
/// Writer for inline tokens inside Markdown ordered list items.
/// </summary>
internal sealed class OrderedListItemHtmlWriter : AbstractMetadataToHtmlWriter<OrderedListItemMetadata>
{
    private MarkdownTokenType _lastTokenType;
    private bool _hasContent;

    private readonly bool _inlineOnly;

    internal OrderedListItemHtmlWriter(bool inlineOnly = false)
    {
        _inlineOnly = inlineOnly;
    }

    internal override void WriteAdditionalCss(StringBuilder css)
    {

    }

    internal override Task WriteContentAsync(OrderedListItemMetadata metadata, TextWriter writer)
    {
        var dispatcher = new MarkdownBlockTokenDispatcher();
        var inlineWriter = new InlineMarkdownTokenWriter();
        writer.Write("<li>");
        return metadata.RegisterInlineTokenHandler(token =>
        {
            if (!_hasContent && token.TokenType is MarkdownTokenType.ParagraphBlockStart or MarkdownTokenType.Blockquote or MarkdownTokenType.ListStart or MarkdownTokenType.CodeBlock or MarkdownTokenType.IndentedCodeBlock or MarkdownTokenType.Heading or MarkdownTokenType.Table or MarkdownTokenType.HorizontalRule)
            {
                writer.Write('\n');
            }
            _hasContent = true;
            _lastTokenType = token.TokenType;
            if (_inlineOnly)
            {
                if (token.TokenType is MarkdownTokenType.ListStart or MarkdownTokenType.ListEnd)
                {
                    return;
                }
                if (token.TokenType is MarkdownTokenType.UnorderedListItem or MarkdownTokenType.OrderedListItem)
                {
                    writer.Write("</li><li>");
                    return;
                }
                inlineWriter.WriteToken(token, writer);
            }
            else
            {
                _ = dispatcher.WriteTokenAsync(token, writer);
            }
        },
        () =>
        {
            dispatcher.FlushPendingBlockBreak(writer);
            if (_lastTokenType == MarkdownTokenType.ListEnd)
            {
                writer.Write('\n');
            }
            writer.Write("</li>\n");
        });
    }
}
