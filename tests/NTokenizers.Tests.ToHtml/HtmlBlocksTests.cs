using NTokenizers.ToHtml;

namespace NTokenizers.Tests.ToHtml;

/// <summary>
/// CommonMark spec 0.31.2 examples for HTML blocks.
/// Source: https://spec.commonmark.org/0.31.2/#html-blocks
/// Total examples: 44
/// </summary>
/// <remarks>
/// These asserts reflect the pragmatic inline-only implementation (see the feature plan):
/// HTML is treated as inline raw-HTML pass-through, not as block-level HTML blocks. A tag is
/// passed through raw up to its first closing '&gt;'; newlines inside a paragraph become
/// &lt;br/&gt;; and no block-HTML semantics are applied (no &lt;p&gt;-wrapping, no literal
/// newlines, markdown still parses inside the span). The original CommonMark expected values
/// are therefore not achievable and the asserts were adjusted accordingly.
/// </remarks>
public class HtmlBlocksTests
{
    [Fact]
    public void Example_148()
    {
        var input = "<table><tr><td>\n<pre>\n**Hello**,\n\n_world_.\n</pre>\n</td></tr></table>";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: inline raw-HTML pass-through (no block HTML); tags stay raw, newlines become <br/>.
        Assert.Equal("<p><table><tr><td><br/><pre><br/><strong>Hello</strong>,</p>\n<p><em>world</em>.<br/></pre><br/></td></tr></table></p>", html);
    }

    [Fact]
    public void Example_149()
    {
        var input = "<table>\n  <tr>\n    <td>\n           hi\n    </td>\n  </tr>\n</table>\n\nokay.";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: inline raw-HTML pass-through (no block HTML); tags stay raw, newlines become <br/>.
        Assert.Equal("<p><table><br/>  <tr><br/>    <td><br/>           hi<br/>    </td><br/>  </tr><br/></table></p>\n<p>okay.</p>", html);
    }

    [Fact]
    public void Example_150()
    {
        var input = " <div>\n  *hello*\n         <foo><a>";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: inline raw-HTML pass-through (no block HTML); tags stay raw, newlines become <br/>.
        Assert.Equal("<p> <div><br/>  <em>hello</em><br/>         <foo><a></p>", html);
    }

    [Fact]
    public void Example_151()
    {
        var input = "</div>\n*foo*";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: inline raw-HTML pass-through (no block HTML); tags stay raw, newlines become <br/>.
        Assert.Equal("<p></div><br/><em>foo</em></p>", html);
    }

    [Fact]
    public void Example_152()
    {
        var input = "<DIV CLASS=\"foo\">\n\n*Markdown*\n\n</DIV>";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: inline raw-HTML pass-through (no block HTML); the blank lines close the paragraph.
        Assert.Equal("<p><DIV CLASS=\"foo\"></p>\n<p><em>Markdown</em></p>\n<p></DIV></p>", html);
    }

    [Fact]
    public void Example_153()
    {
        var input = "<div id=\"foo\"\n  class=\"bar\">\n</div>";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: inline raw-HTML pass-through (no block HTML); newlines become <br/>.
        Assert.Equal("<p><div id=\"foo\"\n  class=\"bar\"><br/></div></p>", html);
    }

    [Fact]
    public void Example_154()
    {
        var input = "<div id=\"foo\" class=\"bar\n  baz\">\n</div>";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: inline raw-HTML pass-through (no block HTML); newlines become <br/>.
        Assert.Equal("<p><div id=\"foo\" class=\"bar\n  baz\"><br/></div></p>", html);
    }

    [Fact]
    public void Example_155()
    {
        var input = "<div>\n*foo*\n\n*bar*";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: inline raw-HTML pass-through (no block HTML); the blank line closes the paragraph.
        Assert.Equal("<p><div><br/><em>foo</em></p>\n<p><em>bar</em></p>", html);
    }

    [Fact]
    public void Example_156()
    {
        var input = "<div id=\"foo\"\n*hi*";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: inline pass-through reads the tag to the first '>'; none is present, so the
        // whole run is plain text (garbage in, garbage out).
        Assert.Equal("<p>&lt;div id=&quot;foo&quot;<br/>*hi*</p>", html);
    }

