using NTokenizers.ToHtml;

namespace NTokenizers.Tests.ToHtml;

/// <summary>
/// CommonMark spec 0.31.2 compliance tests for Code spans.
/// Source: https://spec.commonmark.org/0.31.2/#code-spans
/// Total examples: 22
/// </summary>
public class CodeSpansTests
{

    [Fact]
    public void Example_328()
    {
        var input = "`foo`";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><code>foo</code></p>", html);
    }

    [Fact]
    public void Example_329()
    {
        var input = "`` foo ` bar ``";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Whitespace is preserved (deviation from CommonMark), so the inner single backtick is
        // content and the surrounding spaces are kept.
        Assert.Equal("<p><code> foo ` bar </code></p>", html);
    }

    [Fact]
    public void Example_330()
    {
        var input = "`  ``  `";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Whitespace is preserved (deviation from CommonMark), and the inner double-backtick
        // run is content, not a closing delimiter.
        Assert.Equal("<p><code>  ``  </code></p>", html);
    }

    [Fact]
    public void Example_331()
    {
        var input = "` a`";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><code> a</code></p>", html);
    }

    [Fact]
    public void Example_332()
    {
        var input = "` b `";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><code> b </code></p>", html);
    }

    [Fact]
    public void Example_333()
    {
        var input = "` `\n`  `";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><code> </code><br/><code>  </code></p>", html);
    }

    [Fact]
    public void Example_334()
    {
        var input = "``\nfoo\nbar  \nbaz\n``";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Whitespace and line breaks are preserved (deviation from CommonMark), so the raw
        // multi-line content is kept as-is inside the code span.
        Assert.Equal("<p><code>\nfoo\nbar  \nbaz\n</code></p>", html);
    }

    [Fact]
    public void Example_335()
    {
        var input = "``\nfoo \n``";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><code>\nfoo \n</code></p>", html);
    }

    [Fact]
    public void Example_336()
    {
        var input = "`foo   bar \nbaz`";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><code>foo   bar \nbaz</code></p>", html);
    }

    [Fact]
    public void Example_337()
    {
        var input = "`foo\\`bar`";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><code>foo\\</code>bar`</p>", html);
    }

    [Fact]
    public void Example_338()
    {
        var input = "``foo`bar``";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><code>foo`bar</code></p>", html);
    }

    [Fact]
    public void Example_339()
    {
        var input = "` foo `` bar `";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><code> foo `` bar </code></p>", html);
    }

    [Fact]
    public void Example_340()
    {
        var input = "*foo`*`";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><em>foo`</em>`</p>", html);
    }

    [Fact]
    public void Example_341()
    {
        var input = "[not a `link](/foo`)";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><a href=\"/foo`\">not a `link</a></p>", html);
    }

    [Fact]
    public void Example_342()
    {
        var input = "`<a href=\"`\">`";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><code>&lt;a href=&quot;</code>&quot;&gt;`</p>", html);
    }

    [Fact]
    public void Example_343()
    {
        var input = "<a href=\"`\">`";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><a href=\"`\">`</p>", html);
    }

    [Fact]
    public void Example_344()
    {
        var input = "`<https://foo.bar.`baz>`";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><code>&lt;https://foo.bar.</code>baz&gt;`</p>", html);
    }

    [Fact]
    public void Example_345()
    {
        var input = "<https://foo.bar.`baz>`";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><https://foo.bar.`baz>`</p>", html);
    }

    [Fact]
    public void Example_346()
    {
        var input = "```foo``";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<div class=\"code-block-container\">\n<div class=\"code-block-header\">\n<span class=\"code-block-language\">foo``</span>\n<button class=\"code-block-copy\" onclick=\"copyCode(this)\" title=\"Copy to clipboard\">Copy</button>\n</div>\n<pre><code class=\"language-foo``\"></code></pre>\n</div>\n", html);
    }

    [Fact]
    public void Example_347()
    {
        var input = "`foo";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>`foo</p>", html);
    }

    [Fact]
    public void Example_348()
    {
        var input = "`foo``bar``";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>`foo``bar``</p>", html);
    }
}
