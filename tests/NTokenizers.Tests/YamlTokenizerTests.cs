using NTokenizers.Yaml;
using System.Text;

namespace Yaml;

public class YamlTokenizerTests
{
    [Fact]
    public void TestDocumentStart()
    {
        var input = "---";
        var (tokens, text) = Tokenize(input);
        Assert.Equal(input, text);
        Assert.Single(tokens);
        Assert.Equal(YamlTokenType.DocumentStart, tokens[0].TokenType);
        Assert.Equal("---", tokens[0].Value);
    }

    [Fact]
    public void TestDocumentEnd()
    {
        var input = "...";
        var (tokens, text) = Tokenize(input);
        Assert.Equal(input, text);
        Assert.Single(tokens);
        Assert.Equal(YamlTokenType.DocumentEnd, tokens[0].TokenType);
        Assert.Equal("...", tokens[0].Value);
    }

    [Fact]
    public void TestSimpleKeyValue()
    {
        var input = "name: Alice";
        var (tokens, text) = Tokenize(input);
        Assert.Equal(input, text);
        Assert.Equal(3, tokens.Count);
        Assert.Equal(YamlTokenType.Key, tokens[0].TokenType);
        Assert.Equal("name", tokens[0].Value);
        Assert.Equal(YamlTokenType.Colon, tokens[1].TokenType);
        Assert.Equal(":", tokens[1].Value);
        Assert.Equal(YamlTokenType.Value, tokens[2].TokenType);
        Assert.Equal(" Alice", tokens[2].Value);
    }

    [Fact]
    public void TestKeyValueWithWhitespace()
    {
        var input = "  name  :  Alice  ";
        var (tokens, text) = Tokenize(input);
        Assert.Equal(input, text);
        
        // Check that we have whitespace, key, colon, and value tokens
        Assert.Contains(tokens, t => t.TokenType == YamlTokenType.Whitespace);
        Assert.Contains(tokens, t => t.TokenType == YamlTokenType.Key && t.Value == "name");
        Assert.Contains(tokens, t => t.TokenType == YamlTokenType.Colon);
        Assert.Contains(tokens, t => t.TokenType == YamlTokenType.Value && t.Value.Contains("Alice"));
    }

    [Fact]
    public void TestComment()
    {
        var input = "# This is a comment\n";
        var (tokens, text) = Tokenize(input);
        Assert.Equal(input, text);
        Assert.Single(tokens);
        Assert.Equal(YamlTokenType.Comment, tokens[0].TokenType);
        Assert.Equal("# This is a comment\n", tokens[0].Value);
    }

    [Fact]
    public void TestKeyValueWithComment()
    {
        var input = "name: Alice # comment\n";
        var (tokens, text) = Tokenize(input);
        Assert.Equal(input, text);
        Assert.Equal(4, tokens.Count);
        Assert.Equal(YamlTokenType.Key, tokens[0].TokenType);
        Assert.Equal("name", tokens[0].Value);
        Assert.Equal(YamlTokenType.Colon, tokens[1].TokenType);
        Assert.Equal(":", tokens[1].Value);
        Assert.Equal(YamlTokenType.Value, tokens[2].TokenType);
        Assert.Equal(" Alice ", tokens[2].Value);
        Assert.Equal(YamlTokenType.Comment, tokens[3].TokenType);
        Assert.Equal("# comment\n", tokens[3].Value);
    }

    [Fact]
    public void TestQuotedString()
    {
        var input = "\"Hello World\"";
        var (tokens, text) = Tokenize(input);
        Assert.Equal(input, text);
        Assert.Equal(3, tokens.Count);
        Assert.Equal(YamlTokenType.Quote, tokens[0].TokenType);
        Assert.Equal("\"", tokens[0].Value);
        Assert.Equal(YamlTokenType.String, tokens[1].TokenType);
        Assert.Equal("Hello World", tokens[1].Value);
        Assert.Equal(YamlTokenType.Quote, tokens[2].TokenType);
        Assert.Equal("\"", tokens[2].Value);
    }

    [Fact]
    public void TestKeyWithQuotedValue()
    {
        var input = "name: \"Alice\"";
        var (tokens, text) = Tokenize(input);
        Assert.Equal(input, text);
        Assert.Equal(6, tokens.Count);
        Assert.Equal(YamlTokenType.Key, tokens[0].TokenType);
        Assert.Equal("name", tokens[0].Value);
        Assert.Equal(YamlTokenType.Colon, tokens[1].TokenType);
        Assert.Equal(":", tokens[1].Value);
        Assert.Equal(YamlTokenType.Whitespace, tokens[2].TokenType);
        Assert.Equal(" ", tokens[2].Value);
        Assert.Equal(YamlTokenType.Quote, tokens[3].TokenType);
        Assert.Equal("\"", tokens[3].Value);
        Assert.Equal(YamlTokenType.String, tokens[4].TokenType);
        Assert.Equal("Alice", tokens[4].Value);
        Assert.Equal(YamlTokenType.Quote, tokens[5].TokenType);
        Assert.Equal("\"", tokens[5].Value);
    }

