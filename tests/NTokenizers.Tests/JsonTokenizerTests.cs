using NTokenizers.Json;
using System.Text;

namespace Json;

public class JsonTokenizerTests
{
    [Fact]
    public void TestEmptyObject()
    {
        var tokens = Tokenize("{}");
        Assert.Equal(2, tokens.Count);
        Assert.Equal(JsonTokenType.StartObject, tokens[0].TokenType);
        Assert.Equal("{", tokens[0].Value);
        Assert.Equal(JsonTokenType.EndObject, tokens[1].TokenType);
        Assert.Equal("}", tokens[1].Value);
    }

    [Fact]
    public void TestEmptyArray()
    {
        var tokens = Tokenize("[]");
        Assert.Equal(2, tokens.Count);
        Assert.Equal(JsonTokenType.StartArray, tokens[0].TokenType);
        Assert.Equal("[", tokens[0].Value);
        Assert.Equal(JsonTokenType.EndArray, tokens[1].TokenType);
        Assert.Equal("]", tokens[1].Value);
    }

    [Fact]
    public void TestSimpleObject()
    {
        var tokens = Tokenize("{\"name\":\"John\"}");
        Assert.Equal(5, tokens.Count);
        Assert.Equal(JsonTokenType.StartObject, tokens[0].TokenType);
        Assert.Equal("{", tokens[0].Value);
        Assert.Equal(JsonTokenType.PropertyName, tokens[1].TokenType);
        Assert.Equal("\"name\"", tokens[1].Value);
        Assert.Equal(JsonTokenType.Colon, tokens[2].TokenType);
        Assert.Equal(":", tokens[2].Value);
        Assert.Equal(JsonTokenType.StringValue, tokens[3].TokenType);
        Assert.Equal("\"John\"", tokens[3].Value);
        Assert.Equal(JsonTokenType.EndObject, tokens[4].TokenType);
        Assert.Equal("}", tokens[4].Value);
    }

    [Fact]
    public void TestNestedObjects()
    {
        var tokens = Tokenize("{\"person\": {\"name\": \"John\"}}");
        Assert.Equal(11, tokens.Count);
        Assert.Equal(JsonTokenType.StartObject, tokens[0].TokenType);
        Assert.Equal("{", tokens[0].Value);
        Assert.Equal(JsonTokenType.PropertyName, tokens[1].TokenType);
        Assert.Equal("\"person\"", tokens[1].Value);
        Assert.Equal(JsonTokenType.Colon, tokens[2].TokenType);
        Assert.Equal(":", tokens[2].Value);
        Assert.Equal(JsonTokenType.Whitespace, tokens[3].TokenType);
        Assert.Equal(" ", tokens[3].Value);
        Assert.Equal(JsonTokenType.StartObject, tokens[4].TokenType);
        Assert.Equal("{", tokens[4].Value);
        Assert.Equal(JsonTokenType.PropertyName, tokens[5].TokenType);
        Assert.Equal("\"name\"", tokens[5].Value);
        Assert.Equal(JsonTokenType.Colon, tokens[6].TokenType);
        Assert.Equal(":", tokens[6].Value);
        Assert.Equal(JsonTokenType.Whitespace, tokens[7].TokenType);
        Assert.Equal(" ", tokens[7].Value);
        Assert.Equal(JsonTokenType.StringValue, tokens[8].TokenType);
        Assert.Equal("\"John\"", tokens[8].Value);
        Assert.Equal(JsonTokenType.EndObject, tokens[9].TokenType);
        Assert.Equal("}", tokens[9].Value);
        Assert.Equal(JsonTokenType.EndObject, tokens[10].TokenType);
        Assert.Equal("}", tokens[10].Value);
    }

