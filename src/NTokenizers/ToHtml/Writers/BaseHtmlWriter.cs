namespace NTokenizers.ToHtml.Writers;

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
    /// Asynchronously writes a value to the writer, optionally wrapped in a CSS class span.
    /// In pre blocks, newlines are preserved. In inline context, newlines become &lt;br/&gt;.
    /// </summary>
    internal async Task WriteValueAsync(TextWriter writer, string value, string? cssClass = null, bool inPreBlock = false)
    {
        var escaped = EscapeHtml(value);

        if (cssClass == null)
        {
            // Whitespace or plain text — no span wrapper
            if (inPreBlock)
            {
                await writer.WriteAsync(escaped);
            }
            else
            {
                await WriteWithBrAsync(writer, escaped);
            }
            return;
        }

        if (inPreBlock)
        {
            await writer.WriteAsync($"<span class=\"{cssClass}\">{escaped}</span>");
        }
        else
        {
            await WriteWithBrInSpanAsync(writer, escaped, cssClass);
        }
    }

    private static async Task WriteWithBrAsync(TextWriter writer, string escaped)
    {
        var parts = escaped.Split('\n');
        for (int i = 0; i < parts.Length; i++)
        {
            await writer.WriteAsync(parts[i]);
            if (i < parts.Length - 1)
                await writer.WriteAsync("<br/>");
        }
    }

    private static async Task WriteWithBrInSpanAsync(TextWriter writer, string escaped, string cssClass)
    {
        var parts = escaped.Split('\n');
        for (int i = 0; i < parts.Length; i++)
        {
            await writer.WriteAsync($"<span class=\"{cssClass}\">{parts[i]}</span>");
            if (i < parts.Length - 1)
                await writer.WriteAsync("<br/>");
        }
    }
}
