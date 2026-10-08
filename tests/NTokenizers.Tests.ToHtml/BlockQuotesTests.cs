using NTokenizers.ToHtml;

namespace NTokenizers.Tests.ToHtml;

/// <summary>
/// Blockquote rendering tests for <see cref="MarkdownConverter.ToHtml"/>. The blockquote
/// token streams its content as a full markdown sub-document, so the quote content is
/// rendered exactly like top-level markdown. Where the streaming grammar deviates from the
/// CommonMark spec 0.31.2 reference output, the expectation follows the faithful output of
/// the actual token stream and the deviation is documented in a comment.
/// Source (spec reference): https://spec.commonmark.org/0.31.2/#block-quotes
/// </summary>
public class BlockQuotesTests
{
    [Fact]
    public void Example_228()
    {
        var input = "># Foo\n>bar\n> baz";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<blockquote>\n<h1>Foo</h1>\n<p>bar<br/>baz</p>\n</blockquote>", html);
    }

    [Fact]
    public void Example_229()
    {
        var input = "   > # Foo\n   > bar\n > baz";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Matches the CommonMark spec: up to three leading spaces before the '>' are part
        // of the blockquote marker, so all three lines belong to one blockquote.
        Assert.Equal("<blockquote>\n<h1>Foo</h1>\n<p>bar<br/>baz</p>\n</blockquote>", html);
    }