    [Fact]
    public void TestFlowSequence()
    {
        var input = "[1, 2, 3]";
        var (tokens, text) = Tokenize(input);
        Assert.Equal(input, text);
        Assert.Equal(9, tokens.Count);
        Assert.Equal(YamlTokenType.FlowSeqStart, tokens[0].TokenType);
        Assert.Equal("[", tokens[0].Value);
        Assert.Equal(YamlTokenType.Value, tokens[1].TokenType);
        Assert.Equal("1", tokens[1].Value);
        Assert.Equal(YamlTokenType.FlowEntry, tokens[2].TokenType);
        Assert.Equal(",", tokens[2].Value);
        Assert.Equal(YamlTokenType.Whitespace, tokens[3].TokenType);
        Assert.Equal(" ", tokens[3].Value);
        Assert.Equal(YamlTokenType.Value, tokens[4].TokenType);
        Assert.Equal("2", tokens[4].Value);
        Assert.Equal(YamlTokenType.FlowEntry, tokens[5].TokenType);
        Assert.Equal(",", tokens[5].Value);
        Assert.Equal(YamlTokenType.Whitespace, tokens[6].TokenType);
        Assert.Equal(" ", tokens[6].Value);
        Assert.Equal(YamlTokenType.Value, tokens[7].TokenType);
        Assert.Equal("3", tokens[7].Value);
        Assert.Equal(YamlTokenType.FlowSeqEnd, tokens[8].TokenType);
        Assert.Equal("]", tokens[8].Value);
    }

    [Fact]
    public void TestFlowMapping()
    {
        var input = "{name: Alice}";
        var (tokens, text) = Tokenize(input);
        Assert.Equal(input, text);
        Assert.Equal(5, tokens.Count);
        Assert.Equal(YamlTokenType.FlowMapStart, tokens[0].TokenType);
        Assert.Equal("{", tokens[0].Value);
        Assert.Equal(YamlTokenType.Key, tokens[1].TokenType);
        Assert.Equal("name", tokens[1].Value);
        Assert.Equal(YamlTokenType.Colon, tokens[2].TokenType);
        Assert.Equal(":", tokens[2].Value);
        Assert.Equal(YamlTokenType.Value, tokens[3].TokenType);
        Assert.Equal(" Alice", tokens[3].Value);
        Assert.Equal(YamlTokenType.FlowMapEnd, tokens[4].TokenType);
        Assert.Equal("}", tokens[4].Value);
    }

    [Fact]
    public void TestBlockSequenceEntry()
    {
        var input = "- item1\n- item2";
        var (tokens, text) = Tokenize(input);
        Assert.Equal(input, text);
        Assert.Equal(5, tokens.Count);
        Assert.Equal(YamlTokenType.BlockSeqEntry, tokens[0].TokenType);
        Assert.Equal("-", tokens[0].Value);
        Assert.Equal(YamlTokenType.Value, tokens[1].TokenType);
        Assert.Equal(" item1", tokens[1].Value);
        Assert.Equal(YamlTokenType.Whitespace, tokens[2].TokenType);
        Assert.Equal("\n", tokens[2].Value);
        Assert.Equal(YamlTokenType.BlockSeqEntry, tokens[3].TokenType);
        Assert.Equal("-", tokens[3].Value);
        Assert.Equal(YamlTokenType.Value, tokens[4].TokenType);
        Assert.Equal(" item2", tokens[4].Value);
    }

    [Fact]
    public void TestAnchor()
    {
        var input = "&anchor";
        var (tokens, text) = Tokenize(input);
        Assert.Equal(input, text);
        Assert.Single(tokens);
        Assert.Equal(YamlTokenType.Anchor, tokens[0].TokenType);
        Assert.Equal("&anchor", tokens[0].Value);
    }

    [Fact]
    public void TestAnchorWithKey()
    {
        var input = "name: &p Alice";
        var (tokens, text) = Tokenize(input);
        Assert.Equal(input, text);
        
        // Check that we have the key, colon, and anchor tokens
        Assert.Contains(tokens, t => t.TokenType == YamlTokenType.Key && t.Value == "name");
        Assert.Contains(tokens, t => t.TokenType == YamlTokenType.Colon);
        Assert.Contains(tokens, t => t.TokenType == YamlTokenType.Anchor && t.Value == "&p");
        // May also have whitespace and value tokens
    }

    [Fact]
    public void TestAlias()
    {
        var input = "*alias";
        var (tokens, text) = Tokenize(input);
        Assert.Equal(input, text);
        Assert.Single(tokens);
        Assert.Equal(YamlTokenType.Alias, tokens[0].TokenType);
        Assert.Equal("*alias", tokens[0].Value);
    }

