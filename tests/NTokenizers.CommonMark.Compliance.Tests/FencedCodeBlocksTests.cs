using NTokenizers.ToHtml;

namespace NTokenizers.CommonMark.Compliance.Tests;

/// <summary>
/// CommonMark spec 0.31.2 compliance tests for Fenced code blocks.
/// Source: https://spec.commonmark.org/0.31.2/#fenced-code-blocks
/// Total examples: 29
/// </summary>
public class FencedCodeBlocksTests
{
    [Fact]
    public void Example_119()
    {
        var input = "```\n<\n >\n```";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<pre><code>&lt;\n &gt;\n</code></pre>", html);
    }

    [Fact]
    public void Example_120()
    {
        var input = "~~~\n<\n >\n~~~";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<pre><code>&lt;\n &gt;\n</code></pre>", html);
    }

    [Fact]
    public void Example_121()
    {
        var input = "``\nfoo\n``";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><code>foo</code></p>", html);
    }

    [Fact]
    public void Example_122()
    {
        var input = "```\naaa\n~~~\n```";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<pre><code>aaa\n~~~\n</code></pre>", html);
    }

    [Fact]
    public void Example_123()
    {
        var input = "~~~\naaa\n```\n~~~";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<pre><code>aaa\n```\n</code></pre>", html);
    }

    [Fact]
    public void Example_124()
    {
        var input = "````\naaa\n```\n``````";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<pre><code>aaa\n```\n</code></pre>", html);
    }

    [Fact]
    public void Example_125()
    {
        var input = "~~~~\naaa\n~~~\n~~~~";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<pre><code>aaa\n~~~\n</code></pre>", html);
    }

    [Fact]
    public void Example_126()
    {
        var input = "```";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<pre><code></code></pre>", html);
    }

    [Fact]
    public void Example_127()
    {
        var input = "`````\n\n```\naaa";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<pre><code>\n```\naaa\n</code></pre>", html);
    }

    [Fact]
    public void Example_128()
    {
        var input = "> ```\n> aaa\n\nbbb";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<blockquote>\n<pre><code>aaa\n</code></pre>\n</blockquote>\n<p>bbb</p>", html);
    }

    [Fact]
    public void Example_129()
    {
        var input = "```\n\n  \n```";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<pre><code>\n  \n</code></pre>", html);
    }

    [Fact]
    public void Example_130()
    {
        var input = "```\n```";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<pre><code></code></pre>", html);
    }

    [Fact]
    public void Example_131()
    {
        var input = " ```\n aaa\naaa\n```";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<pre><code>aaa\naaa\n</code></pre>", html);
    }

    [Fact]
    public void Example_132()
    {
        var input = "  ```\naaa\n  aaa\naaa\n  ```";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<pre><code>aaa\naaa\naaa\n</code></pre>", html);
    }

    [Fact]
    public void Example_133()
    {
        var input = "   ```\n   aaa\n    aaa\n  aaa\n   ```";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<pre><code>aaa\n aaa\naaa\n</code></pre>", html);
    }

    [Fact]
    public void Example_134()
    {
        var input = "    ```\n    aaa\n    ```";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<pre><code>```\naaa\n```\n</code></pre>", html);
    }

    [Fact]
    public void Example_135()
    {
        var input = "```\naaa\n  ```";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<pre><code>aaa\n</code></pre>", html);
    }

    [Fact]
    public void Example_136()
    {
        var input = "   ```\naaa\n  ```";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<pre><code>aaa\n</code></pre>", html);
    }

    [Fact]
    public void Example_137()
    {
        var input = "```\naaa\n    ```";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<pre><code>aaa\n    ```\n</code></pre>", html);
    }

    [Fact]
    public void Example_138()
    {
        var input = "``` ```\naaa";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><code> </code>\naaa</p>", html);
    }

    [Fact]
    public void Example_139()
    {
        var input = "~~~~~~\naaa\n~~~ ~~";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<pre><code>aaa\n~~~ ~~\n</code></pre>", html);
    }

    [Fact]
    public void Example_140()
    {
        var input = "foo\n```\nbar\n```\nbaz";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>foo</p>\n<pre><code>bar\n</code></pre>\n<p>baz</p>", html);
    }

    [Fact]
    public void Example_141()
    {
        var input = "foo\n---\n~~~\nbar\n~~~\n# baz";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<h2>foo</h2>\n<pre><code>bar\n</code></pre>\n<h1>baz</h1>", html);
    }

    [Fact]
    public void Example_142()
    {
        var input = "```ruby\ndef foo(x)\n  return 3\nend\n```";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<pre><code class=\"language-ruby\">def foo(x)\n  return 3\nend\n</code></pre>", html);
    }

    [Fact]
    public void Example_143()
    {
        var input = "~~~~    ruby startline=3 $%@#$\ndef foo(x)\n  return 3\nend\n~~~~~~~";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<pre><code class=\"language-ruby\">def foo(x)\n  return 3\nend\n</code></pre>", html);
    }

    [Fact]
    public void Example_144()
    {
        var input = "````;\n````";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<pre><code class=\"language-;\"></code></pre>", html);
    }

    [Fact]
    public void Example_145()
    {
        var input = "``` aa ```\nfoo";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><code>aa</code>\nfoo</p>", html);
    }

    [Fact]
    public void Example_146()
    {
        var input = "~~~ aa ``` ~~~\nfoo\n~~~";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<pre><code class=\"language-aa\">foo\n</code></pre>", html);
    }

    [Fact]
    public void Example_147()
    {
        var input = "```\n``` aaa\n```";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<pre><code>``` aaa\n</code></pre>", html);
    }

}
