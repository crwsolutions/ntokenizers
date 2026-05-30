using NTokenizers.Java;
using NTokenizers.Markdown;
using System.Text;

namespace NTokenizers.Tests;

/// <summary>
/// Tests for the Java tokenizer.
/// </summary>
public class JavaTokenizerTests
{
    private static List<JavaToken> Tokenize(string input) => JavaTokenizer.Create().Parse(input);

    [Fact]
    public void TestSimpleClassDeclaration()
    {
        var tokens = Tokenize("public class Main { }");
        Assert.Equal(9, tokens.Count);
        Assert.Equal(JavaTokenType.Keyword, tokens[0].TokenType);
        Assert.Equal("public", tokens[0].Value);
        Assert.Equal(JavaTokenType.Whitespace, tokens[1].TokenType);
        Assert.Equal(JavaTokenType.Keyword, tokens[2].TokenType);
        Assert.Equal("class", tokens[2].Value);
        Assert.Equal(JavaTokenType.Whitespace, tokens[3].TokenType);
        Assert.Equal(JavaTokenType.Identifier, tokens[4].TokenType);
        Assert.Equal("Main", tokens[4].Value);
        Assert.Equal(JavaTokenType.Whitespace, tokens[5].TokenType);
        Assert.Equal(JavaTokenType.OpenBrace, tokens[6].TokenType);
        Assert.Equal("{", tokens[6].Value);
        Assert.Equal(JavaTokenType.Whitespace, tokens[7].TokenType);
        Assert.Equal(JavaTokenType.CloseBrace, tokens[8].TokenType);
        Assert.Equal("}", tokens[8].Value);
    }

    [Fact]
    public void TestMethodWithParameters()
    {
        var tokens = Tokenize("public static void main(String[] args) {}");
        Assert.Equal(JavaTokenType.Keyword, tokens[0].TokenType);
        Assert.Equal("public", tokens[0].Value);
        Assert.Equal(JavaTokenType.Whitespace, tokens[1].TokenType);
        Assert.Equal(JavaTokenType.Keyword, tokens[2].TokenType);
        Assert.Equal("static", tokens[2].Value);
        Assert.Equal(JavaTokenType.Whitespace, tokens[3].TokenType);
        Assert.Equal(JavaTokenType.Keyword, tokens[4].TokenType);
        Assert.Equal("void", tokens[4].Value);
        Assert.Equal(JavaTokenType.Whitespace, tokens[5].TokenType);
        Assert.Equal(JavaTokenType.Identifier, tokens[6].TokenType);
        Assert.Equal("main", tokens[6].Value);
        Assert.Equal(JavaTokenType.OpenParenthesis, tokens[7].TokenType);
        Assert.Equal(JavaTokenType.Identifier, tokens[8].TokenType);
        Assert.Equal("String", tokens[8].Value);
        Assert.Equal(JavaTokenType.OpenBracket, tokens[9].TokenType);
        Assert.Equal(JavaTokenType.CloseBracket, tokens[10].TokenType);
        Assert.Equal(JavaTokenType.Whitespace, tokens[11].TokenType);
        Assert.Equal(JavaTokenType.Identifier, tokens[12].TokenType);
        Assert.Equal("args", tokens[12].Value);
        Assert.Equal(JavaTokenType.CloseParenthesis, tokens[13].TokenType);
    }

    [Fact]
    public void TestStringLiteralsWithEscapes()
    {
        var tokens = Tokenize("\"hello\\nworld\\t\"");
        Assert.Single(tokens);
        Assert.Equal(JavaTokenType.StringValue, tokens[0].TokenType);
        Assert.Equal("\"hello\\nworld\\t\"", tokens[0].Value);
    }

    [Fact]
    public void TestCharLiterals()
    {
        var tokens = Tokenize("'a'");
        Assert.Single(tokens);
        Assert.Equal(JavaTokenType.CharValue, tokens[0].TokenType);
        Assert.Equal("'a'", tokens[0].Value);
    }

    [Fact]
    public void TestNumbers()
    {
        var tokens = Tokenize("42 3.14 0xFF 0b1010 077 1e-10f 100L");
        var numbers = tokens.Where(t => t.TokenType == JavaTokenType.Number).ToList();
        Assert.Equal(7, numbers.Count);
        Assert.Equal("42", numbers[0].Value);
        Assert.Equal("3.14", numbers[1].Value);
        Assert.Equal("0xFF", numbers[2].Value);
        Assert.Equal("0b1010", numbers[3].Value);
        Assert.Equal("077", numbers[4].Value);
        Assert.Equal("1e-10f", numbers[5].Value);
        Assert.Equal("100L", numbers[6].Value);
    }

