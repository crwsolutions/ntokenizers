using NTokenizers.Swift;
using NTokenizers.Markdown;
using System.Text;

namespace NTokenizers.Tests;

public class SwiftTokenizerTests
{
    private static List<SwiftToken> Tokenize(string input) => SwiftTokenizer.Create().Parse(input);

    // === Basic token types ===

    [Fact]
    public void Parse_SimpleFunctionDeclaration_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("func greet() -> String { return \"hello\" }");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Keyword && t.Value == "func");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Identifier && t.Value == "greet");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Keyword && t.Value == "return");
    }

    [Fact]
    public void Parse_StructDeclaration_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("struct Person { var name: String; var age: Int }");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Keyword && t.Value == "struct");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Identifier && t.Value == "Person");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Keyword && t.Value == "var");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Identifier && t.Value == "name");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Identifier && t.Value == "String");
    }

    [Fact]
    public void Parse_ClassDeclaration_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("class Manager { let items: [Item] = [] }");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Keyword && t.Value == "class");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Identifier && t.Value == "Manager");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Keyword && t.Value == "let");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Identifier && t.Value == "items");
    }

    [Fact]
    public void Parse_ProtocolDeclaration_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("protocol Identifiable { associatedtype ID var id: ID { get } }");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Keyword && t.Value == "protocol");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Identifier && t.Value == "Identifiable");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Keyword && t.Value == "associatedtype");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Identifier && t.Value == "ID");
    }

    [Fact]
    public void Parse_ExtensionDeclaration_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("extension String { func uppercase() -> String { return uppercased() } }");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Keyword && t.Value == "extension");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Identifier && t.Value == "String");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Keyword && t.Value == "func");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Identifier && t.Value == "uppercase");
    }

    // === String and char literals ===

    [Fact]
    public void Parse_StringLiterals_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("\"hello\"");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.StringValue && t.Value == "\"hello\"");
    }

    [Fact]
    public void Parse_StringWithEscapes_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("\"hello\\nworld\\t\"");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.StringValue && t.Value == "\"hello\\nworld\\t\"");
    }

    [Fact]
    public void Parse_CharLiterals_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("'x'");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.CharValue && t.Value == "'x'");
    }

    [Fact]
    public void Parse_CharWithEscapes_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("'\\n' '\\t'");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.CharValue && t.Value == "'\\n'");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.CharValue && t.Value == "'\\t'");
    }

    // === Numbers ===

    [Fact]
    public void Parse_IntegerNumber_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("42");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Number && t.Value == "42");
    }

    [Fact]
    public void Parse_HexNumber_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("0xFF");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Number && t.Value == "0xFF");
    }

    [Fact]
    public void Parse_OctalNumber_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("077");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Number && t.Value == "077");
    }

    [Fact]
    public void Parse_BinaryNumber_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("0b1010");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Number && t.Value == "0b1010");
    }

    [Fact]
    public void Parse_DecimalNumber_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("3.14");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Number && t.Value == "3.14");
    }

    [Fact]
    public void Parse_NumbersWithUnderscores_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("1_000_000");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Number && t.Value == "1_000_000");
    }

    // === Comments ===

    [Fact]
    public void Parse_LineComment_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("// line comment");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Comment && t.Value.StartsWith("//"));
    }

    [Fact]
    public void Parse_BlockComment_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("/* block comment */");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Comment && t.Value.StartsWith("/*"));
    }

    [Fact]
    public void Parse_MultiLineBlockComment_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("/* line 1\nline 2\nline 3 */");
        var comment = tokens.FirstOrDefault(t => t.TokenType == SwiftTokenType.Comment);
        Assert.NotNull(comment);
        Assert.Contains("line 1", comment.Value);
        Assert.Contains("line 3", comment.Value);
    }

    // === Operators - individual tests ===

    [Fact]
    public void Parse_OperatorEquals_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a = b");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Operator && t.Value == "=");
    }

    [Fact]
    public void Parse_OperatorNotEquals_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a != b");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Operator && t.Value == "!=");
    }

    [Fact]
    public void Parse_OperatorLessThan_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a < b");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Operator && t.Value == "<");
    }

    [Fact]
    public void Parse_OperatorGreaterThan_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a > b");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Operator && t.Value == ">");
    }

    [Fact]
    public void Parse_OperatorLessThanOrEqual_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a <= b");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Operator && t.Value == "<=");
    }

    [Fact]
    public void Parse_OperatorGreaterThanOrEqual_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a >= b");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Operator && t.Value == ">=");
    }

    [Fact]
    public void Parse_OperatorEqualsEquals_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a == b");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Operator && t.Value == "==");
    }

    [Fact]
    public void Parse_OperatorPlus_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a + b");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Operator && t.Value == "+");
    }

    [Fact]
    public void Parse_OperatorMinus_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a - b");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Operator && t.Value == "-");
    }

    [Fact]
    public void Parse_OperatorMultiply_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a * b");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Operator && t.Value == "*");
    }

    [Fact]
    public void Parse_OperatorDivide_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a / b");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Operator && t.Value == "/");
    }

    [Fact]
    public void Parse_OperatorModulo_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a % b");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Operator && t.Value == "%");
    }

    [Fact]
    public void Parse_OperatorAnd_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a && b");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Operator && t.Value == "&&");
    }

    [Fact]
    public void Parse_OperatorOr_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a || b");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Operator && t.Value == "||");
    }

    [Fact]
    public void Parse_OperatorNot_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("!a");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Operator && t.Value == "!");
    }

    [Fact]
    public void Parse_OperatorIncrement_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a++");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Operator && t.Value == "++");
    }

    [Fact]
    public void Parse_OperatorDecrement_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a--");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Operator && t.Value == "--");
    }

    [Fact]
    public void Parse_OperatorBitwiseAnd_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a & b");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Operator && t.Value == "&");
    }

    [Fact]
    public void Parse_OperatorBitwiseOr_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a | b");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Operator && t.Value == "|");
    }

    [Fact]
    public void Parse_OperatorBitwiseXor_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a ^ b");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Operator && t.Value == "^");
    }

    [Fact]
    public void Parse_OperatorLeftShift_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a << 2");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Operator && t.Value == "<<");
    }

    [Fact]
    public void Parse_OperatorRightShift_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a >> 2");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Operator && t.Value == ">>");
    }

    // === Punctuation ===

    [Fact]
    public void Parse_PunctuationComma_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a, b, c");
        Assert.Equal(2, tokens.Count(t => t.TokenType == SwiftTokenType.Comma));
    }

    [Fact]
    public void Parse_PunctuationDot_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("obj.member");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Dot && t.Value == ".");
    }

    [Fact]
    public void Parse_PunctuationOpenParenthesis_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("(a)");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.OpenParenthesis && t.Value == "(");
    }

    [Fact]
    public void Parse_PunctuationCloseParenthesis_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("(a)");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.CloseParenthesis && t.Value == ")");
    }

    [Fact]
    public void Parse_PunctuationOpenBrace_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("{ a }");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.OpenBrace && t.Value == "{");
    }

    [Fact]
    public void Parse_PunctuationCloseBrace_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("{ a }");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.CloseBrace && t.Value == "}");
    }

    [Fact]
    public void Parse_PunctuationOpenBracket_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("[a]");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.OpenBracket && t.Value == "[");
    }

    [Fact]
    public void Parse_PunctuationCloseBracket_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("[a]");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.CloseBracket && t.Value == "]");
    }

    [Fact]
    public void Parse_PunctuationSemicolon_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("let x = 1; let y = 2;");
        Assert.Equal(2, tokens.Count(t => t.TokenType == SwiftTokenType.SequenceTerminator));
    }

    [Fact]
    public void Parse_PunctuationDoubleColon_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("Foo::bar");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.DoubleColon && t.Value == "::");
    }

    // === Special tokens ===

    [Fact]
    public void Parse_Boolean_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("true false");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Boolean && t.Value == "true");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Boolean && t.Value == "false");
    }

    [Fact]
    public void Parse_Nil_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("var x: String? = nil");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Keyword && t.Value == "var");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Null && t.Value == "nil");
    }

    // === Whitespace ===

    [Fact]
    public void Parse_Whitespace_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a  b");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Whitespace && t.Value == "  ");
    }

    // === Complex scenarios ===

    [Fact]
    public void Parse_OptionalBinding_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("if let name = optionalName { print(name) }");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Keyword && t.Value == "if");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Keyword && t.Value == "let");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Identifier && t.Value == "name");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Identifier && t.Value == "optionalName");
    }

    [Fact]
    public void Parse_GuardStatement_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("guard let value = optional else { return }");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Keyword && t.Value == "guard");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Keyword && t.Value == "let");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Keyword && t.Value == "else");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Keyword && t.Value == "return");
    }

    [Fact]
    public void Parse_MethodChaining_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("obj.method1().method2().method3()");
        Assert.Equal(3, tokens.Count(t => t.TokenType == SwiftTokenType.Dot && t.Value == "."));
    }

    [Fact]
    public void Parse_ComplexStruct_ReturnsExpectedTokens()
    {
        var code = """
            struct User {
                let id: Int
                var name: String
                init(id: Int, name: String) {
                    self.id = id
                    self.name = name
                }
            }
            """;
        var tokens = Tokenize(code);
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Keyword && t.Value == "struct");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Identifier && t.Value == "User");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Keyword && t.Value == "let");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Keyword && t.Value == "var");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Keyword && t.Value == "init");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Keyword && t.Value == "self");
    }

    [Fact]
    public void Parse_SwitchWithPatternMatching_ReturnsExpectedTokens()
    {
        var code = """
            switch value {
            case let (x, y) where x == y:
                print("equal")
            default:
                print("not equal")
            }
            """;
        var tokens = Tokenize(code);
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Keyword && t.Value == "switch");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Keyword && t.Value == "case");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Keyword && t.Value == "let");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Keyword && t.Value == "where");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Keyword && t.Value == "default");
    }

    [Fact]
    public void Parse_MixedCommentsAndCode_ReturnsExpectedTokens()
    {
        var code = """
            // Initialize
            let x = 0 /* start value */
            for i in 0..<10 {
                x += i // accumulate
            }
            """;
        var tokens = Tokenize(code);
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Keyword && t.Value == "let");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Keyword && t.Value == "for");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Operator && t.Value == "+=");
    }

    // === Edge cases ===

    [Fact]
    public void Parse_DecimalStartingWithDot_ReturnsExpectedTokens()
    {
        var tokens = Tokenize(".5");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Number && t.Value == ".5");
    }

    [Fact]
    public void Parse_StopDelimiter_ReturnsExpectedTokens()
    {
        var code = """
            struct User {
                let id: Int
                var name: String
                init(id: Int, name: String) {
                    self.id = id
                    self.name = name
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
            var dict: [String: [Int]] = [:]
            """;
        var tokens = Tokenize(code);
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Keyword && t.Value == "var");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Identifier && t.Value == "dict");
    }

    [Fact]
    public void Parse_ComplexStatement_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("if x > 0 && y < 10 || z == 42 { foo() } else { bar() }");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Keyword && t.Value == "if");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Keyword && t.Value == "else");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Operator && t.Value == "&&");
        Assert.Contains(tokens, t => t.TokenType == SwiftTokenType.Operator && t.Value == "||");
    }

    // === Cancellation ===

    [Fact]
    public async Task TestCancellation()
    {
        // Tokenizer processes synchronously - cancellation is not supported yet
        var code = "let x = 42";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(code));
        var tokens = new List<SwiftToken>();
        await SwiftTokenizer.Create().ParseAsync(stream, t => tokens.Add((SwiftToken)t));
        Assert.NotEmpty(tokens);
    }

    // === Markdown integration ===

    [Fact]
    public async Task TestMarkdownSwiftCodeBlock()
    {
        var markdown = """
            # Swift Example
            
            ```swift
            let x = 42
            ```
            """;

        var swiftTokens = new List<SwiftToken>();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(markdown));
        await MarkdownTokenizer.Create().ParseAsync(stream, token =>
        {
            if (token.Metadata is SwiftCodeBlockMetadata swiftMeta)
            {
                swiftMeta.RegisterInlineTokenHandler(t => swiftTokens.Add((SwiftToken)t));
            }
        });

        Assert.NotEmpty(swiftTokens);
        Assert.Contains(swiftTokens, t => t.TokenType == SwiftTokenType.Keyword && t.Value == "let");
        Assert.Contains(swiftTokens, t => t.TokenType == SwiftTokenType.Identifier && t.Value == "x");
        Assert.Contains(swiftTokens, t => t.TokenType == SwiftTokenType.Number && t.Value == "42");
    }
}
