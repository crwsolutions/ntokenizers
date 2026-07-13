using NTokenizers.ToHtml;

namespace NTokenizers.CommonMark.Compliance.Tests;

/// <summary>
/// CommonMark spec 0.31.2 compliance tests for Tabs.
/// Source: https://spec.commonmark.org/0.31.2/#tabs
/// Total examples: 11
/// </summary>
public class TabsTests
{
    [Fact]
    public void Example_001()
    {
        var input = "\tfoo\tbaz\t\tbim";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<pre><code>foo\tbaz\t\tbim\n</code></pre>", html);
    }

    [Fact]
    public void Example_002()
    {
        var input = "  \tfoo\tbaz\t\tbim";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<pre><code>foo\tbaz\t\tbim\n</code></pre>", html);
    }

    [Fact]
    public void Example_003()
    {
        var input = "    a\ta\n    ὐ\ta";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<pre><code>a\ta\nὐ\ta\n</code></pre>", html);
    }

    [Fact]
    public void Example_004()
    {
        var input = "  - foo\n\n\tbar";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<ul>\n<li>\n<p>foo</p>\n<p>bar</p>\n</li>\n</ul>", html);
    }

    [Fact]
    public void Example_005()
    {
        var input = "- foo\n\n\t\tbar";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<ul>\n<li>\n<p>foo</p>\n<pre><code>  bar\n</code></pre>\n</li>\n</ul>", html);
    }

    [Fact]
    public void Example_006()
    {
        var input = ">\t\tfoo";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<blockquote>\n<pre><code>  foo\n</code></pre>\n</blockquote>", html);
    }

    [Fact]
    public void Example_007()
    {
        var input = "-\t\tfoo";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<ul>\n<li>\n<pre><code>  foo\n</code></pre>\n</li>\n</ul>", html);
    }

    [Fact]
    public void Example_008()
    {
        var input = "    foo\n\tbar";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<pre><code>foo\nbar\n</code></pre>", html);
    }

    [Fact]
    public void Example_009()
    {
        var input = " - foo\n   - bar\n\t - baz";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<ul>\n<li>foo\n<ul>\n<li>bar\n<ul>\n<li>baz</li>\n</ul>\n</li>\n</ul>\n</li>\n</ul>", html);
    }

    [Fact]
    public void Example_010()
    {
        var input = "#\tFoo";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<h1>Foo</h1>", html);
    }

    [Fact]
    public void Example_011()
    {
        var input = "*\t*\t*\t";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<hr />", html);
    }

}