    [Fact]
    public void TestTag()
    {
        var input = "!tag";
        var (tokens, text) = Tokenize(input);
        Assert.Equal(input, text);
        Assert.Single(tokens);
        Assert.Equal(YamlTokenType.Tag, tokens[0].TokenType);
        Assert.Equal("!tag", tokens[0].Value);
    }

    [Fact]
    public void TestDoubleTag()
    {
        var input = "!!str";
        var (tokens, text) = Tokenize(input);
        Assert.Equal(input, text);
        Assert.Single(tokens);
        Assert.Equal(YamlTokenType.Tag, tokens[0].TokenType);
        Assert.Equal("!!str", tokens[0].Value);
    }

    [Fact]
    public void TestTagWithValue()
    {
        var input = "!!str \"value\"";
        var (tokens, text) = Tokenize(input);
        Assert.Equal(input, text);
        Assert.Equal(5, tokens.Count);
        Assert.Equal(YamlTokenType.Tag, tokens[0].TokenType);
        Assert.Equal("!!str", tokens[0].Value);
        Assert.Equal(YamlTokenType.Whitespace, tokens[1].TokenType);
        Assert.Equal(" ", tokens[1].Value);
        Assert.Equal(YamlTokenType.Quote, tokens[2].TokenType);
        Assert.Equal("\"", tokens[2].Value);
        Assert.Equal(YamlTokenType.String, tokens[3].TokenType);
        Assert.Equal("value", tokens[3].Value);
        Assert.Equal(YamlTokenType.Quote, tokens[4].TokenType);
        Assert.Equal("\"", tokens[4].Value);
    }

    [Fact]
    public void TestMultilineDocument()
    {
        var yaml = @"---
name: Alice
age: 30
...";
        var input = yaml;
        var (tokens, text) = Tokenize(input);
        Assert.Equal(input, text);
        
        // Find key tokens
        var keyTokens = tokens.Where(t => t.TokenType == YamlTokenType.Key).ToList();
        Assert.Equal(2, keyTokens.Count);
        Assert.Equal("name", keyTokens[0].Value);
        Assert.Equal("age", keyTokens[1].Value);
        
        // Check document markers
        Assert.Contains(tokens, t => t.TokenType == YamlTokenType.DocumentStart && t.Value == "---");
        Assert.Contains(tokens, t => t.TokenType == YamlTokenType.DocumentEnd && t.Value == "...");
    }

    [Fact]
    public void TestNestedFlowSequence()
    {
        var input = "[[1, 2], [3, 4]]";
        var (tokens, text) = Tokenize(input);
        Assert.Equal(input, text);
        Assert.Equal(YamlTokenType.FlowSeqStart, tokens[0].TokenType);
        Assert.Equal("[", tokens[0].Value);
        Assert.Equal(YamlTokenType.FlowSeqStart, tokens[1].TokenType);
        Assert.Equal("[", tokens[1].Value);
    }

    [Fact]
    public void TestComplexYaml()
    {
        var yaml = @"person: &p
  name: ""Alice""
  age: 30
manager: *p";
        var input = yaml;
        var (tokens, text) = Tokenize(input);
        Assert.Equal(input, text);
        
        // Verify we have key tokens
        var keyTokens = tokens.Where(t => t.TokenType == YamlTokenType.Key).ToList();
        Assert.Contains(keyTokens, k => k.Value == "person");
        Assert.Contains(keyTokens, k => k.Value.Trim() == "name");
        Assert.Contains(keyTokens, k => k.Value.Trim() == "age");
        Assert.Contains(keyTokens, k => k.Value == "manager");
        
        // Verify anchor and alias
        Assert.Contains(tokens, t => t.TokenType == YamlTokenType.Anchor && t.Value == "&p");
        Assert.Contains(tokens, t => t.TokenType == YamlTokenType.Alias && t.Value == "*p");
        
        // Verify quoted string
        Assert.Contains(tokens, t => t.TokenType == YamlTokenType.String && t.Value == "Alice");
    }

    [Fact]
    public void TestStopDelimiter()
    {
        var yaml = @"name: Alice
---STOP---
name: Bob";
        var tokens = Tokenize(yaml, "---STOP---");
        
        // Should only parse until the delimiter
        var keyTokens = tokens.Where(t => t.TokenType == YamlTokenType.Key).ToList();
        Assert.Single(keyTokens);
        Assert.Equal("name", keyTokens[0].Value);
        
        // Should not contain Bob
        Assert.DoesNotContain(tokens, t => t.Value.Contains("Bob"));
    }

