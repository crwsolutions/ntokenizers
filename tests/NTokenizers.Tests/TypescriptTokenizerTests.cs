using NTokenizers.Typescript;
using System.Text;

namespace Typescript;

public class TypescriptTokenizerTests
{
    [Fact]
    public void TestSimpleVariableDeclaration()
    {
        var tokens = Tokenize("let x = 5;");
        Assert.Equal(8, tokens.Count);
        Assert.Equal(TypescriptTokenType.Keyword, tokens[0].TokenType);
        Assert.Equal("let", tokens[0].Value);
        Assert.Equal(TypescriptTokenType.Whitespace, tokens[1].TokenType);
        Assert.Equal(TypescriptTokenType.Identifier, tokens[2].TokenType);
        Assert.Equal("x", tokens[2].Value);
        Assert.Equal(TypescriptTokenType.Whitespace, tokens[3].TokenType);
        Assert.Equal(TypescriptTokenType.Operator, tokens[4].TokenType);
        Assert.Equal("=", tokens[4].Value);
        Assert.Equal(TypescriptTokenType.Whitespace, tokens[5].TokenType);
        Assert.Equal(TypescriptTokenType.Number, tokens[6].TokenType);
        Assert.Equal("5", tokens[6].Value);
        Assert.Equal(TypescriptTokenType.SequenceTerminator, tokens[7].TokenType);
    }

    [Fact]
    public void TestConstDeclaration()
    {
        var tokens = Tokenize("const name = \"John\";");
        Assert.Equal(TypescriptTokenType.Keyword, tokens[0].TokenType);
        Assert.Equal("const", tokens[0].Value);
        Assert.Equal(TypescriptTokenType.Identifier, tokens[2].TokenType);
        Assert.Equal("name", tokens[2].Value);
        Assert.Equal(TypescriptTokenType.StringValue, tokens[6].TokenType);
        Assert.Equal("\"John\"", tokens[6].Value);
    }

    [Fact]
    public void TestFunctionDeclaration()
    {
        var tokens = Tokenize("function add(a, b) { return a + b; }");
        Assert.Equal(TypescriptTokenType.Keyword, tokens[0].TokenType);
        Assert.Equal("function", tokens[0].Value);
        Assert.Equal(TypescriptTokenType.Identifier, tokens[2].TokenType);
        Assert.Equal("add", tokens[2].Value);
        Assert.Equal(TypescriptTokenType.OpenParenthesis, tokens[3].TokenType);
        Assert.Equal(TypescriptTokenType.Identifier, tokens[4].TokenType);
        Assert.Equal("a", tokens[4].Value);
        Assert.Equal(TypescriptTokenType.Comma, tokens[5].TokenType);
        Assert.Equal(TypescriptTokenType.CloseParenthesis, tokens[8].TokenType);
    }

    [Fact]
    public void TestArrowFunction()
    {
        var tokens = Tokenize("const greet = (name) => `Hello ${name}`;");
        Assert.Contains(tokens, t => t.TokenType == TypescriptTokenType.Keyword && t.Value == "const");
        Assert.Contains(tokens, t => t.TokenType == TypescriptTokenType.Operator && t.Value == "=>");
        Assert.Contains(tokens, t => t.TokenType == TypescriptTokenType.StringValue && t.Value.StartsWith('`'));
    }

    [Fact]
    public void TestAsyncAwait()
    {
        var tokens = Tokenize("async function fetch() { await getData(); }");
        Assert.Equal(TypescriptTokenType.Keyword, tokens[0].TokenType);
        Assert.Equal("async", tokens[0].Value);
        Assert.Equal(TypescriptTokenType.Keyword, tokens[2].TokenType);
        Assert.Equal("function", tokens[2].Value);
        Assert.Contains(tokens, t => t.TokenType == TypescriptTokenType.Keyword && t.Value == "await");
    }