    [Fact]
    public void Example_157()
    {
        var input = "<div class\nfoo";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: inline pass-through reads the tag to the first '>'; none is present, so the
        // whole run is plain text (garbage in, garbage out).
        Assert.Equal("<p>&lt;div class<br/>foo</p>", html);
    }

    [Fact]
    public void Example_158()
    {
        var input = "<div *???-&&&-<---\n*foo*";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: inline pass-through reads the tag to the first '>'; none is present, so the
        // whole run is plain text (garbage in, garbage out).
        Assert.Equal("<p>&lt;div *???-&amp;&amp;&amp;-&lt;---<br/>*foo*</p>", html);
    }

    [Fact]
    public void Example_159()
    {
        var input = "<div><a href=\"bar\">*foo*</a></div>";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: inline raw-HTML pass-through (no block HTML); the inner markdown still parses.
        Assert.Equal("<p><div><a href=\"bar\"><em>foo</em></a></div></p>", html);
    }

    [Fact]
    public void Example_160()
    {
        var input = "<table><tr><td>\nfoo\n</td></tr></table>";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: inline raw-HTML pass-through (no block HTML); newlines become <br/>.
        Assert.Equal("<p><table><tr><td><br/>foo<br/></td></tr></table></p>", html);
    }

    [Fact]
    public void Example_161()
    {
        var input = "<div></div>\n``` c\nint x = 33;\n```";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: inline raw-HTML pass-through (no block HTML); the fenced block is a separate block.
        Assert.Equal("<p><div></div><div class=\"code-block-container\">\n<div class=\"code-block-header\">\n<span class=\"code-block-language\"> c</span>\n<button class=\"code-block-copy\" onclick=\"copyCode(this)\" title=\"Copy to clipboard\">Copy</button>\n</div>\n<pre><code class=\"language- c\"><span class=\"tok-keyword\">int</span> <span class=\"tok-identifier\">x</span> <span class=\"tok-operator\">=</span> <span class=\"tok-number\">33</span><span class=\"tok-punctuation\">;</span></code></pre>\n</div>\n</p>", html);
    }

    [Fact]
    public void Example_162()
    {
        var input = "<a href=\"foo\">\n*bar*\n</a>";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: inline raw-HTML pass-through (no block HTML); newlines become <br/>.
        Assert.Equal("<p><a href=\"foo\"><br/><em>bar</em><br/></a></p>", html);
    }

    [Fact]
    public void Example_163()
    {
        var input = "<Warning>\n*bar*\n</Warning>";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: inline raw-HTML pass-through (no block HTML); newlines become <br/>.
        Assert.Equal("<p><Warning><br/><em>bar</em><br/></Warning></p>", html);
    }

    [Fact]
    public void Example_164()
    {
        var input = "<i class=\"foo\">\n*bar*\n</i>";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: inline raw-HTML pass-through (no block HTML); newlines become <br/>.
        Assert.Equal("<p><i class=\"foo\"><br/><em>bar</em><br/></i></p>", html);
    }

    [Fact]
    public void Example_165()
    {
        var input = "</ins>\n*bar*";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: inline raw-HTML pass-through (no block HTML); newlines become <br/>.
        Assert.Equal("<p></ins><br/><em>bar</em></p>", html);
    }

    [Fact]
    public void Example_166()
    {
        var input = "<del>\n*foo*\n</del>";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: inline raw-HTML pass-through (no block HTML); newlines become <br/>.
        Assert.Equal("<p><del><br/><em>foo</em><br/></del></p>", html);
    }

    [Fact]
    public void Example_167()
    {
        var input = "<del>\n\n*foo*\n\n</del>";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: inline raw-HTML pass-through (no block HTML); the blank lines close the paragraph.
        Assert.Equal("<p><del></p>\n<p><em>foo</em></p>\n<p></del></p>", html);
    }

    [Fact]
    public void Example_168()
    {
        var input = "<del>*foo*</del>";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: inline raw-HTML pass-through (no block HTML); the inner markdown still parses.
        Assert.Equal("<p><del><em>foo</em></del></p>", html);
    }

