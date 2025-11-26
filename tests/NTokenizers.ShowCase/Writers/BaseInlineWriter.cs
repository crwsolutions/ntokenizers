using NTokenizers.Markup;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace NTokenizers.ShowCase.Writers;
public abstract class BaseInlineWriter<TToken, TTokentype> where TToken : IToken<TTokentype> where TTokentype : Enum
{
    protected virtual Style GetStyle(TTokentype token) => Style.Plain;

    protected readonly Paragraph _liveParagraph = new("");

    public void Write(InlineMarkupMetadata<TToken> metadata)
    {
        AnsiConsole.Live(GetIRendable())
        .Start(ctx =>
        {
            Started(metadata);
            metadata.OnInlineToken = inlineToken =>
            {
                WriteToken(_liveParagraph, inlineToken);
                ctx.Refresh();
            };

            while (metadata.IsProcessing)
            {
                Thread.Sleep(3);
            }
            Finalize(metadata);
            ctx.Refresh();
        });
    }

    protected virtual IRenderable GetIRendable()
    {
        return new Panel(_liveParagraph)
            .Border(new LeftBoxBorder())
            .BorderStyle(new Style(Color.Blue));
    }

    protected virtual void Started(InlineMarkupMetadata<TToken> metadata)
    {

    }

    protected virtual void Finalize(InlineMarkupMetadata<TToken> metadata)
    {

    }

    protected virtual void WriteToken(Paragraph liveParagraph, TToken token)
    {
        liveParagraph.Append(Spectre.Console.Markup.Escape(token.Value), GetStyle(token.TokenType));
    }
}
