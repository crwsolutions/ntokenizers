using NTokenizers.Markup.Metadata;

namespace Spectre.Console.Extensions.NTokenizers.Writers;

internal static class MarkupFootnoteWriter
{
    internal static void Write(FootnoteMetadata footnoteMeta)
    {
        AnsiConsole.Write($"[^{footnoteMeta.Id}]");
    }
}