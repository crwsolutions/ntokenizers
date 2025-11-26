using NTokenizers.Markup;
using Spectre.Console;

namespace NTokenizers.ShowCase.Writers;

internal static class MarkupEmojiWriter
{
    internal static void Write(EmojiMetadata emojiMeta)
    {
        AnsiConsole.Write(emojiMeta.Name);
    }
}