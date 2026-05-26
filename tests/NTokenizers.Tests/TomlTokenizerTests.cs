using NTokenizers.Toml;
using System.Text;

namespace Toml;

public class TomlTokenizerTests
{
    [Fact]
    public void TestEmptyInput()
    {
        var tokens = Tokenize("");
        Assert.Empty(tokens);
    }

    [Fact]
    public void TestWhitespace()
    {
        var tokens = Tokenize("   \n\t");
        Assert.Single(tokens);
        Assert.Equal(TomlTokenType.Whitespace, tokens[0].TokenType);
        Assert.Equal("   \n\t", tokens[0].Value);
    }

    [Fact]
    public void TestComment()
    {
        var tokens = Tokenize("# this is a comment\nkey = \"value\"");
        Assert.Equal(8, tokens.Count);
        Assert.Equal(TomlTokenType.Comment, tokens[0].TokenType);
        Assert.Equal("# this is a comment\n", tokens[0].Value);
        Assert.Equal(TomlTokenType.Identifier, tokens[1].TokenType);
        Assert.Equal("key", tokens[1].Value);
        Assert.Equal(TomlTokenType.Whitespace, tokens[2].TokenType);
        Assert.Equal(" ", tokens[2].Value);
        Assert.Equal(TomlTokenType.Equal, tokens[3].TokenType);
        Assert.Equal("=", tokens[3].Value);
        Assert.Equal(TomlTokenType.Whitespace, tokens[4].TokenType);
        Assert.Equal(" ", tokens[4].Value);
        Assert.Equal(TomlTokenType.StringQuote, tokens[5].TokenType);
        Assert.Equal("\"", tokens[5].Value);
        Assert.Equal(TomlTokenType.StringValue, tokens[6].TokenType);
        Assert.Equal("value", tokens[6].Value);
        Assert.Equal(TomlTokenType.StringQuote, tokens[7].TokenType);
        Assert.Equal("\"", tokens[7].Value);
    }

    [Fact]
    public void TestBasicString()
    {
        var tokens = Tokenize("key = \"hello world\"");
        Assert.Equal(7, tokens.Count);
        Assert.Equal(TomlTokenType.Identifier, tokens[0].TokenType);
        Assert.Equal("key", tokens[0].Value);
        Assert.Equal(TomlTokenType.Whitespace, tokens[1].TokenType);
        Assert.Equal(" ", tokens[1].Value);
        Assert.Equal(TomlTokenType.Equal, tokens[2].TokenType);
        Assert.Equal("=", tokens[2].Value);
        Assert.Equal(TomlTokenType.Whitespace, tokens[3].TokenType);
        Assert.Equal(" ", tokens[3].Value);
        Assert.Equal(TomlTokenType.StringQuote, tokens[4].TokenType);
        Assert.Equal("\"", tokens[4].Value);
        Assert.Equal(TomlTokenType.StringValue, tokens[5].TokenType);
        Assert.Equal("hello world", tokens[5].Value);
        Assert.Equal(TomlTokenType.StringQuote, tokens[6].TokenType);
        Assert.Equal("\"", tokens[6].Value);
    }

    [Fact]
    public void TestLiteralString()
    {
        var tokens = Tokenize("key = 'hello world'");
        Assert.Equal(7, tokens.Count);
        Assert.Equal(TomlTokenType.Identifier, tokens[0].TokenType);
        Assert.Equal("key", tokens[0].Value);
        Assert.Equal(TomlTokenType.StringQuote, tokens[4].TokenType);
        Assert.Equal("'", tokens[4].Value);
        Assert.Equal(TomlTokenType.StringValue, tokens[5].TokenType);
        Assert.Equal("hello world", tokens[5].Value);
        Assert.Equal(TomlTokenType.StringQuote, tokens[6].TokenType);
        Assert.Equal("'", tokens[6].Value);
    }

    [Fact]
    public void TestEscapedString()
    {
        var tokens = Tokenize("key = \"hello \\\"world\\\"\"");
        Assert.Equal(7, tokens.Count);
        Assert.Equal(TomlTokenType.StringValue, tokens[5].TokenType);
        Assert.Equal("hello \\\"world\\\"", tokens[5].Value);
    }

