using NTokenizers.Markup;
using Spectre.Console;

internal static class MarkupListItemWriter
{
    internal static void Write(ListItemMetadata listItemMeta)
    {
        AnsiConsole.Write($"{listItemMeta.Number} ");
        listItemMeta.OnInlineToken = MarkupWriter.Write;
    }
}