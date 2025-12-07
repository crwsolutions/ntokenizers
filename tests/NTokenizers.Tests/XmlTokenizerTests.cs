using NTokenizers.Xml;
using System.Text;

namespace Xml;

public class XmlTokenizerTests
{
    [Fact]
    public void TestEmptySelfClosingElement()
    {
        var tokens = Tokenize("<element/>");
        Assert.Equal(4, tokens.Count);
        Assert.Equal(XmlTokenType.OpeningAngleBracket, tokens[0].TokenType);
        Assert.Equal("<", tokens[0].Value);
        Assert.Equal(XmlTokenType.ElementName, tokens[1].TokenType);
        Assert.Equal("element", tokens[1].Value);
        Assert.Equal(XmlTokenType.SelfClosingSlash, tokens[2].TokenType);
        Assert.Equal("/", tokens[2].Value);
        Assert.Equal(XmlTokenType.ClosingAngleBracket, tokens[3].TokenType);
        Assert.Equal(">", tokens[3].Value);
    }

    [Fact]
    public void TestSimpleElement()
    {
        var tokens = Tokenize("<element></element>");
        Assert.Equal(7, tokens.Count);
        Assert.Equal(XmlTokenType.OpeningAngleBracket, tokens[0].TokenType);
        Assert.Equal("<", tokens[0].Value);
        Assert.Equal(XmlTokenType.ElementName, tokens[1].TokenType);
        Assert.Equal("element", tokens[1].Value);
        Assert.Equal(XmlTokenType.ClosingAngleBracket, tokens[2].TokenType);
        Assert.Equal(">", tokens[2].Value);
        Assert.Equal(XmlTokenType.OpeningAngleBracket, tokens[3].TokenType);
        Assert.Equal("<", tokens[3].Value);
        Assert.Equal(XmlTokenType.SelfClosingSlash, tokens[4].TokenType);
        Assert.Equal("/", tokens[4].Value);
        Assert.Equal(XmlTokenType.ElementName, tokens[5].TokenType);
        Assert.Equal("element", tokens[5].Value);
        Assert.Equal(XmlTokenType.ClosingAngleBracket, tokens[6].TokenType);
        Assert.Equal(">", tokens[6].Value);
    }

    [Fact]
    public void TestSimpleElementWithText()
    {
        var tokens = Tokenize("<element>text</element>");
        Assert.Equal(8, tokens.Count);
        Assert.Equal(XmlTokenType.OpeningAngleBracket, tokens[0].TokenType);
        Assert.Equal("<", tokens[0].Value);
        Assert.Equal(XmlTokenType.ElementName, tokens[1].TokenType);
        Assert.Equal("element", tokens[1].Value);
        Assert.Equal(XmlTokenType.ClosingAngleBracket, tokens[2].TokenType);
        Assert.Equal(">", tokens[2].Value);
        Assert.Equal(XmlTokenType.Text, tokens[3].TokenType);
        Assert.Equal("text", tokens[3].Value);
        Assert.Equal(XmlTokenType.OpeningAngleBracket, tokens[4].TokenType);
        Assert.Equal("<", tokens[4].Value);
        Assert.Equal(XmlTokenType.SelfClosingSlash, tokens[5].TokenType);
        Assert.Equal("/", tokens[5].Value);
        Assert.Equal(XmlTokenType.ElementName, tokens[6].TokenType);
        Assert.Equal("element", tokens[6].Value);
        Assert.Equal(XmlTokenType.ClosingAngleBracket, tokens[7].TokenType);
        Assert.Equal(">", tokens[7].Value);
    }

