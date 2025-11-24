using NTokenizers.Markup;
using Spectre.Console;

internal static class MarkupHeadingWriter
{
    private static readonly Style HeadingLevel1Color = Color.Yellow2;
    private static readonly Style HeadingLevel2To4Style = Color.DarkOliveGreen1_1;
    private static readonly Style HeadingLevel5AndAboveStyle = Color.DarkSeaGreen1_1;

    internal static void Write(HeadingMetadata meta)
    {
        var style = meta.Level switch
        {
            1 => HeadingLevel1Color,
            >= 2 and <= 4 => HeadingLevel2To4Style,
            _ => HeadingLevel5AndAboveStyle
        };
        var lenght = 0;
        if (meta.Level == 1)
        {
            AnsiConsole.Write(new Markup("** ", style));
            lenght += 3;
        }

        meta.OnInlineToken = inlineToken =>
        {
            var inlineValue = Markup.Escape(inlineToken.Value);
            lenght += inlineToken.Value.Length;
            MarkupWriter.Write(inlineToken);
            //var inlineColored = inlineToken.TokenType switch
            //{
            //    MarkupTokenType.Bold => new Markup($"[blue]{inlineValue}[/]"),
            //    _ => new Markup($"[yellow]{inlineValue}[/]")
            //};
            //AnsiConsole.Write(inlineColored);

            if (inlineToken.TokenType == MarkupTokenType.EndOfInline)
            {
                if (meta.Level == 1)
                {
                    AnsiConsole.Write(new Markup(" **", style));
                    lenght += 3;
                }
                if (meta.Level < 3)
                {
                    AnsiConsole.WriteLine();
                    AnsiConsole.Write(new Markup(new string('=', lenght), style));
                }
                else if (meta.Level < 5)
                {
                    AnsiConsole.WriteLine();
                    AnsiConsole.Write(new Markup(new string('-', lenght), style));
                }
            }
        };
    }
}
