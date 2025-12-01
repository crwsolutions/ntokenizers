using NTokenizers.Markup.Metadata;
using Spectre.Console.Extensions.NTokenizers.Styles;

namespace Spectre.Console.Extensions.NTokenizers.Writers;

internal sealed class MarkupListItemWriter(MarkupListItemStyles styles)
{
    internal async Task WriteAsync(ListItemMetadata metadata)
    {
        AnsiConsole.Write(new Console.Markup($"{metadata.Marker} ", styles.Marker));
        await metadata.RegisterInlineTokenHandler(
            async token => await MarkupWriter.WriteAsync(token));
        AnsiConsole.WriteLine();
    }
}