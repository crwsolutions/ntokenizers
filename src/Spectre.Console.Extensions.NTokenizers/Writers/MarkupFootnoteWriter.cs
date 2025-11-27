using NTokenizers.Markup;

namespace Spectre.Console.Extensions.NTokenizers.Writers;

public static class MarkupFootnoteWriter
{
    public static void Write(FootnoteMetadata footnoteMeta)
    {
        AnsiConsole.Write($"[^{footnoteMeta.Id}]");
    }
}