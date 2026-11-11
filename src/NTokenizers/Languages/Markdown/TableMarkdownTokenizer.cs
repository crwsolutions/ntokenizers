using NTokenizers.Markdown.Metadata;

namespace NTokenizers.Markdown;

internal class TableMarkdownTokenizer : BaseMarkdownTokenizer
{
    private readonly TableMetadata _tableMetaData;

    internal TableMarkdownTokenizer(TableMetadata metadata) => _tableMetaData = metadata;

    internal protected override async Task ParseAsync(CancellationToken ct)
    {
        do
        {
            if (ct.IsCancellationRequested) break;

            if (Peek() == '|')
            {
                Read();
            }

            if ((Peek() == ' ' || Peek() == '-' || Peek() == ':') && (PeekAhead(1) == '-' || PeekAhead(1) == ':')) //Alignments row
            {
                var alignments = new List<Justify>();
                while (Peek() != -1 && Peek() != '\n' && Peek() != '\r')
                {
                    alignments.Add(ReadAlignment());
                    if (Peek() == '|')
                    {
                        Read();
                    }
                }
                _tableMetaData.Alignments = alignments;
                await _onToken(new MarkdownToken(MarkdownTokenType.TableAlignments, string.Empty));
            }
            else
            {
                await _onToken(new MarkdownToken(MarkdownTokenType.TableRow, string.Empty));
                await _onToken(new MarkdownToken(MarkdownTokenType.TableCell, string.Empty));

                while (Peek() != -1 && Peek() != '\n' && Peek() != '\r')
                {
                    if (Peek() == '|')
                    {
                        await EmitTextAsync();

                        Read();
                        if (Peek() != -1 && Peek() != '\n' && Peek() != '\r')
                        {
                            await _onToken(new MarkdownToken(MarkdownTokenType.TableCell, string.Empty));
                        }
                        else
                        {
                            break;
                        }
                    }

                    // Try inline constructs
                    if (await TryParseBoldOrItalicAsync()) continue;
                    if (await TryParseInlineCodeAsync()) continue;
                    if (await TryParseLinkAsync()) continue;
                    if (await TryParseImageAsync()) continue;
                    if (await TryParseEmojiAsync()) continue;
                    if (await TryParseSubscriptAsync()) continue;
                    if (await TryParseStrikethroughAsync()) continue;
                    if (await TryParseSuperscriptAsync()) continue;
                    if (await TryParseInsertedTextAsync()) continue;
                    if (await TryParseMarkedTextAsync()) continue;

                    // Regular character
                    _buffer.Append((char)Read());
                }
            }

            if (Peek() == '\r')
            {
                Read();
            }
            if (Peek() == '\n')
            {
                Read();
            }

        } while (Peek() == '|' && !ct.IsCancellationRequested); //Next line of the table

        await EmitTextAsync();
    }

    private Justify ReadAlignment()
    {
        var justify = Justify.Left;

        while (Peek() == ' ')
        {
            Read();
        }

        if (Peek() == ':')
        {
            justify = Justify.Center;
            Read();
        }
        while (Peek() == '-' || Peek() == ' ')
        {
            Read();
        }
        if (Peek() == ':')
        {
            if (justify != Justify.Center)
            {
                justify = Justify.Right;
            }
            Read();
        }
        else
        {
            justify = Justify.Left;
            Read();
        }

        while (Peek() == ' ')
        {
            Read();
        }

        return justify;
    }
}
