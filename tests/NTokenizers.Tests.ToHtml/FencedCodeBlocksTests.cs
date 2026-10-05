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
        // A '~~~' tilde fence is recognized just like a backtick fence, so this renders as
        // the decorated code block container. The content '<\n >' is generic code.
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
        // Matches the CommonMark spec content: only the '~~~' fence closes the tilde
        // block, so '```' stays as code content. Deviation: the decorated container
        // wrapper.
        Assert.Equal(
            "<div class=\"code-block-container\">\n" +
            "<div class=\"code-block-header\">\n" +
            "<span class=\"code-block-language\"></span>\n" +
            "<button class=\"code-block-copy\" onclick=\"copyCode(this)\" title=\"Copy to clipboard\">Copy</button>\n" +
            "</div>\n" +
            "<pre><code class=\"language-\"><span class=\"tok-generic\">aaa\n```</span></code></pre>\n" +
            "</div>\n",
            html);
    }

    [Fact]
    public void Example_124()
    {
        var input = "````\naaa\n```\n``````";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Matches the CommonMark spec content (fence length matching): the '```' line is
        // shorter than the opening '````' fence, so it is content and the '``````' line
        // closes the block. Deviations: the decorated container wrapper, and the
        // unclosed-block content is cut at the first line carrying the stop delimiter, so
        // the two backticks of '``````' beyond the required run form a paragraph after.
        Assert.Equal(
            "<div class=\"code-block-container\">\n" +
            "<div class=\"code-block-header\">\n" +
            "<span class=\"code-block-language\"></span>\n" +
            "<button class=\"code-block-copy\" onclick=\"copyCode(this)\" title=\"Copy to clipboard\">Copy</button>\n" +
            "</div>\n" +
            "<pre><code class=\"language-\"><span class=\"tok-generic\">aaa\n```</span></code></pre>\n" +
            "</div>\n" +
            "<p>``</p>",
            html);
    }

    [Fact]
    public void Example_125()
    {
        var input = "~~~~\naaa\n~~~\n~~~~";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Matches the CommonMark spec content (fence length matching): the '~~~' line is
        // shorter than the opening '~~~~' fence, so it is content and the final '~~~~'
        // closes the block. Deviation: the decorated container wrapper.
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
        // Matches the CommonMark spec content (fence length matching): the bare '```' line
        // is shorter than the opening '`````' fence, so it is content and the block runs
        // to end of stream. Deviations: the decorated container wrapper, and the unclosed
        // block content is cut at the first line carrying the stop delimiter, so the extra
        // backtick of the '```' line (and the 'aaa') stay in the code instead of being
        // emitted as a paragraph.
        Assert.Equal(
            "<div class=\"code-block-container\">\n" +
            "<div class=\"code-block-header\">\n" +
            "<span class=\"code-block-language\"></span>\n" +
            "<button class=\"code-block-copy\" onclick=\"copyCode(this)\" title=\"Copy to clipboard\">Copy</button>\n" +
            "</div>\n" +
            "<pre><code class=\"language-\"><span class=\"tok-generic\">\n```\naaa</span></code></pre>\n" +
            "</div>\n",
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
        // Matches the CommonMark spec content (fence length matching): the '~~~ ~~' line
        // is not a run of six tildes, so it is content and the block runs to end of
        // stream. Deviations: the decorated container wrapper, and the '~~~ ~~' content is
        // emitted as two generic spans split at the space.
        Assert.Equal(
            "<div class=\"code-block-container\">\n" +
            "<div class=\"code-block-header\">\n" +
            "<span class=\"code-block-language\"></span>\n" +
            "<button class=\"code-block-copy\" onclick=\"copyCode(this)\" title=\"Copy to clipboard\">Copy</button>\n" +
            "</div>\n" +
            "<pre><code class=\"language-\"><span class=\"tok-generic\">aaa\n~~~ </span>" +
            "<span class=\"tok-generic\">~~</span></code></pre>\n" +
            "</div>\n",
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
        // Matches the CommonMark spec content: the '~~~' lines open and close a tilde
        // code block between the thematic break and the heading. Deviation: the
        // decorated container wrapper.
        Assert.Equal(
            "<p>foo</p>\n" +
            "<hr />\n" +
            "<div class=\"code-block-container\">\n" +
            "<div class=\"code-block-header\">\n" +
            "<span class=\"code-block-language\"></span>\n" +
            "<button class=\"code-block-copy\" onclick=\"copyCode(this)\" title=\"Copy to clipboard\">Copy</button>\n" +
            "</div>\n" +
            "<pre><code class=\"language-\"><span class=\"tok-generic\">bar</span></code></pre>\n" +
            "</div>\n" +
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
        // Matches the CommonMark spec content: a '~~~~' tilde fence with an info string,
        // closed by the longer '~~~~~~~' fence. Deviations: the decorated container
        // wrapper, and the info string is not trimmed or reduced to its first word, so
        // the language keeps its leading spaces ('    ruby startline=3 $%@#$').
        Assert.Equal(
            "<div class=\"code-block-container\">\n" +
            "<div class=\"code-block-header\">\n" +
            "<span class=\"code-block-language\">    ruby startline=3 $%@#$</span>\n" +
            "<button class=\"code-block-copy\" onclick=\"copyCode(this)\" title=\"Copy to clipboard\">Copy</button>\n" +
            "</div>\n" +
            "<pre><code class=\"language-    ruby startline=3 $%@#$\"><span class=\"tok-generic\">def </span>" +
            "<span class=\"tok-generic\">foo(x)\n </span>" +
            "<span class=\"tok-generic\"> </span>" +
            "<span class=\"tok-generic\">return </span>" +
            "<span class=\"tok-generic\">3\nend</span></code></pre>\n" +
            "</div>\n" +
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
    public void Example_144()
    {
        var input = "````;\n````";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Matches the CommonMark spec content (fence length matching): a '````' fence with
        // info string ';' is closed by the bare '````' line, so the block is empty.
        // Deviation: the decorated container wrapper.
        Assert.Equal(
            "<div class=\"code-block-container\">\n" +
            "<div class=\"code-block-header\">\n" +
            "<span class=\"code-block-language\">;</span>\n" +
            "<button class=\"code-block-copy\" onclick=\"copyCode(this)\" title=\"Copy to clipboard\">Copy</button>\n" +
            "</div>\n" +
            "<pre><code class=\"language-;\"></code></pre>\n" +
            "</div>\n",
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
        // Matches the CommonMark spec content: a '~~~' tilde fence whose info string may
        // contain backticks; the block is closed by the final '~~~'. Deviations: the
        // decorated container wrapper, and the info string is not reduced to its first
        // word, so the language keeps ' aa ``` ~~~'.
        Assert.Equal(
            "<div class=\"code-block-container\">\n" +
            "<div class=\"code-block-header\">\n" +
            "<span class=\"code-block-language\"> aa ``` ~~~</span>\n" +
            "<button class=\"code-block-copy\" onclick=\"copyCode(this)\" title=\"Copy to clipboard\">Copy</button>\n" +
            "</div>\n" +
            "<pre><code class=\"language- aa ``` ~~~\"><span class=\"tok-generic\">foo</span></code></pre>\n" +
            "</div>\n",
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
        // second empty code block inside a paragraph. Deviation: the newline after ' aaa'
        // is faithfully emitted as a Text("\n") separator after the PEnd; the dispatcher
        // writes its block separation before that following token, so the output ends with
        // a trailing line break.
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
            "</div>\n</p>\n",
            html);
    }
}