    [Fact]
    public void TestClassDefinition()
    {
        var tokens = Tokenize("class Person { constructor(name) { this.name = name; } }");
        Assert.Equal(TypescriptTokenType.Keyword, tokens[0].TokenType);
        Assert.Equal("class", tokens[0].Value);
        Assert.Contains(tokens, t => t.TokenType == TypescriptTokenType.Identifier && t.Value == "Person");
        Assert.Contains(tokens, t => t.TokenType == TypescriptTokenType.Keyword && t.Value == "constructor");
        Assert.Contains(tokens, t => t.TokenType == TypescriptTokenType.Keyword && t.Value == "this");
    }

    [Fact]
    public void TestImportExport()
    {
        var tokens = Tokenize("import { Component } from 'react';");
        Assert.Equal(TypescriptTokenType.Keyword, tokens[0].TokenType);
        Assert.Equal("import", tokens[0].Value);
        Assert.Contains(tokens, t => t.TokenType == TypescriptTokenType.Keyword && t.Value == "from");
        Assert.Contains(tokens, t => t.TokenType == TypescriptTokenType.StringValue && t.Value == "'react'");
    }

    [Fact]
    public void TestExportStatement()
    {
        var tokens = Tokenize("export default class MyClass {}");
        Assert.Equal(TypescriptTokenType.Keyword, tokens[0].TokenType);
        Assert.Equal("export", tokens[0].Value);
        Assert.Contains(tokens, t => t.TokenType == TypescriptTokenType.Keyword && t.Value == "default");
        Assert.Contains(tokens, t => t.TokenType == TypescriptTokenType.Keyword && t.Value == "class");
    }

    [Fact]
    public void TestStringWithSingleQuotes()
    {
        var tokens = Tokenize("let str = 'Hello World';");
        Assert.Contains(tokens, t => t.TokenType == TypescriptTokenType.StringValue && t.Value == "'Hello World'");
    }

    [Fact]
    public void TestStringWithDoubleQuotes()
    {
        var tokens = Tokenize("let str = \"Hello World\";");
        Assert.Contains(tokens, t => t.TokenType == TypescriptTokenType.StringValue && t.Value == "\"Hello World\"");
    }

    [Fact]
    public void TestTemplateString()
    {
        var tokens = Tokenize("let str = `Hello World`;");
        Assert.Contains(tokens, t => t.TokenType == TypescriptTokenType.StringValue && t.Value == "`Hello World`");
    }

    [Fact]
    public void TestEscapedQuotesInString()
    {
        var tokens = Tokenize("let str = \"Hello \\\"World\\\"\";");
        Assert.Contains(tokens, t => t.TokenType == TypescriptTokenType.StringValue && t.Value == "\"Hello \\\"World\\\"\"");
    }

    [Fact]
    public void TestIntegerNumber()
    {
        var tokens = Tokenize("let num = 42;");
        Assert.Contains(tokens, t => t.TokenType == TypescriptTokenType.Number && t.Value == "42");
    }

    [Fact]
    public void TestDecimalNumber()
    {
        var tokens = Tokenize("let num = 3.14159;");
        Assert.Contains(tokens, t => t.TokenType == TypescriptTokenType.Number && t.Value == "3.14159");
    }

    [Fact]
    public void TestNegativeNumber()
    {
        var tokens = Tokenize("let num = -42;");
        Assert.Contains(tokens, t => t.TokenType == TypescriptTokenType.Number && t.Value == "42");
    }

    [Fact]
    public void TestScientificNotation()
    {
        var tokens = Tokenize("let num = 1.5e10;");
        Assert.Contains(tokens, t => t.TokenType == TypescriptTokenType.Number && t.Value == "1.5e10");
    }

    [Fact]
    public void TestDateTimeValue()
    {
        var tokens = Tokenize("let date = 2025-11-19T12:34:56;");
        Assert.Contains(tokens, t => t.TokenType == TypescriptTokenType.DateTimeValue && t.Value.Contains('T'));
    }

    [Fact]
    public void TestKeywordsCaseInsensitive()
    {
        var tokens = Tokenize("LET x = 5; CONST y = 10; VAR z = 15;");
        var keywords = tokens.Where(t => t.TokenType == TypescriptTokenType.Keyword).ToList();
        Assert.Equal(3, keywords.Count);
        Assert.Equal("LET", keywords[0].Value);
        Assert.Equal("CONST", keywords[1].Value);
        Assert.Equal("VAR", keywords[2].Value);
    }

