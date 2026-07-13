using NTokenizers.ToHtml;

namespace NTokenizers.CommonMark.Compliance.Tests;

/// <summary>
/// CommonMark spec 0.31.2 compliance tests for ATX headings.
/// Source: https://spec.commonmark.org/0.31.2/#atx-headings
/// Total examples: 18
/// </summary>
public class AtxHeadingsTests
{
    [Fact]
    public void Example_062()
    {
        var input = "# foo\n## foo\n### foo\n#### foo\n##### foo\n###### foo";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<h1>foo</h1>\n<h2>foo</h2>\n<h3>foo</h3>\n<h4>foo</h4>\n<h5>foo</h5>\n<h6>foo</h6>", html);
    }

    [Fact]
    public void Example_063()
    {
        var input = "####### foo";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>####### foo</p>", html);
    }

    [Fact]
    public void Example_064()
    {
        var input = "#5 bolt\n\n#hashtag";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>#5 bolt</p>\n<p>#hashtag</p>", html);
    }

    [Fact]
    public void Example_065()
    {
        var input = "\\## foo";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>## foo</p>", html);
    }

    [Fact]
    public void Example_066()
    {
        var input = "# foo *bar* \\*baz\\*";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<h1>foo <em>bar</em> *baz*</h1>", html);
    }

    [Fact]
    public void Example_067()
    {
        var input = "#                  foo                     ";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<h1>foo</h1>", html);
    }

    [Fact]
    public void Example_068()
    {
        var input = " ### foo\n  ## foo\n   # foo";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<h3>foo</h3>\n<h2>foo</h2>\n<h1>foo</h1>", html);
    }

    [Fact]
    public void Example_069()
    {
        var input = "    # foo";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<pre><code># foo\n</code></pre>", html);
    }

    [Fact]
    public void Example_070()
    {
        var input = "foo\n    # bar";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>foo\n# bar</p>", html);
    }

    [Fact]
    public void Example_071()
    {
        var input = "## foo ##\n  ###   bar    ###";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<h2>foo</h2>\n<h3>bar</h3>", html);
    }

    [Fact]
    public void Example_072()
    {
        var input = "# foo ##################################\n##### foo ##";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<h1>foo</h1>\n<h5>foo</h5>", html);
    }

    [Fact]
    public void Example_073()
    {
        var input = "### foo ###     ";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<h3>foo</h3>", html);
    }

    [Fact]
    public void Example_074()
    {
        var input = "### foo ### b";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<h3>foo ### b</h3>", html);
    }

    [Fact]
    public void Example_075()
    {
        var input = "# foo#";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<h1>foo#</h1>", html);
    }

    [Fact]
    public void Example_076()
    {
        var input = "### foo \\###\n## foo #\\##\n# foo \\#";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<h3>foo ###</h3>\n<h2>foo ###</h2>\n<h1>foo #</h1>", html);
    }

    [Fact]
    public void Example_077()
    {
        var input = "****\n## foo\n****";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<hr />\n<h2>foo</h2>\n<hr />", html);
    }

    [Fact]
    public void Example_078()
    {
        var input = "Foo bar\n# baz\nBar foo";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>Foo bar</p>\n<h1>baz</h1>\n<p>Bar foo</p>", html);
    }

    [Fact]
    public void Example_079()
    {
        var input = "## \n#\n### ###";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<h2></h2>\n<h1></h1>\n<h3></h3>", html);
    }

}
