using NTokenizers.ToHtml;

namespace NTokenizers.CommonMark.Compliance.Tests;

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
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>!&quot;#$%&amp;'()*+,-./:;&lt;=&gt;?@[\\]^_`{|}~</p>", html);
    }

    [Fact]
    public void Example_013()
    {
        var input = "\\\t\\A\\a\\ \\3\\φ\\«";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>\\\t\\A\\a\\ \\3\\φ\\«</p>", html);
    }

    [Fact]
    public void Example_014()
    {
        var input = "\\*not emphasized*\n\\<br/> not a tag\n\\[not a link](/foo)\n\\`not code`\n1\\. not a list\n\\* not a list\n\\# not a heading\n\\[foo]: /url \"not a reference\"\n\\&ouml; not a character entity";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>*not emphasized*\n&lt;br/&gt; not a tag\n[not a link](/foo)\n`not code`\n1. not a list\n* not a list\n# not a heading\n[foo]: /url &quot;not a reference&quot;\n&amp;ouml; not a character entity</p>", html);
    }

    [Fact]
    public void Example_015()
    {
        var input = "\\\\*emphasis*";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>\\<em>emphasis</em></p>", html);
    }

    [Fact]
    public void Example_016()
    {
        var input = "foo\\\nbar";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>foo<br />\nbar</p>", html);
    }

    [Fact]
    public void Example_017()
    {
        var input = "`` \\[\\` ``";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><code>\\[\\`</code></p>", html);
    }

    [Fact]
    public void Example_018()
    {
        var input = "    \\[\\]";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<pre><code>\\[\\]\n</code></pre>", html);
    }

    [Fact]
    public void Example_019()
    {
        var input = "~~~\n\\[\\]\n~~~";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<pre><code>\\[\\]\n</code></pre>", html);
    }

    [Fact]
    public void Example_020()
    {
        var input = "<https://example.com?find=\\*>";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"https://example.com?find=%5C*\">https://example.com?find=\\*</a></p>", html);
    }

    [Fact]
    public void Example_021()
    {
        var input = "<a href=\"/bar\\/)\">";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<a href=\"/bar\\/)\">", html);
    }

    [Fact]
    public void Example_022()
    {
        var input = "[foo](/bar\\* \"ti\\*tle\")";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"/bar*\" title=\"ti*tle\">foo</a></p>", html);
    }

    [Fact]
    public void Example_023()
    {
        var input = "[foo]\n\n[foo]: /bar\\* \"ti\\*tle\"";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"/bar*\" title=\"ti*tle\">foo</a></p>", html);
    }

    [Fact]
    public void Example_024()
    {
        var input = "``` foo\\+bar\nfoo\n```";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<pre><code class=\"language-foo+bar\">foo\n</code></pre>", html);
    }

}
