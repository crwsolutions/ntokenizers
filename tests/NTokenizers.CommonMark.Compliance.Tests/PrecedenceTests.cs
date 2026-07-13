using NTokenizers.ToHtml;

namespace NTokenizers.CommonMark.Compliance.Tests;

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
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<ul>\n<li>`one</li>\n<li>two`</li>\n</ul>", html);
    }

}