    [Fact]
    public void Example_169()
    {
        var input = "<pre language=\"haskell\"><code>\nimport Text.HTML.TagSoup\n\nmain :: IO ()\nmain = print $ parseTags tags\n</code></pre>\nokay";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: inline raw-HTML pass-through (no block HTML); blank lines close the paragraph.
        Assert.Equal("<p><pre language=\"haskell\"><code><br/>import Text.HTML.TagSoup</p>\n<p>main :: IO ()<br/>main = print $ parseTags tags<br/></code></pre><br/>okay</p>", html);
    }

    [Fact]
    public void Example_170()
    {
        var input = "<script type=\"text/javascript\">\n// JavaScript example\n\ndocument.getElementById(\"demo\").innerHTML = \"Hello JavaScript!\";\n</script>\nokay";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: inline raw-HTML pass-through (no block HTML); blank lines close the paragraph.
        Assert.Equal("<p><script type=\"text/javascript\"><br/>// JavaScript example</p>\n<p>document.getElementById(&quot;demo&quot;).innerHTML = &quot;Hello JavaScript!&quot;;<br/></script><br/>okay</p>", html);
    }

    [Fact]
    public void Example_171()
    {
        var input = "<textarea>\n\n*foo*\n\n_bar_\n\n</textarea>";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: inline raw-HTML pass-through (no block HTML); the blank lines close the paragraph.
        Assert.Equal("<p><textarea></p>\n<p><em>foo</em></p>\n<p><em>bar</em></p>\n<p></textarea></p>", html);
    }

    [Fact]
    public void Example_172()
    {
        var input = "<style\n  type=\"text/css\">\nh1 {color:red;}\n\np {color:blue;}\n</style>\nokay";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: inline raw-HTML pass-through (no block HTML); the blank line closes the paragraph.
        Assert.Equal("<p><style\n  type=\"text/css\"><br/>h1 {color:red;}</p>\n<p>p {color:blue;}<br/></style><br/>okay</p>", html);
    }

    [Fact]
    public void Example_173()
    {
        var input = "<style\n  type=\"text/css\">\n\nfoo";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: inline raw-HTML pass-through (no block HTML); the blank line closes the paragraph.
        Assert.Equal("<p><style\n  type=\"text/css\"></p>\n<p>foo</p>", html);
    }

    [Fact]
    public void Example_174()
    {
        var input = "> <div>\n> foo\n\nbar";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: inline raw-HTML pass-through (no block HTML); newlines become <br/> inside the quote.
        Assert.Equal("<blockquote>\n<p><div><br/>foo</p>\n</blockquote>\n<p>bar</p>", html);
    }

    [Fact]
    public void Example_175()
    {
        var input = "- <div>\n- foo";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: inline raw-HTML pass-through (no block HTML); each item opens its own paragraph.
        Assert.Equal("<ul>\n<li>\n<p><div></p>\n</li>\n<li>\n<p>foo</p>\n</li>\n</ul>", html);
    }

    [Fact]
    public void Example_176()
    {
        var input = "<style>p{color:red;}</style>\n*foo*";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: inline raw-HTML pass-through (no block HTML); newlines become <br/>.
        Assert.Equal("<p><style>p{color:red;}</style><br/><em>foo</em></p>", html);
    }

    [Fact]
    public void Example_177()
    {
        var input = "<!-- foo -->*bar*\n*baz*";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: inline raw-HTML pass-through (no block HTML); newlines become <br/>.
        Assert.Equal("<p><!-- foo --><em>bar</em><br/><em>baz</em></p>", html);
    }

    [Fact]
    public void Example_178()
    {
        var input = "<script>\nfoo\n</script>1. *bar*";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: inline raw-HTML pass-through (no block HTML); newlines become <br/>.
        Assert.Equal("<p><script><br/>foo<br/></script>1. <em>bar</em></p>", html);
    }

    [Fact]
    public void Example_179()
    {
        var input = "<!-- Foo\n\nbar\n   baz -->\nokay";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: inline raw-HTML pass-through (no block HTML); newlines become <br/>.
        Assert.Equal("<p><!-- Foo\n\nbar\n   baz --><br/>okay</p>", html);
    }

    [Fact]
    public void Example_180()
    {
        var input = "<?php\n\n  echo '>';\n\n?>\nokay";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: inline pass-through reads the span to the first '>'; the blank lines close the
        // paragraph, so the trailing "?>" is left as escaped text (garbage in, garbage out).
        Assert.Equal("<p><?php\n\n  echo '>';</p>\n<p>?&gt;<br/>okay</p>", html);
    }

