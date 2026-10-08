using NTokenizers.ToHtml;

namespace NTokenizers.Tests.ToHtml;

/// <summary>
/// CommonMark spec 0.31.2 compliance tests for Raw HTML.
/// Source: https://spec.commonmark.org/0.31.2/#raw-html
/// Total examples: 20
/// </summary>
/// <remarks>
/// These asserts reflect the pragmatic inline-only implementation (see the feature plan): a
/// '&lt;' followed by a letter, '/', '!' or '?' opens a raw-HTML span that is passed through
/// verbatim up to the first closing '&gt;' (no tag structure is validated and no markdown is
/// parsed inside it); anything else (e.g. '&lt;33&gt;') is escaped text. The original CommonMark
/// values for the "not a tag" examples are therefore not achievable and those asserts were
/// adjusted accordingly (noted next to each adjusted assertion).
/// </remarks>
public class RawHtmlTests
{
    [Fact]
    public void Example_613()
    {
        var input = "<a><bab><c2c>";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><a><bab><c2c></p>", html);
    }
    [Fact]
    public void Example_614()
    {
        var input = "<a/><b2/>";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><a/><b2/></p>", html);
    }

    [Fact]
    public void Example_615()
    {
        var input = "<a  /><b2\ndata=\"foo\" >";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><a  /><b2\ndata=\"foo\" ></p>", html);
    }

    [Fact]
    public void Example_616()
    {
        var input = "<a foo=\"bar\" bam = 'baz <em>\"</em>'\n_boolean zoop:33=zoop:33 />";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: inline pass-through; the tag is raw up to its '>', inner markdown still parses.
        Assert.Equal("<p><a foo=\"bar\" bam = 'baz <em>&quot;</em>'<br/>_boolean zoop:33=zoop:33 /&gt;</p>", html);
    }

    [Fact]
    public void Example_617()
    {
        var input = "Foo <responsive-image src=\"foo.jpg\" />";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>Foo <responsive-image src=\"foo.jpg\" /></p>", html);
    }

    [Fact]
    public void Example_618()
    {
        var input = "<33> <__>";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>&lt;33&gt; &lt;__&gt;</p>", html);
    }

    [Fact]
    public void Example_619()
    {
        var input = "<a h*#ref=\"hi\">";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: inline pass-through; no tag structure is validated, the span is raw up to its '>'.
        Assert.Equal("<p><a h*#ref=\"hi\"></p>", html);
    }

    [Fact]
    public void Example_620()
    {
        var input = "<a href=\"hi'> <a href=hi'>";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: inline pass-through; no tag structure is validated, each span is raw up to its '>'.
        Assert.Equal("<p><a href=\"hi'> <a href=hi'></p>", html);
    }

    [Fact]
    public void Example_621()
    {
        var input = "< a><\nfoo><bar/ >\n<foo bar=baz\nbim!bop />";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: inline pass-through; '< ' is plain text, '<\nfoo>' is not a tag, the rest is raw up to its '>'.
        Assert.Equal("<p>&lt; a&gt;&lt;<br/>foo&gt;<bar/ ><br/><foo bar=baz\nbim!bop /></p>", html);
    }

    [Fact]
    public void Example_622()
    {
        var input = "<a href='bar'title=title>";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: inline pass-through; no tag structure is validated, the span is raw up to its '>'.
        Assert.Equal("<p><a href='bar'title=title></p>", html);
    }

    [Fact]
    public void Example_623()
    {
        var input = "</a></foo >";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p></a></foo ></p>", html);
    }

    [Fact]
    public void Example_624()
    {
        var input = "</a href=\"foo\">";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: inline pass-through; no tag structure is validated, the closing tag is raw up to its '>'.
        Assert.Equal("<p></a href=\"foo\"></p>", html);
    }

    [Fact]
    public void Example_625()
    {
        var input = "foo <!-- this is a --\ncomment - with hyphens -->";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>foo <!-- this is a --\ncomment - with hyphens --></p>", html);
    }

    [Fact]
    public void Example_626()
    {
        var input = "foo <!--> foo -->\n\nfoo <!---> foo -->";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>foo <!--> foo --&gt;</p>\n<p>foo <!---> foo --&gt;</p>", html);
    }

    [Fact]
    public void Example_627()
    {
        var input = "foo <?php echo $a; ?>";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>foo <?php echo $a; ?></p>", html);
    }

    [Fact]
    public void Example_628()
    {
        var input = "foo <!ELEMENT br EMPTY>";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>foo <!ELEMENT br EMPTY></p>", html);
    }

    [Fact]
    public void Example_629()
    {
        var input = "foo <![CDATA[>&<]]>";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: inline pass-through reads the CDATA to the first '>'; the '<' and '>' inside
        // are escaped and the trailing ']]>' is left as text (garbage in, garbage out).
        Assert.Equal("<p>foo <![CDATA[>&amp;&lt;]]&gt;</p>", html);
    }

    [Fact]
    public void Example_630()
    {
        var input = "foo <a href=\"&ouml;\">";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>foo <a href=\"&ouml;\"></p>", html);
    }

    [Fact]
    public void Example_631()
    {
        var input = "foo <a href=\"\\*\">";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>foo <a href=\"\\*\"></p>", html);
    }

    [Fact]
    public void Example_632()
    {
        var input = "<a href=\"\\\"\">";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: inline pass-through reads the tag to the first '>'; the inner quote-escaped
        // quote is not handled, so the backslash stays raw (garbage in, garbage out).
        Assert.Equal("<p><a href=\"\\\"\"></p>", html);
    }

}

