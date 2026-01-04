using NTokenizers.Html;
using System.Text;

namespace Html;

public class HtmlTokenizerTests
{
    [Fact]
    public void TestEmptySelfClosingElement()
    {
        var tokens = Tokenize("<br/>");
        Assert.Equal(4, tokens.Count);
        Assert.Equal(HtmlTokenType.OpeningAngleBracket, tokens[0].TokenType);
        Assert.Equal("<", tokens[0].Value);
        Assert.Equal(HtmlTokenType.ElementName, tokens[1].TokenType);
        Assert.Equal("br", tokens[1].Value);
        Assert.Equal(HtmlTokenType.SelfClosingSlash, tokens[2].TokenType);
        Assert.Equal("/", tokens[2].Value);
        Assert.Equal(HtmlTokenType.ClosingAngleBracket, tokens[3].TokenType);
        Assert.Equal(">", tokens[3].Value);
    }

    [Fact]
    public void TestSimpleElement()
    {
        var tokens = Tokenize("<div></div>");
        Assert.Equal(7, tokens.Count);
        Assert.Equal(HtmlTokenType.OpeningAngleBracket, tokens[0].TokenType);
        Assert.Equal("<", tokens[0].Value);
        Assert.Equal(HtmlTokenType.ElementName, tokens[1].TokenType);
        Assert.Equal("div", tokens[1].Value);
        Assert.Equal(HtmlTokenType.ClosingAngleBracket, tokens[2].TokenType);
        Assert.Equal(">", tokens[2].Value);
        Assert.Equal(HtmlTokenType.OpeningAngleBracket, tokens[3].TokenType);
        Assert.Equal("<", tokens[3].Value);
        Assert.Equal(HtmlTokenType.SelfClosingSlash, tokens[4].TokenType);
        Assert.Equal("/", tokens[4].Value);
        Assert.Equal(HtmlTokenType.ElementName, tokens[5].TokenType);
        Assert.Equal("div", tokens[5].Value);
        Assert.Equal(HtmlTokenType.ClosingAngleBracket, tokens[6].TokenType);
        Assert.Equal(">", tokens[6].Value);
    }

    [Fact]
    public void TestSimpleElementWithText()
    {
        var tokens = Tokenize("<p>Hello World</p>");
        Assert.Equal(8, tokens.Count);
        Assert.Equal(HtmlTokenType.OpeningAngleBracket, tokens[0].TokenType);
        Assert.Equal("<", tokens[0].Value);
        Assert.Equal(HtmlTokenType.ElementName, tokens[1].TokenType);
        Assert.Equal("p", tokens[1].Value);
        Assert.Equal(HtmlTokenType.ClosingAngleBracket, tokens[2].TokenType);
        Assert.Equal(">", tokens[2].Value);
        Assert.Equal(HtmlTokenType.Text, tokens[3].TokenType);
        Assert.Equal("Hello World", tokens[3].Value);
        Assert.Equal(HtmlTokenType.OpeningAngleBracket, tokens[4].TokenType);
        Assert.Equal("<", tokens[4].Value);
        Assert.Equal(HtmlTokenType.SelfClosingSlash, tokens[5].TokenType);
        Assert.Equal("/", tokens[5].Value);
        Assert.Equal(HtmlTokenType.ElementName, tokens[6].TokenType);
        Assert.Equal("p", tokens[6].Value);
        Assert.Equal(HtmlTokenType.ClosingAngleBracket, tokens[7].TokenType);
        Assert.Equal(">", tokens[7].Value);
    }