    [Fact]
    public void TestMultilineBasicString()
    {
        var tokens = Tokenize("key = \"\"\"multi\nline\nstring\"\"\"");
        Assert.Equal(7, tokens.Count);
        Assert.Equal(TomlTokenType.StringQuote, tokens[4].TokenType);
        Assert.Equal("\"\"\"", tokens[4].Value);
        Assert.Equal(TomlTokenType.StringValue, tokens[5].TokenType);
        Assert.Equal("multi\nline\nstring", tokens[5].Value);
        Assert.Equal(TomlTokenType.StringQuote, tokens[6].TokenType);
        Assert.Equal("\"\"\"", tokens[6].Value);
    }

    [Fact]
    public void TestMultilineLiteralString()
    {
        var tokens = Tokenize("key = '''multi\nline\nliteral'''");
        Assert.Equal(7, tokens.Count);
        Assert.Equal(TomlTokenType.StringQuote, tokens[4].TokenType);
        Assert.Equal("'''", tokens[4].Value);
        Assert.Equal(TomlTokenType.StringValue, tokens[5].TokenType);
        Assert.Equal("multi\nline\nliteral", tokens[5].Value);
        Assert.Equal(TomlTokenType.StringQuote, tokens[6].TokenType);
        Assert.Equal("'''", tokens[6].Value);
    }

    [Fact]
    public void TestInteger()
    {
        var tokens = Tokenize("key = 42");
        Assert.Equal(5, tokens.Count);
        Assert.Equal(TomlTokenType.Identifier, tokens[0].TokenType);
        Assert.Equal("key", tokens[0].Value);
        Assert.Equal(TomlTokenType.Whitespace, tokens[1].TokenType);
        Assert.Equal(" ", tokens[1].Value);
        Assert.Equal(TomlTokenType.Equal, tokens[2].TokenType);
        Assert.Equal("=", tokens[2].Value);
        Assert.Equal(TomlTokenType.Whitespace, tokens[3].TokenType);
        Assert.Equal(" ", tokens[3].Value);
        Assert.Equal(TomlTokenType.Number, tokens[4].TokenType);
        Assert.Equal("42", tokens[4].Value);
    }

    [Fact]
    public void TestNegativeInteger()
    {
        var tokens = Tokenize("key = -17");
        Assert.Equal(5, tokens.Count);
        Assert.Equal(TomlTokenType.Number, tokens[4].TokenType);
        Assert.Equal("-17", tokens[4].Value);
    }

    [Fact]
    public void TestFloat()
    {
        var tokens = Tokenize("key = 3.14");
        Assert.Equal(5, tokens.Count);
        Assert.Equal(TomlTokenType.Number, tokens[4].TokenType);
        Assert.Equal("3.14", tokens[4].Value);
    }

    [Fact]
    public void TestScientificNotation()
    {
        var tokens = Tokenize("key = 1e6");
        Assert.Equal(5, tokens.Count);
        Assert.Equal(TomlTokenType.Number, tokens[4].TokenType);
        Assert.Equal("1e6", tokens[4].Value);
    }

    [Fact]
    public void TestNegativeScientificNotation()
    {
        var tokens = Tokenize("key = -2.5e-3");
        Assert.Equal(5, tokens.Count);
        Assert.Equal(TomlTokenType.Number, tokens[4].TokenType);
        Assert.Equal("-2.5e-3", tokens[4].Value);
    }

    [Fact]
    public void TestHexNumber()
    {
        var tokens = Tokenize("key = 0xDEADBEEF");
        Assert.Equal(5, tokens.Count);
        Assert.Equal(TomlTokenType.Number, tokens[4].TokenType);
        Assert.Equal("0xDEADBEEF", tokens[4].Value);
    }

    [Fact]
    public void TestOctalNumber()
    {
        var tokens = Tokenize("key = 0o755");
        Assert.Equal(5, tokens.Count);
        Assert.Equal(TomlTokenType.Number, tokens[4].TokenType);
        Assert.Equal("0o755", tokens[4].Value);
    }

    [Fact]
    public void TestBinaryNumber()
    {
        var tokens = Tokenize("key = 0b101010");
        Assert.Equal(5, tokens.Count);
        Assert.Equal(TomlTokenType.Number, tokens[4].TokenType);
        Assert.Equal("0b101010", tokens[4].Value);
    }

