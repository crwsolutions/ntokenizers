using NTokenizers.Markup;
using Spectre.Console;

internal static class MarkupHeadingWriter
{
    internal static void Write(HeadingMetadata meta)
    {
        meta.OnInlineToken = inlineToken =>
        {
            var inlineValue = Markup.Escape(inlineToken.Value);
            var inlineColored = inlineToken.TokenType switch
            {
                MarkupTokenType.Bold => new Markup($"[blue]{inlineValue}[/]"),
                _ => new Markup($"[yellow]{inlineValue}[/]")
            };
            AnsiConsole.Write(inlineColored);
        };
    }
}