    [Fact]
    public void TestKeywordsVsIdentifiers()
    {
        var tokens = Tokenize("let letVariable = 5; const constant = 10;");
        var keywords = tokens.Where(t => t.TokenType == TypescriptTokenType.Keyword).Select(t => t.Value).ToList();
        var identifiers = tokens.Where(t => t.TokenType == TypescriptTokenType.Identifier).Select(t => t.Value).ToList();
        
        Assert.Contains("let", keywords);
        Assert.Contains("const", keywords);
        Assert.Contains("letVariable", identifiers);
        Assert.Contains("constant", identifiers);
    }

    [Fact]
    public void TestCommonKeywords()
    {
        var keywords = new[] { "if", "else", "for", "while", "return", "function", "class", "interface", 
                              "type", "import", "export", "async", "await", "new", "this", "super" };
        
        foreach (var keyword in keywords)
        {
            var tokens = Tokenize($"{keyword} test");
            Assert.Equal(TypescriptTokenType.Keyword, tokens[0].TokenType);
            Assert.Equal(keyword, tokens[0].Value);
        }
    }

    [Fact]
    public void TestTypeScriptSpecificKeywords()
    {
        var keywords = new[] { "interface", "type", "readonly", "keyof", "infer", "unknown", "never", 
                              "satisfies", "asserts", "declare", "namespace", "module" };
        
        foreach (var keyword in keywords)
        {
            var tokens = Tokenize($"{keyword} test");
            Assert.Equal(TypescriptTokenType.Keyword, tokens[0].TokenType);
            Assert.Equal(keyword, tokens[0].Value);
        }
    }

    [Fact]
    public void TestEqualsOperator()
    {
        var tokens = Tokenize("a == b");
        Assert.Contains(tokens, t => t.TokenType == TypescriptTokenType.Equals && t.Value == "==");
    }

    [Fact]
    public void TestStrictEqualsOperator()
    {
        var tokens = Tokenize("a === b");
        Assert.Contains(tokens, t => t.TokenType == TypescriptTokenType.Equals && t.Value == "===");
    }

    [Fact]
    public void TestNotEqualsOperator()
    {
        var tokens = Tokenize("a != b");
        Assert.Contains(tokens, t => t.TokenType == TypescriptTokenType.NotEquals && t.Value == "!=");
    }

    [Fact]
    public void TestStrictNotEqualsOperator()
    {
        var tokens = Tokenize("a !== b");
        Assert.Contains(tokens, t => t.TokenType == TypescriptTokenType.NotEquals && t.Value == "!==");
    }

    [Fact]
    public void TestAndOperator()
    {
        var tokens = Tokenize("a && b");
        Assert.Contains(tokens, t => t.TokenType == TypescriptTokenType.And && t.Value == "&&");
    }

    [Fact]
    public void TestOrOperator()
    {
        var tokens = Tokenize("a || b");
        Assert.Contains(tokens, t => t.TokenType == TypescriptTokenType.Or && t.Value == "||");
    }

    [Fact]
    public void TestArithmeticOperators()
    {
        var tokens = Tokenize("a + b - c * d / e % f");
        var operators = tokens.Where(t => t.TokenType == TypescriptTokenType.Operator).Select(t => t.Value).ToList();
        Assert.Contains("+", operators);
        Assert.Contains("-", operators);
        Assert.Contains("*", operators);
        Assert.Contains("/", operators);
        Assert.Contains("%", operators);
    }

    [Fact]
    public void TestComparisonOperators()
    {
        var tokens = Tokenize("a < b <= c > d >= e");
        var operators = tokens.Where(t => t.TokenType == TypescriptTokenType.Operator).Select(t => t.Value).ToList();
        Assert.Contains("<", operators);
        Assert.Contains("<=", operators);
        Assert.Contains(">", operators);
        Assert.Contains(">=", operators);
    }