    [Fact]
    public void TestInfinityValue()
    {
        var tokens = Tokenize("key = inf");
        Assert.Equal(5, tokens.Count);
        Assert.Equal(TomlTokenType.Number, tokens[4].TokenType);
        Assert.Equal("inf", tokens[4].Value);
    }

    [Fact]
    public void TestNegativeInfinityValue()
    {
        var tokens = Tokenize("key = -inf");
        Assert.Equal(5, tokens.Count);
        Assert.Equal(TomlTokenType.Number, tokens[4].TokenType);
        Assert.Equal("-inf", tokens[4].Value);
    }

    [Fact]
    public void TestNaNValue()
    {
        var tokens = Tokenize("key = nan");
        Assert.Equal(5, tokens.Count);
        Assert.Equal(TomlTokenType.Number, tokens[4].TokenType);
        Assert.Equal("nan", tokens[4].Value);
    }

    [Fact]
    public void TestInfinityAsKey()
    {
        var tokens = Tokenize("inf = inf");
        Assert.Equal(5, tokens.Count);
        Assert.Equal(TomlTokenType.Identifier, tokens[0].TokenType);
        Assert.Equal("inf", tokens[0].Value);
        Assert.Equal(TomlTokenType.Whitespace, tokens[1].TokenType);
        Assert.Equal(" ", tokens[1].Value);
        Assert.Equal(TomlTokenType.Equal, tokens[2].TokenType);
        Assert.Equal("=", tokens[2].Value);
        Assert.Equal(TomlTokenType.Whitespace, tokens[3].TokenType);
        Assert.Equal(" ", tokens[3].Value);
        Assert.Equal(TomlTokenType.Number, tokens[4].TokenType);
        Assert.Equal("inf", tokens[4].Value);
    }

    [Fact]
    public void TestNaNAsKey()
    {
        var tokens = Tokenize("nan = nan");
        Assert.Equal(5, tokens.Count);
        Assert.Equal(TomlTokenType.Identifier, tokens[0].TokenType);
        Assert.Equal("nan", tokens[0].Value);
        Assert.Equal(TomlTokenType.Whitespace, tokens[1].TokenType);
        Assert.Equal(" ", tokens[1].Value);
        Assert.Equal(TomlTokenType.Equal, tokens[2].TokenType);
        Assert.Equal("=", tokens[2].Value);
        Assert.Equal(TomlTokenType.Whitespace, tokens[3].TokenType);
        Assert.Equal(" ", tokens[3].Value);
        Assert.Equal(TomlTokenType.Number, tokens[4].TokenType);
        Assert.Equal("nan", tokens[4].Value);
    }

    [Fact]
    public void TestBooleanTrue()
    {
        var tokens = Tokenize("key = true");
        Assert.Equal(5, tokens.Count);
        Assert.Equal(TomlTokenType.Boolean, tokens[4].TokenType);
        Assert.Equal("true", tokens[4].Value);
    }

    [Fact]
    public void TestBooleanFalse()
    {
        var tokens = Tokenize("key = false");
        Assert.Equal(5, tokens.Count);
        Assert.Equal(TomlTokenType.Boolean, tokens[4].TokenType);
        Assert.Equal("false", tokens[4].Value);
    }

    [Fact]
    public void TestLocalDate()
    {
        var tokens = Tokenize("key = 2026-05-26");
        Assert.Equal(5, tokens.Count);
        Assert.Equal(TomlTokenType.DateTime, tokens[4].TokenType);
        Assert.Equal("2026-05-26", tokens[4].Value);
    }

    [Fact]
    public void TestLocalTime()
    {
        var tokens = Tokenize("key = 13:45:30");
        Assert.Equal(5, tokens.Count);
        Assert.Equal(TomlTokenType.DateTime, tokens[4].TokenType);
        Assert.Equal("13:45:30", tokens[4].Value);
    }

    [Fact]
    public void TestLocalDateTime()
    {
        var tokens = Tokenize("key = 2026-05-26T13:45:30");
        Assert.Equal(5, tokens.Count);
        Assert.Equal(TomlTokenType.DateTime, tokens[4].TokenType);
        Assert.Equal("2026-05-26T13:45:30", tokens[4].Value);
    }

