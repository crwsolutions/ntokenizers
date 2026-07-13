using NTokenizers.ToHtml;

namespace NTokenizers.CommonMark.Compliance.Tests;

/// <summary>
/// CommonMark spec 0.31.2 compliance tests for Code spans.
/// Source: https://spec.commonmark.org/0.31.2/#code-spans
/// Total examples: 22
/// </summary>
public class CodeSpansTests
{
    [Fact]
    public void Example_328()
    {
        var input = "`` foo ` bar ``";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><code>foo ` bar</code></p>", html);
    }

    [Fact]
    public void Example_329()
    {
        var input = "` `` `";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><code>``</code></p>", html);
    }

    [Fact]
    public void Example_330()
    {
        var input = "`  ``  `";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><code> `` </code></p>", html);
    }

    [Fact]
    public void Example_331()
    {
        var input = "` a`";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><code> a</code></p>", html);
    }

    [Fact]
    public void Example_332()
    {
        var input = "` b `";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><code> b </code></p>", html);
    }

    [Fact]
    public void Example_333()
    {
        var input = "` `\n`  `";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><code> </code>\n<code>  </code></p>", html);
    }

    [Fact]
    public void Example_334()
    {
        var input = "``\nfoo\nbar  \nbaz\n``";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><code>foo bar   baz</code></p>", html);
    }

    [Fact]
    public void Example_335()
    {
        var input = "``\nfoo \n``";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><code>foo </code></p>", html);
    }

    [Fact]
    public void Example_336()
    {
        var input = "`foo   bar \nbaz`";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><code>foo   bar  baz</code></p>", html);
    }

    [Fact]
    public void Example_337()
    {
        var input = "`foo\\`bar`";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><code>foo\\</code>bar`</p>", html);
    }

    [Fact]
    public void Example_338()
    {
        var input = "``foo`bar``";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><code>foo`bar</code></p>", html);
    }

    [Fact]
    public void Example_339()
    {
        var input = "` foo `` bar `";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><code>foo `` bar</code></p>", html);
    }

    [Fact]
    public void Example_340()
    {
        var input = "*foo`*`";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>*foo<code>*</code></p>", html);
    }

    [Fact]
    public void Example_341()
    {
        var input = "[not a `link](/foo`)";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>[not a <code>link](/foo</code>)</p>", html);
    }

    [Fact]
    public void Example_342()
    {
        var input = "`<a href=\"`\">`";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><code>&lt;a href=&quot;</code>&quot;&gt;`</p>", html);
    }

    [Fact]
    public void Example_343()
    {
        var input = "<a href=\"`\">`";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"`\">`</p>", html);
    }

    [Fact]
    public void Example_344()
    {
        var input = "`<https://foo.bar.`baz>`";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><code>&lt;https://foo.bar.</code>baz&gt;`</p>", html);
    }

    [Fact]
    public void Example_345()
    {
        var input = "<https://foo.bar.`baz>`";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"https://foo.bar.%60baz\">https://foo.bar.`baz</a>`</p>", html);
    }

    [Fact]
    public void Example_346()
    {
        var input = "```foo``";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>```foo``</p>", html);
    }

    [Fact]
    public void Example_347()
    {
        var input = "`foo";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>`foo</p>", html);
    }

    [Fact]
    public void Example_348()
    {
        var input = "`foo``bar``";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>`foo<code>bar</code></p>", html);
    }

    [Fact]
    public void Example_349()
    {
        var input = "*foo bar*";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><em>foo bar</em></p>", html);
    }

}
