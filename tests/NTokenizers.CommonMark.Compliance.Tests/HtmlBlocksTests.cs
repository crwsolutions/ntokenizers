using NTokenizers.ToHtml;

namespace NTokenizers.CommonMark.Compliance.Tests;

/// <summary>
/// CommonMark spec 0.31.2 compliance tests for HTML blocks.
/// Source: https://spec.commonmark.org/0.31.2/#html-blocks
/// Total examples: 44
/// </summary>
public class HtmlBlocksTests
{
    [Fact]
    public void Example_148()
    {
        var input = "<table><tr><td>\n<pre>\n**Hello**,\n\n_world_.\n</pre>\n</td></tr></table>";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<table><tr><td>\n<pre>\n**Hello**,\n<p><em>world</em>.\n</pre></p>\n</td></tr></table>", html);
    }

    [Fact]
    public void Example_149()
    {
        var input = "<table>\n  <tr>\n    <td>\n           hi\n    </td>\n  </tr>\n</table>\n\nokay.";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<table>\n  <tr>\n    <td>\n           hi\n    </td>\n  </tr>\n</table>\n<p>okay.</p>", html);
    }

    [Fact]
    public void Example_150()
    {
        var input = " <div>\n  *hello*\n         <foo><a>";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal(" <div>\n  *hello*\n         <foo><a>", html);
    }

    [Fact]
    public void Example_151()
    {
        var input = "</div>\n*foo*";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("</div>\n*foo*", html);
    }

