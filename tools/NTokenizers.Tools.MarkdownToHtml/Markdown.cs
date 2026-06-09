using NTokenizers.Markdown;
using NTokenizers.Tools.MarkdownToHtml.Writers;
using System.Text;

namespace NTokenizers.Tools.MarkdownToHtml;

/// <summary>
/// Renders a Markdown input stream as a complete HTML document to the provided writer.
/// </summary>
public sealed class Markdown
{
    /// <summary>
    /// Renders the Markdown from the input stream as a styled HTML document.
    /// </summary>
    /// <param name="inputStream">The stream containing the Markdown content.</param>
    /// <param name="writer">The writer that receives the HTML output.</param>
    public static async Task ToHtmlAsync(Stream inputStream, TextWriter writer)
    {
        var markdownWriter = new MarkdownHtmlWriter();
        var bob = new StringBuilder();
        markdownWriter.WriteAdditionalCss(bob);

        // Write HTML skeleton header
        await writer.WriteLineAsync("<!DOCTYPE html>");
        await writer.WriteLineAsync("<html lang=\"en\">");
        await writer.WriteLineAsync("<head>");
        await writer.WriteLineAsync("  <meta charset=\"UTF-8\" />");
        await writer.WriteLineAsync("  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\" />");
        await writer.WriteLineAsync("  <title>Converted Markdown</title>");
        await writer.WriteLineAsync("  <style>");
        await writer.WriteLineAsync(bob.ToString());
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

        // Process markdown through the tokenizer stream
        await MarkdownTokenizer.Create().ParseAsync(inputStream, onToken: async token =>
        {
            await markdownWriter.WriteTokenAsync(token, writer);
        });

        // Close HTML skeleton
        await writer.WriteLineAsync("</body>");
        await writer.WriteLineAsync("</html>");
    }
}