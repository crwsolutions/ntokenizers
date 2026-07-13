using NTokenizers.ToHtml;

namespace NTokenizers.CommonMark.Compliance.Tests;

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
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<ol>\n<li>\n<p>A paragraph\nwith two lines.</p>\n<pre><code>indented code\n</code></pre>\n<blockquote>\n<p>A block quote.</p>\n</blockquote>\n</li>\n</ol>", html);
    }

    [Fact]
    public void Example_254()
    {
        var input = "- one\n\n two";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<ul>\n<li>one</li>\n</ul>\n<p>two</p>", html);
    }

    [Fact]
    public void Example_255()
    {
        var input = "- one\n\n  two";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<ul>\n<li>\n<p>one</p>\n<p>two</p>\n</li>\n</ul>", html);
    }

    [Fact]
    public void Example_256()
    {
        var input = " -    one\n\n     two";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<ul>\n<li>one</li>\n</ul>\n<pre><code> two\n</code></pre>", html);
    }

    [Fact]
    public void Example_257()
    {
        var input = " -    one\n\n      two";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<ul>\n<li>\n<p>one</p>\n<p>two</p>\n</li>\n</ul>", html);
    }

    [Fact]
    public void Example_258()
    {
        var input = "   > > 1.  one\n>>\n>>     two";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<blockquote>\n<blockquote>\n<ol>\n<li>\n<p>one</p>\n<p>two</p>\n</li>\n</ol>\n</blockquote>\n</blockquote>", html);
    }

    [Fact]
    public void Example_259()
    {
        var input = ">>- one\n>>\n  >  > two";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<blockquote>\n<blockquote>\n<ul>\n<li>one</li>\n</ul>\n<p>two</p>\n</blockquote>\n</blockquote>", html);
    }

    [Fact]
    public void Example_260()
    {
        var input = "-one\n\n2.two";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>-one</p>\n<p>2.two</p>", html);
    }

    [Fact]
    public void Example_261()
    {
        var input = "- foo\n\n\n  bar";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<ul>\n<li>\n<p>foo</p>\n<p>bar</p>\n</li>\n</ul>", html);
    }

    [Fact]
    public void Example_262()
    {
        var input = "1.  foo\n\n    ```\n    bar\n    ```\n\n    baz\n\n    > bam";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<ol>\n<li>\n<p>foo</p>\n<pre><code>bar\n</code></pre>\n<p>baz</p>\n<blockquote>\n<p>bam</p>\n</blockquote>\n</li>\n</ol>", html);
    }

    [Fact]
    public void Example_263()
    {
        var input = "- Foo\n\n      bar\n\n\n      baz";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<ul>\n<li>\n<p>Foo</p>\n<pre><code>bar\n\n\nbaz\n</code></pre>\n</li>\n</ul>", html);
    }

    [Fact]
    public void Example_264()
    {
        var input = "123456789. ok";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<ol start=\"123456789\">\n<li>ok</li>\n</ol>", html);
    }

    [Fact]
    public void Example_265()
    {
        var input = "1234567890. not ok";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>1234567890. not ok</p>", html);
    }

    [Fact]
    public void Example_266()
    {
        var input = "0. ok";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<ol start=\"0\">\n<li>ok</li>\n</ol>", html);
    }

    [Fact]
    public void Example_267()
    {
        var input = "003. ok";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<ol start=\"3\">\n<li>ok</li>\n</ol>", html);
    }

    [Fact]
    public void Example_268()
    {
        var input = "-1. not ok";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>-1. not ok</p>", html);
    }

    [Fact]
    public void Example_269()
    {
        var input = "- foo\n\n      bar";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<ul>\n<li>\n<p>foo</p>\n<pre><code>bar\n</code></pre>\n</li>\n</ul>", html);
    }

    [Fact]
    public void Example_270()
    {
        var input = "  10.  foo\n\n           bar";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<ol start=\"10\">\n<li>\n<p>foo</p>\n<pre><code>bar\n</code></pre>\n</li>\n</ol>", html);
    }

    [Fact]
    public void Example_271()
    {
        var input = "    indented code\n\nparagraph\n\n    more code";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<pre><code>indented code\n</code></pre>\n<p>paragraph</p>\n<pre><code>more code\n</code></pre>", html);
    }

    [Fact]
    public void Example_272()
    {
        var input = "1.     indented code\n\n   paragraph\n\n       more code";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<ol>\n<li>\n<pre><code>indented code\n</code></pre>\n<p>paragraph</p>\n<pre><code>more code\n</code></pre>\n</li>\n</ol>", html);
    }

    [Fact]
    public void Example_273()
    {
        var input = "1.      indented code\n\n   paragraph\n\n       more code";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<ol>\n<li>\n<pre><code> indented code\n</code></pre>\n<p>paragraph</p>\n<pre><code>more code\n</code></pre>\n</li>\n</ol>", html);
    }

    [Fact]
    public void Example_274()
    {
        var input = "   foo\n\nbar";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>foo</p>\n<p>bar</p>", html);
    }

    [Fact]
    public void Example_275()
    {
        var input = "-    foo\n\n  bar";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<ul>\n<li>foo</li>\n</ul>\n<p>bar</p>", html);
    }

    [Fact]
    public void Example_276()
    {
        var input = "-  foo\n\n   bar";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<ul>\n<li>\n<p>foo</p>\n<p>bar</p>\n</li>\n</ul>", html);
    }

    [Fact]
    public void Example_277()
    {
        var input = "-\n  foo\n-\n  ```\n  bar\n  ```\n-\n      baz";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<ul>\n<li>foo</li>\n<li>\n<pre><code>bar\n</code></pre>\n</li>\n<li>\n<pre><code>baz\n</code></pre>\n</li>\n</ul>", html);
    }

    [Fact]
    public void Example_278()
    {
        var input = "-   \n  foo";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<ul>\n<li>foo</li>\n</ul>", html);
    }

    [Fact]
    public void Example_279()
    {
        var input = "-\n\n  foo";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<ul>\n<li></li>\n</ul>\n<p>foo</p>", html);
    }

    [Fact]
    public void Example_280()
    {
        var input = "- foo\n-\n- bar";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<ul>\n<li>foo</li>\n<li></li>\n<li>bar</li>\n</ul>", html);
    }

    [Fact]
    public void Example_281()
    {
        var input = "- foo\n-   \n- bar";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<ul>\n<li>foo</li>\n<li></li>\n<li>bar</li>\n</ul>", html);
    }

    [Fact]
    public void Example_282()
    {
        var input = "1. foo\n2.\n3. bar";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<ol>\n<li>foo</li>\n<li></li>\n<li>bar</li>\n</ol>", html);
    }

    [Fact]
    public void Example_283()
    {
        var input = "*";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<ul>\n<li></li>\n</ul>", html);
    }

    [Fact]
    public void Example_284()
    {
        var input = "foo\n*\n\nfoo\n1.";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>foo\n*</p>\n<p>foo\n1.</p>", html);
    }

    [Fact]
    public void Example_285()
    {
        var input = " 1.  A paragraph\n     with two lines.\n\n         indented code\n\n     > A block quote.";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<ol>\n<li>\n<p>A paragraph\nwith two lines.</p>\n<pre><code>indented code\n</code></pre>\n<blockquote>\n<p>A block quote.</p>\n</blockquote>\n</li>\n</ol>", html);
    }

    [Fact]
    public void Example_286()
    {
        var input = "  1.  A paragraph\n      with two lines.\n\n          indented code\n\n      > A block quote.";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<ol>\n<li>\n<p>A paragraph\nwith two lines.</p>\n<pre><code>indented code\n</code></pre>\n<blockquote>\n<p>A block quote.</p>\n</blockquote>\n</li>\n</ol>", html);
    }

    [Fact]
    public void Example_287()
    {
        var input = "   1.  A paragraph\n       with two lines.\n\n           indented code\n\n       > A block quote.";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<ol>\n<li>\n<p>A paragraph\nwith two lines.</p>\n<pre><code>indented code\n</code></pre>\n<blockquote>\n<p>A block quote.</p>\n</blockquote>\n</li>\n</ol>", html);
    }

    [Fact]
    public void Example_288()
    {
        var input = "    1.  A paragraph\n        with two lines.\n\n            indented code\n\n        > A block quote.";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<pre><code>1.  A paragraph\n    with two lines.\n\n        indented code\n\n    &gt; A block quote.\n</code></pre>", html);
    }

    [Fact]
    public void Example_289()
    {
        var input = "  1.  A paragraph\nwith two lines.\n\n          indented code\n\n      > A block quote.";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<ol>\n<li>\n<p>A paragraph\nwith two lines.</p>\n<pre><code>indented code\n</code></pre>\n<blockquote>\n<p>A block quote.</p>\n</blockquote>\n</li>\n</ol>", html);
    }

    [Fact]
    public void Example_290()
    {
        var input = "  1.  A paragraph\n    with two lines.";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<ol>\n<li>A paragraph\nwith two lines.</li>\n</ol>", html);
    }

    [Fact]
    public void Example_291()
    {
        var input = "> 1. > Blockquote\ncontinued here.";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<blockquote>\n<ol>\n<li>\n<blockquote>\n<p>Blockquote\ncontinued here.</p>\n</blockquote>\n</li>\n</ol>\n</blockquote>", html);
    }

    [Fact]
    public void Example_292()
    {
        var input = "> 1. > Blockquote\n> continued here.";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<blockquote>\n<ol>\n<li>\n<blockquote>\n<p>Blockquote\ncontinued here.</p>\n</blockquote>\n</li>\n</ol>\n</blockquote>", html);
    }

    [Fact]
    public void Example_293()
    {
        var input = "- foo\n  - bar\n    - baz\n      - boo";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<ul>\n<li>foo\n<ul>\n<li>bar\n<ul>\n<li>baz\n<ul>\n<li>boo</li>\n</ul>\n</li>\n</ul>\n</li>\n</ul>\n</li>\n</ul>", html);
    }

    [Fact]
    public void Example_294()
    {
        var input = "- foo\n - bar\n  - baz\n   - boo";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<ul>\n<li>foo</li>\n<li>bar</li>\n<li>baz</li>\n<li>boo</li>\n</ul>", html);
    }

    [Fact]
    public void Example_295()
    {
        var input = "10) foo\n    - bar";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<ol start=\"10\">\n<li>foo\n<ul>\n<li>bar</li>\n</ul>\n</li>\n</ol>", html);
    }

    [Fact]
    public void Example_296()
    {
        var input = "10) foo\n   - bar";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<ol start=\"10\">\n<li>foo</li>\n</ol>\n<ul>\n<li>bar</li>\n</ul>", html);
    }

    [Fact]
    public void Example_297()
    {
        var input = "- - foo";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<ul>\n<li>\n<ul>\n<li>foo</li>\n</ul>\n</li>\n</ul>", html);
    }

    [Fact]
    public void Example_298()
    {
        var input = "1. - 2. foo";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<ol>\n<li>\n<ul>\n<li>\n<ol start=\"2\">\n<li>foo</li>\n</ol>\n</li>\n</ul>\n</li>\n</ol>", html);
    }

    [Fact]
    public void Example_299()
    {
        var input = "- # Foo\n- Bar\n  ---\n  baz";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<ul>\n<li>\n<h1>Foo</h1>\n</li>\n<li>\n<h2>Bar</h2>\nbaz</li>\n</ul>", html);
    }

    [Fact]
    public void Example_300()
    {
        var input = "- foo\n- bar\n+ baz";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<ul>\n<li>foo</li>\n<li>bar</li>\n</ul>\n<ul>\n<li>baz</li>\n</ul>", html);
    }

}
