using NTokenizers.ToHtml;

namespace NTokenizers.CommonMark.Compliance.Tests;

/// <summary>
/// CommonMark spec 0.31.2 compliance tests for Raw HTML.
/// Source: https://spec.commonmark.org/0.31.2/#raw-html
/// Total examples: 20
/// </summary>
public class RawHtmlTests
{
    [Fact]
    public void Example_613()
    {
        var input = "<a/><b2/>";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a/><b2/></p>", html);
    }

    [Fact]
    public void Example_614()
    {
        var input = "<a  /><b2\ndata=\"foo\" >";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a  /><b2\ndata=\"foo\" ></p>", html);
    }

    [Fact]
    public void Example_615()
    {
        var input = "<a foo=\"bar\" bam = 'baz <em>\"</em>'\n_boolean zoop:33=zoop:33 />";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a foo=\"bar\" bam = 'baz <em>\"</em>'\n_boolean zoop:33=zoop:33 /></p>", html);
    }

    [Fact]
    public void Example_616()
    {
        var input = "Foo <responsive-image src=\"foo.jpg\" />";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>Foo <responsive-image src=\"foo.jpg\" /></p>", html);
    }

    [Fact]
    public void Example_617()
    {
        var input = "<33> <__>";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>&lt;33&gt; &lt;__&gt;</p>", html);
    }

    [Fact]
    public void Example_618()
    {
        var input = "<a h*#ref=\"hi\">";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>&lt;a h*#ref=&quot;hi&quot;&gt;</p>", html);
    }

    [Fact]
    public void Example_619()
    {
        var input = "<a href=\"hi'> <a href=hi'>";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>&lt;a href=&quot;hi'&gt; &lt;a href=hi'&gt;</p>", html);
    }

    [Fact]
    public void Example_620()
    {
        var input = "< a><\nfoo><bar/ >\n<foo bar=baz\nbim!bop />";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>&lt; a&gt;&lt;\nfoo&gt;&lt;bar/ &gt;\n&lt;foo bar=baz\nbim!bop /&gt;</p>", html);
    }

    [Fact]
    public void Example_621()
    {
        var input = "<a href='bar'title=title>";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>&lt;a href='bar'title=title&gt;</p>", html);
    }

    [Fact]
    public void Example_622()
    {
        var input = "</a></foo >";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p></a></foo ></p>", html);
    }

    [Fact]
    public void Example_623()
    {
        var input = "</a href=\"foo\">";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>&lt;/a href=&quot;foo&quot;&gt;</p>", html);
    }

    [Fact]
    public void Example_624()
    {
        var input = "foo <!-- this is a --\ncomment - with hyphens -->";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>foo <!-- this is a --\ncomment - with hyphens --></p>", html);
    }

    [Fact]
    public void Example_625()
    {
        var input = "foo <!--> foo -->\n\nfoo <!---> foo -->";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>foo <!--> foo --&gt;</p>\n<p>foo <!---> foo --&gt;</p>", html);
    }

    [Fact]
    public void Example_626()
    {
        var input = "foo <?php echo $a; ?>";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>foo <?php echo $a; ?></p>", html);
    }

    [Fact]
    public void Example_627()
    {
        var input = "foo <!ELEMENT br EMPTY>";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>foo <!ELEMENT br EMPTY></p>", html);
    }

    [Fact]
    public void Example_628()
    {
        var input = "foo <![CDATA[>&<]]>";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>foo <![CDATA[>&<]]></p>", html);
    }

    [Fact]
    public void Example_629()
    {
        var input = "foo <a href=\"&ouml;\">";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>foo <a href=\"&ouml;\"></p>", html);
    }

    [Fact]
    public void Example_630()
    {
        var input = "foo <a href=\"\\*\">";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>foo <a href=\"\\*\"></p>", html);
    }

    [Fact]
    public void Example_631()
    {
        var input = "<a href=\"\\\"\">";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>&lt;a href=&quot;&quot;&quot;&gt;</p>", html);
    }

    [Fact]
    public void Example_632()
    {
        var input = "foo  \nbaz";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>foo<br />\nbaz</p>", html);
    }

}
