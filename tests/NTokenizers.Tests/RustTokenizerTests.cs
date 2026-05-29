using NTokenizers.Markdown;
using NTokenizers.Rust;
using System.Text;

namespace NTokenizers.Tests;

public class RustTokenizerTests
{
    private static List<RustToken> Tokenize(string input) => RustTokenizer.Create().Parse(input);

    // === Basic token types ===

    [Fact]
    public void Parse_SimpleFunctionDeclaration_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("fn main() { let x = 42; }");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Keyword && t.Value == "fn");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Identifier && t.Value == "main");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Keyword && t.Value == "let");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Identifier && t.Value == "x");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Number && t.Value == "42");
    }

    [Fact]
    public void Parse_StructWithLifetime_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("struct Foo<'a> { x: &'a str }");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Keyword && t.Value == "struct");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Identifier && t.Value == "Foo");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Lifetime && t.Value == "'a");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Identifier && t.Value == "x");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Identifier && t.Value == "str");
    }

    [Fact]
    public void Parse_EnumWithVariants_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("enum Option<T> { None, Some(T) }");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Keyword && t.Value == "enum");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Identifier && t.Value == "Option");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Identifier && t.Value == "None");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Identifier && t.Value == "Some");
    }

    [Fact]
    public void Parse_PatternMatching_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("match x { Some(v) => v, None => 0 }");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Keyword && t.Value == "match");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Identifier && t.Value == "Some");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Identifier && t.Value == "None");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.FatArrow && t.Value == "=>");
    }

    [Fact]
    public void Parse_GenericsWithTraitBounds_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("fn foo<T: Trait + 'static>() {}");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Keyword && t.Value == "fn");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Identifier && t.Value == "foo");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Identifier && t.Value == "Trait");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Lifetime && t.Value == "'static");
    }

    // === String and char literals ===

    [Fact]
    public void Parse_StringLiterals_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("\"hello\"");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.StringValue && t.Value == "\"hello\"");
    }

    [Fact]
    public void Parse_StringWithEscapes_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("\"hello\\nworld\\t\"");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.StringValue && t.Value == "\"hello\\nworld\\t\"");
    }

    [Fact]
    public void Parse_CharLiterals_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("'x'");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.CharValue && t.Value == "'x'");
    }

    [Fact]
    public void Parse_CharWithEscapes_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("'\\n' '\\t'");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.CharValue && t.Value == "'\\n'");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.CharValue && t.Value == "'\\t'");
    }

    // === Lifetimes ===

    [Fact]
    public void Parse_Lifetimes_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("'static 'a");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Lifetime && t.Value == "'static");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Lifetime && t.Value == "'a");
    }

    // === Numbers ===

    [Fact]
    public void Parse_IntegerNumber_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("42");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Number && t.Value == "42");
    }

    [Fact]
    public void Parse_HexNumber_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("0xFF");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Number && t.Value == "0xFF");
    }

    [Fact]
    public void Parse_OctalNumber_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("0o77");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Number && t.Value == "0o77");
    }

    [Fact]
    public void Parse_BinaryNumber_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("0b1010");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Number && t.Value == "0b1010");
    }

    [Fact]
    public void Parse_DecimalNumber_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("3.14");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Number && t.Value == "3.14");
    }

    [Fact]
    public void Parse_NumbersWithSuffixes_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("42u32 3.14f64 0xFFusize");
        var numbers = tokens.Where(t => t.TokenType == RustTokenType.Number).ToList();
        Assert.Equal(3, numbers.Count);
    }

    // === Comments ===

    [Fact]
    public void Parse_LineComment_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("// line comment");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Comment && t.Value.StartsWith("//"));
    }

    [Fact]
    public void Parse_BlockComment_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("/* block comment */");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Comment && t.Value.StartsWith("/*"));
    }

    [Fact]
    public void Parse_MultiLineBlockComment_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("/* line 1\nline 2\nline 3 */");
        var comment = tokens.FirstOrDefault(t => t.TokenType == RustTokenType.Comment);
        Assert.NotNull(comment);
        Assert.Contains("line 1", comment.Value);
        Assert.Contains("line 3", comment.Value);
    }

    // === Operators - individual tests ===

    [Fact]
    public void Parse_OperatorEquals_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a = b");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Operator && t.Value == "=");
    }

    [Fact]
    public void Parse_OperatorNotEquals_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a != b");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Operator && t.Value == "!=");
    }

    [Fact]
    public void Parse_OperatorLessThan_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a < b");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Operator && t.Value == "<");
    }

    [Fact]
    public void Parse_OperatorGreaterThan_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a > b");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Operator && t.Value == ">");
    }

    [Fact]
    public void Parse_OperatorLessThanOrEqual_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a <= b");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Operator && t.Value == "<=");
    }

    [Fact]
    public void Parse_OperatorGreaterThanOrEqual_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a >= b");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Operator && t.Value == ">=");
    }

    [Fact]
    public void Parse_OperatorEqualsEquals_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a == b");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Operator && t.Value == "==");
    }

    [Fact]
    public void Parse_OperatorPlus_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a + b");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Operator && t.Value == "+");
    }

    [Fact]
    public void Parse_OperatorMinus_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a - b");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Operator && t.Value == "-");
    }

    [Fact]
    public void Parse_OperatorMultiply_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a * b");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Operator && t.Value == "*");
    }

    [Fact]
    public void Parse_OperatorDivide_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a / b");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Operator && t.Value == "/");
    }

    [Fact]
    public void Parse_OperatorModulo_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a % b");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Operator && t.Value == "%");
    }

    [Fact]
    public void Parse_OperatorAnd_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a && b");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Operator && t.Value == "&&");
    }

    [Fact]
    public void Parse_OperatorOr_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a || b");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Operator && t.Value == "||");
    }

    [Fact]
    public void Parse_OperatorNot_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("!a");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Operator && t.Value == "!");
    }

    [Fact]
    public void Parse_OperatorIncrement_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a++");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Operator && t.Value == "++");
    }

    [Fact]
    public void Parse_OperatorDecrement_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a--");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Operator && t.Value == "--");
    }

    [Fact]
    public void Parse_OperatorBitwiseAnd_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a & b");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Operator && t.Value == "&");
    }

    [Fact]
    public void Parse_OperatorBitwiseOr_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a | b");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Operator && t.Value == "|");
    }

    [Fact]
    public void Parse_OperatorBitwiseXor_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a ^ b");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Operator && t.Value == "^");
    }

    [Fact]
    public void Parse_OperatorLeftShift_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a << 2");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Operator && t.Value == "<<");
    }

    [Fact]
    public void Parse_OperatorRightShift_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a >> 2");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Operator && t.Value == ">>");
    }

    [Fact]
    public void Parse_OperatorFatArrow_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a => b");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.FatArrow && t.Value == "=>");
    }

    // === Punctuation ===

    [Fact]
    public void Parse_PunctuationComma_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a, b, c");
        Assert.Equal(2, tokens.Count(t => t.TokenType == RustTokenType.Comma));
    }

    [Fact]
    public void Parse_PunctuationDot_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("obj.member");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Dot && t.Value == ".");
    }

    [Fact]
    public void Parse_PunctuationOpenParenthesis_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("(a)");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.OpenParenthesis && t.Value == "(");
    }

    [Fact]
    public void Parse_PunctuationCloseParenthesis_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("(a)");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.CloseParenthesis && t.Value == ")");
    }

    [Fact]
    public void Parse_PunctuationOpenBrace_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("{ a }");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.OpenBrace && t.Value == "{");
    }

    [Fact]
    public void Parse_PunctuationCloseBrace_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("{ a }");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.CloseBrace && t.Value == "}");
    }

    [Fact]
    public void Parse_PunctuationOpenBracket_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("[a]");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.OpenBracket && t.Value == "[");
    }

    [Fact]
    public void Parse_PunctuationCloseBracket_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("[a]");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.CloseBracket && t.Value == "]");
    }

    [Fact]
    public void Parse_PunctuationSemicolon_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("let x = 1; let y = 2;");
        Assert.Equal(2, tokens.Count(t => t.TokenType == RustTokenType.SequenceTerminator));
    }

    [Fact]
    public void Parse_PunctuationDoubleColon_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("std::vec::Vec");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.DoubleColon && t.Value == "::");
    }

    // === Special tokens ===

    [Fact]
    public void Parse_Boolean_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("true false");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Boolean && t.Value == "true");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Boolean && t.Value == "false");
    }

    [Fact]
    public void Parse_Null_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("None Some(null)");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Identifier && t.Value == "None");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Identifier && t.Value == "Some");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Identifier && t.Value == "null");
    }

    // === Whitespace ===

    [Fact]
    public void Parse_Whitespace_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a  b");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Whitespace && t.Value == "  ");
    }

    // === Complex scenarios ===

    [Fact]
    public void Parse_MacroInvocations_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("println!(\"hello\"); vec![1, 2, 3]");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Identifier && t.Value == "println");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Identifier && t.Value == "vec");
    }

    [Fact]
    public void Parse_MethodChaining_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("obj.method1().method2().method3()");
        Assert.Equal(3, tokens.Count(t => t.TokenType == RustTokenType.Dot && t.Value == "."));
    }

    [Fact]
    public void Parse_ArrayAccess_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("arr[i] = arr[j][k]");
        var brackets = tokens.Where(t => t.TokenType == RustTokenType.OpenBracket || t.TokenType == RustTokenType.CloseBracket).ToList();
        Assert.Equal(6, brackets.Count);
    }

    [Fact]
    public void Parse_ComplexAsyncFunction_ReturnsExpectedTokens()
    {
        var code = """
            async fn fetch<T: Send + 'static>(url: &str) -> Result<T, Error> {
                match reqwest::get(url).await? {
                    Ok(resp) => Ok(resp),
                    Err(e) => Err(e),
                }
            }
            """;
        var tokens = Tokenize(code);
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Keyword && t.Value == "async");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Keyword && t.Value == "fn");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Identifier && t.Value == "fetch");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Keyword && t.Value == "match");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.FatArrow && t.Value == "=>");
    }

    [Fact]
    public void Parse_StructImplementation_ReturnsExpectedTokens()
    {
        var code = """
            impl<T> Foo<T> {
                fn new(x: T) -> Self {
                    Self { x }
                }
                fn get(&self) -> &T {
                    &self.x
                }
            }
            """;
        var tokens = Tokenize(code);
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Keyword && t.Value == "impl");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Identifier && t.Value == "Foo");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Keyword && t.Value == "fn");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Identifier && t.Value == "new");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Keyword && t.Value == "Self");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Keyword && t.Value == "self");
    }

    [Fact]
    public void Parse_MixedCommentsAndCode_ReturnsExpectedTokens()
    {
        var code = """
            // Initialize
            let x = 0; /* start value */
            for i in 0..10 {
                x += i; // accumulate
            }
            """;
        var tokens = Tokenize(code);
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Keyword && t.Value == "let");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Keyword && t.Value == "for");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Operator && t.Value == "+=");
    }

    // === Edge cases ===

    [Fact]
    public void Parse_DecimalStartingWithDot_ReturnsExpectedTokens()
    {
        var tokens = Tokenize(".5");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Number && t.Value == ".5");
    }

    [Fact]
    public void Parse_StopDelimiter_ReturnsExpectedTokens()
    {
        var code = """
            impl<T> Foo<T> {
                fn new(x: T) -> Self {
                    Self { x }
                }
            }
            """;
        var tokens = Tokenize(code);
        Assert.NotEmpty(tokens);
    }

    [Fact]
    public void Parse_StopDelimiterInMiddleOfToken_ReturnsExpectedTokens()
    {
        var code = """
            std::vec::Vec<std::collections::HashMap<String, i32>>;
            """;
        var tokens = Tokenize(code);
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Identifier && t.Value == "std");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Identifier && t.Value == "vec");
    }

    [Fact]
    public void Parse_ComplexStatement_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("if x > 0 && y < 10 || z == 42 { foo(); } else { bar(); }");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Keyword && t.Value == "if");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Keyword && t.Value == "else");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Operator && t.Value == "&&");
        Assert.Contains(tokens, t => t.TokenType == RustTokenType.Operator && t.Value == "||");
    }

    // === Cancellation ===

    [Fact]
    public async Task TestCancellation()
    {
        // Tokenizer processes synchronously - cancellation is not supported yet
        var code = "let x = 42;";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(code));
        var tokens = new List<RustToken>();
        await RustTokenizer.Create().ParseAsync(stream, t => tokens.Add((RustToken)t));
        Assert.NotEmpty(tokens);
    }

    // === Markdown integration ===

    [Fact]
    public async Task TestMarkdownRustCodeBlock()
    {
        var markdown = """
            # Rust Example
            
            ```rust
            let x = 42;
            ```
            """;

        var rustTokens = new List<RustToken>();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(markdown));
        await MarkdownTokenizer.Create().ParseAsync(stream, token =>
        {
            if (token.Metadata is RustCodeBlockMetadata rustMeta)
            {
                rustMeta.RegisterInlineTokenHandler(t => rustTokens.Add((RustToken)t));
            }
        });

        Assert.NotEmpty(rustTokens);
        Assert.Contains(rustTokens, t => t.TokenType == RustTokenType.Keyword && t.Value == "let");
        Assert.Contains(rustTokens, t => t.TokenType == RustTokenType.Identifier && t.Value == "x");
        Assert.Contains(rustTokens, t => t.TokenType == RustTokenType.Number && t.Value == "42");
    }
}
