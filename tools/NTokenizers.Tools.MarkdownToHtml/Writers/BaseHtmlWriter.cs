namespace NTokenizers.Tools.MarkdownToHtml.Writers;

/// <summary>
/// Shared utilities for all HTML writers: escaping, newline handling, and common CSS.
/// </summary>
internal class BaseHtmlWriter
{
    /// <summary>
    /// Escapes HTML special characters in the given value.
    /// </summary>
    internal string EscapeHtml(string value)
    {
        return value
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;");
    }

    /// <summary>
    /// Writes a value to the writer, optionally wrapped in a CSS class span.
    /// In pre blocks, newlines are preserved. In inline context, newlines become &lt;br/&gt;.
    /// </summary>
    internal void WriteValue(TextWriter writer, string value, string? cssClass, bool inPreBlock = false)
    {
        var escaped = EscapeHtml(value);

        if (cssClass == null)
        {
            // Whitespace or plain text — no span wrapper
            if (inPreBlock)
            {
                writer.Write(escaped);
            }
            else
            {
                WriteWithBr(writer, escaped);
            }
            return;
        }

        if (inPreBlock)
        {
            writer.Write($"<span class=\"{cssClass}\">{escaped}</span>");
        }
        else
        {
            WriteWithBrInSpan(writer, escaped, cssClass);
        }
    }

    private static void WriteWithBr(TextWriter writer, string escaped)
    {
        var parts = escaped.Split('\n');
        for (int i = 0; i < parts.Length; i++)
        {
            writer.Write(parts[i]);
            if (i < parts.Length - 1)
                writer.Write("<br/>");
        }
    }

    private static void WriteWithBrInSpan(TextWriter writer, string escaped, string cssClass)
    {
        var parts = escaped.Split('\n');
        for (int i = 0; i < parts.Length; i++)
        {
            writer.Write($"<span class=\"{cssClass}\">{parts[i]}</span>");
            if (i < parts.Length - 1)
                writer.Write("<br/>");
        }
    }
}
