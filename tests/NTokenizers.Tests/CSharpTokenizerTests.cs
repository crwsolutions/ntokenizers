using NTokenizers.CSharp;
using System.Text;

namespace CSharp;

public class CSharpTokenizerTests
{
    [Fact]
    public void TestSimpleVariableDeclaration()
    {
        var tokens = Tokenize("int x = 5;");
        Assert.Equal(8, tokens.Count);
        Assert.Equal(CSharpTokenType.Keyword, tokens[0].TokenType);
        Assert.Equal("int", tokens[0].Value);
        Assert.Equal(CSharpTokenType.Whitespace, tokens[1].TokenType);
        Assert.Equal(" ", tokens[1].Value);
        Assert.Equal(CSharpTokenType.Identifier, tokens[2].TokenType);
        Assert.Equal("x", tokens[2].Value);
        Assert.Equal(CSharpTokenType.Whitespace, tokens[3].TokenType);
        Assert.Equal(" ", tokens[3].Value);
        Assert.Equal(CSharpTokenType.Operator, tokens[4].TokenType);
        Assert.Equal("=", tokens[4].Value);
        Assert.Equal(CSharpTokenType.Whitespace, tokens[5].TokenType);
        Assert.Equal(" ", tokens[5].Value);
        Assert.Equal(CSharpTokenType.Number, tokens[6].TokenType);
        Assert.Equal("5", tokens[6].Value);
        Assert.Equal(CSharpTokenType.SequenceTerminator, tokens[7].TokenType);
        Assert.Equal(";", tokens[7].Value);
    }

    [Fact]
    public void TestKeywordRecognition()
    {
        var tokens = Tokenize("class public static void");
        Assert.Equal(7, tokens.Count);
        Assert.Equal(CSharpTokenType.Keyword, tokens[0].TokenType);
        Assert.Equal("class", tokens[0].Value);
        Assert.Equal(CSharpTokenType.Keyword, tokens[2].TokenType);
        Assert.Equal("public", tokens[2].Value);
        Assert.Equal(CSharpTokenType.Keyword, tokens[4].TokenType);
        Assert.Equal("static", tokens[4].Value);
        Assert.Equal(CSharpTokenType.Keyword, tokens[6].TokenType);
        Assert.Equal("void", tokens[6].Value);
    }

    [Fact]
    public void TestIdentifierVsKeyword()
    {
        var tokens = Tokenize("int myVariable");
        Assert.Equal(3, tokens.Count);
        Assert.Equal(CSharpTokenType.Keyword, tokens[0].TokenType);
        Assert.Equal("int", tokens[0].Value);
        Assert.Equal(CSharpTokenType.Identifier, tokens[2].TokenType);
        Assert.Equal("myVariable", tokens[2].Value);
    }

    [Fact]
    public void TestStringLiteral()
    {
        var tokens = Tokenize("\"Hello World\"");
        Assert.Single(tokens);
        Assert.Equal(CSharpTokenType.StringValue, tokens[0].TokenType);
        Assert.Equal("\"Hello World\"", tokens[0].Value);
    }

    [Fact]
    public void TestStringWithEscapes()
    {
        var tokens = Tokenize("\"Hello \\\"World\\\"\"");
        Assert.Single(tokens);
        Assert.Equal(CSharpTokenType.StringValue, tokens[0].TokenType);
        Assert.Equal("\"Hello \\\"World\\\"\"", tokens[0].Value);
    }

    [Fact]
    public void TestVerbatimString()
    {
        var tokens = Tokenize("@\"C:\\Path\\To\\File\"");
        Assert.Single(tokens);
        Assert.Equal(CSharpTokenType.StringValue, tokens[0].TokenType);
        Assert.Equal("@\"C:\\Path\\To\\File\"", tokens[0].Value);
    }

