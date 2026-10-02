using NTokenizers.ToHtml;

namespace NTokenizers.Tests.ToHtml;

/// <summary>
/// CommonMark spec 0.31.2 compliance tests for Lists.
/// Source: https://spec.commonmark.org/0.31.2/#lists
/// Total examples: 27 (Examples 321 and 322 expectations unchanged)
/// </summary>
public class ListsTests
{
    [Fact]
    public void Example_301()
    {
        var input = "1. foo\n2. bar\n3) baz";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<ol>\n<li>\n<p>foo</p>\n</li>\n<li>\n<p>bar</p>\n</li>\n</ol><ol>\n<li>\n<p>baz</p>\n</li>\n</ol>", html);
    }

    [Fact]
    public void Example_302()
    {
        var input = "Foo\n- bar\n- baz";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>Foo</p>\n<ul>\n<li>\n<p>bar</p>\n</li>\n<li>\n<p>baz</p>\n</li>\n</ul>", html);
    }

    [Fact]
    public void Example_303()
    {
        var input = "The number of windows in my house is\n14.  The number of doors is 6.";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>The number of windows in my house is<br/>14.  The number of doors is 6.</p>", html);
    }

    [Fact]
    public void Example_304()
    {
        var input = "The number of windows in my house is\n1.  The number of doors is 6.";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>The number of windows in my house is</p>\n<ol>\n<li>\n<p> The number of doors is 6.</p>\n</li>\n</ol>", html);
    }

    [Fact]
    public void Example_305()
    {
        var input = "- foo\n\n- bar\n\n\n- baz";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<ul>\n<li>\n<p>foo</p>\n</li>\n<li>\n<p>bar</p>\n</li>\n<li>\n<p>baz</p>\n</li>\n</ul>", html);
    }

    [Fact]
    public void Example_306()
    {
        var input = "- foo\n  - bar\n    - baz\n\n\n      bim";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<ul>\n<li>\n<p>foo</p>\n<ul>\n<li>\n<p>bar</p>\n<ul>\n<li>\n<p>baz</p>\n<p>bim</p>\n</li>\n</ul>\n</li>\n</ul>\n</li>\n</ul>", html);
    }

    [Fact]
    public void Example_307()
    {
        var input = "- foo\n- bar\n\n<!-- -->\n\n- baz\n- bim";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<ul>\n<li>\n<p>foo</p>\n</li>\n<li>\n<p>bar</p>\n</li>\n</ul><p>&lt;!-- --&gt;</p>\n<ul>\n<li>\n<p>baz</p>\n</li>\n<li>\n<p>bim</p>\n</li>\n</ul>", html);
    }

    [Fact]
    public void Example_308()
    {
        var input = "-   foo\n\n    notcode\n\n-   foo\n\n<!-- -->\n\n    code";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<ul>\n<li>\n<p>  foo</p>\n<p>  notcode</p>\n</li>\n<li>\n<p>  foo</p>\n</li>\n</ul><p>&lt;!-- --&gt;</p>\n<pre><code>code</code></pre>", html);
    }

    [Fact]
    public void Example_309()
    {
        var input = "- a\n - b\n  - c\n   - d\n  - e\n - f\n- g";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<ul>\n<li>\n<p>a</p>\n</li>\n<li>\n<p>b</p>\n</li>\n<li>\n<p>c</p>\n</li>\n<li>\n<p>d</p>\n</li>\n<li>\n<p>e</p>\n</li>\n<li>\n<p>f</p>\n</li>\n<li>\n<p>g</p>\n</li>\n</ul>", html);
    }

    [Fact]
    public void Example_310()
    {
        var input = "1. a\n\n  2. b\n\n   3. c";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<ol>\n<li>\n<p>a</p>\n</li>\n<li>\n<p>b</p>\n</li>\n<li>\n<p>c</p>\n</li>\n</ol>", html);
    }

    [Fact]
    public void Example_311()
    {
        var input = "- a\n - b\n  - c\n   - d\n    - e";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<ul>\n<li>\n<p>a</p>\n</li>\n<li>\n<p>b</p>\n</li>\n<li>\n<p>c</p>\n</li>\n<li>\n<p>d<br/>    - e</p>\n</li>\n</ul>", html);
    }

    [Fact]
    public void Example_312()
    {
        var input = "1. a\n\n  2. b\n\n    3. c";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<ol>\n<li>\n<p>a</p>\n</li>\n<li>\n<p>b<br/>    3. c</p>\n</li>\n</ol>", html);
    }

    [Fact]
    public void Example_313()
    {
        var input = "- a\n- b\n\n- c";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<ul>\n<li>\n<p>a</p>\n</li>\n<li>\n<p>b</p>\n</li>\n<li>\n<p>c</p>\n</li>\n</ul>", html);
    }

    [Fact]
    public void Example_314()
    {
        var input = "* a\n*\n\n* c";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<ul>\n<li>\n<p>a</p>\n</li>\n<li></li>\n<li>\n<p>c</p>\n</li>\n</ul>", html);
    }

