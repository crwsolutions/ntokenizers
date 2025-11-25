using NTokenizers.Markup;
using Spectre.Console;
using System.Collections.Concurrent;

namespace NTokenizers.ShowCase.Writers;
public abstract class BaseInlineWriter<TToken, TTokentype> where TToken : IToken<TTokentype> where TTokentype : Enum
{
    protected abstract Style GetStyle(TTokentype token);

    public void Write(InlineMarkupMetadata<TToken> metadata)
    {
        var started = false;
        var tokenQueue = new ConcurrentQueue<TToken>();
        var liveParagraph = new Paragraph("");

        metadata.OnInlineToken = async inlineToken =>
        {
            tokenQueue.Enqueue(inlineToken);

            if (!started)
            {
                started = true;

                await AnsiConsole.Live(
                    new Panel(liveParagraph)
                        .Border(new LeftBoxBorder())
                        .BorderStyle(new Style(Color.Blue))
                )
                .StartAsync(async ctx =>
                {
                    while (metadata.IsProcessing || !tokenQueue.IsEmpty)
                    {
                        while (tokenQueue.TryDequeue(out var token))
                        {
                            liveParagraph.Append(Spectre.Console.Markup.Escape(token.Value), GetStyle(token.TokenType));
                            ctx.Refresh();
                        }

                        await Task.Delay(2);
                    }
                });
            }
        };
    }
}