    [Fact]
    public void TestVerbatimStringWithDoubleQuotes()
    {
        var tokens = Tokenize("@\"He said \"\"Hello\"\"\"");
        Assert.Single(tokens);
        Assert.Equal(CSharpTokenType.StringValue, tokens[0].TokenType);
        Assert.Equal("@\"He said \"\"Hello\"\"\"", tokens[0].Value);
    }

    [Fact]
    public void TestIntegerNumber()
    {
        var tokens = Tokenize("123");
        Assert.Single(tokens);
        Assert.Equal(CSharpTokenType.Number, tokens[0].TokenType);
        Assert.Equal("123", tokens[0].Value);
    }

    [Fact]
    public void TestDecimalNumber()
    {
        var tokens = Tokenize("123.45");
        Assert.Single(tokens);
        Assert.Equal(CSharpTokenType.Number, tokens[0].TokenType);
        Assert.Equal("123.45", tokens[0].Value);
    }

    [Fact]
    public void TestScientificNotation()
    {
        var tokens = Tokenize("1.2e3");
        Assert.Single(tokens);
        Assert.Equal(CSharpTokenType.Number, tokens[0].TokenType);
        Assert.Equal("1.2e3", tokens[0].Value);
    }

    [Fact]
    public void TestOperatorEquals()
    {
        var tokens = Tokenize("==");
        Assert.Single(tokens);
        Assert.Equal(CSharpTokenType.Equals, tokens[0].TokenType);
        Assert.Equal("==", tokens[0].Value);
    }

    [Fact]
    public void TestOperatorNotEquals()
    {
        var tokens = Tokenize("!=");
        Assert.Single(tokens);
        Assert.Equal(CSharpTokenType.NotEquals, tokens[0].TokenType);
        Assert.Equal("!=", tokens[0].Value);
    }

    [Fact]
    public void TestOperatorAnd()
    {
        var tokens = Tokenize("&&");
        Assert.Single(tokens);
        Assert.Equal(CSharpTokenType.And, tokens[0].TokenType);
        Assert.Equal("&&", tokens[0].Value);
    }

    [Fact]
    public void TestOperatorOr()
    {
        var tokens = Tokenize("||");
        Assert.Single(tokens);
        Assert.Equal(CSharpTokenType.Or, tokens[0].TokenType);
        Assert.Equal("||", tokens[0].Value);
    }

    [Fact]
    public void TestOperatorNot()
    {
        var tokens = Tokenize("!");
        Assert.Single(tokens);
        Assert.Equal(CSharpTokenType.Not, tokens[0].TokenType);
        Assert.Equal("!", tokens[0].Value);
    }

    [Fact]
    public void TestOperatorGreaterThan()
    {
        var tokens = Tokenize(">");
        Assert.Single(tokens);
        Assert.Equal(CSharpTokenType.GreaterThan, tokens[0].TokenType);
        Assert.Equal(">", tokens[0].Value);
    }

    [Fact]
    public void TestOperatorLessThan()
    {
        var tokens = Tokenize("<");
        Assert.Single(tokens);
        Assert.Equal(CSharpTokenType.LessThan, tokens[0].TokenType);
        Assert.Equal("<", tokens[0].Value);
    }

    [Fact]
    public void TestOperatorGreaterThanOrEqual()
    {
        var tokens = Tokenize(">=");
        Assert.Single(tokens);
        Assert.Equal(CSharpTokenType.GreaterThanOrEqual, tokens[0].TokenType);
        Assert.Equal(">=", tokens[0].Value);
    }

    [Fact]
    public void TestOperatorLessThanOrEqual()
    {
        var tokens = Tokenize("<=");
        Assert.Single(tokens);
        Assert.Equal(CSharpTokenType.LessThanOrEqual, tokens[0].TokenType);
        Assert.Equal("<=", tokens[0].Value);
    }

    [Fact]
    public void TestOperatorPlus()
    {
        var tokens = Tokenize("+");
        Assert.Single(tokens);
        Assert.Equal(CSharpTokenType.Plus, tokens[0].TokenType);
        Assert.Equal("+", tokens[0].Value);
    }

