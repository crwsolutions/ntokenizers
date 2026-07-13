using NTokenizers.ToHtml;

namespace NTokenizers.CommonMark.Compliance.Tests;

/// <summary>
/// CommonMark spec 0.31.2 compliance tests for Block quotes.
/// Source: https://spec.commonmark.org/0.31.2/#block-quotes
/// Total examples: 25
/// </summary>
public class BlockQuotesTests
{
    [Fact]
    public void Example_228()
    {
        var input = "># Foo\n>bar\n> baz";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<blockquote>\n<h1>Foo</h1>\n<p>bar\nbaz</p>\n</blockquote>", html);
    }

    [Fact]
    public void Example_229()
    {
        var input = "   > # Foo\n   > bar\n > baz";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<blockquote>\n<h1>Foo</h1>\n<p>bar\nbaz</p>\n</blockquote>", html);
    }

    [Fact]
    public void Example_230()
    {
        var input = "    > # Foo\n    > bar\n    > baz";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<pre><code>&gt; # Foo\n&gt; bar\n&gt; baz\n</code></pre>", html);
    }

    [Fact]
    public void Example_231()
    {
        var input = "> # Foo\n> bar\nbaz";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<blockquote>\n<h1>Foo</h1>\n<p>bar\nbaz</p>\n</blockquote>", html);
    }

    [Fact]
    public void Example_232()
    {
        var input = "> bar\nbaz\n> foo";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<blockquote>\n<p>bar\nbaz\nfoo</p>\n</blockquote>", html);
    }

    [Fact]
    public void Example_233()
    {
        var input = "> foo\n---";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<blockquote>\n<p>foo</p>\n</blockquote>\n<hr />", html);
    }

    [Fact]
    public void Example_234()
    {
        var input = "> - foo\n- bar";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<blockquote>\n<ul>\n<li>foo</li>\n</ul>\n</blockquote>\n<ul>\n<li>bar</li>\n</ul>", html);
    }

    [Fact]
    public void Example_235()
    {
        var input = ">     foo\n    bar";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<blockquote>\n<pre><code>foo\n</code></pre>\n</blockquote>\n<pre><code>bar\n</code></pre>", html);
    }

    [Fact]
    public void Example_236()
    {
        var input = "> ```\nfoo\n```";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<blockquote>\n<pre><code></code></pre>\n</blockquote>\n<p>foo</p>\n<pre><code></code></pre>", html);
    }

    [Fact]
    public void Example_237()
    {
        var input = "> foo\n    - bar";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<blockquote>\n<p>foo\n- bar</p>\n</blockquote>", html);
    }

    [Fact]
    public void Example_238()
    {
        var input = ">";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<blockquote>\n</blockquote>", html);
    }

    [Fact]
    public void Example_239()
    {
        var input = ">\n>  \n> ";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<blockquote>\n</blockquote>", html);
    }

    [Fact]
    public void Example_240()
    {
        var input = ">\n> foo\n>  ";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<blockquote>\n<p>foo</p>\n</blockquote>", html);
    }

    [Fact]
    public void Example_241()
    {
        var input = "> foo\n\n> bar";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<blockquote>\n<p>foo</p>\n</blockquote>\n<blockquote>\n<p>bar</p>\n</blockquote>", html);
    }

    [Fact]
    public void Example_242()
    {
        var input = "> foo\n> bar";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<blockquote>\n<p>foo\nbar</p>\n</blockquote>", html);
    }

    [Fact]
    public void Example_243()
    {
        var input = "> foo\n>\n> bar";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<blockquote>\n<p>foo</p>\n<p>bar</p>\n</blockquote>", html);
    }

    [Fact]
    public void Example_244()
    {
        var input = "foo\n> bar";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>foo</p>\n<blockquote>\n<p>bar</p>\n</blockquote>", html);
    }

    [Fact]
    public void Example_245()
    {
        var input = "> aaa\n***\n> bbb";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<blockquote>\n<p>aaa</p>\n</blockquote>\n<hr />\n<blockquote>\n<p>bbb</p>\n</blockquote>", html);
    }

    [Fact]
    public void Example_246()
    {
        var input = "> bar\nbaz";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<blockquote>\n<p>bar\nbaz</p>\n</blockquote>", html);
    }

    [Fact]
    public void Example_247()
    {
        var input = "> bar\n\nbaz";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<blockquote>\n<p>bar</p>\n</blockquote>\n<p>baz</p>", html);
    }

    [Fact]
    public void Example_248()
    {
        var input = "> bar\n>\nbaz";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<blockquote>\n<p>bar</p>\n</blockquote>\n<p>baz</p>", html);
    }

    [Fact]
    public void Example_249()
    {
        var input = "> > > foo\nbar";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<blockquote>\n<blockquote>\n<blockquote>\n<p>foo\nbar</p>\n</blockquote>\n</blockquote>\n</blockquote>", html);
    }

    [Fact]
    public void Example_250()
    {
        var input = ">>> foo\n> bar\n>>baz";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<blockquote>\n<blockquote>\n<blockquote>\n<p>foo\nbar\nbaz</p>\n</blockquote>\n</blockquote>\n</blockquote>", html);
    }

    [Fact]
    public void Example_251()
    {
        var input = ">     code\n\n>    not code";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<blockquote>\n<pre><code>code\n</code></pre>\n</blockquote>\n<blockquote>\n<p>not code</p>\n</blockquote>", html);
    }

    [Fact]
    public void Example_252()
    {
        var input = "A paragraph\nwith two lines.\n\n    indented code\n\n> A block quote.";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>A paragraph\nwith two lines.</p>\n<pre><code>indented code\n</code></pre>\n<blockquote>\n<p>A block quote.</p>\n</blockquote>", html);
    }

}
