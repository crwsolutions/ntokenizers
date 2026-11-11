using System.Text;
using NTokenizers.Core;

namespace NTokenizers.Markdown.Metadata;

/// <summary>
/// Metadata for heading tokens, containing the heading level.
/// </summary>
/// <param name="Level">The heading level (1-6).</param>
/// <param name="reader">The section reader the sub-tokenizer reads from.</param>
/// <param name="capture">The capture builder that records every character of the stream.</param>
/// <param name="lookahead">The lookahead queue the sub-tokenizer reads from.</param>
public sealed class HeadingMetadata(int Level, TextReader reader, StringBuilder capture, Queue<char> lookahead)
    : InlineMetadata<MarkdownToken>(reader, capture, lookahead)
{
    /// <summary>
    /// Gets the current level of the heading (1-6).
    /// </summary>
    public int Level { get; } = Level;

    internal override Task RunInlinesAsync(Func<MarkdownToken, Task> onToken, CancellationToken ct) =>
        HeadingMarkdownTokenizer.Create().ParseAsync(Reader, Capture, Lookahead, ct, onToken);
}
