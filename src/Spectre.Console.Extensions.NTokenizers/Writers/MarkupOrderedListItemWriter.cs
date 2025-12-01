using NTokenizers.Markup.Metadata;
using Spectre.Console.Extensions.NTokenizers.Styles;

namespace Spectre.Console.Extensions.NTokenizers.Writers;

internal sealed class MarkupOrderedListItemWriter(MarkupOrderedListItemStyles styles)
{
    internal async Task WriteAsync(OrderedListItemMetadata metadata)
    {
        AnsiConsole.Write(new Console.Markup($"{metadata.Number}. ", styles.Number));
        await metadata.RegisterInlineTokenHandler(
            async token => await MarkupWriter.WriteAsync(token));
        AnsiConsole.WriteLine();
    }
}
