using NTokenizers.ToHtml;

namespace NTokenizers.CommonMark.Compliance.Tests;

/// <summary>
/// CommonMark spec 0.31.2 compliance tests for Thematic breaks.
/// Source: https://spec.commonmark.org/0.31.2/#thematic-breaks
/// Total examples: 19
/// </summary>
public class ThematicBreaksTests
{
    [Fact]
    public void Example_043()
    {
        var input = "***\n---\n___";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<hr />\n<hr />\n<hr />", html);
    }

    [Fact]
    public void Example_044()
    {
        var input = "+++";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>+++</p>", html);
    }

    [Fact]
    public void Example_045()
    {
        var input = "===";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>===</p>", html);
    }

    [Fact]
    public void Example_046()
    {
        var input = "--\n**\n__";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>--\n**\n__</p>", html);
    }

    [Fact]
    public void Example_047()
    {
        var input = " ***\n  ***\n   ***";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<hr />\n<hr />\n<hr />", html);
    }

    [Fact]
    public void Example_048()
    {
        var input = "    ***";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<pre><code>***\n</code></pre>", html);
    }

    [Fact]
    public void Example_049()
    {
        var input = "Foo\n    ***";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>Foo\n***</p>", html);
    }

    [Fact]
    public void Example_050()
    {
        var input = "_____________________________________";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<hr />", html);
    }

    [Fact]
    public void Example_051()
    {
        var input = " - - -";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<hr />", html);
    }

    [Fact]
    public void Example_052()
    {
        var input = " **  * ** * ** * **";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<hr />", html);
    }

    [Fact]
    public void Example_053()
    {
        var input = "-     -      -      -";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<hr />", html);
    }

    [Fact]
    public void Example_054()
    {
        var input = "- - - -    ";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<hr />", html);
    }

    [Fact]
    public void Example_055()
    {
        var input = "_ _ _ _ a\n\na------\n\n---a---";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>_ _ _ _ a</p>\n<p>a------</p>\n<p>---a---</p>", html);
    }

    [Fact]
    public void Example_056()
    {
        var input = " *-*";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><em>-</em></p>", html);
    }

    [Fact]
    public void Example_057()
    {
        var input = "- foo\n***\n- bar";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<ul>\n<li>foo</li>\n</ul>\n<hr />\n<ul>\n<li>bar</li>\n</ul>", html);
    }

    [Fact]
    public void Example_058()
    {
        var input = "Foo\n***\nbar";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>Foo</p>\n<hr />\n<p>bar</p>", html);
    }

    [Fact]
    public void Example_059()
    {
        var input = "Foo\n---\nbar";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<h2>Foo</h2>\n<p>bar</p>", html);
    }

    [Fact]
    public void Example_060()
    {
        var input = "* Foo\n* * *\n* Bar";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<ul>\n<li>Foo</li>\n</ul>\n<hr />\n<ul>\n<li>Bar</li>\n</ul>", html);
    }

    [Fact]
    public void Example_061()
    {
        var input = "- Foo\n- * * *";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<ul>\n<li>Foo</li>\n<li>\n<hr />\n</li>\n</ul>", html);
    }

}