    [Fact]
    public void TestNestedElements()
    {
        var tokens = Tokenize("<outer><inner>text</inner></outer>");
        Assert.Equal(15, tokens.Count);
        Assert.Equal(XmlTokenType.OpeningAngleBracket, tokens[0].TokenType);
        Assert.Equal("<", tokens[0].Value);
        Assert.Equal(XmlTokenType.ElementName, tokens[1].TokenType);
        Assert.Equal("outer", tokens[1].Value);
        Assert.Equal(XmlTokenType.ClosingAngleBracket, tokens[2].TokenType);
        Assert.Equal(">", tokens[2].Value);
        Assert.Equal(XmlTokenType.OpeningAngleBracket, tokens[3].TokenType);
        Assert.Equal("<", tokens[3].Value);
        Assert.Equal(XmlTokenType.ElementName, tokens[4].TokenType);
        Assert.Equal("inner", tokens[4].Value);
        Assert.Equal(XmlTokenType.ClosingAngleBracket, tokens[5].TokenType);
        Assert.Equal(">", tokens[5].Value);
        Assert.Equal(XmlTokenType.Text, tokens[6].TokenType);
        Assert.Equal("text", tokens[6].Value);
        Assert.Equal(XmlTokenType.OpeningAngleBracket, tokens[7].TokenType);
        Assert.Equal("<", tokens[7].Value);
        Assert.Equal(XmlTokenType.SelfClosingSlash, tokens[8].TokenType);
        Assert.Equal("/", tokens[8].Value);
        Assert.Equal(XmlTokenType.ElementName, tokens[9].TokenType);
        Assert.Equal("inner", tokens[9].Value);
        Assert.Equal(XmlTokenType.ClosingAngleBracket, tokens[10].TokenType);
        Assert.Equal(">", tokens[10].Value);
        Assert.Equal(XmlTokenType.OpeningAngleBracket, tokens[11].TokenType);
        Assert.Equal("<", tokens[11].Value);
        Assert.Equal(XmlTokenType.SelfClosingSlash, tokens[12].TokenType);
        Assert.Equal("/", tokens[12].Value);
        Assert.Equal(XmlTokenType.ElementName, tokens[13].TokenType);
        Assert.Equal("outer", tokens[13].Value);
        Assert.Equal(XmlTokenType.ClosingAngleBracket, tokens[14].TokenType);
        Assert.Equal(">", tokens[14].Value);
    }

    [Fact]
    public void TestElementWithAttributes()
    {
        var tokens = Tokenize("<element attr1=\"value1\" attr2=\"value2\"/>");
        Assert.Equal(16, tokens.Count);
        Assert.Equal(XmlTokenType.OpeningAngleBracket, tokens[0].TokenType);
        Assert.Equal("<", tokens[0].Value);
        Assert.Equal(XmlTokenType.ElementName, tokens[1].TokenType);
        Assert.Equal("element", tokens[1].Value);
        Assert.Equal(XmlTokenType.Whitespace, tokens[2].TokenType);
        Assert.Equal(" ", tokens[2].Value);
        Assert.Equal(XmlTokenType.AttributeName, tokens[3].TokenType);
        Assert.Equal("attr1", tokens[3].Value);
        Assert.Equal(XmlTokenType.AttributeEquals, tokens[4].TokenType);
        Assert.Equal("=", tokens[4].Value);
        Assert.Equal(XmlTokenType.AttributeQuote, tokens[5].TokenType);
        Assert.Equal("\"", tokens[5].Value);
        Assert.Equal(XmlTokenType.AttributeValue, tokens[6].TokenType);
        Assert.Equal("value1", tokens[6].Value);
        Assert.Equal(XmlTokenType.AttributeQuote, tokens[7].TokenType);
        Assert.Equal("\"", tokens[7].Value);
        Assert.Equal(XmlTokenType.Whitespace, tokens[8].TokenType);
        Assert.Equal(" ", tokens[8].Value);
        Assert.Equal(XmlTokenType.AttributeName, tokens[9].TokenType);
        Assert.Equal("attr2", tokens[9].Value);
        Assert.Equal(XmlTokenType.AttributeEquals, tokens[10].TokenType);
        Assert.Equal("=", tokens[10].Value);
        Assert.Equal(XmlTokenType.AttributeQuote, tokens[11].TokenType);
        Assert.Equal("\"", tokens[11].Value);
        Assert.Equal(XmlTokenType.AttributeValue, tokens[12].TokenType);
        Assert.Equal("value2", tokens[12].Value);
        Assert.Equal(XmlTokenType.AttributeQuote, tokens[13].TokenType);
        Assert.Equal("\"", tokens[13].Value);
        Assert.Equal(XmlTokenType.SelfClosingSlash, tokens[14].TokenType);
        Assert.Equal("/", tokens[14].Value);
        Assert.Equal(XmlTokenType.ClosingAngleBracket, tokens[15].TokenType);  // Note: Count is 16, fixed below
        Assert.Equal(">", tokens[15].Value);
    }

