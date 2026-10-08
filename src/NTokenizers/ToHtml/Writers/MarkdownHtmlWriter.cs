using System.Text;
using NTokenizers.Markdown;

namespace NTokenizers.ToHtml.Writers;

/// <summary>
/// Top-level Markdown document writer. Delegates all token dispatch to the shared
/// <see cref="MarkdownBlockTokenDispatcher"/> so that top-level and blockquote content render
/// identically.
/// </summary>
internal sealed class MarkdownHtmlWriter : BaseHtmlWriter, IAdditionalCssWriter
{
    private readonly MarkdownBlockTokenDispatcher _dispatcher = new();

    public void WriteAdditionalCss(StringBuilder bob)
    {
        var codeBlockWriter = new CodeblockHtmlWriter();
        codeBlockWriter.WriteAdditionalCss(bob);
    }

    internal Task WriteTokenAsync(MarkdownToken token, TextWriter writer) =>
        _dispatcher.WriteTokenAsync(token, writer);
}
