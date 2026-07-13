using NTokenizers.ToHtml;

namespace NTokenizers.CommonMark.Compliance.Tests;

/// <summary>
/// CommonMark spec 0.31.2 compliance tests for Blank lines.
/// Source: https://spec.commonmark.org/0.31.2/#blank-lines
/// Total examples: 1
/// </summary>
public class BlankLinesTests
{
    [Fact]
    public void Example_227()
    {
        var input = "> # Foo\n> bar\n> baz";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<blockquote>\n<h1>Foo</h1>\n<p>bar\nbaz</p>\n</blockquote>", html);
    }

}