    [Fact]
    public void TestAssignmentOperators()
    {
        var tokens = Tokenize("a = b += c -= d *= e /= f");
        var operators = tokens.Where(t => t.TokenType == TypescriptTokenType.Operator).Select(t => t.Value).ToList();
        Assert.Contains("=", operators);
        Assert.Contains("+=", operators);
        Assert.Contains("-=", operators);
        Assert.Contains("*=", operators);
        Assert.Contains("/=", operators);
    }

    [Fact]
    public void TestIncrementDecrementOperators()
    {
        var tokens = Tokenize("a++ b--");
        Assert.Contains(tokens, t => t.TokenType == TypescriptTokenType.Operator && t.Value == "++");
        Assert.Contains(tokens, t => t.TokenType == TypescriptTokenType.Operator && t.Value == "--");
    }

    [Fact]
    public void TestLineComment()
    {
        var tokens = Tokenize("let x = 5; // This is a comment");
        Assert.Contains(tokens, t => t.TokenType == TypescriptTokenType.Comment && t.Value == "// This is a comment");
    }

    [Fact]
    public void TestBlockComment()
    {
        var tokens = Tokenize("let x = /* comment */ 5;");
        Assert.Contains(tokens, t => t.TokenType == TypescriptTokenType.Comment && t.Value == "/* comment */");
    }

    [Fact]
    public void TestMultilineBlockComment()
    {
        var tokens = Tokenize("let x = /* line1\nline2\nline3 */ 5;");
        var comment = tokens.FirstOrDefault(t => t.TokenType == TypescriptTokenType.Comment);
        Assert.NotNull(comment);
        Assert.Contains("line1", comment.Value);
        Assert.Contains("line2", comment.Value);
        Assert.Contains("line3", comment.Value);
    }

    [Fact]
    public void TestCommentAtEndOfFile()
    {
        var tokens = Tokenize("let x = 5; // comment");
        var comment = tokens.LastOrDefault(t => t.TokenType == TypescriptTokenType.Comment);
        Assert.NotNull(comment);
        Assert.Equal("// comment", comment.Value);
    }

    [Fact]
    public void TestPunctuation()
    {
        var tokens = Tokenize("f(a, b); x.y;");
        Assert.Contains(tokens, t => t.TokenType == TypescriptTokenType.OpenParenthesis);
        Assert.Contains(tokens, t => t.TokenType == TypescriptTokenType.CloseParenthesis);
        Assert.Contains(tokens, t => t.TokenType == TypescriptTokenType.Comma);
        Assert.Contains(tokens, t => t.TokenType == TypescriptTokenType.Dot);
        Assert.Contains(tokens, t => t.TokenType == TypescriptTokenType.SequenceTerminator);
    }

    [Fact]
    public void TestParentheses()
    {
        var tokens = Tokenize("(a + b)");
        Assert.Equal(TypescriptTokenType.OpenParenthesis, tokens[0].TokenType);
        Assert.Equal(TypescriptTokenType.CloseParenthesis, tokens[6].TokenType);
    }

    [Fact]
    public void TestComma()
    {
        var tokens = Tokenize("a, b, c");
        var commas = tokens.Where(t => t.TokenType == TypescriptTokenType.Comma).ToList();
        Assert.Equal(2, commas.Count);
    }

    [Fact]
    public void TestDot()
    {
        var tokens = Tokenize("obj.property");
        Assert.Contains(tokens, t => t.TokenType == TypescriptTokenType.Dot);
    }

    [Fact]
    public void TestSemicolon()
    {
        var tokens = Tokenize("let x = 5;");
        var semicolon = tokens.LastOrDefault(t => t.TokenType == TypescriptTokenType.SequenceTerminator);
        Assert.NotNull(semicolon);
        Assert.Equal(";", semicolon.Value);
    }

    [Fact]
    public void TestWhitespacePreservation()
    {
        var tokens = Tokenize("let  x   =    5;");
        var whitespaces = tokens.Where(t => t.TokenType == TypescriptTokenType.Whitespace).ToList();
        Assert.True(whitespaces.Count >= 3);
        Assert.Equal("  ", whitespaces[0].Value);
        Assert.Equal("   ", whitespaces[1].Value);
        Assert.Equal("    ", whitespaces[2].Value);
    }