    [Fact]
    public void TestOffsetDateTime()
    {
        var tokens = Tokenize("key = 2026-05-26T13:45:30+02:00");
        Assert.Equal(5, tokens.Count);
        Assert.Equal(TomlTokenType.DateTime, tokens[4].TokenType);
        Assert.Equal("2026-05-26T13:45:30+02:00", tokens[4].Value);
    }

    [Fact]
    public void TestZuluDateTime()
    {
        var tokens = Tokenize("key = 2026-05-26T13:45:30Z");
        Assert.Equal(5, tokens.Count);
        Assert.Equal(TomlTokenType.DateTime, tokens[4].TokenType);
        Assert.Equal("2026-05-26T13:45:30Z", tokens[4].Value);
    }

    [Fact]
    public void TestDottedKey()
    {
        var tokens = Tokenize("a.b.c = \"dotted key\"");
        Assert.Equal(11, tokens.Count);
        Assert.Equal(TomlTokenType.Identifier, tokens[0].TokenType);
        Assert.Equal("a", tokens[0].Value);
        Assert.Equal(TomlTokenType.Dot, tokens[1].TokenType);
        Assert.Equal(".", tokens[1].Value);
        Assert.Equal(TomlTokenType.Identifier, tokens[2].TokenType);
        Assert.Equal("b", tokens[2].Value);
        Assert.Equal(TomlTokenType.Dot, tokens[3].TokenType);
        Assert.Equal(".", tokens[3].Value);
        Assert.Equal(TomlTokenType.Identifier, tokens[4].TokenType);
        Assert.Equal("c", tokens[4].Value);
        Assert.Equal(TomlTokenType.Whitespace, tokens[5].TokenType);
        Assert.Equal(" ", tokens[5].Value);
        Assert.Equal(TomlTokenType.Equal, tokens[6].TokenType);
        Assert.Equal("=", tokens[6].Value);
        Assert.Equal(TomlTokenType.Whitespace, tokens[7].TokenType);
        Assert.Equal(" ", tokens[7].Value);
        Assert.Equal(TomlTokenType.StringQuote, tokens[8].TokenType);
        Assert.Equal("\"", tokens[8].Value);
        Assert.Equal(TomlTokenType.StringValue, tokens[9].TokenType);
        Assert.Equal("dotted key", tokens[9].Value);
        Assert.Equal(TomlTokenType.StringQuote, tokens[10].TokenType);
        Assert.Equal("\"", tokens[10].Value);
    }

    [Fact]
    public void TestTableHeader()
    {
        var tokens = Tokenize("[table]");
        Assert.Equal(3, tokens.Count);
        Assert.Equal(TomlTokenType.OpenBracket, tokens[0].TokenType);
        Assert.Equal("[", tokens[0].Value);
        Assert.Equal(TomlTokenType.Identifier, tokens[1].TokenType);
        Assert.Equal("table", tokens[1].Value);
        Assert.Equal(TomlTokenType.CloseBracket, tokens[2].TokenType);
        Assert.Equal("]", tokens[2].Value);
    }

    [Fact]
    public void TestArrayTableHeader()
    {
        var tokens = Tokenize("[[array_table]]");
        Assert.Equal(5, tokens.Count);
        Assert.Equal(TomlTokenType.OpenBracket, tokens[0].TokenType);
        Assert.Equal("[", tokens[0].Value);
        Assert.Equal(TomlTokenType.OpenBracket, tokens[1].TokenType);
        Assert.Equal("[", tokens[1].Value);
        Assert.Equal(TomlTokenType.Identifier, tokens[2].TokenType);
        Assert.Equal("array_table", tokens[2].Value);
        Assert.Equal(TomlTokenType.CloseBracket, tokens[3].TokenType);
        Assert.Equal("]", tokens[3].Value);
        Assert.Equal(TomlTokenType.CloseBracket, tokens[4].TokenType);
        Assert.Equal("]", tokens[4].Value);
    }

