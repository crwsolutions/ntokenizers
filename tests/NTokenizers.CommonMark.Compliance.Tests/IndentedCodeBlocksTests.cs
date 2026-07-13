using NTokenizers.ToHtml;

namespace NTokenizers.CommonMark.Compliance.Tests;

/// <summary>
/// CommonMark spec 0.31.2 compliance tests for Indented code blocks.
/// Source: https://spec.commonmark.org/0.31.2/#indented-code-blocks
/// Total examples: 12
/// </summary>
public class IndentedCodeBlocksTests
{
    [Fact]
    public void Example_107()
    {
        var input = "    a simple\n      indented code block";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<pre><code>a simple\n  indented code block\n</code></pre>", html);
    }

    [Fact]
    public void Example_108()
    {
        var input = "  - foo\n\n    bar";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<ul>\n<li>\n<p>foo</p>\n<p>bar</p>\n</li>\n</ul>", html);
    }

    [Fact]
    public void Example_109()
    {
        var input = "1.  foo\n\n    - bar";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<ol>\n<li>\n<p>foo</p>\n<ul>\n<li>bar</li>\n</ul>\n</li>\n</ol>", html);
    }

    [Fact]
    public void Example_110()
    {
        var input = "    <a/>\n    *hi*\n\n    - one";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<pre><code>&lt;a/&gt;\n*hi*\n\n- one\n</code></pre>", html);
    }

    [Fact]
    public void Example_111()
    {
        var input = "    chunk1\n\n    chunk2\n  \n \n \n    chunk3";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<pre><code>chunk1\n\nchunk2\n\n\n\nchunk3\n</code></pre>", html);
    }

    [Fact]
    public void Example_112()
    {
        var input = "    chunk1\n      \n      chunk2";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<pre><code>chunk1\n  \n  chunk2\n</code></pre>", html);
    }

    [Fact]
    public void Example_113()
    {
        var input = "Foo\n    bar\n";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>Foo\nbar</p>", html);
    }

    [Fact]
    public void Example_114()
    {
        var input = "    foo\nbar";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<pre><code>foo\n</code></pre>\n<p>bar</p>", html);
    }

    [Fact]
    public void Example_115()
    {
        var input = "# Heading\n    foo\nHeading\n------\n    foo\n----";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<h1>Heading</h1>\n<pre><code>foo\n</code></pre>\n<h2>Heading</h2>\n<pre><code>foo\n</code></pre>\n<hr />", html);
    }

    [Fact]
    public void Example_116()
    {
        var input = "        foo\n    bar";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<pre><code>    foo\nbar\n</code></pre>", html);
    }

    [Fact]
    public void Example_117()
    {
        var input = "\n    \n    foo\n    \n";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<pre><code>foo\n</code></pre>", html);
    }

    [Fact]
    public void Example_118()
    {
        var input = "    foo  ";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<pre><code>foo  \n</code></pre>", html);
    }

}
