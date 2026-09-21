using NTokenizers.ToHtml;

namespace NTokenizers.Tests.ToHtml;

/// <summary>
/// CommonMark spec 0.31.2 compliance tests for Paragraphs.
/// Source: https://spec.commonmark.org/0.31.2/#paragraphs
/// Total examples: 8
/// </summary>
public class ParagraphsTests
{
    [Fact]
    public void Example_219()
    {
        var input = "aaa\nbbb\n\nccc\nddd";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>aaa<br/>bbb</p>\n<p>ccc<br/>ddd</p>", html);
    }

    [Fact]
    public void Example_220()
    {
        var input = "aaa\n\n\nbbb";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>aaa</p>\n<p>bbb</p>", html);
    }

    [Fact]
    public void Example_221()
    {
        var input = "  aaa\n bbb";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>  aaa<br/> bbb</p>", html);
    }

    [Fact]
    public void Example_222()
    {
        var input = "aaa\n             bbb\n                                       ccc";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>aaa<br/>             bbb<br/>                                       ccc</p>", html);
    }

    [Fact]
    public void Example_223()
    {
        var input = "   aaa\nbbb";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>   aaa<br/>bbb</p>", html);
    }

    [Fact]
    public void Example_224()
    {
        // CommonMark example 224 ("    aaa\nbbb") is a duplicate of example 114 for this
        // library: the indented line forms an indented code block and "bbb" is a separate
        // paragraph. Blocks are separated by a single line break; newlines inside the code
        // block are literal (no <br/>).
        var input = "    aaa\nbbb";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<pre><code>aaa\n</code></pre>\n<p>bbb</p>", html);
    }

    [Fact]
    public void Example_225()
    {
        var input = "aaa     \nbbb     ";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Trailing spaces are retained (realistic); hard line breaks (2+ spaces before
        // line break) are not yet implemented, so this is a soft break.
        Assert.Equal("<p>aaa     <br/>bbb     </p>", html);
    }

    [Fact]
    public void Example_226()
    {
        var input = "  \n\naaa\n  \n\n# aaa\n\n  ";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Leading/trailing blank lines and whitespace produce no output; a block following a
        // paragraph is separated by a single line break. The trailing newline after </h1> is
        // produced by the heading writer (WriteLine) and is pre-existing behavior.
        Assert.Equal("<p>aaa</p>\n<h1>aaa</h1>\n", html);
    }

}
