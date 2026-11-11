using System.Text;
using NTokenizers.Markdown;
using NTokenizers.ToHtml.Writers;

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
    /// Converts a Markdown string to an HTML fragment (no document wrapper).
    /// </summary>
    public static Task<string> ToHtmlAsync(string input)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        return ToHtmlAsync(stream);
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
    /// The writer is only written to through <c>WriteAsync</c> and <c>FlushAsync</c>, so it can
    /// safely wrap a stream that disallows synchronous IO, such as a Kestrel response body.
    /// </summary>
    /// <returns>The consumed Markdown source text, as decoded from the stream (with any BOM detected and stripped).</returns>
    public static async Task<string> WriteHtmlAsync(Stream inputStream, TextWriter writer, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        using var reader = new StreamReader(inputStream);
        return await WriteHtmlAsync(reader, writer, ct);
    }

    /// <summary>
    /// Converts a Markdown text reader to an HTML fragment and writes it directly to the provided writer.
    /// The writer is only written to through <c>WriteAsync</c> and <c>FlushAsync</c> (in token order,
    /// with a single final flush), so it can safely wrap a stream that disallows synchronous IO,
    /// such as a Kestrel response body. A write failure (e.g. the client went away)
    /// propagates from the awaited token callback and ends the parse.
    /// </summary>
    /// <returns>The consumed Markdown source text, exactly as read from the reader.</returns>
    public static async Task<string> WriteHtmlAsync(TextReader reader, TextWriter writer, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var markdownWriter = new MarkdownHtmlWriter();

        // Each token is written to completion before the tokenizer emits the next one,
        // so the output cannot get ahead of the writer.
        var markdown = await MarkdownTokenizer.Create().ParseAsync(reader, new StringBuilder(), new Queue<char>(), ct, async token =>
        {
            await markdownWriter.WriteTokenAsync(token, writer, ct);
        });

        await writer.FlushAsync();
        return markdown;
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
    /// Converts a Markdown string to a complete HTML document (with DOCTYPE, head, body wrapper).
    /// </summary>
    public static Task<string> ToHtmlDocumentAsync(string input)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        return ToHtmlDocumentAsync(stream);
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
    /// <returns>The consumed Markdown source text, as decoded from the stream (with any BOM detected and stripped).</returns>
    public static async Task<string> WriteHtmlDocumentAsync(Stream inputStream, TextWriter writer, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        using var reader = new StreamReader(inputStream);
        return await WriteHtmlDocumentAsync(reader, writer, ct);
    }

    /// <summary>
    /// Converts a Markdown text reader to a complete HTML document and writes it directly to the provided writer.
    /// </summary>
    /// <returns>The consumed Markdown source text, exactly as read from the reader.</returns>
    public static async Task<string> WriteHtmlDocumentAsync(TextReader reader, TextWriter writer, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
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
        await writer.WriteLineAsync("<div class=\"markdown-document\">");

        // Process markdown through the fragment writer
        var markdown = await WriteHtmlAsync(reader, writer, ct);

        // Close HTML skeleton
        await writer.WriteLineAsync("</div>");
        await writer.WriteLineAsync("</body>");
        await writer.WriteLineAsync("</html>");

        return markdown;
    }
}