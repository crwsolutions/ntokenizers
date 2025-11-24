using NTokenizers.Markup;
using Spectre.Console;

internal static class MarkupOrderedListItemWriter
{
    internal static void Write(OrderedListItemMetadata listItemMeta)
    {
        AnsiConsole.Write($"{listItemMeta.Number}. ".PadRight(5));
        listItemMeta.OnInlineToken = MarkupWriter.Write;
    }
}
