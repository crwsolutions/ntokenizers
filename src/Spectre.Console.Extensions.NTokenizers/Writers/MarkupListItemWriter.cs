using NTokenizers.Markup;
using Spectre.Console.Rendering;

namespace Spectre.Console.Extensions.NTokenizers.Writers;

public sealed class MarkupListItemWriter : BaseInlineWriter<MarkupToken, MarkupTokenType>
{
    protected override void Started(InlineMarkupMetadata<MarkupToken> metadata)
    {
        if (metadata is not ListItemMetadata listItemMeta)
        {
            return;
        }
        _liveParagraph.Append($"\n{listItemMeta.Marker} ", new Style(Color.Turquoise2));
    }

    protected override void Finalize(InlineMarkupMetadata<MarkupToken> metadata)
    {
        //_liveParagraph.Append("koekoek");
    }

    protected override IRenderable GetIRendable() => _liveParagraph;
}