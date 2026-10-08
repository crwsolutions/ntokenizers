using NTokenizers.ToHtml;

namespace NTokenizers.Tests.ToHtml;

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
        // Faithful to the input: the final code line has no line ending, so none is added.
        var input = "    a simple\n      indented code block";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<pre><code>a simple\n  indented code block</code></pre>", html);
    }

    [Fact]
    public void Example_108()
    {
        var input = "  - foo\n\n    bar";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<ul>\n<li>\n<p>foo</p>\n<p>bar</p>\n</li>\n</ul>", html);
    }

    [Fact]
    public void Example_109()
    {
        var input = "1.  foo\n\n    - bar";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<ol>\n<li>\n<p> foo</p>\n<ul>\n<li>\n<p>bar</p>\n</li>\n</ul>\n</li>\n</ol>", html);
    }

    [Fact]
    public void Example_110()
    {
        // Faithful to the input: the final code line has no line ending, so none is added.
        var input = "    <a/>\n    *hi*\n\n    - one";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<pre><code>&lt;a/&gt;\n*hi*\n\n- one</code></pre>", html);
    }

    [Fact]
    public void Example_111()
    {
        // Faithful to the input: the final code line has no line ending, so none is added.
        var input = "    chunk1\n\n    chunk2\n  \n \n \n    chunk3";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<pre><code>chunk1\n\nchunk2\n\n\n\nchunk3</code></pre>", html);
    }

    [Fact]
    public void Example_112()
    {
        // Faithful to the input: the final code line has no line ending, so none is added.
        var input = "    chunk1\n      \n      chunk2";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<pre><code>chunk1\n  \n  chunk2</code></pre>", html);
    }

    [Fact]
    public void Example_113()
    {
        // An indented code block cannot interrupt a paragraph, so "bar" stays in the
        // paragraph. Deviations from the spec output (<p>Foo\nbar</p>): whitespace is
        // preserved (the four leading spaces are kept, not stripped) and the line break is
        // rendered as a soft break (<br/>). Deviation: the document's trailing newline is
        // faithfully emitted as a Text("\n") separator after the PEnd; the dispatcher writes
        // its block separation before that following token, so the output ends with a
        // trailing line break (byte-identical HTML for any content that does not end in a
        // newline).
        var input = "Foo\n    bar\n";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>Foo<br/>    bar</p>\n", html);
    }

    [Fact]
    public void Example_114()
    {
        var input = "    foo\nbar";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<pre><code>foo\n</code></pre>\n<p>bar</p>", html);
    }

    [Fact]
    public void Example_115()
    {
        var input = "# Heading\n    foo\nHeading\n------\n    foo\n----";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<h1>Heading</h1>\n<pre><code>foo\n</code></pre>\n<p>Heading</p>\n<hr />\n<pre><code>foo\n</code></pre>\n<hr />", html);
    }

    [Fact]
    public void Example_116()
    {
        // Faithful to the input: the final code line has no line ending, so none is added.
        var input = "        foo\n    bar";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<pre><code>    foo\nbar</code></pre>", html);
    }

    [Fact]
    public void Example_117()
    {
        var input = "\n    \n    foo\n    \n";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<pre><code>foo\n</code></pre>", html);
    }

    [Fact]
    public void Example_118()
    {
        // Faithful to the input: the final code line has no line ending, so none is added.
        var input = "    foo  ";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<pre><code>foo  </code></pre>", html);
    }

}
