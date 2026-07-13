using NTokenizers.ToHtml;

namespace NTokenizers.CommonMark.Compliance.Tests;

/// <summary>
/// CommonMark spec 0.31.2 compliance tests for Entity and numeric character references.
/// Source: https://spec.commonmark.org/0.31.2/#entity-and-numeric-character-references
/// Total examples: 17
/// </summary>
public class EntityReferencesTests
{
    [Fact]
    public void Example_025()
    {
        var input = "&nbsp; &amp; &copy; &AElig; &Dcaron;\n&frac34; &HilbertSpace; &DifferentialD;\n&ClockwiseContourIntegral; &ngE;";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>  &amp; © Æ Ď\n¾ ℋ ⅆ\n∲ ≧̸</p>", html);
    }

    [Fact]
    public void Example_026()
    {
        var input = "&#35; &#1234; &#992; &#0;";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p># Ӓ Ϡ �</p>", html);
    }

    [Fact]
    public void Example_027()
    {
        var input = "&#X22; &#XD06; &#xcab;";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>&quot; ആ ಫ</p>", html);
    }

    [Fact]
    public void Example_028()
    {
        var input = "&nbsp &x; &#; &#x;\n&#87654321;\n&#abcdef0;\n&ThisIsNotDefined; &hi?;";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>&amp;nbsp &amp;x; &amp;#; &amp;#x;\n&amp;#87654321;\n&amp;#abcdef0;\n&amp;ThisIsNotDefined; &amp;hi?;</p>", html);
    }

    [Fact]
    public void Example_029()
    {
        var input = "&copy";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>&amp;copy</p>", html);
    }

    [Fact]
    public void Example_030()
    {
        var input = "&MadeUpEntity;";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>&amp;MadeUpEntity;</p>", html);
    }

    [Fact]
    public void Example_031()
    {
        var input = "<a href=\"&ouml;&ouml;.html\">";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<a href=\"&ouml;&ouml;.html\">", html);
    }

    [Fact]
    public void Example_032()
    {
        var input = "[foo](/f&ouml;&ouml; \"f&ouml;&ouml;\")";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"/f%C3%B6%C3%B6\" title=\"föö\">foo</a></p>", html);
    }

    [Fact]
    public void Example_033()
    {
        var input = "[foo]\n\n[foo]: /f&ouml;&ouml; \"f&ouml;&ouml;\"";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"/f%C3%B6%C3%B6\" title=\"föö\">foo</a></p>", html);
    }

    [Fact]
    public void Example_034()
    {
        var input = "``` f&ouml;&ouml;\nfoo\n```";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<pre><code class=\"language-föö\">foo\n</code></pre>", html);
    }

    [Fact]
    public void Example_035()
    {
        var input = "`f&ouml;&ouml;`";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><code>f&amp;ouml;&amp;ouml;</code></p>", html);
    }

    [Fact]
    public void Example_036()
    {
        var input = "    f&ouml;f&ouml;";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<pre><code>f&amp;ouml;f&amp;ouml;\n</code></pre>", html);
    }

    [Fact]
    public void Example_037()
    {
        var input = "&#42;foo&#42;\n*foo*";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>*foo*\n<em>foo</em></p>", html);
    }

    [Fact]
    public void Example_038()
    {
        var input = "&#42; foo\n\n* foo";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>* foo</p>\n<ul>\n<li>foo</li>\n</ul>", html);
    }

    [Fact]
    public void Example_039()
    {
        var input = "foo&#10;&#10;bar";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>foo\n\nbar</p>", html);
    }

    [Fact]
    public void Example_040()
    {
        var input = "&#9;foo";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>\tfoo</p>", html);
    }

    [Fact]
    public void Example_041()
    {
        var input = "[a](url &quot;tit&quot;)";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>[a](url &quot;tit&quot;)</p>", html);
    }

}
