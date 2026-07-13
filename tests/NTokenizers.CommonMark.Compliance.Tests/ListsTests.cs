using NTokenizers.ToHtml;

namespace NTokenizers.CommonMark.Compliance.Tests;

/// <summary>
/// CommonMark spec 0.31.2 compliance tests for Lists.
/// Source: https://spec.commonmark.org/0.31.2/#lists
/// Total examples: 27
/// </summary>
public class ListsTests
{
    [Fact]
    public void Example_301()
    {
        var input = "1. foo\n2. bar\n3) baz";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<ol>\n<li>foo</li>\n<li>bar</li>\n</ol>\n<ol start=\"3\">\n<li>baz</li>\n</ol>", html);
    }

    [Fact]
    public void Example_302()
    {
        var input = "Foo\n- bar\n- baz";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>Foo</p>\n<ul>\n<li>bar</li>\n<li>baz</li>\n</ul>", html);
    }

    [Fact]
    public void Example_303()
    {
        var input = "The number of windows in my house is\n14.  The number of doors is 6.";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>The number of windows in my house is\n14.  The number of doors is 6.</p>", html);
    }

    [Fact]
    public void Example_304()
    {
        var input = "The number of windows in my house is\n1.  The number of doors is 6.";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>The number of windows in my house is</p>\n<ol>\n<li>The number of doors is 6.</li>\n</ol>", html);
    }

    [Fact]
    public void Example_305()
    {
        var input = "- foo\n\n- bar\n\n\n- baz";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<ul>\n<li>\n<p>foo</p>\n</li>\n<li>\n<p>bar</p>\n</li>\n<li>\n<p>baz</p>\n</li>\n</ul>", html);
    }

    [Fact]
    public void Example_306()
    {
        var input = "- foo\n  - bar\n    - baz\n\n\n      bim";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<ul>\n<li>foo\n<ul>\n<li>bar\n<ul>\n<li>\n<p>baz</p>\n<p>bim</p>\n</li>\n</ul>\n</li>\n</ul>\n</li>\n</ul>", html);
    }

    [Fact]
    public void Example_307()
    {
        var input = "- foo\n- bar\n\n<!-- -->\n\n- baz\n- bim";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<ul>\n<li>foo</li>\n<li>bar</li>\n</ul>\n<!-- -->\n<ul>\n<li>baz</li>\n<li>bim</li>\n</ul>", html);
    }

    [Fact]
    public void Example_308()
    {
        var input = "-   foo\n\n    notcode\n\n-   foo\n\n<!-- -->\n\n    code";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<ul>\n<li>\n<p>foo</p>\n<p>notcode</p>\n</li>\n<li>\n<p>foo</p>\n</li>\n</ul>\n<!-- -->\n<pre><code>code\n</code></pre>", html);
    }

    [Fact]
    public void Example_309()
    {
        var input = "- a\n - b\n  - c\n   - d\n  - e\n - f\n- g";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<ul>\n<li>a</li>\n<li>b</li>\n<li>c</li>\n<li>d</li>\n<li>e</li>\n<li>f</li>\n<li>g</li>\n</ul>", html);
    }

    [Fact]
    public void Example_310()
    {
        var input = "1. a\n\n  2. b\n\n   3. c";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<ol>\n<li>\n<p>a</p>\n</li>\n<li>\n<p>b</p>\n</li>\n<li>\n<p>c</p>\n</li>\n</ol>", html);
    }

    [Fact]
    public void Example_311()
    {
        var input = "- a\n - b\n  - c\n   - d\n    - e";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<ul>\n<li>a</li>\n<li>b</li>\n<li>c</li>\n<li>d\n- e</li>\n</ul>", html);
    }

    [Fact]
    public void Example_312()
    {
        var input = "1. a\n\n  2. b\n\n    3. c";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<ol>\n<li>\n<p>a</p>\n</li>\n<li>\n<p>b</p>\n</li>\n</ol>\n<pre><code>3. c\n</code></pre>", html);
    }

    [Fact]
    public void Example_313()
    {
        var input = "- a\n- b\n\n- c";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<ul>\n<li>\n<p>a</p>\n</li>\n<li>\n<p>b</p>\n</li>\n<li>\n<p>c</p>\n</li>\n</ul>", html);
    }

    [Fact]
    public void Example_314()
    {
        var input = "* a\n*\n\n* c";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<ul>\n<li>\n<p>a</p>\n</li>\n<li></li>\n<li>\n<p>c</p>\n</li>\n</ul>", html);
    }

    [Fact]
    public void Example_315()
    {
        var input = "- a\n- b\n\n  c\n- d";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<ul>\n<li>\n<p>a</p>\n</li>\n<li>\n<p>b</p>\n<p>c</p>\n</li>\n<li>\n<p>d</p>\n</li>\n</ul>", html);
    }

    [Fact]
    public void Example_316()
    {
        var input = "- a\n- b\n\n  [ref]: /url\n- d";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<ul>\n<li>\n<p>a</p>\n</li>\n<li>\n<p>b</p>\n</li>\n<li>\n<p>d</p>\n</li>\n</ul>", html);
    }

    [Fact]
    public void Example_317()
    {
        var input = "- a\n- ```\n  b\n\n\n  ```\n- c";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<ul>\n<li>a</li>\n<li>\n<pre><code>b\n\n\n</code></pre>\n</li>\n<li>c</li>\n</ul>", html);
    }

    [Fact]
    public void Example_318()
    {
        var input = "- a\n  - b\n\n    c\n- d";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<ul>\n<li>a\n<ul>\n<li>\n<p>b</p>\n<p>c</p>\n</li>\n</ul>\n</li>\n<li>d</li>\n</ul>", html);
    }

    [Fact]
    public void Example_319()
    {
        var input = "* a\n  > b\n  >\n* c";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<ul>\n<li>a\n<blockquote>\n<p>b</p>\n</blockquote>\n</li>\n<li>c</li>\n</ul>", html);
    }

    [Fact]
    public void Example_320()
    {
        var input = "- a\n  > b\n  ```\n  c\n  ```\n- d";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<ul>\n<li>a\n<blockquote>\n<p>b</p>\n</blockquote>\n<pre><code>c\n</code></pre>\n</li>\n<li>d</li>\n</ul>", html);
    }

    [Fact]
    public void Example_321()
    {
        var input = "- a";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<ul>\n<li>a</li>\n</ul>", html);
    }

    [Fact]
    public void Example_322()
    {
        var input = "- a\n  - b";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<ul>\n<li>a\n<ul>\n<li>b</li>\n</ul>\n</li>\n</ul>", html);
    }

    [Fact]
    public void Example_323()
    {
        var input = "1. ```\n   foo\n   ```\n\n   bar";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<ol>\n<li>\n<pre><code>foo\n</code></pre>\n<p>bar</p>\n</li>\n</ol>", html);
    }

    [Fact]
    public void Example_324()
    {
        var input = "* foo\n  * bar\n\n  baz";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<ul>\n<li>\n<p>foo</p>\n<ul>\n<li>bar</li>\n</ul>\n<p>baz</p>\n</li>\n</ul>", html);
    }

    [Fact]
    public void Example_325()
    {
        var input = "- a\n  - b\n  - c\n\n- d\n  - e\n  - f";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<ul>\n<li>\n<p>a</p>\n<ul>\n<li>b</li>\n<li>c</li>\n</ul>\n</li>\n<li>\n<p>d</p>\n<ul>\n<li>e</li>\n<li>f</li>\n</ul>\n</li>\n</ul>", html);
    }

    [Fact]
    public void Example_326()
    {
        var input = "`hi`lo`";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><code>hi</code>lo`</p>", html);
    }

    [Fact]
    public void Example_327()
    {
        var input = "`foo`";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><code>foo</code></p>", html);
    }

}
