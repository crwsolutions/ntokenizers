namespace NTokenizers.Markdown;

/// <summary>
/// Specialized parser context for inline tokens only (no block-level constructs).
/// </summary>
internal class InlineMarkdownTokenizer : BaseMarkdownTokenizer
{
    internal static InlineMarkdownTokenizer Create() => new();

    internal protected override async Task ParseAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            var ch = Peek();
            if (ch == -1 || ch == '\n' || ch == '\r')
            {
                break;
            }

            if (await TryParseInlineConstructAsync((char)ch))
            {
                continue;
            }

            // Regular character
            _buffer.Append((char)Read());
        }

        await EmitTextAsync();
    }
}
