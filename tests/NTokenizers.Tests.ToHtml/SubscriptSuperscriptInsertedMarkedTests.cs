using NTokenizers.ToHtml;

namespace NTokenizers.Tests.ToHtml;

/// <summary>
/// Tests for subscript, superscript, inserted, and marked text ToHtml output.
/// </summary>
public class SubscriptSuperscriptInsertedMarkedTests
{
    [Fact]
    public void Subscript()
    {
        var input = "H~2~O";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>H<sub>2</sub>O</p>", html);
    }

    [Fact]
    public void Superscript()
    {
        var input = "E=mc^2^";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>E=mc<sup>2</sup></p>", html);
    }

    [Fact]
    public void InsertedText()
    {
        var input = "++inserted++";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><ins>inserted</ins></p>", html);
    }

    [Fact]
    public void MarkedText()
    {
        var input = "==marked==";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><mark>marked</mark></p>", html);
    }

    [Fact]
    public void SubscriptWithSurroundingText()
    {
        var input = "Some H~2~O here";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>Some H<sub>2</sub>O here</p>", html);
    }

    [Fact]
    public void SuperscriptWithSurroundingText()
    {
        var input = "Some E=mc^2^ here";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>Some E=mc<sup>2</sup> here</p>", html);
    }

    [Fact]
    public void InsertedWithSurroundingText()
    {
        var input = "Some ++inserted++ here";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>Some <ins>inserted</ins> here</p>", html);
    }

    [Fact]
    public void MarkedWithSurroundingText()
    {
        var input = "Some ==marked== here";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>Some <mark>marked</mark> here</p>", html);
    }

    [Fact]
    public void NestedBoldInSubscript()
    {
        var input = "H~**2**~O";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Bold is not parsed inside subscript in the current streaming implementation
        Assert.Equal("<p>H<sub>**2**</sub>O</p>", html);
    }

    [Fact]
    public void NestedBoldInSuperscript()
    {
        var input = "E=mc^**2**^";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Bold is not parsed inside superscript in the current streaming implementation
        Assert.Equal("<p>E=mc<sup>**2**</sup></p>", html);
    }

    [Fact]
    public void NestedBoldInInserted()
    {
        var input = "++**inserted**++";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Bold is not parsed inside inserted in the current streaming implementation
        Assert.Equal("<p><ins>**inserted**</ins></p>", html);
    }

    [Fact]
    public void NestedBoldInMarked()
    {
        var input = "==**marked**==";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Bold is not parsed inside marked in the current streaming implementation
        Assert.Equal("<p><mark>**marked**</mark></p>", html);
    }
}