    [Fact]
    public void TestNestedElements()
    {
        var tokens = Tokenize("<div><span>text</span></div>");
        Assert.Equal(15, tokens.Count);
        Assert.Equal(HtmlTokenType.OpeningAngleBracket, tokens[0].TokenType);
        Assert.Equal("<", tokens[0].Value);
        Assert.Equal(HtmlTokenType.ElementName, tokens[1].TokenType);
        Assert.Equal("div", tokens[1].Value);
        Assert.Equal(HtmlTokenType.ClosingAngleBracket, tokens[2].TokenType);
        Assert.Equal(">", tokens[2].Value);
        Assert.Equal(HtmlTokenType.OpeningAngleBracket, tokens[3].TokenType);
        Assert.Equal("<", tokens[3].Value);
        Assert.Equal(HtmlTokenType.ElementName, tokens[4].TokenType);
        Assert.Equal("span", tokens[4].Value);
        Assert.Equal(HtmlTokenType.ClosingAngleBracket, tokens[5].TokenType);
        Assert.Equal(">", tokens[5].Value);
        Assert.Equal(HtmlTokenType.Text, tokens[6].TokenType);
        Assert.Equal("text", tokens[6].Value);
        Assert.Equal(HtmlTokenType.OpeningAngleBracket, tokens[7].TokenType);
        Assert.Equal("<", tokens[7].Value);
        Assert.Equal(HtmlTokenType.SelfClosingSlash, tokens[8].TokenType);
        Assert.Equal("/", tokens[8].Value);
        Assert.Equal(HtmlTokenType.ElementName, tokens[9].TokenType);
        Assert.Equal("span", tokens[9].Value);
        Assert.Equal(HtmlTokenType.ClosingAngleBracket, tokens[10].TokenType);
        Assert.Equal(">", tokens[10].Value);
        Assert.Equal(HtmlTokenType.OpeningAngleBracket, tokens[11].TokenType);
        Assert.Equal("<", tokens[11].Value);
        Assert.Equal(HtmlTokenType.SelfClosingSlash, tokens[12].TokenType);
        Assert.Equal("/", tokens[12].Value);
        Assert.Equal(HtmlTokenType.ElementName, tokens[13].TokenType);
        Assert.Equal("div", tokens[13].Value);
        Assert.Equal(HtmlTokenType.ClosingAngleBracket, tokens[14].TokenType);
        Assert.Equal(">", tokens[14].Value);
    }

    [Fact]
    public void TestElementWithAttributes()
    {
        var tokens = Tokenize("<a href=\"https://example.com\">Link</a>");
        Assert.Equal(14, tokens.Count);
        Assert.Equal(HtmlTokenType.OpeningAngleBracket, tokens[0].TokenType);
        Assert.Equal("<", tokens[0].Value);
        Assert.Equal(HtmlTokenType.ElementName, tokens[1].TokenType);
        Assert.Equal("a", tokens[1].Value);
        Assert.Equal(HtmlTokenType.Whitespace, tokens[2].TokenType);
        Assert.Equal(" ", tokens[2].Value);
        Assert.Equal(HtmlTokenType.AttributeName, tokens[3].TokenType);
        Assert.Equal("href", tokens[3].Value);
        Assert.Equal(HtmlTokenType.AttributeEquals, tokens[4].TokenType);
        Assert.Equal("=", tokens[4].Value);
        Assert.Equal(HtmlTokenType.AttributeQuote, tokens[5].TokenType);
        Assert.Equal("\"", tokens[5].Value);
        Assert.Equal(HtmlTokenType.AttributeValue, tokens[6].TokenType);
        Assert.Equal("https://example.com", tokens[6].Value);
        Assert.Equal(HtmlTokenType.AttributeQuote, tokens[7].TokenType);
        Assert.Equal("\"", tokens[7].Value);
        Assert.Equal(HtmlTokenType.ClosingAngleBracket, tokens[8].TokenType);
        Assert.Equal(">", tokens[8].Value);
        Assert.Equal(HtmlTokenType.Text, tokens[9].TokenType);
        Assert.Equal("Link", tokens[9].Value);
        Assert.Equal(HtmlTokenType.OpeningAngleBracket, tokens[10].TokenType);
        Assert.Equal("<", tokens[10].Value);
        Assert.Equal(HtmlTokenType.SelfClosingSlash, tokens[11].TokenType);
        Assert.Equal("/", tokens[11].Value);
        Assert.Equal(HtmlTokenType.ElementName, tokens[12].TokenType);
        Assert.Equal("a", tokens[12].Value);
        Assert.Equal(HtmlTokenType.ClosingAngleBracket, tokens[13].TokenType);
        Assert.Equal(">", tokens[13].Value);
    }

    [Fact]
    public void TestElementWithMultipleAttributes()
    {
        var tokens = Tokenize("<input type=\"text\" name=\"username\" />");
        Assert.Contains(tokens, t => t.TokenType == HtmlTokenType.ElementName && t.Value == "input");
        Assert.Contains(tokens, t => t.TokenType == HtmlTokenType.AttributeName && t.Value == "type");
        Assert.Contains(tokens, t => t.TokenType == HtmlTokenType.AttributeValue && t.Value == "text");
        Assert.Contains(tokens, t => t.TokenType == HtmlTokenType.AttributeName && t.Value == "name");
        Assert.Contains(tokens, t => t.TokenType == HtmlTokenType.AttributeValue && t.Value == "username");
        Assert.Contains(tokens, t => t.TokenType == HtmlTokenType.SelfClosingSlash);
    }

