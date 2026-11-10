using System.Text;
using NTokenizers.Markdown;
using NTokenizers.Markdown.Metadata;

namespace NTokenizers.ToHtml.Writers;

/// <summary>
/// Writer for Markdown table tokens. Delegates all inline cell content
/// to an InlineMarkdownTokenWriter for consistent rendering.
/// 
/// Only handles structural tokens: TableRow, TableCell, TableAlignments.
/// All other tokens are passed through to InlineMarkdownTokenWriter.WriteTokenAsync().
/// 
/// Tokens from the first (header) row are queued until TableAlignments or the second
/// TableRow arrives, at which point the queue is flushed and processed with alignment info.
/// </summary>
internal sealed class TableHtmlWriter : IAdditionalCssWriter
{
    private readonly InlineMarkdownTokenWriter _inlineWriter = new();
    private readonly TableState _state = new();

    void IAdditionalCssWriter.WriteAdditionalCss(StringBuilder css) => WriteAdditionalCss(css);

    internal void WriteAdditionalCss(StringBuilder css)
    {
        // Table CSS is in BaseHtmlWriter.WriteCommonCss()
    }

    internal async Task WriteContentAsync(TableMetadata metadata, TextWriter writer, CancellationToken ct = default)
    {
        await writer.WriteAsync("<table>\n");
        await metadata.ParseInlinesAsync(async token => await HandleTableTokenAsync(token, metadata, writer, ct));
        await Close(metadata, writer, ct);
        await writer.WriteLineAsync("</table>");
    }

    private async Task HandleTableTokenAsync(MarkdownToken token, TableMetadata metadata, TextWriter writer, CancellationToken ct = default)
    {
        // Queue tokens until TableAlignments or second TableRow arrives
        if (_state.Queueing)
        {
            // Check if this token is the trigger to flush
            if (token.TokenType == MarkdownTokenType.TableAlignments || (token.TokenType == MarkdownTokenType.TableRow && _state.IsRowOpen))
            {
                await FlushQueueAsync(metadata, writer);
            }
            else
            {
                _state.Queue.Enqueue(token);
                return;
            }
        }

        // Normal processing
        switch (token.TokenType)
        {
            case MarkdownTokenType.TableRow:
                // Close previous row if open
                if (_state.IsRowOpen)
                {
                    // Close the last open cell of the previous row before closing the row,
                    // otherwise the cell is left unclosed and the next row starts with a stray </td>.
                    if (_state.IsCellOpen)
                    {
                        await CloseBodyCell(writer);
                    }

                    await writer.WriteAsync("</tr>\n");
                    _state.IsRowOpen = false;

                    // Close thead after header row
                    if (_state.IsInThead)
                    {
                        await writer.WriteAsync("</thead>\n");
                        _state.IsInThead = false;
                        _state.IsHeaderRow = false;
                    }
                }

                await ProcessRow(writer);
                break;

            case MarkdownTokenType.TableCell:
                // Close previous cell if open
                if (_state.IsCellOpen)
                {
                    await CloseBodyCell(writer);
                }

                // Open new body cell with alignment
                var alignStyle = string.Empty;
                if (_state.Alignments != null && _state.ColumnIndex < _state.Alignments.Count)
                {
                    alignStyle = $" style=\"text-align:{_state.Alignments[_state.ColumnIndex].ToString().ToLowerInvariant()}\"";
                }
                await writer.WriteAsync($"<td{alignStyle}>");
                _state.ColumnIndex++;
                _state.IsCellOpen = true;
                break;

            default:
                // Inline content (Text, Bold, Link, CodeInline, etc.).
                // Rendered via the inline writer (not the top-level MarkdownHtmlWriter) so that
                // table cell text is not affected by the top-level paragraph/whitespace handling.
                await _inlineWriter.WriteTokenAsync(token, writer, ct);
                break;
        }
    }

    private async Task FlushQueueAsync(TableMetadata metadata, TextWriter writer, CancellationToken ct = default)
    {
        _state.Queueing = false;
        _state.Alignments = metadata.Alignments;
        while (_state.Queue.Count > 0)
        {
            var token = _state.Queue.Dequeue();
            await HandleTableTokenAsync(token, metadata, writer, ct);
        }
    }

    private async Task ProcessRow(TextWriter writer)
    {
        if (_state.IsHeaderRow)
        {
            // Open thead for header row
            await writer.WriteAsync("<thead>\n");
            _state.IsInThead = true;
        }
        else
        {
            // Open tbody on first body row
            if (!_state.IsInTbody)
            {
                await writer.WriteAsync("<tbody>\n");
                _state.IsInTbody = true;
            }
        }

        await writer.WriteAsync("<tr>");
        _state.IsRowOpen = true;
        _state.ColumnIndex = 0;
    }

    private async Task CloseBodyCell(TextWriter writer)
    {
        await writer.WriteAsync("</td>");
        _state.IsCellOpen = false;
    }

    private async Task Close(TableMetadata metadata, TextWriter writer, CancellationToken ct = default)
    {
        // Flush queue if still queueing (table with only header row, no alignments, no second row)
        if (_state.Queueing)
        {
            await FlushQueueAsync(metadata, writer, ct);
        }

        // Close last header cell if open
        if (_state.IsHeaderRow && _state.IsCellOpen)
        {
            await writer.WriteAsync("</th>");
            _state.IsCellOpen = false;
        }

        // Close last header row if open
        if (_state.IsRowOpen && _state.IsInThead)
        {
            await writer.WriteAsync("</tr>\n</thead>\n");
            _state.IsRowOpen = false;
        }

        // Close body cell if open
        if (_state.IsCellOpen)
        {
            await CloseBodyCell(writer);
        }

        // Close body row if open
        if (_state.IsRowOpen)
        {
            await writer.WriteAsync("</tr>\n");
            _state.IsRowOpen = false;
        }

        // Close tbody if open
        if (_state.IsInTbody)
        {
            await writer.WriteAsync("</tbody>\n");
        }
    }

    private sealed class TableState
    {
        internal bool IsRowOpen;
        internal bool IsCellOpen;
        internal bool IsHeaderRow = true;
        internal bool IsInThead;
        internal bool IsInTbody;
        internal bool Queueing = true;
        internal int ColumnIndex;

        internal Queue<MarkdownToken> Queue = new();
        internal List<Justify>? Alignments;
    }
}