    [Fact]
    public void TestMultipleWhitespaceTypes()
    {
        var tokens = Tokenize("let\tx\n=\r\n5;");
        var whitespaces = tokens.Where(t => t.TokenType == TypescriptTokenType.Whitespace).ToList();
        Assert.True(whitespaces.Count >= 3);
        Assert.Contains(whitespaces, w => w.Value.Contains('\t'));
        Assert.Contains(whitespaces, w => w.Value.Contains('\n'));
    }

    [Fact]
    public void TestNestedFunctionCalls()
    {
        var tokens = Tokenize("outer(inner(a, b), c)");
        var openParens = tokens.Where(t => t.TokenType == TypescriptTokenType.OpenParenthesis).ToList();
        var closeParens = tokens.Where(t => t.TokenType == TypescriptTokenType.CloseParenthesis).ToList();
        Assert.Equal(2, openParens.Count);
        Assert.Equal(2, closeParens.Count);
    }

    [Fact]
    public void TestComplexExpression()
    {
        var tokens = Tokenize("if (a > 5 && b < 10 || c === true) { return false; }");
        Assert.Contains(tokens, t => t.TokenType == TypescriptTokenType.Keyword && t.Value == "if");
        Assert.Contains(tokens, t => t.TokenType == TypescriptTokenType.And);
        Assert.Contains(tokens, t => t.TokenType == TypescriptTokenType.Or);
        Assert.Contains(tokens, t => t.TokenType == TypescriptTokenType.Equals && t.Value == "===");
        Assert.Contains(tokens, t => t.TokenType == TypescriptTokenType.Keyword && t.Value == "true");
        Assert.Contains(tokens, t => t.TokenType == TypescriptTokenType.Keyword && t.Value == "return");
        Assert.Contains(tokens, t => t.TokenType == TypescriptTokenType.Keyword && t.Value == "false");
    }

    [Fact]
    public void TestArrayDeclaration()
    {
        var tokens = Tokenize("let arr = [1, 2, 3];");
        Assert.Contains(tokens, t => t.TokenType == TypescriptTokenType.Operator && t.Value == "[");
        Assert.Contains(tokens, t => t.TokenType == TypescriptTokenType.Operator && t.Value == "]");
        var numbers = tokens.Where(t => t.TokenType == TypescriptTokenType.Number).ToList();
        Assert.Equal(3, numbers.Count);
    }

    [Fact]
    public void TestObjectLiteral()
    {
        var tokens = Tokenize("let obj = { name: \"John\", age: 30 };");
        Assert.Contains(tokens, t => t.TokenType == TypescriptTokenType.Operator && t.Value == "{");
        Assert.Contains(tokens, t => t.TokenType == TypescriptTokenType.Operator && t.Value == "}");
        Assert.Contains(tokens, t => t.TokenType == TypescriptTokenType.Operator && t.Value == ":");
        Assert.Contains(tokens, t => t.TokenType == TypescriptTokenType.StringValue && t.Value == "\"John\"");
        Assert.Contains(tokens, t => t.TokenType == TypescriptTokenType.Number && t.Value == "30");
    }

    [Fact]
    public void TestForLoop()
    {
        var tokens = Tokenize("for (let i = 0; i < 10; i++) { console.log(i); }");
        Assert.Contains(tokens, t => t.TokenType == TypescriptTokenType.Keyword && t.Value == "for");
        Assert.Contains(tokens, t => t.TokenType == TypescriptTokenType.Keyword && t.Value == "let");
        var semicolons = tokens.Where(t => t.TokenType == TypescriptTokenType.SequenceTerminator).ToList();
        Assert.Equal(3, semicolons.Count);
    }

    [Fact]
    public void TestTernaryOperator()
    {
        var tokens = Tokenize("let result = condition ? true : false;");
        Assert.Contains(tokens, t => t.TokenType == TypescriptTokenType.Operator && t.Value == "?");
        Assert.Contains(tokens, t => t.TokenType == TypescriptTokenType.Operator && t.Value == ":");
    }

