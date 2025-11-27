using NTokenizers.Markup;

namespace Spectre.Console.Extensions.NTokenizers.Writers;

public static class MarkupLinkWriter
{
    public static void Write(LinkMetadata linkMeta)
    {
        AnsiConsole.Write($"[{linkMeta.Title}]({linkMeta.Url})");
    }
}