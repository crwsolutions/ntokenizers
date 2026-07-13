using NTokenizers.Markdown;
using NTokenizers.ToHtml.Writers;
using System.Text;

namespace NTokenizers.ToHtml;

/// <summary>
/// Converts Markdown content to HTML output.
/// Provides methods for rendering fragments (body-only) or full HTML documents.
/// </summary>
public sealed class MarkdownConverter
{
    /// <summary>
    /// Gets the default CSS required for rendering Markdown HTML output.
    /// </summary>
    public static string GetCss()
    {
        var markdownWriter = new MarkdownHtmlWriter();
        var bob = new StringBuilder();
        markdownWriter.WriteAdditionalCss(bob);
        return bob.ToString();
    }

    /// <summary>
    /// Converts a Markdown string to an HTML fragment (no document wrapper).
    /// </summary>
    public static string ToHtml(string input)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        return ToHtmlAsync(stream).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Converts a Markdown stream to an HTML fragment (no document wrapper).
    /// </summary>
    public static async Task<string> ToHtmlAsync(Stream inputStream)
    {
        using var writer = new StringWriter();
        await WriteHtmlAsync(inputStream, writer);
        return writer.ToString();
    }

    /// <summary>
    /// Converts a Markdown stream to an HTML fragment and writes it directly to the provided writer.
    /// </summary>
    public static async Task WriteHtmlAsync(Stream inputStream, TextWriter writer)
    {
        var markdownWriter = new MarkdownHtmlWriter();
        await MarkdownTokenizer.Create().ParseAsync(inputStream, onToken: async token =>
        {
            await markdownWriter.WriteTokenAsync(token, writer);
        });
    }

    /// <summary>
    /// Converts a Markdown string to a complete HTML document (with DOCTYPE, head, body wrapper).
    /// </summary>
    public static string ToHtmlDocument(string input)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        return ToHtmlDocumentAsync(stream).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Converts a Markdown stream to a complete HTML document (with DOCTYPE, head, body wrapper).
    /// </summary>
    public static async Task<string> ToHtmlDocumentAsync(Stream inputStream)
    {
        using var writer = new StringWriter();
        await WriteHtmlDocumentAsync(inputStream, writer);
        return writer.ToString();
    }

    /// <summary>
    /// Converts a Markdown stream to a complete HTML document and writes it directly to the provided writer.
    /// </summary>
    public static async Task WriteHtmlDocumentAsync(Stream inputStream, TextWriter writer)
    {
        var css = GetCss();

        // Write HTML skeleton header
        await writer.WriteLineAsync("<!DOCTYPE html>");
        await writer.WriteLineAsync("<html lang=\"en\">");
        await writer.WriteLineAsync("<head>");
        await writer.WriteLineAsync("  <meta charset=\"UTF-8\" />");
        await writer.WriteLineAsync("  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\" />");
        await writer.WriteLineAsync("  <title>Converted Markdown</title>");
        await writer.WriteLineAsync("  <style>");
        await writer.WriteLineAsync(css);
        await writer.WriteLineAsync("  </style>");
        await writer.WriteLineAsync("  <script>");
        await writer.WriteLineAsync("    function copyCode(button) {");
        await writer.WriteLineAsync("      var container = button.closest('.code-block-container');");
        await writer.WriteLineAsync("      var code = container.querySelector('code');");
        await writer.WriteLineAsync("      var text = code.textContent || code.innerText;");
        await writer.WriteLineAsync("      navigator.clipboard.writeText(text.trim()).then(function() {");
        await writer.WriteLineAsync("        button.textContent = 'Copied!';");
        await writer.WriteLineAsync("        button.classList.add('code-block-copied');");
        await writer.WriteLineAsync("        setTimeout(function() {");
        await writer.WriteLineAsync("          button.textContent = 'Copy';");
        await writer.WriteLineAsync("          button.classList.remove('code-block-copied');");
        await writer.WriteLineAsync("        }, 1500);");
        await writer.WriteLineAsync("      });");
        await writer.WriteLineAsync("    }");
        await writer.WriteLineAsync("  </script>");
        await writer.WriteLineAsync("</head>");
        await writer.WriteLineAsync("<body>");

        // Process markdown through the fragment writer
        await WriteHtmlAsync(inputStream, writer);

        // Close HTML skeleton
        await writer.WriteLineAsync("</body>");
        await writer.WriteLineAsync("</html>");
    }
}