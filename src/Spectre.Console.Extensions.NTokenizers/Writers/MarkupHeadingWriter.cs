using NTokenizers.Markup;
using Spectre.Console.Rendering;

namespace Spectre.Console.Extensions.NTokenizers.Writers;

public sealed class MarkupHeadingWriter : BaseInlineWriter<MarkupToken, MarkupTokenType>
{
    private static readonly Style HeadingLevel1Color = Color.Yellow2;
    private static readonly Style HeadingLevel2To4Style = Color.DarkOliveGreen1_1;
    private static readonly Style HeadingLevel5AndAboveStyle = Color.DarkSeaGreen1_1;

    private Style _style = default!;
    private int _lenght = 0;

    protected override Style GetStyle(MarkupTokenType token)
    {
        return _style;
    }

    protected override void Started(InlineMarkupMetadata<MarkupToken> metadata)
    {
        if (metadata is HeadingMetadata meta)
        {
            _style = meta.Level switch
            {
                1 => HeadingLevel1Color,
                >= 2 and <= 4 => HeadingLevel2To4Style,
                _ => HeadingLevel5AndAboveStyle
            };
            if (meta.Level == 1)
            {
                _liveParagraph.Append("** ", _style);
                _lenght += 3;
            }
        }
    }

    protected override void Finalize(InlineMarkupMetadata<MarkupToken> metadata)
    {
        if (metadata is HeadingMetadata meta)
        {
            if (meta.Level == 1)
            {
                _liveParagraph.Append(" **", _style);
                _lenght += 3;
            }
            if (meta.Level < 3)
            {
                _liveParagraph.Append("\n");
                _liveParagraph.Append(new string('=', _lenght), _style);
            }
            else if (meta.Level < 5)
            {
                _liveParagraph.Append("\n");
                _liveParagraph.Append(new string('-', _lenght), _style);
            }

            _liveParagraph.Append("\n");
        }
    }

    protected override void WriteToken(Paragraph liveParagraph, MarkupToken token)
    {
        var inlineValue = Markup.Escape(token.Value);
        _lenght += token.Value.Length;
        MarkupWriter.Write(liveParagraph, token, _style);
    }

    protected override IRenderable GetIRendable() => _liveParagraph;
}