    [Fact]
    public void TestNestedArrays()
    {
        var tokens = Tokenize("[[1, 2], [3, 4]]");
        Assert.Equal(16, tokens.Count);
        Assert.Equal(JsonTokenType.StartArray, tokens[0].TokenType);
        Assert.Equal("[", tokens[0].Value);
        Assert.Equal(JsonTokenType.StartArray, tokens[1].TokenType);
        Assert.Equal("[", tokens[1].Value);
        Assert.Equal(JsonTokenType.Number, tokens[2].TokenType);
        Assert.Equal("1", tokens[2].Value);
        Assert.Equal(JsonTokenType.Comma, tokens[3].TokenType);
        Assert.Equal(",", tokens[3].Value);
        Assert.Equal(JsonTokenType.Whitespace, tokens[4].TokenType);
        Assert.Equal(" ", tokens[4].Value);
        Assert.Equal(JsonTokenType.Number, tokens[5].TokenType);
        Assert.Equal("2", tokens[5].Value);
        Assert.Equal(JsonTokenType.EndArray, tokens[6].TokenType);
        Assert.Equal("]", tokens[6].Value);
        Assert.Equal(JsonTokenType.Comma, tokens[7].TokenType);
        Assert.Equal(",", tokens[7].Value);
        Assert.Equal(JsonTokenType.Whitespace, tokens[8].TokenType);
        Assert.Equal(" ", tokens[8].Value);
        Assert.Equal(JsonTokenType.StartArray, tokens[9].TokenType);
        Assert.Equal("[", tokens[9].Value);
        Assert.Equal(JsonTokenType.Number, tokens[10].TokenType);
        Assert.Equal("3", tokens[10].Value);
        Assert.Equal(JsonTokenType.Comma, tokens[11].TokenType);
        Assert.Equal(",", tokens[11].Value);
        Assert.Equal(JsonTokenType.Whitespace, tokens[12].TokenType);
        Assert.Equal(" ", tokens[12].Value);
        Assert.Equal(JsonTokenType.Number, tokens[13].TokenType);
        Assert.Equal("4", tokens[13].Value);
        Assert.Equal(JsonTokenType.EndArray, tokens[14].TokenType);
        Assert.Equal("]", tokens[14].Value);
        Assert.Equal(JsonTokenType.EndArray, tokens[15].TokenType);
        Assert.Equal("]", tokens[15].Value);
    }

    [Fact]
    public void TestMixedTypes()
    {
        var tokens = Tokenize("{\"name\": \"John\", \"age\": 30, \"active\": true}");
        Assert.Equal(18, tokens.Count);
        Assert.Equal(JsonTokenType.StartObject, tokens[0].TokenType);
        Assert.Equal("{", tokens[0].Value);
        Assert.Equal(JsonTokenType.PropertyName, tokens[1].TokenType);
        Assert.Equal("\"name\"", tokens[1].Value);
        Assert.Equal(JsonTokenType.Colon, tokens[2].TokenType);
        Assert.Equal(":", tokens[2].Value);
        Assert.Equal(JsonTokenType.Whitespace, tokens[3].TokenType);
        Assert.Equal(" ", tokens[3].Value);
        Assert.Equal(JsonTokenType.StringValue, tokens[4].TokenType);
        Assert.Equal("\"John\"", tokens[4].Value);
        Assert.Equal(JsonTokenType.Comma, tokens[5].TokenType);
        Assert.Equal(",", tokens[5].Value);
        Assert.Equal(JsonTokenType.Whitespace, tokens[6].TokenType);
        Assert.Equal(" ", tokens[6].Value);
        Assert.Equal(JsonTokenType.PropertyName, tokens[7].TokenType);
        Assert.Equal("\"age\"", tokens[7].Value);
        Assert.Equal(JsonTokenType.Colon, tokens[8].TokenType);
        Assert.Equal(":", tokens[8].Value);
        Assert.Equal(JsonTokenType.Whitespace, tokens[9].TokenType);
        Assert.Equal(" ", tokens[9].Value);
        Assert.Equal(JsonTokenType.Number, tokens[10].TokenType);
        Assert.Equal("30", tokens[10].Value);
        Assert.Equal(JsonTokenType.Comma, tokens[11].TokenType);
        Assert.Equal(",", tokens[11].Value);
        Assert.Equal(JsonTokenType.Whitespace, tokens[12].TokenType);
        Assert.Equal(" ", tokens[12].Value);
        Assert.Equal(JsonTokenType.PropertyName, tokens[13].TokenType);
        Assert.Equal("\"active\"", tokens[13].Value);
        Assert.Equal(JsonTokenType.Colon, tokens[14].TokenType);
        Assert.Equal(":", tokens[14].Value);
        Assert.Equal(JsonTokenType.Whitespace, tokens[15].TokenType);
        Assert.Equal(" ", tokens[15].Value);
        Assert.Equal(JsonTokenType.True, tokens[16].TokenType);
        Assert.Equal("true", tokens[16].Value);
        Assert.Equal(JsonTokenType.EndObject, tokens[17].TokenType);
        Assert.Equal("}", tokens[17].Value);
    }