    [Fact]
    public void TestOperatorMinus()
    {
        var tokens = Tokenize("-");
        Assert.Single(tokens);
        Assert.Equal(CSharpTokenType.Minus, tokens[0].TokenType);
        Assert.Equal("-", tokens[0].Value);
    }

    [Fact]
    public void TestOperatorMultiply()
    {
        var tokens = Tokenize("*");
        Assert.Single(tokens);
        Assert.Equal(CSharpTokenType.Multiply, tokens[0].TokenType);
        Assert.Equal("*", tokens[0].Value);
    }

    [Fact]
    public void TestOperatorDivide()
    {
        var tokens = Tokenize("/");
        Assert.Single(tokens);
        Assert.Equal(CSharpTokenType.Divide, tokens[0].TokenType);
        Assert.Equal("/", tokens[0].Value);
    }

    [Fact]
    public void TestOperatorModulo()
    {
        var tokens = Tokenize("%");
        Assert.Single(tokens);
        Assert.Equal(CSharpTokenType.Modulo, tokens[0].TokenType);
        Assert.Equal("%", tokens[0].Value);
    }

    [Fact]
    public void TestOperatorNullCoalescing()
    {
        var tokens = Tokenize("??");
        Assert.Single(tokens);
        Assert.Equal(CSharpTokenType.Operator, tokens[0].TokenType);
        Assert.Equal("??", tokens[0].Value);
    }

    [Fact]
    public void TestOperatorNullConditional()
    {
        var tokens = Tokenize("?.");
        Assert.Single(tokens);
        Assert.Equal(CSharpTokenType.Operator, tokens[0].TokenType);
        Assert.Equal("?.", tokens[0].Value);
    }

    [Fact]
    public void TestOperatorLambda()
    {
        var tokens = Tokenize("=>");
        Assert.Single(tokens);
        Assert.Equal(CSharpTokenType.Operator, tokens[0].TokenType);
        Assert.Equal("=>", tokens[0].Value);
    }

    [Fact]
    public void TestPunctuationComma()
    {
        var tokens = Tokenize(",");
        Assert.Single(tokens);
        Assert.Equal(CSharpTokenType.Comma, tokens[0].TokenType);
        Assert.Equal(",", tokens[0].Value);
    }

    [Fact]
    public void TestPunctuationDot()
    {
        var tokens = Tokenize(".");
        Assert.Single(tokens);
        Assert.Equal(CSharpTokenType.Dot, tokens[0].TokenType);
        Assert.Equal(".", tokens[0].Value);
    }

    [Fact]
    public void TestPunctuationOpenParenthesis()
    {
        var tokens = Tokenize("(");
        Assert.Single(tokens);
        Assert.Equal(CSharpTokenType.OpenParenthesis, tokens[0].TokenType);
        Assert.Equal("(", tokens[0].Value);
    }

    [Fact]
    public void TestPunctuationCloseParenthesis()
    {
        var tokens = Tokenize(")");
        Assert.Single(tokens);
        Assert.Equal(CSharpTokenType.CloseParenthesis, tokens[0].TokenType);
        Assert.Equal(")", tokens[0].Value);
    }

    [Fact]
    public void TestPunctuationSemicolon()
    {
        var tokens = Tokenize(";");
        Assert.Single(tokens);
        Assert.Equal(CSharpTokenType.SequenceTerminator, tokens[0].TokenType);
        Assert.Equal(";", tokens[0].Value);
    }

    [Fact]
    public void TestLineComment()
    {
        var tokens = Tokenize("// This is a comment\n");
        Assert.Equal(2, tokens.Count);
        Assert.Equal(CSharpTokenType.Comment, tokens[0].TokenType);
        Assert.Equal("// This is a comment", tokens[0].Value);
        Assert.Equal(CSharpTokenType.Whitespace, tokens[1].TokenType);
    }

