using NTokenizers.ToHtml;

namespace NTokenizers.CommonMark.Compliance.Tests;

/// <summary>
/// CommonMark spec 0.31.2 compliance tests for Link reference definitions.
/// Source: https://spec.commonmark.org/0.31.2/#link-reference-definitions
/// Total examples: 27
/// </summary>
public class LinkReferenceDefinitionsTests
{
    [Fact]
    public void Example_192()
    {
        var input = "[foo]: /url \"title\"\n\n[foo]";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"/url\" title=\"title\">foo</a></p>", html);
    }

    [Fact]
    public void Example_193()
    {
        var input = "   [foo]: \n      /url  \n           'the title'  \n\n[foo]";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"/url\" title=\"the title\">foo</a></p>", html);
    }

    [Fact]
    public void Example_194()
    {
        var input = "[Foo*bar\\]]:my_(url) 'title (with parens)'\n\n[Foo*bar\\]]";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"my_(url)\" title=\"title (with parens)\">Foo*bar]</a></p>", html);
    }

    [Fact]
    public void Example_195()
    {
        var input = "[Foo bar]:\n<my url>\n'title'\n\n[Foo bar]";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"my%20url\" title=\"title\">Foo bar</a></p>", html);
    }

    [Fact]
    public void Example_196()
    {
        var input = "[foo]: /url '\ntitle\nline1\nline2\n'\n\n[foo]";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"/url\" title=\"\ntitle\nline1\nline2\n\">foo</a></p>", html);
    }

    [Fact]
    public void Example_197()
    {
        var input = "[foo]: /url 'title\n\nwith blank line'\n\n[foo]";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>[foo]: /url 'title</p>\n<p>with blank line'</p>\n<p>[foo]</p>", html);
    }

    [Fact]
    public void Example_198()
    {
        var input = "[foo]:\n/url\n\n[foo]";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"/url\">foo</a></p>", html);
    }

    [Fact]
    public void Example_199()
    {
        var input = "[foo]:\n\n[foo]";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>[foo]:</p>\n<p>[foo]</p>", html);
    }

    [Fact]
    public void Example_200()
    {
        var input = "[foo]: <>\n\n[foo]";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"\">foo</a></p>", html);
    }

    [Fact]
    public void Example_201()
    {
        var input = "[foo]: <bar>(baz)\n\n[foo]";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>[foo]: <bar>(baz)</p>\n<p>[foo]</p>", html);
    }

    [Fact]
    public void Example_202()
    {
        var input = "[foo]: /url\\bar\\*baz \"foo\\\"bar\\baz\"\n\n[foo]";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"/url%5Cbar*baz\" title=\"foo&quot;bar\\baz\">foo</a></p>", html);
    }

    [Fact]
    public void Example_203()
    {
        var input = "[foo]\n\n[foo]: url";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"url\">foo</a></p>", html);
    }

    [Fact]
    public void Example_204()
    {
        var input = "[foo]\n\n[foo]: first\n[foo]: second";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"first\">foo</a></p>", html);
    }

    [Fact]
    public void Example_205()
    {
        var input = "[FOO]: /url\n\n[Foo]";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"/url\">Foo</a></p>", html);
    }

    [Fact]
    public void Example_206()
    {
        var input = "[ΑΓΩ]: /φου\n\n[αγω]";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"/%CF%86%CE%BF%CF%85\">αγω</a></p>", html);
    }

    [Fact]
    public void Example_207()
    {
        var input = "[foo]: /url";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("````````````````````````````````\n\n\nHere is another one:\n", html);
    }

    [Fact]
    public void Example_208()
    {
        var input = "[foo]: /url \"title\" ok";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>[foo]: /url &quot;title&quot; ok</p>", html);
    }

    [Fact]
    public void Example_209()
    {
        var input = "[foo]: /url\n\"title\" ok";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>&quot;title&quot; ok</p>", html);
    }

    [Fact]
    public void Example_210()
    {
        var input = "    [foo]: /url \"title\"\n\n[foo]";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<pre><code>[foo]: /url &quot;title&quot;\n</code></pre>\n<p>[foo]</p>", html);
    }

    [Fact]
    public void Example_211()
    {
        var input = "```\n[foo]: /url\n```\n\n[foo]";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<pre><code>[foo]: /url\n</code></pre>\n<p>[foo]</p>", html);
    }

    [Fact]
    public void Example_212()
    {
        var input = "Foo\n[bar]: /baz\n\n[bar]";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>Foo\n[bar]: /baz</p>\n<p>[bar]</p>", html);
    }

    [Fact]
    public void Example_213()
    {
        var input = "# [Foo]\n[foo]: /url\n> bar";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<h1><a href=\"/url\">Foo</a></h1>\n<blockquote>\n<p>bar</p>\n</blockquote>", html);
    }

    [Fact]
    public void Example_214()
    {
        var input = "[foo]: /url\nbar\n===\n[foo]";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<h1>bar</h1>\n<p><a href=\"/url\">foo</a></p>", html);
    }

    [Fact]
    public void Example_215()
    {
        var input = "[foo]: /url\n===\n[foo]";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>===\n<a href=\"/url\">foo</a></p>", html);
    }

    [Fact]
    public void Example_216()
    {
        var input = "[foo]: /foo-url \"foo\"\n[bar]: /bar-url\n  \"bar\"\n[baz]: /baz-url\n\n[foo],\n[bar],\n[baz]";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"/foo-url\" title=\"foo\">foo</a>,\n<a href=\"/bar-url\" title=\"bar\">bar</a>,\n<a href=\"/baz-url\">baz</a></p>", html);
    }

    [Fact]
    public void Example_217()
    {
        var input = "[foo]\n\n> [foo]: /url";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"/url\">foo</a></p>\n<blockquote>\n</blockquote>", html);
    }

    [Fact]
    public void Example_218()
    {
        var input = "aaa\n\nbbb";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>aaa</p>\n<p>bbb</p>", html);
    }

}
