using NTokenizers.Markup;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace NTokenizers.ShowCase.Writers;

internal sealed class MarkupOrderedListItemWriter : BaseInlineWriter<MarkupToken, MarkupTokenType>
{
    protected override void Started(InlineMarkupMetadata<MarkupToken> metadata)
    {
        if (metadata is not OrderedListItemMetadata listItemMeta)
        {
            return;
        }
        _liveParagraph.Append($"\n{listItemMeta.Number}. ".PadRight(5), new Style(Color.Aqua));
    }

    protected override IRenderable GetIRendable() => _liveParagraph;
}
