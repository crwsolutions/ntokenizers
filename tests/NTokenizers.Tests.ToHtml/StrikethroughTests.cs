using NTokenizers.ToHtml;

namespace NTokenizers.Tests.ToHtml;

/// <summary>
/// Tests for GFM strikethrough support (~text~).
/// </summary>
public class StrikethroughTests
{
    [Fact]
    public void BasicStrikethrough()
    {
        var input = "~~deleted~~";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><del>deleted</del></p>", html);
    }

    [Fact]
    public void StrikethroughWithSurroundingText()
    {
        var input = "Some ~~deleted~~ text";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>Some <del>deleted</del> text</p>", html);
    }

    [Fact]
    public void StrikethroughWithBoldInside()
    {
        var input = "~~bold and **bold** text~~";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Bold is not parsed inside strikethrough in the current streaming implementation
        Assert.Equal("<p><del>bold and **bold** text</del></p>", html);
    }

    [Fact]
    public void StrikethroughAdjacentToBold()
    {
        var input = "**bold** ~~deleted~~";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><strong>bold</strong> <del>deleted</del></p>", html);
    }

    [Fact]
    public void UnclosedStrikethroughRendersAsText()
    {
        var input = "~~unclosed";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>~~unclosed</p>", html);
    }

    [Fact]
    public void SuperscriptStillWorks()
    {
        var input = "^sup^";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><sup>sup</sup></p>", html);
    }

    [Fact]
    public void DoubleTildeIsStrikethroughNotSuperscript()
    {
        var input = "~~deleted~~";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Contains("<del>", html);
        Assert.DoesNotContain("<sup>", html);
    }
}
