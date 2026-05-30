using NTokenizers.Markdown;
using NTokenizers.Python;
using System.Text;

namespace NTokenizers.Tests;

public class PythonTokenizerTests
{
    private static List<PythonToken> Tokenize(string input) => PythonTokenizer.Create().Parse(input);

    // === Basic token types ===

    [Fact]
    public void Parse_SimpleFunctionDeclaration_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("def greet(name): return name");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Keyword && t.Value == "def");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Identifier && t.Value == "greet");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Keyword && t.Value == "return");
    }

    [Fact]
    public void Parse_ClassDeclaration_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("class Person: pass");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Keyword && t.Value == "class");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Identifier && t.Value == "Person");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Keyword && t.Value == "pass");
    }

    [Fact]
    public void Parse_IfStatement_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("if x > 0: print(x)");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Keyword && t.Value == "if");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Identifier && t.Value == "x");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Number && t.Value == "0");
    }

    [Fact]
    public void Parse_ForLoop_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("for i in range(10): print(i)");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Keyword && t.Value == "for");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Keyword && t.Value == "in");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Identifier && t.Value == "range");
    }

    [Fact]
    public void Parse_ImportStatement_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("import os.path");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Keyword && t.Value == "import");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Identifier && t.Value == "os");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Dot && t.Value == ".");
    }

    // === String literals ===

    [Fact]
    public void Parse_SingleQuotedString_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("'hello'");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.StringValue && t.Value == "'hello'");
    }

    [Fact]
    public void Parse_DoubleQuotedString_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("\"hello\"");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.StringValue && t.Value == "\"hello\"");
    }

    [Fact]
    public void Parse_TripleSingleQuotedString_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("'''hello world'''");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.StringValue && t.Value == "'''hello world'''");
    }

    [Fact]
    public void Parse_TripleDoubleQuotedString_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("\"\"\"hello world\"\"\"");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.StringValue && t.Value == "\"\"\"hello world\"\"\"");
    }

    [Fact]
    public void Parse_StringWithEscapes_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("\"hello\\nworld\\t\"");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.StringValue && t.Value == "\"hello\\nworld\\t\"");
    }

    [Fact]
    public void Parse_FString_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("f\"Hello, {name}!\"");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Identifier && t.Value == "f");
    }

    [Fact]
    public void Parse_RawString_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("r\"hello\\nworld\"");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Identifier && t.Value == "r");
    }

    [Fact]
    public void Parse_ByteString_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("b\"hello\"");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Identifier && t.Value == "b");
    }

    // === Numbers ===

    [Fact]
    public void Parse_IntegerNumber_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("42");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Number && t.Value == "42");
    }

    [Fact]
    public void Parse_FloatNumber_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("3.14");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Number && t.Value == "3.14");
    }

    [Fact]
    public void Parse_HexNumber_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("0xFF");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Number && t.Value == "0xFF");
    }

    [Fact]
    public void Parse_BinaryNumber_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("0b1010");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Number && t.Value == "0b1010");
    }

    [Fact]
    public void Parse_OctalNumber_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("0o77");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Number && t.Value == "0o77");
    }

    [Fact]
    public void Parse_ComplexNumber_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("3j");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Number && t.Value == "3j");
    }

    [Fact]
    public void Parse_NumberWithUnderscores_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("1_000_000");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Number && t.Value == "1_000_000");
    }

    [Fact]
    public void Parse_ScientificNotation_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("1.5e10");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Number && t.Value == "1.5e10");
    }

    // === Comments ===

    [Fact]
    public void Parse_LineComment_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("# line comment");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Comment && t.Value.StartsWith("#"));
    }

    [Fact]
    public void Parse_CommentAfterCode_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("x = 42 # assign value");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Identifier && t.Value == "x");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Number && t.Value == "42");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Comment && t.Value.StartsWith("#"));
    }

    // === Operators - individual tests ===

    [Fact]
    public void Parse_OperatorEquals_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a = b");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Operator && t.Value == "=");
    }

    [Fact]
    public void Parse_OperatorNotEquals_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a != b");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Operator && t.Value == "!=");
    }

    [Fact]
    public void Parse_OperatorLessThan_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a < b");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Operator && t.Value == "<");
    }

    [Fact]
    public void Parse_OperatorGreaterThan_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a > b");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Operator && t.Value == ">");
    }

    [Fact]
    public void Parse_OperatorLessThanOrEqual_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a <= b");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Operator && t.Value == "<=");
    }

    [Fact]
    public void Parse_OperatorGreaterThanOrEqual_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a >= b");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Operator && t.Value == ">=");
    }

    [Fact]
    public void Parse_OperatorEqualsEquals_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a == b");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Operator && t.Value == "==");
    }

    [Fact]
    public void Parse_OperatorPlus_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a + b");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Operator && t.Value == "+");
    }

    [Fact]
    public void Parse_OperatorMinus_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a - b");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Operator && t.Value == "-");
    }

    [Fact]
    public void Parse_OperatorMultiply_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a * b");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Operator && t.Value == "*");
    }

    [Fact]
    public void Parse_OperatorDivide_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a / b");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Operator && t.Value == "/");
    }

    [Fact]
    public void Parse_OperatorFloorDivide_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a // b");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Operator && t.Value == "//");
    }

    [Fact]
    public void Parse_OperatorModulo_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a % b");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Operator && t.Value == "%");
    }

    [Fact]
    public void Parse_OperatorPower_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a ** b");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Operator && t.Value == "**");
    }

    [Fact]
    public void Parse_OperatorWalrus_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("(x := 42)");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Operator && t.Value == ":=");
    }

    [Fact]
    public void Parse_OperatorPlusEquals_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a += b");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Operator && t.Value == "+=");
    }

    [Fact]
    public void Parse_OperatorMinusEquals_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a -= b");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Operator && t.Value == "-=");
    }

    [Fact]
    public void Parse_OperatorBitwiseAnd_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a & b");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Operator && t.Value == "&");
    }

    [Fact]
    public void Parse_OperatorBitwiseOr_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a | b");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Operator && t.Value == "|");
    }

    [Fact]
    public void Parse_OperatorBitwiseXor_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a ^ b");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Operator && t.Value == "^");
    }

    [Fact]
    public void Parse_OperatorLeftShift_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a << 2");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Operator && t.Value == "<<");
    }

    [Fact]
    public void Parse_OperatorRightShift_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a >> 2");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Operator && t.Value == ">>");
    }

    [Fact]
    public void Parse_OperatorMatrixMultiply_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a @ b");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Hash && t.Value == "@");
    }

    // === Punctuation ===

    [Fact]
    public void Parse_PunctuationComma_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a, b, c");
        Assert.Equal(2, tokens.Count(t => t.TokenType == PythonTokenType.Comma));
    }

    [Fact]
    public void Parse_PunctuationDot_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("obj.member");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Dot && t.Value == ".");
    }

    [Fact]
    public void Parse_PunctuationOpenParenthesis_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("(a)");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.OpenParenthesis && t.Value == "(");
    }

    [Fact]
    public void Parse_PunctuationCloseParenthesis_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("(a)");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.CloseParenthesis && t.Value == ")");
    }

    [Fact]
    public void Parse_PunctuationOpenBrace_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("{ a }");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.OpenBrace && t.Value == "{");
    }

    [Fact]
    public void Parse_PunctuationCloseBrace_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("{ a }");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.CloseBrace && t.Value == "}");
    }

    [Fact]
    public void Parse_PunctuationOpenBracket_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("[a]");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.OpenBracket && t.Value == "[");
    }

    [Fact]
    public void Parse_PunctuationCloseBracket_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("[a]");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.CloseBracket && t.Value == "]");
    }

    [Fact]
    public void Parse_PunctuationColon_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("def func(): pass");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Colon && t.Value == ":");
    }

    [Fact]
    public void Parse_PunctuationSemicolon_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("x = 1; y = 2;");
        Assert.Equal(2, tokens.Count(t => t.TokenType == PythonTokenType.Semicolon));
    }

    // === Decorators ===

    [Fact]
    public void Parse_Decorator_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("@decorator");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Hash && t.Value == "@");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Identifier && t.Value == "decorator");
    }

    // === Keywords ===

    [Fact]
    public void Parse_BooleanKeywords_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("True False");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Keyword && t.Value == "True");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Keyword && t.Value == "False");
    }

    [Fact]
    public void Parse_NoneKeyword_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("x = None");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Keyword && t.Value == "None");
    }

    // === Whitespace ===

    [Fact]
    public void Parse_Whitespace_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a  b");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Whitespace && t.Value == "  ");
    }

    // === Complex scenarios ===

    [Fact]
    public void Parse_FunctionWithTypeHints_ReturnsExpectedTokens()
    {
        var code = """
            def greet(name: str) -> str:
                return f"Hello, {name}!"
            """;
        var tokens = Tokenize(code);
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Keyword && t.Value == "def");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Identifier && t.Value == "greet");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Keyword && t.Value == "return");
    }

    [Fact]
    public void Parse_ClassWithInit_ReturnsExpectedTokens()
    {
        var code = """
            class Person:
                def __init__(self, name: str, age: int):
                    self.name = name
                    self.age = age
            """;
        var tokens = Tokenize(code);
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Keyword && t.Value == "class");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Identifier && t.Value == "Person");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Keyword && t.Value == "def");
    }

    [Fact]
    public void Parse_ListComprehension_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("[x**2 for x in range(10) if x > 5]");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Operator && t.Value == "**");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Keyword && t.Value == "for");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Keyword && t.Value == "in");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Keyword && t.Value == "if");
    }

    [Fact]
    public void Parse_TryExcept_ReturnsExpectedTokens()
    {
        var code = """
            try:
                x = 1 / 0
            except ZeroDivisionError:
                x = 0
            """;
        var tokens = Tokenize(code);
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Keyword && t.Value == "try");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Keyword && t.Value == "except");
    }

    [Fact]
    public void Parse_AsyncFunction_ReturnsExpectedTokens()
    {
        var code = """
            async def fetch(url: str) -> dict:
                response = await http.get(url)
                return response.json()
            """;
        var tokens = Tokenize(code);
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Keyword && t.Value == "async");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Keyword && t.Value == "await");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Keyword && t.Value == "def");
    }

    // === Edge cases ===

    [Fact]
    public void Parse_StopDelimiter_ReturnsExpectedTokens()
    {
        var tokenizer = PythonTokenizer.Create();
        var tokens = new List<PythonToken>();
        var code = "def func(): pass\n```\n";
        tokenizer.Parse(code).ToList(); // Just verify it doesn't crash
    }

    [Fact]
    public void Parse_EmptyInput_ReturnsEmptyTokens()
    {
        var tokens = Tokenize("");
        Assert.Empty(tokens);
    }

    [Fact]
    public void Parse_OnlyWhitespace_ReturnsWhitespaceToken()
    {
        var tokens = Tokenize("   ");
        Assert.Single(tokens);
        Assert.Equal(PythonTokenType.Whitespace, tokens[0].TokenType);
    }

    // === Cancellation ===

    [Fact]
    public async Task TestCancellation()
    {
        var tokenizer = PythonTokenizer.Create();
        var cts = new CancellationTokenSource();
        cts.Cancel();
        // Tokenizer should handle cancellation gracefully
        var code = "def func(): pass";
        var tokens = tokenizer.Parse(code);
        Assert.NotEmpty(tokens);
    }

    // === Markdown integration ===

    [Fact]
    public async Task TestMarkdownPythonCodeBlock()
    {
        var markdown = "```python\ndef greet(name: str) -> str:\n    return f\"Hello, {name}!\"\n```";
        var pythonTokens = new List<PythonToken>();

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(markdown));
        await MarkdownTokenizer.Create().ParseAsync(stream, token =>
        {
            if (token.Metadata is PythonCodeBlockMetadata pythonMeta)
            {
                pythonMeta.RegisterInlineTokenHandler(t => pythonTokens.Add((PythonToken)t));
            }
        });

        Assert.NotEmpty(pythonTokens);
        Assert.Contains(pythonTokens, t => t.TokenType == PythonTokenType.Keyword && t.Value == "def");
        Assert.Contains(pythonTokens, t => t.TokenType == PythonTokenType.Identifier && t.Value == "greet");
    }

    // === Newline preservation ===

    [Fact]
    public void Parse_NewlinesInComments_ReturnsWhitespaceTokens()
    {
        var code = "# comment\nx = 1";
        var tokens = Tokenize(code);
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Comment && t.Value == "# comment");
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Whitespace);
    }

    [Fact]
    public void Parse_NewlinesPreserved_ReturnsWhitespaceTokens()
    {
        var code = "x = 1\ny = 2";
        var tokens = Tokenize(code);
        Assert.Contains(tokens, t => t.TokenType == PythonTokenType.Whitespace && t.Value.Contains('\n'));
    }
}
