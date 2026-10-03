using NTokenizers.ToHtml;

namespace NTokenizers.Tests.ToHtml;

/// <summary>
/// Fenced code block rendering tests for <see cref="MarkdownConverter.ToHtml"/>. The
/// fenced code block token renders as a decorated container: a header (language label
/// and copy button) above a <c>&lt;pre&gt;&lt;code&gt;</c> element. Where the streaming
/// grammar deviates from the CommonMark spec 0.31.2 reference output, the expectation
/// follows the faithful output of the actual token stream and the deviation is
/// documented in a comment.
/// Source (spec reference): https://spec.commonmark.org/0.31.2/#fenced-code-blocks
/// </summary>
public class FencedCodeBlocksTests
{
    [Fact]
    public void Example_119()
    {
        var input = "```\n<\n >\n```";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: a fenced code block renders as the decorated container (header with
        // language label and copy button above pre/code) instead of a bare <pre><code>.
        // The content '<\n >' is generic code, so it is emitted as tok-generic spans.
        Assert.Equal(
            "<div class=\"code-block-container\">\n" +
            "<div class=\"code-block-header\">\n" +
            "<span class=\"code-block-language\"></span>\n" +
            "<button class=\"code-block-copy\" onclick=\"copyCode(this)\" title=\"Copy to clipboard\">Copy</button>\n" +
            "</div>\n" +
            "<pre><code class=\"language-\"><span class=\"tok-generic\">&lt;\n </span>" +
            "<span class=\"tok-generic\">&gt;</span></code></pre>\n" +
            "</div>\n",
            html);
    }

    [Fact]
    public void Example_120()
    {
        var input = "~~~\n<\n >\n~~~";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: '~~~' is not recognized as a fence; a line-start '~' parses as a
        // superscript marker, so the whole line is plain text with an empty superscript.
        Assert.Equal(
            "<p><sup><span class=\"tok-superscript\"></span></sup>~<br/>&lt;</p>\n" +
            "<blockquote>\n</blockquote>\n" +
            "<p><sup><span class=\"tok-superscript\"></span></sup>~</p>",
            html);
    }

    [Fact]
    public void Example_121()
    {
        var input = "``\nfoo\n``";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Matches the CommonMark spec: two backticks are not a fence, so the pair opens
        // an inline code span holding the lines between them.
        Assert.Equal("<p><code>\nfoo\n</code></p>", html);
    }

    [Fact]
    public void Example_122()
    {
        var input = "```\naaa\n~~~\n```";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Matches the CommonMark spec content: only the '```' fence closes the block, so
        // '~~~' stays as code content. Deviation: the decorated container wrapper.
        Assert.Equal(
            "<div class=\"code-block-container\">\n" +
            "<div class=\"code-block-header\">\n" +
            "<span class=\"code-block-language\"></span>\n" +
            "<button class=\"code-block-copy\" onclick=\"copyCode(this)\" title=\"Copy to clipboard\">Copy</button>\n" +
            "</div>\n" +
            "<pre><code class=\"language-\"><span class=\"tok-generic\">aaa\n~~~</span></code></pre>\n" +
            "</div>\n",
            html);
    }

    [Fact]
    public void Example_123()
    {
        var input = "~~~\naaa\n```\n~~~";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: '~~~' is not a fence, so the first line and 'aaa' form a paragraph.
        // The '```' line then opens a fenced code block whose content is the final '~~~'.
        Assert.Equal(
            "<p><sup><span class=\"tok-superscript\"></span></sup>~<br/>aaa" +
            "<div class=\"code-block-container\">\n" +
            "<div class=\"code-block-header\">\n" +
            "<span class=\"code-block-language\"></span>\n" +
            "<button class=\"code-block-copy\" onclick=\"copyCode(this)\" title=\"Copy to clipboard\">Copy</button>\n" +
            "</div>\n" +
            "<pre><code class=\"language-\"><span class=\"tok-generic\">~~~</span></code></pre>\n" +
            "</div>\n</p>",
            html);
    }

    [Fact]
    public void Example_124()
    {
        var input = "````\naaa\n```\n``````";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: the opening '````' is read as '```' plus a language of '`', so the
        // block contains 'aaa' and is closed by the bare '```' line. The final '``````'
        // line then opens a second (empty) code block whose language is '```'.
        Assert.Equal(
            "<div class=\"code-block-container\">\n" +
            "<div class=\"code-block-header\">\n" +
            "<span class=\"code-block-language\">`</span>\n" +
            "<button class=\"code-block-copy\" onclick=\"copyCode(this)\" title=\"Copy to clipboard\">Copy</button>\n" +
            "</div>\n" +
            "<pre><code class=\"language-`\"><span class=\"tok-generic\">aaa</span></code></pre>\n" +
            "</div>\n" +
            "<div class=\"code-block-container\">\n" +
            "<div class=\"code-block-header\">\n" +
            "<span class=\"code-block-language\">```</span>\n" +
            "<button class=\"code-block-copy\" onclick=\"copyCode(this)\" title=\"Copy to clipboard\">Copy</button>\n" +
            "</div>\n" +
            "<pre><code class=\"language-```\"></code></pre>\n" +
            "</div>\n",
            html);
    }

    [Fact]
    public void Example_125()
    {
        var input = "~~~~\naaa\n~~~\n~~~~";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: '~~~~' is not a fence; the runs of '~' parse as superscript markers,
        // so the whole block is a plain paragraph.
        Assert.Equal(
            "<p><sup><span class=\"tok-superscript\"></span></sup>" +
            "<sup><span class=\"tok-superscript\"></span></sup><br/>aaa<br/>" +
            "<sup><span class=\"tok-superscript\"></span></sup>~<br/>" +
            "<sup><span class=\"tok-superscript\"></span></sup>" +
            "<sup><span class=\"tok-superscript\"></span></sup></p>",
            html);
    }

    [Fact]
    public void Example_126()
    {
        var input = "```";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: a single '```' opens an empty code block (the content runs to end of
        // stream). Rendered as the decorated container with an empty pre/code.
        Assert.Equal(
            "<div class=\"code-block-container\">\n" +
            "<div class=\"code-block-header\">\n" +
            "<span class=\"code-block-language\"></span>\n" +
            "<button class=\"code-block-copy\" onclick=\"copyCode(this)\" title=\"Copy to clipboard\">Copy</button>\n" +
            "</div>\n" +
            "<pre><code class=\"language-\"></code></pre>\n" +
            "</div>\n",
            html);
    }

    [Fact]
    public void Example_127()
    {
        var input = "`````\n\n```\naaa";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: the opening '`````' is read as '```' plus a language of '``'. The
        // following blank line is consumed as the block's (empty) content line, and the
        // bare '```' line closes the block. The remaining 'aaa' forms a paragraph.
        Assert.Equal(
            "<div class=\"code-block-container\">\n" +
            "<div class=\"code-block-header\">\n" +
            "<span class=\"code-block-language\">``</span>\n" +
            "<button class=\"code-block-copy\" onclick=\"copyCode(this)\" title=\"Copy to clipboard\">Copy</button>\n" +
            "</div>\n" +
            "<pre><code class=\"language-``\"></code></pre>\n" +
            "</div>\n" +
            "<p>aaa</p>",
            html);
    }

    [Fact]
    public void Example_128()
    {
        var input = "> ```\n> aaa\n\nbbb";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: the fence content reads the raw stream until end of stream, so the
        // unquoted 'bbb' line leaks into the code content (and the blockquote is never
        // closed on its own, leaving 'bbb' inside the code).
        Assert.Equal(
            "<blockquote>\n" +
            "<div class=\"code-block-container\">\n" +
            "<div class=\"code-block-header\">\n" +
            "<span class=\"code-block-language\"></span>\n" +
            "<button class=\"code-block-copy\" onclick=\"copyCode(this)\" title=\"Copy to clipboard\">Copy</button>\n" +
            "</div>\n" +
            "<pre><code class=\"language-\"><span class=\"tok-generic\">aaa\n\nbbb</span></code></pre>\n" +
            "</div>\n" +
            "</blockquote>",
            html);
    }

    [Fact]
    public void Example_129()
    {
        var input = "```\n\n  \n```";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Matches the CommonMark spec content: the blank line and the two spaces are
        // code content. Deviation: the decorated container wrapper, and the two spaces
        // are emitted as a second tok-generic span.
        Assert.Equal(
            "<div class=\"code-block-container\">\n" +
            "<div class=\"code-block-header\">\n" +
            "<span class=\"code-block-language\"></span>\n" +
            "<button class=\"code-block-copy\" onclick=\"copyCode(this)\" title=\"Copy to clipboard\">Copy</button>\n" +
            "</div>\n" +
            "<pre><code class=\"language-\"><span class=\"tok-generic\">\n </span>" +
            "<span class=\"tok-generic\"> </span></code></pre>\n" +
            "</div>\n",
            html);
    }

    [Fact]
    public void Example_130()
    {
        var input = "```\n```";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Matches the CommonMark spec content: an empty code block. Deviation: the
        // decorated container wrapper.
        Assert.Equal(
            "<div class=\"code-block-container\">\n" +
            "<div class=\"code-block-header\">\n" +
            "<span class=\"code-block-language\"></span>\n" +
            "<button class=\"code-block-copy\" onclick=\"copyCode(this)\" title=\"Copy to clipboard\">Copy</button>\n" +
            "</div>\n" +
            "<pre><code class=\"language-\"></code></pre>\n" +
            "</div>\n",
            html);
    }

    [Fact]
    public void Example_131()
    {
        var input = " ```\n aaa\naaa\n```";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: up to three leading spaces before '```' still open the fence, but the
        // content keeps its full line indentation (the spec strips the fence indent), so
        // the content is ' aaa' / 'aaa' instead of 'aaa' / 'aaa'.
        Assert.Equal(
            "<div class=\"code-block-container\">\n" +
            "<div class=\"code-block-header\">\n" +
            "<span class=\"code-block-language\"></span>\n" +
            "<button class=\"code-block-copy\" onclick=\"copyCode(this)\" title=\"Copy to clipboard\">Copy</button>\n" +
            "</div>\n" +
            "<pre><code class=\"language-\"><span class=\"tok-generic\"> </span>" +
            "<span class=\"tok-generic\">aaa\naaa</span></code></pre>\n" +
            "</div>\n",
            html);
    }

    [Fact]
    public void Example_132()
    {
        var input = "  ```\naaa\n  aaa\naaa\n  ```";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: the content keeps its full line indentation (the spec strips the two
        // column fence indent), so the content is 'aaa' / '  aaa' / 'aaa' / '  '.
        Assert.Equal(
            "<div class=\"code-block-container\">\n" +
            "<div class=\"code-block-header\">\n" +
            "<span class=\"code-block-language\"></span>\n" +
            "<button class=\"code-block-copy\" onclick=\"copyCode(this)\" title=\"Copy to clipboard\">Copy</button>\n" +
            "</div>\n" +
            "<pre><code class=\"language-\"><span class=\"tok-generic\">aaa\n </span>" +
            "<span class=\"tok-generic\"> </span>" +
            "<span class=\"tok-generic\">aaa\naaa\n </span>" +
            "<span class=\"tok-generic\"> </span></code></pre>\n" +
            "</div>\n",
            html);
    }

    [Fact]
    public void Example_133()
    {
        var input = "   ```\n   aaa\n    aaa\n  aaa\n   ```";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: the content keeps its full line indentation (the spec strips the
        // three column fence indent), so the content is '   aaa' / '    aaa' / '  aaa' /
        // '   '.
        Assert.Equal(
            "<div class=\"code-block-container\">\n" +
            "<div class=\"code-block-header\">\n" +
            "<span class=\"code-block-language\"></span>\n" +
            "<button class=\"code-block-copy\" onclick=\"copyCode(this)\" title=\"Copy to clipboard\">Copy</button>\n" +
            "</div>\n" +
            "<pre><code class=\"language-\"><span class=\"tok-generic\"> </span>" +
            "<span class=\"tok-generic\"> </span>" +
            "<span class=\"tok-generic\"> </span>" +
            "<span class=\"tok-generic\">aaa\n </span>" +
            "<span class=\"tok-generic\"> </span>" +
            "<span class=\"tok-generic\"> </span>" +
            "<span class=\"tok-generic\"> </span>" +
            "<span class=\"tok-generic\">aaa\n </span>" +
            "<span class=\"tok-generic\"> </span>" +
            "<span class=\"tok-generic\">aaa\n </span>" +
            "<span class=\"tok-generic\"> </span>" +
            "<span class=\"tok-generic\"> </span></code></pre>\n" +
            "</div>\n",
            html);
    }

    [Fact]
    public void Example_134()
    {
        var input = "    ```\n    aaa\n    ```";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Matches the CommonMark spec: four leading columns make this an indented code
        // block (not a fence), so the '```' lines are literal content and the block ends
        // at end of stream (no trailing newline).
        Assert.Equal("<pre><code>```\naaa\n```</code></pre>", html);
    }

    [Fact]
    public void Example_135()
    {
        var input = "```\naaa\n  ```";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: the indented closing fence '  ```' is not recognized as a closing
        // fence (the spec allows up to three leading spaces), so it stays as code content
        // and the block runs to end of stream.
        Assert.Equal(
            "<div class=\"code-block-container\">\n" +
            "<div class=\"code-block-header\">\n" +
            "<span class=\"code-block-language\"></span>\n" +
            "<button class=\"code-block-copy\" onclick=\"copyCode(this)\" title=\"Copy to clipboard\">Copy</button>\n" +
            "</div>\n" +
            "<pre><code class=\"language-\"><span class=\"tok-generic\">aaa\n </span>" +
            "<span class=\"tok-generic\"> </span></code></pre>\n" +
            "</div>\n",
            html);
    }

    [Fact]
    public void Example_136()
    {
        var input = "   ```\naaa\n  ```";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: the indented closing fence '  ```' is not recognized as a closing
        // fence, so it stays as code content and the block runs to end of stream.
        Assert.Equal(
            "<div class=\"code-block-container\">\n" +
            "<div class=\"code-block-header\">\n" +
            "<span class=\"code-block-language\"></span>\n" +
            "<button class=\"code-block-copy\" onclick=\"copyCode(this)\" title=\"Copy to clipboard\">Copy</button>\n" +
            "</div>\n" +
            "<pre><code class=\"language-\"><span class=\"tok-generic\">aaa\n </span>" +
            "<span class=\"tok-generic\"> </span></code></pre>\n" +
            "</div>\n",
            html);
    }

    [Fact]
    public void Example_137()
    {
        var input = "```\naaa\n    ```";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: the indented closing fence '    ```' is not recognized as a closing
        // fence, so it stays as code content and the block runs to end of stream.
        Assert.Equal(
            "<div class=\"code-block-container\">\n" +
            "<div class=\"code-block-header\">\n" +
            "<span class=\"code-block-language\"></span>\n" +
            "<button class=\"code-block-copy\" onclick=\"copyCode(this)\" title=\"Copy to clipboard\">Copy</button>\n" +
            "</div>\n" +
            "<pre><code class=\"language-\"><span class=\"tok-generic\">aaa\n </span>" +
            "<span class=\"tok-generic\"> </span>" +
            "<span class=\"tok-generic\"> </span>" +
            "<span class=\"tok-generic\"> </span></code></pre>\n" +
            "</div>\n",
            html);
    }

    [Fact]
    public void Example_138()
    {
        var input = "``` ```\naaa";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: the spec says a line containing only the fence and its language is
        // a code block, but '``` ```' is read as a fence with a language of ' ```' (the
        // trailing backticks are part of the info string), so the content is 'aaa'.
        Assert.Equal(
            "<div class=\"code-block-container\">\n" +
            "<div class=\"code-block-header\">\n" +
            "<span class=\"code-block-language\"> ```</span>\n" +
            "<button class=\"code-block-copy\" onclick=\"copyCode(this)\" title=\"Copy to clipboard\">Copy</button>\n" +
            "</div>\n" +
            "<pre><code class=\"language- ```\"><span class=\"tok-generic\">aaa</span></code></pre>\n" +
            "</div>\n",
            html);
    }

    [Fact]
    public void Example_139()
    {
        var input = "~~~~~~\naaa\n~~~ ~~";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: '~~~~~~' is not a fence; the runs of '~' parse as superscript
        // markers, so the whole block is a plain paragraph.
        Assert.Equal(
            "<p><sup><span class=\"tok-superscript\"></span></sup>" +
            "<sup><span class=\"tok-superscript\"></span></sup>" +
            "<sup><span class=\"tok-superscript\"></span></sup><br/>aaa<br/>" +
            "<sup><span class=\"tok-superscript\"></span></sup>" +
            "<sup><span class=\"tok-superscript\"> </span></sup>~</p>",
            html);
    }

    [Fact]
    public void Example_140()
    {
        var input = "foo\n```\nbar\n```\nbaz";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: the decorated container wrapper, and the open 'foo' paragraph is not
        // closed before the code block (the spec closes the paragraph first), so the
        // container is rendered inside the paragraph element.
        Assert.Equal(
            "<p>foo" +
            "<div class=\"code-block-container\">\n" +
            "<div class=\"code-block-header\">\n" +
            "<span class=\"code-block-language\"></span>\n" +
            "<button class=\"code-block-copy\" onclick=\"copyCode(this)\" title=\"Copy to clipboard\">Copy</button>\n" +
            "</div>\n" +
            "<pre><code class=\"language-\"><span class=\"tok-generic\">bar</span></code></pre>\n" +
            "</div>\n</p>\n" +
            "<p>baz</p>",
            html);
    }

    [Fact]
    public void Example_141()
    {
        var input = "foo\n---\n~~~\nbar\n~~~\n# baz";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: '~~~' is not a fence, so the lines between the '---' horizontal rule
        // and the heading form a paragraph with superscript markers instead of a code block.
        Assert.Equal(
            "<p>foo</p>\n" +
            "<hr />\n" +
            "<p><sup><span class=\"tok-superscript\"></span></sup>~<br/>bar<br/>" +
            "<sup><span class=\"tok-superscript\"></span></sup>~</p>\n" +
            "<h1>baz</h1>",
            html);
    }

    [Fact]
    public void Example_142()
    {
        var input = "```ruby\ndef foo(x)\n  return 3\nend\n```";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: the decorated container wrapper. The 'ruby' language is not a known
        // tokenizer, so the content is emitted as generic code spans (the spec output has
        // no highlighting at all, but the structure and language are the same).
        Assert.Equal(
            "<div class=\"code-block-container\">\n" +
            "<div class=\"code-block-header\">\n" +
            "<span class=\"code-block-language\">ruby</span>\n" +
            "<button class=\"code-block-copy\" onclick=\"copyCode(this)\" title=\"Copy to clipboard\">Copy</button>\n" +
            "</div>\n" +
            "<pre><code class=\"language-ruby\"><span class=\"tok-generic\">def </span>" +
            "<span class=\"tok-generic\">foo(x)\n </span>" +
            "<span class=\"tok-generic\"> </span>" +
            "<span class=\"tok-generic\">return </span>" +
            "<span class=\"tok-generic\">3\nend</span></code></pre>\n" +
            "</div>\n",
            html);
    }

    [Fact]
    public void Example_143()
    {
        var input = "~~~~    ruby startline=3 $%@#$\ndef foo(x)\n  return 3\nend\n~~~~~~~";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: '~~~~' is not a fence; the runs of '~' parse as superscript markers,
        // so the whole block is a plain paragraph (the spec treats it as a ruby code block
        // with an info string).
        Assert.Equal(
            "<p><sup><span class=\"tok-superscript\"></span></sup>" +
            "<sup><span class=\"tok-superscript\"></span></sup>" +
            "    ruby startline=3 $%@#$<br/>def foo(x)<br/>  return 3<br/>end<br/>" +
            "<sup><span class=\"tok-superscript\"></span></sup>" +
            "<sup><span class=\"tok-superscript\"></span></sup>" +
            "<sup><span class=\"tok-superscript\"></span></sup>~</p>",
            html);
    }

    [Fact]
    public void Example_144()
    {
        var input = "````;\n````";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: the opening '````;' is read as '```' plus a language of '`;' (the
        // spec strips the extra backtick from the info string), and the bare '````' line
        // does not close the block (the spec requires at least the opening length of
        // backticks), so the trailing '`' of '````' opens a second (empty) code block.
        Assert.Equal(
            "<div class=\"code-block-container\">\n" +
            "<div class=\"code-block-header\">\n" +
            "<span class=\"code-block-language\">`;</span>\n" +
            "<button class=\"code-block-copy\" onclick=\"copyCode(this)\" title=\"Copy to clipboard\">Copy</button>\n" +
            "</div>\n" +
            "<pre><code class=\"language-`;\"></code></pre>\n" +
            "</div>\n" +
            "<p>`</p>",
            html);
    }

    [Fact]
    public void Example_145()
    {
        var input = "``` aa ```\nfoo";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: the spec says a line containing only the fence and its language is a
        // code block, but '``` aa ```' is read as a fence with a language of ' aa ```'
        // (the trailing backticks are part of the info string), so the content is 'foo'.
        Assert.Equal(
            "<div class=\"code-block-container\">\n" +
            "<div class=\"code-block-header\">\n" +
            "<span class=\"code-block-language\"> aa ```</span>\n" +
            "<button class=\"code-block-copy\" onclick=\"copyCode(this)\" title=\"Copy to clipboard\">Copy</button>\n" +
            "</div>\n" +
            "<pre><code class=\"language- aa ```\"><span class=\"tok-generic\">foo</span></code></pre>\n" +
            "</div>\n",
            html);
    }

    [Fact]
    public void Example_146()
    {
        var input = "~~~ aa ``` ~~~\nfoo\n~~~";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: '~~~' is not a fence; the runs of '~' parse as superscript markers,
        // so the whole block is a plain paragraph (the spec treats it as an 'aa' code
        // block).
        Assert.Equal(
            "<p><sup><span class=\"tok-superscript\"></span></sup>" +
            "<sup><span class=\"tok-superscript\"> aa ``` </span></sup>" +
            "<sup><span class=\"tok-superscript\"></span></sup><br/>foo<br/>" +
            "<sup><span class=\"tok-superscript\"></span></sup>~</p>",
            html);
    }

    [Fact]
    public void Example_147()
    {
        var input = "```\n``` aaa\n```";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: the closing fence is a bare '```' line, so '``` aaa' is read as a
        // new opening fence (with an empty content block) rather than as code content
        // followed by a closing fence, and the remaining ' aaa' and final '```' form a
        // second empty code block inside a paragraph.
        Assert.Equal(
            "<div class=\"code-block-container\">\n" +
            "<div class=\"code-block-header\">\n" +
            "<span class=\"code-block-language\"></span>\n" +
            "<button class=\"code-block-copy\" onclick=\"copyCode(this)\" title=\"Copy to clipboard\">Copy</button>\n" +
            "</div>\n" +
            "<pre><code class=\"language-\"></code></pre>\n" +
            "</div>\n" +
            "<p> aaa" +
            "<div class=\"code-block-container\">\n" +
            "<div class=\"code-block-header\">\n" +
            "<span class=\"code-block-language\"></span>\n" +
            "<button class=\"code-block-copy\" onclick=\"copyCode(this)\" title=\"Copy to clipboard\">Copy</button>\n" +
            "</div>\n" +
            "<pre><code class=\"language-\"></code></pre>\n" +
            "</div>\n</p>",
            html);
    }
}
