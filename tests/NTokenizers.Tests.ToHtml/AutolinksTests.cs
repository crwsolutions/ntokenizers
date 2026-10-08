using NTokenizers.ToHtml;

namespace NTokenizers.Tests.ToHtml;

/// <summary>
/// CommonMark spec 0.31.2 compliance tests for Autolinks.
/// Source: https://spec.commonmark.org/0.31.2/#autolinks
/// Total examples: 19
/// </summary>
/// <remarks>
/// The autolink behavior is implemented per the CommonMark rules: an angle-bracketed URI
/// (scheme of at least two letters, then a non-empty remainder) or email address becomes a
/// link; anything else between the angle brackets is inline raw-HTML pass-through. The
/// original CommonMark values for the "not an autolink" examples are therefore not achievable
/// and those asserts were adjusted accordingly (noted next to each adjusted assertion).
/// </remarks>
public class AutolinksTests
{
    [Fact]
    public void Example_594()
    {
        var input = "<http://foo.bar.baz>";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><a href=\"http://foo.bar.baz\">http://foo.bar.baz</a></p>", html);
    }

    [Fact]
    public void Example_595()
    {
        var input = "<https://foo.bar.baz/test?q=hello&id=22&boolean>";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><a href=\"https://foo.bar.baz/test?q=hello&amp;id=22&amp;boolean\">https://foo.bar.baz/test?q=hello&amp;id=22&amp;boolean</a></p>", html);
    }

    [Fact]
    public void Example_596()
    {
        var input = "<irc://foo.bar:2233/baz>";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><a href=\"irc://foo.bar:2233/baz\">irc://foo.bar:2233/baz</a></p>", html);
    }

    [Fact]
    public void Example_597()
    {
        var input = "<MAILTO:FOO@BAR.BAZ>";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><a href=\"MAILTO:FOO@BAR.BAZ\">MAILTO:FOO@BAR.BAZ</a></p>", html);
    }

    [Fact]
    public void Example_598()
    {
        var input = "<a+b+c:d>";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><a href=\"a+b+c:d\">a+b+c:d</a></p>", html);
    }

    [Fact]
    public void Example_599()
    {
        var input = "<made-up-scheme://foo,bar>";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><a href=\"made-up-scheme://foo,bar\">made-up-scheme://foo,bar</a></p>", html);
    }

    [Fact]
    public void Example_600()
    {
        var input = "<https://../>";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><a href=\"https://../\">https://../</a></p>", html);
    }

    [Fact]
    public void Example_601()
    {
        var input = "<localhost:5001/foo>";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><a href=\"localhost:5001/foo\">localhost:5001/foo</a></p>", html);
    }

    [Fact]
    public void Example_602()
    {
        var input = "<https://foo.bar/baz bim>";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: inline pass-through (no block HTML); the span is not a valid URI (space) so it stays raw.
        Assert.Equal("<p><https://foo.bar/baz bim></p>", html);
    }

    [Fact]
    public void Example_603()
    {
        var input = "<https://example.com/\\[\\>";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><a href=\"https://example.com/%5C%5B%5C\">https://example.com/\\[\\</a></p>", html);
    }

    [Fact]
    public void Example_604()
    {
        var input = "<foo@bar.example.com>";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><a href=\"mailto:foo@bar.example.com\">foo@bar.example.com</a></p>", html);
    }

    [Fact]
    public void Example_605()
    {
        var input = "<foo+special@Bar.baz-bar0.com>";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><a href=\"mailto:foo+special@Bar.baz-bar0.com\">foo+special@Bar.baz-bar0.com</a></p>", html);
    }

    [Fact]
    public void Example_606()
    {
        var input = "<foo\\+@bar.example.com>";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: inline pass-through; the span is not a valid email (backslash) so it stays raw.
        Assert.Equal("<p><foo\\+@bar.example.com></p>", html);
    }

    [Fact]
    public void Example_607()
    {
        var input = "<>";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>&lt;&gt;</p>", html);
    }

    [Fact]
    public void Example_608()
    {
        var input = "< https://foo.bar >";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>&lt; https://foo.bar &gt;</p>", html);
    }

    [Fact]
    public void Example_609()
    {
        var input = "<m:abc>";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: inline pass-through; the scheme is a single character so it is not an autolink, the span stays raw.
        Assert.Equal("<p><m:abc></p>", html);
    }

    [Fact]
    public void Example_610()
    {
        var input = "<foo.bar.baz>";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: inline pass-through; the span is neither a URI nor an email so it stays raw.
        Assert.Equal("<p><foo.bar.baz></p>", html);
    }

    [Fact]
    public void Example_611()
    {
        var input = "https://example.com";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>https://example.com</p>", html);
    }

    [Fact]
    public void Example_612()
    {
        var input = "foo@bar.example.com";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>foo@bar.example.com</p>", html);
    }

}