    [Fact]
    public void TestStopDelimiterSimple()
    {
        var tokens = Tokenize("let x = 5; END let y = 10;", "END");
        Assert.DoesNotContain(tokens, t => t.Value.Contains("y"));
        Assert.Contains(tokens, t => t.Value.Contains("x"));
    }

    [Fact]
    public void TestStopDelimiterInMiddle()
    {
        var tokens = Tokenize("function test() { return 42; } STOP function other() { }", "STOP");
        Assert.Contains(tokens, t => t.Value == "test");
        Assert.DoesNotContain(tokens, t => t.Value == "other");
    }

    [Fact]
    public void TestStopDelimiterMultiChar()
    {
        var tokens = Tokenize("let a = 1; ```end let b = 2;", "```");
        Assert.Contains(tokens, t => t.Value == "a");
        Assert.DoesNotContain(tokens, t => t.Value == "b");
    }

    [Fact]
    public void TestStopDelimiterAtStart()
    {
        var tokens = Tokenize("STOP let x = 5;", "STOP");
        Assert.Empty(tokens);
    }

    [Fact]
    public void TestNoStopDelimiter()
    {
        var tokens = Tokenize("let x = 5; let y = 10;", null);
        Assert.Contains(tokens, t => t.Value == "x");
        Assert.Contains(tokens, t => t.Value == "y");
    }

    [Fact]
    public void TestInterfaceDeclaration()
    {
        var tokens = Tokenize("interface User { name: string; age: number; }");
        Assert.Equal(TypescriptTokenType.Keyword, tokens[0].TokenType);
        Assert.Equal("interface", tokens[0].Value);
        Assert.Contains(tokens, t => t.TokenType == TypescriptTokenType.Identifier && t.Value == "User");
    }

    [Fact]
    public void TestTypeAlias()
    {
        var tokens = Tokenize("type ID = string | number;");
        Assert.Equal(TypescriptTokenType.Keyword, tokens[0].TokenType);
        Assert.Equal("type", tokens[0].Value);
        // Single | is just an Operator, not Or (which requires ||)
        Assert.Contains(tokens, t => t.TokenType == TypescriptTokenType.Operator && t.Value == "|");
    }

    [Fact]
    public void TestGenericType()
    {
        var tokens = Tokenize("function identity<T>(arg: T): T { return arg; }");
        Assert.Contains(tokens, t => t.TokenType == TypescriptTokenType.Keyword && t.Value == "function");
        Assert.Contains(tokens, t => t.TokenType == TypescriptTokenType.Identifier && t.Value == "identity");
        Assert.Contains(tokens, t => t.TokenType == TypescriptTokenType.Operator && t.Value == "<");
        Assert.Contains(tokens, t => t.TokenType == TypescriptTokenType.Operator && t.Value == ">");
    }

    [Fact]
    public void TestNullishCoalescing()
    {
        var tokens = Tokenize("let value = input ?? defaultValue;");
        Assert.Contains(tokens, t => t.TokenType == TypescriptTokenType.Operator && t.Value == "??");
    }

    [Fact]
    public void TestOptionalChaining()
    {
        var tokens = Tokenize("let value = obj?.property;");
        Assert.Contains(tokens, t => t.TokenType == TypescriptTokenType.Operator && t.Value == "?.");
    }

    [Fact]
    public void TestSpreadOperator()
    {
        var tokens = Tokenize("let arr = [...oldArray, newItem];");
        // Note: ... may be tokenized as multiple dots or a single operator depending on implementation
        var dots = tokens.Where(t => t.TokenType == TypescriptTokenType.Dot || 
                                     (t.TokenType == TypescriptTokenType.Operator && t.Value.Contains('.'))).ToList();
        Assert.True(dots.Count >= 1);
    }

    private static List<TypescriptToken> Tokenize(string input)
    {
        var tokens = new List<TypescriptToken>();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        TypescriptTokenizer.Parse(stream, null, token => tokens.Add(token));
        return tokens;
    }

    private static List<TypescriptToken> Tokenize(string input, string? stopDelimiter)
    {
        var tokens = new List<TypescriptToken>();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        TypescriptTokenizer.Parse(stream, stopDelimiter, token => tokens.Add(token));
        return tokens;
    }
}
