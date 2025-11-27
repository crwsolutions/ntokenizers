using NTokenizers.Markup;
using Spectre.Console.Extensions.NTokenizers.Styles;
using Spectre.Console.Rendering;

namespace Spectre.Console.Extensions.NTokenizers.Writers;

public sealed class MarkupListItemWriter(MarkupListItemStyles styles) : BaseInlineWriter<MarkupToken, MarkupTokenType>
{
    protected override void Started(InlineMarkupMetadata<MarkupToken> metadata)
    {
        if (metadata is not ListItemMetadata listItemMeta)
        {
            return;
        }
        _liveParagraph.Append($"\n{listItemMeta.Marker} ", styles.Marker);
    }

    protected override void Finalize(InlineMarkupMetadata<MarkupToken> metadata)
    {
        //_liveParagraph.Append("koekoek");
    }

    protected override IRenderable GetIRendable() => _liveParagraph;
}