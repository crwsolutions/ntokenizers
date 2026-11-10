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
    private readonly int? _value;

    internal OrderedListItemHtmlWriter(bool inlineOnly = false, int? value = null)
    {
        _inlineOnly = inlineOnly;
        _value = value;
    }

    internal override void WriteAdditionalCss(StringBuilder css)
    {

    }

    internal override async Task WriteContentAsync(OrderedListItemMetadata metadata, TextWriter writer, CancellationToken ct = default)
    {
        var dispatcher = new MarkdownBlockTokenDispatcher();
        var inlineWriter = new InlineMarkdownTokenWriter();
        // value="N" is written only where the source number deviates from the logical
        // sequence (the dispatcher's counter simulation); the browser continues from it.
        await writer.WriteAsync(_value.HasValue ? $"<li value=\"{_value.Value}\">" : "<li>");

        // Item content tokens arrive in stream order; each dispatch is awaited before the
        // next token arrives.
        await metadata.ParseInlinesAsync(async token =>
        {
            if (!_hasContent && token.TokenType is MarkdownTokenType.ParagraphBlockStart or MarkdownTokenType.Blockquote or MarkdownTokenType.ListStart or MarkdownTokenType.CodeBlock or MarkdownTokenType.IndentedCodeBlock or MarkdownTokenType.Heading or MarkdownTokenType.Table or MarkdownTokenType.HorizontalRule)
            {
                await writer.WriteAsync("\n");
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
                    await writer.WriteAsync("</li><li>");
                    return;
                }
                await inlineWriter.WriteTokenAsync(token, writer, ct);
            }
            else
            {
                await dispatcher.WriteTokenAsync(token, writer, ct);
            }
        });

        await dispatcher.FlushPendingBlockBreak(writer, ct);
        if (_lastTokenType == MarkdownTokenType.ListEnd)
        {
            await writer.WriteAsync("\n");
        }
        await writer.WriteAsync("</li>\n");
    }
}