    [Fact]
    public void TestBlockComment()
    {
        var tokens = Tokenize("/* This is a block comment */");
        Assert.Single(tokens);
        Assert.Equal(CSharpTokenType.Comment, tokens[0].TokenType);
        Assert.Equal("/* This is a block comment */", tokens[0].Value);
    }

    [Fact]
    public void TestMultiLineBlockComment()
    {
        var tokens = Tokenize("/* Line 1\nLine 2\nLine 3 */");
        Assert.Single(tokens);
        Assert.Equal(CSharpTokenType.Comment, tokens[0].TokenType);
        Assert.Equal("/* Line 1\nLine 2\nLine 3 */", tokens[0].Value);
    }

    [Fact]
    public void TestWhitespace()
    {
        var tokens = Tokenize("   \t\n");
        Assert.Single(tokens);
        Assert.Equal(CSharpTokenType.Whitespace, tokens[0].TokenType);
        Assert.Equal("   \t\n", tokens[0].Value);
    }

    [Fact]
    public void TestClassDefinition()
    {
        var tokens = Tokenize("public class MyClass { }");
        Assert.Equal(9, tokens.Count);
        Assert.Equal(CSharpTokenType.Keyword, tokens[0].TokenType);
        Assert.Equal("public", tokens[0].Value);
        Assert.Equal(CSharpTokenType.Keyword, tokens[2].TokenType);
        Assert.Equal("class", tokens[2].Value);
        Assert.Equal(CSharpTokenType.Identifier, tokens[4].TokenType);
        Assert.Equal("MyClass", tokens[4].Value);
        Assert.Equal(CSharpTokenType.Operator, tokens[6].TokenType);
        Assert.Equal("{", tokens[6].Value);
        Assert.Equal(CSharpTokenType.Operator, tokens[8].TokenType);
        Assert.Equal("}", tokens[8].Value);
    }

    [Fact]
    public void TestMethodCall()
    {
        var tokens = Tokenize("Console.WriteLine(\"Hello\");");
        Assert.Equal(7, tokens.Count);
        Assert.Equal(CSharpTokenType.Identifier, tokens[0].TokenType);
        Assert.Equal("Console", tokens[0].Value);
        Assert.Equal(CSharpTokenType.Dot, tokens[1].TokenType);
        Assert.Equal(".", tokens[1].Value);
        Assert.Equal(CSharpTokenType.Identifier, tokens[2].TokenType);
        Assert.Equal("WriteLine", tokens[2].Value);
        Assert.Equal(CSharpTokenType.OpenParenthesis, tokens[3].TokenType);
        Assert.Equal("(", tokens[3].Value);
        Assert.Equal(CSharpTokenType.StringValue, tokens[4].TokenType);
        Assert.Equal("\"Hello\"", tokens[4].Value);
        Assert.Equal(CSharpTokenType.CloseParenthesis, tokens[5].TokenType);
        Assert.Equal(")", tokens[5].Value);
        Assert.Equal(CSharpTokenType.SequenceTerminator, tokens[6].TokenType);
        Assert.Equal(";", tokens[6].Value);
    }

    [Fact]
    public void TestAsyncAwait()
    {
        var tokens = Tokenize("async Task Method() { await Task.Delay(100); }");
        var keywordTokens = tokens.Where(t => t.TokenType == CSharpTokenType.Keyword).ToList();
        Assert.Equal(2, keywordTokens.Count);
        Assert.Equal("async", keywordTokens[0].Value);
        Assert.Equal("await", keywordTokens[1].Value);
    }

    [Fact]
    public void TestGenerics()
    {
        var tokens = Tokenize("List<int>");
        Assert.Equal(4, tokens.Count);
        Assert.Equal(CSharpTokenType.Identifier, tokens[0].TokenType);
        Assert.Equal("List", tokens[0].Value);
        Assert.Equal(CSharpTokenType.LessThan, tokens[1].TokenType);
        Assert.Equal("<", tokens[1].Value);
        Assert.Equal(CSharpTokenType.Keyword, tokens[2].TokenType);
        Assert.Equal("int", tokens[2].Value);
        Assert.Equal(CSharpTokenType.GreaterThan, tokens[3].TokenType);
        Assert.Equal(">", tokens[3].Value);
    }

