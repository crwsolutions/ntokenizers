using NTokenizers.Cpp;
using NTokenizers.Markdown;
using System.Text;

namespace NTokenizers.Tests;

public class CppTokenizerTests
{
    private static List<CppToken> Tokenize(string input) => CppTokenizer.Create().Parse(input);

    // === Basic token types ===

    [Fact]
    public void Parse_SimpleClassDeclaration_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("class Foo { public: int x; };");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Keyword && t.Value == "class");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Identifier && t.Value == "Foo");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.OpenBrace && t.Value == "{");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Keyword && t.Value == "public");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Keyword && t.Value == "int");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Identifier && t.Value == "x");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.SequenceTerminator && t.Value == ";");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.CloseBrace && t.Value == "}");
    }

    [Fact]
    public void Parse_TemplateSyntax_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("template<typename T> T identity(T x) { return x; }");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Keyword && t.Value == "template");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Keyword && t.Value == "typename");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Identifier && t.Value == "identity");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Keyword && t.Value == "return");
    }

    [Fact]
    public void Parse_NamespaceDeclaration_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("namespace foo { int bar = 42; }");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Keyword && t.Value == "namespace");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Identifier && t.Value == "foo");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Identifier && t.Value == "bar");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Number && t.Value == "42");
    }

    [Fact]
    public void Parse_LambdaExpression_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("auto fn = [](int x, int y) { return x + y; };");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Keyword && t.Value == "auto");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Identifier && t.Value == "fn");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Operator && t.Value == "=");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Keyword && t.Value == "int");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Operator && t.Value == "+");
    }

    // === String and char literals ===

    [Fact]
    public void Parse_StringLiterals_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("\"hello\" L\"world\"");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.StringValue && t.Value == "\"hello\"");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.StringValue && t.Value == "L\"world\"");
    }

    [Fact]
    public void Parse_StringWithEscapes_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("\"hello\\nworld\\t\"");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.StringValue && t.Value == "\"hello\\nworld\\t\"");
    }

    [Fact]
    public void Parse_CharLiterals_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("'x' L'y'");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.CharValue && t.Value == "'x'");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.CharValue && t.Value == "L'y'");
    }

    [Fact]
    public void Parse_CharWithEscapes_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("'\\n' '\\t'");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.CharValue && t.Value == "'\\n'");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.CharValue && t.Value == "'\\t'");
    }

    // === Numbers ===

    [Fact]
    public void Parse_IntegerNumber_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("42");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Number && t.Value == "42");
    }

    [Fact]
    public void Parse_HexNumber_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("0xFF");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Number && t.Value == "0xFF");
    }

    [Fact]
    public void Parse_BinaryNumber_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("0b1010");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Number && t.Value == "0b1010");
    }

    [Fact]
    public void Parse_DecimalNumber_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("3.14");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Number && t.Value == "3.14");
    }

    [Fact]
    public void Parse_ScientificNotation_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("3.14e-2");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Number && t.Value == "3.14e-2");
    }

    [Fact]
    public void Parse_NumbersWithSuffixes_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("42ULL 3.14f 0xFFL");
        var numbers = tokens.Where(t => t.TokenType == CppTokenType.Number).ToList();
        Assert.Equal(3, numbers.Count);
    }

    // === Comments ===

    [Fact]
    public void Parse_LineComment_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("// line comment");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Comment && t.Value.StartsWith("//"));
    }

    [Fact]
    public void Parse_BlockComment_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("/* block comment */");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Comment && t.Value.StartsWith("/*"));
    }

    [Fact]
    public void Parse_MultiLineBlockComment_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("/* line 1\nline 2\nline 3 */");
        var comment = tokens.FirstOrDefault(t => t.TokenType == CppTokenType.Comment);
        Assert.NotNull(comment);
        Assert.Contains("line 1", comment.Value);
        Assert.Contains("line 3", comment.Value);
    }

    // === Preprocessor ===

    [Fact]
    public void Parse_PreprocessorInclude_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("#include <iostream>");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Keyword);
    }

    [Fact]
    public void Parse_PreprocessorDefine_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("#define MAX 100");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Keyword);
    }

    [Fact]
    public void Parse_PreprocessorPreservesNewlines()
    {
        var code = "#include <iostream>\n#include <vector>\nint main() { return 0; }\n";
        var tokens = Tokenize(code);
        // Check that newlines after preprocessor directives are preserved as whitespace
        var keywordTokens = tokens.Where(t => t.TokenType == CppTokenType.Keyword).ToList();
        Assert.Contains(keywordTokens, t => t.Value.Contains("#include"));
        // Check that whitespace tokens exist after preprocessor directives
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Whitespace && t.Value.Contains('\n'));
        // Check that the code after preprocessor directives is still tokenized correctly
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Keyword && t.Value == "int");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Identifier && t.Value == "main");
    }

    [Fact]
    public void Parse_PreprocessorWithCarriageReturn()
    {
        var code = "#include <iostream>\r\nint main() { return 0; }\r\n";
        var tokens = Tokenize(code);
        // Check that carriage returns after preprocessor directives are preserved as whitespace
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Keyword);
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Whitespace && t.Value.Contains('\r'));
        // Check that the code after preprocessor directives is still tokenized correctly
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Keyword && t.Value == "int");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Identifier && t.Value == "main");
    }

    [Fact]
    public void Parse_PreprocessorMultipleLines()
    {
        var code = "#ifndef HEADER_H\n#define HEADER_H\n#endif\n";
        var tokens = Tokenize(code);
        var keywordTokens = tokens.Where(t => t.TokenType == CppTokenType.Keyword).ToList();
        Assert.Equal(3, keywordTokens.Count);
        // Check that whitespace tokens exist between preprocessor directives
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Whitespace && t.Value.Contains('\n'));
    }

    [Fact]
    public void Parse_PreprocessorOutputMatchesInput()
    {
        var code = "#include <iostream>\n#include <vector>\nint main() { return 0; }\n";
        var tokens = Tokenize(code);
        // Concatenate all token values - should match original code
        var output = string.Concat(tokens.Select(t => t.Value));
        Assert.Equal(code, output);
    }

    // === Operators - individual tests ===

    [Fact]
    public void Parse_OperatorEquals_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a = b");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Operator && t.Value == "=");
    }

    [Fact]
    public void Parse_OperatorNotEquals_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a != b");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Operator && t.Value == "!=");
    }

    [Fact]
    public void Parse_OperatorLessThan_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a < b");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Operator && t.Value == "<");
    }

    [Fact]
    public void Parse_OperatorGreaterThan_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a > b");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Operator && t.Value == ">");
    }

    [Fact]
    public void Parse_OperatorLessThanOrEqual_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a <= b");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Operator && t.Value == "<=");
    }

    [Fact]
    public void Parse_OperatorGreaterThanOrEqual_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a >= b");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Operator && t.Value == ">=");
    }

    [Fact]
    public void Parse_OperatorEqualsEquals_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a == b");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Operator && t.Value == "==");
    }

    [Fact]
    public void Parse_OperatorPlus_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a + b");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Operator && t.Value == "+");
    }

    [Fact]
    public void Parse_OperatorMinus_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a - b");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Operator && t.Value == "-");
    }

    [Fact]
    public void Parse_OperatorMultiply_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a * b");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Operator && t.Value == "*");
    }

    [Fact]
    public void Parse_OperatorDivide_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a / b");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Operator && t.Value == "/");
    }

    [Fact]
    public void Parse_OperatorModulo_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a % b");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Operator && t.Value == "%");
    }

    [Fact]
    public void Parse_OperatorAnd_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a && b");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Operator && t.Value == "&&");
    }

    [Fact]
    public void Parse_OperatorOr_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a || b");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Operator && t.Value == "||");
    }

    [Fact]
    public void Parse_OperatorNot_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("!a");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Operator && t.Value == "!");
    }

    [Fact]
    public void Parse_OperatorIncrement_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a++");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Operator && t.Value == "++");
    }

    [Fact]
    public void Parse_OperatorDecrement_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a--");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Operator && t.Value == "--");
    }

    [Fact]
    public void Parse_OperatorBitwiseAnd_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a & b");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Operator && t.Value == "&");
    }

    [Fact]
    public void Parse_OperatorBitwiseOr_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a | b");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Operator && t.Value == "|");
    }

    [Fact]
    public void Parse_OperatorBitwiseXor_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a ^ b");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Operator && t.Value == "^");
    }

    [Fact]
    public void Parse_OperatorLeftShift_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a << 2");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Operator && t.Value == "<<");
    }

    [Fact]
    public void Parse_OperatorRightShift_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a >> 2");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Operator && t.Value == ">>");
    }

    [Fact]
    public void Parse_OperatorCompoundAssignment_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a += b; c -= d; e *= f");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Operator && t.Value == "+=");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Operator && t.Value == "-=");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Operator && t.Value == "*=");
    }

    [Fact]
    public void Parse_OperatorArrow_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("ptr->member");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Operator && t.Value == "->");
    }

    [Fact]
    public void Parse_OperatorTernary_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a ? b : c");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Operator && t.Value == "?");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Colon && t.Value == ":");
    }

    [Fact]
    public void Parse_OperatorQuestionMark_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a ? b");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Operator && t.Value == "?");
    }

    [Fact]
    public void Parse_OperatorColon_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a:b");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Colon && t.Value == ":");
    }

    // === Punctuation ===

    [Fact]
    public void Parse_PunctuationComma_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a, b, c");
        Assert.Equal(2, tokens.Count(t => t.TokenType == CppTokenType.Comma));
    }

    [Fact]
    public void Parse_PunctuationDot_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("obj.member");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Dot && t.Value == ".");
    }

    [Fact]
    public void Parse_PunctuationOpenParenthesis_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("(a)");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.OpenParenthesis && t.Value == "(");
    }

    [Fact]
    public void Parse_PunctuationCloseParenthesis_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("(a)");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.CloseParenthesis && t.Value == ")");
    }

    [Fact]
    public void Parse_PunctuationOpenBrace_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("{ a }");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.OpenBrace && t.Value == "{");
    }

    [Fact]
    public void Parse_PunctuationCloseBrace_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("{ a }");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.CloseBrace && t.Value == "}");
    }

    [Fact]
    public void Parse_PunctuationOpenBracket_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("[a]");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.OpenBracket && t.Value == "[");
    }

    [Fact]
    public void Parse_PunctuationCloseBracket_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("[a]");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.CloseBracket && t.Value == "]");
    }

    [Fact]
    public void Parse_PunctuationSemicolon_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("int x; int y;");
        Assert.Equal(2, tokens.Count(t => t.TokenType == CppTokenType.SequenceTerminator));
    }

    [Fact]
    public void Parse_PunctuationDoubleColon_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("std::cout");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.DoubleColon && t.Value == "::");
    }

    // === Special tokens ===

    [Fact]
    public void Parse_Boolean_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("true false");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Boolean && t.Value == "true");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Boolean && t.Value == "false");
    }

    [Fact]
    public void Parse_Null_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("nullptr NULL");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Null && t.Value == "nullptr");
    }

    [Fact]
    public void Parse_Attribute_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("__attribute__((unused))");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Identifier && t.Value == "__attribute__");
    }

    // === Whitespace ===

    [Fact]
    public void Parse_Whitespace_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("a  b");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Whitespace && t.Value == "  ");
    }

    // === Complex scenarios ===

    [Fact]
    public void Parse_SmartPointers_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("std::make_unique<int>(42) std::make_shared<double>(3.14)");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.DoubleColon && t.Value == "::");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Identifier && t.Value == "make_unique");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Identifier && t.Value == "make_shared");
    }

    [Fact]
    public void Parse_RangeBasedForLoop_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("for (const auto& item : items) { std::cout << item; }");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Keyword && t.Value == "for");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Keyword && t.Value == "const");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Keyword && t.Value == "auto");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Operator && t.Value == "<<");
    }

    [Fact]
    public void Parse_MethodChaining_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("obj.method1().method2().method3()");
        Assert.Equal(3, tokens.Count(t => t.TokenType == CppTokenType.Dot && t.Value == "."));
    }

    [Fact]
    public void Parse_ArrayAccess_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("arr[i] = arr[j][k]");
        var brackets = tokens.Where(t => t.TokenType == CppTokenType.OpenBracket || t.TokenType == CppTokenType.CloseBracket).ToList();
        Assert.Equal(6, brackets.Count);
    }

    [Fact]
    public void Parse_ComplexTemplateWithLambda_ReturnsExpectedTokens()
    {
        var code = """
            template<typename T>
            class Container {
            public:
                T process(T value) {
                    return [&value](auto x) {
                        return value + x;
                    }(value);
                }
            };
            """;
        var tokens = Tokenize(code);
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Keyword && t.Value == "template");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Keyword && t.Value == "class");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Keyword && t.Value == "public");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Identifier && t.Value == "Container");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Identifier && t.Value == "process");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Keyword && t.Value == "return");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Operator && t.Value == "+");
    }

    [Fact]
    public void Parse_NamespaceWithClassAndSmartPointers_ReturnsExpectedTokens()
    {
        var code = """
            namespace myapp {
            class Manager {
            public:
                Manager() : ptr_(std::make_unique<int>(0)) {}
                std::unique_ptr<int> ptr_;
            };
            }
            """;
        var tokens = Tokenize(code);
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Keyword && t.Value == "namespace");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Keyword && t.Value == "class");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Keyword && t.Value == "public");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Identifier && t.Value == "Manager");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.DoubleColon && t.Value == "::");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Identifier && t.Value == "make_unique");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Number && t.Value == "0");
    }

    [Fact]
    public void Parse_MixedCommentsAndCode_ReturnsExpectedTokens()
    {
        var code = """
            // Initialize
            int x = 0; /* start value */
            for (int i = 0; i < 10; i++) {
                x += i; // accumulate
            }
            """;
        var tokens = Tokenize(code);
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Keyword && t.Value == "int");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Keyword && t.Value == "for");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Operator && t.Value == "+=");
    }

    // === Edge cases ===

    [Fact]
    public void Parse_DecimalStartingWithDot_ReturnsExpectedTokens()
    {
        var tokens = Tokenize(".5");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Number && t.Value == ".5");
    }

    [Fact]
    public void Parse_StopDelimiter_ReturnsExpectedTokens()
    {
        var code = """
            template<typename T>
            class Container {
            public:
                T process(T value) {
                    return [&value](auto x) {
                        return value + x;
                    }(value);
                }
            };
            """;
        var tokens = Tokenize(code);
        Assert.NotEmpty(tokens);
    }

    [Fact]
    public void Parse_StopDelimiterInMiddleOfToken_ReturnsExpectedTokens()
    {
        var code = """
            std::vector<std::pair<int, std::string>> v;
            """;
        var tokens = Tokenize(code);
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Identifier && t.Value == "std");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Identifier && t.Value == "vector");
    }

    [Fact]
    public void Parse_ComplexStatement_ReturnsExpectedTokens()
    {
        var tokens = Tokenize("if (x > 0 && y < 10 || z == 42) { foo(); } else { bar(); }");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Keyword && t.Value == "if");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Keyword && t.Value == "else");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Operator && t.Value == "&&");
        Assert.Contains(tokens, t => t.TokenType == CppTokenType.Operator && t.Value == "||");
    }

    // === Cancellation ===

    [Fact]
    public async Task TestCancellation()
    {
        // Tokenizer processes synchronously - cancellation is not supported yet
        var code = "int x = 42;";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(code));
        var tokens = new List<CppToken>();
        await CppTokenizer.Create().ParseAsync(stream, t => tokens.Add((CppToken)t));
        Assert.NotEmpty(tokens);
    }

    // === Markdown integration ===

    [Fact]
    public async Task TestMarkdownCppCodeBlock()
    {
        var markdown = """
            # C++ Example
            
            ```cpp
            int x = 42;
            ```
            """;

        var cppTokens = new List<CppToken>();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(markdown));
        await MarkdownTokenizer.Create().ParseAsync(stream, token =>
        {
            if (token.Metadata is CppCodeBlockMetadata cppMeta)
            {
                cppMeta.RegisterInlineTokenHandler(t => cppTokens.Add((CppToken)t));
            }
        });

        Assert.NotEmpty(cppTokens);
        Assert.Contains(cppTokens, t => t.TokenType == CppTokenType.Keyword && t.Value == "int");
        Assert.Contains(cppTokens, t => t.TokenType == CppTokenType.Identifier && t.Value == "x");
        Assert.Contains(cppTokens, t => t.TokenType == CppTokenType.Number && t.Value == "42");
    }
}
