using NTokenizers.C;
using NTokenizers.Markdown;
using System.Text;

namespace NTokenizers.Tests;

/// <summary>
/// Tests for the C tokenizer.
/// </summary>
public class CTokenizerTests
{
    private static List<CToken> Tokenize(string input) => CTokenizer.Create().Parse(input);

    [Fact]
    public void TestSimpleFunctionDeclaration()
    {
        var tokens = Tokenize("int main(void) { return 0; }");
        Assert.Equal(CTokenType.Keyword, tokens[0].TokenType);
        Assert.Equal("int", tokens[0].Value);
        Assert.Equal(CTokenType.Identifier, tokens[2].TokenType);
        Assert.Equal("main", tokens[2].Value);
        Assert.Equal(CTokenType.OpenParenthesis, tokens[3].TokenType);
        Assert.Equal(CTokenType.Keyword, tokens[4].TokenType);
        Assert.Equal("void", tokens[4].Value);
        Assert.Equal(CTokenType.CloseParenthesis, tokens[5].TokenType);
    }

    [Fact]
    public void TestStructDefinition()
    {
        var tokens = Tokenize("struct Point { int x; int y; };");
        Assert.Contains(tokens, t => t.TokenType == CTokenType.Keyword && t.Value == "struct");
        Assert.Contains(tokens, t => t.TokenType == CTokenType.Identifier && t.Value == "Point");
        Assert.Contains(tokens, t => t.TokenType == CTokenType.Keyword && t.Value == "int");
    }

    [Fact]
    public void TestStringLiteralsWithEscapes()
    {
        var tokens = Tokenize("\"hello\\nworld\\t\"");
        Assert.Single(tokens);
        Assert.Equal(CTokenType.StringValue, tokens[0].TokenType);
        Assert.Equal("\"hello\\nworld\\t\"", tokens[0].Value);
    }

    [Fact]
    public void TestCharLiterals()
    {
        var tokens = Tokenize("'a'");
        Assert.Single(tokens);
        Assert.Equal(CTokenType.CharValue, tokens[0].TokenType);
        Assert.Equal("'a'", tokens[0].Value);
    }

    [Fact]
    public void TestNumbers()
    {
        var tokens = Tokenize("42 3.14 0xFF 077 1e-10f 100UL");
        var numbers = tokens.Where(t => t.TokenType == CTokenType.Number).ToList();
        Assert.Equal(6, numbers.Count);
        Assert.Equal("42", numbers[0].Value);
        Assert.Equal("3.14", numbers[1].Value);
        Assert.Equal("0xFF", numbers[2].Value);
        Assert.Equal("077", numbers[3].Value);
        Assert.Equal("1e-10f", numbers[4].Value);
        Assert.Equal("100UL", numbers[5].Value);
    }

    [Fact]
    public void TestLineComment()
    {
        var tokens = Tokenize("// line comment\n");
        Assert.Equal(2, tokens.Count);
        Assert.Equal(CTokenType.Comment, tokens[0].TokenType);
        Assert.Equal("// line comment", tokens[0].Value);
        Assert.Equal(CTokenType.Whitespace, tokens[1].TokenType);
    }

    [Fact]
    public void TestNewlinesInComments_ReturnsWhitespaceTokens()
    {
        var code = "// comment\nint x = 42;\n";
        var tokens = Tokenize(code);

        // The comment should be followed by whitespace (the newline), then the code
        var commentToken = tokens.First(t => t.TokenType == CTokenType.Comment);
        Assert.Equal("// comment", commentToken.Value);

        // The newline after the comment should be a whitespace token
        var whitespaceAfterComment = tokens.First(t => t.TokenType == CTokenType.Whitespace);
        Assert.Contains('\n', whitespaceAfterComment.Value);

        // The code after the comment should be tokenized correctly
        Assert.Contains(tokens, t => t.TokenType == CTokenType.Keyword && t.Value == "int");
        Assert.Contains(tokens, t => t.TokenType == CTokenType.Identifier && t.Value == "x");
        Assert.Contains(tokens, t => t.TokenType == CTokenType.Number && t.Value == "42");
    }

    [Fact]
    public void TestBlockComment()
    {
        var tokens = Tokenize("/* block comment */");
        Assert.Single(tokens);
        Assert.Equal(CTokenType.Comment, tokens[0].TokenType);
        Assert.Equal("/* block comment */", tokens[0].Value);
    }

