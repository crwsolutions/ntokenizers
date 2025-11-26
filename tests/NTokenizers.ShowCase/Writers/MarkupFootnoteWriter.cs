using NTokenizers.Markup;
using Spectre.Console;

namespace NTokenizers.ShowCase.Writers;

internal static class MarkupFootnoteWriter
{
    internal static void Write(FootnoteMetadata footnoteMeta)
    {
        AnsiConsole.Write($"[^{footnoteMeta.Id}]");
    }
}