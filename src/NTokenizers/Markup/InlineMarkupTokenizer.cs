namespace NTokenizers.Markup;

/// <summary>
/// Specialized parser context for inline tokens only (no block-level constructs).
/// </summary>
internal class InlineMarkupTokenizer : BaseMarkupTokenizer
{
    internal static InlineMarkupTokenizer Create() => new();

    internal protected override Task ParseAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            var ch = Peek();
            if (ch == -1 || ch == '\n' || ch == '\r')
            {
                break;
            }

            if (TryParseInlineConstruct((char)ch))
            {
                continue;
            }

            // Regular character
            _buffer.Append((char)Read());
        }

        EmitText();

        return Task.CompletedTask;
    }
}
