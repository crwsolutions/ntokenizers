using NTokenizers.Markup;
using Spectre.Console;

internal static class MarkupBlockquoteWriter
{
    internal static void Write(BlockquoteMetadata meta)
    {
        AnsiConsole.WriteLine($"> ");
        //meta.OnInlineToken = MarkupWriter.Write;
    }
}