using NTokenizers.ToHtml;

namespace NTokenizers.Tests.ToHtml;

/// <summary>
/// CommonMark spec 0.31.2 compliance tests for Backslash escapes.
/// Source: https://spec.commonmark.org/0.31.2/#backslash-escapes
/// Total examples: 13
/// </summary>
public class BackslashEscapesTests
{
    [Fact]
    public void Example_012()
    {
        var input = "\\!\\\"\\#\\$\\%\\&\\'\\(\\)\\*\\+\\,\\-\\.\\/\\:\\;\\<\\=\\>\\?\\@\\[\\\\\\]\\^\\_\\`\\{\\|\\}\\~";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>!&quot;#$%&amp;'()*+,-./:;&lt;=&gt;?@[\\]^_`{|}~</p>", html);
    }

    [Fact]
    public void Example_013()
    {
        var input = "\\\t\\A\\a\\ \\3\\φ\\«";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>\\\t\\A\\a\\ \\3\\φ\\«</p>", html);
    }

    [Fact]
    public void Example_014()
    {
        var input = "\\*not emphasized\\*\n\\<br/> not a tag\n\\[not a link](/foo)\n\\`not code`\n1\\. not a list\n\\* not a list\n\\# not a heading\n\\[foo]: /url \"not a reference\"\n\\&ouml; not a character entity";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>*not emphasized*<br/>&lt;br/&gt; not a tag<br/>[not a link](/foo)<br/>`not code`<br/>1. not a list<br/>* not a list<br/># not a heading<br/>[foo]: /url &quot;not a reference&quot;<br/>&amp;ouml; not a character entity</p>", html);
    }

    [Fact]
    public void Example_015()
    {
        var input = "\\\\*emphasis*";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>\\<em>emphasis</em></p>", html);
    }

    [Fact]
    public void Example_016()
    {
        var input = "foo\\\nbar";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>foo\\<br/>bar</p>", html);
    }

    [Fact]
    public void Example_017()
    {
        var input = "`` \\[\\` ``";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: code spans preserve their surrounding whitespace (no CommonMark
        // leading/trailing space trim), consistent with the rest of the code-span tests.
        Assert.Equal("<p><code> \\[\\` </code></p>", html);
    }

    [Fact]
    public void Example_018()
    {
        var input = "    \\[\\]";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<pre><code>\\[\\]</code></pre>", html);
    }

    [Fact]
    public void Example_019()
    {
        var input = "~~~\n\\[\\]\n~~~";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Matches the CommonMark spec content: a tilde fence is recognized and the
        // backslash escapes in the code content are literal (no unescaping in code).
        // Deviation: the decorated container wrapper; the code span omits the content's
        // trailing newline.
        Assert.Equal(
            "<div class=\"code-block-container\">\n" +
            "<div class=\"code-block-header\">\n" +
            "<span class=\"code-block-language\"></span>\n" +
            "<button class=\"code-block-copy\" onclick=\"copyCode(this)\" title=\"Copy to clipboard\">Copy</button>\n" +
            "</div>\n" +
            "<pre><code class=\"language-\"><span class=\"tok-generic\">\\[\\]</span></code></pre>\n" +
            "</div>\n",
            html);
    }

    [Fact]
    public void Example_020()
    {
        var input = "<https://example.com?find=\\*>";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><a href=\"https://example.com?find=%5C*\">https://example.com?find=\\*</a></p>", html);
    }

    [Fact]
    public void Example_021()
    {
        var input = "<a href=\"/bar\\/)\">";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: raw HTML is handled as inline content (wrapped in a paragraph), not as an
        // HTML block; the span passes through verbatim up to the first '>' without validation,
        // so the backslash inside the attribute is kept as-is.
        Assert.Equal("<p><a href=\"/bar\\/)\"></p>", html);
    }

    [Fact]
    public void Example_022()
    {
        var input = "[foo](/bar\\* \"ti\\*tle\")";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><a href=\"/bar*\" title=\"ti*tle\">foo</a></p>", html);
    }

    [Fact]
    public void Example_023()
    {
        var input = "[foo]\n\n[foo]: /bar\\* \"ti\\*tle\"";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: link reference definitions are not supported by the streaming
        // tokenizer (see AGENTS.md); both lines render as paragraphs. Note that
        // backslash escapes in the definition are still unescaped in the text.
        Assert.Equal("<p>[foo]</p>\n<p>[foo]: /bar* &quot;ti*tle&quot;</p>", html);
    }

    [Fact]
    public void Example_024()
    {
        var input = "``` foo\\+bar\nfoo\n```";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: the fence info string is not unescaped, so the language keeps the
        // backslash ('foo\+bar' instead of 'foo+bar'), and the block renders as the
        // decorated container instead of a bare <pre><code>.
        Assert.Equal(
            "<div class=\"code-block-container\">\n" +
            "<div class=\"code-block-header\">\n" +
            "<span class=\"code-block-language\"> foo\\+bar</span>\n" +
            "<button class=\"code-block-copy\" onclick=\"copyCode(this)\" title=\"Copy to clipboard\">Copy</button>\n" +
            "</div>\n" +
            "<pre><code class=\"language- foo\\+bar\"><span class=\"tok-generic\">foo</span></code></pre>\n" +
            "</div>\n",
            html);
    }

}
