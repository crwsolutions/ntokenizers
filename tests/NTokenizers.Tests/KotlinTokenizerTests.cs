using NTokenizers.Kotlin;
using NTokenizers.Markdown;
using System.Text;

namespace NTokenizers.Tests;

public class KotlinTokenizerTests
{
    private static List<KotlinToken> Tokenize(string input) => KotlinTokenizer.Create().Parse(input);

    // === Basic token types ===

    [Fact]
    public void Parse_SimpleClassDeclaration_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("class Foo { val x: Int = 42 }");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Keyword && t.Value == "class");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Identifier && t.Value == "Foo");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Keyword && t.Value == "val");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Identifier && t.Value == "x");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Identifier && t.Value == "Int");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Number && t.Value == "42");
    }

    [Fact]
    public void Parse_DataClassWithProperties_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("data class Person(val name: String, val age: Int)");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Keyword && t.Value == "data");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Keyword && t.Value == "class");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Identifier && t.Value == "Person");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Keyword && t.Value == "val");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Identifier && t.Value == "name");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Identifier && t.Value == "age");
    }

    [Fact]
    public void Parse_FunctionDeclaration_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("fun foo(x: Int, y: String): Boolean { return true }");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Keyword && t.Value == "fun");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Identifier && t.Value == "foo");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Keyword && t.Value == "return");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Boolean && t.Value == "true");
    }

    [Fact]
    public void Parse_WhenExpression_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("when (x) { 1 -> \"one\" else -> \"other\" }");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Keyword && t.Value == "when");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Identifier && t.Value == "x");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Keyword && t.Value == "else");
    }

    [Fact]
    public void Parse_NullableTypes_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("val s: String? = null");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Keyword && t.Value == "val");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Identifier && t.Value == "s");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Identifier && t.Value == "String");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.QuestionMark && t.Value == "?");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Null && t.Value == "null");
    }

    // === String and char literals ===

    [Fact]
    public void Parse_StringLiterals_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("\"hello\"");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.StringValue && t.Value == "\"hello\"");
    }

    [Fact]
    public void Parse_StringWithEscapes_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("\"hello\\nworld\\t\"");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.StringValue && t.Value == "\"hello\\nworld\\t\"");
    }

    [Fact]
    public void Parse_CharLiterals_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("'x'");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.CharValue && t.Value == "'x'");
    }

    [Fact]
    public void Parse_CharWithEscapes_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("'\\n' '\\t'");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.CharValue && t.Value == "'\\n'");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.CharValue && t.Value == "'\\t'");
    }

    // === Numbers ===

    [Fact]
    public void Parse_IntegerNumber_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("42");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Number && t.Value == "42");
    }

    [Fact]
    public void Parse_HexNumber_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("0xFF");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Number && t.Value == "0xFF");
    }

    [Fact]
    public void Parse_BinaryNumber_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("0b1010");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Number && t.Value == "0b1010");
    }

    [Fact]
    public void Parse_DecimalNumber_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("3.14");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Number && t.Value == "3.14");
    }

    [Fact]
    public void Parse_NumbersWithUnderscores_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("1_000_000");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Number && t.Value == "1_000_000");
    }

    // === Comments ===

    [Fact]
    public void Parse_LineComment_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("// line comment");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Comment && t.Value.StartsWith("//"));
    }

    [Fact]
    public void Parse_BlockComment_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("/* block comment */");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Comment && t.Value.StartsWith("/*"));
    }

    [Fact]
    public void Parse_MultiLineBlockComment_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("/* line 1\nline 2\nline 3 */");
        var comment = tokens.FirstOrDefault(t => t.TokenType == KotlinTokenType.Comment);
        Assert.NotNull(comment);
        Assert.Contains("line 1", comment.Value);
        Assert.Contains("line 3", comment.Value);
    }

    // === Operators - individual tests ===

    [Fact]
    public void Parse_OperatorEquals_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a = b");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Operator && t.Value == "=");
    }

    [Fact]
    public void Parse_OperatorNotEquals_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a != b");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Operator && t.Value == "!=");
    }

    [Fact]
    public void Parse_OperatorLessThan_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a < b");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Operator && t.Value == "<");
    }

    [Fact]
    public void Parse_OperatorGreaterThan_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a > b");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Operator && t.Value == ">");
    }

    [Fact]
    public void Parse_OperatorLessThanOrEqual_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a <= b");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Operator && t.Value == "<=");
    }

    [Fact]
    public void Parse_OperatorGreaterThanOrEqual_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a >= b");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Operator && t.Value == ">=");
    }

    [Fact]
    public void Parse_OperatorEqualsEquals_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a == b");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Operator && t.Value == "==");
    }

    [Fact]
    public void Parse_OperatorPlus_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a + b");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Operator && t.Value == "+");
    }

    [Fact]
    public void Parse_OperatorMinus_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a - b");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Operator && t.Value == "-");
    }

    [Fact]
    public void Parse_OperatorMultiply_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a * b");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Operator && t.Value == "*");
    }

    [Fact]
    public void Parse_OperatorDivide_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a / b");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Operator && t.Value == "/");
    }

    [Fact]
    public void Parse_OperatorModulo_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a % b");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Operator && t.Value == "%");
    }

    [Fact]
    public void Parse_OperatorAnd_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a && b");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Operator && t.Value == "&&");
    }

    [Fact]
    public void Parse_OperatorOr_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a || b");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Operator && t.Value == "||");
    }

    [Fact]
    public void Parse_OperatorNot_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("!a");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Operator && t.Value == "!");
    }

    [Fact]
    public void Parse_OperatorIncrement_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a++");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Operator && t.Value == "++");
    }

    [Fact]
    public void Parse_OperatorDecrement_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a--");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Operator && t.Value == "--");
    }

    [Fact]
    public void Parse_OperatorBitwiseAnd_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a and b");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Identifier && t.Value == "and");
    }

    [Fact]
    public void Parse_OperatorBitwiseOr_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a or b");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Identifier && t.Value == "or");
    }

    [Fact]
    public void Parse_OperatorLeftShift_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a shl 2");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Identifier && t.Value == "shl");
    }

    [Fact]
    public void Parse_OperatorRightShift_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a shr 2");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Identifier && t.Value == "shr");
    }

    // === Punctuation ===

    [Fact]
    public void Parse_PunctuationComma_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a, b, c");
        Assert.Equal(2, tokens.Count(t => t.TokenType == KotlinTokenType.Comma));
    }

    [Fact]
    public void Parse_PunctuationDot_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("obj.member");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Dot && t.Value == ".");
    }

    [Fact]
    public void Parse_PunctuationOpenParenthesis_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("(a)");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.OpenParenthesis && t.Value == "(");
    }

    [Fact]
    public void Parse_PunctuationCloseParenthesis_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("(a)");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.CloseParenthesis && t.Value == ")");
    }

    [Fact]
    public void Parse_PunctuationOpenBrace_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("{ a }");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.OpenBrace && t.Value == "{");
    }

    [Fact]
    public void Parse_PunctuationCloseBrace_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("{ a }");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.CloseBrace && t.Value == "}");
    }

    [Fact]
    public void Parse_PunctuationOpenBracket_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("[a]");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.OpenBracket && t.Value == "[");
    }

    [Fact]
    public void Parse_PunctuationCloseBracket_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("[a]");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.CloseBracket && t.Value == "]");
    }

    [Fact]
    public void Parse_PunctuationSemicolon_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("val x = 1; val y = 2;");
        Assert.Equal(2, tokens.Count(t => t.TokenType == KotlinTokenType.SequenceTerminator));
    }

    [Fact]
    public void Parse_PunctuationDoubleColon_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("Foo::bar");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.DoubleColon && t.Value == "::");
    }

    // === Special tokens ===

    [Fact]
    public void Parse_Boolean_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("true false");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Boolean && t.Value == "true");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Boolean && t.Value == "false");
    }

    [Fact]
    public void Parse_Null_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("null");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Null && t.Value == "null");
    }

    // === Whitespace ===

    [Fact]
    public void Parse_Whitespace_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a  b");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Whitespace && t.Value == "  ");
    }

    // === Complex scenarios ===

    [Fact]
    public void Parse_LambdaExpression_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("{ x, y -> x + y }");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.OpenBrace && t.Value == "{");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Identifier && t.Value == "x");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Identifier && t.Value == "y");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Operator && t.Value == "+");
    }

    [Fact]
    public void Parse_MethodChaining_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("obj.method1().method2().method3()");
        Assert.Equal(3, tokens.Count(t => t.TokenType == KotlinTokenType.Dot && t.Value == "."));
    }

    [Fact]
    public void Parse_ComplexDataClass_ReturnsExpectedTokens()
    {
        var code = """
            data class Config(
                val host: String,
                val port: Int,
                val enabled: Boolean = true
            ) {
                companion object {
                    val DEFAULT = Config("localhost", 8080, true)
                }
            }
            """;
        var tokens = Tokenize(code);
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Keyword && t.Value == "data");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Keyword && t.Value == "class");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Identifier && t.Value == "Config");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Keyword && t.Value == "companion");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Keyword && t.Value == "object");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Keyword && t.Value == "val");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Boolean && t.Value == "true");
    }

    [Fact]
    public void Parse_ComplexWhenWithLambda_ReturnsExpectedTokens()
    {
        var code = """
            when (value) {
                in 1..10 -> println("x is in the range")
                in validNumbers -> println("x is valid")
                else -> println("none")
            }
            """;
        var tokens = Tokenize(code);
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Keyword && t.Value == "when");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Keyword && t.Value == "in");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Identifier && t.Value == "value");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Keyword && t.Value == "else");
    }

    [Fact]
    public void Parse_MixedCommentsAndCode_ReturnsExpectedTokens()
    {
        var code = """
            // Initialize
            val x = 0 /* start value */
            for (i in 0..10) {
                x += i // accumulate
            }
            """;
        var tokens = Tokenize(code);
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Keyword && t.Value == "val");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Keyword && t.Value == "for");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Operator && t.Value == "+=");
    }

    // === Edge cases ===

    [Fact]
    public void Parse_DecimalStartingWithDot_ReturnsExpectedTokens()
    {
        var tokens = Tokenize(".5");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Number && t.Value == ".5");
    }

    [Fact]
    public void Parse_StopDelimiter_ReturnsExpectedTokens()
    {
        var code = """
            data class Config(
                val host: String,
                val port: Int
            )
            """;
        var tokens = Tokenize(code);
        Assert.NotEmpty(tokens);
    }

    [Fact]
    public void Parse_StopDelimiterInMiddleOfToken_ReturnsExpectedTokens()
    {
        var code = """
            val map: Map<String, List<Int>> = emptyMap()
            """;
        var tokens = Tokenize(code);
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Keyword && t.Value == "val");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Identifier && t.Value == "map");
    }

    [Fact]
    public void Parse_ComplexStatement_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("if (x > 0 && y < 10 || z == 42) foo() else bar()");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Keyword && t.Value == "if");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Keyword && t.Value == "else");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Operator && t.Value == "&&");
        Assert.Contains(tokens, t => t.TokenType == KotlinTokenType.Operator && t.Value == "||");
    }

    // === Cancellation ===

    [Fact]
    public async Task TestCancellation()
    {
        // Tokenizer processes synchronously - cancellation is not supported yet
        var code = "val x = 42";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(code));
        var tokens = new List<KotlinToken>();
        await KotlinTokenizer.Create().ParseAsync(stream, t => tokens.Add((KotlinToken)t));
        Assert.NotEmpty(tokens);
    }

    // === Markdown integration ===

    [Fact]
    public async Task TestMarkdownKotlinCodeBlock()
    {
        var markdown = """
            # Kotlin Example
            
            ```kotlin
            val x = 42
            ```
            """;

        var kotlinTokens = new List<KotlinToken>();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(markdown));
        await MarkdownTokenizer.Create().ParseAsync(stream, token =>
        {
            if (token.Metadata is KotlinCodeBlockMetadata kotlinMeta)
            {
                kotlinMeta.RegisterInlineTokenHandler(t => kotlinTokens.Add((KotlinToken)t));
            }
        });

        Assert.NotEmpty(kotlinTokens);
        Assert.Contains(kotlinTokens, t => t.TokenType == KotlinTokenType.Keyword && t.Value == "val");
        Assert.Contains(kotlinTokens, t => t.TokenType == KotlinTokenType.Identifier && t.Value == "x");
        Assert.Contains(kotlinTokens, t => t.TokenType == KotlinTokenType.Number && t.Value == "42");
    }
}
