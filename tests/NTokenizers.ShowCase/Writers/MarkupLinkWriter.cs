using NTokenizers.Markup;
using Spectre.Console;

namespace NTokenizers.ShowCase.Writers;

internal static class MarkupLinkWriter
{
    internal static void Write(LinkMetadata linkMeta)
    {
        AnsiConsole.Write($"[{linkMeta.Title}]({linkMeta.Url})");
    }
}