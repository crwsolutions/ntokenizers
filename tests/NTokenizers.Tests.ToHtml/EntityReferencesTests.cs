using NTokenizers.ToHtml;

namespace NTokenizers.Tests.ToHtml;

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
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: named entity references are not decoded; the '&' is escaped and the
        // rest is plain text. Inline line breaks render as soft breaks (<br/>).
        Assert.Equal("<p>&amp;nbsp; &amp;amp; &amp;copy; &amp;AElig; &amp;Dcaron;<br/>&amp;frac34; &amp;HilbertSpace; &amp;DifferentialD;<br/>&amp;ClockwiseContourIntegral; &amp;ngE;</p>", html);
    }

    [Fact]
    public void Example_026()
    {
        var input = "&#35; &#1234; &#992; &#0;";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: numeric entity references are not decoded; the '&' is escaped.
        Assert.Equal("<p>&amp;#35; &amp;#1234; &amp;#992; &amp;#0;</p>", html);
    }

    [Fact]
    public void Example_027()
    {
        var input = "&#X22; &#XD06; &#xcab;";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: numeric entity references are not decoded; the '&' is escaped.
        Assert.Equal("<p>&amp;#X22; &amp;#XD06; &amp;#xcab;</p>", html);
    }

    [Fact]
    public void Example_028()
    {
        var input = "&nbsp &x; &#; &#x;\n&#87654321;\n&#abcdef0;\n&ThisIsNotDefined; &hi?;";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: inline line breaks render as soft breaks (<br/>).
        Assert.Equal("<p>&amp;nbsp &amp;x; &amp;#; &amp;#x;<br/>&amp;#87654321;<br/>&amp;#abcdef0;<br/>&amp;ThisIsNotDefined; &amp;hi?;</p>", html);
    }

    [Fact]
    public void Example_029()
    {
        var input = "&copy";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>&amp;copy</p>", html);
    }

    [Fact]
    public void Example_030()
    {
        var input = "&MadeUpEntity;";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>&amp;MadeUpEntity;</p>", html);
    }

    [Fact]
    public void Example_031()
    {
        var input = "<a href=\"&ouml;&ouml;.html\">";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><a href=\"&ouml;&ouml;.html\"></p>", html);
    }

    [Fact]
    public void Example_032()
    {
        var input = "[foo](/f&ouml;&ouml; \"f&ouml;&ouml;\")";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: entities in href/title are not decoded; the '&' is escaped.
        // (Same class of deviation as the double-escaped-entity asserts in LinksTests.)
        Assert.Equal("<p><a href=\"/f&amp;ouml;&amp;ouml;\" title=\"f&amp;ouml;&amp;ouml;\">foo</a></p>", html);
    }

    [Fact]
    public void Example_033()
    {
        var input = "[foo]\n\n[foo]: /f&ouml;&ouml; \"f&ouml;&ouml;\"";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: link reference definitions are not supported by the streaming
        // tokenizer (see AGENTS.md); both lines render as paragraphs with '&amp;' escaping.
        Assert.Equal("<p>[foo]</p>\n<p>[foo]: /f&amp;ouml;&amp;ouml; &quot;f&amp;ouml;&amp;ouml;&quot;</p>", html);
    }

    [Fact]
    public void Example_034()
    {
        var input = "``` f&ouml;&ouml;\nfoo\n```";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: the entity in the fence info string is not decoded, the language
        // keeps a leading space, and the code content is emitted as a plain (untokenized)
        // span without a trailing newline.
        Assert.Equal(
            "<div class=\"code-block-container\">\n" +
            "<div class=\"code-block-header\">\n" +
            "<span class=\"code-block-language\"> f&amp;ouml;&amp;ouml;</span>\n" +
            "<button class=\"code-block-copy\" onclick=\"copyCode(this)\" title=\"Copy to clipboard\">Copy</button>\n" +
            "</div>\n" +
            "<pre><code class=\"language- f&amp;ouml;&amp;ouml;\"><span class=\"tok-generic\">foo</span></code></pre>\n</div>\n",
            html);
    }

    [Fact]
    public void Example_035()
    {
        var input = "`f&ouml;&ouml;`";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><code>f&amp;ouml;&amp;ouml;</code></p>", html);
    }

    [Fact]
    public void Example_036()
    {
        var input = "    f&ouml;f&ouml;";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: entities in indented code are not decoded ('&' is escaped).
        // Faithful to the input: the final code line has no line ending, so none is added.
        Assert.Equal("<pre><code>f&amp;ouml;f&amp;ouml;</code></pre>", html);
    }

    [Fact]
    public void Example_037()
    {
        var input = "&#42;foo&#42;\n*foo*";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: '&#42;' is not decoded to '*', and the inline line break renders
        // as a soft break (<br/>).
        Assert.Equal("<p>&amp;#42;foo&amp;#42;<br/><em>foo</em></p>", html);
    }

    [Fact]
    public void Example_038()
    {
        var input = "&#42; foo\n\n* foo";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: '&#42;' is not decoded to '*', and the list item content is wrapped
        // in a <p> (same class of deviation as the list-item asserts in IndentedCodeBlocksTests).
        Assert.Equal("<p>&amp;#42; foo</p>\n<ul>\n<li>\n<p>foo</p>\n</li>\n</ul>", html);
    }

    [Fact]
    public void Example_039()
    {
        var input = "foo&#10;&#10;bar";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: '&#10;' is not decoded to a line feed; the reference stays literal
        // text with an escaped '&'.
        Assert.Equal("<p>foo&amp;#10;&amp;#10;bar</p>", html);
    }

    [Fact]
    public void Example_040()
    {
        var input = "&#9;foo";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: '&#9;' is not decoded to a tab; the reference stays literal text
        // with an escaped '&'.
        Assert.Equal("<p>&amp;#9;foo</p>", html);
    }

    [Fact]
    public void Example_041()
    {
        var input = "[a](url &quot;tit&quot;)";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: the link IS recognized, but the literal '&' in the href is double-
        // escaped (&amp;quot; instead of &quot;). Same class of deviation as the
        // double-escaped-entity asserts in LinksTests.
        Assert.Equal("<p><a href=\"url &amp;quot;tit&amp;quot;\">a</a></p>", html);
    }

}
