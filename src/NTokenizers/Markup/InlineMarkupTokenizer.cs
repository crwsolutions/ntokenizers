namespace NTokenizers.Markup;

/// <summary>
/// Specialized parser context for inline tokens only (no block-level constructs).
/// </summary>
internal class InlineMarkupTokenizer : BaseMarkupTokenizer
{
    internal static InlineMarkupTokenizer Create() => new();

    internal protected override Task ParseAsync()
    {
        while (Peek() != -1 && Peek() != '\n' && Peek() != '\r')
        {
            // Try inline constructs
            if (TryParseBoldOrItalic()) continue;
            if (TryParseInlineCode()) continue;
            if (TryParseLink()) continue;
            if (TryParseImage()) continue;
            if (TryParseEmoji()) continue;
            if (TryParseSubscript()) continue;
            if (TryParseSuperscript()) continue;
            if (TryParseInsertedText()) continue;
            if (TryParseMarkedText()) continue;

            // Regular character
            _buffer.Append((char)Read());
        }

        EmitText();

        return Task.CompletedTask;
    }
}
