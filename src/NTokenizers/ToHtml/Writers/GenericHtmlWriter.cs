using NTokenizers.Markdown;
using System.Text;

namespace NTokenizers.ToHtml.Writers;

/// <summary>
/// Writer for generic code blocks (unknown language). Emits plain text with no syntax highlighting.
/// </summary>
internal sealed class GenericHtmlWriter : AbstractTokenToHtmlWriter<MarkdownToken>
{
    internal override void WriteAdditionalCss(StringBuilder css)
    {
        css.AppendLine("/* === Generic code tokens === */");
        css.AppendLine(".tok-generic { color: #393B34; }");
        css.AppendLine();
    }

    internal override void WriteHtml(MarkdownToken token, TextWriter writer)
    {
        // Generic tokenizer only emits Text tokens
        WriteValue(writer, token.Value, "tok-generic", inPreBlock: true);
    }
}