    [Fact]
    public void Example_230()
    {
        var input = "    > # Foo\n    > bar\n    > baz";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: four leading columns make this a top-level indented code block (unchanged
        // behavior); the block ends at end of stream, so there is no trailing newline.
        Assert.Equal("<pre><code>&gt; # Foo\n&gt; bar\n&gt; baz</code></pre>", html);
    }

    [Fact]
    public void Example_231()
    {
        var input = "> # Foo\n> bar\nbaz";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<blockquote>\n<h1>Foo</h1>\n<p>bar<br/>baz</p>\n</blockquote>", html);
    }

    [Fact]
    public void Example_232()
    {
        var input = "> bar\nbaz\n> foo";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Lazy continuation: the unquoted lines stay inside the quote's paragraph.
        Assert.Equal("<blockquote>\n<p>bar<br/>baz<br/>foo</p>\n</blockquote>", html);
    }

    [Fact]
    public void Example_233()
    {
        var input = "> foo\n---";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<blockquote>\n<p>foo</p>\n</blockquote>\n<hr />", html);
    }

    [Fact]
    public void Example_234()
    {
        var input = "> - foo\n- bar";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // The list writer emits </li>\n</ul> with a baked-in newline, so there is no extra
        // newline before the first </blockquote>.
        Assert.Equal(
            "<blockquote>\n<ul>\n<li>foo</li>\n</ul></blockquote>\n" +
            "<ul>\n<li>\n<p>bar</p>\n</li>\n</ul>",
            html);
    }

    [Fact]
    public void Example_235()
    {
        var input = ">     foo\n    bar";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: one indented code block inside the quote holds both lines (the
        // code content tokenizer reads the raw stream and knows no quote prefix).
        Assert.Equal("<blockquote>\n<pre><code>foo\nbar</code></pre>\n</blockquote>", html);
    }

    [Fact]
    public void Example_236()
    {
        var input = "> ```\nfoo\n```";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: one code block inside the quote holds the content (the fence content
        // reads the raw stream until the bare ``` line or end of stream).
        Assert.Equal(
            "<blockquote>\n" +
            "<div class=\"code-block-container\">\n" +
            "<div class=\"code-block-header\">\n" +
            "<span class=\"code-block-language\"></span>\n" +
            "<button class=\"code-block-copy\" onclick=\"copyCode(this)\" title=\"Copy to clipboard\">Copy</button>\n" +
            "</div>\n" +
            "<pre><code class=\"language-\"><span class=\"tok-generic\">foo</span></code></pre>\n" +
            "</div>\n" +
            "</blockquote>",
            html);
    }

    [Fact]
    public void Example_237()
    {
        var input = "> foo\n    - bar";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Lazy continuation while a paragraph is open: the four-space-indented line is part
        // of the paragraph, with its indentation preserved.
        Assert.Equal("<blockquote>\n<p>foo<br/>    - bar</p>\n</blockquote>", html);
    }

    [Fact]
    public void Example_238()
    {
        var input = ">";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<blockquote>\n</blockquote>", html);
    }

    [Fact]
    public void Example_239()
    {
        var input = ">\n>  \n> ";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // No content: the whitespace-only quoted lines produce no paragraph.
        Assert.Equal("<blockquote>\n</blockquote>", html);
    }

    [Fact]
    public void Example_240()
    {
        var input = ">\n> foo\n>  ";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Trailing whitespace of the quoted line is preserved in the paragraph.
        Assert.Equal("<blockquote>\n<p>foo </p>\n</blockquote>", html);
    }

    [Fact]
    public void Example_241()
    {
        var input = "> foo\n\n> bar";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // A blank line (no prefix) ends the quote; the next quoted line starts a new one.
        Assert.Equal(
            "<blockquote>\n<p>foo</p>\n</blockquote>\n" +
            "<blockquote>\n<p>bar</p>\n</blockquote>",
            html);
    }

    [Fact]
    public void Example_242()
    {
        var input = "> foo\n> bar";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Soft line break inside the quoted paragraph renders as <br/>, like top level.
        Assert.Equal("<blockquote>\n<p>foo<br/>bar</p>\n</blockquote>", html);
    }

    [Fact]
    public void Example_243()
    {
        var input = "> foo\n>\n> bar";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // A blank quoted line (prefix only) separates paragraphs inside the quote.
        Assert.Equal("<blockquote>\n<p>foo</p>\n<p>bar</p>\n</blockquote>", html);
    }

    [Fact]
    public void Example_244()
    {
        var input = "foo\n> bar";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>foo</p>\n<blockquote>\n<p>bar</p>\n</blockquote>", html);
    }

    [Fact]
    public void Example_245()
    {
        var input = "> aaa\n***\n> bbb";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal(
            "<blockquote>\n<p>aaa</p>\n</blockquote>\n" +
            "<hr />\n" +
            "<blockquote>\n<p>bbb</p>\n</blockquote>",
            html);
    }

    [Fact]
    public void Example_246()
    {
        var input = "> bar\nbaz";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<blockquote>\n<p>bar<br/>baz</p>\n</blockquote>", html);
    }

    [Fact]
    public void Example_247()
    {
        var input = "> bar\n\nbaz";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<blockquote>\n<p>bar</p>\n</blockquote>\n<p>baz</p>", html);
    }

    [Fact]
    public void Example_248()
    {
        var input = "> bar\n>\nbaz";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // The blank quoted line closes the paragraph; 'baz' (no prefix, no open paragraph)
        // ends the quote and belongs to the outer scope.
        Assert.Equal("<blockquote>\n<p>bar</p>\n</blockquote>\n<p>baz</p>", html);
    }

    [Fact]
    public void Example_249()
    {
        var input = "> > > foo\nbar";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal(
            "<blockquote>\n<blockquote>\n<blockquote>\n" +
            "<p>foo<br/>bar</p>\n" +
            "</blockquote>\n</blockquote>\n</blockquote>",
            html);
    }

    [Fact]
    public void Example_250()
    {
        var input = ">>> foo\n> bar\n>>baz";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Spec-conform (CommonMark example 250): the three leading '>' markers open three
        // blockquotes, and the later lines with fewer markers are lazy continuation of the
        // innermost paragraph.
        Assert.Equal(
            "<blockquote>\n<blockquote>\n<blockquote>\n" +
            "<p>foo<br/>bar<br/>baz</p>\n" +
            "</blockquote>\n</blockquote>\n</blockquote>",
            html);
    }

    [Fact]
    public void Example_251()
    {
        var input = ">     code\n\n>    not code";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: after the indented code block and a blank line, the following line
        // opens a nested quote (its indentation beyond the prefix is preserved).
        Assert.Equal(
            "<blockquote>\n<pre><code>code\n</code></pre>\n" +
            "<blockquote>\n<p>   not code</p>\n</blockquote>\n" +
            "</blockquote>",
            html);
    }

    [Fact]
    public void Example_252()
    {
        var input = "A paragraph\nwith two lines.\n\n    indented code\n\n> A block quote.";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal(
            "<p>A paragraph<br/>with two lines.</p>\n" +
            "<pre><code>indented code\n</code></pre>\n" +
            "<blockquote>\n<p>A block quote.</p>\n</blockquote>",
            html);
    }

}
