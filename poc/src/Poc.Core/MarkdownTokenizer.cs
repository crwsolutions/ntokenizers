using System.Text;

namespace Poc.Core;

/// <summary>
/// A small line-based markdown tokenizer demonstrating the sub-tokenizer
/// architecture:
/// <list type="bullet">
/// <item>fenced code blocks delegate their content to a <see cref="CodeTokenizer"/>
/// (a different token type, <see cref="CodeToken"/>);</item>
/// <item>blockquotes delegate their content to a nested <see cref="MarkdownTokenizer"/>
/// (the same token type: recursion).</item>
/// </list>
/// <para>
/// PoC simplification versus NTokenizers: the parent collects each section
/// into a <see cref="StringReader"/> instead of handing the child the shared
/// reader with a stop delimiter. The architectural contract (client-driven,
/// awaited per token) is identical.
/// </para>
/// </summary>
public sealed class MarkdownTokenizer : BaseTokenizer<MarkdownToken>
{
    private string? _pendingLine;

    /// <summary>Creates a new <see cref="MarkdownTokenizer"/>.</summary>
    public static MarkdownTokenizer Create() => new();

    protected override async Task ParseCoreAsync(CancellationToken ct)
    {
        while (await NextLineAsync() is { } line)
        {
            ct.ThrowIfCancellationRequested();

            if (line.StartsWith("```"))
            {
                // Fenced code block: collect the section, then hand it to a
                // CodeTokenizer through the metadata.
                var language = line["```".Length..].Trim();
                if (language.Length == 0)
                {
                    language = "text";
                }

                var section = new StringBuilder();
                string? contentLine;
                do
                {
                    contentLine = await NextLineAsync();
                    if (contentLine is null || IsClosingFence(contentLine))
                    {
                        break;
                    }

                    section.AppendLine(contentLine);
                } while (true);

                await EmitAsync(new MarkdownToken(
                    MarkdownTokenType.CodeBlock,
                    language,
                    new CodeBlockMetadata(language, new StringReader(section.ToString()))));
            }
            else if (IsQuoteLine(line))
            {
                // Blockquote: collect the quoted lines (markers stripped),
                // then hand the section to a nested MarkdownTokenizer.
                var section = new StringBuilder();
                section.AppendLine(QuoteContent(line));

                string? quoteLine;
                do
                {
                    quoteLine = await NextLineAsync();
                    if (quoteLine is null || !IsQuoteLine(quoteLine))
                    {
                        break;
                    }

                    section.AppendLine(QuoteContent(quoteLine));
                } while (true);

                if (quoteLine is not null)
                {
                    // The line that ended the quote belongs to the outer scope.
                    _pendingLine = quoteLine;
                }

                await EmitAsync(new MarkdownToken(
                    MarkdownTokenType.Blockquote,
                    string.Empty,
                    new BlockquoteMetadata(new StringReader(section.ToString()))));
            }
            else if (line.StartsWith("# "))
            {
                await EmitAsync(new MarkdownToken(MarkdownTokenType.Heading, line["# ".Length..]));
            }
            else if (line.Trim().Length > 0)
            {
                await EmitAsync(new MarkdownToken(MarkdownTokenType.Text, line));
            }
        }
    }

    private async ValueTask<string?> NextLineAsync()
    {
        if (_pendingLine is { } pending)
        {
            _pendingLine = null;
            return pending;
        }

        return await Reader.ReadLineAsync();
    }

    private static bool IsClosingFence(string line) => line.Trim() == "```";

    private static bool IsQuoteLine(string line) => line == ">" || line.StartsWith("> ");

    private static string QuoteContent(string line) =>
        line == ">" ? string.Empty : line["> ".Length..];
}