    [Fact]
    public void TestNestedGenerics()
    {
        var tokens = Tokenize("Dictionary<string, List<int>>");
        Assert.Equal(9, tokens.Count);
        Assert.Equal(CSharpTokenType.Identifier, tokens[0].TokenType);
        Assert.Equal("Dictionary", tokens[0].Value);
        Assert.Equal(CSharpTokenType.LessThan, tokens[1].TokenType);
        Assert.Equal(CSharpTokenType.Keyword, tokens[2].TokenType);
        Assert.Equal("string", tokens[2].Value);
        Assert.Equal(CSharpTokenType.Comma, tokens[3].TokenType);
    }

    [Fact]
    public void TestArithmeticExpression()
    {
        var tokens = Tokenize("x + y * z");
        Assert.Equal(9, tokens.Count);
        Assert.Equal(CSharpTokenType.Identifier, tokens[0].TokenType);
        Assert.Equal("x", tokens[0].Value);
        Assert.Equal(CSharpTokenType.Whitespace, tokens[1].TokenType);
        Assert.Equal(CSharpTokenType.Plus, tokens[2].TokenType);
        Assert.Equal("+", tokens[2].Value);
        Assert.Equal(CSharpTokenType.Whitespace, tokens[3].TokenType);
        Assert.Equal(CSharpTokenType.Identifier, tokens[4].TokenType);
        Assert.Equal("y", tokens[4].Value);
        Assert.Equal(CSharpTokenType.Whitespace, tokens[5].TokenType);
        Assert.Equal(CSharpTokenType.Multiply, tokens[6].TokenType);
        Assert.Equal("*", tokens[6].Value);
        Assert.Equal(CSharpTokenType.Whitespace, tokens[7].TokenType);
        Assert.Equal(CSharpTokenType.Identifier, tokens[8].TokenType);
        Assert.Equal("z", tokens[8].Value);
    }

    [Fact]
    public void TestBooleanExpression()
    {
        var tokens = Tokenize("a && b || c");
        Assert.Equal(9, tokens.Count);
        Assert.Equal(CSharpTokenType.Identifier, tokens[0].TokenType);
        Assert.Equal("a", tokens[0].Value);
        Assert.Equal(CSharpTokenType.Whitespace, tokens[1].TokenType);
        Assert.Equal(CSharpTokenType.And, tokens[2].TokenType);
        Assert.Equal("&&", tokens[2].Value);
        Assert.Equal(CSharpTokenType.Whitespace, tokens[3].TokenType);
        Assert.Equal(CSharpTokenType.Identifier, tokens[4].TokenType);
        Assert.Equal("b", tokens[4].Value);
        Assert.Equal(CSharpTokenType.Whitespace, tokens[5].TokenType);
        Assert.Equal(CSharpTokenType.Or, tokens[6].TokenType);
        Assert.Equal("||", tokens[6].Value);
        Assert.Equal(CSharpTokenType.Whitespace, tokens[7].TokenType);
        Assert.Equal(CSharpTokenType.Identifier, tokens[8].TokenType);
        Assert.Equal("c", tokens[8].Value);
    }

    [Fact]
    public void TestComparisonExpression()
    {
        var tokens = Tokenize("x >= 10 && x <= 20");
        var identifiers = tokens.Where(t => t.TokenType == CSharpTokenType.Identifier).ToList();
        Assert.Equal(2, identifiers.Count);
        var operators = tokens.Where(t => t.TokenType == CSharpTokenType.GreaterThanOrEqual || 
                                          t.TokenType == CSharpTokenType.LessThanOrEqual ||
                                          t.TokenType == CSharpTokenType.And).ToList();
        Assert.Equal(3, operators.Count);
    }