    [Fact]
    public void TestPreprocessorDirectives()
    {
        var tokens = Tokenize("#include <stdio.h>\n");
        Assert.Contains(tokens, t => t.TokenType == CTokenType.Preprocessor);
        Assert.Contains(tokens, t => t.TokenType == CTokenType.Preprocessor && t.Value.Contains("#include"));
    }

    [Fact]
    public void TestPreprocessorPreservesNewlines()
    {
        var code = "#include <stdio.h>\n#include <stdlib.h>\nint main(void) { return 0; }\n";
        var tokens = Tokenize(code);
        // Check that newlines after preprocessor directives are preserved as whitespace
        var preprocessorTokens = tokens.Where(t => t.TokenType == CTokenType.Preprocessor).ToList();
        Assert.Equal(2, preprocessorTokens.Count);
        // Check that whitespace tokens exist after preprocessor directives
        Assert.Contains(tokens, t => t.TokenType == CTokenType.Whitespace && t.Value.Contains('\n'));
        // Check that the code after preprocessor directives is still tokenized correctly
        Assert.Contains(tokens, t => t.TokenType == CTokenType.Keyword && t.Value == "int");
        Assert.Contains(tokens, t => t.TokenType == CTokenType.Identifier && t.Value == "main");
    }

    [Fact]
    public void TestPreprocessorWithCarriageReturn()
    {
        var code = "#include <stdio.h>\r\nint main(void) { return 0; }\r\n";
        var tokens = Tokenize(code);
        // Check that carriage returns after preprocessor directives are preserved as whitespace
        Assert.Contains(tokens, t => t.TokenType == CTokenType.Preprocessor);
        Assert.Contains(tokens, t => t.TokenType == CTokenType.Whitespace && t.Value.Contains('\r'));
        // Check that the code after preprocessor directives is still tokenized correctly
        Assert.Contains(tokens, t => t.TokenType == CTokenType.Keyword && t.Value == "int");
        Assert.Contains(tokens, t => t.TokenType == CTokenType.Identifier && t.Value == "main");
    }

    [Fact]
    public void TestPreprocessorMultipleLines()
    {
        var code = "#ifndef HEADER_H\n#define HEADER_H\n#endif\n";
        var tokens = Tokenize(code);
        var preprocessorTokens = tokens.Where(t => t.TokenType == CTokenType.Preprocessor).ToList();
        Assert.Equal(3, preprocessorTokens.Count);
        // Check that whitespace tokens exist between preprocessor directives
        Assert.Contains(tokens, t => t.TokenType == CTokenType.Whitespace && t.Value.Contains('\n'));
    }

    [Fact]
    public void TestPreprocessorOutputMatchesInput()
    {
        var code = "#include <stdio.h>\n#include <stdlib.h>\nint main(void) { return 0; }\n";
        var tokens = Tokenize(code);
        // Concatenate all token values - should match original code
        var output = string.Concat(tokens.Select(t => t.Value));
        Assert.Equal(code, output);
    }

    [Fact]
    public void TestOperators()
    {
        var tokens = Tokenize("a + b == c && d != e || f < g >= h");
        var operators = tokens.Where(t => t.TokenType == CTokenType.Operator).ToList();
        Assert.Equal(7, operators.Count);
    }

    [Fact]
    public void TestPointerSyntax()
    {
        var tokens = Tokenize("int *ptr = &x;");
        Assert.Contains(tokens, t => t.TokenType == CTokenType.Operator && t.Value == "*");
        Assert.Contains(tokens, t => t.TokenType == CTokenType.Operator && t.Value == "&");
    }

    [Fact]
    public void TestArrayAccess()
    {
        var tokens = Tokenize("arr[0]");
        Assert.Contains(tokens, t => t.TokenType == CTokenType.OpenBracket);
        Assert.Contains(tokens, t => t.TokenType == CTokenType.CloseBracket);
    }

    [Fact]
    public void TestKeywordsVsIdentifiers()
    {
        var tokens = Tokenize("int myInt = 42;");
        Assert.Equal(CTokenType.Keyword, tokens[0].TokenType);
        Assert.Equal("int", tokens[0].Value);
        Assert.Equal(CTokenType.Identifier, tokens[2].TokenType);
        Assert.Equal("myInt", tokens[2].Value);
    }

