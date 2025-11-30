using NTokenizers.Markup;
using NTokenizers.Markup.Metadata;
using Spectre.Console.Extensions.NTokenizers.Extensions;
using Spectre.Console.Extensions.NTokenizers.Styles;
using System.Diagnostics;

namespace Spectre.Console.Extensions.NTokenizers.Writers;

internal class MarkupTableWriter
{
    private readonly MarkupStyles _markupStyles;

    public MarkupTableWriter(MarkupStyles markupStyles)
    {
        _markupStyles = markupStyles;
    }

    internal void Write(TableMetadata metadata)
    {
        var spectreTable = new Table();

        var column = -1;
        //TableColumn? currentColumn = null;
        TableRow? currentRow = null;
        var cellParagraphs = new List<Paragraph>();
        var liveParagraph = new Paragraph();
        AnsiConsole.Live(spectreTable)
        .Start(ctx =>
        {
            metadata.OnInlineToken = inlineToken =>
            {
                if (inlineToken.TokenType == MarkupTokenType.TableAlignments)
                {
                    HandleAlignments(spectreTable, metadata);
                }
                else if (inlineToken.TokenType == MarkupTokenType.TableRow)
                {
                    //Handle new row
                    column = -1;

                    if (spectreTable.Columns.Count > 0)
                    { 
                        cellParagraphs = Enumerable.Range(0, spectreTable.Columns.Count).Select(_ => new Paragraph()).ToList();
                        currentRow = new TableRow(cellParagraphs);
                        spectreTable.AddRow(currentRow);
                    }
                }
                else if (inlineToken.TokenType == MarkupTokenType.TableCell)
                {
                    column++;
                    if (spectreTable.Rows.Count == 0)
                    {
                        liveParagraph = new Paragraph();
                        spectreTable.AddColumn(new TableColumn(liveParagraph));
                    }
                    else
                    {
                        if (column < cellParagraphs.Count)
                        {
                            liveParagraph = cellParagraphs[column];
                        }
                    }
                }
                else //Write cell content
                {
                    WriteToken(liveParagraph, inlineToken);
                }

                ctx.Refresh();
            };

            while (metadata.IsProcessing)
            {
                Thread.Sleep(3);
            }

            ctx.Refresh();
        });
    }

    private void HandleAlignments(Table spectreTable, TableMetadata metadata)
    {
        if (metadata.Alignments == null || metadata.Alignments.Count == 0)
        {
            return;
        }

        var aligns = metadata.Alignments;

        if (spectreTable.Columns.Count == 0)
        {
            foreach (var justify in aligns)
            {
                var col = new TableColumn("")
                {
                    Alignment = justify.ToSpectreJustify()
                };
                spectreTable.AddColumn(col);
            }
        }
        else
        {
            // Case B: Columns already exist → update only
            for (int i = 0; i < spectreTable.Columns.Count; i++)
            {
                if (i < aligns.Count)
                {
                    // Alignment provided → apply it
                    spectreTable.Columns[i].Alignment = aligns[i].ToSpectreJustify();
                }
                else
                {
                    // No alignment provided → optionally leave unchanged
                    // (Or set a default if desired)
                }
            }

            // Case C: More alignments than columns → append new columns
            for (int i = spectreTable.Columns.Count; i < aligns.Count; i++)
            {
                var col = new TableColumn("")
                {
                    Alignment = aligns[i].ToSpectreJustify()
                };
                spectreTable.AddColumn(col);
            }
        }
    }

    private void WriteToken(Paragraph liveParagraph, MarkupToken token)
    {
        if (string.IsNullOrEmpty(token.Value))
        {
            return;
        }
        Debug.WriteLine($"Writing token: `{token.Value}` of type `{token.TokenType}`");
        MarkupWriter.Write(liveParagraph, token, _markupStyles.TableCell);
    }
}
