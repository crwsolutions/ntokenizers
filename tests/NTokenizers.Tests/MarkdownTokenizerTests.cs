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

public class MarkdownTokenizerTests
{
    private static (List<MarkdownToken> tokens, string text) Tokenize(string markdown)
    {
        var tokens = new List<MarkdownToken>();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(markdown));

        // Recursively registers inline token handlers for markdown tokens. A token that
        // carries InlineMetadata<MarkdownToken> (heading, blockquote, list item, indented
        // code block, table) can itself contain markdown tokens - most importantly, a
        // blockquote can nest another blockquote. Registering the handler here (recursively)
        // ensures the whole token tree is captured in `tokens`; a flat registration would
        // leave nested block content unhandled.
        static void RegisterInlineHandlers(MarkdownToken token, List<MarkdownToken> tokens)
        {
            if (token.Metadata is HeadingMetadata headingMeta)
            {
                headingMeta.RegisterInlineTokenHandler(t =>
                {
                    tokens.Add(t);
                    RegisterInlineHandlers(t, tokens);
                });
            }
            else if (token.Metadata is BlockquoteMetadata blockquoteMeta)
            {
                blockquoteMeta.RegisterInlineTokenHandler(t =>
                {
                    tokens.Add(t);
                    RegisterInlineHandlers(t, tokens);
                });
            }
            else if (token.Metadata is ListItemMetadata listMeta)
            {
                listMeta.RegisterInlineTokenHandler(t =>
                {
                    tokens.Add(t);
                    RegisterInlineHandlers(t, tokens);
                });
            }
            else if (token.Metadata is OrderedListItemMetadata orderedListMeta)
            {
                orderedListMeta.RegisterInlineTokenHandler(t =>
                {
                    tokens.Add(t);
                    RegisterInlineHandlers(t, tokens);
                });
            }
            else if (token.Metadata is IndentedCodeBlockMetadata indentedCodeMeta)
            {
                // Indented code blocks stream plain markdown Text tokens.
                indentedCodeMeta.RegisterInlineTokenHandler(tokens.Add);
            }
            else if (token.Metadata is TableMetadata tableMeta)
            {
                tableMeta.RegisterInlineTokenHandler(t =>
                {
                    tokens.Add(t);
                    RegisterInlineHandlers(t, tokens);
                });
            }
        }
        var result = MarkdownTokenizer.Create().ParseAsync(stream, token =>
        {
            tokens.Add(token);
            RegisterInlineHandlers(token, tokens);

            // Code block metadata streams language-specific (non-markdown) tokens; the
            // no-op handlers below let the parser stream and discard that content.
            if (token.Metadata is CSharpCodeBlockMetadata csharpMeta)
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
                // For XML code blocks, we receive XmlToken objects
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
            else if (token.Metadata is GenericCodeBlockMetadata gMeta)
            {
                gMeta.RegisterInlineTokenHandler(token => { });
            }
        }).GetAwaiter().GetResult();
        return (tokens, result);
    }

    /// <summary>
    /// Concatenates the value of every token, including tokens delivered through inline
    /// handlers (headings, blockquotes, list items, code blocks, tables) which <see cref="Tokenize"/>
    /// flattens into the same list. For a prose-only document this equals the input (with the
    /// house CRLF-to-LF line-ending normalization), which is the structural fidelity invariant.
    /// </summary>
    private static string TokenText(List<MarkdownToken> tokens)
    {
        var sb = new StringBuilder();
        foreach (var token in tokens)
        {
            sb.Append(token.Value);
        }
        return sb.ToString();
    }

    [Fact]
    public void TestPlainText()
    {
        var markdown = "Hello world";
        var (tokens, text) = Tokenize(markdown);
        // A plain line is a paragraph: PStart, text..., PEnd.
        Assert.Equal(4, tokens.Count);
        Assert.Equal(MarkdownTokenType.ParagraphBlockStart, tokens[0].TokenType);
        Assert.Equal(string.Empty, tokens[0].Value);
        Assert.Equal(MarkdownTokenType.Text, tokens[1].TokenType);
        Assert.Equal("Hello ", tokens[1].Value);
        Assert.Equal(MarkdownTokenType.Text, tokens[2].TokenType);
        Assert.Equal("world", tokens[2].Value);
        Assert.Equal(MarkdownTokenType.ParagraphBlockEnd, tokens[3].TokenType);
        Assert.Equal(string.Empty, tokens[3].Value);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestParagraphSingleLine()
    {
        var markdown = "justtext";
        var (tokens, text) = Tokenize(markdown);
        // A single non-blank line is a paragraph block: PStart, text, PEnd.
        Assert.Equal(3, tokens.Count);
        Assert.Equal(MarkdownTokenType.ParagraphBlockStart, tokens[0].TokenType);
        Assert.Equal(string.Empty, tokens[0].Value);
        Assert.Null(tokens[0].Metadata);
        Assert.Equal(MarkdownTokenType.Text, tokens[1].TokenType);
        Assert.Equal("justtext", tokens[1].Value);
        Assert.Equal(MarkdownTokenType.ParagraphBlockEnd, tokens[2].TokenType);
        Assert.Equal(string.Empty, tokens[2].Value);
        Assert.Null(tokens[2].Metadata);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestParagraphMultiLineSoftBreak()
    {
        var markdown = "foo\nbar";
        var (tokens, text) = Tokenize(markdown);
        // A multi-line paragraph is a single block: PStart, foo, soft break, bar, PEnd.
        Assert.Equal(5, tokens.Count);
        Assert.Equal(MarkdownTokenType.ParagraphBlockStart, tokens[0].TokenType);
        Assert.Equal(MarkdownTokenType.Text, tokens[1].TokenType);
        Assert.Equal("foo", tokens[1].Value);
        Assert.Equal(MarkdownTokenType.Text, tokens[2].TokenType);
        Assert.Equal("\n", tokens[2].Value); // soft line break
        Assert.Equal(MarkdownTokenType.Text, tokens[3].TokenType);
        Assert.Equal("bar", tokens[3].Value);
        Assert.Equal(MarkdownTokenType.ParagraphBlockEnd, tokens[4].TokenType);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestTwoParagraphsSeparatedByBlankLine()
    {
        var markdown = "one\n\ntwo";
        var (tokens, text) = Tokenize(markdown);
        // Two paragraphs, each wrapped in its own PStart/PEnd (the blank line ends the first).
        // Both newlines survive as plain Text("\n") separators after the first PEnd: the
        // paragraph's closing newline and the blank line's own line ending. The newline count
        // is preserved (no normalization), so a console renderer printing Text tokens
        // reproduces the layout of the input.
        Assert.Equal(8, tokens.Count);
        Assert.Equal(MarkdownTokenType.ParagraphBlockStart, tokens[0].TokenType);
        Assert.Equal(MarkdownTokenType.Text, tokens[1].TokenType);
        Assert.Equal("one", tokens[1].Value);
        Assert.Equal(MarkdownTokenType.ParagraphBlockEnd, tokens[2].TokenType);
        Assert.Equal(MarkdownTokenType.Text, tokens[3].TokenType);
        Assert.Equal("\n", tokens[3].Value); // the paragraph's closing newline, after the PEnd
        Assert.Equal(MarkdownTokenType.Text, tokens[4].TokenType);
        Assert.Equal("\n", tokens[4].Value); // the blank line's own line ending
        Assert.Equal(MarkdownTokenType.ParagraphBlockStart, tokens[5].TokenType);
        Assert.Equal(MarkdownTokenType.Text, tokens[6].TokenType);
        Assert.Equal("two", tokens[6].Value);
        Assert.Equal(MarkdownTokenType.ParagraphBlockEnd, tokens[7].TokenType);
        Assert.Equal(markdown, text);
        Assert.Equal(markdown, TokenText(tokens));
    }

    [Fact]
    public void TestHeadingLevel1()
    {
        var markdown = "# Heading 1";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(2, tokens.Count); // Heading token + inline text token
        Assert.Equal(MarkdownTokenType.Heading, tokens[0].TokenType);
        Assert.Equal(string.Empty, tokens[0].Value); // Value is empty when OnInlineToken is used
        Assert.NotNull(tokens[0].Metadata);
        Assert.IsType<HeadingMetadata>(tokens[0].Metadata);
        Assert.Equal(1, ((HeadingMetadata)tokens[0].Metadata!).Level);

        // Inline content
        Assert.Equal(MarkdownTokenType.Text, tokens[1].TokenType);
        Assert.Equal("Heading 1", tokens[1].Value);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestHeadingLevel2()
    {
        var markdown = "## Heading 2";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(2, tokens.Count); // Heading token + inline text token
        Assert.Equal(MarkdownTokenType.Heading, tokens[0].TokenType);
        Assert.Equal(string.Empty, tokens[0].Value); // Value is empty when OnInlineToken is used
        Assert.Equal(2, ((HeadingMetadata)tokens[0].Metadata!).Level);

        // Inline content
        Assert.Equal(MarkdownTokenType.Text, tokens[1].TokenType);
        Assert.Equal("Heading 2", tokens[1].Value);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestHeadingLevel6()
    {
        var markdown = "###### Heading 6";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(2, tokens.Count); // Heading token + inline text token
        Assert.Equal(MarkdownTokenType.Heading, tokens[0].TokenType);
        Assert.Equal(string.Empty, tokens[0].Value); // Value is empty when OnInlineToken is used
        Assert.Equal(6, ((HeadingMetadata)tokens[0].Metadata!).Level);

        // Inline content
        Assert.Equal(MarkdownTokenType.Text, tokens[1].TokenType);
        Assert.Equal("Heading 6", tokens[1].Value);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestBoldText()
    {
        var markdown = "**bold text**";
        var (tokens, text) = Tokenize(markdown);
        // A line that starts with an inline construct is still a paragraph.
        Assert.Equal(3, tokens.Count);
        Assert.Equal(MarkdownTokenType.ParagraphBlockStart, tokens[0].TokenType);
        Assert.Equal(MarkdownTokenType.Bold, tokens[1].TokenType);
        Assert.Equal("bold text", tokens[1].Value);
        Assert.Equal(MarkdownTokenType.ParagraphBlockEnd, tokens[2].TokenType);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestBoldAlternativeText()
    {
        var markdown = "__bold text__";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(3, tokens.Count);
        Assert.Equal(MarkdownTokenType.ParagraphBlockStart, tokens[0].TokenType);
        Assert.Equal(MarkdownTokenType.Bold, tokens[1].TokenType);
        Assert.Equal("bold text", tokens[1].Value);
        Assert.Equal(MarkdownTokenType.ParagraphBlockEnd, tokens[2].TokenType);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestItalicText()
    {
        var markdown = "*italic text*";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(3, tokens.Count);
        Assert.Equal(MarkdownTokenType.ParagraphBlockStart, tokens[0].TokenType);
        Assert.Equal(MarkdownTokenType.Italic, tokens[1].TokenType);
        Assert.Equal("italic text", tokens[1].Value);
        Assert.Equal(MarkdownTokenType.ParagraphBlockEnd, tokens[2].TokenType);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestItalicAlternativeText()
    {
        var markdown = "_italic text_";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(3, tokens.Count);
        Assert.Equal(MarkdownTokenType.ParagraphBlockStart, tokens[0].TokenType);
        Assert.Equal(MarkdownTokenType.Italic, tokens[1].TokenType);
        Assert.Equal("italic text", tokens[1].Value);
        Assert.Equal(MarkdownTokenType.ParagraphBlockEnd, tokens[2].TokenType);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestLink()
    {
        var markdown = "[link text](http://example.com)";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(3, tokens.Count);
        Assert.Equal(MarkdownTokenType.ParagraphBlockStart, tokens[0].TokenType);
        Assert.Equal(MarkdownTokenType.Link, tokens[1].TokenType);
        Assert.Equal("[link text](http://example.com)", tokens[1].Value);
        Assert.NotNull(tokens[1].Metadata);
        Assert.IsType<LinkMetadata>(tokens[1].Metadata);
        Assert.Equal("http://example.com", ((LinkMetadata)tokens[1].Metadata!).Url);
        Assert.Equal("link text", ((LinkMetadata)tokens[1].Metadata!).Text);
        Assert.Null(((LinkMetadata)tokens[1].Metadata!).Title);
        Assert.Equal(MarkdownTokenType.ParagraphBlockEnd, tokens[2].TokenType);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestLinkWithTitle()
    {
        var markdown = "[link text](http://example.com \"This is the title\")";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(3, tokens.Count);
        Assert.Equal(MarkdownTokenType.ParagraphBlockStart, tokens[0].TokenType);
        Assert.Equal(MarkdownTokenType.Link, tokens[1].TokenType);
        Assert.Equal("[link text](http://example.com \"This is the title\")", tokens[1].Value);
        Assert.NotNull(tokens[1].Metadata);
        Assert.IsType<LinkMetadata>(tokens[1].Metadata);
        Assert.Equal("http://example.com", ((LinkMetadata)tokens[1].Metadata!).Url);
        Assert.Equal("link text", ((LinkMetadata)tokens[1].Metadata!).Text);
        Assert.Equal("This is the title", ((LinkMetadata)tokens[1].Metadata!).Title);
        Assert.Equal(MarkdownTokenType.ParagraphBlockEnd, tokens[2].TokenType);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestLinkWithEmptyText()
    {
        var markdown = "[](http://example.com)";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(3, tokens.Count);
        Assert.Equal(MarkdownTokenType.ParagraphBlockStart, tokens[0].TokenType);
        Assert.Equal(MarkdownTokenType.Link, tokens[1].TokenType);
        Assert.Equal("[](http://example.com)", tokens[1].Value);
        var metadata = (LinkMetadata)tokens[1].Metadata!;
        Assert.Equal("http://example.com", metadata.Url);
        Assert.Null(metadata.Text);
        Assert.Null(metadata.Title);
        Assert.Equal(MarkdownTokenType.ParagraphBlockEnd, tokens[2].TokenType);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestLinkWithBracketedEmptyUrl()
    {
        var markdown = "[link](<>)";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(3, tokens.Count);
        Assert.Equal(MarkdownTokenType.ParagraphBlockStart, tokens[0].TokenType);
        Assert.Equal(MarkdownTokenType.Link, tokens[1].TokenType);
        Assert.Equal("[link](<>)", tokens[1].Value);
        var metadata = (LinkMetadata)tokens[1].Metadata!;
        Assert.Equal("", metadata.Url);
        Assert.Equal("link", metadata.Text);
        Assert.Null(metadata.Title);
        Assert.True(metadata.IsBracketed);
        Assert.Equal(MarkdownTokenType.ParagraphBlockEnd, tokens[2].TokenType);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestLinkWithBracketedUrl()
    {
        var markdown = "[link](</my uri>)";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(3, tokens.Count);
        Assert.Equal(MarkdownTokenType.ParagraphBlockStart, tokens[0].TokenType);
        Assert.Equal(MarkdownTokenType.Link, tokens[1].TokenType);
        Assert.Equal("[link](</my uri>)", tokens[1].Value);
        var metadata = (LinkMetadata)tokens[1].Metadata!;
        Assert.Equal("/my uri", metadata.Url);
        Assert.Equal("link", metadata.Text);
        Assert.Null(metadata.Title);
        Assert.True(metadata.IsBracketed);
        Assert.Equal(MarkdownTokenType.ParagraphBlockEnd, tokens[2].TokenType);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestImageWithBracketedUrl()
    {
        var markdown = "![alt text](<http://example.com/image.png>)";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(3, tokens.Count);
        Assert.Equal(MarkdownTokenType.ParagraphBlockStart, tokens[0].TokenType);
        Assert.Equal(MarkdownTokenType.Image, tokens[1].TokenType);
        Assert.Equal("![alt text](<http://example.com/image.png>)", tokens[1].Value);
        var metadata = (LinkMetadata)tokens[1].Metadata!;
        Assert.Equal("http://example.com/image.png", metadata.Url);
        Assert.Equal("alt text", metadata.Text);
        Assert.Null(metadata.Title);
        Assert.True(metadata.IsBracketed);
        Assert.Equal(MarkdownTokenType.ParagraphBlockEnd, tokens[2].TokenType);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestImage()
    {
        var markdown = "![alt text](http://example.com/image.png)";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(3, tokens.Count);
        Assert.Equal(MarkdownTokenType.ParagraphBlockStart, tokens[0].TokenType);
        Assert.Equal(MarkdownTokenType.Image, tokens[1].TokenType);
        Assert.Equal("![alt text](http://example.com/image.png)", tokens[1].Value);
        Assert.NotNull(tokens[1].Metadata);
        Assert.IsType<LinkMetadata>(tokens[1].Metadata);
        Assert.Equal("http://example.com/image.png", ((LinkMetadata)tokens[1].Metadata!).Url);
        Assert.Equal("alt text", ((LinkMetadata)tokens[1].Metadata!).Text);
        Assert.Null(((LinkMetadata)tokens[1].Metadata!).Title);
        Assert.Equal(MarkdownTokenType.ParagraphBlockEnd, tokens[2].TokenType);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestImageWithTitle()
    {
        var markdown = "![alt text](http://example.com/image.png \"Image title\")";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(3, tokens.Count);
        Assert.Equal(MarkdownTokenType.ParagraphBlockStart, tokens[0].TokenType);
        Assert.Equal(MarkdownTokenType.Image, tokens[1].TokenType);
        Assert.Equal("![alt text](http://example.com/image.png \"Image title\")", tokens[1].Value);
        Assert.NotNull(tokens[1].Metadata);
        Assert.IsType<LinkMetadata>(tokens[1].Metadata);
        Assert.Equal("http://example.com/image.png", ((LinkMetadata)tokens[1].Metadata!).Url);
        Assert.Equal("alt text", ((LinkMetadata)tokens[1].Metadata!).Text);
        Assert.Equal("Image title", ((LinkMetadata)tokens[1].Metadata!).Title);
        Assert.Equal(MarkdownTokenType.ParagraphBlockEnd, tokens[2].TokenType);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestImageWithEmptyText()
    {
        var markdown = "![](http://example.com/image.png)";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(3, tokens.Count);
        Assert.Equal(MarkdownTokenType.ParagraphBlockStart, tokens[0].TokenType);
        Assert.Equal(MarkdownTokenType.Image, tokens[1].TokenType);
        Assert.Equal("![](http://example.com/image.png)", tokens[1].Value);
        Assert.NotNull(tokens[1].Metadata);
        Assert.IsType<LinkMetadata>(tokens[1].Metadata);
        Assert.Equal("http://example.com/image.png", ((LinkMetadata)tokens[1].Metadata!).Url);
        Assert.Null(((LinkMetadata)tokens[1].Metadata!).Text);
        Assert.Null(((LinkMetadata)tokens[1].Metadata!).Title);
        Assert.Equal(MarkdownTokenType.ParagraphBlockEnd, tokens[2].TokenType);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestBlockquote()
    {
        var markdown = "> quoted text";
        var (tokens, text) = Tokenize(markdown);
        // The blockquote streams its content as a full markdown sub-document:
        // a Blockquote token followed by a paragraph (PStart, text, PEnd) for the quoted line.
        Assert.Equal(5, tokens.Count);
        Assert.Equal(MarkdownTokenType.Blockquote, tokens[0].TokenType);
        Assert.Equal(string.Empty, tokens[0].Value); // Value is empty when OnInlineToken is used

        // Inline content (a single-line paragraph)
        Assert.Equal(MarkdownTokenType.ParagraphBlockStart, tokens[1].TokenType);
        Assert.Equal(MarkdownTokenType.Text, tokens[2].TokenType);
        Assert.Equal("quoted ", tokens[2].Value);
        Assert.Equal(MarkdownTokenType.Text, tokens[3].TokenType);
        Assert.Equal("text", tokens[3].Value);
        Assert.Equal(MarkdownTokenType.ParagraphBlockEnd, tokens[4].TokenType);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestHorizontalRuleDash()
    {
        var markdown = "---";
        var (tokens, text) = Tokenize(markdown);
        Assert.Single(tokens);
        Assert.Equal(MarkdownTokenType.HorizontalRule, tokens[0].TokenType);
        Assert.Equal("---", tokens[0].Value);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestHorizontalRuleAsterisk()
    {
        var markdown = "***";
        var (tokens, text) = Tokenize(markdown);
        Assert.Single(tokens);
        Assert.Equal(MarkdownTokenType.HorizontalRule, tokens[0].TokenType);
        Assert.Equal("***", tokens[0].Value);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestHorizontalRuleDashLf()
    {
        var markdown = "---\n";
        var (tokens, text) = Tokenize(markdown);
        Assert.Single(tokens);
        Assert.Equal(MarkdownTokenType.HorizontalRule, tokens[0].TokenType);
        Assert.Equal("---", tokens[0].Value);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestHorizontalRuleAsteriskLf()
    {
        var markdown = "***\n";
        var (tokens, text) = Tokenize(markdown);
        Assert.Single(tokens);
        Assert.Equal(MarkdownTokenType.HorizontalRule, tokens[0].TokenType);
        Assert.Equal("***", tokens[0].Value);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestHorizontalRuleDashCrLf()
    {
        var markdown = "---\r\n";
        var (tokens, text) = Tokenize(markdown);
        Assert.Single(tokens);
        Assert.Equal(MarkdownTokenType.HorizontalRule, tokens[0].TokenType);
        Assert.Equal("---", tokens[0].Value);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestHorizontalRuleAsteriskCrLf()
    {
        var markdown = "***\r\n";
        var (tokens, text) = Tokenize(markdown);
        Assert.Single(tokens);
        Assert.Equal(MarkdownTokenType.HorizontalRule, tokens[0].TokenType);
        Assert.Equal("***", tokens[0].Value);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestEmoji()
    {
        var markdown = ":smile:";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(3, tokens.Count);
        Assert.Equal(MarkdownTokenType.ParagraphBlockStart, tokens[0].TokenType);
        Assert.Equal(MarkdownTokenType.Emoji, tokens[1].TokenType);
        Assert.Equal("smile", tokens[1].Value);
        Assert.NotNull(tokens[1].Metadata);
        Assert.IsType<EmojiMetadata>(tokens[1].Metadata);
        Assert.Equal("smile", ((EmojiMetadata)tokens[1].Metadata!).Name);
        Assert.Equal(MarkdownTokenType.ParagraphBlockEnd, tokens[2].TokenType);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestSubscript()
    {
        var markdown = "^sub^";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(3, tokens.Count);
        Assert.Equal(MarkdownTokenType.ParagraphBlockStart, tokens[0].TokenType);
        Assert.Equal(MarkdownTokenType.Subscript, tokens[1].TokenType);
        Assert.Equal("sub", tokens[1].Value);
        Assert.Equal(MarkdownTokenType.ParagraphBlockEnd, tokens[2].TokenType);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestSuperscript()
    {
        var markdown = "~sup~";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(3, tokens.Count);
        Assert.Equal(MarkdownTokenType.ParagraphBlockStart, tokens[0].TokenType);
        Assert.Equal(MarkdownTokenType.Superscript, tokens[1].TokenType);
        Assert.Equal("sup", tokens[1].Value);
        Assert.Equal(MarkdownTokenType.ParagraphBlockEnd, tokens[2].TokenType);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestInsertedText()
    {
        var markdown = "++inserted++";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(3, tokens.Count);
        Assert.Equal(MarkdownTokenType.ParagraphBlockStart, tokens[0].TokenType);
        Assert.Equal(MarkdownTokenType.InsertedText, tokens[1].TokenType);
        Assert.Equal("inserted", tokens[1].Value);
        Assert.Equal(MarkdownTokenType.ParagraphBlockEnd, tokens[2].TokenType);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestMarkedText()
    {
        var markdown = "==marked==";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(3, tokens.Count);
        Assert.Equal(MarkdownTokenType.ParagraphBlockStart, tokens[0].TokenType);
        Assert.Equal(MarkdownTokenType.MarkedText, tokens[1].TokenType);
        Assert.Equal("marked", tokens[1].Value);
        Assert.Equal(MarkdownTokenType.ParagraphBlockEnd, tokens[2].TokenType);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestHtmlTag()
    {
        var markdown = "<div>";
        var (tokens, text) = Tokenize(markdown);
        // A line that starts with an inline HTML tag is a paragraph.
        Assert.Equal(3, tokens.Count);
        Assert.Equal(MarkdownTokenType.ParagraphBlockStart, tokens[0].TokenType);
        Assert.Equal(MarkdownTokenType.HtmlTag, tokens[1].TokenType);
        Assert.Equal("<div>", tokens[1].Value);
        Assert.Equal(MarkdownTokenType.ParagraphBlockEnd, tokens[2].TokenType);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestHtmlClosingTag()
    {
        var markdown = "</div>";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(3, tokens.Count);
        Assert.Equal(MarkdownTokenType.ParagraphBlockStart, tokens[0].TokenType);
        Assert.Equal(MarkdownTokenType.HtmlTag, tokens[1].TokenType);
        Assert.Equal("</div>", tokens[1].Value);
        Assert.Equal(MarkdownTokenType.ParagraphBlockEnd, tokens[2].TokenType);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestCustomContainer()
    {
        var markdown = "::: warning";
        var (tokens, text) = Tokenize(markdown);
        Assert.Single(tokens);
        Assert.Equal(MarkdownTokenType.CustomContainer, tokens[0].TokenType);
        Assert.Equal("warning", tokens[0].Value);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestMixedTextAndBold()
    {
        var markdown = "Hello **world**";
        var (tokens, text) = Tokenize(markdown);
        // A plain line is a paragraph: PStart, text, bold, PEnd.
        Assert.Equal(4, tokens.Count);
        Assert.Equal(MarkdownTokenType.ParagraphBlockStart, tokens[0].TokenType);
        Assert.Equal(MarkdownTokenType.Text, tokens[1].TokenType);
        Assert.Equal("Hello ", tokens[1].Value);
        Assert.Equal(MarkdownTokenType.Bold, tokens[2].TokenType);
        Assert.Equal("world", tokens[2].Value);
        Assert.Equal(MarkdownTokenType.ParagraphBlockEnd, tokens[3].TokenType);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestMixedInlineElements()
    {
        var markdown = "Text with **bold** and *italic* and `code`";
        var (tokens, text) = Tokenize(markdown);
        // A plain line is a paragraph: PStart, inline content, PEnd.
        Assert.Equal(9, tokens.Count);
        Assert.Equal(MarkdownTokenType.ParagraphBlockStart, tokens[0].TokenType);
        Assert.Equal(MarkdownTokenType.Text, tokens[1].TokenType);
        Assert.Equal("Text ", tokens[1].Value);
        Assert.Equal(MarkdownTokenType.Text, tokens[2].TokenType);
        Assert.Equal("with ", tokens[2].Value);
        Assert.Equal(MarkdownTokenType.Bold, tokens[3].TokenType);
        Assert.Equal("bold", tokens[3].Value);
        Assert.Equal(MarkdownTokenType.Text, tokens[4].TokenType);
        Assert.Equal(" and ", tokens[4].Value);
        Assert.Equal(MarkdownTokenType.Italic, tokens[5].TokenType);
        Assert.Equal("italic", tokens[5].Value);
        Assert.Equal(MarkdownTokenType.Text, tokens[6].TokenType);
        Assert.Equal(" and ", tokens[6].Value);
        Assert.Equal(MarkdownTokenType.CodeInline, tokens[7].TokenType);
        Assert.Equal("code", tokens[7].Value);
        Assert.Equal(MarkdownTokenType.ParagraphBlockEnd, tokens[8].TokenType);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestMultilineDocument()
    {
        var markdown = @"# Title
This is text.
## Subtitle
More text.";

        var (tokens, text) = Tokenize(markdown);

        // Verify we have the key tokens (headings have empty value, content via inline tokens)
        Assert.Contains(tokens, t => t.TokenType == MarkdownTokenType.Heading && ((HeadingMetadata)t.Metadata!).Level == 1);
        Assert.Contains(tokens, t => t.TokenType == MarkdownTokenType.Text && t.Value == "Title"); // Inline token
        Assert.Contains(tokens, t => t.TokenType == MarkdownTokenType.Text && t.Value.Contains("This "));
        Assert.Contains(tokens, t => t.TokenType == MarkdownTokenType.Text && t.Value.Contains("is "));
        Assert.Contains(tokens, t => t.TokenType == MarkdownTokenType.Text && t.Value.Contains("text."));
        Assert.Contains(tokens, t => t.TokenType == MarkdownTokenType.Heading && ((HeadingMetadata)t.Metadata!).Level == 2);
        Assert.Contains(tokens, t => t.TokenType == MarkdownTokenType.Text && t.Value == "Subtitle"); // Inline token
        Assert.Contains(tokens, t => t.TokenType == MarkdownTokenType.Text && t.Value.Contains("More "));
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestListWithMultipleItems()
    {
        var markdown = "- Item 1\n- Item 2\n- Item 3";

        var (tokens, text) = Tokenize(markdown);
        // The item paragraphs are closed when the sibling item begins; each item's closing
        // newline is emitted as a Text("\n") separator after its PEnd (faithful whitespace).
        Assert.Equal(19, tokens.Count);
        Assert.Equal(MarkdownTokenType.ListStart, tokens[0].TokenType);
        Assert.Equal(MarkdownTokenType.UnorderedListItem, tokens[1].TokenType);
        Assert.Equal(string.Empty, tokens[1].Value); // List items have empty value
        Assert.Equal(MarkdownTokenType.ParagraphBlockStart, tokens[2].TokenType);
        Assert.Equal(MarkdownTokenType.Text, tokens[3].TokenType);
        Assert.Equal("Item ", tokens[3].Value);
        Assert.Equal(MarkdownTokenType.Text, tokens[4].TokenType);
        Assert.Equal("1", tokens[4].Value);
        Assert.Equal(MarkdownTokenType.ParagraphBlockEnd, tokens[5].TokenType);
        Assert.Equal(MarkdownTokenType.Text, tokens[6].TokenType);
        Assert.Equal("\n", tokens[6].Value);
        Assert.Equal(MarkdownTokenType.UnorderedListItem, tokens[7].TokenType);
        Assert.Equal(string.Empty, tokens[7].Value);
        Assert.Equal(MarkdownTokenType.ParagraphBlockStart, tokens[8].TokenType);
        Assert.Equal(MarkdownTokenType.Text, tokens[9].TokenType);
        Assert.Equal("Item ", tokens[9].Value);
        Assert.Equal(MarkdownTokenType.Text, tokens[10].TokenType);
        Assert.Equal("2", tokens[10].Value);
        Assert.Equal(MarkdownTokenType.ParagraphBlockEnd, tokens[11].TokenType);
        Assert.Equal(MarkdownTokenType.Text, tokens[12].TokenType);
        Assert.Equal("\n", tokens[12].Value);
        Assert.Equal(MarkdownTokenType.UnorderedListItem, tokens[13].TokenType);
        Assert.Equal(MarkdownTokenType.ParagraphBlockStart, tokens[14].TokenType);
        Assert.Equal(MarkdownTokenType.Text, tokens[15].TokenType);
        Assert.Equal("Item ", tokens[15].Value);
        Assert.Equal(MarkdownTokenType.Text, tokens[16].TokenType);
        Assert.Equal("3", tokens[16].Value);
        Assert.Equal(MarkdownTokenType.ParagraphBlockEnd, tokens[17].TokenType);
        Assert.Equal(MarkdownTokenType.ListEnd, tokens[18].TokenType);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestListWithTrickyCppMultipleItems()
    {
        var markdown = "**supported languages**:\n- c/c++\n- java\n";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(2, tokens.Where(t => t.TokenType == MarkdownTokenType.UnorderedListItem).Count());
    }

    [Fact]
    public void TestComplexDocument()
    {
        var markdown = @"# My Document

This is **bold** and *italic*.

## Code Example

```csharp
var x = 1;
```

Visit [Google](https://google.com) for more.";

        var (tokens, text) = Tokenize(markdown);

        // Verify we have expected tokens (headings have empty value, content via inline tokens)
        Assert.Contains(tokens, t => t.TokenType == MarkdownTokenType.Heading && ((HeadingMetadata)t.Metadata!).Level == 1);
        Assert.Contains(tokens, t => t.TokenType == MarkdownTokenType.Text && t.Value == "My Document"); // Inline token
        Assert.Contains(tokens, t => t.TokenType == MarkdownTokenType.Bold && t.Value == "bold");
        Assert.Contains(tokens, t => t.TokenType == MarkdownTokenType.Italic && t.Value == "italic");
        Assert.Contains(tokens, t => t.TokenType == MarkdownTokenType.Heading && ((HeadingMetadata)t.Metadata!).Level == 2);
        Assert.Contains(tokens, t => t.TokenType == MarkdownTokenType.Text && t.Value == "Code Example"); // Inline token
        Assert.Contains(tokens, t => t.TokenType == MarkdownTokenType.CodeBlock && string.IsNullOrEmpty(t.Value)); // Code block has empty value
        Assert.Contains(tokens, t => t.TokenType == MarkdownTokenType.Link && t.Value == "[Google](https://google.com)");
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestEmptyInput()
    {
        var markdown = "";
        var (tokens, text) = Tokenize(markdown);
        Assert.Empty(tokens);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestNewlineOnly()
    {
        var markdown = "\n";
        var (tokens, text) = Tokenize(markdown);
        Assert.Single(tokens);
        Assert.Equal(MarkdownTokenType.Text, tokens[0].TokenType);
        Assert.Equal("\n", tokens[0].Value);
        Assert.Equal(markdown, text);
    }

    // Whitespace fidelity: the token stream is a faithful representation of the input. A
    // newline at a paragraph boundary is emitted after the PEnd as a plain Text("\n")
    // separator, so no newline of the input is ever dropped and the blank-line count is
    // preserved (no normalization). These tests are the guard that would have caught the
    // "blank line drops newlines" regression.

    [Fact]
    public void TestTrailingNewlineIsPreserved()
    {
        var markdown = "A\n";
        var (tokens, text) = Tokenize(markdown);
        // The document's trailing newline survives as a Text("\n") separator after the PEnd.
        Assert.Equal(4, tokens.Count);
        Assert.Equal(MarkdownTokenType.ParagraphBlockStart, tokens[0].TokenType);
        Assert.Equal(MarkdownTokenType.Text, tokens[1].TokenType);
        Assert.Equal("A", tokens[1].Value);
        Assert.Equal(MarkdownTokenType.ParagraphBlockEnd, tokens[2].TokenType);
        Assert.Equal(MarkdownTokenType.Text, tokens[3].TokenType);
        Assert.Equal("\n", tokens[3].Value);
        Assert.Equal(markdown, text);
        Assert.Equal(markdown, TokenText(tokens));
    }

    [Fact]
    public void TestTripleBlankLinePreservesExactNewlineCount()
    {
        var markdown = "A\n\n\nB";
        var (tokens, text) = Tokenize(markdown);
        // Three newlines total (A's closing newline + two blank lines). The blank-line count
        // is preserved exactly - no normalization - so ascii-art layout survives.
        Assert.Equal(markdown, text);
        Assert.Equal(markdown, TokenText(tokens));
        var newlines = tokens.Where(t => t.TokenType == MarkdownTokenType.Text && t.Value == "\n").ToList();
        Assert.Equal(3, newlines.Count);
        // The three newline separators sit between the two paragraphs, after the first PEnd.
        Assert.Equal(9, tokens.Count);
        Assert.Equal(MarkdownTokenType.ParagraphBlockEnd, tokens[2].TokenType);
        Assert.Equal(MarkdownTokenType.Text, tokens[3].TokenType);
        Assert.Equal(MarkdownTokenType.Text, tokens[4].TokenType);
        Assert.Equal(MarkdownTokenType.Text, tokens[5].TokenType);
        Assert.Equal(MarkdownTokenType.ParagraphBlockStart, tokens[6].TokenType);
    }

    [Fact]
    public void TestBlankLineBeforeHeading()
    {
        var markdown = "A\n\n# H";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(7, tokens.Count);
        Assert.Equal(MarkdownTokenType.ParagraphBlockStart, tokens[0].TokenType);
        Assert.Equal(MarkdownTokenType.Text, tokens[1].TokenType);
        Assert.Equal("A", tokens[1].Value);
        Assert.Equal(MarkdownTokenType.ParagraphBlockEnd, tokens[2].TokenType);
        Assert.Equal(MarkdownTokenType.Text, tokens[3].TokenType);
        Assert.Equal("\n", tokens[3].Value); // A's closing newline, after the PEnd
        Assert.Equal(MarkdownTokenType.Text, tokens[4].TokenType);
        Assert.Equal("\n", tokens[4].Value); // the blank line
        Assert.Equal(MarkdownTokenType.Heading, tokens[5].TokenType);
        Assert.Equal(MarkdownTokenType.Text, tokens[6].TokenType);
        Assert.Equal("H", tokens[6].Value);
        // The heading marker and the closing newline are consumed by the heading construct;
        // the body text plus the separators equal the paragraph-and-blank portion.
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestBlankLineBeforeList()
    {
        var markdown = "A\n\n- item";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(11, tokens.Count);
        Assert.Equal(MarkdownTokenType.ParagraphBlockStart, tokens[0].TokenType);
        Assert.Equal(MarkdownTokenType.Text, tokens[1].TokenType);
        Assert.Equal("A", tokens[1].Value);
        Assert.Equal(MarkdownTokenType.ParagraphBlockEnd, tokens[2].TokenType);
        Assert.Equal(MarkdownTokenType.Text, tokens[3].TokenType);
        Assert.Equal("\n", tokens[3].Value);
        Assert.Equal(MarkdownTokenType.Text, tokens[4].TokenType);
        Assert.Equal("\n", tokens[4].Value);
        Assert.Equal(MarkdownTokenType.ListStart, tokens[5].TokenType);
        Assert.Equal(MarkdownTokenType.UnorderedListItem, tokens[6].TokenType);
        Assert.Equal(MarkdownTokenType.ParagraphBlockStart, tokens[7].TokenType);
        Assert.Equal(MarkdownTokenType.Text, tokens[8].TokenType);
        Assert.Equal("item", tokens[8].Value);
        Assert.Equal(MarkdownTokenType.ParagraphBlockEnd, tokens[9].TokenType);
        Assert.Equal(MarkdownTokenType.ListEnd, tokens[10].TokenType);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestBlankLineBeforeCodeFence()
    {
        var markdown = "A\n\n```";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(6, tokens.Count);
        Assert.Equal(MarkdownTokenType.ParagraphBlockStart, tokens[0].TokenType);
        Assert.Equal(MarkdownTokenType.Text, tokens[1].TokenType);
        Assert.Equal("A", tokens[1].Value);
        Assert.Equal(MarkdownTokenType.ParagraphBlockEnd, tokens[2].TokenType);
        Assert.Equal(MarkdownTokenType.Text, tokens[3].TokenType);
        Assert.Equal("\n", tokens[3].Value);
        Assert.Equal(MarkdownTokenType.Text, tokens[4].TokenType);
        Assert.Equal("\n", tokens[4].Value);
        Assert.Equal(MarkdownTokenType.CodeBlock, tokens[5].TokenType);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestCrlfInputIsFaithfulAfterHouseNormalization()
    {
        // CRLF line endings are normalized to LF by the house rule; the newline count and
        // the surrounding text are preserved, so the token text equals the LF-normalized input.
        var markdown = "one\r\n\r\ntwo";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(8, tokens.Count);
        Assert.Equal(MarkdownTokenType.ParagraphBlockStart, tokens[0].TokenType);
        Assert.Equal(MarkdownTokenType.Text, tokens[1].TokenType);
        Assert.Equal("one", tokens[1].Value);
        Assert.Equal(MarkdownTokenType.ParagraphBlockEnd, tokens[2].TokenType);
        Assert.Equal(MarkdownTokenType.Text, tokens[3].TokenType);
        Assert.Equal("\n", tokens[3].Value);
        Assert.Equal(MarkdownTokenType.Text, tokens[4].TokenType);
        Assert.Equal("\n", tokens[4].Value);
        Assert.Equal(MarkdownTokenType.ParagraphBlockStart, tokens[5].TokenType);
        Assert.Equal(MarkdownTokenType.Text, tokens[6].TokenType);
        Assert.Equal("two", tokens[6].Value);
        Assert.Equal(MarkdownTokenType.ParagraphBlockEnd, tokens[7].TokenType);
        Assert.Equal(markdown.Replace("\r\n", "\n"), TokenText(tokens));
    }

    [Fact]
    public void TestCrlfTrailingNewlineIsPreserved()
    {
        var markdown = "A\r\n";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(4, tokens.Count);
        Assert.Equal(MarkdownTokenType.Text, tokens[3].TokenType);
        Assert.Equal("\n", tokens[3].Value);
        Assert.Equal("A\n", TokenText(tokens));
    }

    // The structural fidelity invariant for prose-only documents (paragraphs + blank lines):
    // the concatenation of every token value equals the (LF-normalized) input. This single
    // invariant is the guard that would have caught the "blank line drops newlines" bug and
    // protects all future tokenizer changes to prose. Marker constructs (lists, quotes,
    // fences) consume their left-side syntax by design, so the strict invariant applies here
    // only to prose; per-construct tests keep subtracting their consumed markers as before.

    [Theory]
    [InlineData("para one\n\npara two\n\npara three\n")]
    [InlineData("para one\r\n\r\npara two\r\n\r\npara three")]
    [InlineData("single line")]
    [InlineData("line one\nline two\n\nline three\nline four")]
    public void TestProseTokenTextEqualsInput(string markdown)
    {
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(markdown.Replace("\r\n", "\n"), TokenText(tokens));
        // No Text("\n") separator may ever be produced inside an open paragraph: every newline
        // separator sits after a PEnd. Verify the token stream is balanced and well-formed.
        Assert.Equal(
            tokens.Count(t => t.TokenType == MarkdownTokenType.ParagraphBlockStart),
            tokens.Count(t => t.TokenType == MarkdownTokenType.ParagraphBlockEnd));
    }

    [Fact]
    public void TestBoldWithinText()
    {
        var markdown = "Start **bold** end";
        var (tokens, text) = Tokenize(markdown);
        // A plain line is a paragraph: PStart, text, bold, text, PEnd.
        Assert.Equal(5, tokens.Count);
        Assert.Equal(MarkdownTokenType.ParagraphBlockStart, tokens[0].TokenType);
        Assert.Equal(MarkdownTokenType.Text, tokens[1].TokenType);
        Assert.Equal("Start ", tokens[1].Value);
        Assert.Equal(MarkdownTokenType.Bold, tokens[2].TokenType);
        Assert.Equal("bold", tokens[2].Value);
        Assert.Equal(MarkdownTokenType.Text, tokens[3].TokenType);
        Assert.Equal(" end", tokens[3].Value);
        Assert.Equal(MarkdownTokenType.ParagraphBlockEnd, tokens[4].TokenType);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestLinkFollowedByText()
    {
        var markdown = "[link](url) text";
        var (tokens, text) = Tokenize(markdown);
        // A plain line is a paragraph: PStart, link, text, PEnd.
        Assert.Equal(4, tokens.Count);
        Assert.Equal(MarkdownTokenType.ParagraphBlockStart, tokens[0].TokenType);
        Assert.Equal(MarkdownTokenType.Link, tokens[1].TokenType);
        Assert.Equal("[link](url)", tokens[1].Value);
        Assert.Equal(MarkdownTokenType.Text, tokens[2].TokenType);
        Assert.Equal(" text", tokens[2].Value);
        Assert.Equal(MarkdownTokenType.ParagraphBlockEnd, tokens[3].TokenType);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestMultipleEmojisSeparated()
    {
        var markdown = ":smile: and :wink:";
        var (tokens, text) = Tokenize(markdown);
        // A plain line is a paragraph: PStart, emoji, text, emoji, PEnd.
        Assert.Equal(5, tokens.Count);
        Assert.Equal(MarkdownTokenType.ParagraphBlockStart, tokens[0].TokenType);
        Assert.Equal(MarkdownTokenType.Emoji, tokens[1].TokenType);
        Assert.Equal("smile", tokens[1].Value);
        Assert.Equal(MarkdownTokenType.Text, tokens[2].TokenType);
        Assert.Equal(" and ", tokens[2].Value);
        Assert.Equal(MarkdownTokenType.Emoji, tokens[3].TokenType);
        Assert.Equal("wink", tokens[3].Value);
        Assert.Equal(MarkdownTokenType.ParagraphBlockEnd, tokens[4].TokenType);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public async Task TestCancellation()
    {
        // Create a large markdown to parse
        var largeMarkdown = string.Join("\n\n", Enumerable.Range(1, 1000).Select(i => $"# Heading {i}\n\nSome text for paragraph {i}."));

        using var cts = new CancellationTokenSource();
        var tokens = new List<MarkdownToken>();
        int tokenCount = 0;

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(largeMarkdown));

        // Cancel after a few tokens
        var parseTask = Task.Run(async () =>
        {
            await MarkdownTokenizer.Create().ParseAsync(stream, cts.Token, token =>
            {
                tokens.Add(token);
                tokenCount++;
                if (tokenCount == 20)
                {
                    cts.Cancel();
                }
            });
        }, TestContext.Current.CancellationToken);

        await parseTask;

        // Should have stopped early
        Assert.True(tokenCount < 1000, "Tokenization should have been cancelled");
    }

    [Fact]
    public async Task TestOnInlinesCompletedCallbackIsInvoked()
    {
        var callbackInvoked = false;
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("# Heading\n"));

        await MarkdownTokenizer.Create().ParseAsync(stream, token =>
        {
            if (token.Metadata is HeadingMetadata headingMeta)
            {
                headingMeta.RegisterInlineTokenHandler(
                    _ => { /* inline handler */ },
                    () => { callbackInvoked = true; }
                );
            }
        });
        Assert.True(callbackInvoked, "The onInlinesCompleted callback should have been invoked");
    }

    [Fact]
    public async Task TestOnInlinesCompletedCallbackRunsBeforeParseCompletes()
    {
        var tokens = new List<MarkdownToken>();
        var callbackInvoked = false;

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("# Heading\nText after"));

        await MarkdownTokenizer.Create().ParseAsync(stream, token =>
        {
            tokens.Add(token);

            if (token.Metadata is HeadingMetadata headingMeta)
            {
                headingMeta.RegisterInlineTokenHandler(
                    _ => { /* inline handler */ },
                    () =>
                    {
                        callbackInvoked = true;
                        // The callback runs before CompleteProcessing, so the parser
                        // has not yet continued to emit subsequent tokens.
                        // At this point, only the heading token and its inline content
                        // should have been collected so far.
                    }
                );
            }
        });

        Assert.True(callbackInvoked, "Callback should have been invoked");

        // Verify the full token stream was produced after the callback
        Assert.Contains(tokens, t => t.TokenType == MarkdownTokenType.Heading);
        Assert.Contains(tokens, t => t.TokenType == MarkdownTokenType.Text && t.Value.StartsWith("Text"));
    }
}
