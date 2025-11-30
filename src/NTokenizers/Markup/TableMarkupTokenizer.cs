using NTokenizers.Markup.Metadata;

namespace NTokenizers.Markup;
internal class TableMarkupTokenizer : BaseMarkupTokenizer
{
    private readonly TableMetadata _tableMetaData;

    internal TableMarkupTokenizer(TableMetadata metadata) => _tableMetaData = metadata;

    internal protected override void Parse()
    {
        do
        {
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
                _onToken(new MarkupToken(MarkupTokenType.TableAlignments, string.Empty));
            }
            else
            {
                _onToken(new MarkupToken(MarkupTokenType.TableRow, string.Empty));
                _onToken(new MarkupToken(MarkupTokenType.TableCell, string.Empty));

                while (Peek() != -1 && Peek() != '\n' && Peek() != '\r')
                {
                    if (Peek() == '|')
                    {
                        EmitText();

                        Read();
                        if (Peek() != -1 && Peek() != '\n' && Peek() != '\r')
                        {
                            _onToken(new MarkupToken(MarkupTokenType.TableCell, string.Empty));
                        }
                        else
                        {
                            break;
                        }
                    }

                    // Try inline constructs
                    if (TryParseBoldOrItalic()) continue;
                    if (TryParseInlineCode()) continue;
                    if (TryParseLink()) continue;
                    if (TryParseImage()) continue;
                    if (TryParseEmoji()) continue;
                    if (TryParseSubscript()) continue;
                    if (TryParseSuperscript()) continue;
                    if (TryParseInsertedText()) continue;
                    if (TryParseMarkedText()) continue;

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

        } while (Peek() == '|'); //Next line of the table

        EmitText();
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

