using NTokenizers.ToHtml;

namespace NTokenizers.CommonMark.Compliance.Tests;

/// <summary>
/// CommonMark spec 0.31.2 compliance tests for Setext headings.
/// Source: https://spec.commonmark.org/0.31.2/#setext-headings
/// Total examples: 27
/// </summary>
public class SetextHeadingsTests
{
    [Fact]
    public void Example_080()
    {
        var input = "Foo *bar*\n=========\n\nFoo *bar*\n---------";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<h1>Foo <em>bar</em></h1>\n<h2>Foo <em>bar</em></h2>", html);
    }

    [Fact]
    public void Example_081()
    {
        var input = "Foo *bar\nbaz*\n====";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<h1>Foo <em>bar\nbaz</em></h1>", html);
    }

    [Fact]
    public void Example_082()
    {
        var input = "  Foo *bar\nbaz*\t\n====";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<h1>Foo <em>bar\nbaz</em></h1>", html);
    }

    [Fact]
    public void Example_083()
    {
        var input = "Foo\n-------------------------\n\nFoo\n=";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<h2>Foo</h2>\n<h1>Foo</h1>", html);
    }

    [Fact]
    public void Example_084()
    {
        var input = "   Foo\n---\n\n  Foo\n-----\n\n  Foo\n  ===";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<h2>Foo</h2>\n<h2>Foo</h2>\n<h1>Foo</h1>", html);
    }

    [Fact]
    public void Example_085()
    {
        var input = "    Foo\n    ---\n\n    Foo\n---";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<pre><code>Foo\n---\n\nFoo\n</code></pre>\n<hr />", html);
    }

    [Fact]
    public void Example_086()
    {
        var input = "Foo\n   ----      ";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<h2>Foo</h2>", html);
    }

    [Fact]
    public void Example_087()
    {
        var input = "Foo\n    ---";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>Foo\n---</p>", html);
    }

    [Fact]
    public void Example_088()
    {
        var input = "Foo\n= =\n\nFoo\n--- -";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>Foo\n= =</p>\n<p>Foo</p>\n<hr />", html);
    }

    [Fact]
    public void Example_089()
    {
        var input = "Foo  \n-----";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<h2>Foo</h2>", html);
    }

    [Fact]
    public void Example_090()
    {
        var input = "Foo\\\n----";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<h2>Foo\\</h2>", html);
    }

    [Fact]
    public void Example_091()
    {
        var input = "`Foo\n----\n`\n\n<a title=\"a lot\n---\nof dashes\"/>";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<h2>`Foo</h2>\n<p>`</p>\n<h2>&lt;a title=&quot;a lot</h2>\n<p>of dashes&quot;/&gt;</p>", html);
    }

    [Fact]
    public void Example_092()
    {
        var input = "> Foo\n---";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<blockquote>\n<p>Foo</p>\n</blockquote>\n<hr />", html);
    }

    [Fact]
    public void Example_093()
    {
        var input = "> foo\nbar\n===";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<blockquote>\n<p>foo\nbar\n===</p>\n</blockquote>", html);
    }

    [Fact]
    public void Example_094()
    {
        var input = "- Foo\n---";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<ul>\n<li>Foo</li>\n</ul>\n<hr />", html);
    }

    [Fact]
    public void Example_095()
    {
        var input = "Foo\nBar\n---";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<h2>Foo\nBar</h2>", html);
    }

    [Fact]
    public void Example_096()
    {
        var input = "---\nFoo\n---\nBar\n---\nBaz";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<hr />\n<h2>Foo</h2>\n<h2>Bar</h2>\n<p>Baz</p>", html);
    }

    [Fact]
    public void Example_097()
    {
        var input = "\n====";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>====</p>", html);
    }

    [Fact]
    public void Example_098()
    {
        var input = "---\n---";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<hr />\n<hr />", html);
    }

    [Fact]
    public void Example_099()
    {
        var input = "- foo\n-----";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<ul>\n<li>foo</li>\n</ul>\n<hr />", html);
    }

    [Fact]
    public void Example_100()
    {
        var input = "    foo\n---";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<pre><code>foo\n</code></pre>\n<hr />", html);
    }

    [Fact]
    public void Example_101()
    {
        var input = "> foo\n-----";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<blockquote>\n<p>foo</p>\n</blockquote>\n<hr />", html);
    }

    [Fact]
    public void Example_102()
    {
        var input = "\\> foo\n------";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<h2>&gt; foo</h2>", html);
    }

    [Fact]
    public void Example_103()
    {
        var input = "Foo\n\nbar\n---\nbaz";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>Foo</p>\n<h2>bar</h2>\n<p>baz</p>", html);
    }

    [Fact]
    public void Example_104()
    {
        var input = "Foo\nbar\n\n---\n\nbaz";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>Foo\nbar</p>\n<hr />\n<p>baz</p>", html);
    }

    [Fact]
    public void Example_105()
    {
        var input = "Foo\nbar\n* * *\nbaz";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>Foo\nbar</p>\n<hr />\n<p>baz</p>", html);
    }

    [Fact]
    public void Example_106()
    {
        var input = "Foo\nbar\n\\---\nbaz";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>Foo\nbar\n---\nbaz</p>", html);
    }

}
