using NTokenizers.Core;
using NTokenizers.Markdown;
using NTokenizers.Markdown.Metadata;
using System.Text;

namespace NTokenizers.Tools.MarkdownToHtml.Writers;

/// <summary>
/// Top-level Markdown document writer. Handles all MarkdownTokenType values and delegates
/// to inline/block writers for headings, blockquotes, lists, tables, and code blocks.
/// </summary>
internal sealed class MarkdownHtmlWriter : BaseHtmlWriter, IAdditionalCssWriter
{
    private readonly InlineMarkdownTokenWriter _inlineWriter = new();

    public void WriteAdditionalCss(StringBuilder bob)
    {
        var codeBlockWriter = new CodeblockHtmlWriter();
        codeBlockWriter.WriteAdditionalCss(bob);
    }

    internal async Task WriteTokenAsync(MarkdownToken token, TextWriter writer)
    {
        switch (token.TokenType)
        {
            case MarkdownTokenType.Text:
                WriteValue(writer, token.Value, null);
                break;

            case MarkdownTokenType.Bold:
            case MarkdownTokenType.Italic:
            case MarkdownTokenType.Emphasis:
            case MarkdownTokenType.CodeInline:
            case MarkdownTokenType.Link:
            case MarkdownTokenType.Image:
            case MarkdownTokenType.Emoji:
            case MarkdownTokenType.Subscript:
            case MarkdownTokenType.Superscript:
            case MarkdownTokenType.InsertedText:
            case MarkdownTokenType.MarkedText:
            case MarkdownTokenType.FootnoteReference:
            case MarkdownTokenType.TypographicReplacement:
                _inlineWriter.WriteToken(token, writer);
                break;

            case MarkdownTokenType.Heading:
                if (token.Metadata is HeadingMetadata headingMeta)
                {
                    var headingWriter = new HeadingHtmlWriter();
                    await headingWriter.WriteContentAsync(headingMeta, writer);
                }
                break;

            case MarkdownTokenType.Blockquote:
                if (token.Metadata is BlockquoteMetadata bqMeta)
                {
                    var bqWriter = new BlockquoteHtmlWriter();
                    await bqWriter.WriteContentAsync(bqMeta, writer);
                }
                break;

            case MarkdownTokenType.UnorderedListItem:
                if (token.Metadata is ListItemMetadata liMeta)
                {
                    var liWriter = new ListItemHtmlWriter();
                    await liWriter.WriteContentAsync(liMeta, writer);
                }
                break;

            case MarkdownTokenType.OrderedListItem:
                if (token.Metadata is OrderedListItemMetadata oliMeta)
                {
                    var oliWriter = new OrderedListItemHtmlWriter();
                    await oliWriter.WriteContentAsync(oliMeta, writer);
                }
                break;

            case MarkdownTokenType.HorizontalRule:
                writer.Write("<hr />\n");
                break;

            case MarkdownTokenType.CodeBlock:
                if (token.Metadata is ICodeBlockMetadata codeBlockMetadata)
                {
                    var codeBlockWriter = new CodeblockHtmlWriter();
                    await codeBlockWriter.WriteCodeBlockAsync(codeBlockMetadata, writer);
                }
                break;

            case MarkdownTokenType.Table:
                // The Table token is emitted by the MarkdownTokenizer. The TableMarkdownTokenizer
                // then sends TableRow/TableCell/TableAlignments tokens through the inline handler.
                if (token.Metadata is TableMetadata tableMeta)
                {
                    var tableWriter = new TableHtmlWriter(this);
                    await tableWriter.WriteContentAsync(tableMeta, writer);
                }
                break;

            // TableRow, TableCell, TableAlignments are handled by TableHtmlWriter via inline token handler
            // Do NOT create a new writer for these — they are part of the Table's inline token stream
            case MarkdownTokenType.TableRow:
            case MarkdownTokenType.TableCell:
            case MarkdownTokenType.TableAlignments:
                // These tokens are consumed by the TableHtmlWriter's inline handler above.
                // If we reach here, the table handler was not registered (shouldn't happen).
                break;

            case MarkdownTokenType.FootnoteDefinition:
                if (token.Metadata is FootnoteMetadata fnDefMeta)
                {
                    writer.Write($"<sup id=\"fn-{fnDefMeta.Id}\">");
                    WriteValue(writer, token.Value, null);
                    writer.Write("</sup>\n");
                }
                break;

            case MarkdownTokenType.DefinitionTerm:
                writer.Write("<dt>");
                WriteValue(writer, token.Value, null);
                writer.Write("</dt>\n");
                break;

            case MarkdownTokenType.DefinitionDescription:
                writer.Write("<dd>");
                WriteValue(writer, token.Value, null);
                writer.Write("</dd>\n");
                break;

            case MarkdownTokenType.Abbreviation:
                WriteValue(writer, token.Value, null);
                break;

            case MarkdownTokenType.CustomContainer:
                writer.Write($"<div class=\"tok-container tok-container-{EscapeHtml(token.Value)}\">\n");
                // Content follows as subsequent tokens
                break;

            case MarkdownTokenType.HtmlTag:
                // Pass through raw HTML tags
                writer.Write(token.Value);
                break;

            default:
                WriteValue(writer, token.Value, null);
                break;
        }
    }
}