    [Fact]
    public async Task TestCancellationToken()
    {
        // Create a very long YAML document
        var yaml = string.Join("\n", Enumerable.Range(0, 10000).Select(i => $"key{i}: value{i}"));
        
        using var cts = new CancellationTokenSource();
        var tokens = new List<YamlToken>();
        
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(yaml));
        
        // Cancel after a short delay
        _ = Task.Run(async () =>
        {
            await Task.Delay(10);
            cts.Cancel();
        }, TestContext.Current.CancellationToken);
        
        await YamlTokenizer.Create().ParseAsync(stream, cts.Token, tokens.Add);
        
        // Should have been cancelled and not parsed all 10000 keys
        var keyCount = tokens.Count(t => t.TokenType == YamlTokenType.Key);
        Assert.True(keyCount < 10000, $"Expected less than 10000 keys due to cancellation, but got {keyCount}");
    }

    [Fact]
    public void TestEscapedQuotedString()
    {
        var input = "\"Hello \\\"World\\\"\"";
        var (tokens, text) = Tokenize(input);
        Assert.Equal(input, text);
        Assert.Equal(3, tokens.Count);
        Assert.Equal(YamlTokenType.Quote, tokens[0].TokenType);
        Assert.Equal("\"", tokens[0].Value);
        Assert.Equal(YamlTokenType.String, tokens[1].TokenType);
        Assert.Equal("Hello \\\"World\\\"", tokens[1].Value);
        Assert.Equal(YamlTokenType.Quote, tokens[2].TokenType);
        Assert.Equal("\"", tokens[2].Value);
    }

    [Fact]
    public void TestFlowMappingWithMultipleEntries()
    {
        var input = "{name: Alice, age: 30}";
        var (tokens, text) = Tokenize(input);
        Assert.Equal(input, text);
        
        var keyTokens = tokens.Where(t => t.TokenType == YamlTokenType.Key).ToList();
        Assert.Equal(2, keyTokens.Count);
        Assert.Equal("name", keyTokens[0].Value);
        Assert.Equal("age", keyTokens[1].Value);
        
        var commaTokens = tokens.Where(t => t.TokenType == YamlTokenType.FlowEntry).ToList();
        Assert.Single(commaTokens);
    }

    [Fact]
    public void TestEmptyFlowSequence()
    {
        var input = "[]";
        var (tokens, text) = Tokenize(input);
        Assert.Equal(input, text);
        Assert.Equal(2, tokens.Count);
        Assert.Equal(YamlTokenType.FlowSeqStart, tokens[0].TokenType);
        Assert.Equal("[", tokens[0].Value);
        Assert.Equal(YamlTokenType.FlowSeqEnd, tokens[1].TokenType);
        Assert.Equal("]", tokens[1].Value);
    }

    [Fact]
    public void TestEmptyFlowMapping()
    {
        var input = "{}";
        var (tokens, text) = Tokenize(input);
        Assert.Equal(input, text);
        Assert.Equal(2, tokens.Count);
        Assert.Equal(YamlTokenType.FlowMapStart, tokens[0].TokenType);
        Assert.Equal("{", tokens[0].Value);
        Assert.Equal(YamlTokenType.FlowMapEnd, tokens[1].TokenType);
        Assert.Equal("}", tokens[1].Value);
    }

    [Fact]
    public void TestMultipleDocuments()
    {
        var yaml = @"---
name: Alice
---
name: Bob
...";
        var input = yaml;
        var (tokens, text) = Tokenize(input);
        Assert.Equal(input, text);
        
        var docStarts = tokens.Where(t => t.TokenType == YamlTokenType.DocumentStart).ToList();
        Assert.Equal(2, docStarts.Count);
        
        var docEnds = tokens.Where(t => t.TokenType == YamlTokenType.DocumentEnd).ToList();
        Assert.Single(docEnds);
    }

    [Fact]
    public void TestWhitespaceOnly()
    {
        var input = "   \n\t  \n  ";
        var (tokens, text) = Tokenize(input);
        Assert.Equal(input, text);
        Assert.All(tokens, t => Assert.Equal(YamlTokenType.Whitespace, t.TokenType));
    }

    [Fact]
    public void TestToStringMethod()
    {
        var token = new YamlToken(YamlTokenType.Key, "name");
        var str = token.ToString();
        Assert.Equal("Key: 'name'", str);
    }

    private static (List<YamlToken> tokens, string text) Tokenize(string input)
    {
        var tokens = new List<YamlToken>();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        var result = YamlTokenizer.Create().Parse(stream, tokens.Add);

        return (tokens, result);
    }

    private static List<YamlToken> Tokenize(string input, string stopDelimiter)
    {
        var tokens = new List<YamlToken>();
        using var reader = new StringReader(input);
        YamlTokenizer.Create().ParseAsync(reader, stopDelimiter, token => tokens.Add(token)).GetAwaiter().GetResult();
        return tokens;
    }
}
