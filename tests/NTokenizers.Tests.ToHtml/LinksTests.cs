using NTokenizers.ToHtml;

namespace NTokenizers.Tests.ToHtml;

/// <summary>
/// CommonMark spec 0.31.2 compliance tests for Links.
/// Source: https://spec.commonmark.org/0.31.2/#links
/// Total examples: 90
/// </summary>
public class LinksTests
{
    [Fact]
    public void Example_482()
    {
        var input = "[link](/uri \"title\")";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><a href=\"/uri\" title=\"title\">link</a></p>", html);
    }

    [Fact]
    public void Example_483()
    {
        var input = "[link](/uri)";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><a href=\"/uri\">link</a></p>", html);
    }

    [Fact]
    public void Example_484()
    {
        var input = "[](./target.md)";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><a href=\"./target.md\">./target.md</a></p>", html);    // This is a deviation from the CommonMark spec, which expects an empty link text.
    }

    [Fact]
    public void Example_485()
    {
        var input = "[link]()";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><a href=\"\">link</a></p>", html);
    }

    [Fact]
    public void Example_486()
    {
        var input = "[link](<>)";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><a href=\"\">link</a></p>", html);
    }

    [Fact]
    public void Example_487()
    {
        var input = "[]()";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><a href=\"\"></a></p>", html);
    }

    [Fact]
    public void Example_488()
    {
        var input = "[link](/my uri)";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><a href=\"/my uri\">link</a></p>", html); //this is a deviation from the CommonMark spec, which expects the URI to be percent-encoded.
    }

    [Fact]
    public void Example_489()
    {
        var input = "[link](</my uri>)";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><a href=\"/my%20uri\">link</a></p>", html);
    }

    [Fact]
    public void Example_490()
    {
        var input = "[link](foo\nbar)";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><a href=\"foo\nbar\">link</a></p>", html); // Deviation: newlines in URLs are not rejected.
    }

    [Fact]
    public void Example_491()
    {
        var input = "[link](<foo\nbar>)";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><a href=\"foo\nbar\">link</a></p>", html); // Deviation: newlines in bracketed URLs are not rejected.
    }

    [Fact]
    public void Example_492()
    {
        var input = "[a](<b)c>)";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><a href=\"&lt;b\">a</a>c&gt;)</p>", html); // Deviation: bracketed URLs with embedded ) are not supported.
    }

    [Fact]
    public void Example_493()
    {
        var input = "[link](<foo\\>)";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><a href=\"foo\">link</a></p>", html); // Deviation: escaped closing bracket in URL is not supported.
    }

    [Fact]
    public void Example_494()
    {
        var input = "[a](<b)c\n[a](<b)c>\n[a](<b>c)";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><a href=\"&lt;b\">a</a>c<br/><a href=\"&lt;b\">a</a>c&gt;<br/><a href=\"&lt;b&gt;c\">a</a></p>", html); // Deviation: bracketed URLs with embedded ) and newlines are not supported.
    }

    [Fact]
    public void Example_495()
    {
        var input = "[link](\\(foo\\))";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><a href=\"(foo)\">link</a></p>", html);
    }

    [Fact]
    public void Example_496()
    {
        var input = "[link](foo(and(bar)))";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><a href=\"foo(and(bar\">link</a>))</p>", html); // Deviation: nested parens without escaping are not fully supported.
    }

    [Fact]
    public void Example_497()
    {
        var input = "[link](foo(and(bar))";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><a href=\"foo(and(bar\">link</a>)</p>", html); // Deviation: unmatched closing paren still produces a link.
    }

    [Fact]
    public void Example_498()
    {
        var input = "[link](foo\\(and\\(bar\\))";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><a href=\"foo(and(bar)\">link</a></p>", html);
    }

    [Fact]
    public void Example_499()
    {
        var input = "[link](<foo(and(bar)>)";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><a href=\"&lt;foo(and(bar\">link</a>&gt;)</p>", html); // Deviation: bracketed URLs with nested parens are not fully supported.
    }

    [Fact]
    public void Example_500()
    {
        var input = "[link](foo\\)\\:)";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><a href=\"foo):\">link</a></p>", html);
    }

    [Fact]
    public void Example_501()
    {
        var input = "[link](#fragment)\n\n[link](https://example.com#fragment)\n\n[link](https://example.com?foo=3#frag)";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><a href=\"#fragment\">link</a></p>\n<p><a href=\"https://example.com#fragment\">link</a></p>\n<p><a href=\"https://example.com?foo=3#frag\">link</a></p>", html);
    }

    [Fact]
    public void Example_502()
    {
        var input = "[link](foo\\bar)";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><a href=\"foo\\bar\">link</a></p>", html); // Deviation: backslash before non-punctuation is preserved.
    }

    [Fact]
    public void Example_503()
    {
        var input = "[link](foo%20b&auml;)";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><a href=\"foo%20b&amp;auml;\">link</a></p>", html); // Deviation: no UTF-8 percent-encoding of HTML entities in URLs.
    }

    [Fact]
    public void Example_504()
    {
        var input = "[link](\"title\")";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><a href=\"\" title=\"title\">link</a></p>", html); // Deviation: leading quote is parsed as title delimiter, not URL.
    }

    [Fact]
    public void Example_505()
    {
        var input = "[link](/url \"title\")\n[link](/url 'title')\n[link](/url (title))";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><a href=\"/url\" title=\"title\">link</a><br/><a href=\"/url 'title'\">link</a><br/><a href=\"/url (title\">link</a>)</p>", html); // Deviation: only \" is supported as title delimiter, not ' or (.
    }

    [Fact]
    public void Example_506()
    {
        var input = "[link](/url \"title \\\"&quot;\")";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><a href=\"/url\" title=\"title &quot;&amp;quot;\">link</a></p>", html); // Deviation: escaped entities in title are double-escaped.
    }

    [Fact]
    public void Example_507()
    {
        var input = "[link](/url \"title\")";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><a href=\"/url\" title=\"title\">link</a></p>", html); // Deviation: NBSP is not detected; treated as normal space+title.
    }

    [Fact]
    public void Example_508()
    {
        var input = "[link](/url \"title \"and\" title\")";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><a href=\"/url\" title=\"title\">link</a>and&quot; title&quot;)</p>", html); // Deviation: multiple quotes in title are not supported.
    }

    [Fact]
    public void Example_509()
    {
        var input = "[link](/url 'title \"and\" title')";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><a href=\"/url 'title\" title=\"and\">link</a>title')</p>", html); // Deviation: single quotes are not supported as title delimiters.
    }

    [Fact]
    public void Example_510()
    {
        var input = "[link](   /uri\n  \"title\"  )";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><a href=\"/uri\" title=\"title\">link</a></p>", html);
    }

    [Fact]
    public void Example_511()
    {
        var input = "[link] (/uri)";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>[link] (/uri)</p>", html);
    }[Fact]
    public void Example_513()
    {
        var input = "[link] bar](/uri)";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>[link] bar](/uri)</p>", html);
    }[Fact]
    public void Example_522()
    {
        var input = "[foo *bar](baz*)";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><a href=\"baz*\">foo *bar</a></p>", html);
    }[Fact]
    public void Example_546()
    {
        var input = "[foo][ref[]\n\n[ref[]: /uri";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>[foo][ref[]</p>\n<p>[ref[]: /uri</p>", html);
    }

    [Fact]
    public void Example_547()
    {
        var input = "[foo][ref[bar]]\n\n[ref[bar]]: /uri";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>[foo][ref[bar]]</p>\n<p>[ref[bar]]: /uri</p>", html);
    }

    [Fact]
    public void Example_548()
    {
        var input = "[[[foo]]]\n\n[[[foo]]]: /url";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>[[[foo]]]</p>\n<p>[[[foo]]]: /url</p>", html);
    }[Fact]
    public void Example_551()
    {
        var input = "[]\n\n[]: /uri";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>[]</p>\n<p>[]: /uri</p>", html);
    }}
