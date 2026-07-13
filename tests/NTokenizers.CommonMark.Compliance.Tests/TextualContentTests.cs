using NTokenizers.ToHtml;

namespace NTokenizers.CommonMark.Compliance.Tests;

/// <summary>
/// CommonMark spec 0.31.2 compliance tests for Textual content.
/// Source: https://spec.commonmark.org/0.31.2/#textual-content
/// Total examples: 2
/// </summary>
public class TextualContentTests
{
    [Fact]
    public void Example_650()
    {
        var input = "Foo χρῆν";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>Foo χρῆν</p>", html);
    }

    [Fact]
    public void Example_651()
    {
        var input = "Multiple     spaces";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>Multiple     spaces</p>", html);
    }

}