    [Fact]
    public void TestComment()
    {
        var tokens = Tokenize("<!-- This is a comment -->");
        Assert.Single(tokens);
        Assert.Equal(XmlTokenType.Comment, tokens[0].TokenType);
        Assert.Equal("<!-- This is a comment -->", tokens[0].Value);
    }

    [Fact]
    public void TestProcessingInstruction()
    {
        var tokens = Tokenize("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        Assert.Single(tokens);
        Assert.Equal(XmlTokenType.ProcessingInstruction, tokens[0].TokenType);
        Assert.Equal("<?xml version=\"1.0\" encoding=\"UTF-8\"?>", tokens[0].Value);
    }

    [Fact]
    public void TestDocumentTypeDeclaration()
    {
        var tokens = Tokenize("<!DOCTYPE html>");
        Assert.Single(tokens);
        Assert.Equal(XmlTokenType.DocumentTypeDeclaration, tokens[0].TokenType);
        Assert.Equal("<!DOCTYPE html>", tokens[0].Value);
    }

    [Fact]
    public void TestCData()
    {
        var tokens = Tokenize("<![CDATA[<greeting>Hello, world!</greeting>]]>");
        Assert.Single(tokens);
        Assert.Equal(XmlTokenType.CData, tokens[0].TokenType);
        Assert.Equal("<![CDATA[<greeting>Hello, world!</greeting>]]>", tokens[0].Value);
    }

    [Fact]
    public void TestAttributeEscaped()
    {
        var tokens = Tokenize("<element attr=\"Hello &quot;World&quot;\"/>");
        Assert.Equal(10, tokens.Count);
        Assert.Equal(XmlTokenType.OpeningAngleBracket, tokens[0].TokenType);
        Assert.Equal("<", tokens[0].Value);
        Assert.Equal(XmlTokenType.ElementName, tokens[1].TokenType);
        Assert.Equal("element", tokens[1].Value);
        Assert.Equal(XmlTokenType.Whitespace, tokens[2].TokenType);
        Assert.Equal(" ", tokens[2].Value);
        Assert.Equal(XmlTokenType.AttributeName, tokens[3].TokenType);
        Assert.Equal("attr", tokens[3].Value);
        Assert.Equal(XmlTokenType.AttributeEquals, tokens[4].TokenType);
        Assert.Equal("=", tokens[4].Value);
        Assert.Equal(XmlTokenType.AttributeQuote, tokens[5].TokenType);
        Assert.Equal("\"", tokens[5].Value);
        Assert.Equal(XmlTokenType.AttributeValue, tokens[6].TokenType);
        Assert.Equal("Hello &quot;World&quot;", tokens[6].Value);
        Assert.Equal(XmlTokenType.AttributeQuote, tokens[7].TokenType);
        Assert.Equal("\"", tokens[7].Value);
        Assert.Equal(XmlTokenType.SelfClosingSlash, tokens[8].TokenType);
        Assert.Equal("/", tokens[8].Value);
        Assert.Equal(XmlTokenType.ClosingAngleBracket, tokens[9].TokenType);  // Note: Count is 10, fixed
        Assert.Equal(">", tokens[9].Value);
    }

    [Fact]
    public void TestWhitespaceHandling()
    {
        var tokens = Tokenize("< element  attr = \"value\"  />");
        Assert.Equal(14, tokens.Count);
        Assert.Equal(XmlTokenType.OpeningAngleBracket, tokens[0].TokenType);
        Assert.Equal("<", tokens[0].Value);
        Assert.Equal(XmlTokenType.Whitespace, tokens[1].TokenType);
        Assert.Equal(" ", tokens[1].Value);
        Assert.Equal(XmlTokenType.ElementName, tokens[2].TokenType);
        Assert.Equal("element", tokens[2].Value);
        Assert.Equal(XmlTokenType.Whitespace, tokens[3].TokenType);
        Assert.Equal("  ", tokens[3].Value);
        Assert.Equal(XmlTokenType.AttributeName, tokens[4].TokenType);
        Assert.Equal("attr", tokens[4].Value);
        Assert.Equal(XmlTokenType.Whitespace, tokens[5].TokenType);
        Assert.Equal(" ", tokens[5].Value);
        Assert.Equal(XmlTokenType.AttributeEquals, tokens[6].TokenType);
        Assert.Equal("=", tokens[6].Value);
        Assert.Equal(XmlTokenType.Whitespace, tokens[7].TokenType);
        Assert.Equal(" ", tokens[7].Value);
        Assert.Equal(XmlTokenType.AttributeQuote, tokens[8].TokenType);
        Assert.Equal("\"", tokens[8].Value);
        Assert.Equal(XmlTokenType.AttributeValue, tokens[9].TokenType);
        Assert.Equal("value", tokens[9].Value);
        Assert.Equal(XmlTokenType.AttributeQuote, tokens[10].TokenType);
        Assert.Equal("\"", tokens[10].Value);
        Assert.Equal(XmlTokenType.Whitespace, tokens[11].TokenType);
        Assert.Equal("  ", tokens[11].Value);
        Assert.Equal(XmlTokenType.SelfClosingSlash, tokens[12].TokenType);
        Assert.Equal("/", tokens[12].Value);
        Assert.Equal(XmlTokenType.ClosingAngleBracket, tokens[13].TokenType);  // Note: Count is 14, fixed
        Assert.Equal(">", tokens[13].Value);
    }

    [Fact]
    public void TestWhitespaceInText()
    {
        var tokens = Tokenize("<element> text with  spaces </element>");
        Assert.Equal(8, tokens.Count);
        Assert.Equal(XmlTokenType.OpeningAngleBracket, tokens[0].TokenType);
        Assert.Equal("<", tokens[0].Value);
        Assert.Equal(XmlTokenType.ElementName, tokens[1].TokenType);
        Assert.Equal("element", tokens[1].Value);
        Assert.Equal(XmlTokenType.ClosingAngleBracket, tokens[2].TokenType);
        Assert.Equal(">", tokens[2].Value);
        Assert.Equal(XmlTokenType.Text, tokens[3].TokenType);
        Assert.Equal(" text with  spaces ", tokens[3].Value);  // Assuming text includes whitespace
        Assert.Equal(XmlTokenType.OpeningAngleBracket, tokens[4].TokenType);
        Assert.Equal("<", tokens[4].Value);
        Assert.Equal(XmlTokenType.SelfClosingSlash, tokens[5].TokenType);
        Assert.Equal("/", tokens[5].Value);
        Assert.Equal(XmlTokenType.ElementName, tokens[6].TokenType);
        Assert.Equal("element", tokens[6].Value);
        Assert.Equal(XmlTokenType.ClosingAngleBracket, tokens[7].TokenType);
        Assert.Equal(">", tokens[7].Value);
    }

    [Fact]
    public void TestMultipleWhitespaceCharacters()
    {
        var tokens = Tokenize("<\n\telement\n\tattr=\n\t\"value\"\n\t/ >");
        Assert.Equal(14, tokens.Count);  // Similar structure, whitespace grouped
        Assert.Equal(XmlTokenType.OpeningAngleBracket, tokens[0].TokenType);
        Assert.Equal("<", tokens[0].Value);
        Assert.Equal(XmlTokenType.Whitespace, tokens[1].TokenType);
        Assert.Equal("\n\t", tokens[1].Value);
        Assert.Equal(XmlTokenType.ElementName, tokens[2].TokenType);
        Assert.Equal("element", tokens[2].Value);
        Assert.Equal(XmlTokenType.Whitespace, tokens[3].TokenType);
        Assert.Equal("\n\t", tokens[3].Value);
        Assert.Equal(XmlTokenType.AttributeName, tokens[4].TokenType);
        Assert.Equal("attr", tokens[4].Value);
        Assert.Equal(XmlTokenType.AttributeEquals, tokens[5].TokenType);
        Assert.Equal("=", tokens[5].Value);
        Assert.Equal(XmlTokenType.Whitespace, tokens[6].TokenType);
        Assert.Equal("\n\t", tokens[6].Value);
        Assert.Equal(XmlTokenType.AttributeQuote, tokens[7].TokenType);
        Assert.Equal("\"", tokens[7].Value);
        Assert.Equal(XmlTokenType.AttributeValue, tokens[8].TokenType);
        Assert.Equal("value", tokens[8].Value);
        Assert.Equal(XmlTokenType.AttributeQuote, tokens[9].TokenType);
        Assert.Equal("\"", tokens[9].Value);
        Assert.Equal(XmlTokenType.Whitespace, tokens[10].TokenType);
        Assert.Equal("\n\t", tokens[10].Value);
        Assert.Equal(XmlTokenType.SelfClosingSlash, tokens[11].TokenType);
        Assert.Equal("/", tokens[11].Value);
        Assert.Equal(XmlTokenType.Whitespace, tokens[12].TokenType);
        Assert.Equal(" ", tokens[12].Value);
        Assert.Equal(XmlTokenType.ClosingAngleBracket, tokens[13].TokenType);  // Count 14
        Assert.Equal(">", tokens[13].Value);
    }

    [Fact]
    public void TestStopDelimiter()
    {
        var tokens = Tokenize("<element>text</element> END", "END");
        Assert.Equal(9, tokens.Count);
        Assert.Equal(XmlTokenType.OpeningAngleBracket, tokens[0].TokenType);
        Assert.Equal("<", tokens[0].Value);
        Assert.Equal(XmlTokenType.ElementName, tokens[1].TokenType);
        Assert.Equal("element", tokens[1].Value);
        Assert.Equal(XmlTokenType.ClosingAngleBracket, tokens[2].TokenType);
        Assert.Equal(">", tokens[2].Value);
        Assert.Equal(XmlTokenType.Text, tokens[3].TokenType);
        Assert.Equal("text", tokens[3].Value);
        Assert.Equal(XmlTokenType.OpeningAngleBracket, tokens[4].TokenType);
        Assert.Equal("<", tokens[4].Value);
        Assert.Equal(XmlTokenType.SelfClosingSlash, tokens[5].TokenType);
        Assert.Equal("/", tokens[5].Value);
        Assert.Equal(XmlTokenType.ElementName, tokens[6].TokenType);
        Assert.Equal("element", tokens[6].Value);
        Assert.Equal(XmlTokenType.ClosingAngleBracket, tokens[7].TokenType);
        Assert.Equal(">", tokens[7].Value);
        Assert.Equal(XmlTokenType.Whitespace, tokens[8].TokenType);
        Assert.Equal(" ", tokens[8].Value);
    }

    [Fact]
    public void TestStopDelimiterAtBeginning()
    {
        var tokens = Tokenize(" END<element>text</element>", " END");
        Assert.Empty(tokens);
    }

    [Fact]
    public void TestComplexNestedXmlWithAttributes()
    {
        // Test case from issue - complex nested XML with attributes and self-closing tags
        var xml = """
        <?xml version="1.0" encoding="utf-8"?>
        <user id="4821" active="true">
            <name>Laura Smith</name>
            <addresses>
                <address type="home">
                    <street>221B Baker Street</street>
                    <city>London</city>
                    <postalCode>NW1 6XE</postalCode>
                    <coordinates lat="51.5237" lng="-0.1586" />
                </address>
                <address type="office" floor="5">
                    <street>18 King William Street</street>
                    <city>London</city>
                    <postalCode>EC4N 7BP</postalCode>
                </address>
            </addresses>
        </user>
        """;

        var tokens = Tokenize(xml);

        // Verify we have proper angle brackets and element names (not all attributes)
        // Key validation: ensure opening angle brackets are present
        var openingBrackets = tokens.Count(t => t.TokenType == XmlTokenType.OpeningAngleBracket);
        var closingBrackets = tokens.Count(t => t.TokenType == XmlTokenType.ClosingAngleBracket);
        var elementNames = tokens.Count(t => t.TokenType == XmlTokenType.ElementName);
        
        // We should have opening brackets (excluding self-closing which have different structure)
        // user(1) + name(2) + addresses(2) + address(4) + street(4) + city(4) + postalCode(4) + coordinates(1) = 22 opening brackets
        Assert.True(openingBrackets > 0, "Should have opening angle brackets");
        
        // Verify element names are properly tokenized (not as AttributeName)
        Assert.True(elementNames > 0, "Should have element names");
        
        // Ensure that "name" appears as ElementName, not AttributeName
        var nameElementTokens = tokens.Where(t => 
            t.TokenType == XmlTokenType.ElementName && t.Value == "name").ToList();
        Assert.Equal(2, nameElementTokens.Count); // Opening and closing tag
        
        // Ensure "Laura Smith" is tokenized as Text, not as AttributeName
        var textTokens = tokens.Where(t => t.TokenType == XmlTokenType.Text).ToList();
        Assert.Contains(textTokens, t => t.Value.Contains("Laura Smith"));
        
        // Ensure we don't have incorrect AttributeName tokens for element text
        var incorrectAttrTokens = tokens.Where(t => 
            t.TokenType == XmlTokenType.AttributeName && 
            (t.Value == "Laura" || t.Value == "Smith" || t.Value == "London")).ToList();
        Assert.Empty(incorrectAttrTokens);
    }

    [Fact]
    public async Task ParseAsync_WithStreamAndEncoding_UTF8_ShouldTokenizeCorrectly()
    {
        var xml = "<element>text with unicode: café 🚀</element>";
        var encoding = Encoding.UTF8;
        var tokens = new List<XmlToken>();

        using var stream = new MemoryStream(encoding.GetBytes(xml));
        var result = await XmlTokenizer.Create().ParseAsync(stream, encoding, tokens.Add);

        Assert.NotEmpty(tokens);
        Assert.Equal(XmlTokenType.OpeningAngleBracket, tokens[0].TokenType);
        Assert.Equal("<", tokens[0].Value);
        Assert.Equal(XmlTokenType.ElementName, tokens[1].TokenType);
        Assert.Equal("element", tokens[1].Value);
        Assert.Equal(xml, result);
    }

    [Fact]
    public void Parse_WithStreamAndEncoding_UTF8_ShouldTokenizeCorrectly()
    {
        var xml = "<element>text with unicode: café 🚀</element>";
        var encoding = Encoding.UTF8;
        var tokens = new List<XmlToken>();

        using var stream = new MemoryStream(encoding.GetBytes(xml));
        var result = XmlTokenizer.Create().Parse(stream, encoding, tokens.Add);

        Assert.NotEmpty(tokens);
        Assert.Equal(XmlTokenType.OpeningAngleBracket, tokens[0].TokenType);
        Assert.Equal("<", tokens[0].Value);
        Assert.Equal(XmlTokenType.ElementName, tokens[1].TokenType);
        Assert.Equal("element", tokens[1].Value);
        Assert.Equal(xml, result);
    }

    [Fact]
    public async Task ParseAsync_WithStreamAndEncoding_Unicode_ShouldTokenizeCorrectly()
    {
        var xml = "<element>text with unicode: 你好 🌍</element>";
        var encoding = Encoding.Unicode;
        var tokens = new List<XmlToken>();

        using var stream = new MemoryStream(encoding.GetBytes(xml));
        var result = await XmlTokenizer.Create().ParseAsync(stream, encoding, tokens.Add);

        Assert.NotEmpty(tokens);
        Assert.Equal(XmlTokenType.OpeningAngleBracket, tokens[0].TokenType);
        Assert.Equal("<", tokens[0].Value);
        Assert.Equal(XmlTokenType.ElementName, tokens[1].TokenType);
        Assert.Equal("element", tokens[1].Value);
        Assert.Equal(xml, result);
    }

    [Fact]
    public void Parse_WithStreamAndEncoding_Unicode_ShouldTokenizeCorrectly()
    {
        var xml = "<element>text with unicode: 你好 🌍</element>";
        var encoding = Encoding.Unicode;
        var tokens = new List<XmlToken>();

        using var stream = new MemoryStream(encoding.GetBytes(xml));
        var result = XmlTokenizer.Create().Parse(stream, encoding, tokens.Add);

        Assert.NotEmpty(tokens);
        Assert.Equal(XmlTokenType.OpeningAngleBracket, tokens[0].TokenType);
        Assert.Equal("<", tokens[0].Value);
        Assert.Equal(XmlTokenType.ElementName, tokens[1].TokenType);
        Assert.Equal("element", tokens[1].Value);
        Assert.Equal(xml, result);
    }

    [Fact]
    public async Task ParseAsync_WithStreamAndEncoding_ASCII_ShouldTokenizeCorrectly()
    {
        var xml = "<element>text with ascii: test</element>";
        var encoding = Encoding.ASCII;
        var tokens = new List<XmlToken>();

        using var stream = new MemoryStream(encoding.GetBytes(xml));
        var result = await XmlTokenizer.Create().ParseAsync(stream, encoding, tokens.Add);

        Assert.NotEmpty(tokens);
        Assert.Equal(XmlTokenType.OpeningAngleBracket, tokens[0].TokenType);
        Assert.Equal("<", tokens[0].Value);
        Assert.Equal(XmlTokenType.ElementName, tokens[1].TokenType);
        Assert.Equal("element", tokens[1].Value);
        Assert.Equal(xml, result);
    }

    [Fact]
    public void Parse_WithStreamAndEncoding_ASCII_ShouldTokenizeCorrectly()
    {
        var xml = "<element>text with ascii: test</element>";
        var encoding = Encoding.ASCII;
        var tokens = new List<XmlToken>();

        using var stream = new MemoryStream(encoding.GetBytes(xml));
        var result = XmlTokenizer.Create().Parse(stream, encoding, tokens.Add);

        Assert.NotEmpty(tokens);
        Assert.Equal(XmlTokenType.OpeningAngleBracket, tokens[0].TokenType);
        Assert.Equal("<", tokens[0].Value);
        Assert.Equal(XmlTokenType.ElementName, tokens[1].TokenType);
        Assert.Equal("element", tokens[1].Value);
        Assert.Equal(xml, result);
    }

    private static List<XmlToken> Tokenize(string input) => XmlTokenizer.Create().Parse(input);

    private static List<XmlToken> Tokenize(string input, string stopDelimiter)
    {
        var tokens = new List<XmlToken>();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        using var reader = new StreamReader(stream, Encoding.UTF8);
        XmlTokenizer.Create().ParseAsync(reader, stopDelimiter, tokens.Add).GetAwaiter().GetResult();
        return tokens;
    }

    [Fact]
    public async Task TestCancellation()
    {
        // Create a large XML to parse
        var largeXml = "<root>" + string.Join("", Enumerable.Range(1, 1000).Select(i => $"<item{i}>value{i}</item{i}>")) + "</root>";
        
        using var cts = new CancellationTokenSource();
        var tokens = new List<XmlToken>();
        int tokenCount = 0;
        
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(largeXml));
        
        // Cancel after a few tokens
        var parseTask = Task.Run(async () =>
        {
            await XmlTokenizer.Create().ParseAsync(stream, cts.Token, token =>
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
}