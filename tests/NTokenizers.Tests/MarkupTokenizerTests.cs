using System.Text;
using NTokenizers.Markup;
using NTokenizers.Markup.Metadata;

namespace Markup;

public class MarkupTokenizerTests
{
    private static async Task<List<MarkupToken>> Tokenize(string markup)
    {
        var tokens = new List<MarkupToken>();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(markup));
        MarkupTokenizer.Create().Parse(stream, token =>
        {
            tokens.Add(token);

            // Automatically set OnInlineToken to capture inline tokens
            if (token.Metadata is HeadingMetadata headingMeta)
            {
                headingMeta.RegisterInlineTokenHandler(tokens.Add);
            }
            else if (token.Metadata is BlockquoteMetadata blockquoteMeta)
            {
                blockquoteMeta.RegisterInlineTokenHandler(tokens.Add).Wait();
            }
            else if (token.Metadata is ListItemMetadata listMeta)
            {
                listMeta.RegisterInlineTokenHandler(tokens.Add).GetAwaiter().GetResult();
            }
            else if (token.Metadata is OrderedListItemMetadata orderedListMeta)
            {
                orderedListMeta.RegisterInlineTokenHandler(tokens.Add).Wait();
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
                tableMeta.RegisterInlineTokenHandler(tokens.Add).Wait();
            }
            else if (token.Metadata is GenericCodeBlockMetadata gMeta)
            {
                gMeta.RegisterInlineTokenHandler(token => { });
            }
        });
        return tokens;
    }

    [Fact]
    public void TestPlainText()
    {
        var tokens = Tokenize("Hello world");
        Assert.Single(tokens);
        Assert.Equal(MarkupTokenType.Text, tokens[0].TokenType);
        Assert.Equal("Hello world", tokens[0].Value);
    }

    [Fact]
    public void TestHeadingLevel1()
    {
        var tokens = Tokenize("# Heading 1");
        Assert.Equal(2, tokens.Count); // Heading token + inline text token
        Assert.Equal(MarkupTokenType.Heading, tokens[0].TokenType);
        Assert.Equal(string.Empty, tokens[0].Value); // Value is empty when OnInlineToken is used
        Assert.NotNull(tokens[0].Metadata);
        Assert.IsType<HeadingMetadata>(tokens[0].Metadata);
        Assert.Equal(1, ((HeadingMetadata)tokens[0].Metadata).Level);
        
        // Inline content
        Assert.Equal(MarkupTokenType.Text, tokens[1].TokenType);
        Assert.Equal("Heading 1", tokens[1].Value);
    }

    [Fact]
    public void TestHeadingLevel2()
    {
        var tokens = Tokenize("## Heading 2");
        Assert.Equal(2, tokens.Count); // Heading token + inline text token
        Assert.Equal(MarkupTokenType.Heading, tokens[0].TokenType);
        Assert.Equal(string.Empty, tokens[0].Value); // Value is empty when OnInlineToken is used
        Assert.Equal(2, ((HeadingMetadata)tokens[0].Metadata!).Level);
        
        // Inline content
        Assert.Equal(MarkupTokenType.Text, tokens[1].TokenType);
        Assert.Equal("Heading 2", tokens[1].Value);
    }

    [Fact]
    public void TestHeadingLevel6()
    {
        var tokens = Tokenize("###### Heading 6");
        Assert.Equal(2, tokens.Count); // Heading token + inline text token
        Assert.Equal(MarkupTokenType.Heading, tokens[0].TokenType);
        Assert.Equal(string.Empty, tokens[0].Value); // Value is empty when OnInlineToken is used
        Assert.Equal(6, ((HeadingMetadata)tokens[0].Metadata!).Level);
        
        // Inline content
        Assert.Equal(MarkupTokenType.Text, tokens[1].TokenType);
        Assert.Equal("Heading 6", tokens[1].Value);
    }

    [Fact]
    public void TestBoldText()
    {
        var tokens = Tokenize("**bold text**");
        Assert.Single(tokens);
        Assert.Equal(MarkupTokenType.Bold, tokens[0].TokenType);
        Assert.Equal("bold text", tokens[0].Value);
    }

    [Fact]
    public void TestBoldAlternativeText()
    {
        var tokens = Tokenize("__bold text__");
        Assert.Single(tokens);
        Assert.Equal(MarkupTokenType.Bold, tokens[0].TokenType);
        Assert.Equal("bold text", tokens[0].Value);
    }

    [Fact]
    public void TestItalicText()
    {
        var tokens = Tokenize("*italic text*");
        Assert.Single(tokens);
        Assert.Equal(MarkupTokenType.Italic, tokens[0].TokenType);
        Assert.Equal("italic text", tokens[0].Value);
    }

    [Fact]
    public void TestItalicAlternativeText()
    {
        var tokens = Tokenize("_italic text_");
        Assert.Single(tokens);
        Assert.Equal(MarkupTokenType.Italic, tokens[0].TokenType);
        Assert.Equal("italic text", tokens[0].Value);
    }

    [Fact]
    public void TestInlineCode()
    {
        var tokens = Tokenize("`code`");
        Assert.Single(tokens);
        Assert.Equal(MarkupTokenType.CodeInline, tokens[0].TokenType);
        Assert.Equal("code", tokens[0].Value);
    }

    [Fact]
    public void TestCodeBlock()
    {
        var tokens = Tokenize("```\ncode\n```");
        Assert.Single(tokens);
        Assert.Equal(MarkupTokenType.CodeBlock, tokens[0].TokenType);
        Assert.Equal(string.Empty, tokens[0].Value); // Code blocks have empty value
    }

    [Fact]
    public void TestCodeBlockWithLanguage()
    {
        var tokens = Tokenize("```javascript\nvar x = 1;\n```");
        // With OnInlineToken set, code blocks with language will emit syntax tokens
        // Without setting OnInlineToken, we just get the code block token with empty value
        Assert.Contains(tokens, t => t.TokenType == MarkupTokenType.CodeBlock);
        var codeBlock = tokens.First(t => t.TokenType == MarkupTokenType.CodeBlock);
        Assert.Equal(string.Empty, codeBlock.Value); // Code blocks have empty value
        Assert.NotNull(codeBlock.Metadata);
        
        // For non-specialized languages like "javascript", metadata is CodeBlockMetadata<MarkupToken>
        var codeMeta = codeBlock.Metadata as dynamic;
        Assert.NotNull(codeMeta);
        Assert.Equal("javascript", codeMeta.Language);
    }

    [Fact]
    public void TestLink()
    {
        var tokens = Tokenize("[link text](http://example.com)");
        Assert.Single(tokens);
        Assert.Equal(MarkupTokenType.Link, tokens[0].TokenType);
        Assert.Equal("link text", tokens[0].Value);
        Assert.NotNull(tokens[0].Metadata);
        Assert.IsType<LinkMetadata>(tokens[0].Metadata);
        Assert.Equal("http://example.com", ((LinkMetadata)tokens[0].Metadata).Url);
    }

    [Fact]
    public void TestLinkWithTitle()
    {
        var tokens = Tokenize("[link text](http://example.com \"title\")");
        Assert.Single(tokens);
        Assert.Equal(MarkupTokenType.Link, tokens[0].TokenType);
        Assert.Equal("link text", tokens[0].Value);
        var metadata = (LinkMetadata)tokens[0].Metadata!;
        Assert.Equal("http://example.com", metadata.Url);
        Assert.Equal("title", metadata.Title);
    }

    [Fact]
    public void TestImage()
    {
        var tokens = Tokenize("![alt text](http://example.com/image.png)");
        Assert.Single(tokens);
        Assert.Equal(MarkupTokenType.Image, tokens[0].TokenType);
        Assert.Equal("alt text", tokens[0].Value);
        Assert.NotNull(tokens[0].Metadata);
        Assert.IsType<LinkMetadata>(tokens[0].Metadata);
        Assert.Equal("http://example.com/image.png", ((LinkMetadata)tokens[0].Metadata).Url);
    }

    [Fact]
    public void TestUnorderedListWithDash()
    {
        var tokens = Tokenize("- item 1");
        Assert.Equal(2, tokens.Count);
        Assert.Equal(MarkupTokenType.UnorderedListItem, tokens[0].TokenType);
        Assert.Equal(string.Empty, tokens[0].Value); // Unordered list items have empty value
    }

    [Fact]
    public void TestUnorderedListWithPlus()
    {
        var tokens = Tokenize("+ item 1");
        Assert.Equal(2, tokens.Count);
        Assert.Equal(MarkupTokenType.UnorderedListItem, tokens[0].TokenType);
        Assert.Equal(string.Empty, tokens[0].Value); // Unordered list items have empty value
    }

    [Fact]
    public void TestUnorderedListWithAsterisk()
    {
        var tokens = Tokenize("* item 1");
        Assert.Equal(2, tokens.Count);
        Assert.Equal(MarkupTokenType.UnorderedListItem, tokens[0].TokenType);
        Assert.Equal(string.Empty, tokens[0].Value); // Unordered list items have empty value
        Assert.Equal(MarkupTokenType.Text, tokens[1].TokenType);
        Assert.Equal("item 1", tokens[1].Value);
    }

    [Fact]
    public void TestUnorderedListWithLeadingSpaces()
    {
        var tokens = Tokenize("     * item 1");
        Assert.Equal(3, tokens.Count);
        Assert.Equal(MarkupTokenType.Text, tokens[0].TokenType);
        Assert.Equal("     ", tokens[0].Value);
        Assert.Equal(MarkupTokenType.UnorderedListItem, tokens[1].TokenType);
        Assert.Equal(string.Empty, tokens[1].Value);
        Assert.Equal(MarkupTokenType.Text, tokens[2].TokenType);
        Assert.Equal("item 1", tokens[2].Value);
    }

    [Fact]
    public void TestOrderedList()
    {
        var tokens = Tokenize("1. item 1");
        Assert.Equal(2, tokens.Count); // OrderedListItem token + inline text token
        Assert.Equal(MarkupTokenType.OrderedListItem, tokens[0].TokenType);
        Assert.Equal(string.Empty, tokens[0].Value); // Value is empty when OnInlineToken is used
        Assert.NotNull(tokens[0].Metadata);
        Assert.IsType<OrderedListItemMetadata>(tokens[0].Metadata);
        Assert.Equal(1, ((OrderedListItemMetadata)tokens[0].Metadata).Number);
        
        // Inline content
        Assert.Equal(MarkupTokenType.Text, tokens[1].TokenType);
        Assert.Equal("item 1", tokens[1].Value);
    }

    [Fact]
    public void TestOrderedListMultipleDigits()
    {
        var tokens = Tokenize("42. item");
        Assert.Equal(2, tokens.Count); // OrderedListItem token + inline text token
        Assert.Equal(MarkupTokenType.OrderedListItem, tokens[0].TokenType);
        Assert.Equal(string.Empty, tokens[0].Value); // Value is empty when OnInlineToken is used
        Assert.Equal(42, ((OrderedListItemMetadata)tokens[0].Metadata!).Number);
        
        // Inline content
        Assert.Equal(MarkupTokenType.Text, tokens[1].TokenType);
        Assert.Equal("item", tokens[1].Value);
    }

    [Fact]
    public void TestBlockquote()
    {
        var tokens = Tokenize("> quoted text");
        Assert.Equal(2, tokens.Count); // Blockquote token + inline text token
        Assert.Equal(MarkupTokenType.Blockquote, tokens[0].TokenType);
        Assert.Equal(string.Empty, tokens[0].Value); // Value is empty when OnInlineToken is used
        
        // Inline content
        Assert.Equal(MarkupTokenType.Text, tokens[1].TokenType);
        Assert.Equal("quoted text", tokens[1].Value);
    }

    [Fact]
    public void TestHorizontalRuleDash()
    {
        var tokens = Tokenize("---");
        Assert.Single(tokens);
        Assert.Equal(MarkupTokenType.HorizontalRule, tokens[0].TokenType);
        Assert.Equal("---", tokens[0].Value);
    }

    [Fact]
    public void TestHorizontalRuleAsterisk()
    {
        var tokens = Tokenize("***");
        Assert.Single(tokens);
        Assert.Equal(MarkupTokenType.HorizontalRule, tokens[0].TokenType);
        Assert.Equal("***", tokens[0].Value);
    }

    [Fact]
    public void TestHorizontalRuleDashLf()
    {
        var tokens = Tokenize("---\n");
        Assert.Equal(1, tokens.Count);
        Assert.Equal(MarkupTokenType.HorizontalRule, tokens[0].TokenType);
        Assert.Equal("---", tokens[0].Value);
    }

    [Fact]
    public void TestHorizontalRuleAsteriskLf()
    {
        var tokens = Tokenize("***\n");
        Assert.Equal(1, tokens.Count);
        Assert.Equal(MarkupTokenType.HorizontalRule, tokens[0].TokenType);
        Assert.Equal("***", tokens[0].Value);
    }

    [Fact]
    public void TestHorizontalRuleDashCrLf()
    {
        var tokens = Tokenize("---\r\n");
        Assert.Equal(1, tokens.Count);
        Assert.Equal(MarkupTokenType.HorizontalRule, tokens[0].TokenType);
        Assert.Equal("---", tokens[0].Value);
    }

    [Fact]
    public void TestHorizontalRuleAsteriskCrLf()
    {
        var tokens = Tokenize("***\r\n");
        Assert.Equal(1, tokens.Count);
        Assert.Equal(MarkupTokenType.HorizontalRule, tokens[0].TokenType);
        Assert.Equal("***", tokens[0].Value);
    }

    [Fact]
    public void TestEmoji()
    {
        var tokens = Tokenize(":smile:");
        Assert.Single(tokens);
        Assert.Equal(MarkupTokenType.Emoji, tokens[0].TokenType);
        Assert.Equal("smile", tokens[0].Value);
        Assert.NotNull(tokens[0].Metadata);
        Assert.IsType<EmojiMetadata>(tokens[0].Metadata);
        Assert.Equal("smile", ((EmojiMetadata)tokens[0].Metadata).Name);
    }

    [Fact]
    public void TestSubscript()
    {
        var tokens = Tokenize("^sub^");
        Assert.Single(tokens);
        Assert.Equal(MarkupTokenType.Subscript, tokens[0].TokenType);
        Assert.Equal("sub", tokens[0].Value);
    }

    [Fact]
    public void TestSuperscript()
    {
        var tokens = Tokenize("~sup~");
        Assert.Single(tokens);
        Assert.Equal(MarkupTokenType.Superscript, tokens[0].TokenType);
        Assert.Equal("sup", tokens[0].Value);
    }

    [Fact]
    public void TestInsertedText()
    {
        var tokens = Tokenize("++inserted++");
        Assert.Single(tokens);
        Assert.Equal(MarkupTokenType.InsertedText, tokens[0].TokenType);
        Assert.Equal("inserted", tokens[0].Value);
    }

    [Fact]
    public void TestMarkedText()
    {
        var tokens = Tokenize("==marked==");
        Assert.Single(tokens);
        Assert.Equal(MarkupTokenType.MarkedText, tokens[0].TokenType);
        Assert.Equal("marked", tokens[0].Value);
    }

    [Fact]
    public void TestHtmlTag()
    {
        var tokens = Tokenize("<div>");
        Assert.Single(tokens);
        Assert.Equal(MarkupTokenType.HtmlTag, tokens[0].TokenType);
        Assert.Equal("<div>", tokens[0].Value);
    }

    [Fact]
    public void TestHtmlClosingTag()
    {
        var tokens = Tokenize("</div>");
        Assert.Single(tokens);
        Assert.Equal(MarkupTokenType.HtmlTag, tokens[0].TokenType);
        Assert.Equal("</div>", tokens[0].Value);
    }

    [Fact]
    public void TestTableCell()
    {
        var tokens = Tokenize("| cell 1 |");
        Assert.Equal(4, tokens.Count);
        Assert.Equal(MarkupTokenType.Table, tokens[0].TokenType);
        Assert.Equal(string.Empty, tokens[0].Value);
        Assert.Equal(MarkupTokenType.TableRow, tokens[1].TokenType);
        Assert.Equal(string.Empty, tokens[1].Value);
        Assert.Equal(MarkupTokenType.TableCell, tokens[2].TokenType);
        Assert.Equal(string.Empty, tokens[2].Value);
        Assert.Equal(MarkupTokenType.Text, tokens[3].TokenType);
        Assert.Equal(" cell 1 ", tokens[3].Value); 
    }

    [Fact]
    public void TestTableCellBold()
    {
        var tokens = Tokenize("|**bold**|");
        Assert.Equal(4, tokens.Count);
        Assert.Equal(MarkupTokenType.Table, tokens[0].TokenType);
        Assert.Equal(string.Empty, tokens[0].Value);
        Assert.Equal(MarkupTokenType.TableRow, tokens[1].TokenType);
        Assert.Equal(string.Empty, tokens[1].Value);
        Assert.Equal(MarkupTokenType.TableCell, tokens[2].TokenType);
        Assert.Equal(string.Empty, tokens[2].Value);
        Assert.Equal(MarkupTokenType.Bold, tokens[3].TokenType);
        Assert.Equal("bold", tokens[3].Value);
    }

    [Fact]
    public void TestTableWithTwoCells()
    {
        var tokens = Tokenize("| | |");
        Assert.Equal(6, tokens.Count);
        Assert.Equal(MarkupTokenType.Table, tokens[0].TokenType);
        Assert.Equal(string.Empty, tokens[0].Value);
        Assert.Equal(MarkupTokenType.TableRow, tokens[1].TokenType);
        Assert.Equal(string.Empty, tokens[1].Value); // Table cells have empty value
        Assert.Equal(MarkupTokenType.TableCell, tokens[2].TokenType);
        Assert.Equal(string.Empty, tokens[2].Value); // Table cells have empty value
        Assert.Equal(MarkupTokenType.Text, tokens[3].TokenType);
        Assert.Equal(" ", tokens[3].Value);
        Assert.Equal(MarkupTokenType.TableCell, tokens[4].TokenType);
        Assert.Equal(string.Empty, tokens[4].Value); // Table cells have empty value
        Assert.Equal(MarkupTokenType.Text, tokens[5].TokenType);
        Assert.Equal(" ", tokens[5].Value);
    }

    [Fact]
    public void TestTableWithTwoRows()
    {
        var tokens = Tokenize("| |\r\n| |");
        Assert.Equal(7, tokens.Count);
        Assert.Equal(MarkupTokenType.Table, tokens[0].TokenType);
        Assert.Equal(string.Empty, tokens[0].Value);
        Assert.Equal(MarkupTokenType.TableRow, tokens[1].TokenType);
        Assert.Equal(string.Empty, tokens[1].Value);
        Assert.Equal(MarkupTokenType.TableCell, tokens[2].TokenType);
        Assert.Equal(string.Empty, tokens[2].Value);
        Assert.Equal(MarkupTokenType.Text, tokens[3].TokenType);
        Assert.Equal(" ", tokens[3].Value);
        Assert.Equal(MarkupTokenType.TableRow, tokens[4].TokenType);
        Assert.Equal(string.Empty, tokens[4].Value);
        Assert.Equal(MarkupTokenType.TableCell, tokens[5].TokenType);
        Assert.Equal(string.Empty, tokens[5].Value);
        Assert.Equal(MarkupTokenType.Text, tokens[6].TokenType);
        Assert.Equal(" ", tokens[6].Value);
    }

    [Fact]
    public void TestTableAlignments()
    {
        var tokens = Tokenize("|----|----|---|");
        var tableMetadata = tokens[0].Metadata as TableMetadata;
        Assert.Equal(2, tokens.Count);
        Assert.Equal(MarkupTokenType.Table, tokens[0].TokenType);
        Assert.Equal(string.Empty, tokens[0].Value);
        Assert.Equal(MarkupTokenType.TableAlignments, tokens[1].TokenType);
        Assert.Equal(string.Empty, tokens[1].Value);
        Assert.Equal(3, tableMetadata!.Alignments!.Count);
        Assert.Equal(Justify.Left, tableMetadata!.Alignments![0]);
        Assert.Equal(Justify.Left, tableMetadata!.Alignments![1]);
        Assert.Equal(Justify.Left, tableMetadata!.Alignments![2]);
    }

    [Fact]
    public void TestTableWithTwoCellsAndAlignment()
    {
        var tokens = Tokenize("| | | |\r\n|:---|:---:|---:|\r\n");
        var tableMetadata = tokens[0].Metadata as TableMetadata;
        Assert.Equal(9, tokens.Count);
        Assert.Equal(MarkupTokenType.Table, tokens[0].TokenType);
        Assert.Equal(string.Empty, tokens[0].Value);
        Assert.Equal(MarkupTokenType.TableRow, tokens[1].TokenType);
        Assert.Equal(string.Empty, tokens[1].Value);
        Assert.Equal(MarkupTokenType.TableCell, tokens[2].TokenType);
        Assert.Equal(string.Empty, tokens[2].Value);
        Assert.Equal(MarkupTokenType.Text, tokens[3].TokenType);
        Assert.Equal(" ", tokens[3].Value);
        Assert.Equal(MarkupTokenType.TableCell, tokens[4].TokenType);
        Assert.Equal(string.Empty, tokens[4].Value);
        Assert.Equal(MarkupTokenType.Text, tokens[5].TokenType);
        Assert.Equal(" ", tokens[5].Value);
        Assert.Equal(MarkupTokenType.TableCell, tokens[6].TokenType);
        Assert.Equal(string.Empty, tokens[6].Value);
        Assert.Equal(MarkupTokenType.Text, tokens[7].TokenType);
        Assert.Equal(" ", tokens[7].Value);
        Assert.Equal(MarkupTokenType.TableAlignments, tokens[8].TokenType);
        Assert.Equal(string.Empty, tokens[8].Value);
        Assert.Equal(Justify.Left, tableMetadata!.Alignments![0]);
        Assert.Equal(Justify.Center, tableMetadata!.Alignments![1]);
        Assert.Equal(Justify.Right, tableMetadata!.Alignments![2]);
    }

    [Fact]
    public void TestTableInvalidCells()
    {
        var tokens = Tokenize("|||");
        Assert.Single(tokens);
        Assert.Equal(MarkupTokenType.Text, tokens[0].TokenType);
        Assert.Equal("|||", tokens[0].Value);
    }

    [Fact]
    public void TestCustomContainer()
    {
        var tokens = Tokenize("::: warning");
        Assert.Single(tokens);
        Assert.Equal(MarkupTokenType.CustomContainer, tokens[0].TokenType);
        Assert.Equal("warning", tokens[0].Value);
    }

    [Fact]
    public void TestMixedTextAndBold()
    {
        var tokens = Tokenize("Hello **world**");
        Assert.Equal(2, tokens.Count);
        Assert.Equal(MarkupTokenType.Text, tokens[0].TokenType);
        Assert.Equal("Hello ", tokens[0].Value);
        Assert.Equal(MarkupTokenType.Bold, tokens[1].TokenType);
        Assert.Equal("world", tokens[1].Value);
    }

    [Fact]
    public void TestMixedInlineElements()
    {
        var tokens = Tokenize("Text with **bold** and *italic* and `code`");
        Assert.Equal(6, tokens.Count);
        Assert.Equal(MarkupTokenType.Text, tokens[0].TokenType);
        Assert.Equal("Text with ", tokens[0].Value);
        Assert.Equal(MarkupTokenType.Bold, tokens[1].TokenType);
        Assert.Equal("bold", tokens[1].Value);
        Assert.Equal(MarkupTokenType.Text, tokens[2].TokenType);
        Assert.Equal(" and ", tokens[2].Value);
        Assert.Equal(MarkupTokenType.Italic, tokens[3].TokenType);
        Assert.Equal("italic", tokens[3].Value);
        Assert.Equal(MarkupTokenType.Text, tokens[4].TokenType);
        Assert.Equal(" and ", tokens[4].Value);
        Assert.Equal(MarkupTokenType.CodeInline, tokens[5].TokenType);
        Assert.Equal("code", tokens[5].Value);
    }

    [Fact]
    public void TestMultilineDocument()
    {
        var markup = @"# Title
This is text.
## Subtitle
More text.";
        
        var tokens = Tokenize(markup);
        
        // Verify we have the key tokens (headings have empty value, content via inline tokens)
        Assert.Contains(tokens, t => t.TokenType == MarkupTokenType.Heading && ((HeadingMetadata)t.Metadata!).Level == 1);
        Assert.Contains(tokens, t => t.TokenType == MarkupTokenType.Text && t.Value == "Title"); // Inline token
        Assert.Contains(tokens, t => t.TokenType == MarkupTokenType.Text && t.Value.Contains("This is text."));
        Assert.Contains(tokens, t => t.TokenType == MarkupTokenType.Heading && ((HeadingMetadata)t.Metadata!).Level == 2);
        Assert.Contains(tokens, t => t.TokenType == MarkupTokenType.Text && t.Value == "Subtitle"); // Inline token
        Assert.Contains(tokens, t => t.TokenType == MarkupTokenType.Text && t.Value.Contains("More text."));
    }

    [Fact]
    public void TestListWithMultipleItems()
    {
        var markup = @"- Item 1
- Item 2
- Item 3";
        
        var tokens = Tokenize(markup);
        Assert.Equal(6, tokens.Count);
        Assert.Equal(MarkupTokenType.UnorderedListItem, tokens[0].TokenType);
        Assert.Equal(string.Empty, tokens[0].Value); // List items have empty value
        Assert.Equal(MarkupTokenType.UnorderedListItem, tokens[2].TokenType);
        Assert.Equal(string.Empty, tokens[2].Value);
        Assert.Equal(MarkupTokenType.UnorderedListItem, tokens[4].TokenType);
        Assert.Equal(string.Empty, tokens[4].Value); // List items have empty value
    }

    [Fact]
    public void TestComplexDocument()
    {
        var markup = @"# My Document

This is **bold** and *italic*.

## Code Example

```csharp
var x = 1;
```

Visit [Google](https://google.com) for more.";

        var tokens = Tokenize(markup);
        
        // Verify we have expected tokens (headings have empty value, content via inline tokens)
        Assert.Contains(tokens, t => t.TokenType == MarkupTokenType.Heading && ((HeadingMetadata)t.Metadata!).Level == 1);
        Assert.Contains(tokens, t => t.TokenType == MarkupTokenType.Text && t.Value == "My Document"); // Inline token
        Assert.Contains(tokens, t => t.TokenType == MarkupTokenType.Bold && t.Value == "bold");
        Assert.Contains(tokens, t => t.TokenType == MarkupTokenType.Italic && t.Value == "italic");
        Assert.Contains(tokens, t => t.TokenType == MarkupTokenType.Heading && ((HeadingMetadata)t.Metadata!).Level == 2);
        Assert.Contains(tokens, t => t.TokenType == MarkupTokenType.Text && t.Value == "Code Example"); // Inline token
        Assert.Contains(tokens, t => t.TokenType == MarkupTokenType.CodeBlock && string.IsNullOrEmpty(t.Value)); // Code block has empty value
        Assert.Contains(tokens, t => t.TokenType == MarkupTokenType.Link && t.Value == "Google");
    }

    [Fact]
    public void TestEmptyInput()
    {
        var tokens = Tokenize("");
        Assert.Empty(tokens);
    }

    [Fact]
    public void TestNewlineOnly()
    {
        var tokens = Tokenize("\n");
        Assert.Single(tokens);
        Assert.Equal(MarkupTokenType.Text, tokens[0].TokenType);
        Assert.Equal("\n", tokens[0].Value);
    }

    [Fact]
    public void TestBoldWithinText()
    {
        var tokens = Tokenize("Start **bold** end");
        Assert.Equal(3, tokens.Count);
        Assert.Equal("Start ", tokens[0].Value);
        Assert.Equal("bold", tokens[1].Value);
        Assert.Equal(" end", tokens[2].Value);
    }

    [Fact]
    public void TestLinkFollowedByText()
    {
        var tokens = Tokenize("[link](url) text");
        Assert.Equal(2, tokens.Count);
        Assert.Equal(MarkupTokenType.Link, tokens[0].TokenType);
        Assert.Equal("link", tokens[0].Value);
        Assert.Equal(MarkupTokenType.Text, tokens[1].TokenType);
        Assert.Equal(" text", tokens[1].Value);
    }

    [Fact]
    public void TestMultipleEmojisSeparated()
    {
        var tokens = Tokenize(":smile: and :wink:");
        Assert.Equal(3, tokens.Count);
        Assert.Equal(MarkupTokenType.Emoji, tokens[0].TokenType);
        Assert.Equal("smile", tokens[0].Value);
        Assert.Equal(MarkupTokenType.Text, tokens[1].TokenType);
        Assert.Equal(" and ", tokens[1].Value);
        Assert.Equal(MarkupTokenType.Emoji, tokens[2].TokenType);
        Assert.Equal("wink", tokens[2].Value);
    }
}
