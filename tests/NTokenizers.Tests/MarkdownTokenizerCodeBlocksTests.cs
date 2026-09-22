using NTokenizers.C;
using NTokenizers.Cpp;
using NTokenizers.CSharp;
using NTokenizers.Css;
using NTokenizers.Generic;
using NTokenizers.Go;
using NTokenizers.Html;
using NTokenizers.Java;
using NTokenizers.Json;
using NTokenizers.Kotlin;
using NTokenizers.Markdown;
using NTokenizers.Markdown.Metadata;
using NTokenizers.Rust;
using NTokenizers.Sql;
using NTokenizers.Swift;
using NTokenizers.Toml;
using NTokenizers.Typescript;
using NTokenizers.Xml;
using System.Text;

namespace Markdown;

public class MarkdownTokenizerCodeBlocksTests
{
    private static (List<MarkdownToken> tokens, string text) Tokenize(string markdown)
    {
        var tokens = new List<MarkdownToken>();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(markdown));
        var result = MarkdownTokenizer.Create().ParseAsync(stream, token =>
        {
            tokens.Add(token);

            // Automatically set OnInlineToken to capture inline tokens
            // Note: We just register the handler without waiting - the processing happens during parsing
            if (token.Metadata is HeadingMetadata headingMeta)
            {
                headingMeta.RegisterInlineTokenHandler(tokens.Add);
            }
            else if (token.Metadata is BlockquoteMetadata blockquoteMeta)
            {
                blockquoteMeta.RegisterInlineTokenHandler(tokens.Add);
            }
            else if (token.Metadata is ListItemMetadata listMeta)
            {
                listMeta.RegisterInlineTokenHandler(tokens.Add);
            }
            else if (token.Metadata is OrderedListItemMetadata orderedListMeta)
            {
                orderedListMeta.RegisterInlineTokenHandler(tokens.Add);
            }
            else if (token.Metadata is CSharpCodeBlockMetadata csharpMeta)
            {
                // For C# code blocks, we receive CSharpToken objects
                csharpMeta.RegisterInlineTokenHandler(token => { /* Capture C# tokens if needed */ });
            }
            else if (token.Metadata is JavaCodeBlockMetadata javaMeta)
            {
                javaMeta.RegisterInlineTokenHandler(token => { });
            }
            else if (token.Metadata is CCodeBlockMetadata cMeta)
            {
                cMeta.RegisterInlineTokenHandler(token => { });
            }
            else if (token.Metadata is CppCodeBlockMetadata cppMeta)
            {
                cppMeta.RegisterInlineTokenHandler(token => { });
            }
            else if (token.Metadata is RustCodeBlockMetadata rustMeta)
            {
                rustMeta.RegisterInlineTokenHandler(token => { });
            }
            else if (token.Metadata is KotlinCodeBlockMetadata kotlinMeta)
            {
                kotlinMeta.RegisterInlineTokenHandler(token => { });
            }
            else if (token.Metadata is GoCodeBlockMetadata goMeta)
            {
                goMeta.RegisterInlineTokenHandler(token => { });
            }
            else if (token.Metadata is SwiftCodeBlockMetadata swiftMeta)
            {
                swiftMeta.RegisterInlineTokenHandler(token => { });
            }
            else if (token.Metadata is JsonCodeBlockMetadata jsonMeta)
            {
                // For JSON code blocks, we receive JsonToken objects
                jsonMeta.RegisterInlineTokenHandler(token => { });
            }
            else if (token.Metadata is XmlCodeBlockMetadata xmlMeta)
            {
                // For XML, XAML, and SVG code blocks, we receive XmlToken objects
                xmlMeta.RegisterInlineTokenHandler(token => { });
            }
            else if (token.Metadata is HtmlCodeBlockMetadata htmlMeta)
            {
                // For XML code blocks, we receive XmlToken objects
                htmlMeta.RegisterInlineTokenHandler(token =>
                {
                    if (token.Metadata is CssCodeBlockMetadata cssMetadata)
                    {
                        cssMetadata.RegisterInlineTokenHandler(token => { });
                    }
                    else if (token.Metadata is TypeScriptCodeBlockMetadata tsMetadata)
                    {
                        tsMetadata.RegisterInlineTokenHandler(token => { });
                    }
                });
            }
            else if (token.Metadata is SqlCodeBlockMetadata sqlMeta)
            {
                // For SQL code blocks, we receive SqlToken objects
                sqlMeta.RegisterInlineTokenHandler(token => { });
            }
            else if (token.Metadata is TypeScriptCodeBlockMetadata tsMeta)
            {
                // For TypeScript code blocks, we receive TypescriptToken objects
                tsMeta.RegisterInlineTokenHandler(token => { });
            }
            else if (token.Metadata is TomlCodeBlockMetadata tomlMeta)
            {
                // For TOML code blocks, we receive TomlToken objects
                tomlMeta.RegisterInlineTokenHandler(token => { });
            }
            else if (token.Metadata is TableMetadata tableMeta)
            {
                tableMeta.RegisterInlineTokenHandler(tokens.Add);
            }
            else if (token.Metadata is GenericCodeBlockMetadata gMeta)
            {
                gMeta.RegisterInlineTokenHandler(token => { });
            }
        }).GetAwaiter().GetResult();
        return (tokens, result);
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
