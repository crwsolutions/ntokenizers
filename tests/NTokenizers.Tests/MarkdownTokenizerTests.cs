using NTokenizers.Markdown;
using NTokenizers.Markdown.Metadata;
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
        Assert.Equal(2, tokens.Count);
        Assert.Equal(MarkdownTokenType.Text, tokens[0].TokenType);
        Assert.Equal("Hello ", tokens[0].Value);
        Assert.Equal(MarkdownTokenType.Text, tokens[1].TokenType);
        Assert.Equal("world", tokens[1].Value);
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
        Assert.Single(tokens);
        Assert.Equal(MarkdownTokenType.Bold, tokens[0].TokenType);
        Assert.Equal("bold text", tokens[0].Value);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestBoldAlternativeText()
    {
        var markdown = "__bold text__";
        var (tokens, text) = Tokenize(markdown);
        Assert.Single(tokens);
        Assert.Equal(MarkdownTokenType.Bold, tokens[0].TokenType);
        Assert.Equal("bold text", tokens[0].Value);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestItalicText()
    {
        var markdown = "*italic text*";
        var (tokens, text) = Tokenize(markdown);
        Assert.Single(tokens);
        Assert.Equal(MarkdownTokenType.Italic, tokens[0].TokenType);
        Assert.Equal("italic text", tokens[0].Value);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestItalicAlternativeText()
    {
        var markdown = "_italic text_";
        var (tokens, text) = Tokenize(markdown);
        Assert.Single(tokens);
        Assert.Equal(MarkdownTokenType.Italic, tokens[0].TokenType);
        Assert.Equal("italic text", tokens[0].Value);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestInlineCode()
    {
        var markdown = "`code`";
        var (tokens, text) = Tokenize(markdown);
        Assert.Single(tokens);
        Assert.Equal(MarkdownTokenType.CodeInline, tokens[0].TokenType);
        Assert.Equal("code", tokens[0].Value);
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

        // For non-specialized languages like "javascript", metadata is CodeBlockMetadata<MarkdownToken>
        var codeMeta = codeBlock.Metadata as dynamic;
        Assert.NotNull(codeMeta);
        Assert.Equal("javascript", codeMeta.Language);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestLink()
    {
        var markdown = "[link text](http://example.com)";
        var (tokens, text) = Tokenize(markdown);
        Assert.Single(tokens);
        Assert.Equal(MarkdownTokenType.Link, tokens[0].TokenType);
        Assert.Equal("link text", tokens[0].Value);
        Assert.NotNull(tokens[0].Metadata);
        Assert.IsType<LinkMetadata>(tokens[0].Metadata);
        Assert.Equal("http://example.com", ((LinkMetadata)tokens[0].Metadata!).Url);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestLinkWithTitle()
    {
        var markdown = "[link text](http://example.com \"title\")";
        var (tokens, text) = Tokenize(markdown);
        Assert.Single(tokens);
        Assert.Equal(MarkdownTokenType.Link, tokens[0].TokenType);
        Assert.Equal("link text", tokens[0].Value);
        var metadata = (LinkMetadata)tokens[0].Metadata!;
        Assert.Equal("http://example.com", metadata.Url);
        Assert.Equal("title", metadata.Title);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestImage()
    {
        var markdown = "![alt text](http://example.com/image.png)";
        var (tokens, text) = Tokenize(markdown);
        Assert.Single(tokens);
        Assert.Equal(MarkdownTokenType.Image, tokens[0].TokenType);
        Assert.Equal("alt text", tokens[0].Value);
        Assert.NotNull(tokens[0].Metadata);
        Assert.IsType<LinkMetadata>(tokens[0].Metadata);
        Assert.Equal("http://example.com/image.png", ((LinkMetadata)tokens[0].Metadata!).Url);
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
        var markdown = "     * item 1";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(3, tokens.Count);
        Assert.Equal(MarkdownTokenType.Text, tokens[0].TokenType);
        Assert.Equal("     ", tokens[0].Value);
        Assert.Equal(MarkdownTokenType.UnorderedListItem, tokens[1].TokenType);
        Assert.Equal(string.Empty, tokens[1].Value);
        Assert.Equal(MarkdownTokenType.Text, tokens[2].TokenType);
        Assert.Equal("item 1", tokens[2].Value);
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
        Assert.Single(tokens);
        Assert.Equal(MarkdownTokenType.Emoji, tokens[0].TokenType);
        Assert.Equal("smile", tokens[0].Value);
        Assert.NotNull(tokens[0].Metadata);
        Assert.IsType<EmojiMetadata>(tokens[0].Metadata);
        Assert.Equal("smile", ((EmojiMetadata)tokens[0].Metadata!).Name);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestSubscript()
    {
        var markdown = "^sub^";
        var (tokens, text) = Tokenize(markdown);
        Assert.Single(tokens);
        Assert.Equal(MarkdownTokenType.Subscript, tokens[0].TokenType);
        Assert.Equal("sub", tokens[0].Value);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestSuperscript()
    {
        var markdown = "~sup~";
        var (tokens, text) = Tokenize(markdown);
        Assert.Single(tokens);
        Assert.Equal(MarkdownTokenType.Superscript, tokens[0].TokenType);
        Assert.Equal("sup", tokens[0].Value);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestInsertedText()
    {
        var markdown = "++inserted++";
        var (tokens, text) = Tokenize(markdown);
        Assert.Single(tokens);
        Assert.Equal(MarkdownTokenType.InsertedText, tokens[0].TokenType);
        Assert.Equal("inserted", tokens[0].Value);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestMarkedText()
    {
        var markdown = "==marked==";
        var (tokens, text) = Tokenize(markdown);
        Assert.Single(tokens);
        Assert.Equal(MarkdownTokenType.MarkedText, tokens[0].TokenType);
        Assert.Equal("marked", tokens[0].Value);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestHtmlTag()
    {
        var markdown = "<div>";
        var (tokens, text) = Tokenize(markdown);
        Assert.Single(tokens);
        Assert.Equal(MarkdownTokenType.HtmlTag, tokens[0].TokenType);
        Assert.Equal("<div>", tokens[0].Value);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestHtmlClosingTag()
    {
        var markdown = "</div>";
        var (tokens, text) = Tokenize(markdown);
        Assert.Single(tokens);
        Assert.Equal(MarkdownTokenType.HtmlTag, tokens[0].TokenType);
        Assert.Equal("</div>", tokens[0].Value);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestTableCell()
    {
        var markdown = "| cell 1 |";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(4, tokens.Count);
        Assert.Equal(MarkdownTokenType.Table, tokens[0].TokenType);
        Assert.Equal(string.Empty, tokens[0].Value);
        Assert.Equal(MarkdownTokenType.TableRow, tokens[1].TokenType);
        Assert.Equal(string.Empty, tokens[1].Value);
        Assert.Equal(MarkdownTokenType.TableCell, tokens[2].TokenType);
        Assert.Equal(string.Empty, tokens[2].Value);
        Assert.Equal(MarkdownTokenType.Text, tokens[3].TokenType);
        Assert.Equal(" cell 1 ", tokens[3].Value);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestTableCellBold()
    {
        var markdown = "|**bold**|";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(4, tokens.Count);
        Assert.Equal(MarkdownTokenType.Table, tokens[0].TokenType);
        Assert.Equal(string.Empty, tokens[0].Value);
        Assert.Equal(MarkdownTokenType.TableRow, tokens[1].TokenType);
        Assert.Equal(string.Empty, tokens[1].Value);
        Assert.Equal(MarkdownTokenType.TableCell, tokens[2].TokenType);
        Assert.Equal(string.Empty, tokens[2].Value);
        Assert.Equal(MarkdownTokenType.Bold, tokens[3].TokenType);
        Assert.Equal("bold", tokens[3].Value);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestTableWithTwoCells()
    {
        var markdown = "| | |";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(6, tokens.Count);
        Assert.Equal(MarkdownTokenType.Table, tokens[0].TokenType);
        Assert.Equal(string.Empty, tokens[0].Value);
        Assert.Equal(MarkdownTokenType.TableRow, tokens[1].TokenType);
        Assert.Equal(string.Empty, tokens[1].Value); // Table cells have empty value
        Assert.Equal(MarkdownTokenType.TableCell, tokens[2].TokenType);
        Assert.Equal(string.Empty, tokens[2].Value); // Table cells have empty value
        Assert.Equal(MarkdownTokenType.Text, tokens[3].TokenType);
        Assert.Equal(" ", tokens[3].Value);
        Assert.Equal(MarkdownTokenType.TableCell, tokens[4].TokenType);
        Assert.Equal(string.Empty, tokens[4].Value); // Table cells have empty value
        Assert.Equal(MarkdownTokenType.Text, tokens[5].TokenType);
        Assert.Equal(" ", tokens[5].Value);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestTableWithTwoRows()
    {
        var markdown = "| |\r\n| |";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(7, tokens.Count);
        Assert.Equal(MarkdownTokenType.Table, tokens[0].TokenType);
        Assert.Equal(string.Empty, tokens[0].Value);
        Assert.Equal(MarkdownTokenType.TableRow, tokens[1].TokenType);
        Assert.Equal(string.Empty, tokens[1].Value);
        Assert.Equal(MarkdownTokenType.TableCell, tokens[2].TokenType);
        Assert.Equal(string.Empty, tokens[2].Value);
        Assert.Equal(MarkdownTokenType.Text, tokens[3].TokenType);
        Assert.Equal(" ", tokens[3].Value);
        Assert.Equal(MarkdownTokenType.TableRow, tokens[4].TokenType);
        Assert.Equal(string.Empty, tokens[4].Value);
        Assert.Equal(MarkdownTokenType.TableCell, tokens[5].TokenType);
        Assert.Equal(string.Empty, tokens[5].Value);
        Assert.Equal(MarkdownTokenType.Text, tokens[6].TokenType);
        Assert.Equal(" ", tokens[6].Value);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestTableAlignments()
    {
        var markdown = "|----|----|---|";
        var (tokens, text) = Tokenize(markdown);
        var tableMetadata = tokens[0].Metadata as TableMetadata;
        Assert.Equal(2, tokens.Count);
        Assert.Equal(MarkdownTokenType.Table, tokens[0].TokenType);
        Assert.Equal(string.Empty, tokens[0].Value);
        Assert.Equal(MarkdownTokenType.TableAlignments, tokens[1].TokenType);
        Assert.Equal(string.Empty, tokens[1].Value);
        Assert.Equal(3, tableMetadata!.Alignments!.Count);
        Assert.Equal(Justify.Left, tableMetadata!.Alignments![0]);
        Assert.Equal(Justify.Left, tableMetadata!.Alignments![1]);
        Assert.Equal(Justify.Left, tableMetadata!.Alignments![2]);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestTableWithTwoCellsAndAlignment()
    {
        var markdown = "| | | |\r\n|:---|:---:|---:|\r\n";
        var (tokens, text) = Tokenize(markdown);
        var tableMetadata = tokens[0].Metadata as TableMetadata;
        Assert.Equal(9, tokens.Count);
        Assert.Equal(MarkdownTokenType.Table, tokens[0].TokenType);
        Assert.Equal(string.Empty, tokens[0].Value);
        Assert.Equal(MarkdownTokenType.TableRow, tokens[1].TokenType);
        Assert.Equal(string.Empty, tokens[1].Value);
        Assert.Equal(MarkdownTokenType.TableCell, tokens[2].TokenType);
        Assert.Equal(string.Empty, tokens[2].Value);
        Assert.Equal(MarkdownTokenType.Text, tokens[3].TokenType);
        Assert.Equal(" ", tokens[3].Value);
        Assert.Equal(MarkdownTokenType.TableCell, tokens[4].TokenType);
        Assert.Equal(string.Empty, tokens[4].Value);
        Assert.Equal(MarkdownTokenType.Text, tokens[5].TokenType);
        Assert.Equal(" ", tokens[5].Value);
        Assert.Equal(MarkdownTokenType.TableCell, tokens[6].TokenType);
        Assert.Equal(string.Empty, tokens[6].Value);
        Assert.Equal(MarkdownTokenType.Text, tokens[7].TokenType);
        Assert.Equal(" ", tokens[7].Value);
        Assert.Equal(MarkdownTokenType.TableAlignments, tokens[8].TokenType);
        Assert.Equal(string.Empty, tokens[8].Value);
        Assert.Equal(Justify.Left, tableMetadata!.Alignments![0]);
        Assert.Equal(Justify.Center, tableMetadata!.Alignments![1]);
        Assert.Equal(Justify.Right, tableMetadata!.Alignments![2]);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestTableInvalidCells()
    {
        var markdown = "|||";
        var (tokens, text) = Tokenize(markdown);
        Assert.Single(tokens);
        Assert.Equal(MarkdownTokenType.Text, tokens[0].TokenType);
        Assert.Equal("|||", tokens[0].Value);
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
        Assert.Equal(2, tokens.Count);
        Assert.Equal(MarkdownTokenType.Text, tokens[0].TokenType);
        Assert.Equal("Hello ", tokens[0].Value);
        Assert.Equal(MarkdownTokenType.Bold, tokens[1].TokenType);
        Assert.Equal("world", tokens[1].Value);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestMixedInlineElements()
    {
        var markdown = "Text with **bold** and *italic* and `code`";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(7, tokens.Count);
        Assert.Equal(MarkdownTokenType.Text, tokens[0].TokenType);
        Assert.Equal("Text ", tokens[0].Value);
        Assert.Equal(MarkdownTokenType.Text, tokens[1].TokenType);
        Assert.Equal("with ", tokens[1].Value);
        Assert.Equal(MarkdownTokenType.Bold, tokens[2].TokenType);
        Assert.Equal("bold", tokens[2].Value);
        Assert.Equal(MarkdownTokenType.Text, tokens[3].TokenType);
        Assert.Equal(" and ", tokens[3].Value);
        Assert.Equal(MarkdownTokenType.Italic, tokens[4].TokenType);
        Assert.Equal("italic", tokens[4].Value);
        Assert.Equal(MarkdownTokenType.Text, tokens[5].TokenType);
        Assert.Equal(" and ", tokens[5].Value);
        Assert.Equal(MarkdownTokenType.CodeInline, tokens[6].TokenType);
        Assert.Equal("code", tokens[6].Value);
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
        Assert.Contains(tokens, t => t.TokenType == MarkdownTokenType.Link && t.Value == "Google");
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
        Assert.Equal(3, tokens.Count);
        Assert.Equal("Start ", tokens[0].Value);
        Assert.Equal("bold", tokens[1].Value);
        Assert.Equal(" end", tokens[2].Value);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestLinkFollowedByText()
    {
        var markdown = "[link](url) text";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(2, tokens.Count);
        Assert.Equal(MarkdownTokenType.Link, tokens[0].TokenType);
        Assert.Equal("link", tokens[0].Value);
        Assert.Equal(MarkdownTokenType.Text, tokens[1].TokenType);
        Assert.Equal(" text", tokens[1].Value);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestMultipleEmojisSeparated()
    {
        var markdown = ":smile: and :wink:";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(3, tokens.Count);
        Assert.Equal(MarkdownTokenType.Emoji, tokens[0].TokenType);
        Assert.Equal("smile", tokens[0].Value);
        Assert.Equal(MarkdownTokenType.Text, tokens[1].TokenType);
        Assert.Equal(" and ", tokens[1].Value);
        Assert.Equal(MarkdownTokenType.Emoji, tokens[2].TokenType);
        Assert.Equal("wink", tokens[2].Value);
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
}
