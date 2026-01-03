using NTokenizers.Css;

namespace Css;

public class CssTokenizerTests
{
    [Fact]
    public void TestStartRuleSet()
    {
        var tokens = Tokenize("{");
        Assert.Single(tokens);
        Assert.Equal(CssTokenType.StartRuleSet, tokens[0].TokenType);
        Assert.Equal("{", tokens[0].Value);
    }

    [Fact]
    public void TestEndRuleSet()
    {
        var tokens = Tokenize("}");
        Assert.Single(tokens);
        Assert.Equal(CssTokenType.EndRuleSet, tokens[0].TokenType);
        Assert.Equal("}", tokens[0].Value);
    }

    [Fact]
    public void TestSelector()
    {
        var tokens = Tokenize("body");
        Assert.Single(tokens);
        Assert.Equal(CssTokenType.Selector, tokens[0].TokenType);
        Assert.Equal("body", tokens[0].Value);
    }

    [Fact]
    public void TestPseudoElement()
    {
        var tokens = Tokenize("::before");
        Assert.Single(tokens);
        Assert.Equal(CssTokenType.PseudoElement, tokens[0].TokenType);
        Assert.Equal("::before", tokens[0].Value);
    }

    [Fact]
    public void TestPropertyName()
    {
        var tokens = Tokenize("b { --color: red; }");
        Assert.Equal(11, tokens.Count);
        Assert.Equal(CssTokenType.PropertyName, tokens[4].TokenType);
        Assert.Equal("--color", tokens[4].Value);
    }

    [Fact]
    public void TestStringValue()
    {
        var tokens = Tokenize("\"hello\"");
        Assert.Equal(3, tokens.Count);
        Assert.Equal(CssTokenType.Quote, tokens[0].TokenType);
        Assert.Equal("\"", tokens[0].Value);
        Assert.Equal(CssTokenType.StringValue, tokens[1].TokenType);
        Assert.Equal("hello", tokens[1].Value);
        Assert.Equal(CssTokenType.Quote, tokens[2].TokenType);
        Assert.Equal("\"", tokens[2].Value);
    }

    [Fact]
    public void TestStringSingleQuoteValue()
    {
        var tokens = Tokenize("'hello'");
        Assert.Equal(3, tokens.Count);
        Assert.Equal(CssTokenType.Quote, tokens[0].TokenType);
        Assert.Equal("'", tokens[0].Value);
        Assert.Equal(CssTokenType.StringValue, tokens[1].TokenType);
        Assert.Equal("hello", tokens[1].Value);
        Assert.Equal(CssTokenType.Quote, tokens[2].TokenType);
        Assert.Equal("'", tokens[2].Value);
    }

    [Fact]
    public void TestNumber()
    {
        var tokens = Tokenize("123");
        Assert.Single(tokens);
        Assert.Equal(CssTokenType.Number, tokens[0].TokenType);
        Assert.Equal("123", tokens[0].Value);
    }

    [Fact]
    public void TestNumberWithDecimals()
    {
        var tokens = Tokenize("1.6");
        Assert.Single(tokens);
        Assert.Equal(CssTokenType.Number, tokens[0].TokenType);
        Assert.Equal("1.6", tokens[0].Value);
    }

    [Fact]
    public void TestDecimalNumberInCss()
    {
        var tokens = Tokenize("body { line-height: 1.6; }");
        Assert.Equal(11, tokens.Count);

        // Find the token for "1.6"
        var numberTokenIndex = tokens.FindIndex(t => t.Value == "1.6");
        Assert.True(numberTokenIndex >= 0, "Number token '1.6' not found");

        // Verify it's correctly identified as a Number token
        Assert.Equal(CssTokenType.Number, tokens[numberTokenIndex].TokenType);
        Assert.Equal("1.6", tokens[numberTokenIndex].Value);
    }

