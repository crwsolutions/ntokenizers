using NTokenizers.ToHtml;
using System.Text;

namespace ToHtml;

public class MarkdownToHtmlTests
{
    // --- GetCss ---

    [Fact]
    public void GetCss_ReturnsNonEmptyCss()
    {
        var css = MarkdownConverter.GetCss();
        Assert.NotEmpty(css);
    }

    [Fact]
    public void GetCss_ContainsExpectedSelectors()
    {
        var css = MarkdownConverter.GetCss();
        Assert.Contains(".tok-", css);
    }

    // --- ToHtml (fragment, string input) ---

    [Fact]
    public void ToHtml_ReturnsHtmlFragmentWithoutWrapper()
    {
        var html = MarkdownConverter.ToHtml("# Hello");
        Assert.DoesNotContain("<!DOCTYPE html>", html);
        Assert.DoesNotContain("<html", html);
        Assert.DoesNotContain("<head>", html);
        Assert.DoesNotContain("<body>", html);
        Assert.Contains("<h1", html);
    }

    [Fact]
    public void ToHtml_RendersHeading()
    {
        var html = MarkdownConverter.ToHtml("# Hello");
        Assert.Contains("<h1", html);
    }

    [Fact]
    public void ToHtml_RendersBold()
    {
        var html = MarkdownConverter.ToHtml("**bold**");
        Assert.Contains("<strong>", html);
    }

    [Fact]
    public void ToHtml_RendersCodeBlock()
    {
        var input = "```csharp\nvar x = 1;\n```\n";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Contains("<pre", html);
        Assert.Contains("code-block-container", html);
        Assert.Contains("var", html);
    }

    // --- ToHtmlAsync (fragment, stream input) ---

    [Fact]
    public async Task ToHtmlAsync_ReturnsHtmlFragmentWithoutWrapper()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("# Hello"));
        var html = await MarkdownConverter.ToHtmlAsync(stream);
        Assert.DoesNotContain("<!DOCTYPE html>", html);
        Assert.DoesNotContain("<html", html);
        Assert.Contains("<h1", html);
    }

    // --- WriteHtmlFragmentAsync (fragment, writer output) ---

    [Fact]
    public async Task WriteHtmlFragmentAsync_WritesFragmentWithoutWrapper()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("# Hello"));
        using var writer = new StringWriter();
        await MarkdownConverter.WriteHtmlAsync(stream, writer);
        var html = writer.ToString();
        Assert.DoesNotContain("<!DOCTYPE html>", html);
        Assert.DoesNotContain("<html", html);
        Assert.Contains("<h1", html);
    }

    // --- ToHtmlDocument (full document, string input) ---

    [Fact]
    public void ToHtmlDocument_ReturnsFullHtmlDocument()
    {
        var html = MarkdownConverter.ToHtmlDocument("# Hello");
        Assert.Contains("<!DOCTYPE html>", html);
        Assert.Contains("<html", html);
        Assert.Contains("<head>", html);
        Assert.Contains("<body>", html);
        Assert.Contains("</html>", html);
        Assert.Contains("<h1", html);
    }

    [Fact]
    public void ToHtmlDocument_IncludesCss()
    {
        var html = MarkdownConverter.ToHtmlDocument("# Hello");
        Assert.Contains("<style>", html);
        Assert.Contains(".tok-", html);
    }

    [Fact]
    public void ToHtmlDocument_IncludesCopyScript()
    {
        var html = MarkdownConverter.ToHtmlDocument("# Hello");
        Assert.Contains("<script>", html);
        Assert.Contains("copyCode", html);
    }

    // --- ToHtmlDocumentAsync (full document, stream input) ---

    [Fact]
    public async Task ToHtmlDocumentAsync_ReturnsFullHtmlDocument()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("# Hello"));
        var html = await MarkdownConverter.ToHtmlDocumentAsync(stream);
        Assert.Contains("<!DOCTYPE html>", html);
        Assert.Contains("<html", html);
        Assert.Contains("<body>", html);
        Assert.Contains("</html>", html);
        Assert.Contains("<h1", html);
    }

    // --- WriteHtmlAsync (full document, writer output) ---

    [Fact]
    public async Task WriteHtmlAsync_WritesFullHtmlDocument()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("# Hello"));
        using var writer = new StringWriter();
        await MarkdownConverter.WriteHtmlDocumentAsync(stream, writer);
        var html = writer.ToString();
        Assert.Contains("<!DOCTYPE html>", html);
        Assert.Contains("<html", html);
        Assert.Contains("<body>", html);
        Assert.Contains("</html>", html);
        Assert.Contains("<h1", html);
    }

    // --- Consistency: fragment + css vs document ---

    [Fact]
    public void FragmentPlusCss_ConsistentWithDocument()
    {
        var input = "## Title\n\nSome **bold** text.\n";
        var fragment = MarkdownConverter.ToHtml(input);
        var css = MarkdownConverter.GetCss();
        var document = MarkdownConverter.ToHtmlDocument(input);

        // The fragment should appear inside the document body
        Assert.Contains(fragment.Trim(), document);
        // The CSS should appear inside the document style tag
        Assert.Contains(css.Trim(), document);
    }

    // --- Complex multiline ---

    [Fact]
    public void ToHtml_RendersComplexMarkdown()
    {
        var input = @"# Title

Some text with **bold** and *italic*.

- item 1
- item 2

```csharp
var x = 1;
var y = 2;
```

> A blockquote
";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Contains("<h1", html);
        Assert.Contains("<strong>", html);
        Assert.Contains("<em>", html);
        Assert.Contains("<li>", html);
        Assert.Contains("<pre", html);
        Assert.Contains("var", html);
        Assert.Contains("x", html);
        Assert.Contains("<blockquote>", html);
    }

    [Fact]
    public void ToHtmlDocument_RendersComplexMarkdown()
    {
        var input = @"# Title

Some text with **bold** and *italic*.

- item 1
- item 2

```csharp
var x = 1;
var y = 2;
```

> A blockquote
";
        var html = MarkdownConverter.ToHtmlDocument(input);
        Assert.Contains("<!DOCTYPE html>", html);
        Assert.Contains("<h1", html);
        Assert.Contains("<strong>", html);
        Assert.Contains("<em>", html);
        Assert.Contains("<li>", html);
        Assert.Contains("<pre", html);
        Assert.Contains("var", html);
        Assert.Contains("x", html);
        Assert.Contains("<blockquote>", html);
        Assert.Contains("</html>", html);
    }
}