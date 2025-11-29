using NTokenizers.Markup.Metadata;
using Spectre.Console.Extensions.NTokenizers.Styles;

namespace Spectre.Console.Extensions.NTokenizers.Writers;

internal sealed class MarkupOrderedListItemWriter(MarkupOrderedListItemStyles styles)
{
    internal void Write(OrderedListItemMetadata metadata)
    {
        AnsiConsole.Write(new Console.Markup($"{metadata.Number}. ", styles.Number));
        metadata.OnInlineToken = MarkupWriter.Write;
        while (metadata.IsProcessing)
        {
            Thread.Sleep(3);
        }
        AnsiConsole.WriteLine();
    }
}