    [Fact]
    public void TestLineComment()
    {
        var tokens = Tokenize("// line comment\n");
        Assert.Equal(2, tokens.Count);
        Assert.Equal(JavaTokenType.Comment, tokens[0].TokenType);
        Assert.Equal("// line comment", tokens[0].Value);
        Assert.Equal(JavaTokenType.Whitespace, tokens[1].TokenType);
    }

    [Fact]
    public void TestNewlinesInComments_ReturnsWhitespaceTokens()
    {
        var code = "// comment\nint x = 42;\n";
        var tokens = Tokenize(code);

        // The comment should be followed by whitespace (the newline), then the code
        var commentToken = tokens.First(t => t.TokenType == JavaTokenType.Comment);
        Assert.Equal("// comment", commentToken.Value);

        // The newline after the comment should be a whitespace token
        var whitespaceAfterComment = tokens.First(t => t.TokenType == JavaTokenType.Whitespace);
        Assert.Contains('\n', whitespaceAfterComment.Value);

        // The code after the comment should be tokenized correctly
        Assert.Contains(tokens, t => t.TokenType == JavaTokenType.Keyword && t.Value == "int");
        Assert.Contains(tokens, t => t.TokenType == JavaTokenType.Identifier && t.Value == "x");
        Assert.Contains(tokens, t => t.TokenType == JavaTokenType.Number && t.Value == "42");
    }

    [Fact]
    public void TestBlockComment()
    {
        var tokens = Tokenize("/* block comment */");
        Assert.Single(tokens);
        Assert.Equal(JavaTokenType.Comment, tokens[0].TokenType);
        Assert.Equal("/* block comment */", tokens[0].Value);
    }

    [Fact]
    public void TestJavadocComment()
    {
        var tokens = Tokenize("/** Javadoc */");
        Assert.Single(tokens);
        Assert.Equal(JavaTokenType.Comment, tokens[0].TokenType);
        Assert.Equal("/** Javadoc */", tokens[0].Value);
    }

    [Fact]
    public void TestOperators()
    {
        var tokens = Tokenize("a + b == c && d != e || f < g >= h");
        var operators = tokens.Where(t => t.TokenType == JavaTokenType.Operator).ToList();
        Assert.Equal(7, operators.Count);
        Assert.Equal("+", operators[0].Value);
        Assert.Equal("==", operators[1].Value);
        Assert.Equal("&&", operators[2].Value);
        Assert.Equal("!=", operators[3].Value);
        Assert.Equal("||", operators[4].Value);
        Assert.Equal("<", operators[5].Value);
        Assert.Equal(">=", operators[6].Value);
    }

    [Fact]
    public void TestLambdaExpressions()
    {
        var tokens = Tokenize("list.stream().map(x -> x * 2).toList()");
        Assert.Contains(tokens, t => t.TokenType == JavaTokenType.Identifier && t.Value == "list");
        Assert.Contains(tokens, t => t.TokenType == JavaTokenType.Operator && t.Value == "->");
        Assert.Contains(tokens, t => t.TokenType == JavaTokenType.Operator && t.Value == "*");
    }

    [Fact]
    public void TestGenerics()
    {
        var tokens = Tokenize("List<String> list = new ArrayList<>();");
        Assert.Contains(tokens, t => t.TokenType == JavaTokenType.Identifier && t.Value == "List");
        Assert.Contains(tokens, t => t.TokenType == JavaTokenType.Identifier && t.Value == "String");
        Assert.Contains(tokens, t => t.TokenType == JavaTokenType.Identifier && t.Value == "ArrayList");
    }

    [Fact]
    public void TestAnnotations()
    {
        var tokens = Tokenize("@Override public void foo() {}");
        Assert.Contains(tokens, t => t.TokenType == JavaTokenType.Operator && t.Value == "@");
        Assert.Contains(tokens, t => t.TokenType == JavaTokenType.Identifier && t.Value == "Override");
    }

    [Fact]
    public void TestKeywordsVsIdentifiers()
    {
        var tokens = Tokenize("int myInt = 42;");
        Assert.Equal(JavaTokenType.Keyword, tokens[0].TokenType);
        Assert.Equal("int", tokens[0].Value);
        Assert.Equal(JavaTokenType.Whitespace, tokens[1].TokenType);
        Assert.Equal(JavaTokenType.Identifier, tokens[2].TokenType);
        Assert.Equal("myInt", tokens[2].Value);
    }

    [Fact]
    public void TestBooleanAndNull()
    {
        var tokens = Tokenize("true false null");
        Assert.Equal(JavaTokenType.Boolean, tokens[0].TokenType);
        Assert.Equal("true", tokens[0].Value);
        Assert.Equal(JavaTokenType.Whitespace, tokens[1].TokenType);
        Assert.Equal(JavaTokenType.Boolean, tokens[2].TokenType);
        Assert.Equal("false", tokens[2].Value);
        Assert.Equal(JavaTokenType.Whitespace, tokens[3].TokenType);
        Assert.Equal(JavaTokenType.Null, tokens[4].TokenType);
        Assert.Equal("null", tokens[4].Value);
    }

