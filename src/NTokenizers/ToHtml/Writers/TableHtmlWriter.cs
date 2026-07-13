using NTokenizers.Markdown;
using NTokenizers.Markdown.Metadata;
using System.Text;

namespace NTokenizers.ToHtml.Writers;

/// <summary>
/// Writer for Markdown table tokens. Delegates all inline content (including TableCell tokens)
/// to the MarkdownHtmlWriter for consistent rendering.
/// 
/// Only handles structural tokens: TableRow, TableCell, TableAlignments.
/// All other tokens are passed through to MarkdownHtmlWriter.WriteTokenAsync().
/// 
/// Tokens from the first (header) row are queued until TableAlignments or the second
/// TableRow arrives, at which point the queue is flushed and processed with alignment info.
/// </summary>
internal sealed class TableHtmlWriter : IAdditionalCssWriter
{
    private readonly MarkdownHtmlWriter _markdownWriter;
    private readonly TableState _state = new();

    internal TableHtmlWriter(MarkdownHtmlWriter markdownWriter)
    {
        _markdownWriter = markdownWriter;
    }

    void IAdditionalCssWriter.WriteAdditionalCss(StringBuilder css) => WriteAdditionalCss(css);

    internal void WriteAdditionalCss(StringBuilder css)
    {
        // Table CSS is in BaseHtmlWriter.WriteCommonCss()
    }

    internal async Task WriteContentAsync(TableMetadata metadata, TextWriter writer)
    {
        writer.Write("<table>\n");
        await metadata.RegisterInlineTokenHandler(async token =>
        {
            await HandleTableTokenAsync(token, metadata, writer);
        },
        () =>
        {
            Close(metadata, writer);
            writer.WriteLine("</table>");
        });
    }

    private async Task HandleTableTokenAsync(MarkdownToken token, TableMetadata metadata, TextWriter writer)
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
                    writer.Write("</tr>\n");
                    _state.IsRowOpen = false;

                    // Close thead after header row
                    if (_state.IsInThead)
                    {
                        writer.Write("</thead>\n");
                        _state.IsInThead = false;
                        _state.IsHeaderRow = false;
                    }
                }

                ProcessRow(writer);
                break;

            case MarkdownTokenType.TableCell:
                // Close previous cell if open
                if (_state.IsCellOpen)
                {
                    CloseBodyCell(writer);
                }

                // Open new body cell with alignment
                var alignStyle = string.Empty;
                if (_state.Alignments != null && _state.ColumnIndex < _state.Alignments.Count)
                {
                    alignStyle = $" style=\"text-align:{_state.Alignments[_state.ColumnIndex].ToString().ToLowerInvariant()}\"";
                }
                writer.Write($"<td{alignStyle}>");
                _state.ColumnIndex++;
                _state.IsCellOpen = true;
                break;

            default:
                // Inline content (Text, Bold, Link, CodeInline, etc.)
                await _markdownWriter.WriteTokenAsync(token, writer);
                break;
        }
    }

    private async Task FlushQueueAsync(TableMetadata metadata, TextWriter writer)
    {
        _state.Queueing = false;
        _state.Alignments = metadata.Alignments;
        while (_state.Queue.Count > 0)
        {
            var token = _state.Queue.Dequeue();
            await HandleTableTokenAsync(token, metadata, writer);
        }
    }

    private void ProcessRow(TextWriter writer)
    {
        if (_state.IsHeaderRow)
        {
            // Open thead for header row
            writer.Write("<thead>\n");
            _state.IsInThead = true;
        }
        else
        {
            // Open tbody on first body row
            if (!_state.IsInTbody)
            {
                writer.Write("<tbody>\n");
                _state.IsInTbody = true;
            }
        }

        writer.Write("<tr>");
        _state.IsRowOpen = true;
        _state.ColumnIndex = 0;
    }

    private void CloseBodyCell(TextWriter writer)
    {
        writer.Write("</td>");
        _state.IsCellOpen = false;
    }

    private void Close(TableMetadata metadata, TextWriter writer)
    {
        // Flush queue if still queueing (table with only header row, no alignments, no second row)
        if (_state.Queueing)
        {
            FlushQueueAsync(metadata, writer).GetAwaiter().GetResult();
        }

        // Close last header cell if open
        if (_state.IsHeaderRow && _state.IsCellOpen)
        {
            writer.Write("</th>");
            _state.IsCellOpen = false;
        }

        // Close last header row if open
        if (_state.IsRowOpen && _state.IsInThead)
        {
            writer.Write("</tr>\n</thead>\n");
            _state.IsRowOpen = false;
        }

        // Close body cell if open
        if (_state.IsCellOpen)
        {
            CloseBodyCell(writer);
        }

        // Close body row if open
        if (_state.IsRowOpen)
        {
            writer.Write("</tr>\n");
            _state.IsRowOpen = false;
        }

        // Close tbody if open
        if (_state.IsInTbody)
        {
            writer.Write("</tbody>\n");
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