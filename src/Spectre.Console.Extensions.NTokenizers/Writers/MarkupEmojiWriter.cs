using NTokenizers.Markup;

namespace Spectre.Console.Extensions.NTokenizers.Writers;

public static class MarkupEmojiWriter
{
    public static void Write(EmojiMetadata emojiMeta)
    {
        AnsiConsole.Write(emojiMeta.Name);
    }
}