using NTokenizers.Markup;
using Spectre.Console;
using Spectre.Console.Rendering;
using System.Collections.Concurrent;

internal static class MarkupBlockquoteWriter
{
    internal static void Write(BlockquoteMetadata meta)
    {
        var started = false;
        var tokenQueue = new ConcurrentQueue<MarkupToken>();
        var liveParagraph = new Paragraph("");

        meta.OnInlineToken = token => 
        {
            tokenQueue.Enqueue(token);

            if (!started)
            { 
                started = true;
                AnsiConsole.Live(
                    new Panel(liveParagraph)
                        .Border(new LeftBoxBorder())
                        .BorderStyle(new Style(Color.Blue))
                )
                .Start(ctx =>
                {
                    while (tokenQueue.TryDequeue(out var token))
                    {
                        if (token.TokenType == MarkupTokenType.EndOfInline)
                        {
                            break;
                        }

                        MarkupWriter.Write(liveParagraph, token);
                        ctx.Refresh();
                    }
                });
            }
        };
    }
}

internal sealed class LeftBoxBorder : BoxBorder
{
    public override string GetPart(BoxBorderPart part) => part switch
    {
        BoxBorderPart.TopLeft => "│ ",
        BoxBorderPart.Left => "│ ",
        BoxBorderPart.BottomLeft => "│ ",
        _ => " "
    };
}