    [Fact]
    public void TestPunctuation()
    {
        var tokens = Tokenize("(x,y){}[a];:");
        Assert.Equal(JavaTokenType.OpenParenthesis, tokens[0].TokenType);
        Assert.Equal(JavaTokenType.Identifier, tokens[1].TokenType);
        Assert.Equal(JavaTokenType.Comma, tokens[2].TokenType);
        Assert.Equal(JavaTokenType.Identifier, tokens[3].TokenType);
        Assert.Equal(JavaTokenType.CloseParenthesis, tokens[4].TokenType);
        Assert.Equal(JavaTokenType.OpenBrace, tokens[5].TokenType);
        Assert.Equal(JavaTokenType.CloseBrace, tokens[6].TokenType);
        Assert.Equal(JavaTokenType.OpenBracket, tokens[7].TokenType);
        Assert.Equal(JavaTokenType.Identifier, tokens[8].TokenType);
        Assert.Equal(JavaTokenType.CloseBracket, tokens[9].TokenType);
        Assert.Equal(JavaTokenType.SequenceTerminator, tokens[10].TokenType);
        Assert.Equal(JavaTokenType.Colon, tokens[11].TokenType);
    }

    [Fact]
    public void TestComplexClassWithGenericsAndAnnotations()
    {
        var code = """
            @SuppressWarnings("unchecked")
            public class MyGenericClass<T extends Comparable<T>> {
                private final List<T> items;
                public MyGenericClass() {
                    this.items = new ArrayList<>();
                }
            }
            """;
        var tokens = Tokenize(code);
        Assert.Contains(tokens, t => t.TokenType == JavaTokenType.Operator && t.Value == "@");
        Assert.Contains(tokens, t => t.TokenType == JavaTokenType.Keyword && t.Value == "public");
        Assert.Contains(tokens, t => t.TokenType == JavaTokenType.Keyword && t.Value == "class");
        Assert.Contains(tokens, t => t.TokenType == JavaTokenType.Identifier && t.Value == "MyGenericClass");
        Assert.Contains(tokens, t => t.TokenType == JavaTokenType.Keyword && t.Value == "extends");
        Assert.Contains(tokens, t => t.TokenType == JavaTokenType.Keyword && t.Value == "private");
        Assert.Contains(tokens, t => t.TokenType == JavaTokenType.Keyword && t.Value == "final");
        Assert.Contains(tokens, t => t.TokenType == JavaTokenType.Identifier && t.Value == "items");
        Assert.Contains(tokens, t => t.TokenType == JavaTokenType.Keyword && t.Value == "this");
        Assert.Contains(tokens, t => t.TokenType == JavaTokenType.Keyword && t.Value == "new");
    }

    [Fact]
        public void TestComplexLambdaWithStreams()
        {
            var code = """
                List<String> result = list.stream()
                    .filter(x -> x != null)
                    .map(x -> x.toUpperCase())
                    .sorted((a, b) -> a.compareTo(b))
                    .toList();
                """;
            var tokens = Tokenize(code);
            Assert.Contains(tokens, t => t.TokenType == JavaTokenType.Operator && t.Value == "->");
            Assert.Contains(tokens, t => t.TokenType == JavaTokenType.Operator && t.Value == "!=");
            Assert.Contains(tokens, t => t.TokenType == JavaTokenType.Dot && t.Value == ".");
            Assert.Contains(tokens, t => t.TokenType == JavaTokenType.Null && t.Value == "null");
        }

    [Fact]
    public async Task TestMarkdownJavaCodeBlock()
    {
        var markdown = """
            # Java Example
            
            ```java
            public class HelloWorld {
                public static void main(String[] args) {
                    System.out.println("Hello, World!");
                }
            }
            ```
            """;
        
        var javaTokens = new List<JavaToken>();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(markdown));
        await MarkdownTokenizer.Create().ParseAsync(stream, token =>
        {
            if (token.Metadata is JavaCodeBlockMetadata javaMeta)
            {
                javaMeta.RegisterInlineTokenHandler(t => javaTokens.Add((JavaToken)t));
            }
        });
        
        Assert.NotEmpty(javaTokens);
        Assert.Contains(javaTokens, t => t.TokenType == JavaTokenType.Keyword && t.Value == "public");
        Assert.Contains(javaTokens, t => t.TokenType == JavaTokenType.Keyword && t.Value == "class");
        Assert.Contains(javaTokens, t => t.TokenType == JavaTokenType.Identifier && t.Value == "HelloWorld");
    }
}
