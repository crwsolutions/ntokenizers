using NTokenizers.Markup;

namespace Spectre.Console.Extensions.NTokenizers.Writers;

internal static class MarkupLinkWriter
{
    internal static void Write(LinkMetadata linkMeta)
    {
        AnsiConsole.Write($"[{linkMeta.Title}]({linkMeta.Url})");
    }
}