    [Fact]
    public void TestArrayValue()
    {
        var tokens = Tokenize("array = [1, 2, 3]");
        Assert.Equal(13, tokens.Count);
        Assert.Equal(TomlTokenType.Identifier, tokens[0].TokenType);
        Assert.Equal("array", tokens[0].Value);
        Assert.Equal(TomlTokenType.Whitespace, tokens[1].TokenType);
        Assert.Equal(" ", tokens[1].Value);
        Assert.Equal(TomlTokenType.Equal, tokens[2].TokenType);
        Assert.Equal("=", tokens[2].Value);
        Assert.Equal(TomlTokenType.Whitespace, tokens[3].TokenType);
        Assert.Equal(" ", tokens[3].Value);
        Assert.Equal(TomlTokenType.OpenBracket, tokens[4].TokenType);
        Assert.Equal("[", tokens[4].Value);
        Assert.Equal(TomlTokenType.Number, tokens[5].TokenType);
        Assert.Equal("1", tokens[5].Value);
        Assert.Equal(TomlTokenType.Comma, tokens[6].TokenType);
        Assert.Equal(",", tokens[6].Value);
        Assert.Equal(TomlTokenType.Whitespace, tokens[7].TokenType);
        Assert.Equal(" ", tokens[7].Value);
        Assert.Equal(TomlTokenType.Number, tokens[8].TokenType);
        Assert.Equal("2", tokens[8].Value);
        Assert.Equal(TomlTokenType.Comma, tokens[9].TokenType);
        Assert.Equal(",", tokens[9].Value);
        Assert.Equal(TomlTokenType.Whitespace, tokens[10].TokenType);
        Assert.Equal(" ", tokens[10].Value);
        Assert.Equal(TomlTokenType.Number, tokens[11].TokenType);
        Assert.Equal("3", tokens[11].Value);
        Assert.Equal(TomlTokenType.CloseBracket, tokens[12].TokenType);
        Assert.Equal("]", tokens[12].Value);
    }

    [Fact]
    public void TestInlineTable()
    {
        var tokens = Tokenize("point = {x = 10, y = 20}");
        Assert.Equal(18, tokens.Count);
        Assert.Equal(TomlTokenType.Identifier, tokens[0].TokenType);
        Assert.Equal("point", tokens[0].Value);
        Assert.Equal(TomlTokenType.Whitespace, tokens[1].TokenType);
        Assert.Equal(" ", tokens[1].Value);
        Assert.Equal(TomlTokenType.Equal, tokens[2].TokenType);
        Assert.Equal("=", tokens[2].Value);
        Assert.Equal(TomlTokenType.Whitespace, tokens[3].TokenType);
        Assert.Equal(" ", tokens[3].Value);
        Assert.Equal(TomlTokenType.OpenBrace, tokens[4].TokenType);
        Assert.Equal("{", tokens[4].Value);
        Assert.Equal(TomlTokenType.Identifier, tokens[5].TokenType);
        Assert.Equal("x", tokens[5].Value);
        Assert.Equal(TomlTokenType.Whitespace, tokens[6].TokenType);
        Assert.Equal(" ", tokens[6].Value);
        Assert.Equal(TomlTokenType.Equal, tokens[7].TokenType);
        Assert.Equal("=", tokens[7].Value);
        Assert.Equal(TomlTokenType.Whitespace, tokens[8].TokenType);
        Assert.Equal(" ", tokens[8].Value);
        Assert.Equal(TomlTokenType.Number, tokens[9].TokenType);
        Assert.Equal("10", tokens[9].Value);
        Assert.Equal(TomlTokenType.Comma, tokens[10].TokenType);
        Assert.Equal(",", tokens[10].Value);
        Assert.Equal(TomlTokenType.Whitespace, tokens[11].TokenType);
        Assert.Equal(" ", tokens[11].Value);
        Assert.Equal(TomlTokenType.Identifier, tokens[12].TokenType);
        Assert.Equal("y", tokens[12].Value);
        Assert.Equal(TomlTokenType.Whitespace, tokens[13].TokenType);
        Assert.Equal(" ", tokens[13].Value);
        Assert.Equal(TomlTokenType.Equal, tokens[14].TokenType);
        Assert.Equal("=", tokens[14].Value);
        Assert.Equal(TomlTokenType.Whitespace, tokens[15].TokenType);
        Assert.Equal(" ", tokens[15].Value);
        Assert.Equal(TomlTokenType.Number, tokens[16].TokenType);
        Assert.Equal("20", tokens[16].Value);
        Assert.Equal(TomlTokenType.CloseBrace, tokens[17].TokenType);
        Assert.Equal("}", tokens[17].Value);
    }

