using NTokenizers.Markup;
using Spectre.Console;

internal static class MarkupOrderedListItemWriter
{
    internal static void Write(OrderedListItemMetadata listItemMeta)
    {
        AnsiConsole.Write($"{listItemMeta.Number} ");
        listItemMeta.OnInlineToken = MarkupWriter.Write;
    }
}