    [Fact]
    public void TestUnit()
    {
        var tokens = Tokenize("123px");
        Assert.Equal(2, tokens.Count);
        Assert.Equal(CssTokenType.Number, tokens[0].TokenType);
        Assert.Equal("123", tokens[0].Value);
        Assert.Equal(CssTokenType.Unit, tokens[1].TokenType);
        Assert.Equal("px", tokens[1].Value);

    }

    [Fact]
    public void TestFunction()
    {
        var tokens = Tokenize("body { color: rgb(255, 0, 0); }");
        Assert.Equal(20, tokens.Count);
        Assert.Equal(CssTokenType.Function, tokens[7].TokenType);
        Assert.Equal("rgb", tokens[7].Value);
        Assert.Equal(CssTokenType.OpenParen, tokens[8].TokenType);
        Assert.Equal("(", tokens[8].Value);
        Assert.Equal(CssTokenType.CloseParen, tokens[16].TokenType);
        Assert.Equal(")", tokens[16].Value);
    }

    [Fact]
    public void TestOpenParen()
    {
        var tokens = Tokenize("(");
        Assert.Single(tokens);
        Assert.Equal(CssTokenType.OpenParen, tokens[0].TokenType);
        Assert.Equal("(", tokens[0].Value);
    }

    [Fact]
    public void TestCloseParen()
    {
        var tokens = Tokenize(")");
        Assert.Single(tokens);
        Assert.Equal(CssTokenType.CloseParen, tokens[0].TokenType);
        Assert.Equal(")", tokens[0].Value);
    }

    [Fact]
    public void TestComment()
    {
        var tokens = Tokenize("/* comment */");
        Assert.Single(tokens);
        Assert.Equal(CssTokenType.Comment, tokens[0].TokenType);
        Assert.Equal("/* comment */", tokens[0].Value);
    }

    [Fact]
    public void TestColon()
    {
        var tokens = Tokenize(":");
        Assert.Single(tokens);
        Assert.Equal(CssTokenType.Colon, tokens[0].TokenType);
        Assert.Equal(":", tokens[0].Value);
    }

    [Fact]
    public void TestSemicolon()
    {
        var tokens = Tokenize(";");
        Assert.Single(tokens);
        Assert.Equal(CssTokenType.Semicolon, tokens[0].TokenType);
        Assert.Equal(";", tokens[0].Value);
    }

    [Fact]
    public void TestComma()
    {
        var tokens = Tokenize(",");
        Assert.Single(tokens);
        Assert.Equal(CssTokenType.Comma, tokens[0].TokenType);
        Assert.Equal(",", tokens[0].Value);
    }

    [Fact]
    public void TestWhitespace()
    {
        var tokens = Tokenize(" ");
        Assert.Single(tokens);
        Assert.Equal(CssTokenType.Whitespace, tokens[0].TokenType);
        Assert.Equal(" ", tokens[0].Value);
    }

    [Fact]
    public void TestAtRule()
    {
        var tokens = Tokenize("@media");
        Assert.Equal(2, tokens.Count);
        Assert.Equal(CssTokenType.AtRule, tokens[0].TokenType);
        Assert.Equal("@", tokens[0].Value);
        Assert.Equal(CssTokenType.Selector, tokens[1].TokenType); // Changed from Identifier to Selector
        Assert.Equal("media", tokens[1].Value);

    }

    [Fact]
    public void TestIdentifier()
    {
        var tokens = Tokenize("b { font: sans; }");
        Assert.Equal(11, tokens.Count);
        Assert.Equal(CssTokenType.Identifier, tokens[7].TokenType);
        Assert.Equal("sans", tokens[7].Value);
    }

    [Fact]
    public void TestOperator()
    {
        var tokens = Tokenize("+");
        Assert.Single(tokens);
        Assert.Equal(CssTokenType.Operator, tokens[0].TokenType);
        Assert.Equal("+", tokens[0].Value);
    }

    [Fact]
    public void TestHashID()
    {
        // HashID is now part of the selector token
        var tokens = Tokenize("#id");
        Assert.Single(tokens);
        Assert.Equal(CssTokenType.Selector, tokens[0].TokenType);
        Assert.Equal("#id", tokens[0].Value);
    }

