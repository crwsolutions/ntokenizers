using NTokenizers.ToHtml;

namespace NTokenizers.CommonMark.Compliance.Tests;

/// <summary>
/// CommonMark spec 0.31.2 compliance tests for Hard line breaks.
/// Source: https://spec.commonmark.org/0.31.2/#hard-line-breaks
/// Total examples: 15
/// </summary>
public class HardLineBreaksTests
{
    [Fact]
    public void Example_633()
    {
        var input = "foo\\\nbaz";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>foo<br />\nbaz</p>", html);
    }

    [Fact]
    public void Example_634()
    {
        var input = "foo       \nbaz";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>foo<br />\nbaz</p>", html);
    }

    [Fact]
    public void Example_635()
    {
        var input = "foo  \n     bar";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>foo<br />\nbar</p>", html);
    }

    [Fact]
    public void Example_636()
    {
        var input = "foo\\\n     bar";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>foo<br />\nbar</p>", html);
    }

    [Fact]
    public void Example_637()
    {
        var input = "*foo  \nbar*";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><em>foo<br />\nbar</em></p>", html);
    }

    [Fact]
    public void Example_638()
    {
        var input = "*foo\\\nbar*";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><em>foo<br />\nbar</em></p>", html);
    }

    [Fact]
    public void Example_639()
    {
        var input = "`code  \nspan`";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><code>code   span</code></p>", html);
    }

    [Fact]
    public void Example_640()
    {
        var input = "`code\\\nspan`";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><code>code\\ span</code></p>", html);
    }

    [Fact]
    public void Example_641()
    {
        var input = "<a href=\"foo  \nbar\">";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"foo  \nbar\"></p>", html);
    }

    [Fact]
    public void Example_642()
    {
        var input = "<a href=\"foo\\\nbar\">";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"foo\\\nbar\"></p>", html);
    }

    [Fact]
    public void Example_643()
    {
        var input = "foo\\";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>foo\\</p>", html);
    }

    [Fact]
    public void Example_644()
    {
        var input = "foo  ";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>foo</p>", html);
    }

    [Fact]
    public void Example_645()
    {
        var input = "### foo\\";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<h3>foo\\</h3>", html);
    }

    [Fact]
    public void Example_646()
    {
        var input = "### foo  ";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<h3>foo</h3>", html);
    }

    [Fact]
    public void Example_647()
    {
        var input = "foo\nbaz";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>foo\nbaz</p>", html);
    }

}