    [Fact]
    public void TestStringsEscaped()
    {
        var tokens = Tokenize("{\"message\": \"Hello \\\"World\\\"\"}");
        Assert.Equal(6, tokens.Count);
        Assert.Equal(JsonTokenType.StartObject, tokens[0].TokenType);
        Assert.Equal("{", tokens[0].Value);
        Assert.Equal(JsonTokenType.PropertyName, tokens[1].TokenType);
        Assert.Equal("\"message\"", tokens[1].Value);
        Assert.Equal(JsonTokenType.Colon, tokens[2].TokenType);
        Assert.Equal(":", tokens[2].Value);
        Assert.Equal(JsonTokenType.Whitespace, tokens[3].TokenType);
        Assert.Equal(" ", tokens[3].Value);
        Assert.Equal(JsonTokenType.StringValue, tokens[4].TokenType);
        Assert.Equal("\"Hello \\\"World\\\"\"", tokens[4].Value);
        Assert.Equal(JsonTokenType.EndObject, tokens[5].TokenType);
        Assert.Equal("}", tokens[5].Value);
    }

    [Fact]
    public void TestNumbers()
    {
        var tokens = Tokenize("{\"value\": 123.45}");
        Assert.Equal(6, tokens.Count);
        Assert.Equal(JsonTokenType.StartObject, tokens[0].TokenType);
        Assert.Equal("{", tokens[0].Value);
        Assert.Equal(JsonTokenType.PropertyName, tokens[1].TokenType);
        Assert.Equal("\"value\"", tokens[1].Value);
        Assert.Equal(JsonTokenType.Colon, tokens[2].TokenType);
        Assert.Equal(":", tokens[2].Value);
        Assert.Equal(JsonTokenType.Whitespace, tokens[3].TokenType);
        Assert.Equal(" ", tokens[3].Value);
        Assert.Equal(JsonTokenType.Number, tokens[4].TokenType);
        Assert.Equal("123.45", tokens[4].Value);
        Assert.Equal(JsonTokenType.EndObject, tokens[5].TokenType);
        Assert.Equal("}", tokens[5].Value);
    }

    [Fact]
    public void TestWhitespaceHandling()
    {
        var tokens = Tokenize("{  \"name\"  :  \"John\"  }");
        Assert.Equal(9, tokens.Count);
        Assert.Equal(JsonTokenType.StartObject, tokens[0].TokenType);
        Assert.Equal("{", tokens[0].Value);
        Assert.Equal(JsonTokenType.Whitespace, tokens[1].TokenType);
        Assert.Equal("  ", tokens[1].Value);
        Assert.Equal(JsonTokenType.PropertyName, tokens[2].TokenType);
        Assert.Equal("\"name\"", tokens[2].Value);
        Assert.Equal(JsonTokenType.Whitespace, tokens[3].TokenType);
        Assert.Equal("  ", tokens[3].Value);
        Assert.Equal(JsonTokenType.Colon, tokens[4].TokenType);
        Assert.Equal(":", tokens[4].Value);
        Assert.Equal(JsonTokenType.Whitespace, tokens[5].TokenType);
        Assert.Equal("  ", tokens[5].Value);
        Assert.Equal(JsonTokenType.StringValue, tokens[6].TokenType);
        Assert.Equal("\"John\"", tokens[6].Value);
        Assert.Equal(JsonTokenType.Whitespace, tokens[7].TokenType);
        Assert.Equal("  ", tokens[7].Value);
        Assert.Equal(JsonTokenType.EndObject, tokens[8].TokenType);
        Assert.Equal("}", tokens[8].Value);
    }

