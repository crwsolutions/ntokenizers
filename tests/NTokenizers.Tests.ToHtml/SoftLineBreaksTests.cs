using NTokenizers.ToHtml;

namespace NTokenizers.Tests.ToHtml;

/// <summary>
/// CommonMark spec 0.31.2 compliance tests for Soft line breaks.
/// Source: https://spec.commonmark.org/0.31.2/#soft-line-breaks
/// Total examples: 2
/// </summary>
public class SoftLineBreaksTests
{
    [Fact]
    public void Example_648()
    {
        var input = "foo\nbaz";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>foo<br/>baz</p>", html);
    }

    [Fact]
    public void Example_649()
    {
        var input = "foo \n baz";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>foo <br/> baz</p>", html);
    }

}
