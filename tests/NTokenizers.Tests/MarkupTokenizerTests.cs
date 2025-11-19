using NTokenizers.Markup;
using System.Text;

namespace Markup;

public class MarkupTokenizerTests
{
    #region Basic Text Tests

    [Fact]
    public void TestPlainText()
    {
        var tokens = Tokenize("Hello World");
        Assert.Single(tokens);
        Assert.Equal(MarkupTokenType.Text, tokens[0].TokenType);
        Assert.Equal("Hello World", tokens[0].Value);
    }

    [Fact]
    public void TestTextWithNewline()
    {
        var tokens = Tokenize("Hello\nWorld");
        Assert.Equal(3, tokens.Count);
        Assert.Equal(MarkupTokenType.Text, tokens[0].TokenType);
        Assert.Equal("Hello", tokens[0].Value);
        Assert.Equal(MarkupTokenType.Text, tokens[1].TokenType);
        Assert.Equal("\n", tokens[1].Value);
        Assert.Equal(MarkupTokenType.Text, tokens[2].TokenType);
        Assert.Equal("World", tokens[2].Value);
    }

    #endregion

    #region Heading Tests

    [Fact]
    public void TestHeadingLevel1()
    {
        var tokens = Tokenize("# Heading 1");
        Assert.Equal(2, tokens.Count);
        Assert.Equal(MarkupTokenType.HeadingDelimiter, tokens[0].TokenType);
        Assert.Equal("#", tokens[0].Value);
        Assert.NotNull(tokens[0].Metadata);
        Assert.IsType<HeadingMetadata>(tokens[0].Metadata);
        Assert.Equal(1, ((HeadingMetadata)tokens[0].Metadata).Level);
        Assert.Equal(MarkupTokenType.Text, tokens[1].TokenType);
        Assert.Equal(" Heading 1", tokens[1].Value);
    }

    [Fact]
    public void TestHeadingLevel2()
    {
        var tokens = Tokenize("## Heading 2");
        Assert.Equal(2, tokens.Count);
        Assert.Equal(MarkupTokenType.HeadingDelimiter, tokens[0].TokenType);
        Assert.Equal("##", tokens[0].Value);
        Assert.Equal(2, ((HeadingMetadata)tokens[0].Metadata!).Level);
    }

    [Fact]
    public void TestHeadingLevel3()
    {
        var tokens = Tokenize("### Heading 3");
        Assert.Equal(2, tokens.Count);
        Assert.Equal(MarkupTokenType.HeadingDelimiter, tokens[0].TokenType);
        Assert.Equal("###", tokens[0].Value);
        Assert.Equal(3, ((HeadingMetadata)tokens[0].Metadata!).Level);
    }

    [Fact]
    public void TestHeadingLevel6()
    {
        var tokens = Tokenize("###### Heading 6");
        Assert.Equal(2, tokens.Count);
        Assert.Equal(MarkupTokenType.HeadingDelimiter, tokens[0].TokenType);
        Assert.Equal("######", tokens[0].Value);
        Assert.Equal(6, ((HeadingMetadata)tokens[0].Metadata!).Level);
    }

    #endregion

    #region Bold and Italic Tests

    [Fact]
    public void TestBoldText()
    {
        var tokens = Tokenize("**bold**");
        Assert.Equal(3, tokens.Count);
        Assert.Equal(MarkupTokenType.BoldDelimiter, tokens[0].TokenType);
        Assert.Equal("**", tokens[0].Value);
        Assert.Equal(MarkupTokenType.Text, tokens[1].TokenType);
        Assert.Equal("bold", tokens[1].Value);
        Assert.Equal(MarkupTokenType.BoldDelimiter, tokens[2].TokenType);
        Assert.Equal("**", tokens[2].Value);
    }