    [Fact]
    public void Example_315()
    {
        var input = "- a\n- b\n\n  c\n- d";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<ul>\n<li>\n<p>a</p>\n</li>\n<li>\n<p>b</p>\n<p>c</p>\n</li>\n<li>\n<p>d</p>\n</li>\n</ul>", html);
    }

    [Fact]
    public void Example_316()
    {
        var input = "- a\n- b\n\n  [ref]: /url\n- d";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<ul>\n<li>\n<p>a</p>\n</li>\n<li>\n<p>b</p>\n<p>[ref]: /url</p>\n</li>\n<li>\n<p>d</p>\n</li>\n</ul>", html);
    }

    [Fact]
    public void Example_317()
    {
        var input = "- a\n- ```\n  b\n\n\n  ```\n- c";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<ul>\n<li>\n<p>a</p>\n</li>\n<li>\n<div class=\"code-block-container\">\n<div class=\"code-block-header\">\n<span class=\"code-block-language\"></span>\n<button class=\"code-block-copy\" onclick=\"copyCode(this)\" title=\"Copy to clipboard\">Copy</button>\n</div>\n<pre><code class=\"language-\"><span class=\"tok-generic\">b\n\n</span></code></pre>\n</div>\n</li>\n<li>\n<p>c</p>\n</li>\n</ul>", html);
    }

    [Fact]
    public void Example_318()
    {
        var input = "- a\n  - b\n\n    c\n- d";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<ul>\n<li>\n<p>a</p>\n<ul>\n<li>\n<p>b</p>\n<p>c</p>\n</li>\n</ul>\n</li>\n<li>\n<p>d</p>\n</li>\n</ul>", html);
    }

    [Fact]
    public void Example_319()
    {
        var input = "* a\n  > b\n  >\n* c";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // The second and third lines (two leading spaces before the '>') belong to the
        // outer list-item quote, so 'b' closes the quote and '* c' starts a new list item.
        Assert.Equal("<ul>\n<li>\n<p>a</p>\n<blockquote>\n<p>b</p>\n</blockquote>\n</li>\n<li>\n<p>c</p>\n</li>\n</ul>", html);
    }

    [Fact]
    public void Example_320()
    {
        var input = "- a\n  > b\n  ```\n  c\n  ```\n- d";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal(
            "<ul>\n<li>\n<p>a</p>\n<blockquote>\n<p>b</p>\n</blockquote>\n<div class=\"code-block-container\">\n" +
            "<div class=\"code-block-header\">\n" +
            "<span class=\"code-block-language\"></span>\n" +
            "<button class=\"code-block-copy\" onclick=\"copyCode(this)\" title=\"Copy to clipboard\">Copy</button>\n" +
            "</div>\n" +
            "<pre><code class=\"language-\"><span class=\"tok-generic\">c</span></code></pre>\n" +
            "</div>\n</li>\n<li>\n<p>d</p>\n</li>\n</ul>",
            html);
    }

    [Fact]
    public void Example_321()
    {
        var input = "- a";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<ul>\n<li>\n<p>a</p>\n</li>\n</ul>", html);
    }

    [Fact]
    public void Example_322()
    {
        var input = "- a\n  - b";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<ul>\n<li>\n<p>a</p>\n<ul>\n<li>\n<p>b</p>\n</li>\n</ul>\n</li>\n</ul>", html);
    }

    [Fact]
    public void Example_323()
    {
        var input = "1. ```\n   foo\n   ```\n\n   bar";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<ol>\n<li>\n<div class=\"code-block-container\">\n<div class=\"code-block-header\">\n<span class=\"code-block-language\"></span>\n<button class=\"code-block-copy\" onclick=\"copyCode(this)\" title=\"Copy to clipboard\">Copy</button>\n</div>\n<pre><code class=\"language-\"><span class=\"tok-generic\">foo</span></code></pre>\n</div>\n<p>bar</p>\n</li>\n</ol>", html);
    }

    [Fact]
    public void Example_324()
    {
        var input = "* foo\n  * bar\n\n  baz";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<ul>\n<li>\n<p>foo</p>\n<ul>\n<li>\n<p>bar</p>\n</li>\n</ul><p>baz</p>\n</li>\n</ul>", html);
    }

    [Fact]
    public void Example_325()
    {
        var input = "- a\n  - b\n  - c\n\n- d\n  - e\n  - f";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<ul>\n<li>\n<p>a</p>\n<ul>\n<li>\n<p>b</p>\n</li>\n<li>\n<p>c</p>\n</li>\n</ul>\n</li>\n<li>\n<p>d</p>\n<ul>\n<li>\n<p>e</p>\n</li>\n<li>\n<p>f</p>\n</li>\n</ul>\n</li>\n</ul>", html);
    }

    [Fact]
    public void Example_326()
    {
        var input = "`hi`lo`";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><code>hi</code>lo`</p>", html);
    }

    [Fact]
    public void Example_327()
    {
        var input = "`foo`";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><code>foo</code></p>", html);
    }

}
