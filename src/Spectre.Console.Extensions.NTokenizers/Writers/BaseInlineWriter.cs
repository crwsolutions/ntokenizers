using NTokenizers.Core;
using NTokenizers.Markup;
using Spectre.Console.Rendering;
using System.Diagnostics;

namespace Spectre.Console.Extensions.NTokenizers.Writers;

public abstract class BaseInlineWriter<TToken, TTokentype> where TToken : IToken<TTokentype> where TTokentype : Enum
{
    protected virtual Style GetStyle(TTokentype token) => Style.Plain;

    protected readonly Paragraph _liveParagraph = new("");

    internal void Write(InlineMarkupMetadata<TToken> metadata)
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
            .BorderStyle(new Style(Color.Green));
    }

    protected virtual void Started(InlineMarkupMetadata<TToken> metadata)
    {

    }

    protected virtual void Finalize(InlineMarkupMetadata<TToken> metadata)
    {

    }

    protected virtual void WriteToken(Paragraph liveParagraph, TToken token)
    {
        if (string.IsNullOrEmpty(token.Value))
        {
            return;
        }
        Debug.WriteLine($"Writing token: `{token.Value}` of type `{token.TokenType}`");
        liveParagraph.Append(token.Value, GetStyle(token.TokenType));
    }
}
