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
        Assert.Equal(6, tokens.Count);
        Assert.Equal(MarkdownTokenType.ParagraphBlockStart, tokens[0].TokenType);
        Assert.Equal(MarkdownTokenType.Text, tokens[1].TokenType);
        Assert.Equal("one", tokens[1].Value);
        Assert.Equal(MarkdownTokenType.ParagraphBlockEnd, tokens[2].TokenType);
        Assert.Equal(MarkdownTokenType.ParagraphBlockStart, tokens[3].TokenType);
        Assert.Equal(MarkdownTokenType.Text, tokens[4].TokenType);
        Assert.Equal("two", tokens[4].Value);
        Assert.Equal(MarkdownTokenType.ParagraphBlockEnd, tokens[5].TokenType);
        Assert.Equal(markdown, text);
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
    public void TestUnorderedListWithDash()
    {
        var markdown = "- item 1";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(2, tokens.Count);
        Assert.Equal(MarkdownTokenType.UnorderedListItem, tokens[0].TokenType);
        Assert.Equal(string.Empty, tokens[0].Value); // Unordered list items have empty value
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestUnorderedListWithPlus()
    {
        var markdown = "+ item 1";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(2, tokens.Count);
        Assert.Equal(MarkdownTokenType.UnorderedListItem, tokens[0].TokenType);
        Assert.Equal(string.Empty, tokens[0].Value); // Unordered list items have empty value
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestUnorderedListWithAsterisk()
    {
        var markdown = "* item 1";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(2, tokens.Count);
        Assert.Equal(MarkdownTokenType.UnorderedListItem, tokens[0].TokenType);
        Assert.Equal(string.Empty, tokens[0].Value); // Unordered list items have empty value
        Assert.Equal(MarkdownTokenType.Text, tokens[1].TokenType);
        Assert.Equal("item 1", tokens[1].Value);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestUnorderedListWithLeadingSpaces()
    {
        // Up to three leading spaces are part of the list item; four or more make the
        // line an indented code block (CommonMark 4.4), so this is code, not a list item.
        var markdown = "   * item 1";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(2, tokens.Count);
        Assert.Equal(MarkdownTokenType.UnorderedListItem, tokens[0].TokenType);
        Assert.Equal("   ", tokens[0].Value); // Indentation is in the Value
        Assert.Equal(MarkdownTokenType.Text, tokens[1].TokenType);
        Assert.Equal("item 1", tokens[1].Value);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestOrderedListWithLeadingSpaces()
    {
        var markdown = "  1. item 1";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(2, tokens.Count);
        Assert.Equal(MarkdownTokenType.OrderedListItem, tokens[0].TokenType);
        Assert.Equal("  ", tokens[0].Value); // Indentation is in the Value
        Assert.NotNull(tokens[0].Metadata);
        Assert.IsType<OrderedListItemMetadata>(tokens[0].Metadata);
        Assert.Equal(1, ((OrderedListItemMetadata)tokens[0].Metadata!).Number);
        Assert.Equal(MarkdownTokenType.Text, tokens[1].TokenType);
        Assert.Equal("item 1", tokens[1].Value);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestNestedUnorderedListItems()
    {
        // Nesting keeps the markers within three columns of indentation; a fourth column
        // would start an indented code block (CommonMark 4.4), not a deeper list item.
        var markdown = "- top\n  - nested\n   - deeper";
        var (tokens, text) = Tokenize(markdown);

        // top-level: no indentation
        Assert.Equal(MarkdownTokenType.UnorderedListItem, tokens[0].TokenType);
        Assert.Equal(string.Empty, tokens[0].Value);

        // nested: 2 spaces indentation
        Assert.Equal(MarkdownTokenType.UnorderedListItem, tokens[2].TokenType);
        Assert.Equal("  ", tokens[2].Value);

        // deeper: 3 spaces indentation
        Assert.Equal(MarkdownTokenType.UnorderedListItem, tokens[4].TokenType);
        Assert.Equal("   ", tokens[4].Value);

        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestOrderedList()
    {
        var markdown = "1. item 1";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(2, tokens.Count); // OrderedListItem token + inline text token
        Assert.Equal(MarkdownTokenType.OrderedListItem, tokens[0].TokenType);
        Assert.Equal(string.Empty, tokens[0].Value); // Value is empty when OnInlineToken is used
        Assert.NotNull(tokens[0].Metadata);
        Assert.IsType<OrderedListItemMetadata>(tokens[0].Metadata);
        Assert.Equal(1, ((OrderedListItemMetadata)tokens[0].Metadata!).Number);

        // Inline content
        Assert.Equal(MarkdownTokenType.Text, tokens[1].TokenType);
        Assert.Equal("item 1", tokens[1].Value);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestOrderedListMultipleDigits()
    {
        var markdown = "42. item";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(2, tokens.Count); // OrderedListItem token + inline text token
        Assert.Equal(MarkdownTokenType.OrderedListItem, tokens[0].TokenType);
        Assert.Equal(string.Empty, tokens[0].Value); // Value is empty when OnInlineToken is used
        Assert.Equal(42, ((OrderedListItemMetadata)tokens[0].Metadata!).Number);

        // Inline content
        Assert.Equal(MarkdownTokenType.Text, tokens[1].TokenType);
        Assert.Equal("item", tokens[1].Value);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestBlockquote()
    {
        var markdown = "> quoted text";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(2, tokens.Count); // Blockquote token + inline text token
        Assert.Equal(MarkdownTokenType.Blockquote, tokens[0].TokenType);
        Assert.Equal(string.Empty, tokens[0].Value); // Value is empty when OnInlineToken is used

        // Inline content
        Assert.Equal(MarkdownTokenType.Text, tokens[1].TokenType);
        Assert.Equal("quoted text", tokens[1].Value);
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
        Assert.Equal(6, tokens.Count);
        Assert.Equal(MarkdownTokenType.UnorderedListItem, tokens[0].TokenType);
        Assert.Equal(string.Empty, tokens[0].Value); // List items have empty value
        Assert.Equal(MarkdownTokenType.UnorderedListItem, tokens[2].TokenType);
        Assert.Equal(string.Empty, tokens[2].Value);
        Assert.Equal(MarkdownTokenType.UnorderedListItem, tokens[4].TokenType);
        Assert.Equal(string.Empty, tokens[4].Value); // List items have empty value
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
    public void TestOnInlinesCompletedCallbackIsInvoked()
    {
        var callbackInvoked = false;
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("# Heading\n"));

        MarkdownTokenizer.Create().ParseAsync(stream, token =>
        {
            if (token.Metadata is HeadingMetadata headingMeta)
            {
                headingMeta.RegisterInlineTokenHandler(
                    _ => { /* inline handler */ },
                    () => { callbackInvoked = true; }
                );
            }
        }).GetAwaiter().GetResult();

        Assert.True(callbackInvoked, "The onInlinesCompleted callback should have been invoked");
    }

    [Fact]
    public void TestOnInlinesCompletedCallbackRunsBeforeParseCompletes()
    {
        var tokens = new List<MarkdownToken>();
        var callbackInvoked = false;

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("# Heading\nText after"));

        MarkdownTokenizer.Create().ParseAsync(stream, token =>
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
        }).GetAwaiter().GetResult();

        Assert.True(callbackInvoked, "Callback should have been invoked");

        // Verify the full token stream was produced after the callback
        Assert.Contains(tokens, t => t.TokenType == MarkdownTokenType.Heading);
        Assert.Contains(tokens, t => t.TokenType == MarkdownTokenType.Text && t.Value.StartsWith("Text"));
    }
}
