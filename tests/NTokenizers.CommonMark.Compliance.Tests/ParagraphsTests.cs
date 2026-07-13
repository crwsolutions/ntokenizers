using NTokenizers.ToHtml;

namespace NTokenizers.CommonMark.Compliance.Tests;

/// <summary>
/// CommonMark spec 0.31.2 compliance tests for Paragraphs.
/// Source: https://spec.commonmark.org/0.31.2/#paragraphs
/// Total examples: 8
/// </summary>
public class ParagraphsTests
{
    [Fact]
    public void Example_219()
    {
        var input = "aaa\nbbb\n\nccc\nddd";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>aaa\nbbb</p>\n<p>ccc\nddd</p>", html);
    }

    [Fact]
    public void Example_220()
    {
        var input = "aaa\n\n\nbbb";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>aaa</p>\n<p>bbb</p>", html);
    }

    [Fact]
    public void Example_221()
    {
        var input = "  aaa\n bbb";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>aaa\nbbb</p>", html);
    }

    [Fact]
    public void Example_222()
    {
        var input = "aaa\n             bbb\n                                       ccc";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>aaa\nbbb\nccc</p>", html);
    }

    [Fact]
    public void Example_223()
    {
        var input = "   aaa\nbbb";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>aaa\nbbb</p>", html);
    }

    [Fact]
    public void Example_224()
    {
        var input = "    aaa\nbbb";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<pre><code>aaa\n</code></pre>\n<p>bbb</p>", html);
    }

    [Fact]
    public void Example_225()
    {
        var input = "aaa     \nbbb     ";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>aaa<br />\nbbb</p>", html);
    }

    [Fact]
    public void Example_226()
    {
        var input = "  \n\naaa\n  \n\n# aaa\n\n  ";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>aaa</p>\n<h1>aaa</h1>", html);
    }

}