    [Fact]
    public void TestWhitespaceInArray()
    {
        var tokens = Tokenize("[ 1 , 2 , 3 ]");
        Assert.Equal(13, tokens.Count);
        Assert.Equal(JsonTokenType.StartArray, tokens[0].TokenType);
        Assert.Equal("[", tokens[0].Value);
        Assert.Equal(JsonTokenType.Whitespace, tokens[1].TokenType);
        Assert.Equal(" ", tokens[1].Value);
        Assert.Equal(JsonTokenType.Number, tokens[2].TokenType);
        Assert.Equal("1", tokens[2].Value);
        Assert.Equal(JsonTokenType.Whitespace, tokens[3].TokenType);
        Assert.Equal(" ", tokens[3].Value);
        Assert.Equal(JsonTokenType.Comma, tokens[4].TokenType);
        Assert.Equal(",", tokens[4].Value);
        Assert.Equal(JsonTokenType.Whitespace, tokens[5].TokenType);
        Assert.Equal(" ", tokens[5].Value);
        Assert.Equal(JsonTokenType.Number, tokens[6].TokenType);
        Assert.Equal("2", tokens[6].Value);
    }

    [Fact]
    public void TestWhitespaceInNestedArray()
    {
        var tokens = Tokenize("[ [ 1 , 2 ] , [ 3 , 4 ] ]");
        Assert.Equal(25, tokens.Count);
        Assert.Equal(JsonTokenType.StartArray, tokens[0].TokenType);
        Assert.Equal("[", tokens[0].Value);
        Assert.Equal(JsonTokenType.Whitespace, tokens[1].TokenType);
        Assert.Equal(" ", tokens[1].Value);
        Assert.Equal(JsonTokenType.StartArray, tokens[2].TokenType);
        Assert.Equal("[", tokens[2].Value);
        Assert.Equal(JsonTokenType.Whitespace, tokens[3].TokenType);
        Assert.Equal(" ", tokens[3].Value);
        Assert.Equal(JsonTokenType.Number, tokens[4].TokenType);
        Assert.Equal("1", tokens[4].Value);
        Assert.Equal(JsonTokenType.Whitespace, tokens[5].TokenType);
        Assert.Equal(" ", tokens[5].Value);
        Assert.Equal(JsonTokenType.Comma, tokens[6].TokenType);
        Assert.Equal(",", tokens[6].Value);
        Assert.Equal(JsonTokenType.Whitespace, tokens[7].TokenType);
        Assert.Equal(" ", tokens[7].Value);
        Assert.Equal(JsonTokenType.Number, tokens[8].TokenType);
        Assert.Equal("2", tokens[8].Value);
        Assert.Equal(JsonTokenType.Whitespace, tokens[9].TokenType);
        Assert.Equal(" ", tokens[9].Value);
        Assert.Equal(JsonTokenType.EndArray, tokens[10].TokenType);
        Assert.Equal("]", tokens[10].Value);
        Assert.Equal(JsonTokenType.Whitespace, tokens[11].TokenType);
        Assert.Equal(" ", tokens[11].Value);
        Assert.Equal(JsonTokenType.Comma, tokens[12].TokenType);
        Assert.Equal(",", tokens[12].Value);
    }