    [Fact]
    public void TestAllTokens()
    {
        var input = @"# Comment over het bestand
title = ""TOML demo""
active = true
debug = false
name = 'literal string'
int_dec = 42
int_neg = -17
float = 3.14
float_exp = 1e6
hex = 0xDEADBEEF
oct = 0o755
bin = 0b101010
inf_val = inf
ninf_val = -inf
nan_val = nan
date = 2026-05-26
time = 13:45:30
datetime = 2026-05-26T13:45:30Z
datetime_offset = 2026-05-26T13:45:30+02:00
array = [1, 2, 3]
point = {x = 10, y = 20}
a.b.c = ""dotted key""
[table]
key = ""value""
[[array_table]]
id = 1";

        var tokens = Tokenize(input);

        // Verify we got tokens for all the key types
        var types = tokens.Select(t => t.TokenType).ToList();

        Assert.Contains(TomlTokenType.Comment, types);
        Assert.Contains(TomlTokenType.Identifier, types);
        Assert.Contains(TomlTokenType.Equal, types);
        Assert.Contains(TomlTokenType.StringQuote, types);
        Assert.Contains(TomlTokenType.StringValue, types);
        Assert.Contains(TomlTokenType.Boolean, types);
        Assert.Contains(TomlTokenType.Number, types);
        Assert.Contains(TomlTokenType.DateTime, types);
        Assert.Contains(TomlTokenType.OpenBracket, types);
        Assert.Contains(TomlTokenType.CloseBracket, types);
        Assert.Contains(TomlTokenType.OpenBrace, types);
        Assert.Contains(TomlTokenType.CloseBrace, types);
        Assert.Contains(TomlTokenType.Comma, types);
        Assert.Contains(TomlTokenType.Dot, types);
        Assert.Contains(TomlTokenType.Whitespace, types);
    }

    [Fact]
    public void TestStopDelimiter()
    {
        var tokens = Tokenize("key = \"value\"\nEND", "END");
        Assert.Equal(7, tokens.Count);
        Assert.Equal(TomlTokenType.Identifier, tokens[0].TokenType);
        Assert.Equal("key", tokens[0].Value);
        Assert.Equal(TomlTokenType.Whitespace, tokens[1].TokenType);
        Assert.Equal(" ", tokens[1].Value);
        Assert.Equal(TomlTokenType.Equal, tokens[2].TokenType);
        Assert.Equal("=", tokens[2].Value);
        Assert.Equal(TomlTokenType.Whitespace, tokens[3].TokenType);
        Assert.Equal(" ", tokens[3].Value);
        Assert.Equal(TomlTokenType.StringQuote, tokens[4].TokenType);
        Assert.Equal("\"", tokens[4].Value);
        Assert.Equal(TomlTokenType.StringValue, tokens[5].TokenType);
        Assert.Equal("value", tokens[5].Value);
        Assert.Equal(TomlTokenType.StringQuote, tokens[6].TokenType);
        Assert.Equal("\"", tokens[6].Value);
    }

    [Fact]
    public void TestStopDelimiterAtBeginning()
    {
        var tokens = Tokenize("END\nkey = \"value\"", "END");
        Assert.Empty(tokens);
    }

    [Fact]
    public async Task TestCancellation()
    {
        var largeToml = string.Join("\n", Enumerable.Range(1, 10000).Select(i => $"key{i} = {i}"));

        using var cts = new CancellationTokenSource();
        var tokens = new List<TomlToken>();
        int tokenCount = 0;

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(largeToml));

        var parseTask = Task.Run(async () =>
        {
            await TomlTokenizer.Create().ParseAsync(stream, cts.Token, token =>
            {
                tokens.Add(token);
                tokenCount++;
                if (tokenCount == 10)
                {
                    cts.Cancel();
                }
            });
        }, TestContext.Current.CancellationToken);

        await parseTask;

        Assert.True(tokenCount < 10000, "Tokenization should have been cancelled");
    }

    private static List<TomlToken> Tokenize(string input) => TomlTokenizer.Create().Parse(input);

    private static List<TomlToken> Tokenize(string input, string stopDelimiter)
    {
        var tokens = new List<TomlToken>();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        using var reader = new StreamReader(stream, Encoding.UTF8);
        TomlTokenizer.Create().ParseAsync(reader, stopDelimiter, token => tokens.Add(token)).GetAwaiter().GetResult();
        return tokens;
    }
}