    [Fact]
    public void TestBoldInText()
    {
        var tokens = Tokenize("This is **bold** text");
        Assert.Equal(5, tokens.Count);
        Assert.Equal(MarkupTokenType.Text, tokens[0].TokenType);
        Assert.Equal("This is ", tokens[0].Value);
        Assert.Equal(MarkupTokenType.BoldDelimiter, tokens[1].TokenType);
        Assert.Equal(MarkupTokenType.Text, tokens[2].TokenType);
        Assert.Equal("bold", tokens[2].Value);
        Assert.Equal(MarkupTokenType.BoldDelimiter, tokens[3].TokenType);
        Assert.Equal(MarkupTokenType.Text, tokens[4].TokenType);
        Assert.Equal(" text", tokens[4].Value);
    }

    #endregion

    #region Code Tests

    [Fact]
    public void TestInlineCode()
    {
        var tokens = Tokenize("Text `code` more");
        Assert.Equal(3, tokens.Count);
        Assert.Equal(MarkupTokenType.Text, tokens[0].TokenType);
        Assert.Equal("Text ", tokens[0].Value);
        Assert.Equal(MarkupTokenType.CodeInline, tokens[1].TokenType);
        Assert.Equal("code", tokens[1].Value);
        Assert.Equal(MarkupTokenType.Text, tokens[2].TokenType);
        Assert.Equal(" more", tokens[2].Value);
    }

    [Fact]
    public void TestInlineCodeInText()
    {
        var tokens = Tokenize("This is `code` inline");
        Assert.Equal(3, tokens.Count);
        Assert.Equal(MarkupTokenType.Text, tokens[0].TokenType);
        Assert.Equal("This is ", tokens[0].Value);
        Assert.Equal(MarkupTokenType.CodeInline, tokens[1].TokenType);
        Assert.Equal("code", tokens[1].Value);
        Assert.Equal(MarkupTokenType.Text, tokens[2].TokenType);
        Assert.Equal(" inline", tokens[2].Value);
    }

    [Fact]
    public void TestCodeFence()
    {
        var tokens = Tokenize("```\ncode block\n```");
        Assert.Equal(3, tokens.Count);
        Assert.Equal(MarkupTokenType.CodeBlockFenceStart, tokens[0].TokenType);
        Assert.Equal("```", tokens[0].Value);
        Assert.Equal(MarkupTokenType.CodeBlockContent, tokens[1].TokenType);
        Assert.Equal("code block\n", tokens[1].Value);
        Assert.Equal(MarkupTokenType.CodeBlockFenceEnd, tokens[2].TokenType);
        Assert.Equal("```", tokens[2].Value);
    }

    [Fact]
    public void TestCodeFenceWithLanguage()
    {
        var tokens = Tokenize("```xml\n<root/>\n```");
        // When delegated to XmlTokenizer, it produces multiple tokens
        Assert.True(tokens.Count >= 3);
        Assert.Equal(MarkupTokenType.CodeBlockFenceStart, tokens[0].TokenType);
        Assert.Equal("```xml", tokens[0].Value);
        Assert.NotNull(tokens[0].Metadata);
        Assert.IsType<CodeFenceMetadata>(tokens[0].Metadata);
        Assert.Equal("xml", ((CodeFenceMetadata)tokens[0].Metadata).Language);
        // Content will be delegated to XmlTokenizer and wrapped as CodeBlockContent
        Assert.Equal(MarkupTokenType.CodeBlockFenceEnd, tokens[^1].TokenType);
    }

    [Fact]
    public void TestCodeFenceWithJson()
    {
        var tokens = Tokenize("```json\n{\"key\": \"value\"}\n```");
        Assert.True(tokens.Count >= 3);
        Assert.Equal(MarkupTokenType.CodeBlockFenceStart, tokens[0].TokenType);
        Assert.Equal("```json", tokens[0].Value);
        Assert.Equal("json", ((CodeFenceMetadata)tokens[0].Metadata!).Language);
        Assert.Equal(MarkupTokenType.CodeBlockFenceEnd, tokens[^1].TokenType);
    }

