using NTokenizers.ToHtml;

namespace NTokenizers.Tests.ToHtml;

/// <summary>
/// CommonMark spec 0.31.2 compliance tests for Precedence.
/// Source: https://spec.commonmark.org/0.31.2/#precedence
/// Total examples: 1
/// </summary>
public class PrecedenceTests
{
    [Fact]
    public void Example_042()
    {
        var input = "- `one\n- two`";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<ul>\n<li>\n<p>`one<br/></p>\n</li>\n<li>\n<p>two`</p>\n</li>\n</ul>", html);
    }

}
