using NTokenizers.Markup;
using Spectre.Console.Extensions.NTokenizers.Styles;
using Spectre.Console.Rendering;

namespace Spectre.Console.Extensions.NTokenizers.Writers;

public sealed class MarkupOrderedListItemWriter(MarkupOrderedListItemStyles styles) : BaseInlineWriter<MarkupToken, MarkupTokenType>
{
    protected override void Started(InlineMarkupMetadata<MarkupToken> metadata)
    {
        if (metadata is not OrderedListItemMetadata listItemMeta)
        {
            return;
        }
        _liveParagraph.Append($"\n{listItemMeta.Number}. ".PadRight(5), styles.Number);
    }

    protected override IRenderable GetIRendable() => _liveParagraph;
}