    [Fact]
    public void TestDotClass()
    {
        var tokens = Tokenize(".class");
        Assert.Equal(2, tokens.Count);
        Assert.Equal(CssTokenType.DotClass, tokens[0].TokenType);
        Assert.Equal(".", tokens[0].Value);
        Assert.Equal(CssTokenType.Selector, tokens[1].TokenType);
        Assert.Equal("class", tokens[1].Value);
    }

    [Fact]
    public void TestLeftBracket()
    {
        var tokens = Tokenize("[");
        Assert.Single(tokens);
        Assert.Equal(CssTokenType.LeftBracket, tokens[0].TokenType);
        Assert.Equal("[", tokens[0].Value);
    }

    [Fact]
    public void TestRightBracket()
    {
        var tokens = Tokenize("]");
        Assert.Single(tokens);
        Assert.Equal(CssTokenType.RightBracket, tokens[0].TokenType);
        Assert.Equal("]", tokens[0].Value);
    }

    [Fact]
    public void TestEquals()
    {
        var tokens = Tokenize("=");
        Assert.Single(tokens);
        Assert.Equal(CssTokenType.Equals, tokens[0].TokenType);
        Assert.Equal("=", tokens[0].Value);
    }

    [Fact]
    public void TestSimpleCss()
    {
        string css = """
            body {
                color: #333;
                font-size: 16px;
            }
            """;

        var tokens = CssTokenizer.Create().Parse(css);

        Assert.NotEmpty(tokens);

        // Check for basic token types
        var tokenTypes = tokens.Select(t => t.TokenType).ToList();
        Assert.Contains(CssTokenType.StartRuleSet, tokenTypes);
        Assert.Contains(CssTokenType.EndRuleSet, tokenTypes);
        Assert.Contains(CssTokenType.Selector, tokenTypes);
    }

    [Fact]
    public void TestMultiplePropertiesInRuleSet()
    {
        string css = """
            .theme-text {
                color: var(--text-color, #333);
                background-color: var(--bg-color, #fff);
            }
            """;

        var tokens = CssTokenizer.Create().Parse(css);

        // Find the indices of 'color' and 'background-color'
        var colorIndex = tokens.FindIndex(t => t.Value == "color");
        var bgColorIndex = tokens.FindIndex(t => t.Value == "background-color");

        Assert.True(colorIndex >= 0, "color token not found");
        Assert.True(bgColorIndex >= 0, "background-color token not found");

        // Both should be Identifier tokens, not Selector tokens
        Assert.Equal(CssTokenType.Identifier, tokens[colorIndex].TokenType);
        Assert.Equal(CssTokenType.Identifier, tokens[bgColorIndex].TokenType);
    }

    [Fact]
    public void TestDistinguishColorValuesFromSelectors()
    {
        string css = 
            """
            #target {
            --primary-color: #3498db;
            }
            """;

        var tokens = CssTokenizer.Create().Parse(css);

        // Find the index of '--primary-color'
        var propertyIndex = tokens.FindIndex(t => t.Value == "--primary-color");
        Assert.True(propertyIndex >= 0, "Custom property token not found");

        // The custom property name should be a PropertyName token
        Assert.Equal(CssTokenType.PropertyName, tokens[propertyIndex].TokenType);

        // Verify that the selector "#target" is correctly identified as a Selector token
        var selectorIndex = tokens.FindIndex(t => t.Value == "#target");
        Assert.True(selectorIndex >= 0, "Selector token not found");
        Assert.Equal(CssTokenType.Selector, tokens[selectorIndex].TokenType);

        // Verify that the colon is correctly identified as a Colon token
        var colonIndex = tokens.FindIndex(t => t.Value == ":");
        Assert.True(colonIndex >= 0, "Colon token not found");
        Assert.Equal(CssTokenType.Colon, tokens[colonIndex].TokenType);
    }

    private static List<CssToken> Tokenize(string input) => CssTokenizer.Create().Parse(input);
}