    [Fact]
    public void Example_152()
    {
        var input = "<DIV CLASS=\"foo\">\n\n*Markdown*\n\n</DIV>";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<DIV CLASS=\"foo\">\n<p><em>Markdown</em></p>\n</DIV>", html);
    }

    [Fact]
    public void Example_153()
    {
        var input = "<div id=\"foo\"\n  class=\"bar\">\n</div>";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<div id=\"foo\"\n  class=\"bar\">\n</div>", html);
    }

    [Fact]
    public void Example_154()
    {
        var input = "<div id=\"foo\" class=\"bar\n  baz\">\n</div>";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<div id=\"foo\" class=\"bar\n  baz\">\n</div>", html);
    }

    [Fact]
    public void Example_155()
    {
        var input = "<div>\n*foo*\n\n*bar*";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<div>\n*foo*\n<p><em>bar</em></p>", html);
    }

    [Fact]
    public void Example_156()
    {
        var input = "<div id=\"foo\"\n*hi*";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<div id=\"foo\"\n*hi*", html);
    }

    [Fact]
    public void Example_157()
    {
        var input = "<div class\nfoo";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<div class\nfoo", html);
    }

    [Fact]
    public void Example_158()
    {
        var input = "<div *???-&&&-<---\n*foo*";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<div *???-&&&-<---\n*foo*", html);
    }

    [Fact]
    public void Example_159()
    {
        var input = "<div><a href=\"bar\">*foo*</a></div>";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<div><a href=\"bar\">*foo*</a></div>", html);
    }

    [Fact]
    public void Example_160()
    {
        var input = "<table><tr><td>\nfoo\n</td></tr></table>";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<table><tr><td>\nfoo\n</td></tr></table>", html);
    }

    [Fact]
    public void Example_161()
    {
        var input = "<div></div>\n``` c\nint x = 33;\n```";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<div></div>\n``` c\nint x = 33;\n```", html);
    }

    [Fact]
    public void Example_162()
    {
        var input = "<a href=\"foo\">\n*bar*\n</a>";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<a href=\"foo\">\n*bar*\n</a>", html);
    }

    [Fact]
    public void Example_163()
    {
        var input = "<Warning>\n*bar*\n</Warning>";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<Warning>\n*bar*\n</Warning>", html);
    }

    [Fact]
    public void Example_164()
    {
        var input = "<i class=\"foo\">\n*bar*\n</i>";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<i class=\"foo\">\n*bar*\n</i>", html);
    }

    [Fact]
    public void Example_165()
    {
        var input = "</ins>\n*bar*";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("</ins>\n*bar*", html);
    }

    [Fact]
    public void Example_166()
    {
        var input = "<del>\n*foo*\n</del>";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<del>\n*foo*\n</del>", html);
    }

    [Fact]
    public void Example_167()
    {
        var input = "<del>\n\n*foo*\n\n</del>";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<del>\n<p><em>foo</em></p>\n</del>", html);
    }

    [Fact]
    public void Example_168()
    {
        var input = "<del>*foo*</del>";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><del><em>foo</em></del></p>", html);
    }

    [Fact]
    public void Example_169()
    {
        var input = "<pre language=\"haskell\"><code>\nimport Text.HTML.TagSoup\n\nmain :: IO ()\nmain = print $ parseTags tags\n</code></pre>\nokay";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<pre language=\"haskell\"><code>\nimport Text.HTML.TagSoup\n\nmain :: IO ()\nmain = print $ parseTags tags\n</code></pre>\n<p>okay</p>", html);
    }

    [Fact]
    public void Example_170()
    {
        var input = "<script type=\"text/javascript\">\n// JavaScript example\n\ndocument.getElementById(\"demo\").innerHTML = \"Hello JavaScript!\";\n</script>\nokay";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<script type=\"text/javascript\">\n// JavaScript example\n\ndocument.getElementById(\"demo\").innerHTML = \"Hello JavaScript!\";\n</script>\n<p>okay</p>", html);
    }

    [Fact]
    public void Example_171()
    {
        var input = "<textarea>\n\n*foo*\n\n_bar_\n\n</textarea>";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<textarea>\n\n*foo*\n\n_bar_\n\n</textarea>", html);
    }

    [Fact]
    public void Example_172()
    {
        var input = "<style\n  type=\"text/css\">\nh1 {color:red;}\n\np {color:blue;}\n</style>\nokay";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<style\n  type=\"text/css\">\nh1 {color:red;}\n\np {color:blue;}\n</style>\n<p>okay</p>", html);
    }

    [Fact]
    public void Example_173()
    {
        var input = "<style\n  type=\"text/css\">\n\nfoo";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<style\n  type=\"text/css\">\n\nfoo", html);
    }

    [Fact]
    public void Example_174()
    {
        var input = "> <div>\n> foo\n\nbar";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<blockquote>\n<div>\nfoo\n</blockquote>\n<p>bar</p>", html);
    }

    [Fact]
    public void Example_175()
    {
        var input = "- <div>\n- foo";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<ul>\n<li>\n<div>\n</li>\n<li>foo</li>\n</ul>", html);
    }

    [Fact]
    public void Example_176()
    {
        var input = "<style>p{color:red;}</style>\n*foo*";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<style>p{color:red;}</style>\n<p><em>foo</em></p>", html);
    }

    [Fact]
    public void Example_177()
    {
        var input = "<!-- foo -->*bar*\n*baz*";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<!-- foo -->*bar*\n<p><em>baz</em></p>", html);
    }

    [Fact]
    public void Example_178()
    {
        var input = "<script>\nfoo\n</script>1. *bar*";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<script>\nfoo\n</script>1. *bar*", html);
    }

    [Fact]
    public void Example_179()
    {
        var input = "<!-- Foo\n\nbar\n   baz -->\nokay";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<!-- Foo\n\nbar\n   baz -->\n<p>okay</p>", html);
    }

    [Fact]
    public void Example_180()
    {
        var input = "<?php\n\n  echo '>';\n\n?>\nokay";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<?php\n\n  echo '>';\n\n?>\n<p>okay</p>", html);
    }

    [Fact]
    public void Example_181()
    {
        var input = "<!DOCTYPE html>";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<!DOCTYPE html>", html);
    }

    [Fact]
    public void Example_182()
    {
        var input = "<![CDATA[\nfunction matchwo(a,b)\n{\n  if (a < b && a < 0) then {\n    return 1;\n\n  } else {\n\n    return 0;\n  }\n}\n]]>\nokay";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<![CDATA[\nfunction matchwo(a,b)\n{\n  if (a < b && a < 0) then {\n    return 1;\n\n  } else {\n\n    return 0;\n  }\n}\n]]>\n<p>okay</p>", html);
    }

    [Fact]
    public void Example_183()
    {
        var input = "  <!-- foo -->\n\n    <!-- foo -->";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("  <!-- foo -->\n<pre><code>&lt;!-- foo --&gt;\n</code></pre>", html);
    }

    [Fact]
    public void Example_184()
    {
        var input = "  <div>\n\n    <div>";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("  <div>\n<pre><code>&lt;div&gt;\n</code></pre>", html);
    }

    [Fact]
    public void Example_185()
    {
        var input = "Foo\n<div>\nbar\n</div>";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>Foo</p>\n<div>\nbar\n</div>", html);
    }

    [Fact]
    public void Example_186()
    {
        var input = "<div>\nbar\n</div>\n*foo*";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<div>\nbar\n</div>\n*foo*", html);
    }

    [Fact]
    public void Example_187()
    {
        var input = "Foo\n<a href=\"bar\">\nbaz";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>Foo\n<a href=\"bar\">\nbaz</p>", html);
    }

    [Fact]
    public void Example_188()
    {
        var input = "<div>\n\n*Emphasized* text.\n\n</div>";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<div>\n<p><em>Emphasized</em> text.</p>\n</div>", html);
    }

    [Fact]
    public void Example_189()
    {
        var input = "<div>\n*Emphasized* text.\n</div>";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<div>\n*Emphasized* text.\n</div>", html);
    }

    [Fact]
    public void Example_190()
    {
        var input = "<table>\n\n<tr>\n\n<td>\nHi\n</td>\n\n</tr>\n\n</table>";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<table>\n<tr>\n<td>\nHi\n</td>\n</tr>\n</table>", html);
    }

    [Fact]
    public void Example_191()
    {
        var input = "<table>\n\n  <tr>\n\n    <td>\n      Hi\n    </td>\n\n  </tr>\n\n</table>";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<table>\n  <tr>\n<pre><code>&lt;td&gt;\n  Hi\n&lt;/td&gt;\n</code></pre>\n  </tr>\n</table>", html);
    }

}
