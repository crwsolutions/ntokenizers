using NTokenizers.Markup;
using Spectre.Console;

internal static class MarkupEmojiWriter
{
    internal static void Write(EmojiMetadata emojiMeta)
    {
        AnsiConsole.Write(emojiMeta.Name);
    }
}