    [Fact]
    public void TestMultipleWhitespaceCharacters()
    {
        var tokens = Tokenize("{\n\t\"name\":\r\n\t\"John\"\r\n}");
        Assert.Equal(8, tokens.Count);
        Assert.Equal(JsonTokenType.StartObject, tokens[0].TokenType);
        Assert.Equal("{", tokens[0].Value);
        Assert.Equal(JsonTokenType.Whitespace, tokens[1].TokenType);
        Assert.Equal("\n\t", tokens[1].Value);
        Assert.Equal(JsonTokenType.PropertyName, tokens[2].TokenType);
        Assert.Equal("\"name\"", tokens[2].Value);
        Assert.Equal(JsonTokenType.Colon, tokens[3].TokenType);
        Assert.Equal(":", tokens[3].Value);
        Assert.Equal(JsonTokenType.Whitespace, tokens[4].TokenType);
        Assert.Equal("\r\n\t", tokens[4].Value);
        Assert.Equal(JsonTokenType.StringValue, tokens[5].TokenType);
        Assert.Equal("\"John\"", tokens[5].Value);
        Assert.Equal(JsonTokenType.Whitespace, tokens[6].TokenType);
        Assert.Equal("\r\n", tokens[6].Value);
        Assert.Equal(JsonTokenType.EndObject, tokens[7].TokenType);
        Assert.Equal("}", tokens[7].Value);
    }

    [Fact]
    public void TestStopDelimiter()
    {
        var tokens = Tokenize("{\"name\": \"John\"} END", "END");
        Assert.Equal(7, tokens.Count);
        Assert.Equal(JsonTokenType.StartObject, tokens[0].TokenType);
        Assert.Equal("{", tokens[0].Value);
        Assert.Equal(JsonTokenType.PropertyName, tokens[1].TokenType);
        Assert.Equal("\"name\"", tokens[1].Value);
        Assert.Equal(JsonTokenType.Colon, tokens[2].TokenType);
        Assert.Equal(":", tokens[2].Value);
        Assert.Equal(JsonTokenType.Whitespace, tokens[3].TokenType);
        Assert.Equal(" ", tokens[3].Value);
        Assert.Equal(JsonTokenType.StringValue, tokens[4].TokenType);
        Assert.Equal("\"John\"", tokens[4].Value);
        Assert.Equal(JsonTokenType.EndObject, tokens[5].TokenType);
        Assert.Equal("}", tokens[5].Value);
        Assert.Equal(JsonTokenType.Whitespace, tokens[6].TokenType);
        Assert.Equal(" ", tokens[6].Value);
    }

    [Fact]
    public void TestStopDelimiterAtBeginning()
    {
        var tokens = Tokenize(" END{\"name\": \"John\"}", " END");
        Assert.Equal(0, tokens.Count);
    }

    [Fact]
    public void TestTrailingComma()
    {
        // Note: This test may not pass since we're not doing strict validation
        var tokens = Tokenize("{\"name\": \"John\",}");
        Assert.Equal(7, tokens.Count);
        Assert.Equal(JsonTokenType.StartObject, tokens[0].TokenType);
        Assert.Equal("{", tokens[0].Value);
        Assert.Equal(JsonTokenType.PropertyName, tokens[1].TokenType);
        Assert.Equal("\"name\"", tokens[1].Value);
        Assert.Equal(JsonTokenType.Colon, tokens[2].TokenType);
        Assert.Equal(":", tokens[2].Value);
        Assert.Equal(JsonTokenType.Whitespace, tokens[3].TokenType);
        Assert.Equal(" ", tokens[3].Value);
        Assert.Equal(JsonTokenType.StringValue, tokens[4].TokenType);
        Assert.Equal("\"John\"", tokens[4].Value);
        Assert.Equal(JsonTokenType.Comma, tokens[5].TokenType);
        Assert.Equal(",", tokens[5].Value);
        Assert.Equal(JsonTokenType.EndObject, tokens[6].TokenType);
        Assert.Equal("}", tokens[6].Value);
    }

    private static List<JsonToken> Tokenize(string input) => JsonTokenizer.Create().Parse(input);

    private static List<JsonToken> Tokenize(string input, string stopDelimiter)
    {
        var tokens = new List<JsonToken>();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        using var reader = new StreamReader(stream, Encoding.UTF8);
        JsonTokenizer.Create().ParseAsync(reader, stopDelimiter, token => tokens.Add(token)).GetAwaiter().GetResult();
        return tokens;
    }
}