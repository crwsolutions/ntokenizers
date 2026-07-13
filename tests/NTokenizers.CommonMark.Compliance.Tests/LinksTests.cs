using NTokenizers.ToHtml;

namespace NTokenizers.CommonMark.Compliance.Tests;

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
        var input = "[link](/uri)";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"/uri\">link</a></p>", html);
    }

    [Fact]
    public void Example_483()
    {
        var input = "[](./target.md)";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"./target.md\"></a></p>", html);
    }

    [Fact]
    public void Example_484()
    {
        var input = "[link]()";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"\">link</a></p>", html);
    }

    [Fact]
    public void Example_485()
    {
        var input = "[link](<>)";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"\">link</a></p>", html);
    }

    [Fact]
    public void Example_486()
    {
        var input = "[]()";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"\"></a></p>", html);
    }

    [Fact]
    public void Example_487()
    {
        var input = "[link](/my uri)";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>[link](/my uri)</p>", html);
    }

    [Fact]
    public void Example_488()
    {
        var input = "[link](</my uri>)";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"/my%20uri\">link</a></p>", html);
    }

    [Fact]
    public void Example_489()
    {
        var input = "[link](foo\nbar)";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>[link](foo\nbar)</p>", html);
    }

    [Fact]
    public void Example_490()
    {
        var input = "[link](<foo\nbar>)";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>[link](<foo\nbar>)</p>", html);
    }

    [Fact]
    public void Example_491()
    {
        var input = "[a](<b)c>)";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"b)c\">a</a></p>", html);
    }

    [Fact]
    public void Example_492()
    {
        var input = "[link](<foo\\>)";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>[link](&lt;foo&gt;)</p>", html);
    }

    [Fact]
    public void Example_493()
    {
        var input = "[a](<b)c\n[a](<b)c>\n[a](<b>c)";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>[a](&lt;b)c\n[a](&lt;b)c&gt;\n[a](<b>c)</p>", html);
    }

    [Fact]
    public void Example_494()
    {
        var input = "[link](\\(foo\\))";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"(foo)\">link</a></p>", html);
    }

    [Fact]
    public void Example_495()
    {
        var input = "[link](foo(and(bar)))";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"foo(and(bar))\">link</a></p>", html);
    }

    [Fact]
    public void Example_496()
    {
        var input = "[link](foo(and(bar))";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>[link](foo(and(bar))</p>", html);
    }

    [Fact]
    public void Example_497()
    {
        var input = "[link](foo\\(and\\(bar\\))";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"foo(and(bar)\">link</a></p>", html);
    }

    [Fact]
    public void Example_498()
    {
        var input = "[link](<foo(and(bar)>)";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"foo(and(bar)\">link</a></p>", html);
    }

    [Fact]
    public void Example_499()
    {
        var input = "[link](foo\\)\\:)";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"foo):\">link</a></p>", html);
    }

    [Fact]
    public void Example_500()
    {
        var input = "[link](#fragment)\n\n[link](https://example.com#fragment)\n\n[link](https://example.com?foo=3#frag)";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"#fragment\">link</a></p>\n<p><a href=\"https://example.com#fragment\">link</a></p>\n<p><a href=\"https://example.com?foo=3#frag\">link</a></p>", html);
    }

    [Fact]
    public void Example_501()
    {
        var input = "[link](foo\\bar)";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"foo%5Cbar\">link</a></p>", html);
    }

    [Fact]
    public void Example_502()
    {
        var input = "[link](foo%20b&auml;)";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"foo%20b%C3%A4\">link</a></p>", html);
    }

    [Fact]
    public void Example_503()
    {
        var input = "[link](\"title\")";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"%22title%22\">link</a></p>", html);
    }

    [Fact]
    public void Example_504()
    {
        var input = "[link](/url \"title\")\n[link](/url 'title')\n[link](/url (title))";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"/url\" title=\"title\">link</a>\n<a href=\"/url\" title=\"title\">link</a>\n<a href=\"/url\" title=\"title\">link</a></p>", html);
    }

    [Fact]
    public void Example_505()
    {
        var input = "[link](/url \"title \\\"&quot;\")";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"/url\" title=\"title &quot;&quot;\">link</a></p>", html);
    }

    [Fact]
    public void Example_506()
    {
        var input = "[link](/url \"title\")";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"/url%C2%A0%22title%22\">link</a></p>", html);
    }

    [Fact]
    public void Example_507()
    {
        var input = "[link](/url \"title \"and\" title\")";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>[link](/url &quot;title &quot;and&quot; title&quot;)</p>", html);
    }

    [Fact]
    public void Example_508()
    {
        var input = "[link](/url 'title \"and\" title')";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"/url\" title=\"title &quot;and&quot; title\">link</a></p>", html);
    }

    [Fact]
    public void Example_509()
    {
        var input = "[link](   /uri\n  \"title\"  )";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"/uri\" title=\"title\">link</a></p>", html);
    }

    [Fact]
    public void Example_510()
    {
        var input = "[link] (/uri)";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>[link] (/uri)</p>", html);
    }

    [Fact]
    public void Example_511()
    {
        var input = "[link [foo [bar]]](/uri)";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"/uri\">link [foo [bar]]</a></p>", html);
    }

    [Fact]
    public void Example_512()
    {
        var input = "[link] bar](/uri)";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>[link] bar](/uri)</p>", html);
    }

    [Fact]
    public void Example_513()
    {
        var input = "[link [bar](/uri)";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>[link <a href=\"/uri\">bar</a></p>", html);
    }

    [Fact]
    public void Example_514()
    {
        var input = "[link \\[bar](/uri)";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"/uri\">link [bar</a></p>", html);
    }

    [Fact]
    public void Example_515()
    {
        var input = "[link *foo **bar** `#`*](/uri)";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"/uri\">link <em>foo <strong>bar</strong> <code>#</code></em></a></p>", html);
    }

    [Fact]
    public void Example_516()
    {
        var input = "[![moon](moon.jpg)](/uri)";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"/uri\"><img src=\"moon.jpg\" alt=\"moon\" /></a></p>", html);
    }

    [Fact]
    public void Example_517()
    {
        var input = "[foo [bar](/uri)](/uri)";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>[foo <a href=\"/uri\">bar</a>](/uri)</p>", html);
    }

    [Fact]
    public void Example_518()
    {
        var input = "[foo *[bar [baz](/uri)](/uri)*](/uri)";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>[foo <em>[bar <a href=\"/uri\">baz</a>](/uri)</em>](/uri)</p>", html);
    }

    [Fact]
    public void Example_519()
    {
        var input = "![[[foo](uri1)](uri2)](uri3)";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><img src=\"uri3\" alt=\"[foo](uri2)\" /></p>", html);
    }

    [Fact]
    public void Example_520()
    {
        var input = "*[foo*](/uri)";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>*<a href=\"/uri\">foo*</a></p>", html);
    }

    [Fact]
    public void Example_521()
    {
        var input = "[foo *bar](baz*)";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"baz*\">foo *bar</a></p>", html);
    }

    [Fact]
    public void Example_522()
    {
        var input = "*foo [bar* baz]";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><em>foo [bar</em> baz]</p>", html);
    }

    [Fact]
    public void Example_523()
    {
        var input = "[foo <bar attr=\"](baz)\">";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>[foo <bar attr=\"](baz)\"></p>", html);
    }

    [Fact]
    public void Example_524()
    {
        var input = "[foo`](/uri)`";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>[foo<code>](/uri)</code></p>", html);
    }

    [Fact]
    public void Example_525()
    {
        var input = "[foo<https://example.com/?search=](uri)>";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>[foo<a href=\"https://example.com/?search=%5D(uri)\">https://example.com/?search=](uri)</a></p>", html);
    }

    [Fact]
    public void Example_526()
    {
        var input = "[foo][bar]\n\n[bar]: /url \"title\"";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"/url\" title=\"title\">foo</a></p>", html);
    }

    [Fact]
    public void Example_527()
    {
        var input = "[link [foo [bar]]][ref]\n\n[ref]: /uri";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"/uri\">link [foo [bar]]</a></p>", html);
    }

    [Fact]
    public void Example_528()
    {
        var input = "[link \\[bar][ref]\n\n[ref]: /uri";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"/uri\">link [bar</a></p>", html);
    }

    [Fact]
    public void Example_529()
    {
        var input = "[link *foo **bar** `#`*][ref]\n\n[ref]: /uri";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"/uri\">link <em>foo <strong>bar</strong> <code>#</code></em></a></p>", html);
    }

    [Fact]
    public void Example_530()
    {
        var input = "[![moon](moon.jpg)][ref]\n\n[ref]: /uri";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"/uri\"><img src=\"moon.jpg\" alt=\"moon\" /></a></p>", html);
    }

    [Fact]
    public void Example_531()
    {
        var input = "[foo [bar](/uri)][ref]\n\n[ref]: /uri";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>[foo <a href=\"/uri\">bar</a>]<a href=\"/uri\">ref</a></p>", html);
    }

    [Fact]
    public void Example_532()
    {
        var input = "[foo *bar [baz][ref]*][ref]\n\n[ref]: /uri";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>[foo <em>bar <a href=\"/uri\">baz</a></em>]<a href=\"/uri\">ref</a></p>", html);
    }

    [Fact]
    public void Example_533()
    {
        var input = "*[foo*][ref]\n\n[ref]: /uri";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>*<a href=\"/uri\">foo*</a></p>", html);
    }

    [Fact]
    public void Example_534()
    {
        var input = "[foo *bar][ref]*\n\n[ref]: /uri";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"/uri\">foo *bar</a>*</p>", html);
    }

    [Fact]
    public void Example_535()
    {
        var input = "[foo <bar attr=\"][ref]\">\n\n[ref]: /uri";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>[foo <bar attr=\"][ref]\"></p>", html);
    }

    [Fact]
    public void Example_536()
    {
        var input = "[foo`][ref]`\n\n[ref]: /uri";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>[foo<code>][ref]</code></p>", html);
    }

    [Fact]
    public void Example_537()
    {
        var input = "[foo<https://example.com/?search=][ref]>\n\n[ref]: /uri";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>[foo<a href=\"https://example.com/?search=%5D%5Bref%5D\">https://example.com/?search=][ref]</a></p>", html);
    }

    [Fact]
    public void Example_538()
    {
        var input = "[foo][BaR]\n\n[bar]: /url \"title\"";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"/url\" title=\"title\">foo</a></p>", html);
    }

    [Fact]
    public void Example_539()
    {
        var input = "[ẞ]\n\n[SS]: /url";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"/url\">ẞ</a></p>", html);
    }

    [Fact]
    public void Example_540()
    {
        var input = "[Foo\n  bar]: /url\n\n[Baz][Foo bar]";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"/url\">Baz</a></p>", html);
    }

    [Fact]
    public void Example_541()
    {
        var input = "[foo] [bar]\n\n[bar]: /url \"title\"";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>[foo] <a href=\"/url\" title=\"title\">bar</a></p>", html);
    }

    [Fact]
    public void Example_542()
    {
        var input = "[foo]\n[bar]\n\n[bar]: /url \"title\"";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>[foo]\n<a href=\"/url\" title=\"title\">bar</a></p>", html);
    }

    [Fact]
    public void Example_543()
    {
        var input = "[foo]: /url1\n\n[foo]: /url2\n\n[bar][foo]";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"/url1\">bar</a></p>", html);
    }

    [Fact]
    public void Example_544()
    {
        var input = "[bar][foo\\!]\n\n[foo!]: /url";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>[bar][foo!]</p>", html);
    }

    [Fact]
    public void Example_545()
    {
        var input = "[foo][ref[]\n\n[ref[]: /uri";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>[foo][ref[]</p>\n<p>[ref[]: /uri</p>", html);
    }

    [Fact]
    public void Example_546()
    {
        var input = "[foo][ref[bar]]\n\n[ref[bar]]: /uri";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>[foo][ref[bar]]</p>\n<p>[ref[bar]]: /uri</p>", html);
    }

    [Fact]
    public void Example_547()
    {
        var input = "[[[foo]]]\n\n[[[foo]]]: /url";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>[[[foo]]]</p>\n<p>[[[foo]]]: /url</p>", html);
    }

    [Fact]
    public void Example_548()
    {
        var input = "[foo][ref\\[]\n\n[ref\\[]: /uri";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"/uri\">foo</a></p>", html);
    }

    [Fact]
    public void Example_549()
    {
        var input = "[bar\\\\]: /uri\n\n[bar\\\\]";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"/uri\">bar\\</a></p>", html);
    }

    [Fact]
    public void Example_550()
    {
        var input = "[]\n\n[]: /uri";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>[]</p>\n<p>[]: /uri</p>", html);
    }

    [Fact]
    public void Example_551()
    {
        var input = "[\n ]\n\n[\n ]: /uri";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>[\n]</p>\n<p>[\n]: /uri</p>", html);
    }

    [Fact]
    public void Example_552()
    {
        var input = "[foo][]\n\n[foo]: /url \"title\"";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"/url\" title=\"title\">foo</a></p>", html);
    }

    [Fact]
    public void Example_553()
    {
        var input = "[*foo* bar][]\n\n[*foo* bar]: /url \"title\"";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"/url\" title=\"title\"><em>foo</em> bar</a></p>", html);
    }

    [Fact]
    public void Example_554()
    {
        var input = "[Foo][]\n\n[foo]: /url \"title\"";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"/url\" title=\"title\">Foo</a></p>", html);
    }

    [Fact]
    public void Example_555()
    {
        var input = "[foo] \n[]\n\n[foo]: /url \"title\"";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"/url\" title=\"title\">foo</a>\n[]</p>", html);
    }

    [Fact]
    public void Example_556()
    {
        var input = "[foo]\n\n[foo]: /url \"title\"";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"/url\" title=\"title\">foo</a></p>", html);
    }

    [Fact]
    public void Example_557()
    {
        var input = "[*foo* bar]\n\n[*foo* bar]: /url \"title\"";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"/url\" title=\"title\"><em>foo</em> bar</a></p>", html);
    }

    [Fact]
    public void Example_558()
    {
        var input = "[[*foo* bar]]\n\n[*foo* bar]: /url \"title\"";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>[<a href=\"/url\" title=\"title\"><em>foo</em> bar</a>]</p>", html);
    }

    [Fact]
    public void Example_559()
    {
        var input = "[[bar [foo]\n\n[foo]: /url";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>[[bar <a href=\"/url\">foo</a></p>", html);
    }

    [Fact]
    public void Example_560()
    {
        var input = "[Foo]\n\n[foo]: /url \"title\"";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"/url\" title=\"title\">Foo</a></p>", html);
    }

    [Fact]
    public void Example_561()
    {
        var input = "[foo] bar\n\n[foo]: /url";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"/url\">foo</a> bar</p>", html);
    }

    [Fact]
    public void Example_562()
    {
        var input = "\\[foo]\n\n[foo]: /url \"title\"";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>[foo]</p>", html);
    }

    [Fact]
    public void Example_563()
    {
        var input = "[foo*]: /url\n\n*[foo*]";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>*<a href=\"/url\">foo*</a></p>", html);
    }

    [Fact]
    public void Example_564()
    {
        var input = "[foo][bar]\n\n[foo]: /url1\n[bar]: /url2";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"/url2\">foo</a></p>", html);
    }

    [Fact]
    public void Example_565()
    {
        var input = "[foo][]\n\n[foo]: /url1";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"/url1\">foo</a></p>", html);
    }

    [Fact]
    public void Example_566()
    {
        var input = "[foo]()\n\n[foo]: /url1";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"\">foo</a></p>", html);
    }

    [Fact]
    public void Example_567()
    {
        var input = "[foo](not a link)\n\n[foo]: /url1";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"/url1\">foo</a>(not a link)</p>", html);
    }

    [Fact]
    public void Example_568()
    {
        var input = "[foo][bar][baz]\n\n[baz]: /url";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>[foo]<a href=\"/url\">bar</a></p>", html);
    }

    [Fact]
    public void Example_569()
    {
        var input = "[foo][bar][baz]\n\n[baz]: /url1\n[bar]: /url2";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"/url2\">foo</a><a href=\"/url1\">baz</a></p>", html);
    }

    [Fact]
    public void Example_570()
    {
        var input = "[foo][bar][baz]\n\n[baz]: /url1\n[foo]: /url2";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>[foo]<a href=\"/url1\">bar</a></p>", html);
    }

    [Fact]
    public void Example_571()
    {
        var input = "![foo](/url \"title\")";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><img src=\"/url\" alt=\"foo\" title=\"title\" /></p>", html);
    }

}