    [Fact]
    public void TestComment()
    {
        var tokens = Tokenize("<!-- This is a comment -->");
        Assert.Single(tokens);
        Assert.Equal(HtmlTokenType.Comment, tokens[0].TokenType);
        Assert.Equal("<!-- This is a comment -->", tokens[0].Value);
    }

    [Fact]
    public void TestDocType()
    {
        var tokens = Tokenize("<!DOCTYPE html>");
        Assert.Single(tokens);
        Assert.Equal(HtmlTokenType.DocumentTypeDeclaration, tokens[0].TokenType);
        Assert.Equal("<!DOCTYPE html>", tokens[0].Value);
    }

    [Fact]
    public void TestWhitespace()
    {
        var tokens = Tokenize("  \n  <div>test</div>");
        Assert.Equal(HtmlTokenType.Whitespace, tokens[0].TokenType);
        Assert.Equal("  \n  ", tokens[0].Value);
    }

    [Fact]
    public void TestStopDelimiter()
    {
        var html = "<div>content</div><div>more</div>";
        var tokens = new List<HtmlToken>();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(html));
        using var reader = new StreamReader(stream, Encoding.UTF8);
        HtmlTokenizer.Create().ParseAsync(reader, "</div>", tokens.Add).GetAwaiter().GetResult();
        
        // Should stop at first </div>
        Assert.Contains(tokens, t => t.Value.Contains("content"));
        Assert.DoesNotContain(tokens, t => t.Value.Contains("more"));
    }

    [Fact]
    public void TestStyleElement()
    {
        var html = "<style>body { color: red; }</style>";
        var tokens = Tokenize(html);
        
        // Should have opening style tag, CSS content, and closing style tag
        Assert.Contains(tokens, t => t.TokenType == HtmlTokenType.ElementName && t.Value == "style");
        Assert.Contains(tokens, t => t.TokenType == HtmlTokenType.Text && t.Value.Contains("color"));
    }

    [Fact]
    public void TestScriptElement()
    {
        var html = "<script>console.log('Hello');</script>";
        var tokens = Tokenize(html);
        
        // Should have opening script tag, JS content, and closing script tag
        Assert.Contains(tokens, t => t.TokenType == HtmlTokenType.ElementName && t.Value == "script");
        Assert.Contains(tokens, t => t.TokenType == HtmlTokenType.Text && t.Value.Contains("console"));
    }

    [Fact]
    public void TestComplexHtmlWithStyleAndScript()
    {
        var html = @"<!DOCTYPE html>
<html>
<head>
    <style>
        body { font-family: Arial; }
    </style>
</head>
<body>
    <h1>Title</h1>
    <script>
        alert('Hi');
    </script>
</body>
</html>";
        
        var tokens = Tokenize(html);
        
        Assert.Contains(tokens, t => t.TokenType == HtmlTokenType.DocumentTypeDeclaration);
        Assert.Contains(tokens, t => t.TokenType == HtmlTokenType.ElementName && t.Value == "html");
        Assert.Contains(tokens, t => t.TokenType == HtmlTokenType.ElementName && t.Value == "style");
        Assert.Contains(tokens, t => t.TokenType == HtmlTokenType.ElementName && t.Value == "script");
        Assert.Contains(tokens, t => t.TokenType == HtmlTokenType.ElementName && t.Value == "h1");
    }

    [Fact]
    public void TestCancellationToken()
    {
        var html = "<div>content</div><div>more</div>";
        var cts = new CancellationTokenSource();
        var tokenCount = 0;
        
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(html));
        
        // Cancel after receiving 5 tokens
        HtmlTokenizer.Create().Parse(stream, cts.Token, token =>
        {
            tokenCount++;
            if (tokenCount >= 5)
            {
                cts.Cancel();
            }
        });
        
        // Should have stopped early - not all tokens should be processed
        Assert.InRange(tokenCount, 5, 7); // Allow some tolerance for the cancellation timing
    }

    [Fact]
    public void TestAttributeWithSingleQuotes()
    {
        var tokens = Tokenize("<a href='https://example.com'>Link</a>");
        Assert.Contains(tokens, t => t.TokenType == HtmlTokenType.AttributeQuote && t.Value == "'");
        Assert.Contains(tokens, t => t.TokenType == HtmlTokenType.AttributeValue && t.Value == "https://example.com");
    }

    private static List<HtmlToken> Tokenize(string html)
    {
        var tokens = new List<HtmlToken>();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(html));
        HtmlTokenizer.Create().Parse(stream, token => tokens.Add(token));
        return tokens;
    }
}