    #endregion

    #region Link and Image Tests

    [Fact]
    public void TestSimpleLink()
    {
        var tokens = Tokenize("See [link](http://example.com) here");
        Assert.Equal(4, tokens.Count);
        Assert.Equal(MarkupTokenType.Text, tokens[0].TokenType);
        Assert.Equal("See ", tokens[0].Value);
        Assert.Equal(MarkupTokenType.LinkText, tokens[1].TokenType);
        Assert.Equal("link", tokens[1].Value);
        Assert.Equal(MarkupTokenType.LinkUrl, tokens[2].TokenType);
        Assert.Equal("http://example.com", tokens[2].Value);
        Assert.NotNull(tokens[2].Metadata);
        Assert.IsType<LinkMetadata>(tokens[2].Metadata);
        Assert.Equal("http://example.com", ((LinkMetadata)tokens[2].Metadata).Url);
    }

    [Fact]
    public void TestLinkInText()
    {
        var tokens = Tokenize("This is a [link](http://example.com) here");
        Assert.Equal(4, tokens.Count);
        Assert.Equal(MarkupTokenType.Text, tokens[0].TokenType);
        Assert.Equal("This is a ", tokens[0].Value);
        Assert.Equal(MarkupTokenType.LinkText, tokens[1].TokenType);
        Assert.Equal("link", tokens[1].Value);
        Assert.Equal(MarkupTokenType.LinkUrl, tokens[2].TokenType);
        Assert.Equal("http://example.com", tokens[2].Value);
        Assert.Equal(MarkupTokenType.Text, tokens[3].TokenType);
        Assert.Equal(" here", tokens[3].Value);
    }

    [Fact]
    public void TestLinkWithTitle()
    {
        var tokens = Tokenize("Text [link](http://example.com \"Title\") more");
        Assert.Equal(5, tokens.Count);
        Assert.Equal(MarkupTokenType.Text, tokens[0].TokenType);
        Assert.Equal("Text ", tokens[0].Value);
        Assert.Equal(MarkupTokenType.LinkText, tokens[1].TokenType);
        Assert.Equal("link", tokens[1].Value);
        Assert.Equal(MarkupTokenType.LinkUrl, tokens[2].TokenType);
        Assert.Equal("http://example.com", tokens[2].Value);
        Assert.Equal(MarkupTokenType.LinkTitle, tokens[3].TokenType);
        Assert.Equal("Title", tokens[3].Value);
    }

    [Fact]
    public void TestImage()
    {
        var tokens = Tokenize("![alt text](http://example.com/image.png)");
        Assert.Equal(2, tokens.Count);
        Assert.Equal(MarkupTokenType.ImageAlt, tokens[0].TokenType);
        Assert.Equal("alt text", tokens[0].Value);
        Assert.Equal(MarkupTokenType.ImageUrl, tokens[1].TokenType);
        Assert.Equal("http://example.com/image.png", tokens[1].Value);
    }

    [Fact]
    public void TestImageWithTitle()
    {
        var tokens = Tokenize("![alt](http://example.com/img.png \"Title\")");
        Assert.Equal(3, tokens.Count);
        Assert.Equal(MarkupTokenType.ImageAlt, tokens[0].TokenType);
        Assert.Equal("alt", tokens[0].Value);
        Assert.Equal(MarkupTokenType.ImageUrl, tokens[1].TokenType);
        Assert.Equal("http://example.com/img.png", tokens[1].Value);
        Assert.Equal(MarkupTokenType.ImageTitle, tokens[2].TokenType);
        Assert.Equal("Title", tokens[2].Value);
    }

    #endregion

    #region List Tests

