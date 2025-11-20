using System.Text;
using NTokenizers.Markup;
using Xunit;

namespace Markup;

public class MarkupTokenizerTests
{
    private static List<MarkupToken> Tokenize(string markup)
    {
        var tokens = new List<MarkupToken>();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(markup));
        MarkupTokenizer.Parse(stream, token =>
        {
            tokens.Add(token);
            
            // Automatically set OnInlineToken to capture inline tokens
            if (token.Metadata is HeadingMetadata headingMeta)
            {
                headingMeta.OnInlineToken = tokens.Add;
            }
            else if (token.Metadata is BlockquoteMetadata blockquoteMeta)
            {
                blockquoteMeta.OnInlineToken = tokens.Add;
            }
            else if (token.Metadata is ListItemMetadata listMeta)
            {
                listMeta.OnInlineToken = tokens.Add;
            }
            else if (token.Metadata is CodeBlockMetadata codeMeta)
            {
                codeMeta.OnInlineToken = tokens.Add;
            }
            else if (token.Metadata is TableMetadata tableMeta)
            {
                tableMeta.OnInlineToken = tokens.Add;
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
    public void TestItalicText()
    {
        var tokens = Tokenize("*italic text*");
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
        Assert.IsType<CodeBlockMetadata>(codeBlock.Metadata);
        Assert.Equal("javascript", ((CodeBlockMetadata)codeBlock.Metadata).Language);
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
        Assert.Single(tokens);
        Assert.Equal(MarkupTokenType.UnorderedListItem, tokens[0].TokenType);
        Assert.Equal(string.Empty, tokens[0].Value); // Unordered list items have empty value
    }

    [Fact]
    public void TestUnorderedListWithPlus()
    {
        var tokens = Tokenize("+ item 1");
        Assert.Single(tokens);
        Assert.Equal(MarkupTokenType.UnorderedListItem, tokens[0].TokenType);
        Assert.Equal(string.Empty, tokens[0].Value); // Unordered list items have empty value
    }

    [Fact]
    public void TestUnorderedListWithAsterisk()
    {
        var tokens = Tokenize("* item 1");
        Assert.Single(tokens);
        Assert.Equal(MarkupTokenType.UnorderedListItem, tokens[0].TokenType);
        Assert.Equal(string.Empty, tokens[0].Value); // Unordered list items have empty value
    }

    [Fact]
    public void TestOrderedList()
    {
        var tokens = Tokenize("1. item 1");
        Assert.Equal(2, tokens.Count); // OrderedListItem token + inline text token
        Assert.Equal(MarkupTokenType.OrderedListItem, tokens[0].TokenType);
        Assert.Equal(string.Empty, tokens[0].Value); // Value is empty when OnInlineToken is used
        Assert.NotNull(tokens[0].Metadata);
        Assert.IsType<ListItemMetadata>(tokens[0].Metadata);
        Assert.Equal(1, ((ListItemMetadata)tokens[0].Metadata).Number);
        
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
        Assert.Equal(42, ((ListItemMetadata)tokens[0].Metadata!).Number);
        
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
        Assert.Equal(2, tokens.Count);
        Assert.Equal(MarkupTokenType.TableCell, tokens[0].TokenType);
        Assert.Equal(string.Empty, tokens[0].Value); // Table cells have empty value
        Assert.Equal(MarkupTokenType.TableCell, tokens[1].TokenType);
        Assert.Equal(string.Empty, tokens[1].Value); // Table cells have empty value
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
        Assert.Equal(5, tokens.Count);
        Assert.Equal(MarkupTokenType.UnorderedListItem, tokens[0].TokenType);
        Assert.Equal(string.Empty, tokens[0].Value); // List items have empty value
        Assert.Equal(MarkupTokenType.Text, tokens[1].TokenType);
        Assert.Equal("\n", tokens[1].Value);
        Assert.Equal(MarkupTokenType.UnorderedListItem, tokens[2].TokenType);
        Assert.Equal(string.Empty, tokens[2].Value); // List items have empty value
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
