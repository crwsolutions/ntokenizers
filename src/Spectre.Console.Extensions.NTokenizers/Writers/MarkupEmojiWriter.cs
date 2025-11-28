using NTokenizers.Markup;

namespace Spectre.Console.Extensions.NTokenizers.Writers;

internal static class MarkupEmojiWriter
{
    internal static void Write(EmojiMetadata emojiMeta)
    {
        AnsiConsole.Write(emojiMeta.Name);
    }
}