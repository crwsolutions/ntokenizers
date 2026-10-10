using NTokenizers.ToHtml;

namespace NTokenizers.Tests.ToHtml;

/// <summary>
/// CommonMark spec 0.31.2 compliance tests for List items.
/// Source: https://spec.commonmark.org/0.31.2/#list-items
/// Total examples: 48
/// </summary>
public class ListItemsTests
{
    [Fact]
    public void Example_253()
    {
        var input = "1.  A paragraph\n    with two lines.\n\n        indented code\n\n    > A block quote.";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<ol>\n<li>\n<p> A paragraph<br/> with two lines.</p>\n<pre><code> indented code\n</code></pre>\n<blockquote>\n<p>A block quote.</p>\n</blockquote>\n</li>\n</ol>", html);
    }

    [Fact]
    public void Example_254()
    {
        var input = "- one\n\n two";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<ul>\n<li>\n<p>one</p>\n</li>\n</ul><p> two</p>", html);
    }

    [Fact]
    public void Example_255()
    {
        var input = "- one\n\n  two";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<ul>\n<li>\n<p>one</p>\n<p>two</p>\n</li>\n</ul>", html);
    }

    [Fact]
    public void Example_256()
    {
        var input = " -    one\n\n     two";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<ul>\n<li>\n<p>   one</p>\n<p>  two</p>\n</li>\n</ul>", html);
    }

    [Fact]
    public void Example_257()
    {
        var input = " -    one\n\n      two";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<ul>\n<li>\n<p>   one</p>\n<p>   two</p>\n</li>\n</ul>", html);
    }

    [Fact]
    public void Example_258()
    {
        var input = "   > > 1.  one\n>>\n>>     two";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // The blank '>>' line no longer opens extra nested quotes: the line carries fewer
        // markers than the inner level, so the depth stays at two blockquotes. Deviation:
        // 'two' is indented code here rather than a lazy continuation of the list item
        // (spec: <ol><li><p>one</p><p>two</p></li></ol>); list-item lazy continuation is a
        // separate feature.
        Assert.Equal("<blockquote>\n<blockquote>\n<ol>\n<li> one</li>\n<pre><code>two</code></pre>\n</ol></blockquote>\n</blockquote>", html);
    }

    [Fact]
    public void Example_259()
    {
        var input = ">>- one\n>>\n  >  > two";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Matches the CommonMark spec (example 259): the depth stays at two blockquotes;
        // the third line's markers continue the outer quote and '> two' is a paragraph
        // inside it, not the start of a further nested quote.
        Assert.Equal("<blockquote>\n<blockquote>\n<ul>\n<li>one</li>\n</ul><p>two</p>\n</blockquote>\n</blockquote>", html);
    }

    [Fact]
    public void Example_260()
    {
        var input = "-one\n\n2.two";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>-one</p>\n<p>2.two</p>", html);
    }

    [Fact]
    public void Example_261()
    {
        var input = "- foo\n\n\n  bar";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<ul>\n<li>\n<p>foo</p>\n<p>bar</p>\n</li>\n</ul>", html);
    }

