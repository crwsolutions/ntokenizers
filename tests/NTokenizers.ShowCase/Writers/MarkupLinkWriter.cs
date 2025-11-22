using NTokenizers.Markup;
using Spectre.Console;

internal static class MarkupLinkWriter
{
    internal static void Write(LinkMetadata linkMeta)
    {
        AnsiConsole.Write($"[{linkMeta.Title}]({linkMeta.Url})");
    }
}