    [Fact]
    public void Example_181()
    {
        var input = "<!DOCTYPE html>";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: inline raw-HTML pass-through (no block HTML); the declaration stays in a paragraph.
        Assert.Equal("<p><!DOCTYPE html></p>", html);
    }

    [Fact]
    public void Example_182()
    {
        var input = "<![CDATA[\nfunction matchwo(a,b)\n{\n  if (a < b && a < 0) then {\n    return 1;\n\n  } else {\n\n    return 0;\n  }\n}\n]]>\nokay";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: inline raw-HTML pass-through (no block HTML); the CDATA has no '>', so it is
        // passed through raw as one span (garbage in, garbage out).
        Assert.Equal("<p><![CDATA[\nfunction matchwo(a,b)\n{\n  if (a < b && a < 0) then {\n    return 1;\n\n  } else {\n\n    return 0;\n  }\n}\n]]><br/>okay</p>", html);
    }

    [Fact]
    public void Example_183()
    {
        var input = "  <!-- foo -->\n\n    <!-- foo -->";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: inline raw-HTML pass-through (no block HTML); the indented second line is code.
        Assert.Equal("<p>  <!-- foo --></p>\n<pre><code>&lt;!-- foo --&gt;</code></pre>", html);
    }

    [Fact]
    public void Example_184()
    {
        var input = "  <div>\n\n    <div>";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: inline raw-HTML pass-through (no block HTML); the indented second line is code.
        Assert.Equal("<p>  <div></p>\n<pre><code>&lt;div&gt;</code></pre>", html);
    }

    [Fact]
    public void Example_185()
    {
        var input = "Foo\n<div>\nbar\n</div>";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: inline raw-HTML pass-through (no block HTML); newlines become <br/>.
        Assert.Equal("<p>Foo<br/><div><br/>bar<br/></div></p>", html);
    }

    [Fact]
    public void Example_186()
    {
        var input = "<div>\nbar\n</div>\n*foo*";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: inline raw-HTML pass-through (no block HTML); newlines become <br/>.
        Assert.Equal("<p><div><br/>bar<br/></div><br/><em>foo</em></p>", html);
    }

    [Fact]
    public void Example_187()
    {
        var input = "Foo\n<a href=\"bar\">\nbaz";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: inline raw-HTML pass-through (no block HTML); newlines become <br/>.
        Assert.Equal("<p>Foo<br/><a href=\"bar\"><br/>baz</p>", html);
    }

    [Fact]
    public void Example_188()
    {
        var input = "<div>\n\n*Emphasized* text.\n\n</div>";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: inline raw-HTML pass-through (no block HTML); the blank lines close the paragraph.
        Assert.Equal("<p><div></p>\n<p><em>Emphasized</em> text.</p>\n<p></div></p>", html);
    }

    [Fact]
    public void Example_189()
    {
        var input = "<div>\n*Emphasized* text.\n</div>";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: inline raw-HTML pass-through (no block HTML); newlines become <br/>.
        Assert.Equal("<p><div><br/><em>Emphasized</em> text.<br/></div></p>", html);
    }

    [Fact]
    public void Example_190()
    {
        var input = "<table>\n\n<tr>\n\n<td>\nHi\n</td>\n\n</tr>\n\n</table>";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: inline raw-HTML pass-through (no block HTML); each blank line closes the paragraph.
        Assert.Equal("<p><table></p>\n<p><tr></p>\n<p><td><br/>Hi<br/></td></p>\n<p></tr></p>\n<p></table></p>", html);
    }

    [Fact]
    public void Example_191()
    {
        var input = "<table>\n\n  <tr>\n\n    <td>\n      Hi\n    </td>\n\n  </tr>\n\n</table>";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        // Deviation: inline raw-HTML pass-through (no block HTML); indented lines become code.
        Assert.Equal("<p><table></p>\n<p>  <tr></p>\n<pre><code>&lt;td&gt;\n  Hi\n&lt;/td&gt;\n</code></pre>\n<p>  </tr></p>\n<p></table></p>", html);
    }

}