    [Fact]
    public void TestLambdaExpression()
    {
        var tokens = Tokenize("x => x * 2");
        Assert.Equal(9, tokens.Count);
        Assert.Equal(CSharpTokenType.Identifier, tokens[0].TokenType);
        Assert.Equal("x", tokens[0].Value);
        Assert.Equal(CSharpTokenType.Whitespace, tokens[1].TokenType);
        Assert.Equal(CSharpTokenType.Operator, tokens[2].TokenType);
        Assert.Equal("=>", tokens[2].Value);
        Assert.Equal(CSharpTokenType.Whitespace, tokens[3].TokenType);
        Assert.Equal(CSharpTokenType.Identifier, tokens[4].TokenType);
        Assert.Equal("x", tokens[4].Value);
        Assert.Equal(CSharpTokenType.Whitespace, tokens[5].TokenType);
        Assert.Equal(CSharpTokenType.Multiply, tokens[6].TokenType);
        Assert.Equal("*", tokens[6].Value);
        Assert.Equal(CSharpTokenType.Whitespace, tokens[7].TokenType);
        Assert.Equal(CSharpTokenType.Number, tokens[8].TokenType);
        Assert.Equal("2", tokens[8].Value);
    }

    [Fact]
    public void TestNullCoalescingExpression()
    {
        var tokens = Tokenize("value ?? defaultValue");
        Assert.Equal(5, tokens.Count);
        Assert.Equal(CSharpTokenType.Identifier, tokens[0].TokenType);
        Assert.Equal("value", tokens[0].Value);
        Assert.Equal(CSharpTokenType.Whitespace, tokens[1].TokenType);
        Assert.Equal(CSharpTokenType.Operator, tokens[2].TokenType);
        Assert.Equal("??", tokens[2].Value);
        Assert.Equal(CSharpTokenType.Whitespace, tokens[3].TokenType);
        Assert.Equal(CSharpTokenType.Identifier, tokens[4].TokenType);
        Assert.Equal("defaultValue", tokens[4].Value);
    }

    [Fact]
    public void TestNullConditionalExpression()
    {
        var tokens = Tokenize("obj?.Property");
        Assert.Equal(3, tokens.Count);
        Assert.Equal(CSharpTokenType.Identifier, tokens[0].TokenType);
        Assert.Equal("obj", tokens[0].Value);
        Assert.Equal(CSharpTokenType.Operator, tokens[1].TokenType);
        Assert.Equal("?.", tokens[1].Value);
        Assert.Equal(CSharpTokenType.Identifier, tokens[2].TokenType);
        Assert.Equal("Property", tokens[2].Value);
    }

    [Fact]
    public void TestStopDelimiter()
    {
        var tokens = Tokenize("int x = 5; STOP more code", "STOP");
        // Should stop before "STOP" and not include " more code"
        var allText = string.Join("", tokens.Select(t => t.Value));
        Assert.DoesNotContain("STOP", allText);
        Assert.DoesNotContain("more", allText);
    }

    [Fact]
    public void TestStopDelimiterInMiddleOfToken()
    {
        var tokens = Tokenize("int x = END 5;", "END");
        var allText = string.Join("", tokens.Select(t => t.Value));
        Assert.DoesNotContain("END", allText);
        Assert.DoesNotContain("5", allText);
    }

    [Fact]
    public void TestComplexStatement()
    {
        var code = @"public async Task<int> CalculateAsync(string input)
{
    if (input == null)
        throw new ArgumentNullException(nameof(input));
    
    var result = await ProcessAsync(input);
    return result.Value ?? 0;
}";
        var tokens = Tokenize(code);
        
        // Verify we have various token types
        Assert.Contains(tokens, t => t.TokenType == CSharpTokenType.Keyword && t.Value.ToLower() == "public");
        Assert.Contains(tokens, t => t.TokenType == CSharpTokenType.Keyword && t.Value.ToLower() == "async");
        Assert.Contains(tokens, t => t.TokenType == CSharpTokenType.Identifier && t.Value == "Task");
        Assert.Contains(tokens, t => t.TokenType == CSharpTokenType.Identifier && t.Value == "CalculateAsync");
        Assert.Contains(tokens, t => t.TokenType == CSharpTokenType.Keyword && t.Value.ToLower() == "await");
        Assert.Contains(tokens, t => t.TokenType == CSharpTokenType.Keyword && t.Value.ToLower() == "return");
    }

