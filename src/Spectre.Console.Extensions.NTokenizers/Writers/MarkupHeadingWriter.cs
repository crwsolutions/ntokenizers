using NTokenizers.Markup;
using Spectre.Console.Extensions.NTokenizers.Styles;
using Spectre.Console.Rendering;

namespace Spectre.Console.Extensions.NTokenizers.Writers;

public sealed class MarkupHeadingWriter(MarkupHeadingStyles styles) : BaseInlineWriter<MarkupToken, MarkupTokenType>
{
    private Style _style = default!;
    private int _lenght = 0;

    protected override Style GetStyle(MarkupTokenType token) => _style;

    protected override void Started(InlineMarkupMetadata<MarkupToken> metadata)
    {
        _liveParagraph.Append("\n");
        if (metadata is HeadingMetadata meta)
        {
            _style = meta.Level switch
            {
                1 => styles.Level1,
                >= 2 and <= 4 => styles.Level2To4,
                _ => styles.Level5AndAbove
            };
            if (meta.Level == 1)
            {
                WriteToken("** ");
            }
        }
    }

    protected override void Finalize(InlineMarkupMetadata<MarkupToken> metadata)
    {
        if (metadata is HeadingMetadata meta)
        {
            if (meta.Level == 1)
            {
                WriteToken(" **");
            }
            if (meta.Level < 3)
            {
                WriteToken($"\n{new string('=', _lenght)}");
            }
            else if (meta.Level < 5)
            {
                WriteToken($"\n{new string('-', _lenght)}");
            }

            //WriteToken("\n");
        }
    }

    private void WriteToken(string text)
    {
        WriteToken(_liveParagraph, new MarkupToken(MarkupTokenType.Text, text));
    }

    protected override void WriteToken(Paragraph liveParagraph, MarkupToken token)
    {
        _lenght += token.Value.Length;
        MarkupWriter.Write(liveParagraph, token, _style);
    }

    protected override IRenderable GetIRendable() => _liveParagraph;
}
