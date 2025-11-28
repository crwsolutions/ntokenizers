using NTokenizers.Markup;
using Spectre.Console.Extensions.NTokenizers.Styles;

namespace Spectre.Console.Extensions.NTokenizers.Writers;

internal sealed class MarkupListItemWriter(MarkupListItemStyles styles)
{
    internal void Write(ListItemMetadata metadata)
    {
        AnsiConsole.Write(new Console.Markup($"{metadata.Marker}. ", styles.Marker));
        metadata.OnInlineToken = MarkupWriter.Write;
        while (metadata.IsProcessing)
        {
            Thread.Sleep(3);
        }
        AnsiConsole.WriteLine();
    }
}