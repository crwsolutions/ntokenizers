using NTokenizers.Go;
using NTokenizers.Markdown;
using System.Text;

namespace NTokenizers.Tests;

public class GoTokenizerTests
{
    private static List<GoToken> Tokenize(string input) => GoTokenizer.Create().Parse(input);

    // === Basic token types ===

    [Fact]
    public void Parse_SimpleFunctionDeclaration_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("func main() { x := 42 }");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Keyword && t.Value == "func");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Identifier && t.Value == "main");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Identifier && t.Value == "x");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Number && t.Value == "42");
    }

    [Fact]
    public void Parse_StructDeclaration_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("type Person struct { Name string; Age int }");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Keyword && t.Value == "type");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Identifier && t.Value == "Person");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Keyword && t.Value == "struct");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Identifier && t.Value == "Name");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Identifier && t.Value == "Age");
    }

    [Fact]
    public void Parse_InterfaceDeclaration_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("type Reader interface { Read(p []byte) (n int, err error) }");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Keyword && t.Value == "type");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Identifier && t.Value == "Reader");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Keyword && t.Value == "interface");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Identifier && t.Value == "Read");
    }

    [Fact]
    public void Parse_SelectStatement_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("select { case <-ch1: fmt.Println(\"received\") default: fmt.Println(\"none\") }");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Keyword && t.Value == "select");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Keyword && t.Value == "case");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Keyword && t.Value == "default");
    }

    [Fact]
    public void Parse_DeferStatement_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("defer file.Close()");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Keyword && t.Value == "defer");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Identifier && t.Value == "file");
    }

    // === String and char literals ===

    [Fact]
    public void Parse_StringLiterals_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("\"hello\"");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.StringValue && t.Value == "\"hello\"");
    }

    [Fact]
    public void Parse_RawStringLiterals_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("`hello world`");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.StringValue && t.Value == "`hello world`");
    }

    [Fact]
    public void Parse_StringWithEscapes_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("\"hello\\nworld\\t\"");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.StringValue && t.Value == "\"hello\\nworld\\t\"");
    }

    [Fact]
    public void Parse_CharLiterals_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("'x'");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.CharValue && t.Value == "'x'");
    }

    [Fact]
    public void Parse_CharWithEscapes_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("'\\n' '\\t'");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.CharValue && t.Value == "'\\n'");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.CharValue && t.Value == "'\\t'");
    }

    // === Numbers ===

    [Fact]
    public void Parse_IntegerNumber_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("42");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Number && t.Value == "42");
    }

    [Fact]
    public void Parse_HexNumber_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("0xFF");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Number && t.Value == "0xFF");
    }

    [Fact]
    public void Parse_OctalNumber_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("077");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Number && t.Value == "077");
    }

    [Fact]
    public void Parse_BinaryNumber_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("0b1010");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Number && t.Value == "0b1010");
    }

    [Fact]
    public void Parse_DecimalNumber_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("3.14");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Number && t.Value == "3.14");
    }

    [Fact]
    public void Parse_NumbersWithSuffixes_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("42i 3.14i");
        var numbers = tokens.Where(t => t.TokenType == GoTokenType.Number).ToList();
        Assert.Equal(2, numbers.Count);
    }

    // === Comments ===

    [Fact]
    public void Parse_LineComment_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("// line comment");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Comment && t.Value.StartsWith("//"));
    }

    [Fact]
    public void Parse_BlockComment_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("/* block comment */");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Comment && t.Value.StartsWith("/*"));
    }

    [Fact]
    public void Parse_MultiLineBlockComment_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("/* line 1\nline 2\nline 3 */");
        var comment = tokens.FirstOrDefault(t => t.TokenType == GoTokenType.Comment);
        Assert.NotNull(comment);
        Assert.Contains("line 1", comment.Value);
        Assert.Contains("line 3", comment.Value);
    }

    // === Operators - individual tests ===

    [Fact]
    public void Parse_OperatorEquals_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a = b");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Operator && t.Value == "=");
    }

    [Fact]
    public void Parse_OperatorNotEquals_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a != b");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Operator && t.Value == "!=");
    }

    [Fact]
    public void Parse_OperatorLessThan_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a < b");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Operator && t.Value == "<");
    }

    [Fact]
    public void Parse_OperatorGreaterThan_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a > b");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Operator && t.Value == ">");
    }

    [Fact]
    public void Parse_OperatorLessThanOrEqual_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a <= b");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Operator && t.Value == "<=");
    }

    [Fact]
    public void Parse_OperatorGreaterThanOrEqual_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a >= b");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Operator && t.Value == ">=");
    }

    [Fact]
    public void Parse_OperatorEqualsEquals_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a == b");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Operator && t.Value == "==");
    }

    [Fact]
    public void Parse_OperatorPlus_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a + b");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Operator && t.Value == "+");
    }

    [Fact]
    public void Parse_OperatorMinus_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a - b");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Operator && t.Value == "-");
    }

    [Fact]
    public void Parse_OperatorMultiply_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a * b");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Operator && t.Value == "*");
    }

    [Fact]
    public void Parse_OperatorDivide_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a / b");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Operator && t.Value == "/");
    }

    [Fact]
    public void Parse_OperatorModulo_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a % b");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Operator && t.Value == "%");
    }

    [Fact]
    public void Parse_OperatorAnd_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a && b");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Operator && t.Value == "&&");
    }

    [Fact]
    public void Parse_OperatorOr_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a || b");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Operator && t.Value == "||");
    }

    [Fact]
    public void Parse_OperatorNot_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("!a");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Operator && t.Value == "!");
    }

    [Fact]
    public void Parse_OperatorIncrement_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a++");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Operator && t.Value == "++");
    }

    [Fact]
    public void Parse_OperatorDecrement_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a--");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Operator && t.Value == "--");
    }

    [Fact]
    public void Parse_OperatorBitwiseAnd_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a & b");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Operator && t.Value == "&");
    }

    [Fact]
    public void Parse_OperatorBitwiseOr_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a | b");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Operator && t.Value == "|");
    }

    [Fact]
    public void Parse_OperatorBitwiseXor_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a ^ b");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Operator && t.Value == "^");
    }

    [Fact]
    public void Parse_OperatorLeftShift_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a << 2");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Operator && t.Value == "<<");
    }

    [Fact]
    public void Parse_OperatorRightShift_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a >> 2");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Operator && t.Value == ">>");
    }

    // === Punctuation ===

    [Fact]
    public void Parse_PunctuationComma_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a, b, c");
        Assert.Equal(2, tokens.Count(t => t.TokenType == GoTokenType.Comma));
    }

    [Fact]
    public void Parse_PunctuationDot_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("obj.member");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Dot && t.Value == ".");
    }

    [Fact]
    public void Parse_PunctuationOpenParenthesis_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("(a)");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.OpenParenthesis && t.Value == "(");
    }

    [Fact]
    public void Parse_PunctuationCloseParenthesis_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("(a)");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.CloseParenthesis && t.Value == ")");
    }

    [Fact]
    public void Parse_PunctuationOpenBrace_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("{ a }");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.OpenBrace && t.Value == "{");
    }

    [Fact]
    public void Parse_PunctuationCloseBrace_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("{ a }");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.CloseBrace && t.Value == "}");
    }

    [Fact]
    public void Parse_PunctuationOpenBracket_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("[a]");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.OpenBracket && t.Value == "[");
    }

    [Fact]
    public void Parse_PunctuationCloseBracket_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("[a]");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.CloseBracket && t.Value == "]");
    }

    [Fact]
    public void Parse_PunctuationSemicolon_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("x := 1; y := 2;");
        Assert.Equal(2, tokens.Count(t => t.TokenType == GoTokenType.SequenceTerminator));
    }

    // === Special tokens ===

    [Fact]
    public void Parse_Boolean_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("true false");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Boolean && t.Value == "true");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Boolean && t.Value == "false");
    }

    [Fact]
    public void Parse_Nil_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("var x *int = nil");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Keyword && t.Value == "var");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Null && t.Value == "nil");
    }

    // === Whitespace ===

    [Fact]
    public void Parse_Whitespace_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a  b");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Whitespace && t.Value == "  ");
    }

    // === Complex scenarios ===

    [Fact]
    public void Parse_MapDeclaration_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("m := make(map[string]int)");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Identifier && t.Value == "m");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Identifier && t.Value == "make");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Keyword && t.Value == "map");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Identifier && t.Value == "string");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Identifier && t.Value == "int");
    }

    [Fact]
    public void Parse_ChannelDeclaration_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("ch := make(chan int)");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Identifier && t.Value == "ch");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Identifier && t.Value == "make");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Keyword && t.Value == "chan");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Identifier && t.Value == "int");
    }

    [Fact]
    public void Parse_MethodChaining_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("obj.Method1().Method2().Method3()");
        Assert.Equal(3, tokens.Count(t => t.TokenType == GoTokenType.Dot && t.Value == "."));
    }

    [Fact]
    public void Parse_ComplexFunction_ReturnsExpectedTokens()
    {
        var code = """
            func fibonacci(n int) []int {
                fibs := make([]int, n)
                for i := 2; i < n; i++ {
                    fibs[i] = fibs[i-1] + fibs[i-2]
                }
                return fibs
            }
            """;
        var tokens = Tokenize(code);
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Keyword && t.Value == "func");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Identifier && t.Value == "fibonacci");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Keyword && t.Value == "for");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Keyword && t.Value == "return");
    }

    [Fact]
    public void Parse_GoRoutines_ReturnsExpectedTokens()
    {
        var code = """
            func worker(ch chan int) {
                for v := range ch {
                    go process(v)
                }
            }
            """;
        var tokens = Tokenize(code);
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Keyword && t.Value == "func");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Keyword && t.Value == "chan");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Keyword && t.Value == "for");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Keyword && t.Value == "range");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Keyword && t.Value == "go");
    }

    [Fact]
    public void Parse_MixedCommentsAndCode_ReturnsExpectedTokens()
    {
        var code = """
            // Initialize
            x := 0 // start value
            for i := 0; i < 10; i++ {
                x += i // accumulate
            }
            """;
        var tokens = Tokenize(code);
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Keyword && t.Value == "for");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Operator && t.Value == "+=");
    }

    // === Edge cases ===

    [Fact]
    public void Parse_DecimalStartingWithDot_ReturnsExpectedTokens()
    {
        var tokens = Tokenize(".5");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Number && t.Value == ".5");
    }

    [Fact]
    public void Parse_StopDelimiter_ReturnsExpectedTokens()
    {
        var code = """
            func fibonacci(n int) []int {
                fibs := make([]int, n)
                for i := 2; i < n; i++ {
                    fibs[i] = fibs[i-1] + fibs[i-2]
                }
                return fibs
            }
            """;
        var tokens = Tokenize(code);
        Assert.NotEmpty(tokens);
    }

    [Fact]
    public void Parse_StopDelimiterInMiddleOfToken_ReturnsExpectedTokens()
    {
        var code = """
            var m map[string][]int
            """;
        var tokens = Tokenize(code);
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Keyword && t.Value == "var");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Identifier && t.Value == "m");
    }

    [Fact]
    public void Parse_ComplexStatement_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("if x > 0 && y < 10 || z == 42 { foo() } else { bar() }");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Keyword && t.Value == "if");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Keyword && t.Value == "else");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Operator && t.Value == "&&");
        Assert.Contains(tokens, t => t.TokenType == GoTokenType.Operator && t.Value == "||");
    }

    // === Cancellation ===

    [Fact]
    public async Task TestCancellation()
    {
        // Tokenizer processes synchronously - cancellation is not supported yet
        var code = "x := 42";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(code));
        var tokens = new List<GoToken>();
        await GoTokenizer.Create().ParseAsync(stream, t => tokens.Add((GoToken)t));
        Assert.NotEmpty(tokens);
    }

    // === Markdown integration ===

    [Fact]
    public async Task TestMarkdownGoCodeBlock()
    {
        var markdown = """
            # Go Example
            
            ```go
            x := 42
            ```
            """;

        var goTokens = new List<GoToken>();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(markdown));
        await MarkdownTokenizer.Create().ParseAsync(stream, token =>
        {
            if (token.Metadata is GoCodeBlockMetadata goMeta)
            {
                goMeta.RegisterInlineTokenHandler(t => goTokens.Add((GoToken)t));
            }
        });

        Assert.NotEmpty(goTokens);
        Assert.Contains(goTokens, t => t.TokenType == GoTokenType.Identifier && t.Value == "x");
        Assert.Contains(goTokens, t => t.TokenType == GoTokenType.Number && t.Value == "42");
    }
}
