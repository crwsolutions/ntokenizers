using NTokenizers.ToHtml;

namespace NTokenizers.CommonMark.Compliance.Tests;

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
        var input = "![foo *bar*]\n\n[foo *bar*]: train.jpg \"train & tracks\"";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><img src=\"train.jpg\" alt=\"foo bar\" title=\"train &amp; tracks\" /></p>", html);
    }

    [Fact]
    public void Example_573()
    {
        var input = "![foo ![bar](/url)](/url2)";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><img src=\"/url2\" alt=\"foo bar\" /></p>", html);
    }

    [Fact]
    public void Example_574()
    {
        var input = "![foo [bar](/url)](/url2)";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><img src=\"/url2\" alt=\"foo bar\" /></p>", html);
    }

    [Fact]
    public void Example_575()
    {
        var input = "![foo *bar*][]\n\n[foo *bar*]: train.jpg \"train & tracks\"";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><img src=\"train.jpg\" alt=\"foo bar\" title=\"train &amp; tracks\" /></p>", html);
    }

    [Fact]
    public void Example_576()
    {
        var input = "![foo *bar*][foobar]\n\n[FOOBAR]: train.jpg \"train & tracks\"";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><img src=\"train.jpg\" alt=\"foo bar\" title=\"train &amp; tracks\" /></p>", html);
    }

    [Fact]
    public void Example_577()
    {
        var input = "![foo](train.jpg)";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><img src=\"train.jpg\" alt=\"foo\" /></p>", html);
    }

    [Fact]
    public void Example_578()
    {
        var input = "My ![foo bar](/path/to/train.jpg  \"title\"   )";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>My <img src=\"/path/to/train.jpg\" alt=\"foo bar\" title=\"title\" /></p>", html);
    }

    [Fact]
    public void Example_579()
    {
        var input = "![foo](<url>)";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><img src=\"url\" alt=\"foo\" /></p>", html);
    }

    [Fact]
    public void Example_580()
    {
        var input = "![](/url)";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><img src=\"/url\" alt=\"\" /></p>", html);
    }

    [Fact]
    public void Example_581()
    {
        var input = "![foo][bar]\n\n[bar]: /url";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><img src=\"/url\" alt=\"foo\" /></p>", html);
    }

    [Fact]
    public void Example_582()
    {
        var input = "![foo][bar]\n\n[BAR]: /url";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><img src=\"/url\" alt=\"foo\" /></p>", html);
    }

    [Fact]
    public void Example_583()
    {
        var input = "![foo][]\n\n[foo]: /url \"title\"";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><img src=\"/url\" alt=\"foo\" title=\"title\" /></p>", html);
    }

    [Fact]
    public void Example_584()
    {
        var input = "![*foo* bar][]\n\n[*foo* bar]: /url \"title\"";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><img src=\"/url\" alt=\"foo bar\" title=\"title\" /></p>", html);
    }

    [Fact]
    public void Example_585()
    {
        var input = "![Foo][]\n\n[foo]: /url \"title\"";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><img src=\"/url\" alt=\"Foo\" title=\"title\" /></p>", html);
    }

    [Fact]
    public void Example_586()
    {
        var input = "![foo] \n[]\n\n[foo]: /url \"title\"";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><img src=\"/url\" alt=\"foo\" title=\"title\" />\n[]</p>", html);
    }

    [Fact]
    public void Example_587()
    {
        var input = "![foo]\n\n[foo]: /url \"title\"";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><img src=\"/url\" alt=\"foo\" title=\"title\" /></p>", html);
    }

    [Fact]
    public void Example_588()
    {
        var input = "![*foo* bar]\n\n[*foo* bar]: /url \"title\"";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><img src=\"/url\" alt=\"foo bar\" title=\"title\" /></p>", html);
    }

    [Fact]
    public void Example_589()
    {
        var input = "![[foo]]\n\n[[foo]]: /url \"title\"";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>![[foo]]</p>\n<p>[[foo]]: /url &quot;title&quot;</p>", html);
    }

    [Fact]
    public void Example_590()
    {
        var input = "![Foo]\n\n[foo]: /url \"title\"";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><img src=\"/url\" alt=\"Foo\" title=\"title\" /></p>", html);
    }

    [Fact]
    public void Example_591()
    {
        var input = "!\\[foo]\n\n[foo]: /url \"title\"";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>![foo]</p>", html);
    }

    [Fact]
    public void Example_592()
    {
        var input = "\\![foo]\n\n[foo]: /url \"title\"";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>!<a href=\"/url\" title=\"title\">foo</a></p>", html);
    }

    [Fact]
    public void Example_593()
    {
        var input = "<http://foo.bar.baz>";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"http://foo.bar.baz\">http://foo.bar.baz</a></p>", html);
    }

}