    [Fact]
    public void TestUnorderedListDash()
    {
        var tokens = Tokenize("\n- item");
        Assert.Equal(3, tokens.Count);
        Assert.Equal(MarkupTokenType.Text, tokens[0].TokenType);
        Assert.Equal("\n", tokens[0].Value);
        Assert.Equal(MarkupTokenType.UnorderedListDelimiter, tokens[1].TokenType);
        Assert.Equal("-", tokens[1].Value);
        Assert.Equal(MarkupTokenType.Text, tokens[2].TokenType);
        Assert.Equal(" item", tokens[2].Value);
    }

    [Fact]
    public void TestUnorderedListPlus()
    {
        var tokens = Tokenize("\n+ item");
        Assert.Equal(3, tokens.Count);
        Assert.Equal(MarkupTokenType.UnorderedListDelimiter, tokens[1].TokenType);
        Assert.Equal("+", tokens[1].Value);
    }

    [Fact]
    public void TestUnorderedListStar()
    {
        var tokens = Tokenize("\n* item");
        Assert.Equal(3, tokens.Count);
        Assert.Equal(MarkupTokenType.UnorderedListDelimiter, tokens[1].TokenType);
        Assert.Equal("*", tokens[1].Value);
    }

    [Fact]
    public void TestOrderedList()
    {
        var tokens = Tokenize("\n1. item");
        Assert.Equal(3, tokens.Count);
        Assert.Equal(MarkupTokenType.Text, tokens[0].TokenType);
        Assert.Equal("\n", tokens[0].Value);
        Assert.Equal(MarkupTokenType.OrderedListDelimiter, tokens[1].TokenType);
        Assert.Equal("1.", tokens[1].Value);
        Assert.Equal(MarkupTokenType.Text, tokens[2].TokenType);
        Assert.Equal(" item", tokens[2].Value);
    }

    [Fact]
    public void TestMultipleListItems()
    {
        var tokens = Tokenize("\n- item 1\n- item 2\n- item 3");
        Assert.Equal(9, tokens.Count);
        Assert.Equal(MarkupTokenType.UnorderedListDelimiter, tokens[1].TokenType);
        Assert.Equal(MarkupTokenType.UnorderedListDelimiter, tokens[4].TokenType);
        Assert.Equal(MarkupTokenType.UnorderedListDelimiter, tokens[7].TokenType);
    }

    #endregion

    #region Blockquote Tests

    [Fact]
    public void TestBlockquote()
    {
        var tokens = Tokenize("\n> quote");
        Assert.Equal(3, tokens.Count);
        Assert.Equal(MarkupTokenType.Text, tokens[0].TokenType);
        Assert.Equal("\n", tokens[0].Value);
        Assert.Equal(MarkupTokenType.BlockquoteDelimiter, tokens[1].TokenType);
        Assert.Equal(">", tokens[1].Value);
        Assert.Equal(MarkupTokenType.Text, tokens[2].TokenType);
        Assert.Equal(" quote", tokens[2].Value);
    }

    #endregion

    #region Horizontal Rule Tests

    [Fact]
    public void TestHorizontalRuleDash()
    {
        var tokens = Tokenize("\n---");
        Assert.Equal(2, tokens.Count);
        Assert.Equal(MarkupTokenType.Text, tokens[0].TokenType);
        Assert.Equal("\n", tokens[0].Value);
        Assert.Equal(MarkupTokenType.HorizontalRule, tokens[1].TokenType);
        Assert.Equal("---", tokens[1].Value);
    }

    [Fact]
    public void TestHorizontalRuleStar()
    {
        var tokens = Tokenize("\n***");
        Assert.Equal(2, tokens.Count);
        Assert.Equal(MarkupTokenType.HorizontalRule, tokens[1].TokenType);
        Assert.Equal("***", tokens[1].Value);
    }

    #endregion

    #region Emoji Tests