    [Fact]
    public void TestPunctuation()
    {
        var tokens = Tokenize("(x,y){}[a];:");
        Assert.Equal(CTokenType.OpenParenthesis, tokens[0].TokenType);
        Assert.Equal(CTokenType.Identifier, tokens[1].TokenType);
        Assert.Equal(CTokenType.Comma, tokens[2].TokenType);
        Assert.Equal(CTokenType.Identifier, tokens[3].TokenType);
        Assert.Equal(CTokenType.CloseParenthesis, tokens[4].TokenType);
        Assert.Equal(CTokenType.OpenBrace, tokens[5].TokenType);
        Assert.Equal(CTokenType.CloseBrace, tokens[6].TokenType);
        Assert.Equal(CTokenType.OpenBracket, tokens[7].TokenType);
        Assert.Equal(CTokenType.Identifier, tokens[8].TokenType);
        Assert.Equal(CTokenType.CloseBracket, tokens[9].TokenType);
        Assert.Equal(CTokenType.SequenceTerminator, tokens[10].TokenType);
        Assert.Equal(CTokenType.Colon, tokens[11].TokenType);
    }

    [Fact]
    public void TestComplexStructWithFunctionPointerAndArray()
    {
        var code = """
            struct Node {
                int data;
                struct Node *next;
                void (*callback)(int x);
                int values[10];
            };
            """;
        var tokens = Tokenize(code);
        Assert.Contains(tokens, t => t.TokenType == CTokenType.Keyword && t.Value == "struct");
        Assert.Contains(tokens, t => t.TokenType == CTokenType.Identifier && t.Value == "Node");
        Assert.Contains(tokens, t => t.TokenType == CTokenType.Keyword && t.Value == "int");
        Assert.Contains(tokens, t => t.TokenType == CTokenType.Keyword && t.Value == "void");
        Assert.Contains(tokens, t => t.TokenType == CTokenType.Identifier && t.Value == "callback");
    }

    [Fact]
    public void TestComplexPreprocessorWithFunction()
    {
        var code = """
            #ifndef HEADER_H
            #define HEADER_H
            
            #ifdef DEBUG
                #define LOG(x) printf(x)
            #else
                #define LOG(x) ((void)0)
            #endif
            
            int main(void) {
                LOG("hello");
                return 0;
            }
            #endif
            """;
        var tokens = Tokenize(code);
        Assert.Contains(tokens, t => t.TokenType == CTokenType.Preprocessor && t.Value.Contains("#ifndef"));
        Assert.Contains(tokens, t => t.TokenType == CTokenType.Preprocessor && t.Value.Contains("#define"));
        Assert.Contains(tokens, t => t.TokenType == CTokenType.Preprocessor && t.Value.Contains("#ifdef"));
        Assert.Contains(tokens, t => t.TokenType == CTokenType.Preprocessor && t.Value.Contains("#else"));
        Assert.Contains(tokens, t => t.TokenType == CTokenType.Preprocessor && t.Value.Contains("#endif"));
        Assert.Contains(tokens, t => t.TokenType == CTokenType.Keyword && t.Value == "int");
        Assert.Contains(tokens, t => t.TokenType == CTokenType.Identifier && t.Value == "main");
    }

    [Fact]
    public async Task TestMarkdownCCodeBlock()
    {
        var markdown = """
            # C Example
            
            ```c
            #include <stdio.h>
            
            int main(void) {
                printf("Hello, World!\n");
                return 0;
            }
            ```
            """;
        
        var cTokens = new List<CToken>();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(markdown));
        await MarkdownTokenizer.Create().ParseAsync(stream, token =>
        {
            if (token.Metadata is CCodeBlockMetadata cMeta)
            {
                cMeta.RegisterInlineTokenHandler(t => cTokens.Add((CToken)t));
            }
        });
        
        Assert.NotEmpty(cTokens);
        Assert.Contains(cTokens, t => t.TokenType == CTokenType.Keyword && t.Value == "int");
        Assert.Contains(cTokens, t => t.TokenType == CTokenType.Keyword && t.Value == "return");
        Assert.Contains(cTokens, t => t.TokenType == CTokenType.Identifier && t.Value == "main");
    }
}