    [Fact]
    public void TestMixedCommentsAndCode()
    {
        var code = @"// Line comment
int x = 5; /* Block comment */ int y = 10;";
        var tokens = Tokenize(code);
        
        var comments = tokens.Where(t => t.TokenType == CSharpTokenType.Comment).ToList();
        Assert.Equal(2, comments.Count);
        Assert.Equal("// Line comment", comments[0].Value);
        Assert.Equal("/* Block comment */", comments[1].Value);
    }

    [Fact]
    public void TestDecimalStartingWithDot()
    {
        var tokens = Tokenize(".5");
        // When starting with a dot followed by a digit, it's treated as a number
        Assert.Single(tokens);
        Assert.Equal(CSharpTokenType.Number, tokens[0].TokenType);
        Assert.Equal(".5", tokens[0].Value);
    }

    [Fact]
    public void TestMethodChaining()
    {
        var tokens = Tokenize("obj.Method1().Method2().Method3()");
        var dots = tokens.Where(t => t.TokenType == CSharpTokenType.Dot).ToList();
        Assert.Equal(3, dots.Count);
        var identifiers = tokens.Where(t => t.TokenType == CSharpTokenType.Identifier).ToList();
        Assert.Equal(4, identifiers.Count);
    }

    [Fact]
    public void TestTernaryOperator()
    {
        var tokens = Tokenize("condition ? trueValue : falseValue");
        var operators = tokens.Where(t => t.TokenType == CSharpTokenType.Operator && 
                                          (t.Value == "?" || t.Value == ":")).ToList();
        Assert.Equal(2, operators.Count);
    }

    [Fact]
    public void TestArrayAccess()
    {
        var tokens = Tokenize("array[0]");
        Assert.Contains(tokens, t => t.TokenType == CSharpTokenType.Operator && t.Value == "[");
        Assert.Contains(tokens, t => t.TokenType == CSharpTokenType.Operator && t.Value == "]");
        Assert.Contains(tokens, t => t.TokenType == CSharpTokenType.Number && t.Value == "0");
    }

    [Fact]
    public void TestAttributeDeclaration()
    {
        var tokens = Tokenize("[Serializable]");
        Assert.Equal(3, tokens.Count);
        Assert.Equal(CSharpTokenType.Operator, tokens[0].TokenType);
        Assert.Equal("[", tokens[0].Value);
        Assert.Equal(CSharpTokenType.Identifier, tokens[1].TokenType);
        Assert.Equal("Serializable", tokens[1].Value);
        Assert.Equal(CSharpTokenType.Operator, tokens[2].TokenType);
        Assert.Equal("]", tokens[2].Value);
    }

    [Fact]
    public void TestIncrementDecrement()
    {
        var tokens = Tokenize("i++; j--;");
        var operators = tokens.Where(t => t.TokenType == CSharpTokenType.Operator && 
                                          (t.Value == "++" || t.Value == "--")).ToList();
        Assert.Equal(2, operators.Count);
    }

    [Fact]
    public void TestCompoundAssignment()
    {
        var tokens = Tokenize("x += 5");
        Assert.Contains(tokens, t => t.TokenType == CSharpTokenType.Operator && t.Value == "+=");
    }

    private static List<CSharpToken> Tokenize(string input) => CSharpTokenizer.Create().Parse(input);

    private static List<CSharpToken> Tokenize(string input, string stopDelimiter)
    {
        var tokens = new List<CSharpToken>();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        using var reader = new StreamReader(stream, Encoding.UTF8);
        CSharpTokenizer.Create().ParseAsync(reader, stopDelimiter, token => tokens.Add(token));
        return tokens;
    }
}
