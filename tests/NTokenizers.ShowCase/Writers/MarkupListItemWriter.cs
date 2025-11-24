using NTokenizers.Markup;
using Spectre.Console;

internal static class MarkupListItemWriter
{
    internal static void Write(ListItemMetadata listItemMeta)
    {
        AnsiConsole.Write(new Markup($"{listItemMeta.Marker} ", new Style(Color.Turquoise2)));
        listItemMeta.OnInlineToken = MarkupWriter.Write;
    }
}