    [Fact]
    public void TestEmoji()
    {
        var tokens = Tokenize("Start :wink: end");
        Assert.Equal(3, tokens.Count);
        Assert.Equal(MarkupTokenType.Text, tokens[0].TokenType);
        Assert.Equal("Start ", tokens[0].Value);
        Assert.Equal(MarkupTokenType.Emoji, tokens[1].TokenType);
        Assert.Equal(":wink:", tokens[1].Value);
        Assert.NotNull(tokens[1].Metadata);
        Assert.IsType<EmojiMetadata>(tokens[1].Metadata);
        Assert.Equal("wink", ((EmojiMetadata)tokens[1].Metadata).Name);
    }

    [Fact]
    public void TestEmojiInText()
    {
        var tokens = Tokenize("Hello :smile: there");
        Assert.Equal(3, tokens.Count);
        Assert.Equal(MarkupTokenType.Text, tokens[0].TokenType);
        Assert.Equal("Hello ", tokens[0].Value);
        Assert.Equal(MarkupTokenType.Emoji, tokens[1].TokenType);
        Assert.Equal(":smile:", tokens[1].Value);
        Assert.Equal("smile", ((EmojiMetadata)tokens[1].Metadata!).Name);
        Assert.Equal(MarkupTokenType.Text, tokens[2].TokenType);
        Assert.Equal(" there", tokens[2].Value);
    }

    #endregion

    #region HTML Tag Tests

    [Fact]
    public void TestHtmlTag()
    {
        var tokens = Tokenize("Text <div> more");
        Assert.Equal(3, tokens.Count);
        Assert.Equal(MarkupTokenType.Text, tokens[0].TokenType);
        Assert.Equal("Text ", tokens[0].Value);
        Assert.Equal(MarkupTokenType.HtmlTag, tokens[1].TokenType);
        Assert.Equal("<div>", tokens[1].Value);
    }

    [Fact]
    public void TestHtmlClosingTag()
    {
        var tokens = Tokenize("Text </div> more");
        Assert.Equal(3, tokens.Count);
        Assert.Equal(MarkupTokenType.Text, tokens[0].TokenType);
        Assert.Equal(MarkupTokenType.HtmlTag, tokens[1].TokenType);
        Assert.Equal("</div>", tokens[1].Value);
    }

    [Fact]
    public void TestHtmlTagWithContent()
    {
        var tokens = Tokenize("Text <div>content</div> more");
        Assert.Equal(5, tokens.Count);
        Assert.Equal(MarkupTokenType.Text, tokens[0].TokenType);
        Assert.Equal("Text ", tokens[0].Value);
        Assert.Equal(MarkupTokenType.HtmlTag, tokens[1].TokenType);
        Assert.Equal("<div>", tokens[1].Value);
        Assert.Equal(MarkupTokenType.Text, tokens[2].TokenType);
        Assert.Equal("content", tokens[2].Value);
        Assert.Equal(MarkupTokenType.HtmlTag, tokens[3].TokenType);
        Assert.Equal("</div>", tokens[3].Value);
    }

    #endregion

    #region Subscript and Superscript Tests

    [Fact]
    public void TestSubscript()
    {
        var tokens = Tokenize("Text ^super^ more");
        Assert.Equal(3, tokens.Count);
        Assert.Equal(MarkupTokenType.Text, tokens[0].TokenType);
        Assert.Equal(MarkupTokenType.Subscript, tokens[1].TokenType);
        Assert.Equal("super", tokens[1].Value);
    }

    [Fact]
    public void TestSuperscript()
    {
        var tokens = Tokenize("Text ~sub~ more");
        Assert.Equal(3, tokens.Count);
        Assert.Equal(MarkupTokenType.Text, tokens[0].TokenType);
        Assert.Equal(MarkupTokenType.Superscript, tokens[1].TokenType);
        Assert.Equal("sub", tokens[1].Value);
    }

    #endregion

    #region Inserted and Marked Text Tests

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

    #endregion

    #region Custom Container Tests