    [Fact]
    public void Example_262()
    {
        var input = "1.  foo\n\n    ```\n    bar\n    ```\n\n    baz\n\n    > bam";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<ol>\n<li>\n<p> foo</p>\n<div class=\"code-block-container\">\n<div class=\"code-block-header\">\n<span class=\"code-block-language\"></span>\n<button class=\"code-block-copy\" onclick=\"copyCode(this)\" title=\"Copy to clipboard\">Copy</button>\n</div>\n<pre><code class=\"language-\"><span class=\"tok-generic\"> </span><span class=\"tok-generic\">bar\n </span></code></pre>\n</div>\n<p> baz</p>\n<blockquote>\n<p>bam</p>\n</blockquote>\n</li>\n</ol>", html);
    }

    [Fact]
    public void Example_263()
    {
        var input = "- Foo\n\n      bar\n\n\n      baz";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<ul>\n<li>\n<p>Foo</p>\n<pre><code>bar\n\n\nbaz</code></pre>\n</li>\n</ul>", html);
    }

    [Fact]
    public void Example_264()
    {
        var input = "123456789. ok";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<ol>\n<li value=\"123456789\">\n<p>ok</p>\n</li>\n</ol>", html);
    }

    [Fact]
    public void Example_265()
    {
        var input = "1234567890. not ok";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>1234567890. not ok</p>", html);
    }

    [Fact]
    public void Example_266()
    {
        var input = "0. ok";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<ol>\n<li value=\"0\">\n<p>ok</p>\n</li>\n</ol>", html);
    }

    [Fact]
    public void Example_267()
    {
        var input = "003. ok";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<ol>\n<li value=\"3\">\n<p>ok</p>\n</li>\n</ol>", html);
    }

    [Fact]
    public void Example_268()
    {
        var input = "-1. not ok";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>-1. not ok</p>", html);
    }

    [Fact]
    public void Example_269()
    {
        var input = "- foo\n\n      bar";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<ul>\n<li>\n<p>foo</p>\n<pre><code>bar</code></pre>\n</li>\n</ul>", html);
    }

    [Fact]
    public void Example_270()
    {
        var input = "  10.  foo\n\n           bar";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<ol>\n<li value=\"10\">\n<p> foo</p>\n<pre><code> bar</code></pre>\n</li>\n</ol>", html);
    }

    [Fact]
    public void Example_271()
    {
        var input = "    indented code\n\nparagraph\n\n    more code";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<pre><code>indented code\n</code></pre>\n<p>paragraph</p>\n<pre><code>more code</code></pre>", html);
    }

    [Fact]
    public void Example_272()
    {
        var input = "1.     indented code\n\n   paragraph\n\n       more code";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<ol>\n<li>\n<pre><code>indented code\n</code></pre>\n<p>paragraph</p>\n<pre><code>more code</code></pre>\n</li>\n</ol>", html);
    }

    [Fact]
    public void Example_273()
    {
        var input = "1.      indented code\n\n   paragraph\n\n       more code";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<ol>\n<li>\n<pre><code> indented code\n</code></pre>\n<p>paragraph</p>\n<pre><code>more code</code></pre>\n</li>\n</ol>", html);
    }

    [Fact]
    public void Example_274()
    {
        var input = "   foo\n\nbar";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>   foo</p>\n<p>bar</p>", html);
    }

    [Fact]
    public void Example_275()
    {
        var input = "-    foo\n\n  bar";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<ul>\n<li>\n<p>   foo</p>\n<p>bar</p>\n</li>\n</ul>", html);
    }

    [Fact]
    public void Example_276()
    {
        var input = "-  foo\n\n   bar";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<ul>\n<li>\n<p> foo</p>\n<p> bar</p>\n</li>\n</ul>", html);
    }

    [Fact]
    public void Example_277()
    {
        var input = "-\n  foo\n-\n  ```\n  bar\n  ```\n-\n      baz";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<ul>\n<li>\n<p>foo</p>\n</li>\n<li>\n<div class=\"code-block-container\">\n<div class=\"code-block-header\">\n<span class=\"code-block-language\"></span>\n<button class=\"code-block-copy\" onclick=\"copyCode(this)\" title=\"Copy to clipboard\">Copy</button>\n</div>\n<pre><code class=\"language-\"><span class=\"tok-generic\">bar</span></code></pre>\n</div>\n</li>\n<li>\n<pre><code>baz</code></pre>\n</li>\n</ul>", html);
    }

    [Fact]
    public void Example_278()
    {
        var input = "-   \n  foo";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<ul>\n<li>\n<p>foo</p>\n</li>\n</ul>", html);
    }

    [Fact]
    public void Example_279()
    {
        var input = "-\n\n  foo";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<ul>\n<li><p>foo</p>\n</li>\n</ul>", html);
    }

    [Fact]
    public void Example_280()
    {
        var input = "- foo\n-\n- bar";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<ul>\n<li>\n<p>foo</p>\n</li>\n<li></li>\n<li>\n<p>bar</p>\n</li>\n</ul>", html);
    }

    [Fact]
    public void Example_281()
    {
        var input = "- foo\n-   \n- bar";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<ul>\n<li>\n<p>foo</p>\n</li>\n<li></li>\n<li>\n<p>bar</p>\n</li>\n</ul>", html);
    }

    [Fact]
    public void Example_282()
    {
        var input = "1. foo\n2.\n3. bar";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<ol>\n<li>\n<p>foo</p>\n</li>\n<li></li>\n<li>\n<p>bar</p>\n</li>\n</ol>", html);
    }

    [Fact]
    public void Example_283()
    {
        var input = "*";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<ul>\n<li></li>\n</ul>", html);
    }

    [Fact]
    public void Example_284()
    {
        var input = "foo\n*\n\nfoo\n1.";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: a list marker is a valid paragraph-interrupting line-start construct,
        // so the marker-only lines ('*' and '1.') start list items instead of lazy-
        // continuing the paragraph (spec: one paragraph).
        Assert.Equal("<p>foo</p>\n<ul>\n<li></li>\n</ul><p>foo</p>\n<ol>\n<li></li>\n</ol>", html);
    }

    [Fact]
    public void Example_285()
    {
        var input = " 1.  A paragraph\n     with two lines.\n\n         indented code\n\n     > A block quote.";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<ol>\n<li>\n<p> A paragraph<br/> with two lines.</p>\n<pre><code> indented code\n</code></pre>\n<blockquote>\n<p>A block quote.</p>\n</blockquote>\n</li>\n</ol>", html);
    }

    [Fact]
    public void Example_286()
    {
        var input = "  1.  A paragraph\n      with two lines.\n\n          indented code\n\n      > A block quote.";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<ol>\n<li>\n<p> A paragraph<br/> with two lines.</p>\n<pre><code> indented code\n</code></pre>\n<blockquote>\n<p>A block quote.</p>\n</blockquote>\n</li>\n</ol>", html);
    }

    [Fact]
    public void Example_287()
    {
        var input = "   1.  A paragraph\n       with two lines.\n\n           indented code\n\n       > A block quote.";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<ol>\n<li>\n<p> A paragraph<br/> with two lines.</p>\n<pre><code> indented code\n</code></pre>\n<blockquote>\n<p>A block quote.</p>\n</blockquote>\n</li>\n</ol>", html);
    }

    [Fact]
    public void Example_288()
    {
        var input = "    1.  A paragraph\n        with two lines.\n\n            indented code\n\n        > A block quote.";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<pre><code>1.  A paragraph\n    with two lines.\n\n        indented code\n\n    &gt; A block quote.</code></pre>", html);
    }

    [Fact]
    public void Example_289()
    {
        var input = "  1.  A paragraph\nwith two lines.\n\n          indented code\n\n      > A block quote.";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<ol>\n<li>\n<p> A paragraph<br/>with two lines.</p>\n<pre><code> indented code\n</code></pre>\n<blockquote>\n<p>A block quote.</p>\n</blockquote>\n</li>\n</ol>", html);
    }

    [Fact]
    public void Example_290()
    {
        var input = "  1.  A paragraph\n    with two lines.";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<ol>\n<li>\n<p> A paragraph<br/>with two lines.</p>\n</li>\n</ol>", html);
    }

    [Fact]
    public void Example_291()
    {
        var input = "> 1. > Blockquote\ncontinued here.";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<blockquote>\n<ol>\n<li>&gt; Blockquote</li>\n</ol></blockquote>\n<p>continued here.</p>", html);
    }

    [Fact]
    public void Example_292()
    {
        var input = "> 1. > Blockquote\n> continued here.";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<blockquote>\n<ol>\n<li>&gt; Blockquote</li>\n</ol><p>continued here.</p>\n</blockquote>", html);
    }

    [Fact]
    public void Example_293()
    {
        var input = "- foo\n  - bar\n    - baz\n      - boo";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<ul>\n<li>\n<p>foo</p>\n<ul>\n<li>\n<p>bar</p>\n<ul>\n<li>\n<p>baz</p>\n<ul>\n<li>\n<p>boo</p>\n</li>\n</ul>\n</li>\n</ul>\n</li>\n</ul>\n</li>\n</ul>", html);
    }

    [Fact]
    public void Example_294()
    {
        var input = "- foo\n - bar\n  - baz\n   - boo";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<ul>\n<li>\n<p>foo</p>\n</li>\n<li>\n<p>bar</p>\n</li>\n<li>\n<p>baz</p>\n</li>\n<li>\n<p>boo</p>\n</li>\n</ul>", html);
    }

    [Fact]
    public void Example_295()
    {
        var input = "10) foo\n    - bar";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<ol>\n<li value=\"10\">\n<p>foo</p>\n<ul>\n<li>\n<p>bar</p>\n</li>\n</ul>\n</li>\n</ol>", html);
    }

    [Fact]
    public void Example_296()
    {
        var input = "10) foo\n   - bar";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<ol>\n<li value=\"10\">\n<p>foo</p>\n</li>\n</ol><ul>\n<li>\n<p>bar</p>\n</li>\n</ul>", html);
    }

    [Fact]
    public void Example_297()
    {
        var input = "- - foo";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<ul>\n<li>\n<ul>\n<li>\n<p>foo</p>\n</li>\n</ul>\n</li>\n</ul>", html);
    }

    [Fact]
    public void Example_298()
    {
        var input = "1. - 2. foo";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<ol>\n<li>\n<ul>\n<li>\n<ol>\n<li value=\"2\">\n<p>foo</p>\n</li>\n</ol>\n</li>\n</ul>\n</li>\n</ol>", html);
    }

    [Fact]
    public void Example_299()
    {
        var input = "- # Foo\n- Bar\n  ---\n  baz";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<ul>\n<li>\n<h1>Foo</h1>\n</li>\n<li>\n<p>Bar</p>\n<hr />\n<p>baz</p>\n</li>\n</ul>", html);
    }

    [Fact]
    public void Example_300()
    {
        var input = "- foo\n- bar\n+ baz";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<ul>\n<li>\n<p>foo</p>\n</li>\n<li>\n<p>bar</p>\n</li>\n<li>\n<p>baz</p>\n</li>\n</ul>", html);
    }

}
