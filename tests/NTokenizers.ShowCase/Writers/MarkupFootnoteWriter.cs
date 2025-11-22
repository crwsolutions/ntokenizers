using NTokenizers.Markup;
using Spectre.Console;

internal static class MarkupFootnoteWriter
{
    internal static void Write(FootnoteMetadata footnoteMeta)
    {
        AnsiConsole.Write($"[^{footnoteMeta.Id}]");
    }
}