    [Fact]
    public void TestCustomContainer()
    {
        var tokens = Tokenize("\n::: warning");
        Assert.Equal(2, tokens.Count);
        Assert.Equal(MarkupTokenType.Text, tokens[0].TokenType);
        Assert.Equal("\n", tokens[0].Value);
        Assert.Equal(MarkupTokenType.CustomContainer, tokens[1].TokenType);
        Assert.Equal("::: warning", tokens[1].Value);
    }

    [Fact]
    public void TestCustomContainerWithNewline()
    {
        var tokens = Tokenize("\n::: warning\n");
        Assert.Equal(2, tokens.Count); // Newline at start, then container (which includes the ending newline in its state transition)
        Assert.Equal(MarkupTokenType.Text, tokens[0].TokenType);
        Assert.Equal("\n", tokens[0].Value);
        Assert.Equal(MarkupTokenType.CustomContainer, tokens[1].TokenType);
        Assert.Equal("::: warning", tokens[1].Value);
    }

    #endregion

    #region Table Tests

    [Fact]
    public void TestTableDelimiter()
    {
        var tokens = Tokenize("\n| col1 | col2 |");
        // Table delimiter transitions to InTable state, but the content is treated as text
        Assert.True(tokens.Count >= 2);
        Assert.Equal(MarkupTokenType.Text, tokens[0].TokenType);
        Assert.Equal("\n", tokens[0].Value);
        Assert.Equal(MarkupTokenType.TableDelimiter, tokens[1].TokenType);
        Assert.Equal("|", tokens[1].Value);
        // Rest is text content
    }

    #endregion

    #region Complex Document Tests

