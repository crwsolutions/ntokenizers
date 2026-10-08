using NTokenizers.ToHtml;

namespace NTokenizers.Tests.ToHtml;

/// <summary>
/// CommonMark spec 0.31.2 compliance tests for Images.
/// Source: https://spec.commonmark.org/0.31.2/#images
/// Total examples: 22
/// </summary>
public class ImagesTests
{
    [Fact]
    public void Example_572()
    {
        var input = "![foo](/url \"title\")";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><img src=\"/url\" alt=\"foo\" title=\"title\" /></p>", html);
    }

    [Fact]
    public void Example_578()
    {
        var input = "![foo](train.jpg)";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><img src=\"train.jpg\" alt=\"foo\" /></p>", html);
    }

    [Fact]
    public void Example_579()
    {
        var input = "My ![foo bar](/path/to/train.jpg  \"title\"   )";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>My <img src=\"/path/to/train.jpg\" alt=\"foo bar\" title=\"title\" /></p>", html);
    }

    [Fact]
    public void Example_580()
    {
        var input = "![foo](<url>)";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><img src=\"url\" alt=\"foo\" /></p>", html);
    }

    [Fact]
    public void Example_581()
    {
        var input = "![](/url)";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><img src=\"/url\" /></p>", html);
    }

    [Fact]
    public void Example_592()
    {
        var input = "!\\[foo]\n\n[foo]: /url \"title\"";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>![foo]</p>\n<p>[foo]: /url &quot;title&quot;</p>", html);
    }

    [Fact]
    public void Example_593()
    {
        var input = "\\![foo]\n\n[foo]: /url \"title\"";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>![foo]</p>\n<p>[foo]: /url &quot;title&quot;</p>", html);
    }

}
