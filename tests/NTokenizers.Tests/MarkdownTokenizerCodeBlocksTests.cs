using NTokenizers.C;
using NTokenizers.Cpp;
using NTokenizers.Go;
using NTokenizers.Java;
using NTokenizers.Kotlin;
using NTokenizers.Markdown;
using NTokenizers.Rust;
using NTokenizers.Swift;
using NTokenizers.Toml;
using NTokenizers.Xml;

namespace Markdown;

public class MarkdownTokenizerCodeBlocksTests : MarkdownTokenizerTestBase
{
    private static (List<MarkdownToken> tokens, string text) Tokenize(string markdown)
    {
        var (tokens, text, _) = MarkdownTokenizerTestBase.Tokenize(markdown);
        return (tokens, text);
    }

    [Fact]
    public void TestInlineCode()
    {
        var markdown = "`code`";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(3, tokens.Count);
        Assert.Equal(MarkdownTokenType.ParagraphBlockStart, tokens[0].TokenType);
        Assert.Equal(MarkdownTokenType.CodeInline, tokens[1].TokenType);
        Assert.Equal("code", tokens[1].Value);
        Assert.Equal(MarkdownTokenType.ParagraphBlockEnd, tokens[2].TokenType);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestInlineCodeWithInnerBackticks()
    {
        // A backtick run shorter than the opening run is content, not a closing delimiter.
        var markdown = "`` foo ` bar ``";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(3, tokens.Count);
        Assert.Equal(MarkdownTokenType.ParagraphBlockStart, tokens[0].TokenType);
        Assert.Equal(MarkdownTokenType.CodeInline, tokens[1].TokenType);
        Assert.Equal(" foo ` bar ", tokens[1].Value);
        Assert.Equal(MarkdownTokenType.ParagraphBlockEnd, tokens[2].TokenType);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestInlineCodeWithDoubleBacktickRunInside()
    {
        // A double-backtick run inside a single-backtick code span is content.
        var markdown = "`  ``  `";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(3, tokens.Count);
        Assert.Equal(MarkdownTokenType.ParagraphBlockStart, tokens[0].TokenType);
        Assert.Equal(MarkdownTokenType.CodeInline, tokens[1].TokenType);
        Assert.Equal("  ``  ", tokens[1].Value);
        Assert.Equal(MarkdownTokenType.ParagraphBlockEnd, tokens[2].TokenType);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestInlineCodeLongerOpeningRun()
    {
        // A run of exactly the opening length closes the span; shorter runs are content.
        var markdown = "``foo`bar``";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(3, tokens.Count);
        Assert.Equal(MarkdownTokenType.ParagraphBlockStart, tokens[0].TokenType);
        Assert.Equal(MarkdownTokenType.CodeInline, tokens[1].TokenType);
        Assert.Equal("foo`bar", tokens[1].Value);
        Assert.Equal(MarkdownTokenType.ParagraphBlockEnd, tokens[2].TokenType);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestCodeBlock()
    {
        var markdown = "```\ncode\n```";
        var (tokens, text) = Tokenize(markdown);
        Assert.Single(tokens);
        Assert.Equal(MarkdownTokenType.CodeBlock, tokens[0].TokenType);
        Assert.Equal(string.Empty, tokens[0].Value); // Code blocks have empty value
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestCodeBlockWithLanguage()
    {
        var markdown = "```javascript\nvar x = 1;\n```";
        var (tokens, text) = Tokenize(markdown);
        // With OnInlineToken set, code blocks with language will emit syntax tokens
        // Without setting OnInlineToken, we just get the code block token with empty value
        Assert.Contains(tokens, t => t.TokenType == MarkdownTokenType.CodeBlock);
        var codeBlock = tokens.First(t => t.TokenType == MarkdownTokenType.CodeBlock);
        Assert.Equal(string.Empty, codeBlock.Value); // Code blocks have empty value
        Assert.NotNull(codeBlock.Metadata);

        Assert.NotNull(codeBlock.Metadata);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestHtmlCodeBlockWithStyleAndScript()
    {
        var markdown = @"# Test

```html
<html>
<head>
    <style>
        body { color: red; }
    </style>
</head>
<body>
    <script>
        console.log('Hi');
    </script>
</body>
</html>
```";

        var (tokens, text) = Tokenize(markdown);

        // Should have heading and code block tokens
        Assert.Contains(tokens, t => t.TokenType == MarkdownTokenType.Heading);
        Assert.Contains(tokens, t => t.TokenType == MarkdownTokenType.CodeBlock);

        // The HTML content should be tokenized
        var codeBlockTokens = tokens.Where(t => t.TokenType == MarkdownTokenType.CodeBlock).ToList();
        Assert.NotEmpty(codeBlockTokens);
    }

    [Fact]
    public void TestTomlCodeBlock()
    {
        var markdown = """
    ## TOML Example
    ```toml
    title = "My App"
    active = true
    count = 42
    ```
    """;

        var (tokens, text) = Tokenize(markdown);

        // Should have heading and code block tokens
        Assert.Contains(tokens, t => t.TokenType == MarkdownTokenType.Heading);
        Assert.Contains(tokens, t => t.TokenType == MarkdownTokenType.CodeBlock);

        // Verify the code block has TOML metadata
        var codeBlockTokens = tokens.Where(t => t.TokenType == MarkdownTokenType.CodeBlock).ToList();
        Assert.NotEmpty(codeBlockTokens);
        Assert.IsType<TomlCodeBlockMetadata>(codeBlockTokens[0].Metadata);
    }

    [Fact]
    public void TestCppCodeBlock()
    {
        var markdown = """
    ## C++ Example
    ```cpp
    int x = 42;
    ```
    """;

        var (tokens, text) = Tokenize(markdown);
        Assert.Contains(tokens, t => t.TokenType == MarkdownTokenType.Heading);
        Assert.Contains(tokens, t => t.TokenType == MarkdownTokenType.CodeBlock);
        var codeBlockTokens = tokens.Where(t => t.TokenType == MarkdownTokenType.CodeBlock).ToList();
        Assert.NotEmpty(codeBlockTokens);
        Assert.IsType<CppCodeBlockMetadata>(codeBlockTokens[0].Metadata);
    }

    [Fact]
    public void TestRustCodeBlock()
    {
        var markdown = """
    ## Rust Example
    ```rust
    let x = 42;
    ```
    """;

        var (tokens, text) = Tokenize(markdown);
        Assert.Contains(tokens, t => t.TokenType == MarkdownTokenType.Heading);
        Assert.Contains(tokens, t => t.TokenType == MarkdownTokenType.CodeBlock);
        var codeBlockTokens = tokens.Where(t => t.TokenType == MarkdownTokenType.CodeBlock).ToList();
        Assert.NotEmpty(codeBlockTokens);
        Assert.IsType<RustCodeBlockMetadata>(codeBlockTokens[0].Metadata);
    }

    [Fact]
    public void TestKotlinCodeBlock()
    {
        var markdown = """
    ## Kotlin Example
    ```kotlin
    val x = 42
    ```
    """;

        var (tokens, text) = Tokenize(markdown);
        Assert.Contains(tokens, t => t.TokenType == MarkdownTokenType.Heading);
        Assert.Contains(tokens, t => t.TokenType == MarkdownTokenType.CodeBlock);
        var codeBlockTokens = tokens.Where(t => t.TokenType == MarkdownTokenType.CodeBlock).ToList();
        Assert.NotEmpty(codeBlockTokens);
        Assert.IsType<KotlinCodeBlockMetadata>(codeBlockTokens[0].Metadata);
    }

    [Fact]
    public void TestGoCodeBlock()
    {
        var markdown = """
    ## Go Example
    ```go
    x := 42
    ```
    """;

        var (tokens, text) = Tokenize(markdown);
        Assert.Contains(tokens, t => t.TokenType == MarkdownTokenType.Heading);
        Assert.Contains(tokens, t => t.TokenType == MarkdownTokenType.CodeBlock);
        var codeBlockTokens = tokens.Where(t => t.TokenType == MarkdownTokenType.CodeBlock).ToList();
        Assert.NotEmpty(codeBlockTokens);
        Assert.IsType<GoCodeBlockMetadata>(codeBlockTokens[0].Metadata);
    }

    [Fact]
    public void TestSwiftCodeBlock()
    {
        var markdown = """
    ## Swift Example
    ```swift
    let x = 42
    ```
    """;

        var (tokens, text) = Tokenize(markdown);
        Assert.Contains(tokens, t => t.TokenType == MarkdownTokenType.Heading);
        Assert.Contains(tokens, t => t.TokenType == MarkdownTokenType.CodeBlock);
        var codeBlockTokens = tokens.Where(t => t.TokenType == MarkdownTokenType.CodeBlock).ToList();
        Assert.NotEmpty(codeBlockTokens);
        Assert.IsType<SwiftCodeBlockMetadata>(codeBlockTokens[0].Metadata);
    }

    [Fact]
    public void TestJavaCodeBlock()
    {
        var markdown = """
    ## Java Example
    ```java
    int x = 42;
    ```
    """;

        var (tokens, text) = Tokenize(markdown);
        Assert.Contains(tokens, t => t.TokenType == MarkdownTokenType.Heading);
        Assert.Contains(tokens, t => t.TokenType == MarkdownTokenType.CodeBlock);
        var codeBlockTokens = tokens.Where(t => t.TokenType == MarkdownTokenType.CodeBlock).ToList();
        Assert.NotEmpty(codeBlockTokens);
        Assert.IsType<JavaCodeBlockMetadata>(codeBlockTokens[0].Metadata);
    }

    [Fact]
    public void TestCCodeBlock()
    {
        var markdown = """
    ## C Example
    ```c
    int x = 42;
    ```
    """;

        var (tokens, text) = Tokenize(markdown);
        Assert.Contains(tokens, t => t.TokenType == MarkdownTokenType.Heading);
        Assert.Contains(tokens, t => t.TokenType == MarkdownTokenType.CodeBlock);
        var codeBlockTokens = tokens.Where(t => t.TokenType == MarkdownTokenType.CodeBlock).ToList();
        Assert.NotEmpty(codeBlockTokens);
        Assert.IsType<CCodeBlockMetadata>(codeBlockTokens[0].Metadata);
    }

    [Fact]
    public void TestXamlCodeBlock()
    {
        var markdown = """
    ## XAML Example
    ```xaml
    <Window xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
            Title="My App" Height="450" Width="800">
        <Button Content="Click me" />
    </Window>
    ```
    """;

        var (tokens, text) = Tokenize(markdown);
        Assert.Contains(tokens, t => t.TokenType == MarkdownTokenType.Heading);
        Assert.Contains(tokens, t => t.TokenType == MarkdownTokenType.CodeBlock);
        var codeBlockTokens = tokens.Where(t => t.TokenType == MarkdownTokenType.CodeBlock).ToList();
        Assert.NotEmpty(codeBlockTokens);
        Assert.IsType<XmlCodeBlockMetadata>(codeBlockTokens[0].Metadata);
    }

    [Fact]
    public void TestTildeCodeBlock()
    {
        // A run of three tildes opens a fenced code block, just like backticks.
        var markdown = "~~~\ncode\n~~~";
        var (tokens, text) = Tokenize(markdown);
        Assert.Single(tokens);
        Assert.Equal(MarkdownTokenType.CodeBlock, tokens[0].TokenType);
        Assert.Equal(string.Empty, tokens[0].Value); // Code blocks have empty value
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestTildeCodeBlockWithLanguage()
    {
        // The language identifier after a tilde fence is carried by the metadata.
        var markdown = "~~~ruby\ncode\n~~~";
        var (tokens, text) = Tokenize(markdown);
        var codeBlock = Assert.Single(tokens, t => t.TokenType == MarkdownTokenType.CodeBlock);
        var metadata = Assert.IsAssignableFrom<NTokenizers.Core.ICodeBlockMetadata>(codeBlock.Metadata);
        Assert.Equal("ruby", metadata.Language);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestLongFenceRequiresMatchingClose()
    {
        // Fence length matching: a closing fence must use the same character and at
        // least as many of it as the opening fence. The '```' line is content, and the
        // '``````' line closes the block; the two extra backticks beyond the required
        // run surface as a following paragraph.
        var markdown = "````\n```\n``````";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(4, tokens.Count);
        Assert.Equal(MarkdownTokenType.CodeBlock, tokens[0].TokenType);
        Assert.Equal(MarkdownTokenType.ParagraphBlockStart, tokens[1].TokenType);
        Assert.Equal(MarkdownTokenType.Text, tokens[2].TokenType);
        Assert.Equal("``", tokens[2].Value);
        Assert.Equal(MarkdownTokenType.ParagraphBlockEnd, tokens[3].TokenType);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestTildeFenceContentWithBackticks()
    {
        // Tildes and backticks cannot be mixed: inside a tilde fence a line of
        // backticks is content, not a closing fence.
        var markdown = "~~~\naaa\n```\n~~~";
        var (tokens, text) = Tokenize(markdown);
        Assert.Single(tokens);
        Assert.Equal(MarkdownTokenType.CodeBlock, tokens[0].TokenType);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestTildeFenceBreaksQuote()
    {
        // Decision table, row 4 (line-start construct): a tilde code fence ends the
        // quote, the same as a backtick fence.
        var markdown = "> foo\n~~~";
        var (tokens, text) = Tokenize(markdown);
        // The code fence line ends the quote. The quoted paragraph's closing newline is
        // consumed, not emitted: a blockquote renders as a box that already closes on a
        // fresh line, so the terminating line must not appear inside the box.
        Assert.Equal(5, tokens.Count);
        Assert.Equal(MarkdownTokenType.Blockquote, tokens[0].TokenType);
        Assert.Equal(MarkdownTokenType.ParagraphBlockStart, tokens[1].TokenType);
        Assert.Equal(MarkdownTokenType.Text, tokens[2].TokenType);
        Assert.Equal("foo", tokens[2].Value);
        Assert.Equal(MarkdownTokenType.ParagraphBlockEnd, tokens[3].TokenType);
        Assert.Equal(MarkdownTokenType.CodeBlock, tokens[4].TokenType);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestSvgCodeBlock()
    {
        var markdown = """
    ## SVG Example
    ```svg
    <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 100 100">
        <circle cx="50" cy="50" r="40" fill="red" />
    </svg>
    ```
    """;

        var (tokens, text) = Tokenize(markdown);
        Assert.Contains(tokens, t => t.TokenType == MarkdownTokenType.Heading);
        Assert.Contains(tokens, t => t.TokenType == MarkdownTokenType.CodeBlock);
        var codeBlockTokens = tokens.Where(t => t.TokenType == MarkdownTokenType.CodeBlock).ToList();
        Assert.NotEmpty(codeBlockTokens);
        Assert.IsType<XmlCodeBlockMetadata>(codeBlockTokens[0].Metadata);
    }
}