    [Fact]
    public void TestComplexDocument()
    {
        var markdown = @"# Title

This is a paragraph with **bold** and text `code` more.

## Section

- Item 1
- Item 2

Text [link](http://example.com) here";

        var tokens = Tokenize(markdown);
        
        // Verify we have tokens
        Assert.True(tokens.Count > 0);
        
        // Check for heading
        Assert.Contains(tokens, t => t.TokenType == MarkupTokenType.HeadingDelimiter);
        
        // Check for bold
        Assert.Contains(tokens, t => t.TokenType == MarkupTokenType.BoldDelimiter);
        
        // Check for inline code
        Assert.Contains(tokens, t => t.TokenType == MarkupTokenType.CodeInline);
        
        // Check for list items
        Assert.Contains(tokens, t => t.TokenType == MarkupTokenType.UnorderedListDelimiter);
        
        // Check for link
        Assert.Contains(tokens, t => t.TokenType == MarkupTokenType.LinkText);
        Assert.Contains(tokens, t => t.TokenType == MarkupTokenType.LinkUrl);
    }

    [Fact]
    public void TestMixedInlineFormatting()
    {
        var tokens = Tokenize("Text with **bold** and text `code` and [link](url)");
        
        Assert.True(tokens.Count >= 8);
        Assert.Contains(tokens, t => t.TokenType == MarkupTokenType.BoldDelimiter);
        Assert.Contains(tokens, t => t.TokenType == MarkupTokenType.CodeInline);
        Assert.Contains(tokens, t => t.TokenType == MarkupTokenType.LinkText);
        Assert.Contains(tokens, t => t.TokenType == MarkupTokenType.LinkUrl);
    }

    [Fact]
    public void TestCodeFenceWithMultipleLanguages()
    {
        var markdown = @"```csharp
var x = 5;
```

```sql
SELECT * FROM users;
```";

        var tokens = Tokenize(markdown);
        
        var fenceStarts = tokens.Where(t => t.TokenType == MarkupTokenType.CodeBlockFenceStart).ToList();
        Assert.Equal(2, fenceStarts.Count);
        
        Assert.Equal("csharp", ((CodeFenceMetadata)fenceStarts[0].Metadata!).Language);
        Assert.Equal("sql", ((CodeFenceMetadata)fenceStarts[1].Metadata!).Language);
    }

    [Fact]
    public void TestNestedMarkup()
    {
        var markdown = @"# Heading

> Text with quote

- List item with text [link](http://example.com)

Text with **bold** and `code`";

        var tokens = Tokenize(markdown);
        
        Assert.Contains(tokens, t => t.TokenType == MarkupTokenType.HeadingDelimiter);
        Assert.Contains(tokens, t => t.TokenType == MarkupTokenType.BlockquoteDelimiter);
        Assert.Contains(tokens, t => t.TokenType == MarkupTokenType.UnorderedListDelimiter);
        Assert.Contains(tokens, t => t.TokenType == MarkupTokenType.LinkText);
        Assert.Contains(tokens, t => t.TokenType == MarkupTokenType.BoldDelimiter);
        Assert.Contains(tokens, t => t.TokenType == MarkupTokenType.CodeInline);
    }

    [Fact]
    public void TestMultipleHeadingLevels()
    {
        var markdown = @"# H1
## H2
### H3
#### H4
##### H5
###### H6";

        var tokens = Tokenize(markdown);
        
        var headings = tokens.Where(t => t.TokenType == MarkupTokenType.HeadingDelimiter).ToList();
        Assert.Equal(6, headings.Count);
        
        for (int i = 0; i < 6; i++)
        {
            Assert.Equal(i + 1, ((HeadingMetadata)headings[i].Metadata!).Level);
        }
    }

    [Fact]
    public void TestListWithMultipleItems()
    {
        var markdown = @"
- First item
- Second item
- Third item

1. First ordered
2. Second ordered
3. Third ordered";

        var tokens = Tokenize(markdown);
        
        var unordered = tokens.Where(t => t.TokenType == MarkupTokenType.UnorderedListDelimiter).ToList();
        var ordered = tokens.Where(t => t.TokenType == MarkupTokenType.OrderedListDelimiter).ToList();
        
        Assert.Equal(3, unordered.Count);
        Assert.Equal(3, ordered.Count);
    }

    [Fact]
    public void TestAllTokenTypes()
    {
        // This test ensures all major token types can be parsed
        var markdown = @"# Heading
**bold**
Text `code` more
```
fence
```
Text [link](url) more
Text ![image](url) more
Text :emoji: more
Text <tag> more
Text ^sub^ more
Text ~super~ more
Text ++inserted++ more
Text ==marked== more
::: container
";

        var tokens = Tokenize(markdown);
        
        var tokenTypes = tokens.Select(t => t.TokenType).Distinct().ToList();
        
        // Verify we have a good variety of token types
        Assert.Contains(MarkupTokenType.HeadingDelimiter, tokenTypes);
        Assert.Contains(MarkupTokenType.BoldDelimiter, tokenTypes);
        Assert.Contains(MarkupTokenType.CodeInline, tokenTypes);
        Assert.Contains(MarkupTokenType.CodeBlockFenceStart, tokenTypes);
        Assert.Contains(MarkupTokenType.LinkText, tokenTypes);
        Assert.Contains(MarkupTokenType.ImageAlt, tokenTypes);
        Assert.Contains(MarkupTokenType.Emoji, tokenTypes);
        Assert.Contains(MarkupTokenType.HtmlTag, tokenTypes);
        Assert.Contains(MarkupTokenType.Subscript, tokenTypes);
        Assert.Contains(MarkupTokenType.Superscript, tokenTypes);
        Assert.Contains(MarkupTokenType.InsertedText, tokenTypes);
        Assert.Contains(MarkupTokenType.MarkedText, tokenTypes);
        Assert.Contains(MarkupTokenType.CustomContainer, tokenTypes);
    }

    #endregion

    #region Helper Methods

    private static List<MarkupToken> Tokenize(string input)
    {
        var tokens = new List<MarkupToken>();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        MarkupTokenizer.Parse(stream, token => tokens.Add(token));
        return tokens;
    }

    #endregion
}
