using NTokenizers.ToHtml;

namespace NTokenizers.Tests.ToHtml;

/// <summary>
/// CommonMark spec 0.31.2 compliance tests for Link reference definitions.
/// Source: https://spec.commonmark.org/0.31.2/#link-reference-definitions
/// Total examples: 27
/// </summary>
public class LinkReferenceDefinitionsTests
{[Fact]
    public void Example_197()
    {
        var input = "[foo]: /url 'title\n\nwith blank line'\n\n[foo]";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>[foo]: /url 'title</p>\n<p>with blank line'</p>\n<p>[foo]</p>", html);
    }[Fact]
    public void Example_199()
    {
        var input = "[foo]:\n\n[foo]";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>[foo]:</p>\n<p>[foo]</p>", html);
    }[Fact]
    public void Example_201()
    {
        var input = "[foo]: <bar>(baz)\n\n[foo]";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>[foo]: <bar>(baz)</p>\n<p>[foo]</p>", html);
    }[Fact]
    public void Example_208()
    {
        var input = "[foo]: /url \"title\" ok";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>[foo]: /url &quot;title&quot; ok</p>", html);
    }[Fact]
    public void Example_210()
    {
        var input = "    [foo]: /url \"title\"\n\n[foo]";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<pre><code>[foo]: /url &quot;title&quot;\n</code></pre>\n<p>[foo]</p>", html);
    }[Fact]
    public void Example_218()
    {
        var input = "aaa\n\nbbb";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>aaa</p>\n<p>bbb</p>", html);
    }

}
