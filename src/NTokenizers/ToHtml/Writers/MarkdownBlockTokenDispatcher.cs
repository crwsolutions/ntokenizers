using System.Text;
using NTokenizers.Core;
using NTokenizers.Markdown;
using NTokenizers.Markdown.Metadata;

namespace NTokenizers.ToHtml.Writers;

/// <summary>
/// Shared markdown block/inline token dispatch used by both the top-level Markdown document
/// writer and the blockquote content writer, so that blockquote content renders exactly like
/// top-level content. Owns the paragraph region state (<see cref="_inParagraph"/>), the
/// pending block separation line break (<see cref="_pendingBlockBreak"/>), and routes inline
/// tokens to the <see cref="InlineMarkdownTokenWriter"/>.
/// </summary>
/// <remarks>
/// Whitespace principle: text tokens are always written in full (escaped; in-paragraph line
/// breaks become <c>&lt;br/&gt;</c>). The writer only ever ADDS line breaks (the block
/// separation written before the next token, and the ones emitted by the individual
/// block writers); it never removes whitespace from the token stream. A pending separation
/// that is never followed by another token (end of stream) is dropped.
/// </remarks>
internal sealed class MarkdownBlockTokenDispatcher : BaseHtmlWriter
{
    private readonly InlineMarkdownTokenWriter _inlineWriter = new();
    private readonly bool _inlineListItems;

    internal MarkdownBlockTokenDispatcher(bool inlineListItems = false)
    {
        _inlineListItems = inlineListItems;
    }

    // Whether the writer is currently inside an open <p>...</p> region, so newline text
    // is rendered as a soft line break (<br/>) rather than dropped.
    private bool _inParagraph;

    // A paragraph ends with </p>. When another token follows, it is separated from the
    // paragraph by a line break; at end of stream no trailing line break is emitted.
    private bool _pendingBlockBreak;

    // Ordered-list counter simulation: mirrors the browser's ol counter so a <li value="N">
    // is emitted only where the source number deviates from the logical sequence.
    private int _orderedCounter;
    private bool _inOrderedList;

    /// <summary>
    /// Writes the pending block separation line break, if any. Called by container writers
    /// (e.g. blockquote) before writing their closing tag, so the separation is written
    /// inside the container and the container's outer scope can add its own separation
    /// afterwards.
    /// </summary>
    internal void FlushPendingBlockBreak(TextWriter writer)
    {
        if (_pendingBlockBreak)
        {
            _pendingBlockBreak = false;
            writer.Write('\n');
        }
    }

    /// <summary>
    /// Writes the given markdown token to the writer, dispatching block tokens to the
    /// dedicated writers and inline tokens to the inline writer.
    /// </summary>
    internal async Task WriteTokenAsync(MarkdownToken token, TextWriter writer)
    {
        // Write the block break that follows a closed paragraph, before the next token.
        // It is intentionally omitted when the paragraph is the last token (end of stream).
        if (_pendingBlockBreak)
        {
            _pendingBlockBreak = false;
            writer.Write('\n');
        }

        switch (token.TokenType)
        {
            case MarkdownTokenType.Text:
                if (!_inParagraph)
                {
                    // Newline/whitespace text outside a paragraph is block separation, not content.
                    break;
                }
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
                    // The heading is a top-level block: a following block is separated from
                    // it by a line break, written before that block and omitted at end of
                    // stream (consistent with paragraphs, code blocks, and thematic breaks).
                    _pendingBlockBreak = true;
                }
                break;

            case MarkdownTokenType.Blockquote:
                if (token.Metadata is BlockquoteMetadata bqMeta)
                {
                    var bqWriter = new BlockquoteHtmlWriter();
                    await bqWriter.WriteContentAsync(bqMeta, writer);
                    // The blockquote is a block: a following block is separated from it by a
                    // line break, written before that block and omitted at end of stream.
                    _pendingBlockBreak = true;
                }
                break;

            case MarkdownTokenType.ListStart:
                // A list is a block: the opening tag is separated from the previous block
                // by a line break (the one already written above from _pendingBlockBreak).
                // Items follow until the matching ListEnd.
                if (token.Metadata is ListMetadata lsMeta)
                {
                    writer.Write(lsMeta.IsOrdered ? "<ol>\n" : "<ul>\n");
                    _inOrderedList = lsMeta.IsOrdered;
                    if (_inOrderedList)
                    {
                        _orderedCounter = 1; // browser default start
                    }
                }
                break;

            case MarkdownTokenType.ListEnd:
                // Close the list. No pending block break is set: consistent with the
                // baseline, a list's closing does not create a block separation (the
                // inter-item whitespace is dropped by the Text case at this level), so a
                // following block starts immediately after the closing tag.
                if (token.Metadata is ListMetadata leMeta)
                {
                    writer.Write(leMeta.IsOrdered ? "</ol>" : "</ul>");
                    if (leMeta.IsOrdered)
                    {
                        _inOrderedList = false;
                    }
                }
                break;

            case MarkdownTokenType.UnorderedListItem:
                if (token.Metadata is ListItemMetadata liMeta)
                {
                    var liWriter = new ListItemHtmlWriter(_inlineListItems || _inParagraph);
                    await liWriter.WriteContentAsync(liMeta, writer);
                }
                break;

            case MarkdownTokenType.OrderedListItem:
                if (token.Metadata is OrderedListItemMetadata oliMeta)
                {
                    // Emit value="N" only where the number deviates from the simulated
                    // browser counter; otherwise the counter simply advances.
                    int? value = _inOrderedList && oliMeta.Number != _orderedCounter ? oliMeta.Number : null;
                    _orderedCounter = value.HasValue ? oliMeta.Number + 1 : _orderedCounter + 1;
                    var oliWriter = new OrderedListItemHtmlWriter(_inlineListItems || _inParagraph, value);
                    await oliWriter.WriteContentAsync(oliMeta, writer);
                }
                break;

            case MarkdownTokenType.HorizontalRule:
                // The line break after <hr /> is written before the next block token and
                // omitted at end of stream (consistent with paragraphs and code blocks).
                writer.Write("<hr />");
                _pendingBlockBreak = true;
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
                    var tableWriter = new TableHtmlWriter();
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

            case MarkdownTokenType.ParagraphBlockStart:
                writer.Write("<p>");
                _inParagraph = true;
                break;

            case MarkdownTokenType.ParagraphBlockEnd:
                writer.Write("</p>");
                _inParagraph = false;
                // A following block is separated from the paragraph by a line break; the
                // break is written before that block (and omitted at end of stream).
                _pendingBlockBreak = true;
                break;

            case MarkdownTokenType.IndentedCodeBlock:
                if (token.Metadata is IndentedCodeBlockMetadata icoMeta)
                {
                    var icoWriter = new IndentedCodeBlockHtmlWriter();
                    await icoWriter.WriteContentAsync(icoMeta, writer);
                    // A following block is separated from the code block by a line break; the
                    // break is written before that block (and omitted at end of stream).
                    _pendingBlockBreak = true;
                }
                break;

            default:
                WriteValue(writer, token.Value, null);
                break;
        }
    }
}
